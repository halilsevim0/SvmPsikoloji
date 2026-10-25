using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EnsoPsikoloji.Models;
using EnsoPsikoloji.ViewModels;

namespace EnsoPsikoloji.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Admin/ (Genel Yonetim ve Finans Dashboard)
        public async Task<IActionResult> Index()
        {
            var model = await GetFinansRaporuModelAsync();
            return View(model);
        }

        // GET: /Admin/Finans (Detayli Finans ve Hakedis Raporu)
        public async Task<IActionResult> Finans(int? psikologId, int? yil, int? ay)
        {
            var model = await GetFinansRaporuModelAsync(psikologId, yil, ay);
            ViewBag.Psikologlar = await _context.Psikologlar.ToListAsync();
            ViewBag.SeciliPsikologId = psikologId;
            return View(model);
        }

        // POST: /Admin/OdemeDurumuGuncelle
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OdemeDurumuGuncelle(int randevuId, string yeniDurum, string donusUrl = "Index")
        {
            var randevu = await _context.Randevular.FindAsync(randevuId);
            if (randevu != null)
            {
                randevu.OdemeDurumu = yeniDurum;
                await _context.SaveChangesAsync();
                TempData["BasariMesaji"] = $"#{randevuId} numarali randevunun odeme durumu '{yeniDurum}' olarak guncellendi.";
            }

            if (donusUrl == "Finans")
                return RedirectToAction(nameof(Finans));

            return RedirectToAction(nameof(Index));
        }

        // POST: /Admin/OrnekVerileriYukle
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OrnekVerileriYukle()
        {
            var sonuc = await DbInitializer.SeedOrnekVerilerAsync(_context);
            TempData["BasariMesaji"] = "Ornek klinik verileri basariyla yuklendi! (" + sonuc + ")";
            return RedirectToAction(nameof(Index));
        }

        private async Task<FinansRaporViewModel> GetFinansRaporuModelAsync(int? psikologId = null, int? yil = null, int? ay = null)
        {
            var randevularQuery = _context.Randevular
                .Include(r => r.Hasta)
                .Include(r => r.Psikolog)
                .AsQueryable();

            if (psikologId.HasValue && psikologId.Value > 0)
            {
                randevularQuery = randevularQuery.Where(r => r.PsikologId == psikologId.Value);
            }

            if (yil.HasValue && yil.Value > 0)
            {
                randevularQuery = randevularQuery.Where(r => r.RandevuTarihi.Year == yil.Value);
            }

            if (ay.HasValue && ay.Value > 0)
            {
                randevularQuery = randevularQuery.Where(r => r.RandevuTarihi.Month == ay.Value);
            }

            var randevular = await randevularQuery.ToListAsync();

            var buAy = DateTime.Now.Month;
            var buYil = DateTime.Now.Year;

            var model = new FinansRaporViewModel
            {
                ToplamRandevuSayisi = randevular.Count,
                TamamlananRandevuSayisi = randevular.Count(r => r.Durum == "Tamamlandi" || (r.Durum == "Aktif" && r.RandevuTarihi < DateTime.Now)),
                IptalRandevuSayisi = randevular.Count(r => r.Durum == "Iptal"),

                ToplamCiro = randevular.Sum(r => r.Ucret ?? 0),
                TahsilEdilenUcret = randevular.Where(r => r.OdemeDurumu == "Odendi").Sum(r => r.Ucret ?? 0),
                BekleyenTahsilat = randevular.Where(r => r.OdemeDurumu != "Odendi").Sum(r => r.Ucret ?? 0),

                BuAyCiro = randevular.Where(r => r.RandevuTarihi.Year == buYil && r.RandevuTarihi.Month == buAy).Sum(r => r.Ucret ?? 0),
                BuAyTahsilEdilen = randevular.Where(r => r.RandevuTarihi.Year == buYil && r.RandevuTarihi.Month == buAy && r.OdemeDurumu == "Odendi").Sum(r => r.Ucret ?? 0),
                BuAyBekleyen = randevular.Where(r => r.RandevuTarihi.Year == buYil && r.RandevuTarihi.Month == buAy && r.OdemeDurumu != "Odendi").Sum(r => r.Ucret ?? 0),

                ToplamHastaSayisi = await _context.Hastalar.CountAsync(),
                ToplamPsikologSayisi = await _context.Psikologlar.CountAsync(),

                ToplamIletisimTalep = await _context.IletisimTalepleri.CountAsync(),
                AranmisTalepSayisi = await _context.IletisimTalepleri.CountAsync(t => t.ArandiMi)
            };

            // Psikolog Dagitimlari
            var psikologlar = await _context.Psikologlar.ToListAsync();
            foreach (var p in psikologlar)
            {
                var pRandevulari = randevular.Where(r => r.PsikologId == p.Id).ToList();
                model.PsikologOzetleri.Add(new PsikologFinansOzeti
                {
                    PsikologId = p.Id,
                    AdSoyad = p.AdSoyad,
                    UzmanlikAlani = p.UzmanlikAlani,
                    ToplamSeans = pRandevulari.Count,
                    ToplamUretilenCiro = pRandevulari.Sum(r => r.Ucret ?? 0),
                    TahsilEdilen = pRandevulari.Where(r => r.OdemeDurumu == "Odendi").Sum(r => r.Ucret ?? 0),
                    Bekleyen = pRandevulari.Where(r => r.OdemeDurumu != "Odendi").Sum(r => r.Ucret ?? 0)
                });
            }

            // Son 10 Seans
            model.SonSeanslar = randevular
                .OrderByDescending(r => r.RandevuTarihi)
                .Take(10)
                .Select(r => new SonSeansHareketi
                {
                    RandevuId = r.Id,
                    DanisanAdi = r.Hasta != null ? $"{r.Hasta.Adi} {r.Hasta.Soyadi}" : "Bilinmiyor",
                    PsikologAdi = r.Psikolog != null ? r.Psikolog.AdSoyad : "Bilinmiyor",
                    Tarih = r.RandevuTarihi,
                    Ucret = r.Ucret ?? 0,
                    OdemeDurumu = r.OdemeDurumu ?? "Bekliyor",
                    Durum = r.Durum ?? "Aktif"
                })
                .ToList();

            return model;
        }
    }
}
