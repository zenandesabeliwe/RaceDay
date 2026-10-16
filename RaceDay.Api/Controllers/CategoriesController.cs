
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using RaceDay.Api.database;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers
{
    [Route("api")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        //database declarations
        private readonly DatabaseConnection database;
        private readonly RaceDayData data;

        //constructor
        public CategoriesController(IConfiguration configuration)
        {
            database = new DatabaseConnection(configuration);
            data = new RaceDayData(database);
        }

        //method to view categories for an event
        //both Organisers and Participants can view categories
        [HttpGet("events/{eventID:int}/categories")]
        public IActionResult get_categories(int eventID)
        {
            //check if user is logged in
            int? userID = HttpContext.Session.GetInt32("userID");

            if (userID == null)
            {
                return Unauthorized("Please log in first.");
            }

            //try and catch
            try
            {
                return Ok(data.get_categories(eventID));
            }
            catch (Exception error)
            {
                System.Diagnostics.Debug.WriteLine(error.Message);
                return StatusCode(500, "Failed to retrieve categories.");
            }
        }

        //method to create a category
        //only Organisers can create categories for their own events
        [HttpPost("events/{eventID:int}/categories")]
        public IActionResult create_category(int eventID, CategoryRequest request)
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
                return StatusCode(403, "Only Organisers can create categories.");
            }

            //validate category information
            if (!valid_category_details(request))
            {
                return BadRequest("Please enter valid category details.");
            }

            //try and catch
            try
            {
                //check if Organiser owns the event
                if (!data.organiser_owns_event(eventID, userID.Value))
                {
                    return NotFound("Event not found or not owned by you.");
                }

                //get Organiser ID
                int organiserID = data.get_organiser_id(userID.Value);

                if (organiserID == 0)
                {
                    return StatusCode(403, "Organiser profile was not found.");
                }

                //store category information
                int newID = data.store_category(request, eventID, organiserID);

                if (newID <= 0)
                {
                    return StatusCode(500, "Failed to create category.");
                }

                return Ok(new
                {
                    message = "Category created successfully.",
                    categoryID = newID
                });
            }
            catch (Exception error)
            {
                System.Diagnostics.Debug.WriteLine(error.Message);
                return StatusCode(500, "Failed to create category.");
            }
        }

        //method to update a category
        //only the Organiser who owns the event can update it
        [HttpPut("categories/{id:int}")]
        public IActionResult update_category(int id, CategoryRequest request)
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
                return StatusCode(403, "Only Organisers can update categories.");
            }

            //validate category information
            if (!valid_category_details(request))
            {
                return BadRequest("Please enter valid category details.");
            }

            //try and catch
            try
            {
                //connect to database
                using SqlConnection connect = database.get_connection();
                connect.Open();

                //SQL update query
                //check ownership through the Events table
                string query = @"UPDATE Categories
                                 SET categoryName = @name,
                                     distance = @distance,
                                     age = @age,
                                     startTime = @start
                                 WHERE categoryID = @id
                                 AND eventID IN
                                 (
                                     SELECT e.eventID
                                     FROM Events e
                                     INNER JOIN Organisers o
                                     ON e.organiserID = o.organiserID
                                     WHERE o.userID = @userID
                                 );";

                SqlCommand run_query = new SqlCommand(query, connect);

                //SQL parameters
                run_query.Parameters.AddWithValue("@name", request.categoryName.Trim());
                run_query.Parameters.AddWithValue("@distance", request.distance);
                run_query.Parameters.AddWithValue("@age", request.age);
                run_query.Parameters.AddWithValue("@start", request.startTime);
                run_query.Parameters.AddWithValue("@id", id);
                run_query.Parameters.AddWithValue("@userID", userID.Value);

                //execute query
                int results = run_query.ExecuteNonQuery();

                if (results > 0)
                {
                    return Ok("Category updated successfully.");
                }
                else
                {
                    return NotFound("Category not found or not owned by you.");
                }
            }
            catch (Exception error)
            {
                System.Diagnostics.Debug.WriteLine(error.Message);
                return StatusCode(500, "Failed to update category.");
            }
        }

        //method to delete a category
        //only the Organiser who owns the event can delete it
        [HttpDelete("categories/{id:int}")]
        public IActionResult delete_category(int id)
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
                return StatusCode(403, "Only Organisers can delete categories.");
            }

            //try and catch
            try
            {
                //connect to database
                using SqlConnection connect = database.get_connection();
                connect.Open();

                //SQL delete query
                //check ownership through the Events table
                string query = @"DELETE FROM Categories
                                 WHERE categoryID = @id
                                 AND eventID IN
                                 (
                                     SELECT e.eventID
                                     FROM Events e
                                     INNER JOIN Organisers o
                                     ON e.organiserID = o.organiserID
                                     WHERE o.userID = @userID
                                 );";

                SqlCommand run_query = new SqlCommand(query, connect);

                //SQL parameters
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
                    return NotFound("Category not found or not owned by you.");
                }
            }
            catch (SqlException error) when (error.Number == 547)
            {
                return Conflict("Cannot delete category because it has related records.");
            }
            catch (Exception error)
            {
                System.Diagnostics.Debug.WriteLine(error.Message);
                return StatusCode(500, "Failed to delete category.");
            }
        }

        //method to validate category information
        private static bool valid_category_details(CategoryRequest request)
        {
            if (request == null)
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(request.categoryName))
            {
                return false;
            }

            if (request.distance <= 0 || request.distance > 999.99m)
            {
                return false;
            }

            if (request.age < 0)
            {
                return false;
            }

            return true;
        }
    }
}
