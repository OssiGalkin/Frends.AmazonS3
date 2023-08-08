using System.Collections.Generic;

namespace Frends.AmazonS3.UploadObject.Definitions;

/// <summary>
/// Task's result.
/// </summary>
public class Result
{
    /// <summary>
    /// Upload complete.
    /// </summary>
    /// <example>true</example>
    public bool Success { get; private set; }

    /// <summary>
    /// Uploaded Object.
    /// </summary>
    /// <example>{C:\temp\ExampleFile.txt}, {temp\ExampleFile.txt}</example>
    public List<string> UploadedObjects { get; private set; }

    /// <summary>
    /// Debug log of file upload.
    /// </summary>
    /// <example>EnvironmentVariableInternalConfiguration 1|2023-02-06T08:52:17.618Z|INFO|The environment variable AWS_ENABLE_ENDPOINT_DISCOVERY was not set with a value...</example>
    public string DebugLog { get; private set; }

    internal Result(bool success, List<string> uploadedObjects, string debugLog)
    {
        Success = success;
        UploadedObjects = uploadedObjects;
        DebugLog = debugLog;
    }
}