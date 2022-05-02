using System;

namespace Frends.AmazonS3.ListObjects.Definitions
{
    /// <summary>
    /// Object data.
    /// </summary>
    public class BucketObject
    {
        /// <summary>
        /// The name of the bucket containing this object.
        /// </summary>
        /// <example>ObjectName</example>
        public string BucketName { get; set; }

        /// <summary>
        /// The key of the object.
        /// </summary>
        /// <example>ObjectDir/ObjectName</example>
        public string Key { get; set; }

        /// <summary>
        /// Entity tag.
        /// <example>2b9338cfb801ca193ee45d49acc2ba99</example>
        /// </summary>
        public string Etag { get; set; }

        /// <summary>
        /// The size of the object.
        /// </summary>
        /// <example>20110</example>
        public long Size { get; set; }

        /// <summary>
        /// The date and time the object was last modified.
        /// </summary>
        /// <example>2022-04-22T00:16:40+02:00</example>
        public DateTime LastModified { get; set; }

    }
}

