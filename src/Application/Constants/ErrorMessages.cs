namespace Application.Constants;

/// <summary>
/// Centralized error messages for consistent user feedback.
/// </summary>
public static class ErrorMessages
{
    /// <summary>
    /// Name field validation messages.
    /// </summary>
    public static class Name
    {
        public const string Required = "Name is required.";
        public const string MinLength = "Name must be at least {0} characters long.";
        public const string MaxLength = "Name cannot exceed {0} characters.";
        public const string InvalidFormat = "Name can only contain letters, spaces, hyphens, and apostrophes.";
        
        public static string GetMinLengthMessage(int minLength) => 
            string.Format(MinLength, minLength);
        
        public static string GetMaxLengthMessage(int maxLength) => 
            string.Format(MaxLength, maxLength);
    }

    /// <summary>
    /// Email field validation messages.
    /// </summary>
    public static class Email
    {
        public const string Required = "Email is required.";
        public const string MaxLength = "Email cannot exceed {0} characters.";
        public const string InvalidFormat = "Please provide a valid email address.";
        public const string AlreadyExists = "A registrant with this email already exists.";
        
        public static string GetMaxLengthMessage(int maxLength) => 
            string.Format(MaxLength, maxLength);
    }

    /// <summary>
    /// Date of birth validation messages.
    /// </summary>
    public static class DateOfBirth
    {
        public const string Required = "Date of birth is required.";
        public const string MinimumAge = "You must be at least {0} years old to register.";
        public const string MaximumAge = "Please enter a valid date of birth.";
        
        public static string GetMinimumAgeMessage(int minAge) => 
            string.Format(MinimumAge, minAge);
    }

    /// <summary>
    /// Country field validation messages.
    /// </summary>
    public static class Country
    {
        public const string Required = "Country is required.";
        public const string InvalidCode = "Please provide a valid ISO 3166-1 alpha-2 country code.";
    }

    /// <summary>
    /// Gender field validation messages.
    /// </summary>
    public static class Gender
    {
        public const string Invalid = "Please select a valid gender option.";
    }

    /// <summary>
    /// Education level validation messages.
    /// </summary>
    public static class EducationLevel
    {
        public const string Invalid = "Please select a valid education level.";
    }

    /// <summary>
    /// Languages field validation messages.
    /// </summary>
    public static class Languages
    {
        public const string NullList = "Languages spoken list cannot be null.";
        public const string MinCount = "Please specify at least one language.";
        public const string MaxCount = "You can specify a maximum of {0} languages.";
        public const string EmptyName = "Language name cannot be empty.";
        public const string MaxNameLength = "Language name cannot exceed {0} characters.";
        
        public static string GetMaxCountMessage(int maxCount) => 
            string.Format(MaxCount, maxCount);
        
        public static string GetMaxNameLengthMessage(int maxLength) => 
            string.Format(MaxNameLength, maxLength);
    }

    /// <summary>
    /// Professional skills validation messages.
    /// </summary>
    public static class Skills
    {
        public const string NullList = "Professional skills list cannot be null.";
        public const string MaxCount = "You can specify a maximum of {0} skills.";
        public const string MaxDescriptionLength = "Skill description cannot exceed {0} characters.";
        
        public static string GetMaxCountMessage(int maxCount) => 
            string.Format(MaxCount, maxCount);
        
        public static string GetMaxDescriptionLengthMessage(int maxLength) => 
            string.Format(MaxDescriptionLength, maxLength);
    }

    /// <summary>
    /// Registrant-related error messages.
    /// </summary>
    public static class Registrant
    {
        public const string IdRequired = "Registrant ID is required.";
        public const string NotFoundTemplate = "Registrant with ID '{0}' was not found.";
        
        public static string NotFound(string id) => 
            string.Format(NotFoundTemplate, id);
    }

    /// <summary>
    /// Repository/database error messages.
    /// </summary>
    public static class Repository
    {
        public const string DatabaseErrorTemplate = "Database error: {0}";
        public const string UnexpectedError = "An unexpected error occurred while saving the registrant.";
        
        public static string DatabaseError(string message) => 
            string.Format(DatabaseErrorTemplate, message);
    }

    /// <summary>
    /// General/fallback error messages.
    /// </summary>
    public static class General
    {
        public const string UnknownError = "An error occurred.";
        public const string UnknownField = "Unknown";
    }
}
