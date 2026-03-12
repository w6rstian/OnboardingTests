using Xunit;
using FluentAssertions;
using Onboarding.Models;
using System.Collections.Generic;

namespace OnboardingXUnitTests.Models
{
    public class UserTests
    {
        // 5 tests
        [Theory]
        [InlineData("Jan", "Kowalski", "IT", "Developer")]
        [InlineData("Anna", "Nowak", "HR", "Recruiter")]
        [InlineData(null, null, null, null)]
        [InlineData("", "", "", "")]
        [InlineData("BardzoDlugieImieDlaTestu", "Nazwisko", "Department", "Pos")]
        public void User_BasicProperties_ShouldStoreValues(string name, string surname, string dept, string pos)
        {
            // Arrange & Act
            var user = new User
            {
                Name = name,
                Surname = surname,
                Department = dept,
                Position = pos
            };

            // Assert
            user.Name.Should().Be(name);
            user.Surname.Should().Be(surname);
            user.Department.Should().Be(dept);
            user.Position.Should().Be(pos);
        }

        // Test Login (Property Login)
        [Fact]
        public void User_Login_Property_ShouldWork()
        {
            // Arrange
            var user = new User
            {
                // Act
                Login = "jkowalski"
            };

            // Assert
            user.Login.Should().Be("jkowalski");
        }

        [Fact]
        public void User_BuddyId_ShouldBeNullable()
        {
            // Arrange & Act
            var user = new User { BuddyId = null };

            // Assert
            user.BuddyId.Should().BeNull();
        }

        [Fact]
        public void User_Buddy_ShouldAllowSettingBuddyObject()
        {
            // Arrange
            var buddyUser = new User { Id = 10, Name = "Opiekun" };

            // Act
            var user = new User { Buddy = buddyUser, BuddyId = 10 };

            // Assert
            user.Buddy.Name.Should().Be("Opiekun");
        }

        [Fact]
        public void User_BuddyRelation_CanBeChanged()
        {
            // Arrange
            var user = new User { BuddyId = 1 };

            // Act
            user.BuddyId = 2;

            // Assert
            user.BuddyId.Should().Be(2);
        }

        [Fact]
        public void User_InheritedIdentityProperties_ShouldWork()
        {
            // Testing inheritence after IdentityUser<int> 

            // Arrange & Act
            var user = new User { Email = "test@test.pl", PhoneNumber = "123456789" };

            // Assert
            user.Email.Should().Be("test@test.pl");
            user.PhoneNumber.Should().Be("123456789");
        }

        // testing collections (6 tests)
        [Fact]
        public void User_Collections_ShouldNotBeNull_WhenAssigned()
        {
            // Arrange
            var user = new User
            {
                UserCourses = [],
                SentMessages = [],
                ReceivedMessages = [],
                GivenRewards = [],
                ReceivedRewards = [],
                Notifications = []
            };

            // Act & Assert
            user.UserCourses.Should().NotBeNull();
            user.SentMessages.Should().NotBeNull();
            user.ReceivedMessages.Should().NotBeNull();
            user.Notifications.Should().NotBeNull();
        }

        [Fact]
        public void User_UserCourses_ShouldStoreItems()
        {
            // Arrange
            var user = new User { UserCourses = [] };
            var course = new UserCourse { CourseId = 1 };

            // Act
            user.UserCourses.Add(course);

            // Assert
            user.UserCourses.Should().Contain(course);
        }

        [Fact]
        public void User_Announcements_ShouldStoreItems()
        {
            // Arrange
            var user = new User { Announcements = [] };
            var announcement = new Announcement { Id = 1 };

            // Act
            user.Announcements.Add(announcement);

            // Assert
            user.Announcements.Should().HaveCount(1);
        }

        [Fact]
        public void User_GivenRewards_ShouldStoreItems()
        {
            // Arrange
            var user = new User { GivenRewards = [] };
            var reward = new Reward
            {
                Id = 1,
                Feedback = "Œwietna robota!",
                Rating = 5
            };

            // Act
            user.GivenRewards.Add(reward);

            // Assert
            user.GivenRewards.Should().Contain(reward).And.HaveCount(1);
        }

        [Fact]
        public void User_ReceivedRewards_ShouldStoreItems()
        {
            // Arrange
            var user = new User { ReceivedRewards = [] };
            var reward = new Reward
            {
                Id = 5,
                Feedback = "Pomocny kolega",
                Rating = 4
            };

            // Act
            user.ReceivedRewards.Add(reward);

            // Assert
            user.ReceivedRewards.Should().HaveCount(1);
            user.ReceivedRewards.First().Feedback.Should().Be("Pomocny kolega");
        }
    }
}