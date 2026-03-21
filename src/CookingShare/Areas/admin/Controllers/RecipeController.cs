using CookingShare.Models;
using CookingShare.Helpers;
using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;

namespace CookingShare.Areas.Admin.Controllers
{
    [Authorize]
    public class RecipeController : Controller
    {
        CookingShareDBEntities db = new CookingShareDBEntities();


        // HIỂN THỊ DANH SÁCH & LỌC TRẠNG THÁI
        public ActionResult Index(int? statusFilter)
        {
            var currentAcc = Session["Account"] as ACCOUNT;
            // ĐÃ SỬA: Chuẩn hóa Routing
            if (currentAcc == null || currentAcc.Role != 1) return RedirectToAction("Login", "Auth", new { area = "admin" });

            ViewBag.TotalRecipes = db.RECIPE.Count();
            ViewBag.PendingRecipes = db.RECIPE.Count(r => r.Status == 0);
            ViewBag.CurrentFilter = statusFilter;

            ViewBag.Categories = db.CATEGORY.ToList();

            // TRUYỀN DỮ LIỆU ĐỂ LÀM DROPDOWN THÊM MỚI Ở BẢNG KIỂM DUYỆT
            ViewBag.AllTags = db.TAG.ToList();
            ViewBag.AllIngredients = db.INGREDIENT.OrderBy(i => i.Name).ToList();
            ViewBag.AllUnits = db.UNIT.ToList();

            var recipes = db.RECIPE
                            .Include(r => r.ACCOUNT)
                            .Include(r => r.CATEGORY)
                            .Include(r => r.RECIPE_DETAIL.Select(d => d.INGREDIENT))
                            .Include(r => r.RECIPE_DETAIL.Select(d => d.UNIT))
                            .Include(r => r.STEPTODO)
                            .Include(r => r.TAG)
                            .AsQueryable();

            if (statusFilter.HasValue) recipes = recipes.Where(r => r.Status == statusFilter.Value);

            return View(recipes.OrderByDescending(r => r.CreateDate).ToList());
        }


