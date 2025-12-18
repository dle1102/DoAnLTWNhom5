using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class GioHangController : Controller
    {
        ShopGearEntities1 db = new ShopGearEntities1();

        // ===================== LẤY GIỎ HÀNG =====================
        public List<GioHang> LayGioHang()
        {
            var lst = Session["GioHang"] as List<GioHang>;
            if (lst == null)
            {
                lst = new List<GioHang>();
                Session["GioHang"] = lst;
            }
            return lst;
        }

        // ===================== THÊM GIỎ HÀNG =====================
        public ActionResult ThemGioHang(int iMaSP, string strURL)
        {
            var sp = db.SanPhams.FirstOrDefault(n => n.MaSP == iMaSP);
            if (sp == null) return HttpNotFound();

            var gioHang = LayGioHang();
            var item = gioHang.FirstOrDefault(n => n.iMaSP == iMaSP);

            if (item == null)
                gioHang.Add(new GioHang(iMaSP));
            else
                item.iSoLuong++;

            return Redirect(strURL);
        }

        // ===================== CẬP NHẬT =====================
        public ActionResult CapNhatGioHang(int iMaSP, FormCollection f)
        {
            var gioHang = LayGioHang();
            var item = gioHang.FirstOrDefault(n => n.iMaSP == iMaSP);

            if (item != null)
            {
                int soLuong;
                if (int.TryParse(f["txtSoLuong"], out soLuong))
                    item.iSoLuong = soLuong;
            }
            return RedirectToAction("GioHang");
        }

        // ===================== XÓA =====================
        public ActionResult XoaGioHang(int iMaSP)
        {
            var gioHang = LayGioHang();
            var item = gioHang.FirstOrDefault(n => n.iMaSP == iMaSP);
            if (item != null) gioHang.Remove(item);

            return gioHang.Count == 0
                ? RedirectToAction("Index", "Banhang")
                : RedirectToAction("GioHang");
        }

        // ===================== XEM GIỎ =====================
        public ActionResult GioHang()
        {
            ViewBag.TongSoLuong = LayGioHang().Sum(n => n.iSoLuong);
            ViewBag.TongTien = LayGioHang().Sum(n => n.dThanhTien);
            return View(LayGioHang());
        }

        // ===================== FORM ĐẶT HÀNG (GET) =====================
        [HttpGet]
        public ActionResult DatHang()
        {
            // 1. Kiểm tra đăng nhập
            if (Session["MaTK"] == null || Session["MaTK"].ToString() == "")
            {
                return RedirectToAction("Index", "DangNhap", new { returnUrl = Url.Action("DatHang", "GioHang") });
            }

            // 2. Kiểm tra giỏ hàng
            if (Session["GioHang"] == null)
            {
                return RedirectToAction("Index", "Banhang");
            }

            var gioHang = LayGioHang();
            if (gioHang.Count == 0)
            {
                return RedirectToAction("Index", "Banhang");
            }

            // 3. TÍNH TOÁN TIỀN
            ViewBag.TongSoLuong = gioHang.Sum(n => n.iSoLuong);
            ViewBag.TongTien = gioHang.Sum(n => n.dThanhTien);

            // 4. LẤY THÔNG TIN USER ĐỂ ĐIỀN SẴN (SĐT)
            int maTK = int.Parse(Session["MaTK"].ToString());
            var user = db.TaiKhoans.Find(maTK);

            // Truyền thông tin user qua ViewBag để View sử dụng
            ViewBag.User = user;

            return View(gioHang);
        }

        // ===================== LƯU ĐƠN HÀNG (POST) =====================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DatHang(FormCollection f)
        {
            // Kiểm tra đăng nhập
            if (Session["MaTK"] == null || Session["MaTK"].ToString() == "")
            {
                return RedirectToAction("Index", "DangNhap");
            }

            var gioHang = LayGioHang();
            if (gioHang.Count == 0)
            {
                return RedirectToAction("Index", "Banhang");
            }

            // Tạo đơn hàng mới
            DonHang dh = new DonHang();
            dh.MaTK = int.Parse(Session["MaTK"].ToString());
            dh.NgayDat = DateTime.Now;
            dh.TrangThai = "cho_xac_nhan";
            dh.TongTien = gioHang.Sum(n => (decimal)n.dThanhTien);

            // --- XỬ LÝ ĐỊA CHỈ ---
            // Lấy dữ liệu từ 3 ô nhập liệu riêng biệt
            string tinhThanh = f["TinhThanh"];   // Tên Tỉnh/Thành
            string quanHuyen = f["QuanHuyen"];   // Tên Quận/Huyện
            string soNha = f["DiaChiCuThe"];     // Số nhà, đường, phường xã

            // Gộp lại thành địa chỉ đầy đủ: "Số 10, Đường A, Xã B, Huyện C, Tỉnh D"
            dh.DiaChiGiao = string.Format("{0}, {1}, {2}", soNha, quanHuyen, tinhThanh);

            // --- XỬ LÝ GHI CHÚ ---
            // Vì bảng DonHang không có cột SĐT người nhận riêng, 
            // ta lưu SĐT vào Ghi chú luôn để Shipper biết đường gọi.
            dh.NguoiNhan = f["NguoiNhan"]?.Trim() ?? "Khách hàng"; // nếu form có ô tên người nhận
            dh.DienThoaiGiao = f["SoDienThoai"]?.Trim(); // ← Lưu đúng vào cột riêng

            // === GHI CHÚ THẬT CỦA KHÁCH ===
            dh.GhiChu = f["GhiChu"]?.Trim(); // chỉ lưu ghi chú, không gộp SĐT nữa

            // Nếu muốn lưu tên người nhận từ tài khoản (nếu không nhập mới)
            if (string.IsNullOrEmpty(dh.NguoiNhan) || dh.NguoiNhan == "Khách hàng")
            {
                int maTK = int.Parse(Session["MaTK"].ToString());
                var user = db.TaiKhoans.Find(maTK);
                if (user != null && !string.IsNullOrEmpty(user.HoTen))
                {
                    dh.NguoiNhan = user.HoTen;
                }
            }

            db.DonHangs.Add(dh);
            db.SaveChanges();

            // Lưu chi tiết đơn hàng
            foreach (var item in gioHang)
            {
                ChiTietDonHang ct = new ChiTietDonHang
                {
                    MaDH = dh.MaDH,
                    MaSP = item.iMaSP,
                    SoLuong = item.iSoLuong,
                    DonGia = (decimal)item.dDonGia
                };
                db.ChiTietDonHangs.Add(ct);
            }
            db.SaveChanges();

            // Xóa giỏ hàng
            Session["GioHang"] = null;

            return RedirectToAction("XacNhanDonHang");
        }

        public ActionResult XacNhanDonHang()
        {
            return View();
        }
    }
}
