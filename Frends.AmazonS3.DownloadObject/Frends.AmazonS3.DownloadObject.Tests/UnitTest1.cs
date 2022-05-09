using Microsoft.VisualStudio.TestTools.UnitTesting;
using Frends.AmazonS3.DownloadObject.Definitions;
using System;
using System.IO;
using System.Linq;
using Amazon.S3;
using Amazon;
using Amazon.S3.Model;

namespace Frends.AmazonS3.DownloadObject.Tests
{
    [TestClass]
    public class UnitTest1
    {
        private readonly string? _accessKey = Environment.GetEnvironmentVariable("HiQ_AWSS3Test_AccessKey");
        private readonly string? _secretAccessKey = Environment.GetEnvironmentVariable("HiQ_AWSS3Test_SecretAccessKey");
        private readonly string? _bucketName = Environment.GetEnvironmentVariable("HiQ_AWSS3Test_BucketName");
        private readonly string _dir = Path.Combine(Environment.CurrentDirectory); // .\Frends.AmazonS3.DownloadObject\Frends.AmazonS3.DownloadObject.Test\bin\Debug\net6.0\

        Connection? _connection;
        Input? _input;
        Options? _options;


        /// <summary>
        /// Create test files and folders. 
        /// !!! Manually upload test files to S3 "2022/"-folder. You can copy files from ".\Frends.AmazonS3.DownloadObject\Frends.AmazonS3.DownloadObject.Test\bin\Debug\net6.0\DownloadTestFiles" after first test run. !!!
        /// </summary>
        [TestInitialize]
        public void Initialize()
        {
            CreateTestFiles();
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

            _connection = new Connection()
            {
                AuthenticationMethod = AuthenticationMethod.PreSignedURL,
                PreSignedURL = CreatePreSignedURL(setS3Key)
            };

            _input = new Input()
            {
                DestinationPath = @$"{_dir}\Download",
                FileName = "Testfile.txt",
            };

            _options = new Options() { };

            var result = AmazonS3.DownloadObject(_connection, _input, _options, default);
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

            _connection = new Connection()
            {
                AuthenticationMethod = AuthenticationMethod.PreSignedURL,
                PreSignedURL = ""
            };

            _input = new Input()
            {
                DestinationPath = @$"{_dir}\Download",
                FileName = "Testfile.txt",
            };

            _options = new Options() { };

            var ex = Assert.ThrowsExceptionAsync<Exception>(async () => await AmazonS3.DownloadObject(_connection, _input, _options, default)).Result;
            Assert.IsTrue(ex.Message.Contains("AWS pre-signed URL required."));
        }

        /// <summary>
        /// Missing destination path.
        /// </summary>
        [TestMethod]
        public void PreSignedMissingDestinationPathTest()
        {
            var setS3Key = $"2022/Testfile.txt"; //S3 key.

            _connection = new Connection()
            {
                AuthenticationMethod = AuthenticationMethod.PreSignedURL,
                PreSignedURL = CreatePreSignedURL(setS3Key)
            };

            _input = new Input()
            {
                DestinationPath = "",
                FileName = "Testfile.txt",
            };

            _options = new Options() { };

            var ex = Assert.ThrowsExceptionAsync<Exception>(async () => await AmazonS3.DownloadObject(_connection, _input, _options, default)).Result;
            Assert.IsTrue(ex.Message.Contains("Destination required."));
        }

        /// <summary>
        /// File already exists in destination. Overwrite and ContinueIfExists = false.
        /// </summary>
        [TestMethod]
        public void PreSignedNoOverwriteContinueIfExistsTest()
        {
            var setS3Key = $"2022/Overwrite.txt"; //S3 key.

            _connection = new Connection()
            {
                AuthenticationMethod = AuthenticationMethod.PreSignedURL,
                PreSignedURL = CreatePreSignedURL(setS3Key)
            };

            _input = new Input()
            {
                DestinationPath = @$"{_dir}\Download",
                FileName = "Overwrite.txt",
            };

            _options = new Options()
            {
                Overwrite = false,
                ContinueIfExists = false,
            };

            var ex = Assert.ThrowsExceptionAsync<Exception>(async () => await AmazonS3.DownloadObject(_connection, _input, _options, default)).Result;
            Assert.IsTrue(ex.Message.Contains("File already exists at"));
        }

