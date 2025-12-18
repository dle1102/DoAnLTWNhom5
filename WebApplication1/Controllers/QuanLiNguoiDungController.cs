using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebApplication1.Controllers
{
    public class QuanLiNguoiDungController : Controller
    {
        // GET: QuanLiNguoiDung
        ShopGearEntities1 db = new ShopGearEntities1();
        public ActionResult Index()
        {
            var list = db.TaiKhoans
                    .Where(t => t.VaiTro == "khachhang")
                    .ToList();

            return View(list);
        }
        public ActionResult Xoa(int id)
        {
            var tk = db.TaiKhoans.Find(id);
            if (tk == null) return HttpNotFound();

            // Không cho xóa admin / nhân viên
            if (tk.VaiTro != "khachhang")
                return HttpNotFound();

            return View(tk);
        }

        // Xóa thực sự
        [HttpPost, ActionName("Xoa")]
        [ValidateAntiForgeryToken]
        public ActionResult XoaConfirmed(int id)
        {
            var tk = db.TaiKhoans.Find(id);
            if (tk == null) return HttpNotFound();

            if (tk.VaiTro != "khachhang")
                return HttpNotFound();

            db.TaiKhoans.Remove(tk);
            db.SaveChanges();

            return RedirectToAction("Index");
        }
    }
}