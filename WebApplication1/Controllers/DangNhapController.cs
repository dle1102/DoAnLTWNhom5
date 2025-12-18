using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;

namespace WebApplication1.Controllers
{
    public class DangNhapController : Controller
    {
        ShopGearEntities1 ql = new ShopGearEntities1();

        // GET: DangNhap
        public ActionResult Index(string returnUrl)
        {
            ViewBag.url = Url.IsLocalUrl(returnUrl)
                ? returnUrl
                : Url.Action("Index", "Banhang");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult XuLyFormDN(FormCollection f, string duongdan)
        {
            string email = f["email"];
            string pass = f["password"];

            var tk = ql.TaiKhoans
                       .FirstOrDefault(t => t.Email == email && t.MatKhau == pass);

            if (tk == null)
            {
                ViewBag.Error = "Email hoặc mật khẩu không đúng!";
                ViewBag.url = duongdan;
                return View("Index");
            }

            // ✅ Đăng nhập thành công
            FormsAuthentication.SetAuthCookie(tk.Email, false);
            Session["MaTK"] = tk.MaTK;
            Session["HoTen"] = tk.HoTen;
            Session["VaiTro"] = tk.VaiTro;

            // 🔥 ĐÁNH DẤU ĐÃ VÀO ADMIN
            if (tk.VaiTro == "admin" || tk.VaiTro == "nhanvien")
            {
                Session["DaVaoAdmin"] = true;
                return RedirectToAction("Index", "AdminPage");
            }

            // Khách hàng
            Session["DaVaoAdmin"] = null;

            if (!string.IsNullOrEmpty(duongdan) && Url.IsLocalUrl(duongdan))
                return Redirect(duongdan);

            return RedirectToAction("Index", "Banhang");
        }

        // 🔥 ACTION QUAY LẠI TRANG QUẢN TRỊ
        public ActionResult VeTrangQuanTri()
        {
            if (Session["VaiTro"] == null)
                return RedirectToAction("Index", "DangNhap");

            string role = Session["VaiTro"].ToString();
            if (role == "admin" || role == "nhanvien")
                return RedirectToAction("Index", "AdminPage");

            return RedirectToAction("Index", "Banhang");
        }

        public ActionResult Logout()
        {
            Session.Clear();
            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Banhang");
        }

        // ==========================================================
        // QUẢN LÝ TÀI KHOẢN CÁ NHÂN
        // ==========================================================

        // 1. Xem thông tin tài khoản
        public ActionResult Info()
        {
            if (Session["MaTK"] == null)
            {
                return RedirectToAction("Index");
            }

            int maTK = int.Parse(Session["MaTK"].ToString());
            var user = ql.TaiKhoans.Find(maTK);

            if (user == null) return HttpNotFound();

            return View(user);
        }

        // 2. Cập nhật thông tin (Họ tên, SĐT, Địa chỉ, Email)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CapNhatThongTin(FormCollection f)
        {
            if (Session["MaTK"] == null) return RedirectToAction("Index");

            int maTK = int.Parse(Session["MaTK"].ToString());
            var user = ql.TaiKhoans.Find(maTK);

            if (user != null)
            {
                user.HoTen = f["HoTen"];
                user.SoDienThoai = f["SoDienThoai"];
                user.DiaChi = f["DiaChi"];
                user.Email = f["Email"]; // Cho phép sửa email nếu muốn

                ql.SaveChanges();

                // Cập nhật lại Session tên hiển thị
                Session["HoTen"] = user.HoTen;

                TempData["Success_Info"] = "Cập nhật thông tin thành công!";
            }
            return RedirectToAction("Info");
        }

        // 3. Đổi mật khẩu
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DoiMatKhau(string MatKhauCu, string MatKhauMoi, string XacNhanMatKhau)
        {
            if (Session["MaTK"] == null) return RedirectToAction("Index");

            int maTK = int.Parse(Session["MaTK"].ToString());
            var user = ql.TaiKhoans.Find(maTK);

            if (user != null)
            {
                // Kiểm tra mật khẩu cũ (Lưu ý: nên mã hóa MD5 nếu hệ thống có dùng, ở đây tôi so sánh thường theo code cũ của bạn)
                if (user.MatKhau != MatKhauCu)
                {
                    TempData["Error_Pass"] = "Mật khẩu cũ không chính xác!";
                }
                else if (MatKhauMoi != XacNhanMatKhau)
                {
                    TempData["Error_Pass"] = "Xác nhận mật khẩu không khớp!";
                }
                else if (MatKhauMoi.Length < 6)
                {
                    TempData["Error_Pass"] = "Mật khẩu mới phải từ 6 ký tự trở lên!";
                }
                else
                {
                    user.MatKhau = MatKhauMoi;
                    ql.SaveChanges();
                    TempData["Success_Pass"] = "Đổi mật khẩu thành công!";
                }
            }
            return RedirectToAction("Info");
        }
    }
}
