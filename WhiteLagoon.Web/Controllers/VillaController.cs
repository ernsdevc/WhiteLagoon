using Microsoft.AspNetCore.Mvc;
using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Domain.Entities;
using WhiteLagoon.Infrastructure.Data;

namespace WhiteLagoon.Web.Controllers
{
    public class VillaController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public VillaController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            var villas = _unitOfWork.Villa.GetAll();
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
                _unitOfWork.Villa.Add(villa);
                _unitOfWork.Save();
                TempData["success"] = "The villa created successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View();
        }

        public IActionResult Update(int villaId)
        {
            Villa? villa = _unitOfWork.Villa.Get(x => x.Id.Equals(villaId));
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
                _unitOfWork.Villa.Update(villa);
                _unitOfWork.Save();
                TempData["success"] = "The villa updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View();
        }

        public IActionResult Delete(int villaId)
        {
            Villa? villa = _unitOfWork.Villa.Get(x => x.Id.Equals(villaId));
            if (villa is null)
            {
                return RedirectToAction("Error", "Home");
            }
            return View(villa);
        }

        [HttpPost]
        public IActionResult Delete(Villa villa)
        {
            Villa? villaFromDb = _unitOfWork.Villa.Get(x => x.Id.Equals(villa.Id));
            if (villaFromDb is not null)
            {
                _unitOfWork.Villa.Remove(villaFromDb);
                _unitOfWork.Save();
                TempData["success"] = "The villa deleted successfully!";
                return RedirectToAction(nameof(Index));
            }
            TempData["error"] = "The villa could not be deleted!";
            return View();
        }
    }
}
