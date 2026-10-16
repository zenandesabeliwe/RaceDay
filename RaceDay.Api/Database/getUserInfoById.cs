using Microsoft.Data.SqlClient;
using RaceDay.Api.Models;

namespace RaceDay.Api.database;

// This follows your getUserInfoById class pattern, adapted to RaceDay Events.
public class getUserInfoById
{
    private readonly DatabaseConnection database;

    public getUserInfoById(DatabaseConnection database)
    {
        this.database = database;
    }

    public Event? get_event_info_by_Id(int id)
    {
        Event? found_event = null;

        using (SqlConnection connect = database.get_connection())
        {
            connect.Open();

            string query = @"SELECT eventID, eventName, description, eventDate, location,
                                    distance, eventType, organiserID
                             FROM Events WHERE eventID = @eventID;";

            using (SqlCommand run_query = new SqlCommand(query, connect))
            {
                run_query.Parameters.AddWithValue("@eventID", id);

                using (SqlDataReader found_data = run_query.ExecuteReader())
                {
                    if (found_data.Read())
                    {
                        found_event = new Event
                        {
                            eventID = Convert.ToInt32(found_data["eventID"]),
                            eventName = found_data["eventName"].ToString() ?? "",
                            description = found_data["description"].ToString() ?? "",
                            eventDate = Convert.ToDateTime(found_data["eventDate"]),
                            location = found_data["location"].ToString() ?? "",
                            distance = Convert.ToDecimal(found_data["distance"]),
                            eventType = found_data["eventType"].ToString() ?? "",
                            organiserID = Convert.ToInt32(found_data["organiserID"])
                        };
                    }
                }
            }
        }

        return found_event;
    }
}
