using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Metal_Mate_MVC.Models.ViewModels
{
    public class AlertRequestIndexViewModel
    {
        public List<Metal_Mate_MVC.Models.AlertRequest> AlertRequests { get; set; }
            = new();

        public string ErrorMessage { get; set; } = string.Empty;
    }
}
