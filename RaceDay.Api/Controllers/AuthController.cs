
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using RaceDay.Api.database;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers
{//start of namespace

    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {//start of class

        //method to register a participant or organiser
        [HttpPost("register")]
        public IActionResult register_user(RegisterRequest user)
        {//start of method

            //temp variable declaration
            string message = "failed to register user";

            //check if all information is entered
            if (user == null ||
                string.IsNullOrWhiteSpace(user.fullName) ||
                string.IsNullOrWhiteSpace(user.emailAddress) ||
                string.IsNullOrWhiteSpace(user.phoneNumber) ||
                string.IsNullOrWhiteSpace(user.password))
            {
                return BadRequest("Please complete all required fields.");
            }

            //check password length
            if (user.password.Length < 8)
            {
                return BadRequest("Password must contain at least 8 characters.");
            }

            //check the user's role
            if (user.userRole != "Organiser" &&
                user.userRole != "Participant")
            {
                return BadRequest("Role must be Organiser or Participant.");
            }

            //hash password before storing it
            user.password = BCrypt.Net.BCrypt.HashPassword(user.password);

            //creating an instance for the class store_info
            //with an object store
            store_info store = new store_info();

            //pass information to store_info
            message = store.store_userinfo(user, out int newUserID);

            //check if registration was successful
            if (newUserID > 0)
            {
                return Ok(new
                {
                    message = message,
                    userID = newUserID,
                    role = user.userRole
                });
            }
            else
            {
                return BadRequest(message);
            }

        }//end of register_user() method


        //method to login using email and password
        [HttpPost("login")]
        public IActionResult login_user(LoginRequest user)
        {//start of method

            //temp variable declaration
            string message = "failed to login";
            string connections = @"Data source=(localdb)\api_both;database=Raceday;";

            //check if email and password are entered
            if (user == null ||
                string.IsNullOrWhiteSpace(user.emailAddress) ||
                string.IsNullOrWhiteSpace(user.password))
            {
                return BadRequest("Please enter email and password.");
            }

            //try and catch
            try
            {//start of try

                //creating an instance for SqlConnection
                //with an object connect
                using (SqlConnection connect = new SqlConnection(connections))
                {
                    //open connection
                    connect.Open();

                    //query to find the registered user
                    string query = @"SELECT userID, fullName,
                                    emailAddress, password, userRole
                                    FROM Users
                                    WHERE emailAddress = @email;";

                    //creating an instance for SqlCommand
                    SqlCommand run_query = new SqlCommand(query, connect);

                    //add email parameter
                    run_query.Parameters.AddWithValue(
                        "@email", user.emailAddress.Trim().ToLowerInvariant());

                    //read information from database
                    using (SqlDataReader reader = run_query.ExecuteReader())
                    {
                        //check if user exists
                        if (reader.Read())
                        {//start of if

                            //get the hashed password from the table
                            string password_fromTable =
                                reader["password"].ToString() ?? "";

                            //verify the entered password
                            Boolean verify_password =
                                BCrypt.Net.BCrypt.Verify(
                                    user.password, password_fromTable);

                            //check password results
                            if (verify_password)
                            {//start of if

                                //get user information
                                int userID = Convert.ToInt32(reader["userID"]);

                                string fullName =
                                    reader["fullName"].ToString() ?? "";

                                string role =
                                    reader["userRole"].ToString() ?? "";

                                //store user information in session
                                HttpContext.Session.SetInt32("userID", userID);
                                HttpContext.Session.SetString("userRole", role);

                                HttpContext.Session.SetString(
                                    "emailAddress",
                                    user.emailAddress.Trim().ToLowerInvariant());

                                message = "login successful";

                                return Ok(new
                                {
                                    message = message,
                                    userID = userID,
                                    fullName = fullName,
                                    userRole = role
                                });

                            }//end of if
                            else
                            {
                                message = "Incorrect email or password.";
                            }
                        }//end of if
                        else
                        {
                            message = "Incorrect email or password.";
                        }
                    }

                    //close connection
                    connect.Close();
                }

                return Unauthorized(message);

            }//end of try
            catch (Exception error)
            {//start of catch

                //display error in Visual Studio Output
                System.Diagnostics.Debug.WriteLine(error.Message);

                message = "Login failed. Please try again.";
                return StatusCode(500, message);

            }//end of catch

        }//end of login_user() method


        //method to logout the user
        [HttpPost("logout")]
        public IActionResult logout_user()
        {//start of method

            //temp variable declaration
            string message = "failed to logout";

            //check if user is logged in
            if (HttpContext.Session.GetInt32("userID") == null)
            {
                message = "Please login first.";
                return Unauthorized(message);
            }

            //clear the user's session
            HttpContext.Session.Clear();

            message = "user logged out successfully";

            return Ok(message);

        }//end of logout_user() method

    }//end of class

}//end of namespace
