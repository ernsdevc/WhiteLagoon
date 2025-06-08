using System.Diagnostics;
using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Syncfusion.Presentation;
using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Application.Common.Utility;
using WhiteLagoon.Web.Models;
using WhiteLagoon.Web.ViewModels;

namespace WhiteLagoon.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public HomeController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            HomeVM homeVM = new HomeVM()
            {
                VillaList = _unitOfWork.Villa.GetAll(includeProperties: "VillaAmenity"),
                Nights = 1,
                CheckInDate = DateOnly.FromDateTime(DateTime.Now)
            };
            return View(homeVM);
        }

        [HttpPost]
        public IActionResult Index(HomeVM homeVM)
        {
            homeVM.VillaList = _unitOfWork.Villa.GetAll(includeProperties: "VillaAmenity");
            return View(homeVM);
        }

        public IActionResult GetVillasByDate(int nights, DateOnly checkInDate)
        {
            var villaList = _unitOfWork.Villa.GetAll(includeProperties: "VillaAmenity");
            var villaNumberList = _unitOfWork.VillaNumber.GetAll().ToList();
            var bookedVillas = _unitOfWork.Booking.GetAll(b => b.Status.Equals(SD.StatusApproved) || b.Status.Equals(SD.StatusCheckedIn)).ToList();

            foreach (var villa in villaList)
            {
                int roomAvailable = SD.VillaRoomsAvailable_Count(villa.Id, villaNumberList, checkInDate, nights, bookedVillas);

                villa.IsAvailable = roomAvailable > 0 ? true : false;
            }
            HomeVM homeVM = new HomeVM()
            {
                VillaList = villaList,
                Nights = nights,
                CheckInDate = checkInDate
            };
            return PartialView("_VillaListPartial", homeVM);
        }

        [HttpPost]
        public IActionResult GeneratePPTExport(int id)
        {
            var villa = _unitOfWork.Villa.Get(v => v.Id.Equals(id), includeProperties: "VillaAmenity");
            if (villa is null)
            {
                return RedirectToAction(nameof(Error));
            }

            string basePath = _webHostEnvironment.WebRootPath;
            string dataPath = basePath + @"/exports/ExportVillaDetails.pptx";

            using IPresentation presentation = Presentation.Open(dataPath);

            ISlide slide = presentation.Slides[0];

            IShape shape = slide.Shapes.FirstOrDefault(s => s.ShapeName.Equals("txtVillaName")) as IShape;
            if (shape is not null)
            {
                shape.TextBody.Text = villa.Name;
            }

            shape = slide.Shapes.FirstOrDefault(s => s.ShapeName.Equals("txtVillaDescription")) as IShape;
            if (shape is not null)
            {
                shape.TextBody.Text = villa.Description;
            }

            shape = slide.Shapes.FirstOrDefault(s => s.ShapeName.Equals("txtOccupancy")) as IShape;
            if (shape is not null)
            {
                shape.TextBody.Text = string.Format("Max Occupancy: {0} adults", villa.Occupancy);
            }

            shape = slide.Shapes.FirstOrDefault(s => s.ShapeName.Equals("txtVillaSize")) as IShape;
            if (shape is not null)
            {
                shape.TextBody.Text = string.Format("Villa Size: {0} sqft", villa.Sqft);
            }

            shape = slide.Shapes.FirstOrDefault(s => s.ShapeName.Equals("txtPricePerNight")) as IShape;
            if (shape is not null)
            {
                shape.TextBody.Text = string.Format("USD {0}/night", villa.Price.ToString("C", CultureInfo.GetCultureInfo("en-US")));
            }

            shape = slide.Shapes.FirstOrDefault(s => s.ShapeName.Equals("txtVillaAmenitiesHeading")) as IShape;
            if (shape is not null)
            {
                List<string> list = villa.VillaAmenity.Select(v => v.Name).ToList();

                shape.TextBody.Text = "";

                foreach (var item in list)
                {
                    IParagraph paragraph = shape.TextBody.AddParagraph();
                    ITextPart textPart = paragraph.AddTextPart(item);

                    paragraph.ListFormat.Type = ListType.Bulleted;
                    paragraph.ListFormat.BulletCharacter = '\u2022';
                    textPart.Font.FontName = "system-ui";
                    textPart.Font.FontSize = 18;
                    textPart.Font.Color = ColorObject.FromArgb(144, 148, 152);
                }
            }

            shape = slide.Shapes.FirstOrDefault(s => s.ShapeName.Equals("imgVilla")) as IShape;
            if (shape is not null)
            {
                byte[] imageData;
                string imageUrl;
                try
                {
                    imageUrl = string.Format("{0}{1}", basePath, villa.ImageUrl);
                    imageData = System.IO.File.ReadAllBytes(imageUrl);
                }
                catch (Exception)
                {
                    imageUrl = string.Format("{0}{1}", basePath, "images/placeholder.png");
                    imageData = System.IO.File.ReadAllBytes(imageUrl);
                }

                slide.Shapes.Remove(shape);
                using MemoryStream imageStream = new(imageData);
                IPicture newPicture = slide.Pictures.AddPicture(imageStream, 60, 120, 300, 200);
            }

            MemoryStream memoryStream = new();
            presentation.Save(memoryStream);
            memoryStream.Position = 0;
            return File(memoryStream, "application/pptx", "Villa.pptx");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
