using System.Collections.Generic;

namespace Frends.AmazonS3.UploadObject;

/// <summary>
/// Result.
/// </summary>
public class Result
{
    /// <summary>
    /// List of upload result(s).
    /// </summary>
    /// <example>"Upload complete: {fullpath}</example>
    public List<UploadResult> Results { get; private set; }

    internal Result(List<UploadResult> uploadResult)
    {
        Results = uploadResult;
    }
}
