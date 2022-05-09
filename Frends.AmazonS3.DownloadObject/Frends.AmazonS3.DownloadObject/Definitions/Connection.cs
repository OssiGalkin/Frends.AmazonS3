using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Frends.AmazonS3.DownloadObject.Definitions
{
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

        #region Pre-signed URL
        /// <summary>
        /// A pre-signed URL allows you to grant temporary access to users who don't have permission to directly run AWS operations in your account.
        /// </summary>
        /// <example>"https://bucket.s3.region.amazonaws.com/object/file.txt?X-Amz-Expires=120X-Amz-Algorithm...</example>
        [PasswordPropertyText]
        [UIHint(nameof(AuthenticationMethod), "", AuthenticationMethod.PreSignedURL)]
        public string PreSignedURL { get; set; }
        #endregion Pre-signed URL

        #region AWSCredentials
        /// <summary>
        /// AWS Access Key ID.
        /// </summary>
        /// <example>AKIAQWERTY7NJ5Q7NZ6Q</example>
        [PasswordPropertyText]
        [UIHint(nameof(AuthenticationMethod), "", AuthenticationMethod.AWSCredentials)]
        public string AwsAccessKeyId { get; set; }

        /// <summary>
        /// AWS Secret Access Key.
        /// </summary>
        /// <example>TVh5hgd3uGY/2CqH+Kkrrg3dadbXLsYe0jC3h+WD</example>
        [PasswordPropertyText]
        [UIHint(nameof(AuthenticationMethod), "", AuthenticationMethod.AWSCredentials)]
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
    }
}


