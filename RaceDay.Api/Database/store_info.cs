

using Microsoft.Data.SqlClient;
using RaceDay.Api.Models;

namespace RaceDay.Api.database
{//start of namespace
    public class store_info
    {//start of class

        //method to store the user's info
        public string store_userinfo(RegisterRequest user, out int newUserID)
        {//start of method

            //temp variable declaration
            string message = "failed to register user";
            string connections = @"Data source=(localdb)\api_both;database=Raceday;";
            string role = user.userRole;

            newUserID = 0;

            //try and catch
            try
            {//start of try

                //creating an instance for the class SqlConnection
                //with an object name connect
                using SqlConnection connect = new SqlConnection(connections);

                //open connection
                connect.Open();

                //query to insert or store the user's info
                string query = @"INSERT INTO Users
                (fullName, emailAddress, phoneNumber, password, userRole)
                OUTPUT INSERTED.userID
                VALUES (@fullName, @email, @phone, @password, @role);";

                //creating an instance for SqlCommand
                //with an object run_query
                SqlCommand run_query = new SqlCommand(query, connect);

                //add the user's information
                run_query.Parameters.AddWithValue("@fullName", user.fullName);
                run_query.Parameters.AddWithValue("@email", user.emailAddress.Trim().ToLowerInvariant());
                run_query.Parameters.AddWithValue("@phone", user.phoneNumber);
                run_query.Parameters.AddWithValue("@password", user.password);
                run_query.Parameters.AddWithValue("@role", role);

                //run query
                newUserID = Convert.ToInt32(run_query.ExecuteScalar());

                //check the user's role
                string role_query = "";

                if (role == "Organiser")
                {//start of if
                    role_query = "INSERT INTO Organisers (userID) VALUES (@userID);";
                }//end of if
                else if (role == "Participant")
                {//start of else if
                    role_query = "INSERT INTO Participants (userID) VALUES (@userID);";
                }//end of else if
                else
                {
                    throw new Exception("Invalid user role");
                }

                //create a command to store the user's role
                SqlCommand run_role = new SqlCommand(role_query, connect);

                run_role.Parameters.AddWithValue("@userID", newUserID);

                //run query
                int results = run_role.ExecuteNonQuery();

                //check if rows are affected
                if (results > 0)
                {//start of if
                    message = "user registered successfully";
                }//end of if
                else
                {//start of else
                    message = "Failed to register user";
                    newUserID = 0;
                }//end of else

                //close connection
                connect.Close();

            }//end of try
            catch (Exception error)
            {//start of catch

                //show the actual error for debugging
                message = error.Message;
                newUserID = 0;

                //display error in Visual Studio Output
                System.Diagnostics.Debug.WriteLine(error.Message);

            }//end of catch

            //return the message variable
            return message;

        }//end of store_userinfo() method

    }//end of class
}//end of namespace
