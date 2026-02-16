namespace AgencyCursor.Data;

/// <summary>
/// Helper class to get the consistent path for the RID interpreters database file.
/// </summary>
public static class RidDbHelper
{
    /// <summary>
    /// Gets the path to the RID interpreters database file.
    /// The file should be placed in the ContentRootPath (project root) of the web application.
    /// </summary>
    /// <param name="contentRootPath">The content root path of the web application</param>
    /// <returns>The full path to rid_interpreters.db</returns>
    public static string GetRidDbPath(string contentRootPath)
    {
        return Path.Combine(contentRootPath, "rid_interpreters.db");
    }
}
