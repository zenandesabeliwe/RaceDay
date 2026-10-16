namespace RaceDay.Api.Models;

// These classes describe the data sent to the API.
public class RegisterRequest
{
    public string fullName { get; set; } = "";
    public string emailAddress { get; set; } = "";
    public string phoneNumber { get; set; } = "";
    public string password { get; set; } = "";
    public string userRole { get; set; } = "Participant";
}

public class LoginRequest
{
    public string emailAddress { get; set; } = "";
    public string password { get; set; } = "";
}

public class ProfileRequest
{
    public string fullName { get; set; } = "";
    public string phoneNumber { get; set; } = "";
}

public class EventRequest
{
    public string eventName { get; set; } = "";
    public string description { get; set; } = "";
    public DateTime eventDate { get; set; }
    public string location { get; set; } = "";
    public decimal distance { get; set; }
    public string eventType { get; set; } = "Run";
}

public class CategoryRequest
{
    public string categoryName { get; set; } = "";
    public decimal distance { get; set; }
    public int age { get; set; }
    public TimeSpan startTime { get; set; }
}

public class EnrolmentRequest
{
    public int eventID { get; set; }
    
}

public class ResultRequest
{
    public TimeSpan finishTime { get; set; }
    public int finishPosition { get; set; }
}
