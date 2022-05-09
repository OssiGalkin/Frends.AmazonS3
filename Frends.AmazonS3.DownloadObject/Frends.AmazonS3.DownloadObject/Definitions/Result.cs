using System.Collections.Generic;

namespace Frends.AmazonS3.DownloadObject.Definitions
{
    /// <summary>
    /// Result.
    /// </summary>
    public class Result
    {
        /// <summary>
        /// List of download result(s).
        /// </summary>
        public List<DownloadResult> Results { get; private set; }

        internal Result(List<DownloadResult> downloadResult)
        {
            Results = downloadResult;
        }
    }
}

