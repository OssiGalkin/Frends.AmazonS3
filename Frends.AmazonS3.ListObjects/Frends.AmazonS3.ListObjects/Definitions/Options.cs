using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Frends.AmazonS3.ListObjects.Definitions
{
    /// <summary>
    /// Options for the task.
    /// </summary>
    public class Options
    {
        /// <summary>
        /// Limits the response to keys that begin with the specified prefix.
        /// </summary>
        /// <example>bucket/object1/object2</example>
        [DefaultValue(null)]
        public string Prefix { get; set; }

        /// <summary>
        /// A delimiter is a character you use to group keys. See: http://docs.aws.amazon.com/AmazonS3/latest/dev/ListingKeysHierarchy.html
        /// </summary>
        /// <example>/</example>
        [DefaultValue(null)]
        public string Delimiter { get; set; }

        /// <summary>
        /// Sets the maximum number of keys returned in the response. Returns up to 1,000 key names. The response might contain fewer keys but will never contain more.
        /// </summary>
        /// <example>1000</example>
        [DefaultValue(1000)]
        public int MaxKeys { get; set; }

        /// <summary>
        /// StartAfter is where you want Amazon S3 to start listing from. 
        /// Eg. In a bucket of objects { 20220401, 20220402, 20220403 }, StartAfter=20220401 returns objects 20220402 and 20220403.
        /// </summary>
        /// <example>20220401</example>
        public string StartAfter { get; set; }
    }
}


