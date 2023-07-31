namespace Frends.AmazonS3.DeleteBucket.Definitions;

/// <summary>
/// Task's result.
/// </summary>
public class Result
{
    /// <summary>
    /// The operation is complete without errors.
    /// The operation is considered successful if the bucket to be deleted does not exist. In that case, there will be additional information in Result.Data.
    /// </summary>
    /// <example>true</example>
    public bool Success { get; private set; }

    /// <summary>
    /// Additional information, such as the 'Bucket to be deleted, does not exist'.
    /// </summary>
    /// <example>Bucket to be deleted, does not exist</example>
    public string Data { get; private set; }

    internal Result(bool success, string data)
    {
        Success = success;
        Data = data;
    }
}