using CookingShare.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;

namespace CookingShare.Controllers
{
    public class RecipeController : Controller
    {
        CookingShareDBEntities db = new CookingShareDBEntities();


        // 1. HÀM HIỂN THỊ DANH SÁCH CÔNG THỨC VÀ XỬ LÝ LỌC TÌM KIẾM

        public ActionResult Index(string search, int? categoryId, string difficulty, List<int> allergyIds, List<int> fridgeIngredientIds, string sort = "newest", int page = 1)
        {
            var query = db.RECIPE.Where(r => r.Status == 1).AsQueryable();

            // 1. Lọc theo Từ khóa
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(r => r.Name.Contains(search));
                ViewBag.SearchKeyword = search;
            }

            // 2. Lọc theo Danh mục
            if (categoryId.HasValue)
            {
                query = query.Where(r => r.CategoryID == categoryId);
                ViewBag.CurrentCategory = categoryId;
            }

            // 3. Lọc theo Độ khó
            if (!string.IsNullOrEmpty(difficulty))
            {
                query = query.Where(r => r.Difficulty == difficulty);
                ViewBag.CurrentDifficulty = difficulty;
            }

            // 4. LỌC THEO DỊ ỨNG (TRÁNH NGUYÊN LIỆU)
            if (allergyIds != null && allergyIds.Count > 0)
            {
                query = query.Where(r => !r.RECIPE_DETAIL.Any(d => allergyIds.Contains(d.IngredientID)));
                ViewBag.SelectedAllergies = allergyIds;
            }

            // 5. TỦ LẠNH THÔNG MINH (CHỨA NGUYÊN LIỆU)
            if (fridgeIngredientIds != null && fridgeIngredientIds.Count > 0)
            {
                // Yêu cầu công thức phải chứa ÍT NHẤT MỘT trong những nguyên liệu được chọn
                query = query.Where(r => r.RECIPE_DETAIL.Any(d => fridgeIngredientIds.Contains(d.IngredientID)));
                ViewBag.SelectedFridgeIngredients = fridgeIngredientIds;
            }

            // 6. Sắp xếp
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

            // Truyền dữ liệu cho bộ lọc
            ViewBag.Categories = db.CATEGORY.ToList();
            ViewBag.Ingredients = db.INGREDIENT.OrderBy(i => i.Name).ToList();

            // Truy xuất Dị ứng và Danh sách Yêu thích của người dùng
            List<int> myAllergyIds = new List<int>();
            List<int> myFavoriteIds = new List<int>();

