# RACEDAY EVENT 
## ASSIGNMENT 2: RESTFUL API DEVELOPMENT

### 1. PROJECT OVERVIEW

RaceDay is an System developed using ASP.NET Core Web API and C#. The system allows organisers to create and manage sporting events, while participants can register, enrol in events and view their race results.
The project was developed using Visual Studio 2022 and connected to a SQL Server database. It uses RESTful API endpoints to perform operations such as creating, retrieving, updating and deleting information. 

The main objective of this project is to demonstrate RESTful API development, database connectivity, user authentication, role-based authorisation, password security and automated testing.

### 2. TECHNOLOGIES USED
The following technologies were used to develop the RaceDay system:
- **C#:** Used to develop the application and implement its functionality.
- **ASP.NET Core Web API:** Used to create RESTful API endpoints.
- **Visual Studio 2022:** Used to write, debug, build and test the application.
- **SQL Server:** Used to store and manage application data.
- **SQL Server Management Studio (SSMS):** Used to create and manage database tables.
- **ADO.NET:** Used to connect the API to SQL Server and execute SQL queries.
- **BCrypt:** Used to hash and verify user passwords.
- **Swagger:** Used to test API endpoints.
- **xUnit:** Used to create and execute automated tests.
- **GitHub and GitHub Actions:** Intended for version control and CI/CD once configured.

### 3. DATABASE IMPLEMENTATION

The system uses a SQL Server database named Raceday.
The database consists of seven main tables:
1. **Users:** Stores user information, including names, email addresses, phone numbers, passwords and user roles.
2. **Organisers:** Stores organiser information linked to registered users.
3. **Participants:** Stores participant information linked to registered users.
4. **Events:** Stores event names, descriptions, dates, locations, distances and event types.
5. **Categories:** Stores the categories associated with sporting events.
6. **Enrolments:** Records participants who have enrolled in events.
7. **Results:** Stores participants' finishing times and finishing positions.

Primary keys and foreign keys are used to maintain relationships between the tables.

The API connects to SQL Server using ADO.NET and parameterised SQL queries.

### 4. RESTFUL API IMPLEMENTATION
The application contains seven controllers responsible for managing different system operations.

**4.1 AuthController**
The AuthController manages user registration, login and logout.
Users can register as either Organisers or Participants. Passwords are hashed using BCrypt before being stored in the database.
During login, the system verifies the entered password against the stored password hash.
Sessions are used to maintain user login information.

**4.2 UsersController**

The UsersController allows authenticated users to view and update their personal information.

Users can retrieve their profiles and update their names and phone numbers.

The controller prevents unauthenticated users from accessing profile information.

**4.3 EventsController**

The EventsController manages sporting events.

Organisers can create, retrieve, update and delete events. Authorisation checks prevent Participants from performing Organiser-only operations.

Organisers are also restricted from modifying events belonging to other Organisers.

**4.4 CategoriesController**

The CategoriesController manages event categories.

It allows event categories to be created, retrieved, updated and deleted.

Categories are linked to their corresponding events in the database.

**4.5 EnrolmentsController**

The EnrolmentsController allows Participants to enrol in sporting events.

The system checks whether a Participant has already enrolled in an event to prevent duplicate enrolments.

Organisers can view enrolments associated with their events.

**4.6 ResultsController**

The ResultsController manages race results.

Organisers can record participants' finishing times and finishing positions.

The system prevents duplicate results from being recorded for the same enrolment.

Participants can view their own race results, while Organisers can view results associated with their events.

**4.7 ParticipantsController**

The ParticipantsController manages the retrieval of participant information.

Organisers can retrieve participant records and search for individual participants.

Participants can view their own profiles but are prevented from accessing other participants' profiles.

### 5. AUTHENTICATION AND SECURITY
The RaceDay system implements authentication and role-based authorisation.

Two user roles are supported: Organiser and Participant.

BCrypt password hashing is used to protect passwords created through the registration API.

Sessions are used to identify logged-in users and determine their access permissions.

Protected endpoints return HTTP 401 Unauthorized when users are not logged in.

HTTP 403 Forbidden is returned when users attempt to access operations that their roles do not permit.

SQL parameters are used when executing queries involving user input to reduce the risk of SQL injection.

### 6. API TESTING USING SWAGGER

Swagger was used to test the RESTful API endpoints.

The following operations were successfully tested:

- User registration and login.
- Organiser and Participant authentication.
- Creating, retrieving, updating and deleting events.
- Creating, retrieving, updating and deleting categories.
- Enrolling participants in events.
- Preventing duplicate event enrolments.
- Recording race results.
- Retrieving event results.
- Retrieving personal race results.
- Restricting unauthorised access to participant information.
- Retrieving and updating user profiles.

The API returned appropriate HTTP response codes, including 200 OK, 400 Bad Request, 403 Forbidden and 409 Conflict during the relevant tests.

### 7. AUTOMATED UNIT TESTING

Automated tests were developed using the xUnit testing framework.

The tests are contained in the RaceDay.Tests project.

The automated tests were organised in a single file named BasicRequirementTests.cs.

The tests cover validation scenarios, including accepted user roles, valid event types, password hashing and verification, finishing positions and finishing times.

The tests were executed locally using Visual Studio 2022 Test Explorer.

**Local Test Results:**

Total tests executed: 24

Tests passed: 24

Tests failed: 0

Tests skipped: 0

Success rate: 100%

Execution time: Approximately 1.8 seconds.

All 24 automated test cases passed successfully.

These tests demonstrate that the implemented validation rules and password verification operations work as expected. Additional automated controller and database integration tests are needed to demonstrate comprehensive coverage of the complete API.

### 8. CONTINUOUS INTEGRATION AND DEPLOYMENT

GitHub Actions is intended to support continuous integration and continuous delivery for the RaceDay project.

The planned CI/CD workflow will automatically restore project dependencies, build the solution and execute automated tests when code is pushed to GitHub.

At the time of preparing this README, the project has been tested locally, but successful execution through GitHub Actions has not yet been verified.

### 9. HOW TO RUN THE APPLICATION

To run the RaceDay application:

1. Open the RaceDay solution in Visual Studio 2022.
2. Ensure that SQL Server LocalDB is installed and running.
3. Open SQL Server Management Studio and ensure the Raceday database exists.
4. Verify that the database connection string is correctly configured in appsettings.json.
5. Build the solution using Ctrl + Shift + B.
6. Run the RaceDay API project using the HTTPS launch profile.
7. Open Swagger in the browser.
8. Use the available API endpoints to register, log in and perform the permitted operations.

To run the automated tests, open Test Explorer in Visual Studio and select Run All Tests.

### 10. CONCLUSION

The RaceDay Event Management System demonstrates the development of a RESTful API using ASP.NET Core Web API, C# and SQL Server.

The application provides user registration, authentication, event management, category management, participant enrolment and race result management.

Role-based authorisation, BCrypt password hashing and parameterised SQL queries were implemented to improve application security.

The main API operations were successfully tested using Swagger, and all 24 automated xUnit test cases passed locally.

The project demonstrates practical knowledge of RESTful API development, database integration, authentication, security and software testing. Further controller-level testing and GitHub Actions verification will complete the remaining testing and CI/CD requirements.

### GitHub.
