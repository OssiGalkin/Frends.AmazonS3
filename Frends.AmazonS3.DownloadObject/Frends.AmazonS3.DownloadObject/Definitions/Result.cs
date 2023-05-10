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
    /// List of downloaded objects.
    /// </summary>
    /// <example>{ "File.txt", "C:\temp\File.txt", true, false, "Additional information" }</example>
    public List<SingleResultObject> Data { get; private set; }

    internal Result(bool success, List<SingleResultObject> data)
    {
        Success = success;
        Data = data;
    }
}