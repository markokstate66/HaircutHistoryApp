using HaircutHistoryApp.Models;

namespace HaircutHistoryApp.Services;

/// <summary>
/// Service interface for data operations.
/// Hierarchy: User → Profile (person) → HaircutRecord (individual haircut)
/// </summary>
public interface IDataService
{
    /// <summary>
    /// Last error message from a failed operation.
    /// </summary>
    string? LastError { get; }

    #region Profile Operations (person management)

    /// <summary>
    /// Gets all profiles owned by the current user.
    /// </summary>
    Task<List<Profile>> GetProfilesAsync();

    /// <summary>
    /// Gets a specific profile by ID.
    /// </summary>
    Task<Profile?> GetProfileAsync(string profileId);

    /// <summary>
    /// Creates or updates a profile.
    /// </summary>
    Task<bool> SaveProfileAsync(Profile profile);

    /// <summary>
    /// Deletes a profile (soft delete).
    /// </summary>
    Task<bool> DeleteProfileAsync(string profileId);

    #endregion

    #region Haircut Record Operations

    /// <summary>
    /// Gets all haircut records for a profile.
    /// </summary>
    Task<List<HaircutRecord>> GetHaircutRecordsAsync(string profileId);

    /// <summary>
    /// Gets a specific haircut record.
    /// </summary>
    Task<HaircutRecord?> GetHaircutRecordAsync(string profileId, string recordId);

    /// <summary>
    /// Creates or updates a haircut record.
    /// </summary>
    Task<bool> SaveHaircutRecordAsync(string profileId, HaircutRecord record);

    /// <summary>
    /// Deletes a haircut record.
    /// </summary>
    Task<bool> DeleteHaircutRecordAsync(string profileId, string recordId);

    #endregion

    #region Share Operations

    /// <summary>
    /// Creates a share session for a profile (generates QR code token).
    /// </summary>
    Task<ShareSession> CreateShareSessionAsync(string profileId);

    /// <summary>
    /// Accepts a share invitation using the token from QR code.
    /// </summary>
    Task<bool> AcceptShareAsync(string token);

    /// <summary>
    /// Gets profiles shared with the current user (as a stylist).
    /// </summary>
    Task<List<Profile>> GetSharedProfilesAsync();

    #endregion
}
