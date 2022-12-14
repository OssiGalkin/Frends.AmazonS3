using System.Collections.Generic;

namespace Frends.AmazonS3.DownloadObject.Definitions;

/// <summary>
/// Result.
/// </summary>
public class Result
{
    /// <summary>
    /// Task complete without errors.
    /// </summary>
    /// <example>True</example>
    public bool Success { get; private set; }

    /// <summary>
    /// List of download results.
    /// </summary>
    public List<SingleResultObject> Results { get; private set; }

    internal Result(bool success, List<SingleResultObject> results)
    {
        Success = success;
        Results = results;
    }
}