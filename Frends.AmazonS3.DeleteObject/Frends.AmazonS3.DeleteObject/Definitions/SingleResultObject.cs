namespace Frends.AmazonS3.DeleteObject.Definitions;

/// <summary>
/// Single delete operation result.
/// </summary>
public class SingleResultObject
{
    /// <summary>
    /// Operation complete without errors.
    /// </summary>
    /// <example>true</example>
    public bool Success { get; set; }

    /// <summary>
    /// The key identifying the deleted object.
    /// </summary>
    /// <example>ExampleKey</example>
    public string Key { get; set; }

    /// <summary>
    /// Returns the version ID of the delete marker created as a result of the DELETE operation.
    /// </summary>
    /// <example>q97fnr1zy_gsDcPAMbbwoW2eY0wgoFPt</example>
    public string VersionId { get; set; }

    /// <summary>
    /// Error message.
    /// </summary>
    /// <example>Object ExampleKey doesn't exist in ExampleBucket.</example>
    public string Error { get; set; }
}
