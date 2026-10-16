namespace RaceDay.Api.Models
{
    public class Participant
    {
        //Primary key
        public int participantID { get; set; }

        //Foreign key from Users table
        public int userID { get; set; }

        //Foreign key from Events table (Part 1 ERD)
        public int? eventID { get; set; }
    }
}