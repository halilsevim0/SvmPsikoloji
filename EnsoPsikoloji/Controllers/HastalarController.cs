using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EnsoPsikoloji.Models;
using System.Linq;
using Microsoft.AspNetCore.Authorization;

namespace EnsoPsikoloji.Controllers
{
    // Kliniğin hastalarına yalnızca klinik personeli (Admin, Sekreter, Psikolog) erişebilir
    [Authorize(Roles = "Admin,Sekreter,Psikolog")]


    public class HastalarController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HastalarController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Hastalar (Tüm klinik personeli hastaları listeleyebilir ve arayabilir)
        public IActionResult Index(string arananKelime)
        {
            var hastalar = _context.Hastalar.AsQueryable();

            if (!string.IsNullOrEmpty(arananKelime))
            {
                hastalar = hastalar.Where(h => h.Adi.Contains(arananKelime) || h.Soyadi.Contains(arananKelime));
            }

            return View(hastalar.ToList());
        }

        // GET: Hastalar/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var hasta = await _context.Hastalar
                .Include(h => h.Randevular)
                    .ThenInclude(r => r.Psikolog)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (hasta == null)
            {
                return NotFound();
            }

            // Hastaya ait geçmiş klinik seans notlarını tarihe göre tersten çekiyoruz
            var seansNotlari = await _context.SeansNotlari
                .Include(s => s.Psikolog)
                .Where(s => s.HastaId == id)
                .OrderByDescending(s => s.OlusturulmaTarihi)
                .ToListAsync();

            ViewBag.SeansNotlari = seansNotlari;

            return View(hasta);
        }
        
        // GET: Hastalar/Create (Talep üzerinden gelindiğinde otomatik doldurma desteğiyle)
        [Authorize(Roles = "Admin,Sekreter")]
        public IActionResult Create(string? adSoyad, string? telefon, int? talepId)
        {
            var model = new Hasta();

            if (!string.IsNullOrEmpty(adSoyad))
            {
                var parcalar = adSoyad.Trim().Split(' ');
                if (parcalar.Length > 1)
                {
                    model.Soyadi = parcalar.Last();
                    model.Adi = string.Join(" ", parcalar.Take(parcalar.Length - 1));
                }
                else
                {
                    model.Adi = adSoyad;
                }
            }

            if (!string.IsNullOrEmpty(telefon))
            {
                model.Telefon = telefon;
            }

            ViewBag.TalepId = talepId;
            return View(model);
        }
        // POST: Hastalar/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Sekreter")]
        public async Task<IActionResult> Create(Hasta hasta, int? talepId)
        {
            if (ModelState.IsValid)
            {
                _context.Hastalar.Add(hasta);
                await _context.SaveChangesAsync();

                // Eğer bu hasta bir iletişim talebinden dönüştürüldüyse, talebi arandı olarak işaretleyelim
                if (talepId.HasValue)
                {
                    var talep = await _context.IletisimTalepleri.FindAsync(talepId.Value);
                    if (talep != null)
                    {
                        talep.ArandiMi = true;
                        await _context.SaveChangesAsync();
                    }
                }

                // Hastayı ekledikten sonra hemen randevu oluşturma sayfasına yönlendirelim
                return RedirectToAction("Create", "Randevular", new { hastaId = hasta.Id });
            }
            return View(hasta);
        }

        // GET: Hastalar/Edit/5 (Yalnızca Admin ve Sekreter hasta iletişim/profil bilgilerini düzenleyebilir)
        [Authorize(Roles = "Admin,Sekreter")]
        public IActionResult Edit(int? id)
        {
            if (id == null) return NotFound();

            var hasta = _context.Hastalar.Find(id);
            if (hasta == null) return NotFound();

            return View(hasta);
        }

        // POST: Hastalar/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Sekreter")]
        public IActionResult Edit(int id, Hasta hasta)
        {
            if (id != hasta.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(hasta);
                    _context.SaveChanges();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Hastalar.Any(e => e.Id == id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(hasta);
        }

        // GET: Hastalar/Delete/5 (HASTA SİLME YALNIZCA ADMİN'E AİTTİR)
        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int? id)
        {
            if (id == null) return NotFound();

            var hasta = _context.Hastalar.FirstOrDefault(m => m.Id == id);
            if (hasta == null) return NotFound();

            return View(hasta);
        }

        // POST: Hastalar/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var hasta = await _context.Hastalar.FindAsync(id);
            if (hasta != null)
            {
                // Fiziksel silme yerine Soft Delete uyguluyoruz
                hasta.IsDeleted = true;
                hasta.DeletedAt = DateTime.UtcNow;

                _context.Update(hasta);
                await _context.SaveChangesAsync();
                TempData["Mesaj"] = "Danışan kaydı güvenli şekilde arşive alındı (silindi).";
            }

            return RedirectToAction(nameof(Index));
        }
    }
    }
