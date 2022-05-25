using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Frends.AmazonS3.DownloadObject.Definitions;

/// <summary>
/// Destination parameters.
/// </summary>
public class Destination
{
    /// <summary>
    /// Actions if destination file already exists.
    /// </summary>
    /// <example>ContinueIfExists</example>
    [DefaultValue(DestinationFileExistsAction.Info)]
    public DestinationFileExistsAction DestinationFileExistsAction { get; set; }

    /// <summary>
    /// Destination directory where to create folders and files.
    /// </summary>
    /// <example>c:\temp, \\network\folder</example>
    [DisplayFormat(DataFormatString = "Text")]
    public string DestinationDirectory { get; set; }

    /// <summary>
    /// Set destination filename when downloading a single file. Leave empty to use the same name as in S3. 
    /// </summary>
    /// <example>File.txt</example>
    [DisplayFormat(DataFormatString = "Text")]
    public string DestinationFilename { get; set; }
}
