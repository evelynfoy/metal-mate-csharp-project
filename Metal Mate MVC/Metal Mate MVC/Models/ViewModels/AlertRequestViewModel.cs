using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Metal_Mate_MVC.Models.ViewModels
{
    public class AlertRequestViewModel
    {
        public string Metal { get; set; } = string.Empty;

        public string Currency { get; set; } = string.Empty;

        public ComparisonOperator Operator { get; set; }

        public int Value { get; set; }

        public bool IsEnabled { get; set; }

        public IEnumerable<SelectListItem>? Metals { get; set; } = Enumerable.Empty<SelectListItem>();
        public IEnumerable<SelectListItem>? Currencies { get; set; } = Enumerable.Empty<SelectListItem>();

        public string? ErrorMessage { get; set; }
    }
}
