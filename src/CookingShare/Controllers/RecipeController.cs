using CookingShare.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;

namespace CookingShare.Controllers
{
    public class RecipeController : Controller
    {
        CookingShareDBEntities db = new CookingShareDBEntities();

        // HÀM HIỂN THỊ DANH SÁCH CÔNG THỨC VÀ XỬ LÝ LỌC TÌM KIẾM
        public ActionResult Index(string search, int? categoryId, string difficulty, List<int> allergyIds, List<int> fridgeIngredientIds, string sort = "newest", int page = 1)
        {
            var query = db.RECIPE
                          .Include(r => r.ACCOUNT)
                          .Include(r => r.CATEGORY)
                          .Where(r => r.Status == 1).AsQueryable();

            // Lọc theo Từ khóa
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(r => r.Name.Contains(search));
                ViewBag.SearchKeyword = search;
            }

            // Lọc theo Danh mục
            if (categoryId.HasValue)
            {
                query = query.Where(r => r.CategoryID == categoryId);
                ViewBag.CurrentCategory = categoryId;
            }

            // Lọc theo Độ khó
            if (!string.IsNullOrEmpty(difficulty))
            {
                query = query.Where(r => r.Difficulty == difficulty);
                ViewBag.CurrentDifficulty = difficulty;
            }

            // LỌC THEO DỊ ỨNG (TRÁNH NGUYÊN LIỆU)
            if (allergyIds != null && allergyIds.Count > 0)
            {
                query = query.Where(r => !r.RECIPE_DETAIL.Any(d => allergyIds.Contains(d.IngredientID)));
                ViewBag.SelectedAllergies = allergyIds;
            }

            // TỦ LẠNH THÔNG MINH (CHỨA NGUYÊN LIỆU)
            if (fridgeIngredientIds != null && fridgeIngredientIds.Count > 0)
            {
                query = query.Where(r => r.RECIPE_DETAIL.Any(d => fridgeIngredientIds.Contains(d.IngredientID)));
                ViewBag.SelectedFridgeIngredients = fridgeIngredientIds;
            }

            //  Sắp xếp
            switch (sort)
            {
                case "views":
                    query = query.OrderByDescending(r => r.Views);
                    break;
                case "time":
                    query = query.OrderBy(r => r.CookTime);
                    break;
                default:
                    query = query.OrderByDescending(r => r.CreateDate);
                    break;
            }
            ViewBag.CurrentSort = sort;

            // LƯU CACHE CHO CATEGORY & INGREDIENT ĐỂ TRÁNH QUÁ TẢI KHI TÌM KIẾM
            var categories = HttpContext.Cache["GlobalCategories"] as List<CATEGORY>;
            if (categories == null)
            {
                categories = db.CATEGORY.ToList();
                HttpContext.Cache.Insert("GlobalCategories", categories, null, DateTime.Now.AddHours(24), System.Web.Caching.Cache.NoSlidingExpiration);
            }
            ViewBag.Categories = categories;

            var ingredients = HttpContext.Cache["GlobalIngredients"] as List<INGREDIENT>;
            if (ingredients == null)
            {
                ingredients = db.INGREDIENT.OrderBy(i => i.Name).ToList();
                HttpContext.Cache.Insert("GlobalIngredients", ingredients, null, DateTime.Now.AddHours(24), System.Web.Caching.Cache.NoSlidingExpiration);
            }
            ViewBag.Ingredients = ingredients;


            List<int> myAllergyIds = new List<int>();
            List<int> myFavoriteIds = new List<int>();

            if (Session["Account"] != null)
            {
                int userId = ((ACCOUNT)Session["Account"]).ID;
                var currentUser = db.ACCOUNT.FirstOrDefault(u => u.ID == userId);
                if (currentUser != null)
                {
                    myAllergyIds = currentUser.INGREDIENT.Select(i => i.ID).ToList();

                    var listFav = db.FAVORITE.Where(f => f.AccountID == userId).ToList();
                    foreach (var item in listFav)
                    {
                        myFavoriteIds.Add(item.RecipeID);
                    }
                }
            }
            ViewBag.MyAllergyIds = myAllergyIds;
            ViewBag.MyFavoriteIds = myFavoriteIds;

