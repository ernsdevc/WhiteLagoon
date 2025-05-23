using WhiteLagoon.Domain.Entities;

namespace WhiteLagoon.Web.ViewModels
{
    public class HomeVM
    {
        public IEnumerable<Villa>? VillaList { get; set; }
        public DateOnly ChecInDate { get; set; }
        public DateOnly ChecOutDate { get; set; }
        public int Nights { get; set; }
    }
}
