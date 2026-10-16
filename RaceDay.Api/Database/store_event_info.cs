using Microsoft.Data.SqlClient;
using RaceDay.Api.Models;

namespace RaceDay.Api.database;

public class store_event_info
{
    private readonly DatabaseConnection database;

    public store_event_info(DatabaseConnection database)
    {
        this.database = database;
    }

    public int store_event(EventRequest eventInfo, int organiserID)
    {
        using (SqlConnection connect = database.get_connection())
        {
            connect.Open();

            string query = @"INSERT INTO Events
                (eventName, description, eventDate, location, distance, eventType, organiserID)
                OUTPUT INSERTED.eventID
                VALUES (@eventName, @description, @eventDate, @location, @distance, @eventType, @organiserID);";

            using (SqlCommand command = new SqlCommand(query, connect))
            {
                command.Parameters.AddWithValue("@eventName", eventInfo.eventName.Trim());
                command.Parameters.AddWithValue("@description", eventInfo.description.Trim());
                command.Parameters.AddWithValue("@eventDate", eventInfo.eventDate.Date);
                command.Parameters.AddWithValue("@location", eventInfo.location.Trim());
                command.Parameters.AddWithValue("@distance", eventInfo.distance);
                command.Parameters.AddWithValue("@eventType", eventInfo.eventType.Trim());
                command.Parameters.AddWithValue("@organiserID", organiserID);
                return Convert.ToInt32(command.ExecuteScalar());
            }
        }
    }
}