            if (Session["Account"] != null)
            {
                int userId = ((ACCOUNT)Session["Account"]).ID;
                var currentUser = db.ACCOUNT.FirstOrDefault(u => u.ID == userId);
                if (currentUser != null)
                {
                    myAllergyIds = currentUser.INGREDIENT.Select(i => i.ID).ToList();

                    // CÁCH LẤY DANH SÁCH ID MÓN ĂN ĐÃ LƯU AN TOÀN NHẤT:
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

            var recipes = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            ViewBag.TotalCount = totalItems;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View(recipes);
        }


        // Nút nhận ID của món ăn từ trang chủ truyền sang
        public ActionResult Detail(int id)
        {
            // Tìm món ăn có ID tương ứng trong Database
            var recipe = db.RECIPE.Find(id);

            // Nếu không tìm thấy (người dùng gõ bậy ID) thì báo lỗi 404
            if (recipe == null)
            {
                return HttpNotFound();
            }

            // Tự động tăng lượt xem mỗi khi load trang
            recipe.Views = (recipe.Views ?? 0) + 1;
            db.SaveChanges();

            // Kiểm tra trạng thái nút Yêu thích (Favorite) và Theo dõi (Follow) để hiển thị đúng trên giao diện
            ViewBag.IsFavorited = false;
            ViewBag.IsFollowed = false;

            if (Session["Account"] != null)
            {
                int currentAccId = ((CookingShare.Models.ACCOUNT)Session["Account"]).ID;

                // Trực tiếp soi thẳng vào DB, chuẩn xác 100%
                ViewBag.IsFavorited = db.FAVORITE.Any(f => f.RecipeID == id && f.AccountID == currentAccId);

                if (recipe.AccountID != null)
                {
                    ViewBag.IsFollowed = db.FOLLOW.Any(f => f.FollowerID == currentAccId && f.FollowedID == recipe.AccountID);
                }
            }

            // Truyền món ăn tìm được sang cho giao diện (View)
            return View(recipe);
        }

        // Hàm xử lý Thêm bình luận
        [HttpPost]
        public ActionResult AddComment(int RecipeID, string Content, int Rating)
        {
            // 1. BẢO MẬT: Kiểm tra đăng nhập (Chỉ cần 1 lần)
            if (Session["Account"] == null)
            {
                // Chưa đăng nhập thì đẩy về trang Login, kèm theo URL để lát đăng nhập xong quay lại đúng món này
                return RedirectToAction("Login", "Account", new { returnUrl = "/Recipe/Detail/" + RecipeID });
            }

            // Lấy thông tin user hiện tại từ Session
            var currentUser = (CookingShare.Models.ACCOUNT)Session["Account"];

            // 2. KIỂM TRA DỮ LIỆU: Nếu nội dung bình luận rỗng thì không làm gì cả, quay lại trang
            if (string.IsNullOrWhiteSpace(Content))
            {
                return RedirectToAction("Detail", "Recipe", new { id = RecipeID });
            }

            // 3. TÌM CÔNG THỨC TRONG DATABASE
            var recipe = db.RECIPE.Find(RecipeID);
            if (recipe == null)
            {
                return HttpNotFound("Không tìm thấy công thức này.");
            }

            // 4. TẠO BÌNH LUẬN MỚI
            var newComment = new CookingShare.Models.COMMENT();
            newComment.RecipeID = RecipeID;
            newComment.AccountID = currentUser.ID;
            newComment.Content = Content;
            newComment.Rating = Rating;
            newComment.CreateDate = DateTime.Now;
            newComment.Status = 1;

            db.COMMENT.Add(newComment);

            // 5. TẠO THÔNG BÁO CHO CHỦ CÔNG THỨC
            // Kiểm tra an toàn: Đảm bảo công thức có AccountID (HasValue) VÀ ID đó khác với người đang comment
            if (recipe.AccountID.HasValue && recipe.AccountID.Value != currentUser.ID)
            {
                var newNoti = new CookingShare.Models.NOTIFICATION();
                newNoti.AccountID = recipe.AccountID.Value; // Lấy ID của chủ món
                newNoti.Content = $"{currentUser.UserName} đã bình luận về món '{recipe.Name}' của bạn.";
                newNoti.LinkURL = $"/Recipe/Detail/{recipe.ID}";
                newNoti.IsRead = false;
                newNoti.CreateDate = DateTime.Now;

                db.NOTIFICATION.Add(newNoti);
            }

            // 6. LƯU TẤT CẢ VÀO DATABASE (Lưu 1 lần cho cả Bình luận và Thông báo)
            db.SaveChanges();

            // 7. HOÀN THÀNH: Tải lại trang chi tiết món ăn
            return RedirectToAction("Detail", "Recipe", new { id = RecipeID });
        }

        // Hàm xử lý việc tải ảnh thành quả (Cooksnap) lên hệ thống
        [HttpPost]
        public ActionResult UploadCooksnap(int RecipeID, string Content, System.Web.HttpPostedFileBase ImageFile)
        {
            // 1. BẢO MẬT: Kiểm tra đăng nhập
            if (Session["Account"] == null)
            {
                return RedirectToAction("Login", "Account", new { returnUrl = "/Recipe/Detail/" + RecipeID });
            }

            var sessionUser = (CookingShare.Models.ACCOUNT)Session["Account"];

            // 2. KIỂM TRA TỒN TẠI: Lấy thông tin món ăn để biết ai là chủ
            var recipe = db.RECIPE.Find(RecipeID);
            if (recipe == null)
            {
                return HttpNotFound("Không tìm thấy công thức này.");
            }

            // 3. XỬ LÝ ẢNH & LƯU THÀNH QUẢ
            if (ImageFile != null && ImageFile.ContentLength > 0)
            {
                // Lấy đuôi file và tạo tên file mới không trùng lặp
                string extension = System.IO.Path.GetExtension(ImageFile.FileName);
                string fileName = "cooksnap_" + RecipeID + "_" + sessionUser.ID + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + extension;

                // Đường dẫn thư mục lưu ảnh
                string path = Server.MapPath("~/assets/images/cooksnaps/");
                if (!System.IO.Directory.Exists(path))
                {
                    System.IO.Directory.CreateDirectory(path);
                }

                // Lưu file vật lý
                ImageFile.SaveAs(path + fileName);

                // Tạo mới một bản ghi COOKSNAP
                var newSnap = new CookingShare.Models.COOKSNAP();
                newSnap.RecipeID = RecipeID;
                newSnap.AccountID = sessionUser.ID;
                newSnap.ImageName = fileName;
                newSnap.Content = Content;
                newSnap.CreateDate = DateTime.Now;

                db.COOKSNAP.Add(newSnap);

                // 4. TẠO THÔNG BÁO CHO CHỦ CÔNG THỨC

                // Đảm bảo công thức có chủ (AccountID có giá trị) VÀ chủ đó không phải người đang đăng ảnh
                if (recipe.AccountID.HasValue && recipe.AccountID.Value != sessionUser.ID)
                {
                    var newNoti = new CookingShare.Models.NOTIFICATION();
                    newNoti.AccountID = recipe.AccountID.Value; // Gửi cho chủ món ăn
                    newNoti.Content = $"Tuyệt vời! {sessionUser.UserName} vừa thực hành món '{recipe.Name}' của bạn và khoe thành quả.";
                    newNoti.LinkURL = $"/Recipe/Detail/{recipe.ID}";
                    newNoti.IsRead = false;
                    newNoti.CreateDate = DateTime.Now;

                    db.NOTIFICATION.Add(newNoti);
                }

                // 5. LƯU TẤT CẢ VÀO DATABASE (Lưu ảnh thành quả và Thông báo trong 1 lượt)
                db.SaveChanges();

                TempData["SuccessMessage"] = "Đăng thành quả thành công!";
            }
            else
            {
                // Fail-safe: Lỡ có ai vượt rào bấm gửi mà không có ảnh
                TempData["ErrorMessage"] = "Vui lòng chọn ảnh trước khi khoe thành quả nhé!";
            }

            // 6. HOÀN THÀNH: Quay lại trang Chi tiết công thức
            return RedirectToAction("Detail", new { id = RecipeID });
        }

        // 1. Hàm lấy chi tiết một bức ảnh thành quả (Trả về một đoạn HTML nhỏ - Partial View)
        public ActionResult GetCooksnapDetail(int id)
        {
            // Tìm bức ảnh theo ID
            var snap = db.COOKSNAP.FirstOrDefault(c => c.ID == id);
            if (snap == null) return HttpNotFound();

            // Trả về file _CooksnapDetail.cshtml 
            return PartialView("_CooksnapDetail", snap);
        }

        // 2. Hàm xử lý khi người dùng Gửi bình luận vào bức ảnh
        [HttpPost]
        public ActionResult AddCooksnapComment(int CooksnapID, int RecipeID, string Content)
        {
            if (Session["Account"] == null)
            {
                // Chưa đăng nhập thì bắt đi đăng nhập
                return RedirectToAction("Login", "Account", new { returnUrl = "/Recipe/Detail/" + RecipeID });
            }

            var sessionUser = (CookingShare.Models.ACCOUNT)Session["Account"];

            // Lưu bình luận mới vào Database
            CookingShare.Models.COOKSNAP_COMMENT newCmt = new CookingShare.Models.COOKSNAP_COMMENT();
            newCmt.CooksnapID = CooksnapID;
            newCmt.AccountID = sessionUser.ID;
            newCmt.Content = Content;
            newCmt.CreateDate = System.DateTime.Now;

            db.COOKSNAP_COMMENT.Add(newCmt); 
            db.SaveChanges();

            // Lưu xong thì load lại trang chi tiết món ăn
            return RedirectToAction("Detail", new { id = RecipeID });
        }

        [HttpPost]
        public JsonResult ToggleFavorite(int recipeId)
        {
            if (Session["Account"] == null) return Json(new { success = false, message = "Vui lòng đăng nhập!" });
            int accId = ((CookingShare.Models.ACCOUNT)Session["Account"]).ID;

            var existing = db.FAVORITE.FirstOrDefault(f => f.RecipeID == recipeId && f.AccountID == accId);
            bool isSaved = false;

            if (existing != null)
            {
                db.FAVORITE.Remove(existing); // Đã lưu thì bỏ lưu
            }
            else
            {
                CookingShare.Models.FAVORITE fav = new CookingShare.Models.FAVORITE { AccountID = accId, RecipeID = recipeId, CreateDate = DateTime.Now };
                db.FAVORITE.Add(fav);
                isSaved = true; // Chưa lưu thì thêm vào
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

        // Hàm đếm số lượng món trong giỏ để hiển thị lên Navbar
        public ActionResult CartCount()
        {
            int count = 0;
            if (Session["Account"] != null)
            {
                int accId = ((CookingShare.Models.ACCOUNT)Session["Account"]).ID;
                count = db.GROCERY.Count(g => g.AccountID == accId); // Đếm số món trong bảng GROCERY
            }
            return PartialView("_CartCount", count);
        }

        // 1. Class nhỏ để chứa dữ liệu nguyên liệu đã gộp
        public class ChotDonViewModel
        {
            public string TenNguyenLieu { get; set; }
            public double TongSoLuong { get; set; }
            public string DonViTinh { get; set; }
        }

        // 2. Hàm load trang Giỏ Đi Chợ
        public ActionResult Grocery()
        {
            if (Session["Account"] == null) return RedirectToAction("Login", "Account");

            int accId = ((CookingShare.Models.ACCOUNT)Session["Account"]).ID;

            // Lấy các món ăn trong giỏ
            var gioHang = db.GROCERY.Where(g => g.AccountID == accId).ToList();
            var danhSachMonAn = gioHang.Select(g => g.RECIPE).ToList();

            // Lấy tất cả nguyên liệu và GỘP lại
            var tatCaNguyenLieu = gioHang.SelectMany(g => g.RECIPE.RECIPE_DETAIL).ToList();
            var nguyenLieuGop = tatCaNguyenLieu
                .GroupBy(d => new { d.IngredientID, Ten = d.INGREDIENT.Name, DonVi = (d.UNIT != null ? d.UNIT.UnitName : "") })
                .Select(group => new ChotDonViewModel
                {
                    TenNguyenLieu = group.Key.Ten,
                    DonViTinh = group.Key.DonVi,
                    TongSoLuong = Math.Round(group.Sum(x => x.Quantity), 2) // Cộng dồn và làm tròn 2 chữ số
                })
                .OrderBy(x => x.TenNguyenLieu)
                .ToList();

            ViewBag.DanhSachDiCho = nguyenLieuGop;
            return View(danhSachMonAn);
        }

        // 3. Hàm Xóa toàn bộ giỏ hàng (Gọi bằng AJAX)
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

        // Hàm Tải file Word danh sách đi chợ

        [HttpPost] 
        public ActionResult ExportWord(List<string> itemNames)
        {
            if (Session["Account"] == null) return RedirectToAction("Login", "Account");
            int accId = ((CookingShare.Models.ACCOUNT)Session["Account"]).ID;

            // 1. Lấy dữ liệu
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

            // Chỉ giữ lại những nguyên liệu có tên nằm trong danh sách "itemNames" gửi từ Web lên
            if (itemNames != null && itemNames.Count > 0)
            {
                nguyenLieuGop = nguyenLieuGop.Where(x => itemNames.Contains(x.TenNguyenLieu)).ToList();
            }
            else
            {
                // Nếu không gửi lên gì (tick hết rồi) thì trả về file rỗng hoặc báo lỗi
                nguyenLieuGop = new List<ChotDonViewModel>();
            }

            nguyenLieuGop = nguyenLieuGop.OrderBy(x => x.TenNguyenLieu).ToList();

            // 2. Dùng HTML vẽ nội dung Word
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
            html += "<br/><p style='text-align:center; color: gray;'><i>Được tạo tự động từ ứng dụng CookingShare by Hoang Anh</i></p>";
            html += "</body></html>";

            // 3. Xuất file
            byte[] fileBytes = System.Text.Encoding.UTF8.GetBytes(html);
            return File(fileBytes, "application/msword", "DanhSachDiCho_CookingShare.doc");
        }

    
        // 1. HÀM HIỂN THỊ GIAO DIỆN ĐĂNG CÔNG THỨC (GET)
    
        [HttpGet]
        public ActionResult Create()
        {
            if (Session["Account"] == null) return RedirectToAction("Login", "Account");

            // --- LẤY DỮ LIỆU TỪ DATABASE ĐỂ ĐỔ VÀO DROPDOWN ---
            // 1. Danh mục món ăn
            ViewBag.Categories = db.CATEGORY.ToList();

            // 2. Nguyên liệu (Sắp xếp theo A-Z cho dễ tìm)
            ViewBag.Ingredients = db.INGREDIENT.OrderBy(i => i.Name).ToList();

            // 3. Đơn vị tính
            ViewBag.Units = db.UNIT.ToList();

            return View();
        }

        
        // 2. HÀM XỬ LÝ LƯU DỮ LIỆU KHI BẤM "ĐĂNG CÔNG THỨC" (POST)
      
        [HttpPost]
        [ValidateInput(false)] // Cho phép người dùng gõ ký tự đặc biệt như <, > (VD: < 500g)
        public ActionResult Create(RECIPE recipe,
                                   HttpPostedFileBase ImageFile,
                                   int[] IngredientIDs,
                                   double[] IngredientQuantities,
                                   int[] UnitIDs,
                                   string[] StepContents,
                                   HttpPostedFileBase[] StepImages)
        {
            // 1. Kiểm tra bảo mật: Chắc chắn người dùng đã đăng nhập
            if (Session["Account"] == null) return RedirectToAction("Login", "Account");
            int accId = ((CookingShare.Models.ACCOUNT)Session["Account"]).ID;

            try
            {
           
                // BƯỚC 1: LƯU THÔNG TIN MÓN ĂN (BẢNG RECIPE)
          
                recipe.AccountID = accId;
                recipe.CreateDate = DateTime.Now;
                recipe.Views = 0;
                recipe.Status = 0; 

                // Xử lý lưu Ảnh đại diện món ăn
                if (ImageFile != null && ImageFile.ContentLength > 0)
                {
                    string extension = Path.GetExtension(ImageFile.FileName);
                    // Đổi tên ảnh theo công thức: "Recipe_ThoiGian.jpg" để KHÔNG BAO GIỜ BỊ TRÙNG
                    string fileName = "Recipe_" + DateTime.Now.ToString("yyyyMMddHHmmss") + extension;
                    string path = Path.Combine(Server.MapPath("~/assets/images/"), fileName);
                    ImageFile.SaveAs(path);

                    recipe.MainImage = fileName; // Lưu tên file vào Database
                }
                else
                {
                    recipe.MainImage = "default_recipe.jpg"; // Ảnh dự phòng nếu lỗi
                }

                db.RECIPE.Add(recipe);
                db.SaveChanges(); 

                
                // BƯỚC 2: LƯU NGUYÊN LIỆU (BẢNG RECIPE_DETAIL)
            
                if (IngredientIDs != null && IngredientIDs.Length > 0)
                {
                    // Dùng vòng lặp for để quét qua mảng dữ liệu gửi lên
                    for (int i = 0; i < IngredientIDs.Length; i++)
                    {
                        if (IngredientIDs[i] > 0) // Tránh lưu dòng trống
                        {
                            var detail = new RECIPE_DETAIL();
                            detail.RecipeID = recipe.ID; // Lấy ID của món ăn vừa tạo ở Bước 1
                            detail.IngredientID = IngredientIDs[i];
                            detail.Quantity = IngredientQuantities[i];
                            detail.UnitID = UnitIDs[i];

                            db.RECIPE_DETAIL.Add(detail);
                        }
                    }
                }

              
                // BƯỚC 3: LƯU CÁC BƯỚC THỰC HIỆN (BẢNG STEPTODO)
               
                if (StepContents != null && StepContents.Length > 0)
                {
                    for (int i = 0; i < StepContents.Length; i++)
                    {
                        if (!string.IsNullOrWhiteSpace(StepContents[i]))
                        {
                            var step = new STEPTODO();
                            step.RecipeID = recipe.ID;
                            step.StepOrder = i + 1; // Số thứ tự tự động tăng (1, 2, 3...)
                            step.Content = StepContents[i];

                            // Kiểm tra xem bước này người dùng có upload ảnh không
                            if (StepImages != null && i < StepImages.Length && StepImages[i] != null && StepImages[i].ContentLength > 0)
                            {
                                var sFile = StepImages[i];
                                string sExt = Path.GetExtension(sFile.FileName);
                                // Tên ảnh bước làm: "Step_RecipeID_SoThuTu_ThoiGian.jpg"
                                string sFileName = $"Step_{recipe.ID}_{step.StepOrder}_{DateTime.Now.ToString("HHmmss")}{sExt}";
                                string sPath = Path.Combine(Server.MapPath("~/assets/images/"), sFileName);
                                sFile.SaveAs(sPath);

                                step.ImageURL = sFileName;
                            }

                            db.STEPTODO.Add(step);
                        }
                    }
                }

                // Chốt sổ: Lưu toàn bộ Chi tiết và Bước làm vào Database
                db.SaveChanges();

                // Thành công  -> Chuyển hướng người dùng đến trang Chi tiết của món ăn vừa tạo để họ chiêm ngưỡng!
                return RedirectToAction("Detail", "Recipe", new { id = recipe.ID });
            }
            catch (Exception ex)
            {
                // Nếu có lỗi xảy ra (Ví dụ mất mạng, lỗi DB...)
                ViewBag.ErrorMessage = "Có lỗi xảy ra: " + ex.Message;

                // Phải nạp lại dữ liệu cho Dropdown trước khi trả về View lỗi, tránh trường hợp web văng
                ViewBag.Categories = db.CATEGORY.ToList();
                ViewBag.Ingredients = db.INGREDIENT.OrderBy(i => i.Name).ToList();
                ViewBag.Units = db.UNIT.ToList();

                return View(recipe);
            }
        }



    }
}