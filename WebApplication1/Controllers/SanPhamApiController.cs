using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace WebApplication1.Controllers
{
    [RoutePrefix("api/sanpham")]
    public class SanPhamApiController : ApiController
    {
        private ShopGearEntities1 BH = new ShopGearEntities1();
        [HttpGet]
        [Route("danhsach")]
        public IHttpActionResult DanhSach(int? maLoai = null, decimal? minPrice = null, decimal? maxPrice = null, string keyword = null)
        {
            IQueryable<SanPham> query = BH.SanPhams;

            // Lọc theo loại
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(sp => sp.TenSP.Contains(keyword) || sp.MoTa.Contains(keyword));
            }
            if (maLoai.HasValue && maLoai.Value > 0)
            {
                query = query.Where(sp => sp.MaLoai == maLoai.Value);
            }

            // Lọc theo giá min
            if (minPrice.HasValue)
            {
                query = query.Where(sp => sp.GiaBan >= minPrice.Value);
            }

            // Lọc theo giá max
            if (maxPrice.HasValue)
            {
                query = query.Where(sp => sp.GiaBan <= maxPrice.Value);
            }

            var result = query
                .OrderByDescending(s => s.GiaBan)
                .Select(s => new
                {
                    s.MaSP,
                    s.TenSP,
                    s.GiaBan,
                    s.HinhAnh
                })
                .ToList();

            return Ok(result);
        }
       


        [HttpGet]
        [Route("chitiet/{masp:int}")]
        public IHttpActionResult ChiTiet(int masp)
        {
            var sp = BH.SanPhams
                .Where(s => s.MaSP == masp)
                .Select(s => new
                {
                    s.MaSP,
                    s.TenSP,
                    s.GiaBan,
                    s.HinhAnh,
                    s.MoTa,
                    s.MaLoai,
                    s.MaNCC
                })
                .FirstOrDefault();

            if (sp == null)
                return NotFound();

            return Ok(sp);
        }

        [HttpGet]
        [Route("lienquan/{masp:int}")]
        public IHttpActionResult LienQuan(int masp)
        {
            var gear = BH.SanPhams.FirstOrDefault(s => s.MaSP == masp);
            if (gear == null) return NotFound();

            var lienquan = BH.SanPhams
                .Where(s => s.MaLoai == gear.MaLoai && s.MaSP != masp)
                .Select(s => new
                {
                    s.MaSP,
                    s.TenSP,
                    s.GiaBan,
                    s.HinhAnh
                })
                .Take(6)
                .ToList();

            return Ok(lienquan);
        }

        [HttpGet]
        [Route("cungncc/{masp:int}")]
        public IHttpActionResult CungNCC(int masp)
        {
            var gear = BH.SanPhams.FirstOrDefault(s => s.MaSP == masp);
            if (gear == null || gear.MaNCC == null)
                return Ok(new List<object>()); // không có NCC → trả list rỗng

            var sanPhamCungNCC = BH.SanPhams
                .Where(s => s.MaNCC == gear.MaNCC && s.MaSP != masp)
                .Select(s => new
                {
                    s.MaSP,
                    s.TenSP,
                    s.GiaBan,
                    s.HinhAnh
                })
                .Take(6)
                .ToList();

            return Ok(sanPhamCungNCC);
        }
    }
}
