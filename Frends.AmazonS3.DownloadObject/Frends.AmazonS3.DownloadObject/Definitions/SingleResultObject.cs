namespace Frends.AmazonS3.DownloadObject.Definitions
{
    /// <summary>
    /// Single download result.
    /// </summary>
    public class SingleResultObject
    {
        /// <summary>
        /// Single download result.
        /// </summary>
        /// <example>Download complete: {fullPath}.</example>
        public string ObjectData { get; private set; }

        internal SingleResultObject(string objectData)
        {
            ObjectData = objectData;
        }
    }
}