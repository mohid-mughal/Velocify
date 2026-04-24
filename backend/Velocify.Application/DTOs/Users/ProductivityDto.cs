namespace Velocify.Application.DTOs.Users;

public class ProductivityDto
{
    public decimal CurrentScore { get; set; }
    public List<ProductivityHistoryPoint> History { get; set; } = new();
}

public class ProductivityHistoryPoint
{
    public DateTime Date { get; set; }
    public decimal Score { get; set; }
}
