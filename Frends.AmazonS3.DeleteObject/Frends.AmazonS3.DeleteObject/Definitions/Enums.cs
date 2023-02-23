namespace Frends.AmazonS3.DeleteObject.Definitions;

/// <summary>
/// AWS regions.
/// </summary>
public enum Region
{
#pragma warning disable CS1591 // Self explanatory.
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
/// How to handle nonexisting object.
/// </summary>
public enum NotExistsHandler
{
    /// <summary>
    /// Task doesn't check if the object exists before the delete operation. Each delete operation will return versionid and success = true unless an exception occurs. 
    /// </summary>
    None,

    /// <summary>
    /// Each nonexisting object will return Success = false and an error message.
    /// </summary>
    Info,

    /// <summary>
    /// Throw an exception if the object doesn't exists. Task will check all objects listed in Input.Objects before continuing to delete process.
    /// </summary>
    Throw
}