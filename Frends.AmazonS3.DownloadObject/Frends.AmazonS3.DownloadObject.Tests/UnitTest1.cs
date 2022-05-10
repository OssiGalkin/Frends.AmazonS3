using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using Amazon.S3;
using Amazon;
using Amazon.S3.Model;
using Frends.AmazonS3.DownloadObject.Definitions;

namespace Frends.AmazonS3.DownloadObject.Tests
{
    [TestClass]
    public class UnitTest1
    {
        private readonly string? _accessKey = Environment.GetEnvironmentVariable("HiQ_AWSS3Test_AccessKey");
        private readonly string? _secretAccessKey = Environment.GetEnvironmentVariable("HiQ_AWSS3Test_SecretAccessKey");
        private readonly string? _bucketName = Environment.GetEnvironmentVariable("HiQ_AWSS3Test_BucketName");
        private readonly string _dir = Path.Combine(Environment.CurrentDirectory); // .\Frends.AmazonS3.DownloadObject\Frends.AmazonS3.DownloadObject.Test\bin\Debug\net6.0\

        Input? _input;


        /// <summary>
        /// Create test files and folders. 
        /// !!! Manually upload test files to S3 "2022/"-folder. You can copy files from ".\Frends.AmazonS3.DownloadObject\Frends.AmazonS3.DownloadObject.Test\bin\Debug\net6.0\DownloadTestFiles" after first test run. !!!
        /// </summary>
        [TestInitialize]
        public void Initialize()
        {
            CreateTestFiles();
        }

        /// <summary>
        /// Delete test files and folders. CleanUp after each test to make sure overwriten file(s) won't make other tests fail.
        /// </summary>
        [TestCleanup]
        public void CleanUp()
        {
            Directory.Delete($@"{_dir}\Download", true);
        }

        #region pre-signed URL
        /// <summary>
        /// Download with pre-signed URL.
        /// Result is not null and file Testfile.txt existed before CleanUp().
        /// </summary>
        [TestMethod]
        public void PreSignedURLTest()
        {
            var setS3Key = $"2022/Testfile.txt"; //S3 key.

            _input = new Input()
            {
                AuthenticationMethod = AuthenticationMethod.PreSignedURL,
                PreSignedURL = CreatePreSignedURL(setS3Key),
                DestinationPath = @$"{_dir}\Download",
                FileName = "Testfile.txt",
            };

            var result = AmazonS3.DownloadObject(_input, default);
            Assert.IsNotNull(result.Result.Results);
            Assert.IsTrue(File.Exists(@$"{_dir}\Download\Testfile.txt"));
            Assert.IsTrue(File.Exists(@$"{_dir}\Download\Overwrite.txt"));
        }

        /// <summary>
        /// Missing pre-signed URL.
        /// </summary>
        [TestMethod]
        public void PreSignedURLMissingURLTest()
        {
            var setS3Key = $"2022/Testfile.txt"; //S3 key.

            _input = new Input()
            {
                AuthenticationMethod = AuthenticationMethod.PreSignedURL,
                PreSignedURL = "",
                DestinationPath = @$"{_dir}\Download",
                FileName = "Testfile.txt",
            };

            var ex = Assert.ThrowsExceptionAsync<Exception>(async () => await AmazonS3.DownloadObject(_input, default)).Result;
            Assert.IsTrue(ex.Message.Contains("AWS pre-signed URL required."));
        }

        /// <summary>
        /// Missing destination path.
        /// </summary>
        [TestMethod]
        public void PreSignedMissingDestinationPathTest()
        {
            var setS3Key = $"2022/Testfile.txt"; //S3 key.

            _input = new Input()
            {
                AuthenticationMethod = AuthenticationMethod.PreSignedURL,
                PreSignedURL = CreatePreSignedURL(setS3Key),
                DestinationPath = "",
                FileName = "Testfile.txt",
            };

            var ex = Assert.ThrowsExceptionAsync<Exception>(async () => await AmazonS3.DownloadObject(_input, default)).Result;
            Assert.IsTrue(ex.Message.Contains("Destination required."));
        }