            // Xử lý phân trang
            int pageSize = 9;
            int totalItems = query.Count();
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            if (page < 1) page = 1;
            if (page > totalPages && totalPages > 0) page = totalPages;

            var recipes = new List<RECIPE>();
            if (totalItems > 0)
            {
                recipes = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            }

            ViewBag.TotalCount = totalItems;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View(recipes);
        }

        // NHẬN ID MÓN ĂN TỪ TRANG CHỦ
        public ActionResult Detail(int id)
        {
            var recipe = db.RECIPE.Find(id);

            if (recipe == null) return HttpNotFound();

            if (recipe.Status != 1)
            {
                var currentUser = Session["Account"] as ACCOUNT;
                if (currentUser == null || (currentUser.Role != 1 && recipe.AccountID != currentUser.ID))
                {
                    return HttpNotFound("Công thức này đang chờ duyệt hoặc đã bị ẩn.");
                }
            }

            // Tự động tăng lượt xem mỗi khi load trang
            recipe.Views = (recipe.Views ?? 0) + 1;
            db.SaveChanges();

            ViewBag.IsFavorited = false;
            ViewBag.IsFollowed = false;

            if (Session["Account"] != null)
            {
                int currentAccId = ((CookingShare.Models.ACCOUNT)Session["Account"]).ID;

                ViewBag.IsFavorited = db.FAVORITE.Any(f => f.RecipeID == id && f.AccountID == currentAccId);

                if (recipe.AccountID != null)
                {
                    ViewBag.IsFollowed = db.FOLLOW.Any(f => f.FollowerID == currentAccId && f.FollowedID == recipe.AccountID);
                }
            }

            return View(recipe);
        }


