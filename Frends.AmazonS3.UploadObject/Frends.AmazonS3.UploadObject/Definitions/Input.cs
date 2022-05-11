using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Frends.AmazonS3.UploadObject;

/// <summary>
/// Input parameters.
/// </summary>
public class Input
{
    /// <summary>
    /// Source directory.
    /// </summary>
    /// <example>c:\temp, \\network\folder</example>
    [DisplayFormat(DataFormatString = "Text")]
    public string FilePath { get; set; }

    /// <summary>
    /// Windows-style filemask. Empty field = all objects (*).
    /// Only one object will be uploaded when using pre-signed URL. Consider using .zip (for example) when uploading multiple objects at the same time.
    /// </summary>
    /// <example>*.* , ?_file.*, foo_*.txt</example>
    [DisplayFormat(DataFormatString = "Text")]
    [DefaultValue("*")]
    public string FileMask { get; set; }

    /// <summary>
    /// AWS S3 root directory. If directory does not exist, it will be created.
    /// </summary>
    /// <example>directory/</example>
    [DisplayFormat(DataFormatString = "Text")]
    public string S3Directory { get; set; }

    /// <summary>
    /// Enable/disable AWS S3 access control list. 
    /// </summary>
    /// <example>false</example>
    public bool UseACL { get; set; }

    /// <summary>
    /// Access control list. Enabled when UseACL is true.
    /// </summary>
    /// <example>Private</example>
    [UIHint(nameof(UseACL), "", true)]
    public ACLs ACL { get; set; }
}
