using Microsoft.Data.SqlClient;
using RaceDay.Api.Models;

namespace RaceDay.Api.database;


public class RaceDayData
{
    private readonly DatabaseConnection database;
    public RaceDayData(DatabaseConnection database) { this.database = database; }

    public List<Event> get_all_events()
    {
        List<Event> events = new();

        using (SqlConnection connect = database.get_connection())
        {
            connect.Open();
            string query = @"SELECT eventID, eventName, description, eventDate, location,
                                    distance, eventType, organiserID
                             FROM Events ORDER BY eventDate;";
            using (SqlCommand command = new SqlCommand(query, connect))
            using (SqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    events.Add(new Event
                    {
                        eventID = Convert.ToInt32(reader["eventID"]),
                        eventName = reader["eventName"].ToString() ?? "",
                        description = reader["description"].ToString() ?? "",
                        eventDate = Convert.ToDateTime(reader["eventDate"]),
                        location = reader["location"].ToString() ?? "",
                        distance = Convert.ToDecimal(reader["distance"]),
                        eventType = reader["eventType"].ToString() ?? "",
                        organiserID = Convert.ToInt32(reader["organiserID"])
                    });
                }
            }
        }
        return events;
    }

    public int get_organiser_id(int userID)
    {
        using SqlConnection connect = database.get_connection();
        connect.Open();
        using SqlCommand command = new SqlCommand("SELECT organiserID FROM Organisers WHERE userID=@userID;", connect);
        command.Parameters.AddWithValue("@userID", userID);
        object? result = command.ExecuteScalar();
        return result == null ? 0 : Convert.ToInt32(result);
    }

    public int get_participant_id(int userID)
    {
        using SqlConnection connect = database.get_connection();
        connect.Open();
        using SqlCommand command = new SqlCommand("SELECT participantID FROM Participants WHERE userID=@userID;", connect);
        command.Parameters.AddWithValue("@userID", userID);
        object? result = command.ExecuteScalar();
        return result == null ? 0 : Convert.ToInt32(result);
    }

    public bool organiser_owns_event(int eventID, int userID)
    {
        using SqlConnection connect = database.get_connection();
        connect.Open();
        string query = @"SELECT COUNT(*) FROM Events e
                         INNER JOIN Organisers o ON e.organiserID=o.organiserID
                         WHERE e.eventID=@eventID AND o.userID=@userID;";
        using SqlCommand command = new SqlCommand(query, connect);
        command.Parameters.AddWithValue("@eventID", eventID);
        command.Parameters.AddWithValue("@userID", userID);
        return Convert.ToInt32(command.ExecuteScalar()) > 0;
    }

    public List<Category> get_categories(int eventID)
    {
        List<Category> categories = new();
        using SqlConnection connect = database.get_connection();
        connect.Open();
        string query = @"SELECT categoryID, categoryName, distance, age, startTime, eventID, organiserID
                         FROM Categories WHERE eventID=@eventID ORDER BY startTime;";
        using SqlCommand command = new SqlCommand(query, connect)
        {
            Parameters = { new SqlParameter("@eventID", eventID) }
        };
        using SqlDataReader reader = command.ExecuteReader();
        while (reader.Read())
        {
            categories.Add(new Category
            {
                categoryID = Convert.ToInt32(reader["categoryID"]),
                categoryName = reader["categoryName"].ToString() ?? "",
                distance = Convert.ToDecimal(reader["distance"]),
                age = Convert.ToInt32(reader["age"]),
                startTime = (TimeSpan)reader["startTime"],
                eventID = Convert.ToInt32(reader["eventID"]),
                organiserID = Convert.ToInt32(reader["organiserID"])
            });
        }
        return categories;
    }

    public int store_category(CategoryRequest category, int eventID, int organiserID)
    {
        using SqlConnection connect = database.get_connection();
        connect.Open();
        string query = @"INSERT INTO Categories (categoryName, distance, age, startTime, eventID, organiserID)
                         OUTPUT INSERTED.categoryID
                         VALUES (@categoryName, @distance, @age, @startTime, @eventID, @organiserID);";
        using SqlCommand command = new SqlCommand(query, connect);
        command.Parameters.AddWithValue("@categoryName", category.categoryName.Trim());
        command.Parameters.AddWithValue("@distance", category.distance);
        command.Parameters.AddWithValue("@age", category.age);
        command.Parameters.AddWithValue("@startTime", category.startTime);
        command.Parameters.AddWithValue("@eventID", eventID);
        command.Parameters.AddWithValue("@organiserID", organiserID);
        return Convert.ToInt32(command.ExecuteScalar());
    }

    public bool category_matches_event(int categoryID, int eventID)
    {
        using SqlConnection connect = database.get_connection();
        connect.Open();
        using SqlCommand command = new SqlCommand("SELECT COUNT(*) FROM Categories WHERE categoryID=@categoryID AND eventID=@eventID;", connect);
        command.Parameters.AddWithValue("@categoryID", categoryID);
        command.Parameters.AddWithValue("@eventID", eventID);
        return Convert.ToInt32(command.ExecuteScalar()) > 0;
    }

    public bool already_enrolled(int eventID, int participantID)
    {
        using SqlConnection connect = database.get_connection();
        connect.Open();
        using SqlCommand command = new SqlCommand("SELECT COUNT(*) FROM Enrolments WHERE eventID=@eventID AND participantID=@participantID;", connect);
        command.Parameters.AddWithValue("@eventID", eventID);
        command.Parameters.AddWithValue("@participantID", participantID);
        return Convert.ToInt32(command.ExecuteScalar()) > 0;
    }


    public int store_enrolment(int eventID, int participantID)
    {
        //temp variable declaration
        int enrolmentID = 0;

        //connect to database
        using SqlConnection connect = database.get_connection();
        connect.Open();

        //SQL query to store enrolment information
        string query = @"INSERT INTO Enrolments
                     (enrolmentDate, eventID, participantID)
                     OUTPUT INSERTED.enrolmentID
                     VALUES (@date, @eventID, @participantID);";

        //execute SQL query
        using SqlCommand command = new SqlCommand(query, connect);

        //SQL parameters
        command.Parameters.AddWithValue("@date", DateTime.Today);
        command.Parameters.AddWithValue("@eventID", eventID);
        command.Parameters.AddWithValue("@participantID", participantID);

        //get newly created enrolment ID
        enrolmentID = Convert.ToInt32(command.ExecuteScalar());

        //return enrolment ID
        return enrolmentID;
    }



    public List<object> get_event_enrolments(int eventID)
    {
        //temp variable declaration
        List<object> enrolments = new List<object>();

        //connect to the database
        using SqlConnection connect = database.get_connection();

        //open connection
        connect.Open();

        //SQL query
        string query = @"SELECT en.enrolmentID,
                            en.enrolmentDate,
                            en.eventID,
                            en.participantID,
                            u.fullName
                     FROM Enrolments en
                     INNER JOIN Participants p
                     ON en.participantID = p.participantID
                     INNER JOIN Users u
                     ON p.userID = u.userID
                     WHERE en.eventID = @eventID";

        //create SQL command
        using SqlCommand command = new SqlCommand(query, connect);

        //add parameter
        command.Parameters.AddWithValue("@eventID", eventID);

        //execute SQL query
        using SqlDataReader reader = command.ExecuteReader();

        //read enrolment information
        while (reader.Read())
        {
            //store enrolment details
            var enrolment = new
            {
                enrolmentID = Convert.ToInt32(reader["enrolmentID"]),
                enrolmentDate = Convert.ToDateTime(reader["enrolmentDate"]),
                eventID = Convert.ToInt32(reader["eventID"]),
                participantID = Convert.ToInt32(reader["participantID"]),
                participantName = reader["fullName"].ToString()
            };

            //add enrolment to the list
            enrolments.Add(enrolment);
        }

        //return enrolment list
        return enrolments;
    }


    public bool enrolment_belongs_to_organiser(int enrolmentID, int userID)
    {
        using SqlConnection connect = database.get_connection();
        connect.Open();
        string query = @"SELECT COUNT(*) FROM Enrolments en
                         INNER JOIN Events e ON en.eventID=e.eventID
                         INNER JOIN Organisers o ON e.organiserID=o.organiserID
                         WHERE en.enrolmentID=@enrolmentID AND o.userID=@userID;";
        using SqlCommand command = new SqlCommand(query, connect);
        command.Parameters.AddWithValue("@enrolmentID", enrolmentID);
        command.Parameters.AddWithValue("@userID", userID);
        return Convert.ToInt32(command.ExecuteScalar()) > 0;
    }

    public bool result_exists(int enrolmentID)
    {
        using SqlConnection connect = database.get_connection();
        connect.Open();
        using SqlCommand command = new SqlCommand("SELECT COUNT(*) FROM Results WHERE enrolmentID=@enrolmentID;", connect);
        command.Parameters.AddWithValue("@enrolmentID", enrolmentID);
        return Convert.ToInt32(command.ExecuteScalar()) > 0;
    }

    public void store_result(int enrolmentID, ResultRequest result)
    {
        using SqlConnection connect = database.get_connection();
        connect.Open();
        string query = @"INSERT INTO Results (finishTime, finishPosition, enrolmentID)
                         VALUES (@finishTime, @finishPosition, @enrolmentID);";
        using SqlCommand command = new SqlCommand(query, connect);
        command.Parameters.AddWithValue("@finishTime", result.finishTime);
        command.Parameters.AddWithValue("@finishPosition", result.finishPosition);
        command.Parameters.AddWithValue("@enrolmentID", enrolmentID);
        command.ExecuteNonQuery();
    }


    public List<object> get_my_results(int participantID)
    {
        //temp variable declaration
        List<object> results = new List<object>();

        //connect to database
        using SqlConnection connect = database.get_connection();
        connect.Open();

        //SQL query
        string query = @"SELECT r.resultID,
                            r.finishTime,
                            r.finishPosition,
                            en.enrolmentID,
                            e.eventID,
                            e.eventName
                     FROM Results r
                     INNER JOIN Enrolments en
                     ON r.enrolmentID = en.enrolmentID
                     INNER JOIN Events e
                     ON en.eventID = e.eventID
                     WHERE en.participantID = @participantID
                     ORDER BY e.eventDate;";

        //create SQL command
        using SqlCommand command = new SqlCommand(query, connect);

        //add parameter
        command.Parameters.AddWithValue("@participantID", participantID);

        //read results
        using SqlDataReader reader = command.ExecuteReader();

        while (reader.Read())
        {
            //store result details
            var result = new
            {
                resultID = Convert.ToInt32(reader["resultID"]),
                finishTime = reader["finishTime"].ToString(),
                finishPosition = reader["finishPosition"] == DBNull.Value
                    ? (int?)null
                    : Convert.ToInt32(reader["finishPosition"]),
                enrolmentID = Convert.ToInt32(reader["enrolmentID"]),
                eventID = Convert.ToInt32(reader["eventID"]),
                eventName = reader["eventName"].ToString()
            };

            //add result to list
            results.Add(result);
        }

        //return results
        return results;
    }



    public List<object> get_event_results(int eventID)
    {
        //temp variable declaration
        List<object> results = new List<object>();

        //connect to database
        using SqlConnection connect = database.get_connection();
        connect.Open();

        //SQL query
        string query = @"SELECT r.resultID,
                            r.finishTime,
                            r.finishPosition,
                            en.enrolmentID,
                            u.fullName
                     FROM Results r
                     INNER JOIN Enrolments en
                     ON r.enrolmentID = en.enrolmentID
                     INNER JOIN Participants p
                     ON en.participantID = p.participantID
                     INNER JOIN Users u
                     ON p.userID = u.userID
                     WHERE en.eventID = @eventID
                     ORDER BY r.finishPosition;";

        //create SQL command
        using SqlCommand command = new SqlCommand(query, connect);

        //add parameter
        command.Parameters.AddWithValue("@eventID", eventID);

        //read results
        using SqlDataReader reader = command.ExecuteReader();

        while (reader.Read())
        {
            //store result details
            var result = new
            {
                resultID = Convert.ToInt32(reader["resultID"]),
                finishTime = reader["finishTime"].ToString(),
                finishPosition = reader["finishPosition"] == DBNull.Value
                    ? (int?)null
                    : Convert.ToInt32(reader["finishPosition"]),
                enrolmentID = Convert.ToInt32(reader["enrolmentID"]),
                participantName = reader["fullName"].ToString()
            };

            //add result to list
            results.Add(result);
        }

        //return results
        return results;
    }
}