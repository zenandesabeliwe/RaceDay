namespace RaceDay.Api.Models;

public class Enrolment
{
    public int enrolmentID { get; set; }
    public DateTime enrolmentDate { get; set; }
    public int eventID { get; set; }
    public int participantID { get; set; }
    public int categoryID { get; set; }
}
