using Microsoft.AspNetCore.Mvc.Rendering;

namespace Metal_Mate_MVC.DTOs
{
    public interface ICommonSelectLists
    {
        IEnumerable<SelectListItem> Metals { get; set; }
        IEnumerable<SelectListItem> Currencies { get; set; }
    }

    public class CommonSelectLists
    {
        public IEnumerable<SelectListItem> Metals { get; init; } = [];
        public IEnumerable<SelectListItem> Currencies { get; init; } = [];
    }
}
