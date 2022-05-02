using System.Collections.Generic;

namespace Frends.AmazonS3.ListObjects.Definitions
{
    /// <summary>
    /// Result.
    /// </summary>
    public class Result
    {
        /// <summary>
        /// List of objects.
        /// </summary>
        public List<BucketObject> ObjectList { get; private set; }

        internal Result(List<BucketObject> bucketObject)
        {
            ObjectList = bucketObject;
        }
    }
}