        /// <summary>
        /// File already exists in destination. Overwrite and ContinueIfExists = false.
        /// </summary>
        [TestMethod]
        public void PreSignedNoOverwriteContinueIfExistsTest()
        {
            var setS3Key = $"2022/Overwrite.txt"; //S3 key.

            _input = new Input()
            {
                AuthenticationMethod = AuthenticationMethod.PreSignedURL,
                PreSignedURL = CreatePreSignedURL(setS3Key),
                DestinationPath = @$"{_dir}\Download",
                FileName = "Overwrite.txt",
                Overwrite = false,
                ContinueIfExists = false,
            };

            var ex = Assert.ThrowsExceptionAsync<Exception>(async () => await AmazonS3.DownloadObject(_input, default)).Result;
            Assert.IsTrue(ex.Message.Contains("File already exists at"));
        }

        /// <summary>
        /// Result is not null, file exists and was overwriten (compare lines).
        /// </summary>
        [TestMethod]
        public void PreSignedURLOverwriteTest()
        {
            var setS3Key = $"2022/Overwrite.txt"; //S3 key.

            _input = new Input()
            {
                AuthenticationMethod = AuthenticationMethod.PreSignedURL,
                PreSignedURL = CreatePreSignedURL(setS3Key),
                DestinationPath = @$"{_dir}\Download",
                FileName = "Overwrite.txt",
                Overwrite = true,
                ContinueIfExists = false,
            };

            var result = AmazonS3.DownloadObject(_input, default);
            Assert.IsNotNull(result.Result.Results);
            Assert.IsTrue(File.Exists(@$"{_dir}\Download\Overwrite.txt"));
            Assert.IsTrue(CompareFiles());
        }

        #endregion pre-signed URL

        #region aws creds
        /// <summary>
        /// AWS creds get all keys.
        /// </summary>
        [TestMethod]
        public void AWSCredsAllKeysTest()
        {
            Directory.Delete($@"{_dir}\Download", true);

            _input = new Input()
            {
                AuthenticationMethod = AuthenticationMethod.AWSCredentials,
                AwsAccessKeyId = _accessKey,
                AwsSecretAccessKey = _secretAccessKey,
                BucketName = _bucketName,
                Region = Region.EuCentral1,
                DestinationPath = @$"{_dir}\Download",
                S3Directory = "2022/",
                SearchPattern = "*",
                DeleteSourceFile = false,
                DownloadFromCurrentDirectoryOnly = false,
                Overwrite = false,
                ThrowErrorIfNoMatch = false,
            };

            var result = AmazonS3.DownloadObject(_input, default);
            Assert.IsNotNull(result.Result.Results);
            Assert.IsTrue(File.Exists(@$"{_dir}\Download\Overwrite.txt"));
            Assert.IsTrue(File.Exists(@$"{_dir}\Download\Testfile.txt"));
            Assert.IsTrue(File.Exists(@$"{_dir}\Download\DownloadFromCurrentDirectoryOnly.txt"));
        }

        /// <summary>
        /// Missing AWS creds.
        /// </summary>
        [TestMethod]
        public void AWSCredsMissingTest()
        {
            _input = new Input()
            {
                AuthenticationMethod = AuthenticationMethod.AWSCredentials,
                AwsAccessKeyId = "",
                AwsSecretAccessKey = _secretAccessKey,
                BucketName = _bucketName,
                Region = Region.EuCentral1,
                DestinationPath = @$"{_dir}\Download",
                S3Directory = "2022/",
                SearchPattern = "*",
                DeleteSourceFile = false,
                DownloadFromCurrentDirectoryOnly = false,
                Overwrite = false,
                ThrowErrorIfNoMatch = false,
            };

            var ex = Assert.ThrowsExceptionAsync<Exception>(async () => await AmazonS3.DownloadObject(_input, default)).Result;
            Assert.IsTrue(ex.Message.Contains("AWS Access Key Id and Secret Access Key required."));
        }

        /// <summary>
        /// ThrowErrorIfNoMatch = true.
        /// </summary>
        [TestMethod]
        public void AWSCredsSearchPatternTest()
        {
            _input = new Input()
            {
                AuthenticationMethod = AuthenticationMethod.AWSCredentials,
                AwsAccessKeyId = _accessKey,
                AwsSecretAccessKey = _secretAccessKey,
                BucketName = _bucketName,
                Region = Region.EuCentral1,
                DestinationPath = @$"{_dir}\Download",
                S3Directory = "2022/",
                SearchPattern = "Test*.txt",
                ThrowErrorIfNoMatch = true,
                DeleteSourceFile = false,
                DownloadFromCurrentDirectoryOnly = false,
                Overwrite = true,
            };

            var result = AmazonS3.DownloadObject(_input, default);
            Assert.IsNotNull(result.Result.Results);
            Assert.IsTrue(File.Exists(@$"{_dir}\Download\Testfile.txt"));
        }

