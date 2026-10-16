namespace RaceDay.Api.Models;

public class Event
{
    public int eventID { get; set; }
    public string eventName { get; set; } = "";
    public string description { get; set; } = "";
    public DateTime eventDate { get; set; }
    public string location { get; set; } = "";
    public decimal distance { get; set; }
    public string eventType { get; set; } = "Run";
    public int organiserID { get; set; }
}
