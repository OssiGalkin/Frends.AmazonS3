
namespace Frends.AmazonS3.DownloadObject.Definitions
{
    /// <summary>
    /// AWS regions.
    /// </summary>
    public enum Region
    {
        #region regions
#pragma warning disable CS1591 // AWS regions. No need for specific XML.
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
