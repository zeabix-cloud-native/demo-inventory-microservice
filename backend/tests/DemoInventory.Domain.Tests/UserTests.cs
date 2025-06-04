using DemoInventory.Domain.Entities;

namespace DemoInventory.Domain.Tests;

public class UserTests
{
    [Fact]
    public void User_Should_Create_With_Valid_Properties()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Username = "testuser",
            Email = "test@example.com",
            FirstName = "John",
            LastName = "Doe",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Assert
        Assert.Equal(1, user.Id);
        Assert.Equal("testuser", user.Username);
        Assert.Equal("test@example.com", user.Email);
        Assert.Equal("John", user.FirstName);
        Assert.Equal("Doe", user.LastName);
        Assert.True(user.IsActive);
        Assert.Equal("John Doe", user.FullName);
        Assert.True(user.CreatedAt <= DateTime.UtcNow);
        Assert.True(user.UpdatedAt <= DateTime.UtcNow);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void User_Username_Should_Throw_When_NullOrEmpty(string invalidUsername)
    {
        // Arrange & Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new User { Username = invalidUsername });
        Assert.Contains("Username cannot be null or empty", exception.Message);
    }

    [Fact]
    public void User_Username_Should_Throw_When_TooShort()
    {
        // Arrange & Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new User { Username = "ab" });
        Assert.Contains("Username must be at least 3 characters long", exception.Message);
    }

    [Fact]
    public void User_Username_Should_Throw_When_TooLong()
    {
        // Arrange
        var longUsername = new string('a', 51);
        
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new User { Username = longUsername });
        Assert.Contains("Username cannot exceed 50 characters", exception.Message);
    }

    [Theory]
    [InlineData("user-name")]
    [InlineData("user@name")]
    [InlineData("user name")]
    [InlineData("user.name")]
    public void User_Username_Should_Throw_When_InvalidFormat(string invalidUsername)
    {
        // Arrange & Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new User { Username = invalidUsername });
        Assert.Contains("Username must contain only letters, numbers, and underscores", exception.Message);
    }

    [Fact]
    public void User_Username_Should_ConvertToLowercase()
    {
        // Arrange & Act
        var user = new User { Username = "TestUser123" };
        
        // Assert
        Assert.Equal("testuser123", user.Username);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void User_Email_Should_Throw_When_NullOrEmpty(string invalidEmail)
    {
        // Arrange & Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new User { Email = invalidEmail });
        Assert.Contains("Email cannot be null or empty", exception.Message);
    }

    [Fact]
    public void User_Email_Should_Throw_When_TooLong()
    {
        // Arrange - Create email that's exactly 255 characters (over the 254 limit)
        var tooLongEmail = new string('a', 246) + "@test.com"; // 246 + 9 = 255 chars total
        
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new User { Email = tooLongEmail });
        Assert.Contains("Email cannot exceed 254 characters", exception.Message);
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("@test.com")]
    [InlineData("test@")]
    [InlineData("test@.com")]
    [InlineData("test.test.com")]
    public void User_Email_Should_Throw_When_InvalidFormat(string invalidEmail)
    {
        // Arrange & Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new User { Email = invalidEmail });
        Assert.Contains("Email format is invalid", exception.Message);
    }

    [Fact]
    public void User_Email_Should_ConvertToLowercase()
    {
        // Arrange & Act
        var user = new User { Email = "Test.User@EXAMPLE.COM" };
        
        // Assert
        Assert.Equal("test.user@example.com", user.Email);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void User_FirstName_Should_Throw_When_NullOrEmpty(string invalidFirstName)
    {
        // Arrange & Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new User { FirstName = invalidFirstName });
        Assert.Contains("First name cannot be null or empty", exception.Message);
    }

    [Fact]
    public void User_FirstName_Should_Throw_When_TooLong()
    {
        // Arrange
        var longFirstName = new string('A', 101);
        
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new User { FirstName = longFirstName });
        Assert.Contains("First name cannot exceed 100 characters", exception.Message);
    }

    [Fact]
    public void User_FirstName_Should_Trim_Whitespace()
    {
        // Arrange & Act
        var user = new User { FirstName = "  John  " };
        
        // Assert
        Assert.Equal("John", user.FirstName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void User_LastName_Should_Throw_When_NullOrEmpty(string invalidLastName)
    {
        // Arrange & Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new User { LastName = invalidLastName });
        Assert.Contains("Last name cannot be null or empty", exception.Message);
    }

    [Fact]
    public void User_LastName_Should_Throw_When_TooLong()
    {
        // Arrange
        var longLastName = new string('A', 101);
        
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new User { LastName = longLastName });
        Assert.Contains("Last name cannot exceed 100 characters", exception.Message);
    }

    [Fact]
    public void User_LastName_Should_Trim_Whitespace()
    {
        // Arrange & Act
        var user = new User { LastName = "  Doe  " };
        
        // Assert
        Assert.Equal("Doe", user.LastName);
    }

    [Fact]
    public void User_FullName_Should_Return_CombinedName()
    {
        // Arrange & Act
        var user = new User 
        { 
            FirstName = "John",
            LastName = "Doe"
        };
        
        // Assert
        Assert.Equal("John Doe", user.FullName);
    }

    [Fact]
    public void User_IsActive_Should_Default_To_True()
    {
        // Arrange & Act
        var user = new User();
        
        // Assert
        Assert.True(user.IsActive);
    }

    [Fact]
    public void User_Validate_Should_Pass_With_Valid_User()
    {
        // Arrange
        var user = new User
        {
            Username = "valid_user123",
            Email = "valid@example.com",
            FirstName = "John",
            LastName = "Doe"
        };
        
        // Act & Assert - Should not throw
        user.Validate();
    }
}