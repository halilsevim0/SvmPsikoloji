using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EnsoPsikoloji.Models;

namespace EnsoPsikoloji.Controllers
{
    [Authorize(Roles = "Admin")]
    public class KullanicilarController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;

        public KullanicilarController(
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        // GET: /Kullanicilar/Index
        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            var model = new List<PersonelListViewModel>();

            foreach (var user in users)
            {
                var roller = await _userManager.GetRolesAsync(user);
                model.Add(new PersonelListViewModel
                {
                    Id = user.Id,
                    Email = user.Email ?? "",
                    UserName = user.UserName ?? "",
                    Roller = string.Join(", ", roller)
                });
            }

            return View(model);
        }

        // GET: /Kullanicilar/Create
        public IActionResult Create()
        {
            ViewBag.Roller = new SelectList(_roleManager.Roles.Select(r => r.Name).ToList());
            return View();
        }

        // POST: /Kullanicilar/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PersonelOlusturViewModel model)
        {
            if (ModelState.IsValid)
            {
                var yeniKullanici = new IdentityUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    EmailConfirmed = true
                };

                var sonuc = await _userManager.CreateAsync(yeniKullanici, model.Password);

                if (sonuc.Succeeded)
                {
                    if (!string.IsNullOrEmpty(model.SecilenRol))
                    {
                        await _userManager.AddToRoleAsync(yeniKullanici, model.SecilenRol);
                    }

                    if (model.SecilenRol == "Psikolog")
                    {
                        var psikolog = new Psikolog
                        {
                            AdSoyad = string.IsNullOrWhiteSpace(model.AdSoyad) ? model.Email : model.AdSoyad,
                            UzmanlikAlani = model.UzmanlikAlani ?? "Genel Psikoloji",
                            Telefon = model.Telefon,
                            Email = model.Email,
                            UserId = yeniKullanici.Id
                        };

                        _context.Psikologlar.Add(psikolog);
                        await _context.SaveChangesAsync();
                    }

                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in sonuc.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            ViewBag.Roller = new SelectList(_roleManager.Roles.Select(r => r.Name).ToList());
            return View(model);
        }

        // POST: /Kullanicilar/Delete/id
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                var bagliPsikolog = await _context.Psikologlar.FirstOrDefaultAsync(p => p.UserId == id);
                if (bagliPsikolog != null)
                {
                    _context.Psikologlar.Remove(bagliPsikolog);
                    await _context.SaveChangesAsync();
                }

                await _userManager.DeleteAsync(user);
            }
            return RedirectToAction(nameof(Index));
        }
    }
}