        /// <summary>
        /// ThrowErrorIfNoMatch = true.
        /// </summary>
        [TestMethod]
        public void AWSCredsThrowErrorIfNoMatchTest()
        {
            _input = new Input()
            {
                AuthenticationMethod = AuthenticationMethod.AWSCredentials,
                AwsAccessKeyId = _accessKey,
                AwsSecretAccessKey = _secretAccessKey,
                BucketName = _bucketName,
                Region = Region.EuCentral1,
                DestinationPath = @$"{_dir}\Download",
                S3Directory = "2022/",
                SearchPattern = "NoFile*",
                ThrowErrorIfNoMatch = true,
                DeleteSourceFile = false,
                DownloadFromCurrentDirectoryOnly = false,
                Overwrite = true,
            };

            var ex = Assert.ThrowsExceptionAsync<Exception>(async () => await AmazonS3.DownloadObject(_input, default)).Result;
            Assert.IsTrue(ex.Message.Contains("No matches found with search pattern"));
        }

        /// <summary>
        /// Missing destination path.
        /// </summary>
        [TestMethod]
        public void AWSCredsDestinationMissingTest()
        {
            _input = new Input()
            {
                AuthenticationMethod = AuthenticationMethod.AWSCredentials,
                AwsAccessKeyId = _accessKey,
                AwsSecretAccessKey = _secretAccessKey,
                BucketName = _bucketName,
                Region = Region.EuCentral1,
                DestinationPath = "",
                S3Directory = "2022/",
                SearchPattern = "*",
                DeleteSourceFile = false,
                DownloadFromCurrentDirectoryOnly = false,
                Overwrite = false,
                ThrowErrorIfNoMatch = false,
            };

            var ex = Assert.ThrowsExceptionAsync<Exception>(async () => await AmazonS3.DownloadObject(_input, default)).Result;
            Assert.IsTrue(ex.Message.Contains("Destination required."));
        }

        /// <summary>
        /// File already exists in destination. Overwrite and ContinueIfExists = false. Task ends up to an error.
        /// </summary>
        [TestMethod]
        public void AWSCredsNoOverwriteNoContinueIfExistsTest()
        {
            Directory.CreateDirectory($@"{_dir}\Download");
            File.AppendAllText($@"{_dir}\Download\Testfile.txt", $"Test {DateTime.UtcNow}");

            _input = new Input()
            {
                AuthenticationMethod = AuthenticationMethod.AWSCredentials,
                AwsAccessKeyId = _accessKey,
                AwsSecretAccessKey = _secretAccessKey,
                BucketName = _bucketName,
                Region = Region.EuCentral1,
                DestinationPath = @$"{_dir}\Download",
                S3Directory = "2022/",
                SearchPattern = "*",
                Overwrite = false,
                DeleteSourceFile = false,
                DownloadFromCurrentDirectoryOnly = false,
                ThrowErrorIfNoMatch = false,
                ContinueIfExists = false,
            };

            var ex = Assert.ThrowsExceptionAsync<Exception>(async () => await AmazonS3.DownloadObject(_input, default)).Result;
            Assert.IsTrue(ex.Message.Contains("Consider using Overwrite or ContinueIfExists options"));
        }

        /// <summary>
        /// File already exists in destination. Overwrite = false and ContinueIfExists = true. Result will contain notification about the skipped file but continued downloading other files.
        /// </summary>
        [TestMethod]
        public void AWSCredsContinueIfExistsTest()
        {
            _input = new Input()
            {
                AuthenticationMethod = AuthenticationMethod.AWSCredentials,
                AwsAccessKeyId = _accessKey,
                AwsSecretAccessKey = _secretAccessKey,
                BucketName = _bucketName,
                Region = Region.EuCentral1,
                DestinationPath = @$"{_dir}\Download",
                S3Directory = "2022/",
                SearchPattern = "*",
                ContinueIfExists = true,
                Overwrite = false,
                DeleteSourceFile = false,
                DownloadFromCurrentDirectoryOnly = false,
                ThrowErrorIfNoMatch = false,
            };

            var result = AmazonS3.DownloadObject(_input, default);
            Assert.IsNotNull(result.Result.Results.Any(x => x.ObjectData.Contains("File Overwrite.txt was skipped because it already exists at ")));
            Assert.IsTrue(File.Exists(@$"{_dir}\Download\Testfile.txt"));
            Assert.IsTrue(File.Exists(@$"{_dir}\Download\DownloadFromCurrentDirectoryOnly.txt"));
        }

