
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using RaceDay.Api.database;

namespace RaceDay.Api.Controllers
{//start of namespace

    [Route("api/[controller]")]
    [ApiController]

    public class ParticipantsController : ControllerBase
    {//start of class

        //global object of database connection
        private readonly DatabaseConnection database;

        //constructor to connect to database
        public ParticipantsController(IConfiguration configuration)
        {
            database = new DatabaseConnection(configuration);
        }

        //GET method to display all participants
        //only Organisers can view the participant list
        [HttpGet]
        public IActionResult get_participants()
        {//start of method

            //temp variable declaration
            int? userID = HttpContext.Session.GetInt32("userID");
            string? role = HttpContext.Session.GetString("userRole");

            //check if user is logged in
            if (userID == null)
            {
                return Unauthorized("Please log in first.");
            }

            //check user role
            if (role != "Organiser")
            {
                return StatusCode(403, "Only Organisers can view participants.");
            }

            try
            {//start of try

                //list to store participant information
                List<object> participants = new List<object>();

                //connect to database
                using (SqlConnection connect = database.get_connection())
                {
                    connect.Open();

                    //SQL query
                    string query = @"SELECT p.participantID,
                                            p.eventID,
                                            p.userID,
                                            u.fullName
                                     FROM Participants p
                                     INNER JOIN Users u
                                     ON p.userID = u.userID;";

                    //execute SQL query
                    SqlCommand run_query = new SqlCommand(query, connect);

                    //read information
                    SqlDataReader found_data = run_query.ExecuteReader();

                    while (found_data.Read())
                    {
                        //add participant to list
                        participants.Add(new
                        {
                            participantID = Convert.ToInt32(found_data["participantID"]),

                            eventID = found_data["eventID"] == DBNull.Value
                                ? (int?)null
                                : Convert.ToInt32(found_data["eventID"]),

                            userID = Convert.ToInt32(found_data["userID"]),

                            fullName = found_data["fullName"].ToString()
                        });
                    }

                    found_data.Close();

                }//end of using

                //return participant list
                return Ok(participants);

            }//end of try

            catch (Exception error)
            {
                System.Diagnostics.Debug.WriteLine(error.Message);
                return StatusCode(500, "Failed to retrieve participants.");
            }

        }//end of method


        //GET method to search for participant by ID
        //Organisers can view a participant
        //Participants can view only their own information
        [HttpGet("{id:int}")]
        public IActionResult get_participant_by_id(int id)
        {//start of method

            //temp variable declaration
            int? userID = HttpContext.Session.GetInt32("userID");
            string? role = HttpContext.Session.GetString("userRole");

            //check if user is logged in
            if (userID == null)
            {
                return Unauthorized("Please log in first.");
            }

            try
            {//start of try

                //connect to database
                using (SqlConnection connect = database.get_connection())
                {
                    connect.Open();

                    //SQL query
                    string query = @"SELECT p.participantID,
                                            p.eventID,
                                            p.userID,
                                            u.fullName
                                     FROM Participants p
                                     INNER JOIN Users u
                                     ON p.userID = u.userID
                                     WHERE p.participantID = @id;";

                    //create SQL command
                    SqlCommand run_query = new SqlCommand(query, connect);

                    //add parameter
                    run_query.Parameters.AddWithValue("@id", id);

                    //execute query
                    SqlDataReader found_data = run_query.ExecuteReader();

                    if (found_data.Read())
                    {
                        //temp variable declarations
                        int participantUserID = Convert.ToInt32(found_data["userID"]);

                        //check if Participant is viewing their own record
                        if (role == "Participant" && participantUserID != userID.Value)
                        {
                            return StatusCode(403, "You can only view your own participant information.");
                        }

                        //check user role
                        if (role != "Organiser" && role != "Participant")
                        {
                            return StatusCode(403, "Access denied.");
                        }

                        //return participant information
                        return Ok(new
                        {
                            participantID = Convert.ToInt32(found_data["participantID"]),

                            eventID = found_data["eventID"] == DBNull.Value
                                ? (int?)null
                                : Convert.ToInt32(found_data["eventID"]),

                            userID = participantUserID,

                            fullName = found_data["fullName"].ToString()
                        });
                    }

                    //participant not found
                    return NotFound("Participant not found.");

                }//end of using

            }//end of try

            catch (Exception error)
            {
                System.Diagnostics.Debug.WriteLine(error.Message);
                return StatusCode(500, "Failed to retrieve participant.");
            }

        }//end of method

    }//end of class

}//end of namespace
