using System.ComponentModel;

namespace Frends.AmazonS3.DeleteObject.Definitions;

/// <summary>
/// Input parameters.
/// </summary>
public class Input
{
    /// <summary>
    /// AWS Access Key ID.
    /// </summary>
    /// <example>AKIAQWERTY7NJ5Q7NZ6Q</example>
    [PasswordPropertyText]
    public string AwsAccessKeyId { get; set; }

    /// <summary>
    /// AWS Secret Access Key.
    /// </summary>
    /// <example>TVh5hgd3uGY/2CqH</example>
    [PasswordPropertyText]
    public string AwsSecretAccessKey { get; set; }

    /// <summary>
    /// The region to connect.
    /// </summary>
    /// <example>EuCentral1</example>
    public Region Region { get; set; }

    /// <summary>
    /// Array of objects to be deleted.
    /// </summary>
    /// <example>[ ExampleBucket, ExampleKey, 1 ], [ ExampleBucket, ExampleKey, 'empty' ]</example>
    public S3ObjectArray[] Objects { get; set; }
}