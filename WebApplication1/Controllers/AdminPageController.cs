using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class AdminPageController : Controller
    {
        // GET: AdminPage
        ShopGearEntities1 ql = new ShopGearEntities1();
        
        
        public ActionResult Index(string period = "30")
        {
            int days;
            string displayText;

            if (period == "today" || period == "1") // Hôm nay
            {
                days = 1;
                displayText = "Hôm nay";
            }
            else if (period == "7")
            {
                days = 7;
                displayText = "7 ngày gần nhất";
            }
            else // mặc định 30 ngày
            {
                days = 30;
                displayText = "30 ngày gần nhất";
            }

            // Khoảng thời gian hiện tại
            DateTime tuNgay = DateTime.Today.AddDays(-days + 1);
            DateTime denNgay = DateTime.Today.AddDays(1); // đến hết ngày hôm nay

            // === Phần còn lại giữ nguyên như cũ ===
            var dhHienTai = ql.DonHangs
                .Where(d => d.NgayDat >= tuNgay && d.NgayDat < denNgay && d.TrangThai == "hoan_thanh")
                .ToList();

            decimal doanhThuHT = dhHienTai.Sum(d => (decimal?)d.TongTien ?? 0);
            int donHangHT = dhHienTai.Count;

            // Kỳ trước để so sánh
            DateTime tuNgayTruoc = tuNgay.AddDays(-days);
            DateTime denNgayTruoc = denNgay.AddDays(-days);

            var dhTruoc = ql.DonHangs
                .Where(d => d.NgayDat >= tuNgayTruoc && d.NgayDat < denNgayTruoc && d.TrangThai == "hoan_thanh")
                .ToList();

            decimal doanhThuTruoc = dhTruoc.Sum(d => (decimal?)d.TongTien ?? 0);
            int donHangTruoc = dhTruoc.Count;

            // Tính % thay đổi (giữ nguyên logic cũ của bạn)
            double ptDoanhThu = doanhThuTruoc > 0
                ? Math.Round((double)((doanhThuHT - doanhThuTruoc) / doanhThuTruoc) * 100, 1)
                : doanhThuHT > 0 ? 100.0 : 0.0;

            double ptDonHang = donHangTruoc > 0
                ? Math.Round((double)((donHangHT - donHangTruoc) / donHangTruoc) * 100, 1)
                : donHangHT > 0 ? 100.0 : 0.0;

            decimal giaTriTB = donHangHT > 0 ? doanhThuHT / donHangHT : 0;
            decimal giaTriTBTruoc = donHangTruoc > 0 ? doanhThuTruoc / donHangTruoc : 0;
            double ptGiaTriTB = giaTriTBTruoc > 0
                ? Math.Round((double)((giaTriTB - giaTriTBTruoc) / giaTriTBTruoc) * 100, 1)
                : giaTriTB > 0 ? 100.0 : 0.0;

            // Khách hàng mới
            var khachHangMoi = ql.DonHangs
                .Where(d => d.NgayDat >= tuNgay && d.NgayDat < denNgay)
                .GroupBy(d => d.MaTK)
                .Count(g => g.Min(x => x.NgayDat) >= tuNgay && g.Any(x => x.TrangThai == "hoan_thanh"));

            // Gán ViewBag
            ViewBag.TongDoanhThu = doanhThuHT;
            ViewBag.TongDonHang = donHangHT;
            ViewBag.KhachHangMoi = khachHangMoi;
            ViewBag.GiaTriTrungBinh = giaTriTB;
            ViewBag.PT_DoanhThu = ptDoanhThu;
            ViewBag.PT_DonHang = ptDonHang;
            ViewBag.PT_GiaTriTB = ptGiaTriTB;
            ViewBag.KhoangThoiGian = displayText;
            ViewBag.CurrentPeriod = period; // để highlight mục đang chọn (tùy chọn)
            ViewBag.TopSanPhamBanChay = GetTop5SanPhamBanChay();
            ViewBag.SanPhamSapHetHang = GetSanPhamSapHetHang(10);

            return View();
        }
        private List<SanPham> GetSanPhamSapHetHang(int nguong = 10)
        {
            var sanPhamsSapHet = ql.SanPhams
                .Where(sp => sp.SoLuong != null && sp.SoLuong <= nguong && sp.SoLuong > 0)
                .OrderBy(sp => sp.SoLuong)
                .Take(8)
                .ToList();

            return sanPhamsSapHet;
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ql?.Dispose();
            }
            base.Dispose(disposing);
        }
        public ActionResult SanPham(int? page = 1, string search = "")
        {
            int pageSize = 10;
            var ds = ql.SanPhams.AsQueryable();

            if (!string.IsNullOrEmpty(search))
                ds = ds.Where(s => s.TenSP.Contains(search));

            var total = ds.Count();
            var sanPhams = ds.OrderBy(s => s.MaSP)
                             .Skip((page.Value - 1) * pageSize)
                             .Take(pageSize)
                             .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(total / (double)pageSize);
            ViewBag.Search = search;

            return View("SanPham", sanPhams); 
        }
        public ActionResult DonHang(string status = "", string dateRange = "", int page = 1)
        {
            int pageSize = 10;
            var ds = ql.DonHangs.AsQueryable();

          
            if (!string.IsNullOrEmpty(status) && status != "All")
            {
                string tt = status; 

                switch (status)
                {
                    case "Pending":
                        tt = "cho_xac_nhan";
                        break;
                    case "Shipped":
                        tt = "dang_giao";
                        break;
                    case "Delivered":
                        tt = "hoan_thanh";
                        break;
                    case "Cancelled":
                        tt = "da_huy";
                        break;
                      
                }

                ds = ds.Where(d => d.TrangThai == tt);
            }

           
            if (!string.IsNullOrEmpty(dateRange))
            {
                DateTime start = DateTime.MinValue;

                if (dateRange == "7")
                    start = DateTime.Today.AddDays(-7);
                else if (dateRange == "30")
                    start = DateTime.Today.AddDays(-30);
                else if (dateRange == "90")
                    start = DateTime.Today.AddDays(-90);
               

                if (start != DateTime.MinValue)
                {
                    ds = ds.Where(d => d.NgayDat >= start);
                }
            }

            int total = ds.Count();
            var list = ds.OrderByDescending(d => d.MaDH)
                         .Skip((page - 1) * pageSize)
                         .Take(pageSize)
                         .ToList();

            ViewBag.Status = status;
            ViewBag.DateRange = dateRange;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(total / (double)pageSize);
            ViewBag.TotalResults = total;

            return View(list);
        }

      
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CapNhatTrangThai(int MaDH, string TrangThai, string returnUrl = null)
        {
            var dh = ql.DonHangs.Find(MaDH);
            if (dh == null)
            {
                TempData["Error"] = "Không tìm thấy đơn hàng.";
                if (!string.IsNullOrEmpty(returnUrl)) return Redirect(returnUrl);
                return RedirectToAction("DonHang");
            }

            dh.TrangThai = TrangThai;

            if (TrangThai == "hoan_thanh")
            {
                dh.NgayGiao = DateTime.Now;
                dh.LyDoHuy = null;
            }
            else if (TrangThai == "da_huy")
            {
                dh.LyDoHuy = "Hủy bởi admin";
                dh.NgayGiao = null;
            }
            else
            {
                dh.NgayGiao = null;
                dh.LyDoHuy = null;
            }

            ql.SaveChanges();
            TempData["Success"] = "Cập nhật trạng thái đơn hàng thành công!";

            if (!string.IsNullOrEmpty(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("ChiTietDonHang", new { id = MaDH });
        }


        public ActionResult ChiTietDonHang(int id)
        {
            var dh = ql.DonHangs.FirstOrDefault(d => d.MaDH == id);
            if (dh == null) return HttpNotFound();
            return View(dh);
        }

      
        private List<SanPhamBanChay> GetTop5SanPhamBanChay()
        {
            var top5 = ql.ChiTietDonHangs
                .Where(ct => ct.DonHang.TrangThai == "hoan_thanh") 
                .GroupBy(ct => ct.SanPham.TenSP)
                .Select(g => new SanPhamBanChay
                {
                    TenSP = g.Key,
                    SoLuongBan = g.Sum(ct => ct.SoLuong)
                })
                .OrderByDescending(x => x.SoLuongBan)
                .Take(5)
                .ToList();

   
            if (top5.Any())
            {
                int max = top5.Max(x => x.SoLuongBan);
                foreach (var item in top5)
                {
                    item.phantramban = max > 0 ? (double)item.SoLuongBan / max * 100 : 0;
                }
            }

            return top5;
        }

    }
}