
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using RaceDay.Api.database;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers
{//start of namespace

    [Route("api/[controller]")]
    [ApiController]

    public class UsersController : ControllerBase
    {//start of class

        //global object of database connection
        private readonly DatabaseConnection database;

        //constructor to connect to database
        public UsersController(IConfiguration configuration)
        {
            database = new DatabaseConnection(configuration);
        }

        //GET method to display the logged-in user's profile
        [HttpGet("profile")]
        public IActionResult get_user_profile()
        {//start of method

            //temp variable declaration
            int? userID = HttpContext.Session.GetInt32("userID");

            //check if user is logged in
            if (userID == null)
            {
                return Unauthorized("Please log in first.");
            }

            try
            {//start of try

                //create object to get user information
                get_info get_data = new get_info(database);

                //get user information
                User? user = get_data.get_user_info(userID.Value);

                //check if user exists
                if (user == null)
                {
                    return NotFound("User was not found.");
                }

                //return user profile without password
                return Ok(new
                {
                    user.userID,
                    user.fullName,
                    user.emailAddress,
                    user.phoneNumber,
                    user.userRole
                });

            }//end of try

            catch (Exception error)
            {
                System.Diagnostics.Debug.WriteLine(error.Message);
                return StatusCode(500, "Failed to retrieve user profile.");
            }

        }//end of method


        //PUT method to update the logged-in user's profile
        [HttpPut("profile")]
        public IActionResult update_user_profile(ProfileRequest request)
        {//start of method

            //temp variable declaration
            int? userID = HttpContext.Session.GetInt32("userID");

            //check if user is logged in
            if (userID == null)
            {
                return Unauthorized("Please log in first.");
            }

            //check if request contains information
            if (request == null)
            {
                return BadRequest("Please provide profile information.");
            }

            //check if full name is empty
            if (string.IsNullOrWhiteSpace(request.fullName))
            {
                return BadRequest("Full name is required.");
            }

            //check if phone number is empty
            if (string.IsNullOrWhiteSpace(request.phoneNumber))
            {
                return BadRequest("Phone number is required.");
            }

            try
            {//start of try

                //connect to database
                using (SqlConnection connect = database.get_connection())
                {
                    connect.Open();

                    //SQL query to update user information
                    string query = @"UPDATE Users
                                     SET fullName = @fullName,
                                         phoneNumber = @phoneNumber
                                     WHERE userID = @userID;";

                    //create SQL command
                    SqlCommand run_query = new SqlCommand(query, connect);

                    //add SQL parameters
                    run_query.Parameters.AddWithValue("@fullName", request.fullName.Trim());

                    run_query.Parameters.AddWithValue("@phoneNumber", request.phoneNumber.Trim());

                    run_query.Parameters.AddWithValue("@userID", userID.Value);

                    //execute SQL query
                    int changed = run_query.ExecuteNonQuery();

                    //check if information was updated
                    if (changed > 0)
                    {
                        return Ok("Profile updated successfully.");
                    }
                    else
                    {
                        return NotFound("User was not found.");
                    }

                }//end of using

            }//end of try

            catch (Exception error)
            {
                System.Diagnostics.Debug.WriteLine(error.Message);
                return StatusCode(500, "Failed to update user profile.");
            }

        }//end of method

    }//end of class

}//end of namespace
