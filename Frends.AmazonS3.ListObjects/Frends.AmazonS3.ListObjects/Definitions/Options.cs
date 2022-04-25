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
        /// Sets the maximum number of keys returned in the response. By default the action returns up to 1,000 key names.
        /// </summary>
        /// <example>1000</example>
        [DefaultValue(100)]
        [DisplayFormat(DataFormatString = "Text")]
        public int MaxKeys { get; set; }

        /// <summary>
        /// StartAfter is where you want Amazon S3 to start listing from.
        /// </summary>
        /// <example>2022-04-22T00:16:40+02:00</example>
        public string StartAfter { get; set; }
    }
}


