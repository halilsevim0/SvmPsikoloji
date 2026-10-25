using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EnsoPsikoloji.Models;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using EnsoPsikoloji.Services;

namespace EnsoPsikoloji.Controllers
{
    [Authorize(Roles = "Admin,Sekreter,Psikolog")] // Yalnizca yetkili klinik personeli erisebilir
    public class RandevularController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;
        private readonly ISmsService _smsService;

        public RandevularController(
            ApplicationDbContext context,
            IEmailService emailService,
            ISmsService smsService)
        {
            _context = context;
            _emailService = emailService;
            _smsService = smsService;
        }

        // GET: Randevular (Role Gore Akilli Filtreleme)
        public async Task<IActionResult> Index(string? arananKelime)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var randevular = _context.Randevular
                .Include(r => r.Hasta)
                .Include(r => r.Psikolog)
                .AsQueryable();

            // Eger giris yapan kullanici Psikolog rolundeyse sadece kendi randevularini gorsun
            if (User.IsInRole("Psikolog"))
            {
                randevular = randevular.Where(r => r.Psikolog != null && r.Psikolog.UserId == currentUserId);
            }

            if (!string.IsNullOrEmpty(arananKelime))
            {
                randevular = randevular.Where(r =>
                    (r.Hasta != null && (r.Hasta.Adi.Contains(arananKelime) || r.Hasta.Soyadi.Contains(arananKelime))) ||
                    (r.Psikolog != null && (r.Psikolog.AdSoyad.Contains(arananKelime))));
            }

