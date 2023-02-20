using System.Collections.Generic;

namespace Frends.AmazonS3.DeleteObject.Definitions;

/// <summary>
/// Result.
/// </summary>
public class Result
{
    /// <summary>
    /// Task complete.
    /// </summary>
    /// <example>True</example>
    public bool Success { get; private set; }

    /// <summary>
    /// List of deleted objects.
    /// </summary>
    /// <example>{ true, "Key1.txt", "etZwWf8lJPf_5MuzOyFzepWqA3eS3EIN", "" }, { false, "Key2.txt", "atZwWf8lJPf_5MuzOyFzepWqA3eS3EIN", "Object ExampleKey doesn't exist in ExampleBucket." }</example>
    public List<SingleResultObject> Data { get; private set; }

    internal Result(bool success, List<SingleResultObject> data)
    {
        Success = success;
        Data = data;
    }
}