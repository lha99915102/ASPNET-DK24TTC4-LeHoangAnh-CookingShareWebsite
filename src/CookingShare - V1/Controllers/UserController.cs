using CookingShare.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CookingShare.Controllers
{
    public class UserController : Controller
    {
        CookingShareDBEntities db = new CookingShareDBEntities();

        // Hàm hiển thị trang cá nhân công khai (Ai cũng xem được)
        public ActionResult PublicProfile(int? id)
        {
            if (id == null)
            {
                // Nếu không truyền ID thì bay về trang chủ
                return RedirectToAction("Index", "Home");
            }

            // Lấy thông tin user dựa vào ID
            var user = db.ACCOUNT.FirstOrDefault(u => u.ID == id && u.Status == 1);

            if (user == null)
            {
                // Trả về trang lỗi 404 nếu không tìm thấy người dùng
                return HttpNotFound("Không tìm thấy đầu bếp này!");
            }

            // Đếm số lượng công thức người này đã đăng (đã duyệt)
            ViewBag.RecipeCount = user.RECIPE.Count(r => r.Status == 1);

            ViewBag.FollowerCount = db.FOLLOW.Count(f => f.FollowedID == id); // Số người theo dõi user này
            ViewBag.FollowingCount = db.FOLLOW.Count(f => f.FollowerID == id); // Số người user này đang theo dõi

            ViewBag.IsFollowing = false;
            ViewBag.IsSelf = false;

            if (Session["Account"] != null)
            {
                int loggedInUserId = ((CookingShare.Models.ACCOUNT)Session["Account"]).ID;

                // Kiểm tra xem có phải đang tự xem trang của mình không
                if (loggedInUserId == id)
                {
                    ViewBag.IsSelf = true;
                }
                else
                {
                    // Nếu xem trang người khác, kiểm tra xem đã theo dõi chưa
                    var checkFollow = db.FOLLOW.FirstOrDefault(f => f.FollowerID == loggedInUserId && f.FollowedID == id);
                    if (checkFollow != null)
                    {
                        ViewBag.IsFollowing = true;
                    }
                }
            }

            var cooksnaps = db.COOKSNAP
                               .Where(c => c.AccountID == id && c.ImageName != null && c.ImageName != "")
                               .OrderByDescending(c => c.CreateDate)
                               .ToList();

            ViewBag.Cooksnaps = cooksnaps;
            ViewBag.CooksnapCount = cooksnaps.Count;

            return View(user); // Truyền nguyên Model ACCOUNT sang View
        }

        // Hàm hiển thị trang Cài đặt cá nhân (Yêu cầu đăng nhập)
        [HttpGet]
        public ActionResult MyProfile()
        {
            if (Session["Account"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var sessionUser = (CookingShare.Models.ACCOUNT)Session["Account"];
            var currentUser = db.ACCOUNT.FirstOrDefault(u => u.ID == sessionUser.ID);

            // 1. Thống kê
            ViewBag.FollowerCount = db.FOLLOW.Count(f => f.FollowedID == currentUser.ID);

            // 2. Lấy danh sách Công thức đã lưu (Favorites)
            ViewBag.Favorites = db.FAVORITE.Where(f => f.AccountID == currentUser.ID).OrderByDescending(f => f.CreateDate).ToList();

            // 3. Lấy thông báo (Chưa đọc đưa lên đầu)
            ViewBag.Notifications = db.NOTIFICATION.Where(n => n.AccountID == currentUser.ID).OrderBy(n => n.IsRead).ThenByDescending(n => n.CreateDate).ToList();
            ViewBag.UnreadNotiCount = db.NOTIFICATION.Count(n => n.AccountID == currentUser.ID && n.IsRead == false);

            // 4. Lấy danh sách tất cả Nguyên Liệu để hiển thị Dị ứng
            ViewBag.AllIngredients = db.INGREDIENT.ToList();

            // LẤY DỊ ỨNG BẰNG CÁCH GỌI THẲNG TỪ USER
            ViewBag.UserAllergies = currentUser.INGREDIENT.Select(i => i.ID).ToList();

            return View(currentUser);
        }

        // Hàm xử lý Lưu thay đổi Cập nhật thông tin (ĐÃ GOM ĐẦY ĐỦ CÁC TRƯỜNG MỚI VÀ DỊ ỨNG)
        [HttpPost]
        public ActionResult UpdateProfile(FormCollection form, HttpPostedFileBase AvatarFile, HttpPostedFileBase KitchenFile, List<int> AllergyIDs)
        {
            if (Session["Account"] == null) return RedirectToAction("Login", "Account");

            var sessionUser = (CookingShare.Models.ACCOUNT)Session["Account"];
            var userInDb = db.ACCOUNT.FirstOrDefault(u => u.ID == sessionUser.ID);
            var profile = db.PROFILE.FirstOrDefault(p => p.AccountID == sessionUser.ID);

            if (userInDb != null && profile != null)
            {
                // 1. CẬP NHẬT ACCOUNT (Số điện thoại)
                userInDb.Phone = form["Phone"];

                // 2. CẬP NHẬT PROFILE CƠ BẢN VÀ CHUYÊN SÂU
                profile.FullName = form["FullName"];
                profile.Bio = form["Bio"];
                profile.Gender = form["Gender"];
                profile.Lifestyle = form["Lifestyle"];
                profile.JobTitle = form["JobTitle"];
                profile.Education = form["Education"];
                profile.CookingPhilosophy = form["CookingPhilosophy"];
                profile.SocialLink = form["SocialLink"];

                // XỬ LÝ AN TOÀN CHO CÁC TRƯỜNG KIỂU SỐ VÀ NGÀY THÁNG 

                // Ngày sinh
                DateTime parsedDate;
                if (DateTime.TryParse(form["BirthDay"], out parsedDate))
                {
                    profile.BirthDay = parsedDate;
                }

                // Chiều cao
                double parsedHeight;
                if (double.TryParse(form["Height"], out parsedHeight))
                {
                    profile.Height = parsedHeight;
                }

                // Cân nặng
                double parsedWeight;
                if (double.TryParse(form["Weight"], out parsedWeight))
                {
                    profile.Weight = parsedWeight;
                }

                // Calo hằng ngày
                int parsedCalo;
                if (int.TryParse(form["CaloDaily"], out parsedCalo))
                {
                    profile.CaloDaily = parsedCalo;
                }

                // 3. XỬ LÝ UPLOAD ẢNH AVATAR
                if (AvatarFile != null && AvatarFile.ContentLength > 0)
                {
                    string extension = System.IO.Path.GetExtension(AvatarFile.FileName);
                    string fileName = "avatar_" + sessionUser.ID.ToString() + "_" + System.DateTime.Now.ToString("yyyyMMddHHmmss") + extension;
                    string path = Server.MapPath("~/assets/images/avatars/");
                    if (!System.IO.Directory.Exists(path)) System.IO.Directory.CreateDirectory(path);
                    AvatarFile.SaveAs(path + fileName);
                    profile.Avatar = fileName;
                }

                // XỬ LÝ UPLOAD ẢNH BẾP (KITCHEN IMAGE)
                if (KitchenFile != null && KitchenFile.ContentLength > 0)
                {
                    string extension = System.IO.Path.GetExtension(KitchenFile.FileName);
                    string fileName = "kitchen_" + sessionUser.ID.ToString() + "_" + System.DateTime.Now.ToString("yyyyMMddHHmmss") + extension;
                    string path = Server.MapPath("~/assets/images/kitchens/");
                    if (!System.IO.Directory.Exists(path)) System.IO.Directory.CreateDirectory(path);
                    KitchenFile.SaveAs(path + fileName);
                    profile.KitchenImage = fileName;
                }

                // 4. XỬ LÝ DỊ ỨNG (Quan hệ Nhiều - Nhiều)
                userInDb.INGREDIENT.Clear(); // Xóa cũ
                if (AllergyIDs != null && AllergyIDs.Count > 0) // Thêm mới
                {
                    foreach (var ingId in AllergyIDs)
                    {
                        var ingredient = db.INGREDIENT.Find(ingId);
                        if (ingredient != null)
                        {
                            userInDb.INGREDIENT.Add(ingredient);
                        }
                    }
                }

                // 5. XỬ LÝ ĐỔI MẬT KHẨU
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
                    string hashedNewPass = CookingShare.Models.SecurityHelper.HashPasswordSHA256(NewPassword);
                    userInDb.Password = hashedNewPass;
                }

                db.SaveChanges(); // Lưu tất cả vào Database

                Session["Account"] = userInDb; // Reset Session để cập nhật thông tin mới
                TempData["SuccessMessage"] = "Cập nhật thông tin thành công!";
            }

            return RedirectToAction("MyProfile");
        }

        // Hàm AJAX: Xử lý Báo cáo (Report) từ người dùng
        [HttpPost]
        public ActionResult SubmitReport(int targetId, string reason, int type)
        {
            try
            {
                if (Session["Account"] == null)
                {
                    return Json(new { success = false, message = "Vui lòng đăng nhập!" });
                }

                int reporterId = ((CookingShare.Models.ACCOUNT)Session["Account"]).ID;

                // Tránh việc người dùng tự báo cáo chính mình
                if (reporterId == targetId && type == 2)
                {
                    return Json(new { success = false, message = "Bạn không thể tự báo cáo chính mình!" });
                }

                // Tạo mới một record Báo cáo
                var newReport = new REPORT();
                newReport.ReporterID = reporterId;
                newReport.TargetID = targetId;
                newReport.ReportType = type; // 2 = Báo cáo người dùng
                newReport.Reason = reason;
                newReport.CreateDate = DateTime.Now;
                newReport.Status = 0; // 0 = Chờ Admin duyệt

                db.REPORT.Add(newReport);
                db.SaveChanges();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        // Hàm AJAX: Xử lý Theo dõi / Bỏ theo dõi người dùng
        [HttpPost]
        public ActionResult ToggleFollow(int targetId)
        {
            try
            {
                if (Session["Account"] == null)
                {
                    return Json(new { success = false, message = "Vui lòng đăng nhập!" });
                }

                int followerId = ((CookingShare.Models.ACCOUNT)Session["Account"]).ID;

                // Cấm tự theo dõi chính mình
                if (followerId == targetId)
                {
                    return Json(new { success = false, message = "Bạn không thể tự theo dõi chính mình!" });
                }

                // Kiểm tra xem đã theo dõi chưa
                var existingFollow = db.FOLLOW.FirstOrDefault(f => f.FollowerID == followerId && f.FollowedID == targetId);

                if (existingFollow != null)
                {
                    // NẾU ĐÃ THEO DÕI -> BỎ THEO DÕI (XÓA KHỎI DB)
                    db.FOLLOW.Remove(existingFollow);
                    db.SaveChanges();

                    return Json(new { success = true, isFollowing = false });
                }
                else
                {
                    // NẾU CHƯA THEO DÕI -> THÊM MỚI VÀO DB
                    var newFollow = new FOLLOW();
                    newFollow.FollowerID = followerId;
                    newFollow.FollowedID = targetId;
                    newFollow.CreateDate = DateTime.Now;

                    db.FOLLOW.Add(newFollow);

                    // TẠO THÔNG BÁO CHO NGƯỜI ĐƯỢC FOLLOW
                    var followerName = ((CookingShare.Models.ACCOUNT)Session["Account"]).UserName;
                    var newNoti = new NOTIFICATION();
                    newNoti.AccountID = targetId; // Người nhận thông báo là người được theo dõi
                    newNoti.Content = $"Người dùng {followerName} đã bắt đầu theo dõi bạn.";
                    newNoti.LinkURL = $"/User/PublicProfile/{followerId}"; // Link dẫn về trang của người vừa follow
                    newNoti.IsRead = false;
                    newNoti.CreateDate = DateTime.Now;

                    db.NOTIFICATION.Add(newNoti);
                    // ==========================================

                    db.SaveChanges();

                    return Json(new { success = true, isFollowing = true });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        // Hàm AJAX: Đánh dấu tất cả thông báo của người dùng là Đã đọc
        [HttpPost]
        public ActionResult MarkNotificationsRead()
        {
            if (Session["Account"] == null) return Json(new { success = false });

            int userId = ((CookingShare.Models.ACCOUNT)Session["Account"]).ID;

            // Tìm các thông báo chưa đọc của user này
            var unreadNotis = db.NOTIFICATION.Where(n => n.AccountID == userId && n.IsRead == false).ToList();

            if (unreadNotis.Count > 0)
            {
                foreach (var noti in unreadNotis)
                {
                    noti.IsRead = true; // Đổi trạng thái
                }
                db.SaveChanges(); // Lưu vào DB
            }

            return Json(new { success = true });
        }

        // Hàm AJAX: Xóa tất cả thông báo ĐÃ ĐỌC của người dùng
        [HttpPost]
        public ActionResult DeleteReadNotifications()
        {
            if (Session["Account"] == null) return Json(new { success = false });

            int userId = ((CookingShare.Models.ACCOUNT)Session["Account"]).ID;

            // Tìm tất cả thông báo ĐÃ ĐỌC (IsRead == true) của user này
            var readNotis = db.NOTIFICATION.Where(n => n.AccountID == userId && n.IsRead == true).ToList();

            if (readNotis.Count > 0)
            {
                db.NOTIFICATION.RemoveRange(readNotis); // Xóa sạch khỏi DB
                db.SaveChanges();
            }

            return Json(new { success = true });
        }

        // Hàm xử lý khi người dùng CLICK vào 1 thông báo cụ thể
        [HttpGet]
        public ActionResult ReadNotification(int id)
        {
            var noti = db.NOTIFICATION.Find(id);
            if (noti != null)
            {
                // 1. Đánh dấu thông báo này là Đã đọc
                noti.IsRead = true;
                db.SaveChanges();

                // 2. Kiểm tra xem thông báo này có Link để nhảy đi không?
                if (!string.IsNullOrEmpty(noti.LinkURL))
                {
                    return Redirect(noti.LinkURL); // Nhảy sang Public Profile hoặc Chi tiết món
                }
            }

            // Nếu thông báo KHÔNG có link (Ví dụ: Thông báo xử phạt), 
            // hoặc không tìm thấy thông báo, thì quay lại trang Quản lý tài khoản
            return RedirectToAction("MyProfile");
        }

        // Hàm này gọi trực tiếp từ Layout để lấy số thông báo chưa đọc
        [ChildActionOnly]
        public ActionResult GetUnreadNotiBadge()
        {
            if (Session["Account"] != null)
            {
                int userId = ((CookingShare.Models.ACCOUNT)Session["Account"]).ID;
                int count = db.NOTIFICATION.Count(n => n.AccountID == userId && n.IsRead == false);

                if (count > 0)
                {
                    // Trả về HTML chứa cái chấm đỏ
                    return Content($"<span class=\"position-absolute top-0 start-100 translate-middle badge rounded-pill bg-danger\" style=\"font-size: 0.6rem;\">{count}</span>");
                }
            }
            return Content(""); // Không có thông báo thì không hiện gì
        }

        // Hàm AJAX: Xóa công thức khỏi danh sách yêu thích
        [HttpPost]
        public ActionResult RemoveFavorite(int recipeId)
        {
            if (Session["Account"] == null) return Json(new { success = false, message = "Vui lòng đăng nhập!" });

            int userId = ((CookingShare.Models.ACCOUNT)Session["Account"]).ID;

            // Tìm record yêu thích trong DB
            var fav = db.FAVORITE.FirstOrDefault(f => f.AccountID == userId && f.RecipeID == recipeId);

            if (fav != null)
            {
                db.FAVORITE.Remove(fav);
                db.SaveChanges();
                return Json(new { success = true });
            }

            return Json(new { success = false });
        }

    }
}