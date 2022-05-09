using System;
using System.ComponentModel;
using System.Threading;
using Frends.AmazonS3.DownloadObject.Definitions;
using System.Threading.Tasks;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using System.IO;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Linq;

namespace Frends.AmazonS3.DownloadObject
{
    /// <summary>
    /// Amazon S3 task.
    /// </summary>
    public class AmazonS3
    {
        public static async Task<Result> DownloadObject([PropertyTab] Connection connection, [PropertyTab] Input input, [PropertyTab] Options options, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(input.DestinationPath)) throw new Exception($"Destination required. {input.DestinationPath}");

            switch (connection.AuthenticationMethod)
            {
                case AuthenticationMethod.AWSCredentials:
                    if (string.IsNullOrWhiteSpace(connection.AwsAccessKeyId) || string.IsNullOrWhiteSpace(connection.AwsSecretAccessKey) || string.IsNullOrWhiteSpace(connection.BucketName) || string.IsNullOrWhiteSpace(connection.BucketName))
                        throw new Exception("AWS Access Key Id and Secret Access Key required.");
                    var resultAWS = await DownloadUtility(connection, input, options, cancellationToken, false);

                    return new Result(resultAWS);

                case AuthenticationMethod.PreSignedURL:
                    if (string.IsNullOrWhiteSpace(connection.PreSignedURL))
                        throw new Exception("AWS pre-signed URL required.");
                    if (string.IsNullOrWhiteSpace(input.FileName))
                        throw new Exception("Filename required.");
                    var resultsPre = await DownloadUtility(connection, input, options, cancellationToken, true);
                    return new Result(resultsPre);
            }
            return null;
        }


        private static async Task<List<DownloadResult>> DownloadUtility(Connection connection, Input input, Options options, CancellationToken cancellationToken, bool preSigned)
        {
            var results = new List<DownloadResult>();

            if (preSigned)
            {
                var httpClient = new HttpClient();
                var responseStream = await httpClient.GetStreamAsync(connection.PreSignedURL, cancellationToken);
                results.Add(new DownloadResult { DownloadedObject = await WriteToFilePreSigned(input.DestinationPath, input.FileName, responseStream, options) });
            }
            else
            {
                var targetPath = input.S3Directory + input.SearchPattern;
                var mask = new Regex(input.SearchPattern.Replace(".", "[.]").Replace("*", ".*").Replace("?", "."));
                var region = RegionSelection(connection.Region);
                var client = new AmazonS3Client(connection.AwsAccessKeyId, connection.AwsSecretAccessKey, region);
                using (client)
                {
                    var allObjectsResponse = await client.ListObjectsAsync(connection.BucketName, cancellationToken);
                    var allObjectsInDirectory = allObjectsResponse.S3Objects;
                    foreach (var fileObject in allObjectsInDirectory)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        if (mask.IsMatch(fileObject.Key.Split('/').Last()) && (targetPath.Split('/').Length == fileObject.Key.Split('/').Length || !options.DownloadFromCurrentDirectoryOnly) && !fileObject.Key.EndsWith("/") && fileObject.Key.StartsWith(input.S3Directory))
                        {
                            if (!input.DestinationPath.EndsWith(Path.DirectorySeparatorChar.ToString())) input.DestinationPath += Path.DirectorySeparatorChar.ToString();

                            var fullPath = Path.Combine(input.DestinationPath, fileObject.Key.Split('/').Last());
                            if (File.Exists(fullPath) && !options.Overwrite && !options.ContinueIfExists)
                            {
                                throw new Exception($"File {fileObject.Key.Split('/').Last()} already exists at {fullPath}. Set Overwrite to true from options to overwrite the file or ContinueIfExists to true to jump to next file in queue.");
                            }
                            else if (File.Exists(fullPath) && !options.Overwrite && options.ContinueIfExists)
                            {
                                results.Add(new DownloadResult
                                    {
                                        DownloadedObject = ($"File {fileObject.Key.Split('/').Last()} was skipped because it already exists at {fullPath} and ContinueIfExists is true. Set Overwrite to true from options to overwrite the file.")
                                    });
                            }
                            else
                            {
                            results.Add(new DownloadResult { DownloadedObject = await WriteToFile(connection, fileObject, client, input.DestinationPath, fullPath) });
                            if (options.DeleteSourceFile) await DeleteSourceFile(client, cancellationToken, connection.BucketName, fileObject.Key);
                            }
                        }
                    }
                }
            }

