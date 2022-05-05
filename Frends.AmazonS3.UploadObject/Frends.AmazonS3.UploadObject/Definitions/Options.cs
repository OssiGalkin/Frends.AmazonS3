using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Frends.AmazonS3.UploadObject.Definitions
{
    /// <summary>
    /// Upload options.
    /// </summary>
    
    public class Options
    {
        /// <summary>
        /// Filter options by authentication method.
        /// </summary>
        /// <example>AWS Credentials</example>
        [DefaultValue(AuthenticationMethod.AWSCredentials)]
        public OptionsFor OptionsFor { get; set; }

        /// <summary>
        /// Set to true to upload object(s) from current directory only.
        /// </summary>
        /// <example>false</example>
        public bool UploadFromCurrentDirectoryOnly { get; set; }

        /// <summary>
        /// Set to true to create subdirectories to S3 bucket.
        /// Requires UploadFromCurrentDirectoryOnly = false and AWS credentials-authentication method.
        /// </summary>
        /// <example>false</example>
        [UIHint(nameof(OptionsFor), "", OptionsFor.AWSCredentials)]
        public bool PreserveFolderStructure { get;  set; }

        /// <summary>
        /// Return object keys from S3 in prefix/filename-format.
        /// </summary>
        /// <example>false</example>
        [UIHint(nameof(OptionsFor), "", OptionsFor.AWSCredentials)]
        public bool ReturnListOfObjectKeys { get;  set; }

        /// <summary>
        /// Set to true to overwrite object(s) with the same path and name (object key).
        /// </summary>
        /// <example>false</example>
        [UIHint(nameof(OptionsFor), "", OptionsFor.AWSCredentials)]
        public bool Overwrite { get;  set; }

        /// <summary>
        /// Delete local source object(s) after upload.
        /// </summary>
        /// <example>false</example>
        public bool DeleteSource { get;  set; }

        /// <summary>
        /// Throw error if there are no object(s) in the path matching the filemask.
        /// </summary>
        public bool ThrowErrorIfNoMatch { get; set; }
    }
}

