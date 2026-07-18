namespace Metal_Mate_MVC.Models
{
    public class HomeViewModel
    {
        public SpotPrice? SpotPrice { get; set; }
        public List<Metal>? metals { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
