using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Frends.AmazonS3.DownloadObject.Definitions
{
    /// <summary>
    /// Download options.
    /// </summary>

    public class Options
    {
        /// <summary>
        /// Filter options by authentication method.
        /// </summary>
        /// <example>AWS Credentials</example>
        [DefaultValue(OptionsFor.AWSCredentials)]
        public OptionsFor OptionsFor { get; set; }

        /// <summary>
        /// Set to true to download files from current directory only.
        /// </summary>
        /// <example>false</example>
        [UIHint(nameof(OptionsFor), "", OptionsFor.AWSCredentials)]
        public bool DownloadFromCurrentDirectoryOnly { get; set; }

        /// <summary>
        /// Delete S3 source files after download. Only avaible when DownloadFromCurrentDirectoryOnly is true. Doesn't delete subfolders.
        /// </summary>
        /// <example>false</example>
        [UIHint(nameof(DownloadFromCurrentDirectoryOnly), "", true)]
        public bool DeleteSourceFile { get; set; }


        /// <summary>
        /// Throw an error if there are no object(s) in the path matching the search pattern.
        /// </summary>
        /// <example>false</example>
        [UIHint(nameof(OptionsFor), "", OptionsFor.AWSCredentials)]
        public bool ThrowErrorIfNoMatch { get; set; }

        /// <summary>
        /// If the file already exists in destination, add an error message to result and continue downloading next file in queue.
        /// </summary>
        /// <example>false</example>
        [UIHint(nameof(OptionsFor), "", OptionsFor.AWSCredentials)]
        public bool ContinueIfExists { get; set; }

        /// <summary>
        /// Set to true to overwrite object(s) with the same path and name (object key).
        /// </summary>
        /// <example>false</example>
        public bool Overwrite { get; set; }
    }
}

