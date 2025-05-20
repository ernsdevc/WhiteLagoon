using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WhiteLagoon.Domain.Entities;
using WhiteLagoon.Infrastructure.Data;
using WhiteLagoon.Web.ViewModels;

namespace WhiteLagoon.Web.Controllers
{
    public class VillaNumberController : Controller
    {
        private readonly ApplicationDbContext _db;

        public VillaNumberController(ApplicationDbContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            var villaNumbers = _db.VillaNumbers.Include(x => x.Villa).ToList();
            return View(villaNumbers);
        }

        public IActionResult Create()
        {
            VillaNumberVM villaNumberVM = new()
            {
                VillaList = _db.Villas.ToList().Select(x => new SelectListItem
                {
                    Text = x.Name,
                    Value = x.Id.ToString()
                })
            };
            return View(villaNumberVM);
        }

        [HttpPost]
        public IActionResult Create(VillaNumberVM villaNumberVM)
        {
            bool roomNumberExist = _db.VillaNumbers.Any(x => x.Villa_Number.Equals(villaNumberVM.VillaNumber.Villa_Number));
            if (roomNumberExist)
            {
                TempData["error"] = "The villa number already exists!";
            }
            if (ModelState.IsValid && !roomNumberExist)
            {
                _db.VillaNumbers.Add(villaNumberVM.VillaNumber);
                _db.SaveChanges();
                TempData["success"] = "The villa number created successfully!";
                return RedirectToAction(nameof(Index));
            }

            villaNumberVM.VillaList = _db.Villas.ToList().Select(x => new SelectListItem
            {
                Text = x.Name,
                Value = x.Id.ToString()
            });
            return View(villaNumberVM);
        }

        public IActionResult Update(int villaNumberId)
        {
            VillaNumberVM villaNumberVM = new()
            {
                VillaList = _db.Villas.ToList().Select(x => new SelectListItem
                {
                    Text = x.Name,
                    Value = x.Id.ToString()
                }),
                VillaNumber = _db.VillaNumbers.FirstOrDefault(x => x.Villa_Number.Equals(villaNumberId))
            };
            if (villaNumberVM is null)
            {
                return RedirectToAction("Error", "Home");
            }
            return View(villaNumberVM);
        }

        [HttpPost]
        public IActionResult Update(VillaNumberVM villaNumberVM)
        {
            if (ModelState.IsValid)
            {
                _db.VillaNumbers.Update(villaNumberVM.VillaNumber);
                _db.SaveChanges();
                TempData["success"] = "The villa number updated successfully!";
                return RedirectToAction(nameof(Index));
            }

            villaNumberVM.VillaList = _db.Villas.ToList().Select(x => new SelectListItem
            {
                Text = x.Name,
                Value = x.Id.ToString()
            });
            return View(villaNumberVM);
        }

        public IActionResult Delete(int villaNumberId)
        {
            VillaNumberVM villaNumberVM = new()
            {
                VillaList = _db.Villas.ToList().Select(x => new SelectListItem
                {
                    Text = x.Name,
                    Value = x.Id.ToString()
                }),
                VillaNumber = _db.VillaNumbers.FirstOrDefault(x => x.Villa_Number.Equals(villaNumberId))
            };
            if (villaNumberVM is null)
            {
                return RedirectToAction("Error", "Home");
            }
            return View(villaNumberVM);
        }

        [HttpPost]
        public IActionResult Delete(VillaNumberVM villaNumberVM)
        {
            VillaNumber? villaNumberFromDb = _db.VillaNumbers.Find(villaNumberVM.VillaNumber.Villa_Number);
            if (villaNumberFromDb is not null)
            {
                _db.VillaNumbers.Remove(villaNumberFromDb);
                _db.SaveChanges();
                TempData["success"] = "The villa number deleted successfully!";
                return RedirectToAction(nameof(Index));
            }
            TempData["error"] = "The villa number could not be deleted!";
            return View();
        }
    }
}
