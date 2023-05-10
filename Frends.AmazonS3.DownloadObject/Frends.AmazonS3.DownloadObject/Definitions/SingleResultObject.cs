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
    public string ObjectName { get; private set; }

    /// <summary>
    /// Full destination path.
    /// </summary>
    /// <example>c:\temp\Filename.txt, \\network\folder\Filename.txt</example>
    public string FullPath { get; private set; }

    /// <summary>
    /// Original destination file was overwritten.
    /// </summary>
    /// <example>false</example>
    public bool Overwritten { get; private set; }

    /// <summary>
    /// Source object was marked to be deleted.
    /// </summary>
    /// <example>false</example>
    public bool SourceDeleted { get; private set; }

    /// <summary>
    /// Additional information of this object.
    /// </summary>
    /// <example>Object skipped because file already exists in destination.</example>
    public string Info { get; private set; }

    internal SingleResultObject(string objectName, string fullPath, bool overwritten, bool sourceDeleted, string info)
    {
        ObjectName = objectName;
        FullPath = fullPath;
        Overwritten = overwritten;
        SourceDeleted = sourceDeleted;
        Info = info;
    }
}