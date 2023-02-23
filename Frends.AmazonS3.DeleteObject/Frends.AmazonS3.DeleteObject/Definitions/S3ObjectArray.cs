namespace Frends.AmazonS3.DeleteObject.Definitions;

/// <summary>
/// Single object.
/// </summary>
public class S3ObjectArray
{
    /// <summary>
    /// AWS S3 bucket's name.
    /// </summary>
    /// <example>ExampleBucket</example>
    public string BucketName { get; set; }

    /// <summary>
    /// The key identifying the object to delete.
    /// </summary>
    /// <example>ExampleKey</example>
    public string Key { get; set; }

    /// <summary>
    /// VersionId used to reference a specific version of the object.
    /// Leave empty to use non-versioned delete operation.
    /// </summary>
    /// <example>1</example>
    public string VersionId { get; set; }
}