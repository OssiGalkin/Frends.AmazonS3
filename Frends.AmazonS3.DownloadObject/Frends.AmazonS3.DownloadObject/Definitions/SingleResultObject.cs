namespace Frends.AmazonS3.DownloadObject.Definitions;

/// <summary>
/// Single download result.
/// </summary>
public class SingleResultObject
{
    /// <summary>
    /// Object's name.
    /// </summary>
    /// <example>Filename.txt</example>
    public string ObjectName { get; set; }

    /// <summary>
    /// Full destination path.
    /// </summary>
    /// <example>c:\temp\Filename.txt, \\network\folder\Filename.txt</example>
    public string FullPath { get; set; }

    /// <summary>
    /// Original destination file was overwritten.
    /// </summary>
    /// <example>false</example>
    public bool Overwritten { get; set; }

    /// <summary>
    /// Source object was deleted.
    /// </summary>
    /// <example>false</example>
    public bool SourceDeleted { get; set; }

    /// <summary>
    /// Additional information of this object.
    /// </summary>
    /// <example>Object skipped because file already exists in destination.</example>
    public string Info { get; set; }
}
