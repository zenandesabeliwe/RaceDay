namespace RaceDay.Api.Models;

public class Result
{
    public int resultID { get; set; }
    public TimeSpan finishTime { get; set; }
    public int finishPosition { get; set; }
    public int enrolmentID { get; set; }
}
