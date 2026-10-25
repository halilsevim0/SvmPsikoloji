using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EnsoPsikoloji.Models;

namespace EnsoPsikoloji.Controllers
{
    [Authorize(Roles = "Admin,Psikolog")]
    public class SeansNotlariController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public SeansNotlariController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // POST: SeansNotlari/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int hastaId, string notIcerigi, bool gizliNotMu, int? randevuId)
        {
            if (string.IsNullOrWhiteSpace(notIcerigi))
            {
                TempData["Hata"] = "Not içeriği boş olamaz.";
                return RedirectToAction("Details", "Hastalar", new { id = hastaId });
            }

            var userId = _userManager.GetUserId(User);
            var psikolog = await _context.Psikologlar.FirstOrDefaultAsync(p => p.UserId == userId);

            // Eğer işlemi yapan Admin ise ve profili yoksa ilk psikoloğu veya özel bir atamayı bağlayabiliriz
            int psikologId;
            if (psikolog != null)
            {
                psikologId = psikolog.Id;
            }
            else
            {
                var varsayilanPsikolog = await _context.Psikologlar.FirstOrDefaultAsync();
                if (varsayilanPsikolog == null)
                {
                    TempData["Hata"] = "Sistemde tanımlı psikolog bulunamadı.";
                    return RedirectToAction("Details", "Hastalar", new { id = hastaId });
                }
                psikologId = varsayilanPsikolog.Id;
            }

            var yeniNot = new SeansNotu
            {
                HastaId = hastaId,
                PsikologId = psikologId,
                RandevuId = randevuId,
                NotIcerigi = notIcerigi.Trim(),
                GizliNotMu = gizliNotMu,
                OlusturulmaTarihi = DateTime.Now
            };

            _context.SeansNotlari.Add(yeniNot);
            await _context.SaveChangesAsync();

            TempData["Mesaj"] = "Klinik seans notu başarıyla kaydedildi.";
            return RedirectToAction("Details", "Hastalar", new { id = hastaId });
        }
    }
}