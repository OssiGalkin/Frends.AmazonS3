namespace Frends.AmazonS3.UploadObject.Definitions;

/// <summary>
/// Upload result.
/// </summary>
public class UploadResult
{
    /// <summary>
    /// Upload result.
    /// </summary>
    /// <example>"Upload complete: {fullpath}</example>
    public string UploadedObject { get; private set; }

    internal UploadResult(string uploadedObject)
    {
        UploadedObject = uploadedObject;
    }
}