        /// <summary>
        /// Result is not null, file exists and was overwriten (compare lines).
        /// </summary>
        [TestMethod]
        public void PreSignedURLOverwriteTest()
        {
            var setS3Key = $"2022/Overwrite.txt"; //S3 key.

            _connection = new Connection()
            {
                AuthenticationMethod = AuthenticationMethod.PreSignedURL,
                PreSignedURL = CreatePreSignedURL(setS3Key)
            };

            _input = new Input()
            {
                DestinationPath = @$"{_dir}\Download",
                FileName = "Overwrite.txt",
            };

            _options = new Options()
            {
                Overwrite = true,
                ContinueIfExists = false,
            };

            var result = AmazonS3.DownloadObject(_connection, _input, _options, default);
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
        public void AWSCredsTestAllKeys()
        {
            Directory.Delete($@"{_dir}\Download", true);

            _connection = new Connection()
            {
                AuthenticationMethod = AuthenticationMethod.AWSCredentials,
                AwsAccessKeyId = _accessKey,
                AwsSecretAccessKey = _secretAccessKey,
                BucketName = _bucketName,
                Region = Region.EuCentral1,
            };

            _input = new Input()
            {
                DestinationPath = @$"{_dir}\Download",
                S3Directory = "2022/",
                SearchPattern = "*"
            };

            _options = new Options()
            {
                DeleteSourceFile = false,
                DownloadFromCurrentDirectoryOnly = false,
                Overwrite = false,
                ThrowErrorIfNoMatch = false,
            };

            var result = AmazonS3.DownloadObject(_connection, _input, _options, default);
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
            _connection = new Connection()
            {
                AuthenticationMethod = AuthenticationMethod.AWSCredentials,
                AwsAccessKeyId = "",
                AwsSecretAccessKey = _secretAccessKey,
                BucketName = _bucketName,
                Region = Region.EuCentral1,
            };

            _input = new Input()
            {
                DestinationPath = @$"{_dir}\Download",
                S3Directory = "2022/",
                SearchPattern = "*"
            };

            _options = new Options()
            {
                DeleteSourceFile = false,
                DownloadFromCurrentDirectoryOnly = false,
                Overwrite = false,
                ThrowErrorIfNoMatch = false,
            };

            var ex = Assert.ThrowsExceptionAsync<Exception>(async () => await AmazonS3.DownloadObject(_connection, _input, _options, default)).Result;
            Assert.IsTrue(ex.Message.Contains("AWS Access Key Id and Secret Access Key required."));
        }

        /// <summary>
        /// ThrowErrorIfNoMatch = true.
        /// </summary>
        [TestMethod]
        public void AWSCredsSearchPatternTest()
        {
            _connection = new Connection()
            {
                AuthenticationMethod = AuthenticationMethod.AWSCredentials,
                AwsAccessKeyId = _accessKey,
                AwsSecretAccessKey = _secretAccessKey,
                BucketName = _bucketName,
                Region = Region.EuCentral1,
            };

            _input = new Input()
            {
                DestinationPath = @$"{_dir}\Download",
                S3Directory = "2022/",
                SearchPattern = "Test*.txt"
            };

            _options = new Options()
            {
                ThrowErrorIfNoMatch = true,
                DeleteSourceFile = false,
                DownloadFromCurrentDirectoryOnly = false,
                Overwrite = true,
            };

            var result = AmazonS3.DownloadObject(_connection, _input, _options, default);
            Assert.IsNotNull(result.Result.Results);
            Assert.IsTrue(File.Exists(@$"{_dir}\Download\Testfile.txt"));
        }

        /// <summary>
        /// ThrowErrorIfNoMatch = true.
        /// </summary>
        [TestMethod]
        public void AWSCredsThrowErrorIfNoMatchTest()
        {
            _connection = new Connection()
            {
                AuthenticationMethod = AuthenticationMethod.AWSCredentials,
                AwsAccessKeyId = _accessKey,
                AwsSecretAccessKey = _secretAccessKey,
                BucketName = _bucketName,
                Region = Region.EuCentral1,
            };

            _input = new Input()
            {
                DestinationPath = @$"{_dir}\Download",
                S3Directory = "2022/",
                SearchPattern = "NoFile*"
            };

            _options = new Options()
            {
                ThrowErrorIfNoMatch = true,
                DeleteSourceFile = false,
                DownloadFromCurrentDirectoryOnly = false,
                Overwrite = true,
            };

            var ex = Assert.ThrowsExceptionAsync<Exception>(async () => await AmazonS3.DownloadObject(_connection, _input, _options, default)).Result;
            Assert.IsTrue(ex.Message.Contains("No matches found with search pattern"));
        }

        /// <summary>
        /// Missing destination path.
        /// </summary>
        [TestMethod]
        public void AWSCredsDestinationMissingTest()
        {
            _connection = new Connection()
            {
                AuthenticationMethod = AuthenticationMethod.AWSCredentials,
                AwsAccessKeyId = _accessKey,
                AwsSecretAccessKey = _secretAccessKey,
                BucketName = _bucketName,
                Region = Region.EuCentral1,
            };

            _input = new Input()
            {
                DestinationPath = "",
                S3Directory = "2022/",
                SearchPattern = "*"
            };

            _options = new Options()
            {
                DeleteSourceFile = false,
                DownloadFromCurrentDirectoryOnly = false,
                Overwrite = false,
                ThrowErrorIfNoMatch = false,
            };

            var ex = Assert.ThrowsExceptionAsync<Exception>(async () => await AmazonS3.DownloadObject(_connection, _input, _options, default)).Result;
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

            _connection = new Connection()
            {
                AuthenticationMethod = AuthenticationMethod.AWSCredentials,
                AwsAccessKeyId = _accessKey,
                AwsSecretAccessKey = _secretAccessKey,
                BucketName = _bucketName,
                Region = Region.EuCentral1,
            };

            _input = new Input()
            {
                DestinationPath = @$"{_dir}\Download",
                S3Directory = "2022/",
                SearchPattern = "*"
            };

            _options = new Options()
            {
                Overwrite = false,
                DeleteSourceFile = false,
                DownloadFromCurrentDirectoryOnly = false,
                ThrowErrorIfNoMatch = false,
                ContinueIfExists = false,
            };

            var ex = Assert.ThrowsExceptionAsync<Exception>(async () => await AmazonS3.DownloadObject(_connection, _input, _options, default)).Result;
            Assert.IsTrue(ex.Message.Contains("Set Overwrite to true from options to overwrite the file or ContinueIfExists to true to jump to next file in queue"));
        }

        /// <summary>
        /// File already exists in destination. Overwrite = false and ContinueIfExists = true. Result will contain notification about the skipped file but continued downloading other files.
        /// </summary>
        [TestMethod]
        public void AWSCredsContinueIfExistsTest()
        {
            _connection = new Connection()
            {
                AuthenticationMethod = AuthenticationMethod.AWSCredentials,
                AwsAccessKeyId = _accessKey,
                AwsSecretAccessKey = _secretAccessKey,
                BucketName = _bucketName,
                Region = Region.EuCentral1,
            };

            _input = new Input()
            {
                DestinationPath = @$"{_dir}\Download",
                S3Directory = "2022/",
                SearchPattern = "*"
            };

            _options = new Options()
            {
                ContinueIfExists = true,
                Overwrite = false,
                DeleteSourceFile = false,
                DownloadFromCurrentDirectoryOnly = false,
                ThrowErrorIfNoMatch = false,
            };

            var result = AmazonS3.DownloadObject(_connection, _input, _options, default);
            Assert.IsNotNull(result.Result.Results.Any(x => x.DownloadedObject.Contains("File Overwrite.txt was skipped because it already exists at ")));
            Assert.IsTrue(File.Exists(@$"{_dir}\Download\Testfile.txt"));
            Assert.IsTrue(File.Exists(@$"{_dir}\Download\DownloadFromCurrentDirectoryOnly.txt"));
        }

        /// <summary>
        /// Download only from S3 2022/ folder. DownloadFromCurrentDirectoryOnly.txt shouldn't exists.
        /// </summary>
        [TestMethod]
        public void AWSCredsDownloadFromCurrentDirectoryOnlyTest()
        {
            _connection = new Connection()
            {
                AuthenticationMethod = AuthenticationMethod.AWSCredentials,
                AwsAccessKeyId = _accessKey,
                AwsSecretAccessKey = _secretAccessKey,
                BucketName = _bucketName,
                Region = Region.EuCentral1,
            };

            _input = new Input()
            {
                DestinationPath = @$"{_dir}\Download",
                S3Directory = "2022/",
                SearchPattern = "*"
            };

            _options = new Options()
            {
                DownloadFromCurrentDirectoryOnly = true,
                Overwrite = true,
                DeleteSourceFile = false,
                ThrowErrorIfNoMatch = false,
            };

            var result = AmazonS3.DownloadObject(_connection, _input, _options, default);
            Assert.IsNotNull(result.Result.Results);
            Assert.IsTrue(File.Exists(@$"{_dir}\Download\Testfile.txt"));
            Assert.IsTrue(File.Exists(@$"{_dir}\Download\Overwrite.txt"));
            Assert.IsFalse(File.Exists(@$"{_dir}\Download\DownloadFromCurrentDirectoryOnly.txt"));
        }

        /// <summary>
        /// Delete source files.
        /// </summary>
        [TestMethod]
        public void AWSCredsDeleteSourceFileTest()
        {
            _connection = new Connection()
            {
                AuthenticationMethod = AuthenticationMethod.AWSCredentials,
                AwsAccessKeyId = _accessKey,
                AwsSecretAccessKey = _secretAccessKey,
                BucketName = _bucketName,
                Region = Region.EuCentral1,
            };

            _input = new Input()
            {
                DestinationPath = @$"{_dir}\Download",
                S3Directory = "2022/",
                SearchPattern = "*"
            };

            _options = new Options()
            {
                DeleteSourceFile = true,
                DownloadFromCurrentDirectoryOnly = true,
                Overwrite = true,
                ThrowErrorIfNoMatch = false,
            };

            var result = AmazonS3.DownloadObject(_connection, _input, _options, default);
            Assert.IsNotNull(result.Result.Results);
            Assert.IsTrue(File.Exists(@$"{_dir}\Download\Testfile.txt"));
            Assert.IsTrue(File.Exists(@$"{_dir}\Download\Overwrite.txt"));
            Assert.IsFalse(File.Exists(@$"{_dir}\Download\DownloadFromCurrentDirectoryOnly.txt"));
        }

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
            return mainFile.Contains("Overwrite complete") && !mainFile.Contains("To Be Overwriten") ? true : false;
        }

        /// <summary>
        /// Delete test files and folders.
        /// </summary>
        [TestCleanup]
        public void CleanUp()
        {
            Directory.Delete($@"{_dir}\Download", true);
        }
    }

}