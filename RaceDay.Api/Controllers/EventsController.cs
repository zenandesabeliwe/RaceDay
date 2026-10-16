
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using RaceDay.Api.database;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventsController : ControllerBase
    {
        //database declarations
        private readonly DatabaseConnection database;
        private readonly RaceDayData data;

        //constructor
        public EventsController(IConfiguration configuration)
        {
            database = new DatabaseConnection(configuration);
            data = new RaceDayData(database);
        }

        //method to view all events
        [HttpGet]
        public IActionResult get_all_events()
        {
            //check if user is logged in
            int? userID = HttpContext.Session.GetInt32("userID");

            if (userID == null)
            {
                return Unauthorized("Please log in first.");
            }

            try
            {
                return Ok(data.get_all_events());
            }
            catch (Exception error)
            {
                System.Diagnostics.Debug.WriteLine(error.Message);
                return StatusCode(500, "Failed to retrieve events.");
            }
        }

        //method to view an event by ID
        [HttpGet("{id:int}")]
        public IActionResult get_event_by_id(int id)
        {
            //check if user is logged in
            int? userID = HttpContext.Session.GetInt32("userID");

            if (userID == null)
            {
                return Unauthorized("Please log in first.");
            }

            try
            {
                getUserInfoById get_data = new getUserInfoById(database);

                Event? foundEvent = get_data.get_event_info_by_Id(id);

                if (foundEvent == null)
                {
                    return NotFound("Event was not found.");
                }

                return Ok(foundEvent);
            }
            catch (Exception error)
            {
                System.Diagnostics.Debug.WriteLine(error.Message);
                return StatusCode(500, "Failed to retrieve event.");
            }
        }

        //method to create an event
        //only organisers can create events
        [HttpPost]
        public IActionResult create_event(EventRequest request)
        {
            //temp variable declarations
            int? userID = HttpContext.Session.GetInt32("userID");
            string? role = HttpContext.Session.GetString("userRole");

            //check if user is logged in
            if (userID == null)
            {
                return Unauthorized("Please log in first.");
            }

            //check if user is an organiser
            if (role != "Organiser")
            {
                return StatusCode(403, "Only Organisers can create events.");
            }

            //validate event information
            if (!valid_event_details(request))
            {
                return BadRequest("Please enter valid event details. Distance must be greater than zero and no more than 999.99.");
            }

            //validate event type
            if (!valid_event_type(request.eventType))
            {
                return BadRequest("Event type must be Run, Walk, Cycle, Marathon, Trail Run, or Fun Run.");
            }

            //try and catch
            try
            {
                //get organiser ID
                int organiserID = data.get_organiser_id(userID.Value);

                if (organiserID == 0)
                {
                    return StatusCode(403, "Organiser profile was not found.");
                }

                //store event information
                store_event_info store = new store_event_info(database);

                int newID = store.store_event(request, organiserID);

                if (newID <= 0)
                {
                    return StatusCode(500, "Failed to create event.");
                }

                return Ok(new
                {
                    message = "Event created successfully.",
                    eventID = newID
                });
            }
            catch (Exception error)
            {
                System.Diagnostics.Debug.WriteLine(error.Message);
                return StatusCode(500, "Failed to create event.");
            }
        }

        //method to update an event
        //only the organiser who owns the event can update it
        [HttpPut("{id:int}")]
        public IActionResult update_event(int id, EventRequest request)
        {
            //temp variable declarations
            int? userID = HttpContext.Session.GetInt32("userID");
            string? role = HttpContext.Session.GetString("userRole");

            //check if user is logged in
            if (userID == null)
            {
                return Unauthorized("Please log in first.");
            }

            //check if user is an organiser
            if (role != "Organiser")
            {
                return StatusCode(403, "Only Organisers can update events.");
            }

            //validate event information
            if (!valid_event_details(request))
            {
                return BadRequest("Please enter valid event details. Distance must be greater than zero and no more than 999.99.");
            }

            //validate event type
            if (!valid_event_type(request.eventType))
            {
                return BadRequest("Event type must be Run, Walk, Cycle, Marathon, Trail Run, or Fun Run.");
            }

            //try and catch
            try
            {
                //check if organiser owns the event
                if (!data.organiser_owns_event(id, userID.Value))
                {
                    return NotFound("Event not found or not owned by you.");
                }

                //connect to database
                using SqlConnection connect = database.get_connection();
                connect.Open();

                //SQL update query
                string query = @"UPDATE Events
                                 SET eventName = @name,
                                     description = @description,
                                     eventDate = @date,
                                     location = @location,
                                     distance = @distance,
                                     eventType = @type
                                 WHERE eventID = @id
                                 AND organiserID IN
                                 (
                                     SELECT organiserID
                                     FROM Organisers
                                     WHERE userID = @userID
                                 );";

                SqlCommand run_query = new SqlCommand(query, connect);

                //SQL parameters
                run_query.Parameters.AddWithValue("@name", request.eventName.Trim());
                run_query.Parameters.AddWithValue("@description", request.description.Trim());
                run_query.Parameters.AddWithValue("@date", request.eventDate.Date);
                run_query.Parameters.AddWithValue("@location", request.location.Trim());
                run_query.Parameters.AddWithValue("@distance", request.distance);
                run_query.Parameters.AddWithValue("@type", request.eventType.Trim());
                run_query.Parameters.AddWithValue("@id", id);
                run_query.Parameters.AddWithValue("@userID", userID.Value);

                //execute query
                int results = run_query.ExecuteNonQuery();

                if (results > 0)
                {
                    return Ok("Event updated successfully.");
                }
                else
                {
                    return NotFound("Event not found or not owned by you.");
                }
            }
            catch (Exception error)
            {
                System.Diagnostics.Debug.WriteLine(error.Message);
                return StatusCode(500, "Failed to update event.");
            }
        }

        //method to delete an event
        //only the organiser who owns the event can delete it
        [HttpDelete("{id:int}")]
        public IActionResult delete_event(int id)
        {
            //temp variable declarations
            int? userID = HttpContext.Session.GetInt32("userID");
            string? role = HttpContext.Session.GetString("userRole");

            //check if user is logged in
            if (userID == null)
            {
                return Unauthorized("Please log in first.");
            }

            //check if user is an organiser
            if (role != "Organiser")
            {
                return StatusCode(403, "Only Organisers can delete events.");
            }

            //try and catch
            try
            {
                //check if organiser owns the event
                if (!data.organiser_owns_event(id, userID.Value))
                {
                    return NotFound("Event not found or not owned by you.");
                }

                //connect to database
                using SqlConnection connect = database.get_connection();
                connect.Open();

                //check if event has enrolments
                string enrolment_query =
                    "SELECT COUNT(*) FROM Enrolments WHERE eventID = @id;";

                SqlCommand check_enrolments =
                    new SqlCommand(enrolment_query, connect);

                check_enrolments.Parameters.AddWithValue("@id", id);

                int enrolments = Convert.ToInt32(check_enrolments.ExecuteScalar());

                if (enrolments > 0)
                {
                    return Conflict("Cannot delete an event that has enrolments.");
                }

                //check if event has categories
                string category_query =
                    "SELECT COUNT(*) FROM Categories WHERE eventID = @id;";

                SqlCommand check_categories =
                    new SqlCommand(category_query, connect);

                check_categories.Parameters.AddWithValue("@id", id);

                int categories = Convert.ToInt32(check_categories.ExecuteScalar());

                if (categories > 0)
                {
                    return Conflict("Cannot delete an event that has categories.");
                }

                //SQL delete query
                string query = @"DELETE FROM Events
                                 WHERE eventID = @id
                                 AND organiserID IN
                                 (
                                     SELECT organiserID
                                     FROM Organisers
                                     WHERE userID = @userID
                                 );";

                SqlCommand run_query = new SqlCommand(query, connect);

                run_query.Parameters.AddWithValue("@id", id);
                run_query.Parameters.AddWithValue("@userID", userID.Value);

                //execute query
                int results = run_query.ExecuteNonQuery();

                if (results > 0)
                {
                    return NoContent();
                }
                else
                {
                    return NotFound("Event not found or not owned by you.");
                }
            }
            catch (SqlException error) when (error.Number == 547)
            {
                return Conflict("Cannot delete an event that has related records.");
            }
            catch (Exception error)
            {
                System.Diagnostics.Debug.WriteLine(error.Message);
                return StatusCode(500, "Failed to delete event.");
            }
        }

        //method to validate event details
        private static bool valid_event_details(EventRequest request)
        {
            if (request == null)
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(request.eventName) ||
                string.IsNullOrWhiteSpace(request.description) ||
                string.IsNullOrWhiteSpace(request.location))
            {
                return false;
            }

            if (request.distance <= 0 || request.distance > 999.99m)
            {
                return false;
            }

            return true;
        }

        //method to validate event type
        private static bool valid_event_type(string type)
        {
            if (string.IsNullOrWhiteSpace(type))
            {
                return false;
            }

            string eventType = type.Trim().ToLowerInvariant();

            return eventType == "run" ||
                   eventType == "walk" ||
                   eventType == "cycle" ||
                   eventType == "marathon" ||
                   eventType == "trail run" ||
                   eventType == "fun run";
        }
    }
}