        // BÌNH LUẬN & THÀNH QUẢ
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddComment(int RecipeID, string Content, int Rating)
        {
            if (Session["Account"] == null) return RedirectToAction("Login", "Account", new { returnUrl = "/Recipe/Detail/" + RecipeID });

            var currentUser = (CookingShare.Models.ACCOUNT)Session["Account"];

            if (string.IsNullOrWhiteSpace(Content)) return RedirectToAction("Detail", "Recipe", new { id = RecipeID });

            var recipe = db.RECIPE.Find(RecipeID);
            if (recipe == null) return HttpNotFound("Không tìm thấy công thức này.");

            var newComment = new CookingShare.Models.COMMENT();
            newComment.RecipeID = RecipeID;
            newComment.AccountID = currentUser.ID;
            newComment.Content = Content;
            newComment.Rating = Rating;
            newComment.CreateDate = DateTime.Now;
            newComment.Status = 1;

            db.COMMENT.Add(newComment);

            if (recipe.AccountID.HasValue && recipe.AccountID.Value != currentUser.ID)
            {
                var newNoti = new CookingShare.Models.NOTIFICATION();
                newNoti.AccountID = recipe.AccountID.Value;
                newNoti.Content = $"{currentUser.UserName} đã bình luận về món '{recipe.Name}' của bạn.";
                newNoti.LinkURL = $"/Recipe/Detail/{recipe.ID}";
                newNoti.IsRead = false;
                newNoti.CreateDate = DateTime.Now;
                db.NOTIFICATION.Add(newNoti);
            }

            db.SaveChanges();
            return RedirectToAction("Detail", "Recipe", new { id = RecipeID });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UploadCooksnap(int RecipeID, string Content, System.Web.HttpPostedFileBase ImageFile)
        {
            if (Session["Account"] == null) return RedirectToAction("Login", "Account", new { returnUrl = "/Recipe/Detail/" + RecipeID });

            var sessionUser = (CookingShare.Models.ACCOUNT)Session["Account"];
            var recipe = db.RECIPE.Find(RecipeID);

            if (recipe == null) return HttpNotFound("Không tìm thấy công thức này.");

            if (ImageFile != null && ImageFile.ContentLength > 0)
            {
                string prefix = $"cooksnap_{RecipeID}_{sessionUser.ID}_{DateTime.Now.Ticks}";
                string fileName = FileHelper.UploadAndReplaceImage(ImageFile, "~/assets/images/cooksnaps/", prefix, null);

                var newSnap = new CookingShare.Models.COOKSNAP();
                newSnap.RecipeID = RecipeID;
                newSnap.AccountID = sessionUser.ID;
                newSnap.ImageName = fileName;
                newSnap.Content = Content;
                newSnap.CreateDate = DateTime.Now;
                newSnap.Status = 1;

                db.COOKSNAP.Add(newSnap);

                if (recipe.AccountID.HasValue && recipe.AccountID.Value != sessionUser.ID)
                {
                    var newNoti = new CookingShare.Models.NOTIFICATION();
                    newNoti.AccountID = recipe.AccountID.Value;
                    newNoti.Content = $"Tuyệt vời! {sessionUser.UserName} vừa thực hành món '{recipe.Name}' của bạn và khoe thành quả.";
                    newNoti.LinkURL = $"/Recipe/Detail/{recipe.ID}";
                    newNoti.IsRead = false;
                    newNoti.CreateDate = DateTime.Now;
                    db.NOTIFICATION.Add(newNoti);
                }

                db.SaveChanges();
                TempData["SuccessMessage"] = "Đăng thành quả thành công!";
            }
            else
            {
                TempData["ErrorMessage"] = "Vui lòng chọn ảnh trước khi khoe thành quả nhé!";
            }

            return RedirectToAction("Detail", new { id = RecipeID });
        }

        public ActionResult GetCooksnapDetail(int id)
        {
            var snap = db.COOKSNAP.FirstOrDefault(c => c.ID == id);
            if (snap == null) return HttpNotFound();
            return PartialView("_CooksnapDetail", snap);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddCooksnapComment(int CooksnapID, int RecipeID, string Content)
        {
            if (Session["Account"] == null) return RedirectToAction("Login", "Account", new { returnUrl = "/Recipe/Detail/" + RecipeID });

            var sessionUser = (CookingShare.Models.ACCOUNT)Session["Account"];

            CookingShare.Models.COOKSNAP_COMMENT newCmt = new CookingShare.Models.COOKSNAP_COMMENT();
            newCmt.CooksnapID = CooksnapID;
            newCmt.AccountID = sessionUser.ID;
            newCmt.Content = Content;
            newCmt.CreateDate = System.DateTime.Now;
            newCmt.Status = 1;

            db.COOKSNAP_COMMENT.Add(newCmt);
            db.SaveChanges();

            return RedirectToAction("Detail", new { id = RecipeID });
        }

        // CÁC TÍNH NĂNG TƯƠNG TÁC (AJAX)
        [HttpPost]
        public JsonResult ToggleFavorite(int recipeId)
        {
            if (Session["Account"] == null) return Json(new { success = false, message = "Vui lòng đăng nhập!" });
            int accId = ((CookingShare.Models.ACCOUNT)Session["Account"]).ID;

            var existing = db.FAVORITE.FirstOrDefault(f => f.RecipeID == recipeId && f.AccountID == accId);
            bool isSaved = false;

            if (existing != null)
            {
                db.FAVORITE.Remove(existing);
            }
            else
            {
                CookingShare.Models.FAVORITE fav = new CookingShare.Models.FAVORITE { AccountID = accId, RecipeID = recipeId, CreateDate = DateTime.Now };
                db.FAVORITE.Add(fav);
                isSaved = true;
            }
            db.SaveChanges();
            return Json(new { success = true, isSaved = isSaved });
        }

        [HttpPost]
        public JsonResult ToggleFollow(int targetId)
        {
            if (Session["Account"] == null) return Json(new { success = false, message = "Vui lòng đăng nhập!" });
            int accId = ((CookingShare.Models.ACCOUNT)Session["Account"]).ID;

            if (accId == targetId) return Json(new { success = false, message = "Bạn không thể tự theo dõi chính mình!" });

            var existing = db.FOLLOW.FirstOrDefault(f => f.FollowerID == accId && f.FollowedID == targetId);
            bool isFollowed = false;

            if (existing != null)
            {
                db.FOLLOW.Remove(existing);
            }
            else
            {
                CookingShare.Models.FOLLOW flw = new CookingShare.Models.FOLLOW { FollowerID = accId, FollowedID = targetId, CreateDate = DateTime.Now };
                db.FOLLOW.Add(flw);
                isFollowed = true;
            }
            db.SaveChanges();
            return Json(new { success = true, isFollowed = isFollowed });
        }

        [HttpPost]
        public JsonResult AddToGrocery(int recipeId)
        {
            if (Session["Account"] == null) return Json(new { success = false, message = "Vui lòng đăng nhập!" });
            int accId = ((CookingShare.Models.ACCOUNT)Session["Account"]).ID;

            var existing = db.GROCERY.FirstOrDefault(g => g.RecipeID == recipeId && g.AccountID == accId);
            if (existing == null)
            {
                CookingShare.Models.GROCERY gr = new CookingShare.Models.GROCERY { AccountID = accId, RecipeID = recipeId, CreateDate = DateTime.Now };
                db.GROCERY.Add(gr);
                db.SaveChanges();
            }
            return Json(new { success = true });
        }

        // DANH SÁCH ĐI CHỢ & XUẤT WORD
        public ActionResult CartCount()
        {
            int count = 0;
            if (Session["Account"] != null)
            {
                int accId = ((CookingShare.Models.ACCOUNT)Session["Account"]).ID;
                count = db.GROCERY.Count(g => g.AccountID == accId);
            }
            return PartialView("_CartCount", count);
        }

        public class ChotDonViewModel
        {
            public string TenNguyenLieu { get; set; }
            public double TongSoLuong { get; set; }
            public string DonViTinh { get; set; }
        }

        public ActionResult Grocery()
        {
            if (Session["Account"] == null) return RedirectToAction("Login", "Account");
            int accId = ((CookingShare.Models.ACCOUNT)Session["Account"]).ID;

            var gioHang = db.GROCERY.Where(g => g.AccountID == accId).ToList();
            var danhSachMonAn = gioHang.Select(g => g.RECIPE).ToList();

            var tatCaNguyenLieu = gioHang.SelectMany(g => g.RECIPE.RECIPE_DETAIL).ToList();
            var nguyenLieuGop = tatCaNguyenLieu
                .GroupBy(d => new { d.IngredientID, Ten = d.INGREDIENT.Name, DonVi = (d.UNIT != null ? d.UNIT.UnitName : "") })
                .Select(group => new ChotDonViewModel
                {
                    TenNguyenLieu = group.Key.Ten,
                    DonViTinh = group.Key.DonVi,
                    TongSoLuong = Math.Round(group.Sum(x => x.Quantity), 2)
                })
                .OrderBy(x => x.TenNguyenLieu)
                .ToList();

            ViewBag.DanhSachDiCho = nguyenLieuGop;
            return View(danhSachMonAn);
        }

        [HttpPost]
        public ActionResult ClearGrocery()
        {
            if (Session["Account"] == null) return Json(new { success = false });
            int accId = ((CookingShare.Models.ACCOUNT)Session["Account"]).ID;

            var items = db.GROCERY.Where(g => g.AccountID == accId);
            db.GROCERY.RemoveRange(items);
            db.SaveChanges();

            return Json(new { success = true });
        }

        [HttpPost]
        public ActionResult ExportWord(List<string> itemNames)
        {
            if (Session["Account"] == null) return RedirectToAction("Login", "Account");
            int accId = ((CookingShare.Models.ACCOUNT)Session["Account"]).ID;

            var gioHang = db.GROCERY.Where(g => g.AccountID == accId).ToList();
            var tatCaNguyenLieu = gioHang.SelectMany(g => g.RECIPE.RECIPE_DETAIL).ToList();

            var nguyenLieuGop = tatCaNguyenLieu
                .GroupBy(d => new { d.IngredientID, Ten = d.INGREDIENT.Name, DonVi = (d.UNIT != null ? d.UNIT.UnitName : "") })
                .Select(group => new ChotDonViewModel
                {
                    TenNguyenLieu = group.Key.Ten,
                    DonViTinh = group.Key.DonVi,
                    TongSoLuong = Math.Round(group.Sum(x => x.Quantity), 2)
                })
                .ToList();

            if (itemNames != null && itemNames.Count > 0)
            {
                nguyenLieuGop = nguyenLieuGop.Where(x => itemNames.Contains(x.TenNguyenLieu)).ToList();
            }
            else
            {
                nguyenLieuGop = new List<ChotDonViewModel>();
            }

            nguyenLieuGop = nguyenLieuGop.OrderBy(x => x.TenNguyenLieu).ToList();

            string html = "<html><head><meta charset='utf-8'></head><body style='font-family: Arial, sans-serif;'>";
            html += "<h2 style='text-align:center; color: #198754;'>DANH SÁCH ĐI CHỢ</h2>";
            html += $"<p style='text-align:center; font-style: italic;'>Ngày tạo: {DateTime.Now.ToString("dd/MM/yyyy HH:mm")}</p>";
            html += "<hr/>";
            html += "<ul style='font-size: 14pt; line-height: 1.8;'>";

            foreach (var item in nguyenLieuGop)
            {
                html += $"<li><b>{item.TongSoLuong} {item.DonViTinh}</b> - {item.TenNguyenLieu}</li>";
            }

            html += "</ul>";
            html += "<br/><p style='text-align:center; color: gray;'><i>Được tạo tự động từ ứng dụng CookingShare</i></p>";
            html += "</body></html>";

            byte[] fileBytes = System.Text.Encoding.UTF8.GetBytes(html);
            return File(fileBytes, "application/msword", "DanhSachDiCho_CookingShare.doc");
        }


        // ĐĂNG BÀI VIẾT MỚI
        [HttpGet]
        public ActionResult Create()
        {
            if (Session["Account"] == null) return RedirectToAction("Login", "Account");

            ViewBag.Categories = db.CATEGORY.ToList();
            ViewBag.Ingredients = db.INGREDIENT.OrderBy(i => i.Name).ToList();
            ViewBag.Units = db.UNIT.ToList();

            return View();
        }

        [HttpPost]
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult Create(RECIPE recipe, HttpPostedFileBase ImageFile, int[] IngredientIDs, double[] IngredientQuantities, int[] UnitIDs, string[] StepContents, HttpPostedFileBase[] StepImages)
        {
            if (Session["Account"] == null) return RedirectToAction("Login", "Account");
            int accId = ((CookingShare.Models.ACCOUNT)Session["Account"]).ID;

            try
            {
                // LƯU THÔNG TIN MÓN ĂN
                recipe.AccountID = accId;
                recipe.CreateDate = DateTime.Now;
                recipe.Views = 0;
                recipe.Status = 0;


                // SỬ DỤNG FILEHELPER LƯU ẢNH CHÍNH (MAIN IMAGE)
                if (ImageFile != null && ImageFile.ContentLength > 0)
                {
                    string prefix = "recipe_" + Guid.NewGuid().ToString("N").Substring(0, 8);
                    recipe.MainImage = FileHelper.UploadAndReplaceImage(ImageFile, "~/assets/images/recipes/", prefix, null);
                }
                else
                {
                    recipe.MainImage = "default_recipe.jpg";
                }

                db.RECIPE.Add(recipe);
                db.SaveChanges(); // Lưu xong mới có recipe.ID để tạo tiếp

                // LƯU NGUYÊN LIỆU
                if (IngredientIDs != null && IngredientIDs.Length > 0)
                {
                    for (int i = 0; i < IngredientIDs.Length; i++)
                    {
                        if (IngredientIDs[i] > 0)
                        {
                            var detail = new RECIPE_DETAIL();
                            detail.RecipeID = recipe.ID;
                            detail.IngredientID = IngredientIDs[i];
                            detail.Quantity = IngredientQuantities[i];
                            detail.UnitID = UnitIDs[i];

                            db.RECIPE_DETAIL.Add(detail);
                        }
                    }
                }

                // LƯU CÁC BƯỚC THỰC HIỆN VÀ ẢNH TỪNG BƯỚC
                if (StepContents != null && StepContents.Length > 0)
                {
                    for (int i = 0; i < StepContents.Length; i++)
                    {
                        if (!string.IsNullOrWhiteSpace(StepContents[i]))
                        {
                            var step = new STEPTODO();
                            step.RecipeID = recipe.ID;
                            step.StepOrder = i + 1;
                            step.Content = StepContents[i];


                            // SỬ DỤNG FILEHELPER LƯU ẢNH CÁC BƯỚC LÀM
                            if (StepImages != null && i < StepImages.Length && StepImages[i] != null && StepImages[i].ContentLength > 0)
                            {
                                var sFile = StepImages[i];
                                string sPrefix = $"step_{recipe.ID}_{step.StepOrder}";
                                step.ImageURL = FileHelper.UploadAndReplaceImage(sFile, "~/assets/images/steps/", sPrefix, null);
                            }

                            db.STEPTODO.Add(step);
                        }
                    }
                }

                db.SaveChanges();
                return RedirectToAction("Detail", "Recipe", new { id = recipe.ID });
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Có lỗi xảy ra: " + ex.Message;

                ViewBag.Categories = db.CATEGORY.ToList();
                ViewBag.Ingredients = db.INGREDIENT.OrderBy(i => i.Name).ToList();
                ViewBag.Units = db.UNIT.ToList();

                return View(recipe);
            }
        }

        //  ĐỒNG BỘ LOGIC XÓA BÀI VIẾT TỪ KHU VỰC ADMIN ĐỂ CHỐNG LỖI SẬP DATA (FOREIGN KEY EXCEPTION)
        [HttpGet]
        public ActionResult Delete(int id)
        {
            if (Session["Account"] == null) return RedirectToAction("Login", "Account");
            int accId = ((CookingShare.Models.ACCOUNT)Session["Account"]).ID;

            try
            {
                var recipe = db.RECIPE.Include(r => r.RECIPE_DETAIL)
                                      .Include(r => r.STEPTODO)
                                      .Include(r => r.COOKSNAP)
                                      .Include(r => r.COMMENT)
                                      .FirstOrDefault(r => r.ID == id && r.AccountID == accId);

                if (recipe != null)
                {
                    // LƯU LẠI STATUS TRƯỚC KHI XÓA ĐỂ ĐẬP VỠ CACHE TRANG CHỦ NẾU CẦN
                    int oldStatus = recipe.Status ?? 0;

                    //  DỌN SẠCH CÁC BẢNG TRUNG GIAN KHÔNG CÓ ẢNH (Tags, Chi tiết, Yêu thích, Đi chợ)
                    recipe.TAG.Clear();
                    db.RECIPE_DETAIL.RemoveRange(recipe.RECIPE_DETAIL);

                    var favorites = db.FAVORITE.Where(f => f.RecipeID == recipe.ID).ToList();
                    db.FAVORITE.RemoveRange(favorites);

                    var groceries = db.GROCERY.Where(g => g.RecipeID == recipe.ID).ToList();
                    db.GROCERY.RemoveRange(groceries);

                    // XÓA BÌNH LUẬN VÀ REPORT CỦA BÌNH LUẬN ĐÓ
                    foreach (var cmt in recipe.COMMENT.ToList())
                    {
                        var cmtReports = db.REPORT.Where(r => r.TargetID == cmt.ID && r.ReportType == 2).ToList();
                        db.REPORT.RemoveRange(cmtReports);
                    }
                    db.COMMENT.RemoveRange(recipe.COMMENT);

                    // DỌN RÁC ẢNH VÀ XÓA CÁC BƯỚC LÀM (STEPTODO)
                    foreach (var step in recipe.STEPTODO.ToList())
                    {
                        FileHelper.DeleteImage("~/assets/images/steps/", step.ImageURL);
                    }
                    db.STEPTODO.RemoveRange(recipe.STEPTODO);

                    //  DỌN RÁC ẢNH VÀ XÓA THÀNH QUẢ (COOKSNAP) KÈM REPORT CỦA NÓ
                    foreach (var snap in recipe.COOKSNAP.ToList())
                    {
                        FileHelper.DeleteImage("~/assets/images/cooksnaps/", snap.ImageName);

                        var snapReports = db.REPORT.Where(r => r.TargetID == snap.ID && r.ReportType == 3).ToList();
                        db.REPORT.RemoveRange(snapReports);

                        var snapCmts = db.COOKSNAP_COMMENT.Where(c => c.CooksnapID == snap.ID).ToList();
                        db.COOKSNAP_COMMENT.RemoveRange(snapCmts);
                    }
                    db.COOKSNAP.RemoveRange(recipe.COOKSNAP);

                    // XÓA REPORT CỦA CHÍNH CÔNG THỨC NÀY
                    var recipeReports = db.REPORT.Where(r => r.TargetID == recipe.ID && r.ReportType == 1).ToList();
                    db.REPORT.RemoveRange(recipeReports);

                    // DỌN RÁC ẢNH ĐẠI DIỆN MÓN ĂN VÀ XÓA CÔNG THỨC CUỐI CÙNG
                    FileHelper.DeleteImage("~/assets/images/recipes/", recipe.MainImage);

                    db.RECIPE.Remove(recipe);
                    db.SaveChanges();

                    //  Nếu bài này đã được duyệt (nằm ngoài trang chủ) thì phải đập Cache đi
                    if (oldStatus == 1)
                    {
                        HttpContext.Cache.Remove("HomeLatestRecipes");
                    }

                    TempData["SuccessMessage"] = "Đã xóa công thức và dọn sạch mọi dữ liệu liên quan!";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Không thể xóa bài viết này. " + ex.Message;
            }
            return RedirectToAction("MyProfile", "User");
        }


        // HÀM XỬ LÝ GỬI BÁO CÁO VI PHẠM (TỪ USER)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SubmitReport(int TargetID, int ReportType, string Reason, string ReturnUrl)
        {
            if (Session["Account"] == null)
            {
                return RedirectToAction("Login", "Account", new { returnUrl = ReturnUrl });
            }

            try
            {
                int accId = ((CookingShare.Models.ACCOUNT)Session["Account"]).ID;

                var newReport = new CookingShare.Models.REPORT();
                newReport.ReporterID = accId;
                newReport.TargetID = TargetID;
                newReport.ReportType = ReportType;
                newReport.Reason = Reason;
                newReport.Status = 0;
                newReport.CreateDate = DateTime.Now;

                db.REPORT.Add(newReport);
                db.SaveChanges();

                TempData["Success"] = "Cảm ơn bạn đã báo cáo! Quản trị viên sẽ xem xét và xử lý trong thời gian sớm nhất.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Có lỗi xảy ra khi gửi báo cáo: " + ex.Message;
            }

            if (!string.IsNullOrEmpty(ReturnUrl))
            {
                return Redirect(ReturnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        // Dọn dẹp kết nối Database
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}