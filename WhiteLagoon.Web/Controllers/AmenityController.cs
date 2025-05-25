using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Application.Common.Utility;
using WhiteLagoon.Domain.Entities;
using WhiteLagoon.Infrastructure.Data;
using WhiteLagoon.Web.ViewModels;

namespace WhiteLagoon.Web.Controllers
{
    [Authorize(Roles = SD.Role_Admin)]
    public class AmenityController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public AmenityController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            var Amenitys = _unitOfWork.Amenity.GetAll(includeProperties: "Villa");
            return View(Amenitys);
        }

        public IActionResult Create()
        {
            AmenityVM amenityVM = new()
            {
                VillaList = _unitOfWork.Villa.GetAll().Select(x => new SelectListItem
                {
                    Text = x.Name,
                    Value = x.Id.ToString()
                })
            };
            return View(amenityVM);
        }

        [HttpPost]
        public IActionResult Create(AmenityVM amenityVM)
        {
            if (ModelState.IsValid)
            {
                _unitOfWork.Amenity.Add(amenityVM.Amenity);
                _unitOfWork.Save();
                TempData["success"] = "The amenity created successfully!";
                return RedirectToAction(nameof(Index));
            }

            amenityVM.VillaList = _unitOfWork.Villa.GetAll().Select(x => new SelectListItem
            {
                Text = x.Name,
                Value = x.Id.ToString()
            });
            return View(amenityVM);
        }

        public IActionResult Update(int amenityId)
        {
            AmenityVM amenityVM = new()
            {
                VillaList = _unitOfWork.Villa.GetAll().Select(x => new SelectListItem
                {
                    Text = x.Name,
                    Value = x.Id.ToString()
                }),
                Amenity = _unitOfWork.Amenity.Get(x => x.Id.Equals(amenityId))
            };
            if (amenityVM is null)
            {
                return RedirectToAction("Error", "Home");
            }
            return View(amenityVM);
        }

        [HttpPost]
        public IActionResult Update(AmenityVM AmenityVM)
        {
            if (ModelState.IsValid)
            {
                _unitOfWork.Amenity.Update(AmenityVM.Amenity);
                _unitOfWork.Save();
                TempData["success"] = "The amenity updated successfully!";
                return RedirectToAction(nameof(Index));
            }

            AmenityVM.VillaList = _unitOfWork.Villa.GetAll().Select(x => new SelectListItem
            {
                Text = x.Name,
                Value = x.Id.ToString()
            });
            return View(AmenityVM);
        }

        public IActionResult Delete(int amenityId)
        {
            AmenityVM amenityVM = new()
            {
                VillaList = _unitOfWork.Villa.GetAll().Select(x => new SelectListItem
                {
                    Text = x.Name,
                    Value = x.Id.ToString()
                }),
                Amenity = _unitOfWork.Amenity.Get(x => x.Id.Equals(amenityId))
            };
            if (amenityVM is null)
            {
                return RedirectToAction("Error", "Home");
            }
            return View(amenityVM);
        }

        [HttpPost]
        public IActionResult Delete(AmenityVM amenityVM)
        {
            Amenity? amenityFromDb = _unitOfWork.Amenity.Get(x => x.Id.Equals(amenityVM.Amenity.Id));
            if (amenityFromDb is not null)
            {
                _unitOfWork.Amenity.Remove(amenityFromDb);
                _unitOfWork.Save();
                TempData["success"] = "The amenity deleted successfully!";
                return RedirectToAction(nameof(Index));
            }
            TempData["error"] = "The amenity could not be deleted!";
            return View();
        }
    }
}
