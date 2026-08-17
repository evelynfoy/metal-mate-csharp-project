namespace Metal_Mate_MVC.Models
{
    public enum ComparisonOperator
    {
        LessThan,
        GreaterThan,
    }

    public class AlertRequest
    {
        public int Id { get; set; }
        public string Metal { get; set; } = null!;
        public string Currency { get; set; } = null!;
        public ComparisonOperator Operator { get; set; } = ComparisonOperator.LessThan;
        public int Value { get; set; } = 0;
        public bool IsEnabled { get; set; } = true;

        // Foreign key
        public string UserId { get; set; } = null!;

        // Navigation property
        public ApplicationUser User { get; set; } = null!;
    }
}
