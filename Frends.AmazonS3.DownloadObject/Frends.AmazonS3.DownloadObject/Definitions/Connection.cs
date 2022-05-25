using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Frends.AmazonS3.DownloadObject.Definitions;

/// <summary>
/// Connection parameters.
/// </summary>
public class Connection
{
    /// <summary>
    /// Authentication method to use when connecting to AWS S3 bucket. Options are pre-signed URL or AWS Access Key ID+AWS Secret Access Key.
    /// </summary>
    /// <example>AWSCredentials</example>
    [DefaultValue(AuthenticationMethod.AWSCredentials)]
    public AuthenticationMethod AuthenticationMethod { get; set; }

    /// <summary>
    /// A pre-signed URL allows you to grant temporary access to users who don't have permission to directly run AWS operations in your account.
    /// </summary>
    /// <example>"https://bucket.s3.region.amazonaws.com/object/file.txt?X...</example>
    [PasswordPropertyText]
    [UIHint(nameof(AuthenticationMethod), "", AuthenticationMethod.PreSignedURL)]
    [DefaultValue(null)]
    public string PreSignedURL { get; set; }

    #region AWSCredentials
    /// <summary>
    /// AWS Access Key ID.
    /// </summary>
    /// <example>AKIAQWERTY7NJ5Q7NZ6Q</example>
    [PasswordPropertyText]
    [UIHint(nameof(AuthenticationMethod), "", AuthenticationMethod.AWSCredentials)]
    [DefaultValue(null)]
    public string AwsAccessKeyId { get; set; }

    /// <summary>
    /// AWS Secret Access Key.
    /// </summary>
    /// <example>TVh5hgd3uGY/2CqH</example>
    [PasswordPropertyText]
    [UIHint(nameof(AuthenticationMethod), "", AuthenticationMethod.AWSCredentials)]
    [DefaultValue(null)]
    public string AwsSecretAccessKey { get; set; }

    /// <summary>
    /// AWS S3 bucket's name.
    /// </summary>
    /// <example>Bucket</example>
    [UIHint(nameof(AuthenticationMethod), "", AuthenticationMethod.AWSCredentials)]
    public string BucketName { get; set; }

    /// <summary>
    /// AWS S3 bucket's region.
    /// </summary>
    /// <example>EuCentral1</example>
    [UIHint(nameof(AuthenticationMethod), "", AuthenticationMethod.AWSCredentials)]
    public Region Region { get; set; }
    #endregion AWSCredentials

    /// <summary>
    /// Downloads all objects with this prefix. Enabled when using AWSCredentials.
    /// </summary>
    /// <example>directory/</example>
    [UIHint(nameof(AuthenticationMethod), "", AuthenticationMethod.AWSCredentials)]
    [DisplayFormat(DataFormatString = "Text")]
    public string S3Directory { get; set; }

    /// <summary>
    /// String pattern to search objects. Enabled when using AWSCredentials.
    /// </summary>
    /// <example>*.*, *file?.txt</example>
    [UIHint(nameof(AuthenticationMethod), "", AuthenticationMethod.AWSCredentials)]
    [DisplayFormat(DataFormatString = "Text")]
    [DefaultValue("*")]
    public string SearchPattern { get; set; }

    /// <summary>
    /// Set to true to download objects from current directory only. Enabled when using AWSCredentials.
    /// </summary>
    /// <example>false</example>
    [UIHint(nameof(AuthenticationMethod), "", AuthenticationMethod.AWSCredentials)]
    public bool DownloadFromCurrentDirectoryOnly { get; set; }

    /// <summary>
    /// Delete S3 source object after download. Subfolders will also be deleted if they are part of the object's key and there are no objects left. Create subfolders manually to make sure they won't be deleted. 
    /// Enabled when using AWSCredentials.
    /// </summary>
    /// <example>false</example>
    [UIHint(nameof(AuthenticationMethod), "", AuthenticationMethod.AWSCredentials)]
    public bool DeleteSourceObject { get; set; }

    /// <summary>
    /// Throw an error if there are no objects in the path matching the search pattern. Enabled when using AWSCredentials.
    /// </summary>
    /// <example>false</example>
    [UIHint(nameof(AuthenticationMethod), "", AuthenticationMethod.AWSCredentials)]
    public bool ThrowErrorIfNoMatch { get; set; }
}
