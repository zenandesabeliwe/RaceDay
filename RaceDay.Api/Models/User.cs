namespace RaceDay.Api.Models;

// Matches the Users table in the RaceDay database.
public class User
{
    public int userID { get; set; }
    public string fullName { get; set; } = "";
    public string emailAddress { get; set; } = "";
    public string phoneNumber { get; set; } = "";
    // This property contains the HASH, not the original password.
    public string password { get; set; } = "";
    public string userRole { get; set; } = "Participant";
}
