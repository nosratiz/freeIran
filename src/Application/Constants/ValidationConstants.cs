namespace Application.Constants;

/// <summary>
/// Centralized validation constants for consistent rule enforcement.
/// </summary>
public static class ValidationConstants
{
    /// <summary>
    /// Name field validation rules.
    /// </summary>
    public static class Name
    {
        public const int MinLength = 2;
        public const int MaxLength = 100;
        
        /// <summary>
        /// Regex pattern allowing Unicode letters, combining marks, spaces, hyphens, and apostrophes.
        /// </summary>
        public const string Pattern = @"^[\p{L}\p{M}' -]+$";
    }

    /// <summary>
    /// Email field validation rules.
    /// </summary>
    public static class Email
    {
        /// <summary>
        /// RFC 5321 maximum email address length.
        /// </summary>
        public const int MaxLength = 254;
    }

    /// <summary>
    /// Age-related validation rules.
    /// </summary>
    public static class Age
    {
        public const int MinimumAge = 13;
        public const int MaximumAge = 120;
    }

    /// <summary>
    /// Age group boundaries for categorization.
    /// </summary>
    public static class AgeGroups
    {
        public const int Under18Max = 17;
        public const int Age18To24Min = 18;
        public const int Age18To24Max = 24;
        public const int Age25To34Min = 25;
        public const int Age25To34Max = 34;
        public const int Age35To44Min = 35;
        public const int Age35To44Max = 44;
        public const int Age45To54Min = 45;
        public const int Age45To54Max = 54;
        public const int Age55To64Min = 55;
        public const int Age55To64Max = 64;
        public const int Age65PlusMin = 65;
    }

    /// <summary>
    /// Languages field validation rules.
    /// </summary>
    public static class Languages
    {
        public const int MinCount = 1;
        public const int MaxCount = 20;
        public const int MaxNameLength = 50;
    }

    /// <summary>
    /// Professional skills field validation rules.
    /// </summary>
    public static class Skills
    {
        public const int MaxCount = 50;
        public const int MaxDescriptionLength = 200;
    }
}

/// <summary>
/// ISO 3166-1 alpha-2 country codes for validation.
/// </summary>
public static class CountryCodes
{
    /// <summary>
    /// Valid ISO 3166-1 alpha-2 country codes.
    /// </summary>
    public static readonly HashSet<string> ValidCodes = new(StringComparer.OrdinalIgnoreCase)
    {
        "AF", "AL", "DZ", "AD", "AO", "AR", "AM", "AU", "AT", "AZ",
        "BH", "BD", "BY", "BE", "BZ", "BJ", "BT", "BO", "BA", "BW",
        "BR", "BN", "BG", "BF", "BI", "KH", "CM", "CA", "CV", "CF",
        "TD", "CL", "CN", "CO", "KM", "CG", "CD", "CR", "CI", "HR",
        "CU", "CY", "CZ", "DK", "DJ", "DM", "DO", "EC", "EG", "SV",
        "GQ", "ER", "EE", "ET", "FJ", "FI", "FR", "GA", "GM", "GE",
        "DE", "GH", "GR", "GT", "GN", "GW", "GY", "HT", "HN", "HU",
        "IS", "IN", "ID", "IR", "IQ", "IE", "IL", "IT", "JM", "JP",
        "JO", "KZ", "KE", "KI", "KP", "KR", "KW", "KG", "LA", "LV",
        "LB", "LS", "LR", "LY", "LI", "LT", "LU", "MK", "MG", "MW",
        "MY", "MV", "ML", "MT", "MH", "MR", "MU", "MX", "FM", "MD",
        "MC", "MN", "ME", "MA", "MZ", "MM", "NA", "NR", "NP", "NL",
        "NZ", "NI", "NE", "NG", "NO", "OM", "PK", "PW", "PA", "PG",
        "PY", "PE", "PH", "PL", "PT", "QA", "RO", "RU", "RW", "KN",
        "LC", "VC", "WS", "SM", "ST", "SA", "SN", "RS", "SC", "SL",
        "SG", "SK", "SI", "SB", "SO", "ZA", "SS", "ES", "LK", "SD",
        "SR", "SZ", "SE", "CH", "SY", "TW", "TJ", "TZ", "TH", "TL",
        "TG", "TO", "TT", "TN", "TR", "TM", "TV", "UG", "UA", "AE",
        "GB", "US", "UY", "UZ", "VU", "VA", "VE", "VN", "YE", "ZM", "ZW"
    };

    /// <summary>
    /// Checks if the provided country code is valid.
    /// </summary>
    public static bool IsValid(string? countryCode) =>
        !string.IsNullOrWhiteSpace(countryCode) && 
        ValidCodes.Contains(countryCode.Trim().ToUpperInvariant());
}

/// <summary>
/// Common languages for informational purposes (non-restrictive validation).
/// </summary>
public static class CommonLanguages
{
    /// <summary>
    /// Common world languages.
    /// </summary>
    public static readonly HashSet<string> Languages = new(StringComparer.OrdinalIgnoreCase)
    {
        "English", "Spanish", "French", "German", "Italian", "Portuguese",
        "Russian", "Chinese", "Japanese", "Korean", "Arabic", "Hindi",
        "Bengali", "Urdu", "Indonesian", "Turkish", "Vietnamese", "Thai",
        "Dutch", "Polish", "Ukrainian", "Romanian", "Greek", "Czech",
        "Swedish", "Hungarian", "Finnish", "Danish", "Norwegian", "Hebrew",
        "Persian", "Farsi", "Dari", "Pashto", "Kurdish", "Azerbaijani",
        "Armenian", "Georgian", "Tajik", "Uzbek", "Turkmen", "Kazakh",
        "Swahili", "Amharic", "Somali", "Hausa", "Yoruba", "Igbo",
        "Malay", "Tagalog", "Tamil", "Telugu", "Kannada", "Malayalam",
        "Marathi", "Gujarati", "Punjabi", "Nepali", "Sinhala", "Burmese"
    };
}