            if (results.Count == 0 && options.ThrowErrorIfNoMatch) throw new Exception($"No matches found with search pattern {input.SearchPattern}");
            return results;
        }

        private static async Task<string> WriteToFilePreSigned(string destinationPath, string fileName, Stream responseStream, Options options)
        {
            string responseBody;
            if (!destinationPath.EndsWith(Path.DirectorySeparatorChar.ToString())) destinationPath += Path.DirectorySeparatorChar.ToString();
            var fullPath = $@"{destinationPath}{fileName}";

            using (var reader = new StreamReader(responseStream)) responseBody = await reader.ReadToEndAsync();


            if (File.Exists(fullPath) && !options.Overwrite)
            {
                throw new Exception($"File already exists at {fullPath}. Set Overwrite to true from options to overwrite the file.");
            }
            if (File.Exists(fullPath) && options.Overwrite)
            {
                var file = new FileInfo(fullPath);

                while (IsFileLocked(file)) Thread.Sleep(1000);
                File.Delete(fullPath);
                File.WriteAllText(fullPath, responseBody);
                return $@"Download complete: {fullPath}";
            }
            else
            {
                if (!Directory.Exists(destinationPath)) Directory.CreateDirectory(destinationPath);
                File.WriteAllText(fullPath, responseBody);
                return $@"Download complete: {fullPath}";
            }
        }

        private static async Task<string> WriteToFile(Connection connection, S3Object fileObject, AmazonS3Client s3Client, string destinationFolder, string fullPath)
        {
            string responseBody;
            var request = new GetObjectRequest
            {
                BucketName = connection.BucketName,
                Key = fileObject.Key
            };

            using (var response = await s3Client.GetObjectAsync(request))
            using (var responseStream = response.ResponseStream)
            using (var reader = new StreamReader(responseStream)) responseBody = await reader.ReadToEndAsync();
            if (File.Exists($@"{fullPath}"))
            {
                File.Delete($@"{fullPath}");
                File.WriteAllText(fullPath, responseBody);
                return $@"Download complete: {fullPath}";
            }
            else
            {
                if (!Directory.Exists(destinationFolder)) Directory.CreateDirectory(destinationFolder);
                File.WriteAllText(fullPath, responseBody);
                return $@"Download complete: {fullPath}";
            }
        }

        private static async Task<string> DeleteSourceFile(AmazonS3Client client, CancellationToken cancellationToken, string bucketName, string key)
        {
            try
            {
                var deleteObjectRequest = new DeleteObjectRequest //delete source file from S3
                {
                    BucketName = bucketName,
                    Key = key
                };

                await client.DeleteObjectAsync(deleteObjectRequest, cancellationToken);
                return $@"Source file {key} deleted.";
            }
            catch (Exception ex) { throw new Exception($"Delete failed. {ex}"); }
        }

        private static bool IsFileLocked(FileInfo file)
        {
            FileStream stream = null;

            try
            {
                stream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            }
            catch (Exception)
            {
                // The file is unavailable because it is:
                // 1. Still being written to.
                // 2. Being processed by another thread.
                // 3. Does not exist (has already been processed).
                return true;
            }
            finally { stream?.Close(); }

            // File is not locked.
            return false;
        }

        private static RegionEndpoint RegionSelection(Region region)
        {
            return region switch
            {
                Region.AfSouth1 => RegionEndpoint.AFSouth1,
                Region.ApEast1 => RegionEndpoint.APEast1,
                Region.ApNortheast1 => RegionEndpoint.APNortheast1,
                Region.ApNortheast2 => RegionEndpoint.APNortheast2,
                Region.ApNortheast3 => RegionEndpoint.APNortheast3,
                Region.ApSouth1 => RegionEndpoint.APSouth1,
                Region.ApSoutheast1 => RegionEndpoint.APSoutheast1,
                Region.ApSoutheast2 => RegionEndpoint.APSoutheast2,
                Region.CaCentral1 => RegionEndpoint.CACentral1,
                Region.CnNorth1 => RegionEndpoint.CNNorth1,
                Region.CnNorthWest1 => RegionEndpoint.CNNorthWest1,
                Region.EuCentral1 => RegionEndpoint.EUCentral1,
                Region.EuNorth1 => RegionEndpoint.EUNorth1,
                Region.EuSouth1 => RegionEndpoint.EUSouth1,
                Region.EuWest1 => RegionEndpoint.EUWest1,
                Region.EuWest2 => RegionEndpoint.EUWest2,
                Region.EuWest3 => RegionEndpoint.EUWest3,
                Region.MeSouth1 => RegionEndpoint.MESouth1,
                Region.SaEast1 => RegionEndpoint.SAEast1,
                Region.UsEast1 => RegionEndpoint.USEast1,
                Region.UsEast2 => RegionEndpoint.USEast2,
                Region.UsWest1 => RegionEndpoint.USWest1,
                Region.UsWest2 => RegionEndpoint.USWest2,
                _ => RegionEndpoint.EUWest1,
            };
        }
    }
}
