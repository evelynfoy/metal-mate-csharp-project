using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using Metal_Mate_MVC.DTOs;

namespace Metal_Mate_MVC.Models.ViewModels
{
    public class AlertRequestViewModel : ICommonSelectLists
    {
        public int Id { get; set; }

        public string Metal { get; set; } = string.Empty;

        public string Currency { get; set; } = string.Empty;

        public ComparisonOperator Operator { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Value must be a positive integer.")]
        public int Value { get; set; }

        public bool IsEnabled { get; set; } = true;

        public IEnumerable<SelectListItem> Metals { get; set; } = Enumerable.Empty<SelectListItem>();
        public IEnumerable<SelectListItem> Currencies { get; set; } = Enumerable.Empty<SelectListItem>();

        public string ErrorMessage { get; set; } = String.Empty;
    }
}
