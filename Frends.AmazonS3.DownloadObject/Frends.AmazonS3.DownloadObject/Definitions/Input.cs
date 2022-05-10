using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Frends.AmazonS3.DownloadObject.Definitions
{
    /// <summary>
    /// Connection and destination parameters.
    /// </summary>
    public class Input
    {
        /// <summary>
        /// Authentication method to use when connecting to AWS S3 bucket. Options are pre-signed URL or AWS Access Key ID+AWS Secret Access Key.
        /// </summary>
        /// <example>AWSCredentials</example>
        [DefaultValue(AuthenticationMethod.AWSCredentials)]
        public AuthenticationMethod AuthenticationMethod { get; set; }

        #region Pre-signed URL
        /// <summary>
        /// A pre-signed URL allows you to grant temporary access to users who don't have permission to directly run AWS operations in your account.
        /// </summary>
        /// <example>"https://bucket.s3.region.amazonaws.com/object/file.txt?X...</example>
        [PasswordPropertyText]
        [UIHint(nameof(AuthenticationMethod), "", AuthenticationMethod.PreSignedURL)]
        [DefaultValue(null)]
        public string PreSignedURL { get; set; }
        #endregion Pre-signed URL

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

        #region Options
        /// <summary>
        /// Downloads all objects with this prefix. Enabled when using AWSCredentials.
        /// </summary>
        /// <example>directory/</example>
        [UIHint(nameof(AuthenticationMethod), "", AuthenticationMethod.AWSCredentials)]
        [DisplayFormat(DataFormatString = "Text")]
        public string S3Directory { get; set; }

        /// <summary>
        /// String pattern to search files. Enabled when using AWSCredentials.
        /// </summary>
        /// <example>*.*, *file?.txt</example>
        [UIHint(nameof(AuthenticationMethod), "", AuthenticationMethod.AWSCredentials)]
        [DisplayFormat(DataFormatString = "Text")]
        [DefaultValue("*")]
        public string SearchPattern { get; set; }

        /// <summary>
        /// Directory where to create folders and files.
        /// </summary>
        /// <example>c:\temp, \\network\folder</example>
        [DisplayFormat(DataFormatString = "Text")]
        public string DestinationPath { get; set; }

        /// <summary>
        /// Set filename (to destination). Enabled when using PreSignedURL.
        /// </summary>
        /// <example>File.txt</example>
        [UIHint(nameof(AuthenticationMethod), "", AuthenticationMethod.PreSignedURL)]
        [DisplayFormat(DataFormatString = "Text")]
        public string FileName { get; set; }

        /// <summary>
        /// Set to true to download files from current directory only. Enabled when using AWSCredentials.
        /// </summary>
        /// <example>false</example>
        [UIHint(nameof(AuthenticationMethod), "", AuthenticationMethod.AWSCredentials)]
        public bool DownloadFromCurrentDirectoryOnly { get; set; }

        /// <summary>
        /// Delete S3 source files after download. Enabled when using AWSCredentials and DownloadFromCurrentDirectoryOnly is true.
        /// </summary>
        /// <example>false</example>
        [UIHint(nameof(DownloadFromCurrentDirectoryOnly), "", true)]
        public bool DeleteSourceFile { get; set; }

        /// <summary>
        /// Throw an error if there are no objects in the path matching the search pattern. Enabled when using AWSCredentials.
        /// </summary>
        /// <example>false</example>
        [UIHint(nameof(AuthenticationMethod), "", AuthenticationMethod.AWSCredentials)]
        public bool ThrowErrorIfNoMatch { get; set; }

        /// <summary>
        /// If the file already exists in destination, add an error message to result and continue downloading next file in queue.
        /// </summary>
        /// <example>false</example>
        [UIHint(nameof(AuthenticationMethod), "", AuthenticationMethod.AWSCredentials)]
        public bool ContinueIfExists { get; set; }

        /// <summary>
        /// Set to true to overwrite objects with the same path and name (object key).
        /// </summary>
        /// <example>false</example>
        public bool Overwrite { get; set; }

        #endregion Options
    }
}