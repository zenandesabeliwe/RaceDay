
using Microsoft.AspNetCore.Mvc;
using RaceDay.Api.database;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers
{
    [Route("api")]
    [ApiController]
    public class ResultsController : ControllerBase
    {
        //database declarations
        private readonly DatabaseConnection database;
        private readonly RaceDayData data;

        //constructor
        public ResultsController(IConfiguration configuration)
        {
            database = new DatabaseConnection(configuration);
            data = new RaceDayData(database);
        }

        //start of method to record race results
        [HttpPost("enrolments/{enrolmentID:int}/results")]
        public IActionResult create_result(int enrolmentID, ResultRequest request)
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
                return StatusCode(403, "Only Organisers can record results.");
            }

            //validate result information
            if (request == null || enrolmentID <= 0)
            {
                return BadRequest("Please enter valid result information.");
            }

            if (request.finishPosition < 1)
            {
                return BadRequest("Finish position must be 1 or greater.");
            }

            if (request.finishTime < TimeSpan.Zero ||
                request.finishTime >= TimeSpan.FromDays(1))
            {
                return BadRequest("Finish time must be valid.");
            }

            //try and catch
            try
            {
                //check if Organiser owns the event
                bool ownsEvent = data.enrolment_belongs_to_organiser(
                    enrolmentID, userID.Value);

                if (!ownsEvent)
                {
                    return NotFound("Enrolment not found or not for your event.");
                }

                //check if result already exists
                if (data.result_exists(enrolmentID))
                {
                    return Conflict("A result already exists for this enrolment.");
                }

                //save result
                data.store_result(enrolmentID, request);

                return Ok("Result recorded successfully.");
            }
            catch (Exception error)
            {
                System.Diagnostics.Debug.WriteLine(error.Message);
                return StatusCode(500, "Failed to record result.");
            }
        }
        //end of method

        //start of method for Participant to view own results
        [HttpGet("results/me")]
        public IActionResult get_my_results()
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
                return StatusCode(403, "Only Participants can view their personal results.");
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

                //retrieve Participant results
                return Ok(data.get_my_results(participantID));
            }
            catch (Exception error)
            {
                System.Diagnostics.Debug.WriteLine(error.Message);
                return StatusCode(500, "Failed to retrieve results.");
            }
        }
        //end of method

        //start of method for Organiser to view event results
        [HttpGet("events/{eventID:int}/results")]
        public IActionResult get_event_results(int eventID)
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
                return StatusCode(403, "Only Organisers can view event results.");
            }

            //try and catch
            try
            {
                //check if Organiser owns the event
                if (!data.organiser_owns_event(eventID, userID.Value))
                {
                    return NotFound("Event not found or not owned by you.");
                }

                //retrieve event results
                return Ok(data.get_event_results(eventID));
            }
            catch (Exception error)
            {
                System.Diagnostics.Debug.WriteLine(error.Message);
                return StatusCode(500, "Failed to retrieve event results.");
            }
        }
        //end of method
    }
}
