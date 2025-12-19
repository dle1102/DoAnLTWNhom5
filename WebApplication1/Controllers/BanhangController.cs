using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebApplication1.Controllers
{
    public class BanhangController : Controller
    {
        ShopGearEntities1 BH = new ShopGearEntities1();
        //
        // GET: /Banhang/
        public ActionResult Index(int? maLoai, decimal? minPrice, decimal? maxPrice, string keyword)
        {
            ViewBag.LoaiSanPham = BH.LoaiSanPhams.OrderBy(l => l.TenLoai).ToList();

           
            ViewBag.MaLoai = maLoai;
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;
            ViewBag.Keyword = keyword;


            return View();
        }



        public ActionResult ChiTietSP(int? masp)
        {
            if (!masp.HasValue)
                return RedirectToAction("Index");

            // Không cần truy vấn DB nữa, tất cả do API xử lý
            return View();
        }
        // ===================== LỊCH SỬ ĐƠN HÀNG =====================
        public ActionResult LichSuDonHang()
        {
            // 1. Kiểm tra đăng nhập
            if (Session["MaTK"] == null)
            {
                return RedirectToAction("Index", "DangNhap");
            }

            // 2. Lấy danh sách đơn hàng của user đó
            int maTK = (int)Session["MaTK"];
            var dsDonHang = BH.DonHangs
                              .Where(n => n.MaTK == maTK)
                              .OrderByDescending(n => n.NgayDat) // Đơn mới nhất lên đầu
                              .ToList();

            return View(dsDonHang);
        }

        // ===================== CHI TIẾT LỊCH SỬ =====================
        public ActionResult ChiTietLichSu(int id)
        {
            if (Session["MaTK"] == null)
                return RedirectToAction("Index", "DangNhap");

            // Lấy đơn hàng theo ID
            var dh = BH.DonHangs.FirstOrDefault(n => n.MaDH == id);

            // Kiểm tra bảo mật: Nếu đơn không tồn tại hoặc không phải của User này -> Chặn
            int maTK = (int)Session["MaTK"];
            if (dh == null || dh.MaTK != maTK)
            {
                return HttpNotFound();
            }

            return View(dh);
        }
        public ActionResult Search(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return RedirectToAction("Index");
            }

            // ✅ Redirect về Index với keyword parameter (sẽ được xử lý bởi API)
            return RedirectToAction("Index", new { keyword = keyword });
        }

    }

}
