
namespace Frends.AmazonS3.UploadObject.Definitions
{
    /// <summary>
    /// AWS regions.
    /// </summary>
    public enum Region
    {
        #region regions
        #pragma warning disable CS1591 // AWS region(s). No need for specific XML.
        AfSouth1,
        ApEast1,
        ApNortheast1,
        ApNortheast2,
        ApNortheast3,
        ApSouth1,
        ApSoutheast1,
        ApSoutheast2,
        CaCentral1,
        CnNorth1,
        CnNorthWest1,
        EuCentral1,
        EuNorth1,
        EuSouth1,
        EuWest1,
        EuWest2,
        EuWest3,
        MeSouth1,
        SaEast1,
        UsEast1,
        UsEast2,
        UsWest1,
        UsWest2
        #pragma warning restore CS1591
        #endregion regions
    }

    /// <summary>
    /// Authentication methods.
    /// </summary>
    public enum AuthenticationMethod
    {
        #region AuthMethods
        /// <summary>
        /// AwsAccessKeyId+AwsSecretAccessKey.
        /// </summary>
        AWSCredentials,

        /// <summary>
        /// Pre-signed URL.
        /// </summary>
        PreSignedURL
        #endregion AuthMethods
    }

    /// <summary>
    /// Access control list.
    /// </summary>
    public enum ACLs
    {
        #region ACLs
        /// <summary>
        /// Owner gets FULL_CONTROL. No one else has access rights (default).
        /// </summary>
        Private,

        /// <summary>
        /// Owner gets FULL_CONTROL. The AllUsers group (see Who is a grantee?) gets READ access.
        /// </summary>
        PublicRead,

        /// <summary>
        /// Owner gets FULL_CONTROL. The AllUsers group gets READ and WRITE access. Granting this on a bucket is generally not recommended.
        /// </summary>
        PublicReadWrite,

        /// <summary>
        /// Owner gets FULL_CONTROL. Amazon EC2 gets READ access to GET an Amazon Machine Image (AMI) bundle from Amazon S3.
        /// </summary>
        AuthenticatedRead,

        /// <summary>
        /// Owner gets FULL_CONTROL. The AuthenticatedUsers group gets READ access.
        /// </summary>
        BucketOwnerRead,

        /// <summary>
        /// Both the object owner and the bucket owner get FULL_CONTROL over the object. If you specify this canned ACL when creating a bucket, Amazon S3 ignores it.
        /// </summary>
        BucketOwnerFullControl,

        /// <summary>
        /// The LogDelivery group gets WRITE and READ_ACP permissions on the bucket. For more information about logs, see (Logging requests using server access logging).
        /// </summary>
        LogDeliveryWrite
        #endregion ACLs
    }

    /// <summary>
    /// Filter options by authentication method.
    /// </summary>
    public enum OptionsFor
    {
        #region OptionsFor
        /// <summary>
        /// AwsAccessKeyId+AwsSecretAccessKey.
        /// </summary>
        AWSCredentials,

        /// <summary>
        /// Pre-signed URL.
        /// </summary>
        PreSignedURL
        #endregion OptionsFor
    }
}
