using System;

namespace Frends.AmazonS3.UploadObject.Definitions;

/// <summary>
/// Upload exception.
/// </summary>
public class UploadException : Exception
{
    /// <summary>
    /// Debug log of object upload.
    /// </summary>
    /// <example>Some log data</example>
    public string DebugLog { get; private set; }

    internal UploadException(string debugLog, string message, Exception innerException) : base(message, innerException)
    {
        DebugLog = debugLog;
    }
}