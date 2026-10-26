namespace CseLibrary.Core.Constants;

/// <summary>Claims the app adds to the login cookie besides Identity's own.</summary>
public static class AppClaims
{
    /// <summary>The user's photo path (<c>Uploads/Users/...</c>); absent when there is none.</summary>
    public const string Photo = "cse:photo";
}
