namespace Frends.AmazonS3.DownloadObject.Definitions;

/// <summary>
/// AWS regions.
/// </summary>
public enum Region
{
#pragma warning disable CS1591 // AWS regions. No need for specific XML.
    AfSouth1,
    ApEast1,
    ApNortheast1,
    ApNortheast2,
    ApNortheast3,
    ApSouth1,
    ApSoutheast1,
    ApSoutheast2,
    CaCentral1,
    CnNorth1,
    CnNorthWest1,
    EuCentral1,
    EuNorth1,
    EuSouth1,
    EuWest1,
    EuWest2,
    EuWest3,
    MeSouth1,
    SaEast1,
    UsEast1,
    UsEast2,
    UsWest1,
    UsWest2
#pragma warning restore CS1591
}

/// <summary>
/// Authentication methods.
/// </summary>
public enum AuthenticationMethods
{
    /// <summary>
    /// AwsAccessKeyId+AwsSecretAccessKey.
    /// </summary>
    AWSCredentials,

    /// <summary>
    /// Pre-signed URL.
    /// </summary>
    PreSignedURL
}

/// <summary>
/// Handle existing file.
/// </summary>
public enum DestinationFileExistsActions
{
    /// <summary>
    /// Overwrite file.
    /// </summary>
    Overwrite,

    /// <summary>
    /// Skip this file and add notification to return.
    /// </summary>
    Info,

    /// <summary>
    /// Stop the process.
    /// </summary>
    Error
}