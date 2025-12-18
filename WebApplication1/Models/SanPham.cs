using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebApplication1.Models;

namespace WebApplication1.Models
{
    public class sanpham:SanPham
    {
        public List<LoaiSanPham> lstLSP { get; set; }
        public List<NhaCungCap> lstNCC { get; set; }

    }
   

}