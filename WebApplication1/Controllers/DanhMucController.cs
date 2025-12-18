using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebApplication1.Controllers
{
    public class DanhMucController : Controller
    {
        ShopGearEntities1 bh = new ShopGearEntities1();
        //
        // GET: /DanhMuc/
        public ActionResult _DanhMuc()
        {
            return PartialView();
        }

        public ActionResult _DanhMucTL()
        {
            List<LoaiSanPham> tl = bh.LoaiSanPhams.ToList();
            return PartialView(tl);
        }

        public ActionResult _DanhMucNCC()
        {
            List<NhaCungCap> tl = bh.NhaCungCaps.ToList();
            return PartialView(tl);
        }

        public ActionResult _TimKiemNC()
        {
            List<LoaiSanPham> tl = bh.LoaiSanPhams.ToList();
            return PartialView(tl);
        }
	}
}