        /// <summary>
        /// Download only from S3 2022/ folder. DownloadFromCurrentDirectoryOnly.txt shouldn't exists.
        /// </summary>
        [TestMethod]
        public void AWSCredsDownloadFromCurrentDirectoryOnlyTest()
        {
            _input = new Input()
            {
                AuthenticationMethod = AuthenticationMethod.AWSCredentials,
                AwsAccessKeyId = _accessKey,
                AwsSecretAccessKey = _secretAccessKey,
                BucketName = _bucketName,
                Region = Region.EuCentral1,
                DestinationPath = @$"{_dir}\Download",
                S3Directory = "2022/",
                SearchPattern = "*",
                DownloadFromCurrentDirectoryOnly = true,
                Overwrite = true,
                DeleteSourceFile = false,
                ThrowErrorIfNoMatch = false,
            };

            var result = AmazonS3.DownloadObject(_input, default);
            Assert.IsNotNull(result.Result.Results);
            Assert.IsTrue(File.Exists(@$"{_dir}\Download\Testfile.txt"));
            Assert.IsTrue(File.Exists(@$"{_dir}\Download\Overwrite.txt"));
            Assert.IsFalse(File.Exists(@$"{_dir}\Download\DownloadFromCurrentDirectoryOnly.txt"));
        }

        /// <summary>
        /// Delete source files.
        /// </summary>
        /*
        [TestMethod]
        public void AWSCredsDeleteSourceFileTest()
        {
            _input = new Input()
            {
                AuthenticationMethod = AuthenticationMethod.AWSCredentials,
                AwsAccessKeyId = _accessKey,
                AwsSecretAccessKey = _secretAccessKey,
                BucketName = _bucketName,
                Region = Region.EuCentral1,
                DestinationPath = @$"{_dir}\Download",
                S3Directory = "2022/",
                SearchPattern = "*",
                DeleteSourceFile = true,
                DownloadFromCurrentDirectoryOnly = true,
                Overwrite = true,
                ThrowErrorIfNoMatch = false,
            };

            var result = AmazonS3.DownloadObject(_input, default);
            Assert.IsNotNull(result.Result.Results);
            Assert.IsTrue(File.Exists(@$"{_dir}\Download\Testfile.txt"));
            Assert.IsTrue(File.Exists(@$"{_dir}\Download\Overwrite.txt"));
            Assert.IsFalse(File.Exists(@$"{_dir}\Download\DownloadFromCurrentDirectoryOnly.txt"));
        }
        */
        #endregion aws creds


        /// <summary>
        /// Create pre-signed URL.
        /// </summary>
        private string CreatePreSignedURL(string key)
        {
            AmazonS3Client? client = new(_accessKey, _secretAccessKey, RegionEndpoint.EUCentral1);

            GetPreSignedUrlRequest request = new()
            {
                BucketName = _bucketName,
                Key = key,
                Verb = HttpVerb.GET,
                Expires = DateTime.UtcNow.AddMinutes(2)
            };
            var url = client.GetPreSignedURL(request);
            return url;
        }

        private void CreateTestFiles()
        {
            Directory.CreateDirectory($@"{_dir}\Download");
            Directory.CreateDirectory($@"{_dir}\DownloadTestFiles\DownloadFromCurrentDirectoryOnly");
            File.AppendAllText($@"{_dir}\Download\Overwrite.txt", "To Be Overwriten");
            File.AppendAllText($@"{_dir}\DownloadTestFiles\Overwrite.txt", $"Overwrite complete {DateTime.UtcNow}");
            File.AppendAllText($@"{_dir}\DownloadTestFiles\Testfile.txt", $"Test {DateTime.UtcNow}");
            File.AppendAllText($@"{_dir}\DownloadTestFiles\DownloadFromCurrentDirectoryOnly\DownloadFromCurrentDirectoryOnly.txt", $"This should exists if DownloadFromCurrentDirectoryOnly = true.  {DateTime.UtcNow}");
        }

        private bool CompareFiles()
        {
            string mainFile = File.ReadAllText($@"{_dir}\Download\Overwrite.txt");
            return mainFile.Contains("Overwrite complete") && !mainFile.Contains("To Be Overwriten");
        }
    }
}