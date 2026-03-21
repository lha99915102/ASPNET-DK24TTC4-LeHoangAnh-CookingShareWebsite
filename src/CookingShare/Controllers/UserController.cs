using CookingShare.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CookingShare.Controllers
{
    public class UserController : Controller
    {
        CookingShareDBEntities db = new CookingShareDBEntities();

        // TRANG CÁ NHÂN CÔNG KHAI
        public ActionResult PublicProfile(int? id)
        {
            if (id == null) return RedirectToAction("Index", "Home");

            try
            {
                var user = db.ACCOUNT.Include(u => u.PROFILE).FirstOrDefault(u => u.ID == id && u.Status == 1);

                if (user == null) return HttpNotFound("Không tìm thấy đầu bếp này!");

                // Đếm trực tiếp dưới Database, không kéo dữ liệu lên RAM
                ViewBag.RecipeCount = db.RECIPE.Count(r => r.AccountID == id && r.Status == 1);
                ViewBag.FollowerCount = db.FOLLOW.Count(f => f.FollowedID == id);
                ViewBag.FollowingCount = db.FOLLOW.Count(f => f.FollowerID == id);

                ViewBag.IsFollowing = false;
                ViewBag.IsSelf = false;

                if (Session["Account"] != null)
                {
                    int loggedInUserId = ((CookingShare.Models.ACCOUNT)Session["Account"]).ID;

                    if (loggedInUserId == id)
                    {
                        ViewBag.IsSelf = true;
                    }
                    else
                    {
                        ViewBag.IsFollowing = db.FOLLOW.Any(f => f.FollowerID == loggedInUserId && f.FollowedID == id);
                    }
                }

                // Chỉ lấy ảnh thành quả hợp lệ (Status == 1) và Include RECIPE để in tên món
                var cooksnaps = db.COOKSNAP
                                   .Include(c => c.RECIPE)
                                   .Where(c => c.AccountID == id && !string.IsNullOrEmpty(c.ImageName) && c.Status == 1)
                                   .OrderByDescending(c => c.CreateDate)
                                   .ToList();

                ViewBag.Cooksnaps = cooksnaps;
                ViewBag.CooksnapCount = cooksnaps.Count;

                return View(user);
            }
            catch (Exception)
            {
                return HttpNotFound("Hệ thống đang bảo trì dữ liệu người dùng.");
            }
        }


        // TRANG CÀI ĐẶT CÁ NHÂN (MY PROFILE)
        [HttpGet]
        public ActionResult MyProfile()
        {
            if (Session["Account"] == null) return RedirectToAction("Login", "Account");

            int sessionId = ((CookingShare.Models.ACCOUNT)Session["Account"]).ID;

            // Tối ưu truy vấn: Include sẵn PROFILE và INGREDIENT
            var currentUser = db.ACCOUNT
                                .Include(u => u.PROFILE)
                                .Include(u => u.INGREDIENT)
                                .FirstOrDefault(u => u.ID == sessionId);

            if (currentUser == null) return RedirectToAction("Login", "Account");

            ViewBag.FollowerCount = db.FOLLOW.Count(f => f.FollowedID == currentUser.ID);

            // Include RECIPE và ACCOUNT (tác giả món) cho danh sách Favorite
            ViewBag.Favorites = db.FAVORITE
                                  .Include(f => f.RECIPE)
                                  .Include(f => f.RECIPE.ACCOUNT)
                                  .Where(f => f.AccountID == currentUser.ID)
                                  .OrderByDescending(f => f.CreateDate)
                                  .ToList();

            ViewBag.Notifications = db.NOTIFICATION.Where(n => n.AccountID == currentUser.ID).OrderBy(n => n.IsRead).ThenByDescending(n => n.CreateDate).ToList();
            ViewBag.UnreadNotiCount = db.NOTIFICATION.Count(n => n.AccountID == currentUser.ID && n.IsRead == false);

            // TÁI SỬ DỤNG BỘ NHỚ ĐỆM (CACHE) CHO BẢNG NGUYÊN LIỆU ĐỂ GIẢM TẢI DB
            var allIngredients = HttpContext.Cache["GlobalIngredients"] as List<INGREDIENT>;
            if (allIngredients == null)
            {
                allIngredients = db.INGREDIENT.OrderBy(i => i.Name).ToList();
                HttpContext.Cache.Insert("GlobalIngredients", allIngredients, null, DateTime.Now.AddHours(24), System.Web.Caching.Cache.NoSlidingExpiration);
            }
            ViewBag.AllIngredients = allIngredients;

            ViewBag.UserAllergies = currentUser.INGREDIENT.Select(i => i.ID).ToList();

            return View(currentUser);
        }


        // CẬP NHẬT THÔNG TIN CÁ NHÂN
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateProfile(FormCollection form, HttpPostedFileBase AvatarFile, HttpPostedFileBase KitchenFile, List<int> AllergyIDs)
        {
            if (Session["Account"] == null) return RedirectToAction("Login", "Account");

            var sessionUser = (CookingShare.Models.ACCOUNT)Session["Account"];
            var userInDb = db.ACCOUNT.FirstOrDefault(u => u.ID == sessionUser.ID);
            var profile = db.PROFILE.FirstOrDefault(p => p.AccountID == sessionUser.ID);

            if (userInDb != null && profile != null)
            {
                userInDb.Phone = form["Phone"];

                profile.FullName = form["FullName"];
                profile.Bio = form["Bio"];
                profile.Gender = form["Gender"];
                profile.Lifestyle = form["Lifestyle"];
                profile.JobTitle = form["JobTitle"];
                profile.Education = form["Education"];
                profile.CookingPhilosophy = form["CookingPhilosophy"];
                profile.SocialLink = form["SocialLink"];

                DateTime parsedDate;
                if (DateTime.TryParse(form["BirthDay"], out parsedDate)) profile.BirthDay = parsedDate;

                // Xử lý dấu thập phân
                double parsedHeight;
                if (double.TryParse(form["Height"]?.Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out parsedHeight)) profile.Height = parsedHeight;

                double parsedWeight;
                if (double.TryParse(form["Weight"]?.Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out parsedWeight)) profile.Weight = parsedWeight;

                int parsedCalo;
                if (int.TryParse(form["CaloDaily"], out parsedCalo)) profile.CaloDaily = parsedCalo;

                // DÙNG FILE HELPER CHUẨN HÓA UPLOAD ẢNH

                // Cập nhật Avatar
                if (AvatarFile != null && AvatarFile.ContentLength > 0)
                {
                    string prefix = "avatar_" + sessionUser.ID;
                    profile.Avatar = FileHelper.UploadAndReplaceImage(AvatarFile, "~/assets/images/avatars/", prefix, profile.Avatar);
                }

                // Cập nhật Ảnh Căn Bếp (Kitchen Image)
                if (KitchenFile != null && KitchenFile.ContentLength > 0)
                {
                    string prefix = "kitchen_" + sessionUser.ID;
                    profile.KitchenImage = FileHelper.UploadAndReplaceImage(KitchenFile, "~/assets/images/kitchens/", prefix, profile.KitchenImage);
                }

                userInDb.INGREDIENT.Clear();
                if (AllergyIDs != null && AllergyIDs.Count > 0)
                {
                    foreach (var ingId in AllergyIDs)
                    {
                        var ingredient = db.INGREDIENT.Find(ingId);
                        if (ingredient != null) userInDb.INGREDIENT.Add(ingredient);
                    }
                }

                string OldPassword = form["OldPassword"];
                string NewPassword = form["NewPassword"];
                string ConfirmPassword = form["ConfirmPassword"];

                if (!string.IsNullOrEmpty(OldPassword))
                {
                    string hashedOldPass = CookingShare.Models.SecurityHelper.HashPasswordSHA256(OldPassword);
                    if (userInDb.Password != hashedOldPass)
                    {
                        TempData["ErrorMessage"] = "Mật khẩu cũ không chính xác!";
                        return RedirectToAction("MyProfile");
                    }
                    if (string.IsNullOrEmpty(NewPassword) || NewPassword != ConfirmPassword)
                    {
                        TempData["ErrorMessage"] = "Mật khẩu mới và Xác nhận mật khẩu không khớp hoặc bị bỏ trống!";
                        return RedirectToAction("MyProfile");
                    }
                    userInDb.Password = CookingShare.Models.SecurityHelper.HashPasswordSHA256(NewPassword);
                }

                db.SaveChanges();

                Session["Account"] = userInDb;
                TempData["SuccessMessage"] = "Cập nhật thông tin thành công!";
            }

            return RedirectToAction("MyProfile");
        }


        [HttpPost]
        public ActionResult SubmitReport(int targetId, string reason, int type)
        {
            try
            {
                if (Session["Account"] == null) return Json(new { success = false, message = "Vui lòng đăng nhập!" });
                int reporterId = ((CookingShare.Models.ACCOUNT)Session["Account"]).ID;
                if (reporterId == targetId && type == 2) return Json(new { success = false, message = "Bạn không thể tự báo cáo chính mình!" });

                var newReport = new REPORT();
                newReport.ReporterID = reporterId;
                newReport.TargetID = targetId;
                newReport.ReportType = type;
                newReport.Reason = reason;
                newReport.CreateDate = DateTime.Now;
                newReport.Status = 0;

                db.REPORT.Add(newReport);
                db.SaveChanges();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        [HttpPost]
        public ActionResult ToggleFollow(int targetId)
        {
            try
            {
                if (Session["Account"] == null) return Json(new { success = false, message = "Vui lòng đăng nhập!" });
                int followerId = ((CookingShare.Models.ACCOUNT)Session["Account"]).ID;
                if (followerId == targetId) return Json(new { success = false, message = "Bạn không thể tự theo dõi chính mình!" });

                var existingFollow = db.FOLLOW.FirstOrDefault(f => f.FollowerID == followerId && f.FollowedID == targetId);

                if (existingFollow != null)
                {
                    db.FOLLOW.Remove(existingFollow);
                    db.SaveChanges();
                    return Json(new { success = true, isFollowing = false });
                }
                else
                {
                    var newFollow = new FOLLOW { FollowerID = followerId, FollowedID = targetId, CreateDate = DateTime.Now };
                    db.FOLLOW.Add(newFollow);

                    var followerName = ((CookingShare.Models.ACCOUNT)Session["Account"]).UserName;

                    string linkUrl = Url.Action("PublicProfile", "User", new { id = followerId });

                    var newNoti = new NOTIFICATION { AccountID = targetId, Content = $"Người dùng {followerName} đã bắt đầu theo dõi bạn.", LinkURL = linkUrl, IsRead = false, CreateDate = DateTime.Now };
                    db.NOTIFICATION.Add(newNoti);

                    db.SaveChanges();
                    return Json(new { success = true, isFollowing = true });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        [HttpPost]
        public ActionResult MarkNotificationsRead()
        {
            if (Session["Account"] == null) return Json(new { success = false });
            int userId = ((CookingShare.Models.ACCOUNT)Session["Account"]).ID;
            var unreadNotis = db.NOTIFICATION.Where(n => n.AccountID == userId && n.IsRead == false).ToList();
            if (unreadNotis.Count > 0)
            {
                foreach (var noti in unreadNotis) noti.IsRead = true;
                db.SaveChanges();
            }
            return Json(new { success = true });
        }

        [HttpPost]
        public ActionResult DeleteReadNotifications()
        {
            if (Session["Account"] == null) return Json(new { success = false });
            int userId = ((CookingShare.Models.ACCOUNT)Session["Account"]).ID;
            var readNotis = db.NOTIFICATION.Where(n => n.AccountID == userId && n.IsRead == true).ToList();
            if (readNotis.Count > 0)
            {
                db.NOTIFICATION.RemoveRange(readNotis);
                db.SaveChanges();
            }
            return Json(new { success = true });
        }

        [HttpGet]
        public ActionResult ReadNotification(int id)
        {
            var noti = db.NOTIFICATION.Find(id);
            if (noti != null)
            {
                noti.IsRead = true;
                db.SaveChanges();
                if (!string.IsNullOrEmpty(noti.LinkURL)) return Redirect(noti.LinkURL);
            }
            return RedirectToAction("MyProfile");
        }

        [ChildActionOnly]
        public ActionResult GetUnreadNotiBadge()
        {
            if (Session["Account"] != null)
            {
                int userId = ((CookingShare.Models.ACCOUNT)Session["Account"]).ID;
                int count = db.NOTIFICATION.Count(n => n.AccountID == userId && n.IsRead == false);
                if (count > 0) return Content($"<span class=\"position-absolute top-0 start-100 translate-middle badge rounded-pill bg-danger\" style=\"font-size: 0.6rem;\">{count}</span>");
            }
            return Content("");
        }

        [HttpPost]
        public ActionResult RemoveFavorite(int recipeId)
        {
            if (Session["Account"] == null) return Json(new { success = false, message = "Vui lòng đăng nhập!" });
            int userId = ((CookingShare.Models.ACCOUNT)Session["Account"]).ID;
            var fav = db.FAVORITE.FirstOrDefault(f => f.AccountID == userId && f.RecipeID == recipeId);
            if (fav != null)
            {
                db.FAVORITE.Remove(fav);
                db.SaveChanges();
                return Json(new { success = true });
            }
            return Json(new { success = false });
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