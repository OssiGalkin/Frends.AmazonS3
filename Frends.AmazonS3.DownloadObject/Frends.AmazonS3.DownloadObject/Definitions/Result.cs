using System.Collections.Generic;

namespace Frends.AmazonS3.DownloadObject.Definitions;

/// <summary>
/// Result.
/// </summary>
public class Result
{
    /// <summary>
    /// List of download results.
    /// </summary>
    /// <example>Download complete: {fullPath}, Source file {key} deleted, File already exists at {fullPath}.</example>
    public List<SingleResultObject> Results { get; private set; }

    internal Result(List<SingleResultObject> downloadResult)
    {
        Results = downloadResult;
    }
}

