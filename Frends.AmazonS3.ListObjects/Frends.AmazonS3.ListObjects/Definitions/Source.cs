using System.ComponentModel;
namespace Frends.AmazonS3.ListObjects.Definitions
{
    /// <summary>
    /// Source values.
    /// </summary>
    public class Source
    {
        /// <summary>
        /// The name of the bucket containing this object.
        /// </summary>
        /// <example>ObjectName</example>
        public string BucketName { get; set; }

        /// <summary>
        /// AWS Access Key ID.
        /// </summary>
        /// <example>AKIAQWERTY7NJ5Q7NZ6Q</example>
        [PasswordPropertyText(true)]
        public string AwsAccessKeyId { get; set; }

        /// <summary>
        /// AWS Secret Access Key.
        /// </summary>
        /// <example>TVh5hgd3uGY/2CqH+Kkrrg3dadbXLsYe0jC3h+WD</example>
        [PasswordPropertyText(true)]
        public string AwsSecretAccessKey { get; set; }

        /// <summary>
        /// AWS Region selection.
        /// </summary>
        /// <example>EuCentral1</example>
        public Regions Region { get; set; }
    }
}


