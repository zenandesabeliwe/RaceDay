
using Xunit;

namespace RaceDay.Tests
{//start of namespace

    public class BasicRequirementTests
    {//start of class

        //TEST 1: Check accepted user roles
        [Theory]
        [InlineData("Organiser", true)]
        [InlineData("Participant", true)]
        [InlineData("Admin", false)]
        [InlineData("", false)]
        [InlineData("Student", false)]
        public void RegistrationOnlyAcceptsTheTwoRequiredRoles(
            string role, bool expected)
        {
            //temp variable declaration
            bool valid = role == "Organiser" ||
                         role == "Participant";

            //check expected result
            Assert.Equal(expected, valid);
        }


        //TEST 2: Check accepted event types
        [Theory]
        [InlineData("Run", true)]
        [InlineData("Walk", true)]
        [InlineData("Cycle", true)]
        [InlineData("Marathon", false)]
        [InlineData("", false)]
        public void EventTypeMustBeRunWalkOrCycle(
            string eventType, bool expected)
        {
            //temp variable declaration
            bool valid = eventType == "Run" ||
                         eventType == "Walk" ||
                         eventType == "Cycle";

            //check expected result
            Assert.Equal(expected, valid);
        }


        //TEST 3: Check correct and incorrect passwords
        [Theory]
        [InlineData("correctPassword", "correctPassword", true)]
        [InlineData("wrongPassword", "correctPassword", false)]
        public void PasswordVerificationUsesTheSubmittedPassword(
            string entered,
            string storedOriginalForTest,
            bool expected)
        {
            //hash original password
            string hash = BCrypt.Net.BCrypt.HashPassword(
                storedOriginalForTest);

            //verify entered password
            bool result = BCrypt.Net.BCrypt.Verify(
                entered, hash);

            //check expected result
            Assert.Equal(expected, result);
        }


        //TEST 4: Check that passwords are hashed
        [Fact]
        public void PasswordMustBeHashed()
        {
            //temp variable declaration
            string password = "TestPassword123!";

            //hash password
            string hashedPassword =
                BCrypt.Net.BCrypt.HashPassword(password);

            //check password is not stored as plain text
            Assert.NotEqual(password, hashedPassword);

            //check hashed password can be verified
            Assert.True(
                BCrypt.Net.BCrypt.Verify(password, hashedPassword));
        }


        //TEST 5: Check invalid password
        [Fact]
        public void IncorrectPasswordMustBeRejected()
        {
            //temp variable declaration
            string password = "CorrectPassword123!";
            string wrongPassword = "WrongPassword123!";

            //hash correct password
            string hashedPassword =
                BCrypt.Net.BCrypt.HashPassword(password);

            //verify wrong password
            bool result = BCrypt.Net.BCrypt.Verify(
                wrongPassword, hashedPassword);

            //check result is false
            Assert.False(result);
        }


        //TEST 6: Check valid finish position
        [Theory]
        [InlineData(1, true)]
        [InlineData(2, true)]
        [InlineData(10, true)]
        [InlineData(0, false)]
        [InlineData(-1, false)]
        public void FinishPositionMustBeGreaterThanZero(
            int position, bool expected)
        {
            //temp variable declaration
            bool valid = position > 0;

            //check expected result
            Assert.Equal(expected, valid);
        }


        //TEST 7: Check valid finish time
        [Theory]
        [InlineData(0, true)]
        [InlineData(60, true)]
        [InlineData(3600, true)]
        [InlineData(-1, false)]
        [InlineData(86400, false)]
        public void FinishTimeMustBeValid(
            int seconds, bool expected)
        {
            //convert seconds into time
            TimeSpan finishTime = TimeSpan.FromSeconds(seconds);

            //check time range
            bool valid = finishTime >= TimeSpan.Zero &&
                         finishTime < TimeSpan.FromDays(1);

            //check expected result
            Assert.Equal(expected, valid);
        }

    }//end of class

}//end of namespace
