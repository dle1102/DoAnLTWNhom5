using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication1.Models
{
    public class SP:SanPham
    {
        public List<LoaiSanPham> ListLoai { get;set;}
        public List<NhaCungCap> ListNCC { get; set; }
    }
}