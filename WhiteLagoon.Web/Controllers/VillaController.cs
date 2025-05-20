using Microsoft.AspNetCore.Mvc;
using WhiteLagoon.Domain.Entities;
using WhiteLagoon.Infrastructure.Data;

namespace WhiteLagoon.Web.Controllers
{
    public class VillaController : Controller
    {
        private readonly ApplicationDbContext _db;

        public VillaController(ApplicationDbContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            var villas = _db.Villas.ToList();
            return View(villas);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Villa villa)
        {
            if (villa.Name.Equals(villa.Description))
            {
                ModelState.AddModelError("name", "The description cannot exactly match the Name");
            }
            if (ModelState.IsValid)
            {
                _db.Villas.Add(villa);
                _db.SaveChanges();
                TempData["success"] = "The villa created successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View();
        }

        public IActionResult Update(int villaId)
        {
            Villa? villa = _db.Villas.Find(villaId);
            if (villa is null)
            {
                return RedirectToAction("Error", "Home");
            }
            return View(villa);
        }

        [HttpPost]
        public IActionResult Update(Villa villa)
        {
            if (ModelState.IsValid && villa.Id > 0)
            {
                _db.Villas.Update(villa);
                _db.SaveChanges();
                TempData["success"] = "The villa updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View();
        }

        public IActionResult Delete(int villaId)
        {
            Villa? villa = _db.Villas.Find(villaId);
            if (villa is null)
            {
                return RedirectToAction("Error", "Home");
            }
            return View(villa);
        }

        [HttpPost]
        public IActionResult Delete(Villa villa)
        {
            Villa? villaFromDb = _db.Villas.Find(villa.Id);
            if (villaFromDb is not null)
            {
                _db.Villas.Remove(villaFromDb);
                _db.SaveChanges();
                TempData["success"] = "The villa deleted successfully!";
                return RedirectToAction(nameof(Index));
            }
            TempData["error"] = "The villa could not be deleted!";
            return View();
        }
    }
}
