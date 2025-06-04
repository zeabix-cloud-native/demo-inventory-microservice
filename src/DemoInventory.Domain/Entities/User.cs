namespace DemoInventory.Domain.Entities;

public class User
{
    private string _username = string.Empty;
    private string _email = string.Empty;
    private string _firstName = string.Empty;
    private string _lastName = string.Empty;

    public int Id { get; set; }
    
    public string Username 
    { 
        get => _username;
        set 
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Username cannot be null or empty.", nameof(Username));
            if (value.Length < 3)
                throw new ArgumentException("Username must be at least 3 characters long.", nameof(Username));
            if (value.Length > 50)
                throw new ArgumentException("Username cannot exceed 50 characters.", nameof(Username));
            // Username should contain only alphanumeric characters and underscores
            if (!System.Text.RegularExpressions.Regex.IsMatch(value, @"^[a-zA-Z0-9_]+$"))
                throw new ArgumentException("Username must contain only letters, numbers, and underscores.", nameof(Username));
            _username = value.Trim().ToLowerInvariant();
        }
    }
    
    public string Email 
    { 
        get => _email;
        set 
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Email cannot be null or empty.", nameof(Email));
            if (value.Length > 254)
                throw new ArgumentException("Email cannot exceed 254 characters.", nameof(Email));
            // Basic email validation
            if (!System.Text.RegularExpressions.Regex.IsMatch(value, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                throw new ArgumentException("Email format is invalid.", nameof(Email));
            _email = value.Trim().ToLowerInvariant();
        }
    }
    
    public string FirstName 
    { 
        get => _firstName;
        set 
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("First name cannot be null or empty.", nameof(FirstName));
            if (value.Length > 100)
                throw new ArgumentException("First name cannot exceed 100 characters.", nameof(FirstName));
            _firstName = value.Trim();
        }
    }
    
    public string LastName 
    { 
        get => _lastName;
        set 
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Last name cannot be null or empty.", nameof(LastName));
            if (value.Length > 100)
                throw new ArgumentException("Last name cannot exceed 100 characters.", nameof(LastName));
            _lastName = value.Trim();
        }
    }
    
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public string FullName => $"{FirstName} {LastName}";

    public void Validate()
    {
        // Trigger validation for all properties
        var tempUsername = Username;
        var tempEmail = Email;
        var tempFirstName = FirstName;
        var tempLastName = LastName;
    }
}