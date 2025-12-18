using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication1.Models
{
    public class GioHang
    {
        ShopGearEntities1 db = new ShopGearEntities1();

        public int iMaSP { get; set; }
        public string sTenSP { get; set; }
        public string sHinhAnh { get; set; }
        public double dDonGia { get; set; }
        public int iSoLuong { get; set; }
        public double dThanhTien
        {
            get { return iSoLuong * dDonGia; }
        }

        public GioHang(int MaSP)
        {
            iMaSP = MaSP;
            // Use fully qualified name to avoid ambiguity if needed
            WebApplication1.SanPham sp = db.SanPhams.Single(n => n.MaSP == iMaSP);
            sTenSP = sp.TenSP;
            sHinhAnh = sp.HinhAnh;
            dDonGia = double.Parse(sp.GiaBan.ToString());
            iSoLuong = 1; // Mặc định là 1 khi thêm mới
        }

    }
}