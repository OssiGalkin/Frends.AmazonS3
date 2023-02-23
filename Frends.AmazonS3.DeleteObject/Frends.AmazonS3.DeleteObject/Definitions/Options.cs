using System.ComponentModel;

namespace Frends.AmazonS3.DeleteObject.Definitions;

/// <summary>
/// Options parameters.
/// </summary>
public class Options
{
    /// <summary>
    /// (Default) None: Task doesn't check if the object exists before the delete operation. Each delete operation will return versionid and success = true unless an exception occurs.
    /// Info: Task will check if the object exists before each delete operation, and nonexisting objects will be skipped. Each skipped operation will return success = false and an error message.
    /// Throw: Throw an exception if the object doesn't exists. Task will check all objects listed in Input.Objects before continuing to delete process.
    /// </summary>
    /// <example>NotExistsHandler.None</example>
    [DefaultValue(NotExistsHandler.None)]
    public NotExistsHandler NotExistsHandler { get; set; }

    /// <summary>
    /// Timeout in seconds.
    /// </summary>
    /// <example>5</example>
    public int Timeout { get; set; }
}