            return View(await randevular.OrderByDescending(r => r.RandevuTarihi).ToListAsync());
        }

        // GET: Randevular/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var randevu = await _context.Randevular
                .Include(r => r.Hasta)
                .Include(r => r.Psikolog)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (randevu == null) return NotFound();

            // Psikolog baska bir psikologun detayini gormeye calisirsa engelle
            if (User.IsInRole("Psikolog") && randevu.Psikolog?.UserId != currentUserId)
            {
                return Forbid();
            }

            return View(randevu);
        }

        // GET: Randevular/Create
        public IActionResult Create()
        {
            ViewBag.HastaId = new SelectList(_context.Hastalar, "Id", "Adi");
            ViewBag.PsikologId = new SelectList(_context.Psikologlar.Select(p => new { Id = p.Id, AdSoyad = p.AdSoyad }), "Id", "AdSoyad");
            return View();
        }

        // POST: Randevular/Create (Cakisma Kontrollu ve Otomatik E-Posta / SMS Bildirimli)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Randevu randevu)
        {
            bool cakismaVar = await _context.Randevular.AnyAsync(r =>
                r.PsikologId == randevu.PsikologId &&
                r.RandevuTarihi == randevu.RandevuTarihi);

            if (cakismaVar)
            {
                ModelState.AddModelError("RandevuTarihi", "Secilen psikologun bu saatte baska bir randevusu bulunmaktadir!");
            }

            if (ModelState.IsValid && !cakismaVar)
            {
                _context.Randevular.Add(randevu);
                await _context.SaveChangesAsync();

                // Bildirimleri tetikle
                var kayitliRandevu = await _context.Randevular
                    .Include(r => r.Hasta)
                    .Include(r => r.Psikolog)
                    .FirstOrDefaultAsync(r => r.Id == randevu.Id);

                if (kayitliRandevu != null)
                {
                    // Danisana Onay Bildirimi
                    if (kayitliRandevu.Hasta != null)
                    {
                        var hastaAd = $"{kayitliRandevu.Hasta.Adi} {kayitliRandevu.Hasta.Soyadi}";
                        if (!string.IsNullOrEmpty(kayitliRandevu.Hasta.Email))
                        {
                            _ = _emailService.SendRandevuOnayMailiAsync(kayitliRandevu, kayitliRandevu.Hasta.Email, hastaAd, false);
                        }
                        if (!string.IsNullOrEmpty(kayitliRandevu.Hasta.Telefon))
                        {
                            _ = _smsService.SendRandevuSmsAsync(kayitliRandevu, hastaAd, kayitliRandevu.Hasta.Telefon, false);
                        }
                    }

                    // Psikologa Yeni Seans Bildirimi
                    if (kayitliRandevu.Psikolog != null && !string.IsNullOrEmpty(kayitliRandevu.Psikolog.Email))
                    {
                        _ = _emailService.SendRandevuOnayMailiAsync(kayitliRandevu, kayitliRandevu.Psikolog.Email, kayitliRandevu.Psikolog.AdSoyad, true);
                    }
                }

                TempData["BasariMesaji"] = "Randevu basariyla olusturuldu ve danisan/uzman bildirimleri iletildi.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.HastaId = new SelectList(_context.Hastalar, "Id", "Adi", randevu.HastaId);
            ViewBag.PsikologId = new SelectList(_context.Psikologlar.Select(p => new { Id = p.Id, AdSoyad = p.AdSoyad }), "Id", "AdSoyad", randevu.PsikologId);
            return View(randevu);
        }

        // POST: Randevular/Hatirlat/5 (Tek tikla danisana Randevu Hatirlatmasi Gonderme)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Hatirlat(int id)
        {
            var randevu = await _context.Randevular
                .Include(r => r.Hasta)
                .Include(r => r.Psikolog)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (randevu == null) return NotFound();

            if (randevu.Hasta != null)
            {
                var hastaAd = $"{randevu.Hasta.Adi} {randevu.Hasta.Soyadi}";
                bool mailGonderildi = false;
                bool smsGonderildi = false;

                if (!string.IsNullOrEmpty(randevu.Hasta.Email))
                {
                    await _emailService.SendRandevuHatirlatmaMailiAsync(randevu, randevu.Hasta.Email, hastaAd);
                    mailGonderildi = true;
                }

                if (!string.IsNullOrEmpty(randevu.Hasta.Telefon))
                {
                    smsGonderildi = await _smsService.SendRandevuSmsAsync(randevu, hastaAd, randevu.Hasta.Telefon, true);
                }

                TempData["BasariMesaji"] = $"Randevu hatirlatmasi gonderildi. (E-Posta: {(mailGonderildi ? "Iletildi" : "Kayitli Degil")}, SMS: {(smsGonderildi ? "Iletildi" : "Kayitli Degil")})";
            }
            else
            {
                TempData["HataMesaji"] = "Randevuya bagli danisan bulunamadi.";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Randevular/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var randevu = await _context.Randevular.FindAsync(id);
            if (randevu == null) return NotFound();

            // Psikolog baska bir psikologun randevusunu duzenleyemesin
            var psikolog = await _context.Psikologlar.FindAsync(randevu.PsikologId);
            if (User.IsInRole("Psikolog") && psikolog?.UserId != currentUserId)
            {
                return Forbid();
            }

            ViewBag.HastaId = new SelectList(_context.Hastalar, "Id", "Adi", randevu.HastaId);
            ViewBag.PsikologId = new SelectList(_context.Psikologlar.Select(p => new { Id = p.Id, AdSoyad = p.AdSoyad }), "Id", "AdSoyad", randevu.PsikologId);
            return View(randevu);
        }

        // POST: Randevular/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Randevu randevu)
        {
            if (id != randevu.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(randevu);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Randevular.Any(e => e.Id == id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewBag.HastaId = new SelectList(_context.Hastalar, "Id", "Adi", randevu.HastaId);
            ViewBag.PsikologId = new SelectList(_context.Psikologlar.Select(p => new { Id = p.Id, AdSoyad = p.AdSoyad }), "Id", "AdSoyad", randevu.PsikologId);
            return View(randevu);
        }

        // GET: Randevular/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var randevu = await _context.Randevular
                .Include(r => r.Hasta)
                .Include(r => r.Psikolog)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (randevu == null) return NotFound();

            return View(randevu);
        }

        // POST: Randevular/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var randevu = await _context.Randevular.FindAsync(id);
            if (randevu != null)
            {
                _context.Randevular.Remove(randevu);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Randevular/Takvim (Takvim Sayfasi)
        public IActionResult Takvim()
        {
            return View();
        }

        // GET: Randevular/GetRandevularJson (Takvimi Role Gore Filtreleme)
        [HttpGet]
        public IActionResult GetRandevularJson()
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var randevularQuery = _context.Randevular
                .Include(r => r.Hasta)
                .Include(r => r.Psikolog)
                .AsQueryable();

            // Psikolog takvimi actiginda yalnizca kendi seanslarini gorsun
            if (User.IsInRole("Psikolog"))
            {
                randevularQuery = randevularQuery.Where(r => r.Psikolog != null && r.Psikolog.UserId == currentUserId);
            }

            var etkinlikler = randevularQuery
                .Select(r => new
                {
                    id = r.Id,
                    title = (r.Hasta != null ? r.Hasta.Adi + " " + r.Hasta.Soyadi : "Hasta") + " - " +
                            (r.Psikolog != null ? "Uzm. " + r.Psikolog.AdSoyad : "Psikolog"),
                    start = r.RandevuTarihi.ToString("s"),
                    end = r.RandevuTarihi.AddHours(1).ToString("s"),
                    color = r.Durum == "Aktif" ? "#198754" : "#dc3545",
                    url = "/Randevular/Edit/" + r.Id
                })
                .ToList();

            return Json(etkinlikler);
        }
    }
}
