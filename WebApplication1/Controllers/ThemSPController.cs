using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class ThemSPController : Controller
    {
        private readonly ShopGearEntities1 ql = new ShopGearEntities1();

        // Kiểm tra quyền admin trước mỗi action
        private bool IsAdmin()
        {
            // Kiểm tra user đã đăng nhập
            if (string.IsNullOrEmpty(Convert.ToString(Session["MaTK"])))
                return false;

            // Kiểm tra vai trò là admin
            var maTk = (int)Session["MaTK"];
            var user = ql.TaiKhoans.Find(maTk);
            return user != null && user.VaiTro == "admin"; // hoặc "Admin"
        }

        private ActionResult RedirectIfNotAdmin()
        {
            if (!IsAdmin())
                return RedirectToAction("Index", "DangNhap");
            return null;
        }

        // GET: ThemSP
        public ActionResult Index()
        {
            var check = RedirectIfNotAdmin();
            if (check != null) return check;

            var sp = new SP
            {
                ListLoai = ql.LoaiSanPhams.ToList(),
                ListNCC = ql.NhaCungCaps.ToList()
            };
            return View(sp);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Themsp(SP sp, HttpPostedFileBase HinhAnh)
        {
            var check = RedirectIfNotAdmin();
            if (check != null) return check;

            sp.ListLoai = ql.LoaiSanPhams.ToList();
            sp.ListNCC = ql.NhaCungCaps.ToList();

            if (!ModelState.IsValid)
                return View("Index", sp);

            try
            {
                // Validate dữ liệu bắt buộc
                if (string.IsNullOrWhiteSpace(sp.TenSP))
                {
                    ModelState.AddModelError("TenSP", "Tên sản phẩm không được để trống");
                    return View("Index", sp);
                }

                if (!sp.MaLoai.HasValue || sp.MaLoai <= 0)
                {
                    ModelState.AddModelError("MaLoai", "Vui lòng chọn loại sản phẩm");
                    return View("Index", sp);
                }

                if (!sp.MaNCC.HasValue || sp.MaNCC <= 0)
                {
                    ModelState.AddModelError("MaNCC", "Vui lòng chọn nhà cung cấp");
                    return View("Index", sp);
                }

                var gear = new SanPham
                {
                    TenSP = sp.TenSP.Trim(),
                    MaLoai = sp.MaLoai.Value,
                    MaNCC = sp.MaNCC.Value,
                    GiaBan = sp.GiaBan,
                    SoLuong = sp.SoLuong ?? 0,
                    MoTa = sp.MoTa,
                    ThongSoKyThuat = sp.ThongSoKyThuat,
                    NgayTao = DateTime.Now,
                    HinhAnh = "default.jpg"
                };

                // Xử lý upload hình ảnh
                if (HinhAnh != null && HinhAnh.ContentLength > 0)
                {
                    var ext = Path.GetExtension(HinhAnh.FileName).ToLower();
                    if (ext != ".jpg" && ext != ".jpeg" && ext != ".png")
                    {
                        ModelState.AddModelError("HinhAnh", "Chỉ chấp nhận file JPG/JPEG/PNG");
                        return View("Index", sp);
                    }

                    try
                    {
                        var newFileName = Guid.NewGuid() + ext;
                        var uploadPath = Server.MapPath("~/Content/HinhAnh/");

                        if (!Directory.Exists(uploadPath))
                            Directory.CreateDirectory(uploadPath);

                        var filePath = Path.Combine(uploadPath, newFileName);
                        HinhAnh.SaveAs(filePath);
                        gear.HinhAnh = newFileName;
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("HinhAnh", "Lỗi upload ảnh: " + ex.Message);
                        return View("Index", sp);
                    }
                }

                ql.SanPhams.Add(gear);
                ql.SaveChanges();

                TempData["Success"] = "✓ Thêm sản phẩm thành công!";
                return RedirectToAction("Index", "Banhang");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "✗ Lỗi: " + ex.Message;
                return View("Index", sp);
            }
        }

        // ======================= SỬA =======================
        public ActionResult Sua(int id)
        {
            var check = RedirectIfNotAdmin();
            if (check != null) return check;

            var sp = ql.SanPhams.Find(id);
            if (sp == null)
                return HttpNotFound();

            var vm = new SP
            {
                MaSP = sp.MaSP,
                TenSP = sp.TenSP,
                GiaNhap = sp.GiaNhap,
                GiaBan = sp.GiaBan,
                SoLuong = sp.SoLuong ?? 0,
                MoTa = sp.MoTa,
                ThongSoKyThuat = sp.ThongSoKyThuat,
                MaLoai = sp.MaLoai,
                MaNCC = sp.MaNCC,
                NgayTao = sp.NgayTao ?? DateTime.Now,
                HinhAnh = sp.HinhAnh,
                ListLoai = ql.LoaiSanPhams.ToList(),
                ListNCC = ql.NhaCungCaps.ToList()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Suasp(SP vm, HttpPostedFileBase HinhAnh)
        {
            var check = RedirectIfNotAdmin();
            if (check != null) return check;

            vm.ListLoai = ql.LoaiSanPhams.ToList();
            vm.ListNCC = ql.NhaCungCaps.ToList();

            if (!ModelState.IsValid)
                return View("Sua", vm);

            // Validate dữ liệu
            if (string.IsNullOrWhiteSpace(vm.TenSP))
            {
                ModelState.AddModelError("TenSP", "Tên sản phẩm không được để trống");
                return View("Sua", vm);
            }

            if (!vm.MaLoai.HasValue || vm.MaLoai <= 0)
            {
                ModelState.AddModelError("MaLoai", "Vui lòng chọn loại sản phẩm");
                return View("Sua", vm);
            }

            if (!vm.MaNCC.HasValue || vm.MaNCC <= 0)
            {
                ModelState.AddModelError("MaNCC", "Vui lòng chọn nhà cung cấp");
                return View("Sua", vm);
            }

            var sp = ql.SanPhams.Find(vm.MaSP);
            if (sp == null)
                return HttpNotFound();

            sp.TenSP = vm.TenSP.Trim();
            sp.GiaNhap = vm.GiaNhap;
            sp.GiaBan = vm.GiaBan;
            sp.SoLuong = vm.SoLuong ?? 0;
            sp.MoTa = vm.MoTa;
            sp.ThongSoKyThuat = vm.ThongSoKyThuat;
            sp.MaLoai = vm.MaLoai.Value;
            sp.MaNCC = vm.MaNCC.Value;

            // Xử lý upload ảnh mới
            if (HinhAnh != null && HinhAnh.ContentLength > 0)
            {
                var ext = Path.GetExtension(HinhAnh.FileName).ToLower();
                if (ext != ".jpg" && ext != ".jpeg" && ext != ".png")
                {
                    ModelState.AddModelError("HinhAnh", "Chỉ chấp nhận file JPG/JPEG/PNG");
                    return View("Sua", vm);
                }

                try
                {
                    // Xóa ảnh cũ
                    if (!string.IsNullOrEmpty(sp.HinhAnh) && sp.HinhAnh != "default.jpg")
                    {
                        var oldPath = Path.Combine(Server.MapPath("~/Content/HinhAnh/"), sp.HinhAnh);
                        if (System.IO.File.Exists(oldPath))
                            System.IO.File.Delete(oldPath);
                    }

                    // Upload ảnh mới
                    var newFileName = Guid.NewGuid() + ext;
                    var uploadPath = Server.MapPath("~/Content/HinhAnh/");

                    if (!Directory.Exists(uploadPath))
                        Directory.CreateDirectory(uploadPath);

                    var filePath = Path.Combine(uploadPath, newFileName);
                    HinhAnh.SaveAs(filePath);
                    sp.HinhAnh = newFileName;
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("HinhAnh", "Lỗi upload ảnh: " + ex.Message);
                    return View("Sua", vm);
                }
            }

            try
            {
                ql.SaveChanges();
                TempData["Success"] = "✓ Sửa sản phẩm thành công!";
                return RedirectToAction("Index", "Banhang");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "✗ Lỗi: " + ex.Message;
                return View("Sua", vm);
            }
        }

        // ======================= XÓA =======================
        public ActionResult Xoa(int id)
        {
            var check = RedirectIfNotAdmin();
            if (check != null) return check;

            var sp = ql.SanPhams.Find(id);
            if (sp == null)
                return HttpNotFound();

            var vm = new SP
            {
                MaSP = sp.MaSP,
                TenSP = sp.TenSP,
                GiaNhap = sp.GiaNhap,
                GiaBan = sp.GiaBan,
                SoLuong = sp.SoLuong ?? 0,
                MoTa = sp.MoTa,
                NgayTao = sp.NgayTao,
                ThongSoKyThuat = sp.ThongSoKyThuat,
                MaLoai = sp.MaLoai,
                MaNCC = sp.MaNCC,
                HinhAnh = sp.HinhAnh,
                ListLoai = ql.LoaiSanPhams.ToList(),
                ListNCC = ql.NhaCungCaps.ToList()
            };

            return View(vm);
        }

        [HttpPost, ActionName("Xoa")]
        [ValidateAntiForgeryToken]
        public ActionResult XoaConfirmed(int id)
        {
            var check = RedirectIfNotAdmin();
            if (check != null) return check;

            var sp = ql.SanPhams.Find(id);
            if (sp == null)
                return HttpNotFound();

            try
            {
                // Xóa ảnh nếu không phải default
                if (!string.IsNullOrEmpty(sp.HinhAnh) && sp.HinhAnh != "default.jpg")
                {
                    var oldPath = Path.Combine(Server.MapPath("~/Content/HinhAnh/"), sp.HinhAnh);
                    if (System.IO.File.Exists(oldPath))
                    {
                        try
                        {
                            System.IO.File.Delete(oldPath);
                        }
                        catch
                        {
                            // Không dừng xóa sản phẩm nếu xóa ảnh thất bại
                        }
                    }
                }

                ql.SanPhams.Remove(sp);
                ql.SaveChanges();

                TempData["Success"] = "✓ Xóa sản phẩm thành công!";
                return RedirectToAction("Index", "Banhang");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "✗ Lỗi: " + ex.Message;
                return RedirectToAction("Index", "Banhang");
            }
        }
    }
}