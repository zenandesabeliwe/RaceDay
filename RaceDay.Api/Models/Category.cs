namespace RaceDay.Api.Models;

public class Category
{
    public int categoryID { get; set; }
    public string categoryName { get; set; } = "";
    public decimal distance { get; set; }
    public int age { get; set; }
    public TimeSpan startTime { get; set; }
    public int eventID { get; set; }
    public int organiserID { get; set; }
}
