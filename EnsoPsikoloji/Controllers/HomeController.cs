using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EnsoPsikoloji.Models;

namespace EnsoPsikoloji.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Home/Index (Ziyaretcinin gordugu vitrin ve doktor kadrosu sayfasi)
        public async Task<IActionResult> Index()
        {
            var psikologlar = await _context.Psikologlar.ToListAsync();
            return View(psikologlar);
        }

        // GET: /Home/SeedData (Tek tikla ornek verileri yukleme)
        public async Task<IActionResult> SeedData()
        {
            var sonuc = await DbInitializer.SeedOrnekVerilerAsync(_context);
            TempData["BasariMesaji"] = "Klinik verileri basariyla yuklendi! (" + sonuc + ")";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Home/TalepGonder (Danisan formdan iletisim bilgisi biraktiginda)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TalepGonder(IletisimTalep talep)
        {
            if (ModelState.IsValid)
            {
                _context.IletisimTalepleri.Add(talep);
                await _context.SaveChangesAsync();
                TempData["BasariMesaji"] = "Bilgileriniz basariyla alindi! Klinik koordinatorumuz en kisa surede sizinle iletisime gececektir.";
                return RedirectToAction(nameof(Index));
            }

            var psikologlar = await _context.Psikologlar.ToListAsync();
            return View("Index", psikologlar);
        }

        // GET: /Home/Talepler (Sadece Sekreter ve Admin gelen arama taleplerini gorsun)
        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin,Sekreter")]
        public async Task<IActionResult> Talepler()
        {
            var talepler = await _context.IletisimTalepleri
                .OrderByDescending(t => t.OlusturmaTarihi)
                .ToListAsync();
            return View(talepler);
        }

        // POST: /Home/TalepTamamla/5 (Telefonla arandiktan sonra 'Arandi' isaretleme)
        [HttpPost]
        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin,Sekreter")]
        public async Task<IActionResult> TalepTamamla(int id)
        {
            var talep = await _context.IletisimTalepleri.FindAsync(id);
            if (talep != null)
            {
                talep.ArandiMi = true;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Talepler));
        }
    }
}
