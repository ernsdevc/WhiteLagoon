using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WhiteLagoon.Domain.Entities;
using WhiteLagoon.Infrastructure.Data;

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
            var villaNumbers = _db.VillaNumbers.ToList();
            return View(villaNumbers);
        }

        public IActionResult Create()
        {
            IEnumerable<SelectListItem> list = _db.Villas.ToList().Select(x => new SelectListItem
            {
                Text = x.Name,
                Value = x.Id.ToString()
            });
            ViewBag.villaList = list;
            return View();
        }

        [HttpPost]
        public IActionResult Create(VillaNumber villaNumber)
        {
            if (ModelState.IsValid)
            {
                _db.VillaNumbers.Add(villaNumber);
                _db.SaveChanges();
                TempData["success"] = "The villa number created successfully!";
                return RedirectToAction("Index");
            }
            return View();
        }
    }
}
