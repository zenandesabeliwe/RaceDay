
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using RaceDay.Api.database;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EnrolmentsController : ControllerBase
    {
        //database declarations
        private readonly DatabaseConnection database;
        private readonly RaceDayData data;

        //constructor
        public EnrolmentsController(IConfiguration configuration)
        {
            database = new DatabaseConnection(configuration);
            data = new RaceDayData(database);
        }

        //start of method to enrol a Participant in an event
        [HttpPost]
        public IActionResult create_enrolment(EnrolmentRequest request)
        {
            //temp variable declarations
            int? userID = HttpContext.Session.GetInt32("userID");
            string? role = HttpContext.Session.GetString("userRole");

            //check if user is logged in
            if (userID == null)
            {
                return Unauthorized("Please log in first.");
            }

            //check if user is a Participant
            if (role != "Participant")
            {
                return StatusCode(403, "Only Participants can enrol in events.");
            }

            //validate event ID
            if (request == null || request.eventID <= 0)
            {
                return BadRequest("Please enter a valid event ID.");
            }

            //try and catch
            try
            {
                //get Participant ID
                int participantID = data.get_participant_id(userID.Value);

                if (participantID == 0)
                {
                    return StatusCode(403, "Participant profile was not found.");
                }

                //check if event exists
                using SqlConnection connect = database.get_connection();
                connect.Open();

                string query = "SELECT COUNT(*) FROM Events WHERE eventID = @eventID;";

                using SqlCommand command = new SqlCommand(query, connect);
                command.Parameters.AddWithValue("@eventID", request.eventID);

                int eventCount = Convert.ToInt32(command.ExecuteScalar());

                if (eventCount == 0)
                {
                    return NotFound("Event not found.");
                }

                //check if Participant is already enrolled
                if (data.already_enrolled(request.eventID, participantID))
                {
                    return Conflict("You are already enrolled in this event.");
                }

                //store enrolment information
                int newID = data.store_enrolment(request.eventID, participantID);

                return Ok(new
                {
                    message = "Enrolment successful.",
                    enrolmentID = newID
                });
            }
            catch (SqlException error) when (error.Number == 547)
            {
                return BadRequest("Invalid event or participant information.");
            }
            catch (Exception error)
            {
                System.Diagnostics.Debug.WriteLine(error.Message);
                return StatusCode(500, "Failed to create enrolment.");
            }
        }
        //end of method

        //start of method to view event enrolments
        //only Organisers can view enrolments for their own events
        [HttpGet("/api/events/{eventID:int}/enrolments")]
        public IActionResult get_event_enrolments(int eventID)
        {
            //temp variable declarations
            int? userID = HttpContext.Session.GetInt32("userID");
            string? role = HttpContext.Session.GetString("userRole");

            //check if user is logged in
            if (userID == null)
            {
                return Unauthorized("Please log in first.");
            }

            //check if user is an Organiser
            if (role != "Organiser")
            {
                return StatusCode(403, "Only Organisers can view event enrolments.");
            }

            //try and catch
            try
            {
                //check if Organiser owns the event
                if (!data.organiser_owns_event(eventID, userID.Value))
                {
                    return NotFound("Event not found or not owned by you.");
                }

                //retrieve enrolments
                return Ok(data.get_event_enrolments(eventID));
            }
            catch (Exception error)
            {
                System.Diagnostics.Debug.WriteLine(error.Message);
                return StatusCode(500, "Failed to retrieve enrolments.");
            }
        }
        //end of method
    }
}
