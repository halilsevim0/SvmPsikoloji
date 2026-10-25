using Microsoft.AspNetCore.Mvc;
using EnsoPsikoloji.Models;
using System.Linq;
using Microsoft.AspNetCore.Authorization;

namespace EnsoPsikoloji.Controllers
{
    [Authorize(Roles = "Admin")] // Tüm Psikologlar modülü yalnızca Admin'e açık
    public class PsikologlarController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PsikologlarController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Psikologlar
        public IActionResult Index()
        {
            var psikologlarListesi = _context.Psikologlar.ToList();
            return View(psikologlarListesi);
        }

        // GET: Psikologlar/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Psikologlar/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Psikolog psikolog)
        {
            if (ModelState.IsValid)
            {
                _context.Psikologlar.Add(psikolog);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            return View(psikolog);
        }

        // GET: Psikologlar/Edit/5
        [HttpGet]
        public IActionResult Edit(int? id)
        {
            if (id == null) return NotFound();

            var psikolog = _context.Psikologlar.Find(id);
            if (psikolog == null) return NotFound();

            return View(psikolog);
        }

        // POST: Psikologlar/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Psikolog psikolog)
        {
            if (id != psikolog.Id) return NotFound();

            if (ModelState.IsValid)
            {
                _context.Update(psikolog);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            return View(psikolog);
        }

        // POST: Psikologlar/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var psikolog = _context.Psikologlar.Find(id);
            if (psikolog != null)
            {
                _context.Psikologlar.Remove(psikolog);
                _context.SaveChanges();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}