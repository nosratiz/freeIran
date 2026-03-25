using Domain.Aggregates;

namespace Application.Constants;

/// <summary>
/// Human-readable display strings for enum values and UI presentation.
/// </summary>
public static class DisplayStrings
{
    /// <summary>
    /// Display strings for age groups.
    /// </summary>
    public static class AgeGroups
    {
        public const string Under18 = "Under 18";
        public const string Age18To24 = "18-24";
        public const string Age25To34 = "25-34";
        public const string Age35To44 = "35-44";
        public const string Age45To54 = "45-54";
        public const string Age55To64 = "55-64";
        public const string Age65Plus = "65+";
        public const string Unknown = "Unknown";

        /// <summary>
        /// Gets the display string for an age group.
        /// </summary>
        public static string Get(AgeGroup ageGroup) => ageGroup switch
        {
            AgeGroup.Under18 => Under18,
            AgeGroup.Age18To24 => Age18To24,
            AgeGroup.Age25To34 => Age25To34,
            AgeGroup.Age35To44 => Age35To44,
            AgeGroup.Age45To54 => Age45To54,
            AgeGroup.Age55To64 => Age55To64,
            AgeGroup.Age65Plus => Age65Plus,
            _ => Unknown
        };
    }

    /// <summary>
    /// Display strings for education levels.
    /// </summary>
    public static class EducationLevels
    {
        public const string NoFormalEducation = "No Formal Education";
        public const string PrimarySchool = "Primary School";
        public const string HighSchool = "High School";
        public const string VocationalTraining = "Vocational Training";
        public const string Bachelors = "Bachelor's Degree";
        public const string Masters = "Master's Degree";
        public const string Doctorate = "Doctorate";
        public const string Other = "Other";
        public const string Unknown = "Unknown";

        /// <summary>
        /// Gets the display string for an education level.
        /// </summary>
        public static string Get(EducationLevel level) => level switch
        {
            EducationLevel.NoFormalEducation => NoFormalEducation,
            EducationLevel.PrimarySchool => PrimarySchool,
            EducationLevel.HighSchool => HighSchool,
            EducationLevel.VocationalTraining => VocationalTraining,
            EducationLevel.Bachelors => Bachelors,
            EducationLevel.Masters => Masters,
            EducationLevel.Doctorate => Doctorate,
            EducationLevel.Other => Other,
            _ => Unknown
        };
    }

    /// <summary>
    /// Log message strings.
    /// </summary>
    public static class LogMessages
    {
        public const string ApplicationStarting = "Starting Free Iran Reconstruction Portal API";
        public const string ApplicationStarted = "Free Iran Portal API started successfully";
        public const string ApplicationStopping = "Shutting down Free Iran Portal API";
        public const string ApplicationFatalError = "Application terminated unexpectedly";
    }
}
