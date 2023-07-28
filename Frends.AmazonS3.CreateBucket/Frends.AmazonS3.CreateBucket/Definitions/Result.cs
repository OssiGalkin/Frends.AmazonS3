namespace Frends.AmazonS3.CreateBucket.Definitions;

/// <summary>
/// Task's result.
/// </summary>
public class Result
{
    /// <summary>
    /// Operation complete without errors.
    /// </summary>
    /// <example>true</example>
    public bool Success { get; private set; }

    /// <summary>
    /// Region the bucket resides in.
    /// Return "Bucket already exists" if bucket already exists.
    /// </summary>
    /// <example>eu-central-1</example>
    public string BucketLocation { get; private set; }

    internal Result(bool success, string bucketLocation)
    {
        Success = success;
        BucketLocation = bucketLocation;
    }
}