        // CẬP NHẬT NỘI DUNG & THAY ĐỔI TRẠNG THÁI
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(RECIPE model, HttpPostedFileBase ImageFile, string rejectReason,
                                 int[] DetailIngredientIDs, double[] DetailQuantities,
                                 int[] StepIDs, string[] StepContents, int NewStatus,
                                 int[] ExistingTagIDs, int[] NewTagIDs,
                                 int[] NewIngredientIDs, double[] NewQuantities, int[] NewUnitIDs, string[] NewStepContents)
        {
            //  BẢO MẬT CHỐNG POSTMAN (Chỉ Admin mới được sửa)
            var currentAcc = Session["Account"] as ACCOUNT;
            if (currentAcc == null || currentAcc.Role != 1) return RedirectToAction("Login", "Auth", new { area = "admin" });

            try
            {
                var recipe = db.RECIPE.Include(r => r.RECIPE_DETAIL)
                                      .Include(r => r.STEPTODO)
                                      .Include(r => r.TAG)
                                      .FirstOrDefault(r => r.ID == model.ID);

                if (recipe != null)
                {
                    // LƯU LẠI STATUS CŨ ĐỂ KIỂM TRA XEM CÓ CẦN XÓA CACHE KHÔNG
                    int oldStatus = recipe.Status ?? 0;

                    // SỬA THÔNG TIN CƠ BẢN
                    recipe.Name = model.Name;
                    recipe.CategoryID = model.CategoryID;
                    recipe.CookTime = model.CookTime;
                    recipe.PrepTime = model.PrepTime;
                    recipe.Servings = model.Servings;
                    recipe.Difficulty = model.Difficulty;
                    recipe.Describe = model.Describe;


                    // XỬ LÝ ẢNH ĐẠI DIỆN MÓN ĂN VÀ DỌN RÁC BẰNG FILEHELPER
                    if (ImageFile != null && ImageFile.ContentLength > 0)
                    {
                        string prefix = "recipe_" + recipe.ID;
                        recipe.MainImage = FileHelper.UploadAndReplaceImage(ImageFile, "~/assets/images/recipes/", prefix, recipe.MainImage);
                    }

                    //  XỬ LÝ TAGS
                    var finalTagIds = new System.Collections.Generic.List<int>();
                    if (ExistingTagIDs != null) finalTagIds.AddRange(ExistingTagIDs);
                    if (NewTagIDs != null) finalTagIds.AddRange(NewTagIDs);

                    recipe.TAG.Clear();
                    foreach (var tagId in finalTagIds.Distinct())
                    {
                        var tagObj = db.TAG.Find(tagId);
                        if (tagObj != null) recipe.TAG.Add(tagObj);
                    }

                    //  XỬ LÝ NGUYÊN LIỆU
                    var detailsToRemove = recipe.RECIPE_DETAIL.ToList();
                    if (DetailIngredientIDs != null)
                    {
                        detailsToRemove = detailsToRemove.Where(d => !DetailIngredientIDs.Contains(d.IngredientID)).ToList();
                    }
                    db.RECIPE_DETAIL.RemoveRange(detailsToRemove);

                    if (DetailIngredientIDs != null && DetailQuantities != null)
                    {
                        for (int i = 0; i < DetailIngredientIDs.Length; i++)
                        {
                            int ingId = DetailIngredientIDs[i];
                            var detail = recipe.RECIPE_DETAIL.FirstOrDefault(d => d.IngredientID == ingId);
                            if (detail != null) detail.Quantity = DetailQuantities[i];
                        }
                    }

                    if (NewIngredientIDs != null && NewQuantities != null && NewUnitIDs != null)
                    {
                        for (int i = 0; i < NewIngredientIDs.Length; i++)
                        {
                            int iId = NewIngredientIDs[i];
                            if (!db.RECIPE_DETAIL.Any(d => d.RecipeID == recipe.ID && d.IngredientID == iId))
                            {
                                db.RECIPE_DETAIL.Add(new RECIPE_DETAIL
                                {
                                    RecipeID = recipe.ID,
                                    IngredientID = iId,
                                    Quantity = NewQuantities[i],
                                    UnitID = NewUnitIDs[i]
                                });
                            }
                        }
                    }

                    // XỬ LÝ BƯỚC LÀM VÀ DỌN RÁC ẢNH CỦA BƯỚC BỊ XÓA
                    var stepsToRemove = recipe.STEPTODO.ToList();
                    if (StepIDs != null)
                    {
                        stepsToRemove = stepsToRemove.Where(s => !StepIDs.Contains(s.ID)).ToList();
                    }


                    //  Dọn rác ảnh vật lý của những bước bị xóa BẰNG FILEHELPER
                    foreach (var step in stepsToRemove)
                    {
                        FileHelper.DeleteImage("~/assets/images/steps/", step.ImageURL);
                    }
                    db.STEPTODO.RemoveRange(stepsToRemove);

                    if (StepIDs != null && StepContents != null)
                    {
                        for (int i = 0; i < StepIDs.Length; i++)
                        {
                            int stepId = StepIDs[i];
                            var step = recipe.STEPTODO.FirstOrDefault(s => s.ID == stepId);
                            if (step != null && !string.IsNullOrWhiteSpace(StepContents[i])) step.Content = StepContents[i];
                        }
                    }

                    if (NewStepContents != null)
                    {
                        int maxStep = db.STEPTODO.Where(s => s.RecipeID == recipe.ID).Select(s => s.StepOrder).DefaultIfEmpty(0).Max();
                        foreach (var content in NewStepContents)
                        {
                            if (!string.IsNullOrWhiteSpace(content))
                            {
                                maxStep++;
                                db.STEPTODO.Add(new STEPTODO { RecipeID = recipe.ID, StepOrder = maxStep, Content = content });
                            }
                        }
                    }

                    // LƯU TRẠNG THÁI 
                    recipe.Status = NewStatus;
                    if (NewStatus == 2)
                    {
                        recipe.RejectReason = rejectReason;
                        TempData["Success"] = $"Đã TỪ CHỐI bài viết: {recipe.Name}";
                    }
                    else
                    {
                        recipe.RejectReason = null;
                        if (NewStatus == 1) TempData["Success"] = $"Đã PHÊ DUYỆT bài viết: {recipe.Name}";
                        else TempData["Success"] = $"Đã LƯU bài viết: {recipe.Name} (Đang chờ duyệt)";
                    }

                    db.SaveChanges();

                    // BỌC THÉP CACHE: Cập nhật trang chủ nếu Admin thay đổi trạng thái (từ Ẩn sang Hiện, hoặc ngược lại)
                    if (oldStatus != NewStatus && (oldStatus == 1 || NewStatus == 1))
                    {
                        HttpContext.Cache.Remove("HomeLatestRecipes");
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Có lỗi xảy ra: " + ex.Message;
            }

            return RedirectToAction("Index");
        }


        //  XÓA CÔNG THỨC (CỰC KỲ QUAN TRỌNG: DỌN RÁC & CASCADE DELETE)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            // BẢO MẬT CHỐNG POSTMAN
            var currentAcc = Session["Account"] as ACCOUNT;
            if (currentAcc == null || currentAcc.Role != 1) return RedirectToAction("Login", "Auth", new { area = "admin" });

            try
            {
                var recipe = db.RECIPE.Include(r => r.RECIPE_DETAIL)
                                      .Include(r => r.STEPTODO)
                                      .Include(r => r.COOKSNAP)
                                      .Include(r => r.COMMENT)
                                      .FirstOrDefault(r => r.ID == id);

                if (recipe != null)
                {
                    int oldStatus = recipe.Status ?? 0;

                    // XÓA BẢNG TRUNG GIAN VÀ BẢNG LIÊN QUAN TRỰC TIẾP
                    recipe.TAG.Clear();
                    db.RECIPE_DETAIL.RemoveRange(recipe.RECIPE_DETAIL);

                    // XÓA BÌNH LUẬN CÔNG THỨC (VÀ REPORT CỦA BÌNH LUẬN ĐÓ)
                    foreach (var cmt in recipe.COMMENT.ToList())
                    {
                        var cmtReports = db.REPORT.Where(r => r.TargetID == cmt.ID && r.ReportType == 2).ToList();
                        db.REPORT.RemoveRange(cmtReports);
                    }
                    db.COMMENT.RemoveRange(recipe.COMMENT);

                    // DỌN RÁC ẢNH & XÓA BƯỚC LÀM (STEPTODO)
                    foreach (var step in recipe.STEPTODO.ToList())
                    {
                        FileHelper.DeleteImage("~/assets/images/steps/", step.ImageURL); // Dùng FileHelper
                    }
                    db.STEPTODO.RemoveRange(recipe.STEPTODO);

                    //  DỌN RÁC ẢNH & XÓA THÀNH QUẢ (COOKSNAP) KÈM BÌNH LUẬN CỦA NÓ
                    foreach (var snap in recipe.COOKSNAP.ToList())
                    {
                        FileHelper.DeleteImage("~/assets/images/cooksnaps/", snap.ImageName); // Dùng FileHelper

                        var snapReports = db.REPORT.Where(r => r.TargetID == snap.ID && r.ReportType == 3).ToList();
                        db.REPORT.RemoveRange(snapReports);

                        var snapCmts = db.COOKSNAP_COMMENT.Where(c => c.CooksnapID == snap.ID).ToList();
                        db.COOKSNAP_COMMENT.RemoveRange(snapCmts);
                    }
                    db.COOKSNAP.RemoveRange(recipe.COOKSNAP);

                    // XÓA REPORT CỦA CHÍNH CÔNG THỨC NÀY
                    var recipeReports = db.REPORT.Where(r => r.TargetID == recipe.ID && r.ReportType == 1).ToList();
                    db.REPORT.RemoveRange(recipeReports);

                    // DỌN RÁC ẢNH ĐẠI DIỆN MÓN ĂN & XÓA CÔNG THỨC
                    FileHelper.DeleteImage("~/assets/images/recipes/", recipe.MainImage); // Dùng FileHelper

                    db.RECIPE.Remove(recipe);
                    db.SaveChanges();

                    // BỌC THÉP CACHE: Nếu xóa một bài đang hiện trên trang chủ, đập vỡ Cache
                    if (oldStatus == 1)
                    {
                        HttpContext.Cache.Remove("HomeLatestRecipes");
                    }

                    TempData["Success"] = "Đã xóa vĩnh viễn công thức và dọn sạch mọi dữ liệu liên quan!";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Không thể xóa bài viết này. " + ex.Message;
            }
            return RedirectToAction("Index");
        }

        // DUYỆT NHANH (TỪ TRANG CHỦ DASHBOARD)
        [HttpPost]
        [ValidateAntiForgeryToken] // Đã bổ sung chống giả mạo cho Duyệt nhanh
        public ActionResult Approve(int id)
        {
            var currentAcc = Session["Account"] as ACCOUNT;
            if (currentAcc == null || currentAcc.Role != 1) return RedirectToAction("Login", "Auth", new { area = "admin" });

            try
            {
                var recipe = db.RECIPE.Find(id);
                if (recipe != null)
                {
                    recipe.Status = 1;
                    recipe.RejectReason = null;
                    db.SaveChanges();

                    // BỌC THÉP CACHE: Duyệt bài xong là trang chủ phải hiện lên ngay!
                    HttpContext.Cache.Remove("HomeLatestRecipes");

                    TempData["Success"] = $"Đã duyệt nhanh công thức: {recipe.Name}";
                }
            }
            catch (Exception ex) { TempData["Error"] = "Có lỗi xảy ra: " + ex.Message; }

            // Redirect về đúng Home của Admin
            return RedirectToAction("Index", "Home", new { area = "admin" });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) { db.Dispose(); }
            base.Dispose(disposing);
        }
    }
}