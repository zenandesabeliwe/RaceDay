using Microsoft.Data.SqlClient;
using RaceDay.Api.Models;

namespace RaceDay.Api.database;

// Gets a user's profile using their ID.
public class get_info
{
    private readonly DatabaseConnection database;

    public get_info(DatabaseConnection database)
    {
        this.database = database;
    }

    public User? get_user_info(int userID)
    {
        User? found_user = null;

        try
        {
            using (SqlConnection connect = database.get_connection())
            {
                connect.Open();

                string query = @"SELECT userID, fullName, emailAddress, phoneNumber, password, userRole
                                 FROM Users WHERE userID = @userID;";

                using (SqlCommand run_query = new SqlCommand(query, connect))
                {
                    run_query.Parameters.AddWithValue("@userID", userID);

                    using (SqlDataReader found_data = run_query.ExecuteReader())
                    {
                        if (found_data.Read())
                        {
                            found_user = new User
                            {
                                userID = Convert.ToInt32(found_data["userID"]),
                                fullName = found_data["fullName"].ToString() ?? "",
                                emailAddress = found_data["emailAddress"].ToString() ?? "",
                                phoneNumber = found_data["phoneNumber"].ToString() ?? "",
                                password = "", // Never return a password hash to the client.
                                userRole = found_data["userRole"].ToString() ?? ""
                            };
                        }
                    }
                }
            }
        }
        catch (Exception error)
        {
            throw new Exception("Could not retrieve user profile: " + error.Message, error);
        }

        return found_user;
    }
}
