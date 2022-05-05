using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Frends.AmazonS3.UploadObject.Definitions;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon;
using System.IO;

namespace Frends.AmazonS3.UploadObject.Test
{
    [TestClass]
    public class UnitTest1
    {
        public TestContext? TestContext { get; set; }

        private readonly string? _accessKey = Environment.GetEnvironmentVariable("HiQ_AWSS3Test_AccessKey");
        private readonly string? _secretAccessKey = Environment.GetEnvironmentVariable("HiQ_AWSS3Test_SecretAccessKey");
        private readonly string? _bucketName = Environment.GetEnvironmentVariable("HiQ_AWSS3Test_BucketName");
        private readonly string _dir = Path.Combine(Environment.CurrentDirectory);


        Connection? _connection;
        Input? _input;
        Options? _options;


        #region PreSigned
        /// <summary>
        /// PreSigned URL testing.
        /// Presigned URL can be used just once and for 1 file.
        /// </summary>
        #region TestPreSignedUpload
        [TestMethod]
        public void PreSignedUploadTest()
        {
            var setS3Key = $"2022/PreSigned/UploadTest.txt"; //S3 "location".
            _input = new Input
            {
                FilePath = $@"{_dir}\AWS",
                ACL = default,
                FileMask = null,
                UseACL = false,
                S3Directory = null,
            };
            _connection = new Connection
            {
                AuthenticationMethod = AuthenticationMethod.PreSignedURL,
                PreSignedURL = CreatePresignedUrl(setS3Key).ToString(),
                AwsAccessKeyId = null,
                AwsSecretAccessKey = null,
                BucketName = null,
                Region = default,
            };
            _options = new Options
            {
                UploadFromCurrentDirectoryOnly = false,
                Overwrite = false,
                PreserveFolderStructure = false,
                ReturnListOfObjectKeys = false,
                DeleteSource = false,
                ThrowErrorIfNoMatch = false,
            };

            var result = AmazonS3.UploadObject(_connection, _input, _options, default);
            Assert.IsNotNull(result.Result.Results);
        }
        #endregion TestPreSignedUpload

        #region TestPreSignedMissing
        /// <summary>
        /// PreSigned URL testing. Missing URL and ends up to exception before checking files etcc.
        /// </summary>
        [TestMethod]
        public void PreSignedMissing()
        {
            _input = new Input
            {
                FilePath = $@"{_dir}\AWS",
                ACL = default,
                FileMask = null,
                UseACL = false,
                S3Directory = null,
            };
            _connection = new Connection
            {
                AuthenticationMethod = AuthenticationMethod.PreSignedURL,
                PreSignedURL = " ",
                AwsAccessKeyId = null,
                AwsSecretAccessKey = null,
                BucketName = null,
                Region = default,
            };
            _options = new Options
            {
                UploadFromCurrentDirectoryOnly = false,
                Overwrite = false,
                PreserveFolderStructure = false,
                ReturnListOfObjectKeys = false,
                DeleteSource = false,
                ThrowErrorIfNoMatch = false,
            };

            var ex = Assert.ThrowsExceptionAsync<Exception>(async () => await AmazonS3.UploadObject(_connection, _input, _options, default)).Result;
            Assert.IsTrue(ex.Message.Contains("AWS pre-signed URL required."));
        }
        #endregion TestPreSignedMissing

        #region PreSignedNoFileInTopDirectoryTest
        /// <summary>
        /// Presigned URL with UploadFromCurrentDirectoryOnly and ThrowErrorIfNoMatch = true
        /// Presigned URL can be used just once and for 1 file upload -> DeleteMainDirectoryFiles() makes sure that only subdirectory contains the file and top directory is empty.
        /// </summary>
        [TestMethod]
        public void PreSignedNoFileInTopDirectoryTest()
        {
            DeleteMainDirectoryFiles();
            var setS3Key = $"2022/PreSigned/PreSignedNoFileInTopDirectoryTest.txt"; //Location where file will be uploaded.

            _input = new Input
            {
                FilePath = $@"{_dir}\AWS",
                ACL = default,
                FileMask = null,
                UseACL = false,
                S3Directory = null,
            };
            _options = new Options
            {
                UploadFromCurrentDirectoryOnly = true,
                ThrowErrorIfNoMatch = true,
                Overwrite = false,
                PreserveFolderStructure = false,
                ReturnListOfObjectKeys = false,
                DeleteSource = false,
            };
            _connection = new Connection
            {
                AuthenticationMethod = AuthenticationMethod.PreSignedURL,
                PreSignedURL = CreatePresignedUrl(setS3Key).ToString(),
                AwsAccessKeyId = null,
                AwsSecretAccessKey = null,
                BucketName = null,
                Region = default,
            };

            var ex = Assert.ThrowsExceptionAsync<Exception>(async () => await AmazonS3.UploadObject(_connection, _input, _options, default)).Result;
            Assert.IsTrue(ex.Message.Contains("No files match the filemask within supplied path."));
        }
        #endregion PreSignedNoFileInTopDirectoryTest

        #region PreSignedNoPath
        /// <summary>
        /// Presigned URL with FilePath not found.
        /// </summary>
        [TestMethod]
        public void PreSignedNoPath()
        {
            var setS3Key = $"2022/PreSigned/PreSignedNoPath.txt"; //Location where file will be uploaded. This file should not exists in S3.
            _input = new Input
            {
                FilePath = "c:/nofileshere/",
                ACL = default,
                FileMask = null,
                UseACL = false,
                S3Directory = null,
            };
            _options = new Options
            {
                UploadFromCurrentDirectoryOnly = true,
                ThrowErrorIfNoMatch = true,
                Overwrite = false,
                PreserveFolderStructure = false,
                ReturnListOfObjectKeys = false,
                DeleteSource = false,
            };
            _connection = new Connection
            {
                AuthenticationMethod = AuthenticationMethod.PreSignedURL,
                PreSignedURL = CreatePresignedUrl(setS3Key).ToString(),
                AwsAccessKeyId = null,
                AwsSecretAccessKey = null,
                BucketName = null,
                Region = default,
            };

            var ex = Assert.ThrowsExceptionAsync<Exception>(async () => await AmazonS3.UploadObject(_connection, _input, _options, default)).Result;
            Assert.IsTrue(ex.Message.Contains("Source path not found."));
        }
        #endregion PreSignedNoPath

        #region PreSignedDeleteSourceFileMask
        /// <summary>
        /// Presigned URL can be used just once and for 1 file so only 1 file will be uploaded and deleted.
        /// Also testing FileMask.
        /// Test asserts that file has been uploaded (result ok) and file doesn't exists in source directory but you can comment CleanUp() and check it out manually.
        /// </summary>
        [TestMethod]
        public void PreSignedDeleteSourceFileMask()
        {
            var fileName = "deletethis_presign.txt";
            var setS3Key = $"2022/PreSigned/{fileName}"; //Location where file will be uploaded. test1.txt should be deleted from source directory.
            _input = new Input
            {
                FilePath = $@"{_dir}\AWS",
                ACL = default,
                FileMask = "deletethis_presign.txt",
                UseACL = false,
                S3Directory = null,
            };
            _options = new Options
            {
                DeleteSource = true,
                UploadFromCurrentDirectoryOnly = false,
                ThrowErrorIfNoMatch = false,
                Overwrite = false,
                PreserveFolderStructure = false,
                ReturnListOfObjectKeys = false,
            };
            _connection = new Connection
            {
                AuthenticationMethod = AuthenticationMethod.PreSignedURL,
                PreSignedURL = CreatePresignedUrl(setS3Key).ToString(),
                AwsAccessKeyId = null,
                AwsSecretAccessKey = null,
                BucketName = null,
                Region = default,
            };

            var result = AmazonS3.UploadObject(_connection, _input, _options, default);
            Assert.IsNotNull(result.Result.Results);
            Assert.IsTrue(!File.Exists($@"{_dir}\AWS\{fileName}"));
        }
        #endregion PreSignedDeleteSourceFileMask
        #endregion PreSigned

        #region AWS Creds

        #region TestAWSCredsUpload
        /// <summary>
        /// AWS creds testing.
        /// All files from FilePath will be uploaded.
        /// </summary>
        [TestMethod]
        public void AWSCredsUploadTest()
        {
            _input = new Input
            {
                FilePath = $@"{_dir}\AWS",
                ACL = default,
                FileMask = null,
                UseACL = false,
                S3Directory = "2022/AWSCreds/UploadTest/",
            };
            _connection = new Connection
            {
                AuthenticationMethod = AuthenticationMethod.AWSCredentials,
                PreSignedURL = null,
                AwsAccessKeyId = _accessKey,
                AwsSecretAccessKey = _secretAccessKey,
                BucketName = _bucketName,
                Region = Region.EuCentral1,
            };
            _options = new Options
            {
                UploadFromCurrentDirectoryOnly = false,
                Overwrite = false,
                PreserveFolderStructure = false,
                ReturnListOfObjectKeys = false,
                DeleteSource = false,
                ThrowErrorIfNoMatch = false,
            };

            var result = AmazonS3.UploadObject(_connection, _input, _options, default);
            Assert.IsNotNull(result.Result.Results);
        }
        #endregion TestAWSCredsUpload

        #region TestAWSCredsMissing
        /// <summary>
        /// AWS creds missing. Ends up to exception before checking files etcc.
        /// </summary>
        [TestMethod]
        public void AWSCredsMissing()
        {
            _input = new Input
            {
                FilePath = $@"{_dir}\AWS",
                ACL = default,
                FileMask = null,
                UseACL = false,
                S3Directory = "2022/AWSCreds/ThisShouldntExistsInS3_AWSCredsMissing/",
            };
            _connection = new Connection
            {
                AuthenticationMethod = AuthenticationMethod.AWSCredentials,
                PreSignedURL = null,
                AwsAccessKeyId = null,
                AwsSecretAccessKey = _secretAccessKey,
                BucketName = _bucketName,
                Region = Region.EuCentral1,
            };
            _options = new Options
            {
                UploadFromCurrentDirectoryOnly = false,
                Overwrite = false,
                PreserveFolderStructure = false,
                ReturnListOfObjectKeys = false,
                DeleteSource = false,
                ThrowErrorIfNoMatch = false,
            };

            var ex = Assert.ThrowsExceptionAsync<Exception>(async () => await AmazonS3.UploadObject(_connection, _input, _options, default)).Result;
            Assert.IsTrue(ex.Message.Contains("AWS Access Key Id and Secret Access Key required."));
        }
        #endregion TestAWSCredsMissing

        #region AWSCredsUploadFromCurrentDirectoryOnly
        /// <summary>
        /// AWS creds testing with UploadFromCurrentDirectoryOnly = true. S3 locations shouldn't contain subfile.txt.
        /// </summary>
        [TestMethod]
        public void AWSCredsUploadFromCurrentDirectoryOnly()
        {
            _input = new Input
            {
                FilePath = $@"{_dir}\AWS",
                ACL = default,
                FileMask = null,
                UseACL = false,
                S3Directory = "2022/AWSCreds/AWSCredsUploadFromCurrentDirectoryOnly/",
            };
            _connection = new Connection
            {
                AuthenticationMethod = AuthenticationMethod.AWSCredentials,
                PreSignedURL = null,
                AwsAccessKeyId = _accessKey,
                AwsSecretAccessKey = _secretAccessKey,
                BucketName = _bucketName,
                Region = Region.EuCentral1,
            };
            _options = new Options
            {
                UploadFromCurrentDirectoryOnly = true,
                Overwrite = false,
                PreserveFolderStructure = false,
                ReturnListOfObjectKeys = false,
                DeleteSource = false,
                ThrowErrorIfNoMatch = false,
            };

            var result = AmazonS3.UploadObject(_connection, _input, _options, default);
            Assert.IsNotNull(result.Result.Results);
        }
        #endregion AWSCredsUploadFromCurrentDirectoryOnly

        #region AWSCredsOverwrite
        /// <summary>
        /// AWS creds testing with Overwrite = true. Run this test 2 times so 2nd time will be overwrite.
        /// </summary>
        [TestMethod]
        public void AWSCredsOverwrite()
        {
            _input = new Input
            {
                FilePath = $@"{_dir}\AWS",
                ACL = default,
                FileMask = null,
                UseACL = false,
                S3Directory = "2022/AWSCreds/Overwrite/",
            };
            _connection = new Connection
            {
                AuthenticationMethod = AuthenticationMethod.AWSCredentials,
                PreSignedURL = null,
                AwsAccessKeyId = _accessKey,
                AwsSecretAccessKey = _secretAccessKey,
                BucketName = _bucketName,
                Region = Region.EuCentral1,
            };
            _options = new Options
            {
                Overwrite = true,
                UploadFromCurrentDirectoryOnly = false,
                PreserveFolderStructure = false,
                ReturnListOfObjectKeys = false,
                DeleteSource = false,
                ThrowErrorIfNoMatch = false,
            };

            var result = AmazonS3.UploadObject(_connection, _input, _options, default);
            Assert.IsNotNull(result.Result.Results);
        }
        #endregion AWSCredsOverwrite

        #region AWSCredsPreserveFolderStructure
        /// <summary>
        /// AWS creds testing with PreserveFolderStructure = true. 
        /// </summary>
        [TestMethod]
        public void AWSCredsPreserveFolderStructure()
        {
            _input = new Input
            {
                FilePath = $@"{_dir}\AWS",
                ACL = default,
                FileMask = null,
                UseACL = false,
                S3Directory = "2022/AWSCreds/PreserveFolderStructure/",
            };
            _connection = new Connection
            {
                AuthenticationMethod = AuthenticationMethod.AWSCredentials,
                PreSignedURL = null,
                AwsAccessKeyId = _accessKey,
                AwsSecretAccessKey = _secretAccessKey,
                BucketName = _bucketName,
                Region = Region.EuCentral1,
            };
            _options = new Options
            {
                PreserveFolderStructure = true,
                Overwrite = false,
                UploadFromCurrentDirectoryOnly = false,
                ReturnListOfObjectKeys = false,
                DeleteSource = false,
                ThrowErrorIfNoMatch = false,
            };

            var result = AmazonS3.UploadObject(_connection, _input, _options, default);
            Assert.IsNotNull(result.Result.Results);
        }
        #endregion AWSCredsPreserveFolderStructure

        #region AWSCredsReturnListOfObjectKeys
        /// <summary>
        /// AWS creds testing with PreserveFolderStructure = true. Check result.
        /// </summary>
        [TestMethod]
        public void AWSCredsReturnListOfObjectKeys()
        {
            _input = new Input
            {
                FilePath = $@"{_dir}\AWS",
                ACL = default,
                FileMask = null,
                UseACL = false,
                S3Directory = "2022/AWSCreds/AWSCredsReturnListOfObjectKeys/",
            };
            _connection = new Connection
            {
                AuthenticationMethod = AuthenticationMethod.AWSCredentials,
                PreSignedURL = null,
                AwsAccessKeyId = _accessKey,
                AwsSecretAccessKey = _secretAccessKey,
                BucketName = _bucketName,
                Region = Region.EuCentral1,
            };
            _options = new Options
            {
                ReturnListOfObjectKeys = true,
                PreserveFolderStructure = false,
                Overwrite = true,
                UploadFromCurrentDirectoryOnly = false,
                DeleteSource = false,
                ThrowErrorIfNoMatch = false,
            };

            var result = AmazonS3.UploadObject(_connection, _input, _options, default);
            Assert.IsNotNull(result.Result.Results);
        }
        #endregion AWSCredsReturnListOfObjectKeys

        #region AWSCredsDeleteSourceFileMask
        /// <summary>
        /// AWS creds testing with DeleteSource = true + FileMask.
        /// Also testing FileMask.
        /// Test asserts that file has been uploaded (result ok) and file doesn't exists in source directory but you can comment CleanUp() and check it out manually.
        /// </summary>
        [TestMethod]
        public void AWSCredsDeleteSourceFileMask()
        {
            var fileName = "deletethis_awscreds.txt";
            _input = new Input
            {
                FilePath = $@"{_dir}\AWS",
                ACL = default,
                FileMask = fileName,
                UseACL = false,
                S3Directory = "2022/AWSCreds/DeleteSource/",
            };
            _connection = new Connection
            {
                AuthenticationMethod = AuthenticationMethod.AWSCredentials,
                PreSignedURL = null,
                AwsAccessKeyId = _accessKey,
                AwsSecretAccessKey = _secretAccessKey,
                BucketName = _bucketName,
                Region = Region.EuCentral1,
            };
            _options = new Options
            {
                DeleteSource = true,
                ReturnListOfObjectKeys = false,
                PreserveFolderStructure = false,
                Overwrite = false,
                UploadFromCurrentDirectoryOnly = false,
                ThrowErrorIfNoMatch = false,
            };

            var result = AmazonS3.UploadObject(_connection, _input, _options, default);
            Assert.IsNotNull(result.Result.Results);
            Assert.IsTrue(!File.Exists($@"{_dir}\AWS\{fileName}"));
        }
        #endregion AWSCredsDeleteSourceFileMask

        #region AWSCredsThrowErrorIfNoMatch
        /// <summary>
        /// AWS creds testing with AWSCredsThrowErrorIfNoMatch = true.
        /// </summary>
        [TestMethod]
        public void AWSCredsThrowErrorIfNoMatch()
        {
            _input = new Input
            {
                FilePath = $@"{_dir}\AWS",
                ACL = default,
                FileMask = "notafile*",
                UseACL = false,
                S3Directory = "2022/AWSCreds/ThisShouldntExistsInS3_AWSCredsThrowErrorIfNoMatch/",
            };
            _connection = new Connection
            {
                AuthenticationMethod = AuthenticationMethod.AWSCredentials,
                PreSignedURL = null,
                AwsAccessKeyId = _accessKey,
                AwsSecretAccessKey = _secretAccessKey,
                BucketName = _bucketName,
                Region = Region.EuCentral1,
            };
            _options = new Options
            {
                ThrowErrorIfNoMatch = true,
                DeleteSource = false,
                ReturnListOfObjectKeys = false,
                PreserveFolderStructure = false,
                Overwrite = false,
                UploadFromCurrentDirectoryOnly = false,
            };

            var ex = Assert.ThrowsExceptionAsync<Exception>(async () => await AmazonS3.UploadObject(_connection, _input, _options, default)).Result;
            Assert.IsTrue(ex.Message.Contains("No files match the filemask within supplied path."));
        }
        #endregion AWSCredsThrowErrorIfNoMatch

        #region AWSCredsACLPrivate
        /// <summary>
        /// AWS creds testing.
        /// ACL Private.
        /// </summary>
        [TestMethod]
        public void AWSCredsACLPrivate()
        {
            _input = new Input
            {
                FilePath = $@"{_dir}\AWS",
                ACL = ACLs.Private,
                FileMask = null,
                UseACL = true,
                S3Directory = "2022/AWSCreds/ACLPrivate/",
            };
            _connection = new Connection
            {
                AuthenticationMethod = AuthenticationMethod.AWSCredentials,
                PreSignedURL = null,
                AwsAccessKeyId = _accessKey,
                AwsSecretAccessKey = _secretAccessKey,
                BucketName = _bucketName,
                Region = Region.EuCentral1,
            };
            _options = new Options
            {
                UploadFromCurrentDirectoryOnly = false,
                Overwrite = false,
                PreserveFolderStructure = false,
                ReturnListOfObjectKeys = false,
                DeleteSource = false,
                ThrowErrorIfNoMatch = false,
            };

            var result = AmazonS3.UploadObject(_connection, _input, _options, default);
            Assert.IsNotNull(result.Result.Results);
        }
        #endregion AWSCredsACLPrivate

        #region AWSCredsACLPublicRead
        /// <summary>
        /// AWS creds testing.
        /// ACL PublicRead.
        /// </summary>
        [TestMethod]
        public void AWSCredsACLPublicRead()
        {
            _input = new Input
            {
                FilePath = $@"{_dir}\AWS",
                ACL = ACLs.PublicRead,
                FileMask = null,
                UseACL = true,
                S3Directory = "2022/AWSCreds/ACLPublicRead/",
            };
            _connection = new Connection
            {
                AuthenticationMethod = AuthenticationMethod.AWSCredentials,
                PreSignedURL = null,
                AwsAccessKeyId = _accessKey,
                AwsSecretAccessKey = _secretAccessKey,
                BucketName = _bucketName,
                Region = Region.EuCentral1,
            };
            _options = new Options
            {
                UploadFromCurrentDirectoryOnly = false,
                Overwrite = false,
                PreserveFolderStructure = false,
                ReturnListOfObjectKeys = false,
                DeleteSource = false,
                ThrowErrorIfNoMatch = false,
            };

            var result = AmazonS3.UploadObject(_connection, _input, _options, default);
            Assert.IsNotNull(result.Result.Results);
        }
        #endregion AWSCredsACLPublicRead

        #region AWSCredsACLPublicReadWrited
        /// <summary>
        /// AWS creds testing.
        /// ACL PublicReadWrite.
        /// </summary>
        [TestMethod]
        public void AWSCredsACLPublicReadWrited()
        {
            _input = new Input
            {
                FilePath = $@"{_dir}\AWS",
                ACL = ACLs.PublicReadWrite,
                FileMask = null,
                UseACL = true,
                S3Directory = "2022/AWSCreds/ACLPublicReadWrite/",
            };
            _connection = new Connection
            {
                AuthenticationMethod = AuthenticationMethod.AWSCredentials,
                PreSignedURL = null,
                AwsAccessKeyId = _accessKey,
                AwsSecretAccessKey = _secretAccessKey,
                BucketName = _bucketName,
                Region = Region.EuCentral1,
            };
            _options = new Options
            {
                UploadFromCurrentDirectoryOnly = false,
                Overwrite = false,
                PreserveFolderStructure = false,
                ReturnListOfObjectKeys = false,
                DeleteSource = false,
                ThrowErrorIfNoMatch = false,
            };

            var result = AmazonS3.UploadObject(_connection, _input, _options, default);
            Assert.IsNotNull(result.Result.Results);
        }
        #endregion AWSCredsACLPublicReadWrited

        #region AWSCredsACLAuthenticatedRead
        /// <summary>
        /// AWS creds testing.
        /// ACL AuthenticatedRead.
        /// </summary>
        [TestMethod]
        public void AWSCredsACLAuthenticatedRead()
        {
            _input = new Input
            {
                FilePath = $@"{_dir}\AWS",
                ACL = ACLs.AuthenticatedRead,
                FileMask = null,
                UseACL = true,
                S3Directory = "2022/AWSCreds/ACLAuthenticatedRead/",
            };
            _connection = new Connection
            {
                AuthenticationMethod = AuthenticationMethod.AWSCredentials,
                PreSignedURL = null,
                AwsAccessKeyId = _accessKey,
                AwsSecretAccessKey = _secretAccessKey,
                BucketName = _bucketName,
                Region = Region.EuCentral1,
            };
            _options = new Options
            {
                UploadFromCurrentDirectoryOnly = false,
                Overwrite = false,
                PreserveFolderStructure = false,
                ReturnListOfObjectKeys = false,
                DeleteSource = false,
                ThrowErrorIfNoMatch = false,
            };

            var result = AmazonS3.UploadObject(_connection, _input, _options, default);
            Assert.IsNotNull(result.Result.Results);
        }
        #endregion AWSCredsACLAuthenticatedRead

        #region AWSCredsACLBucketOwnerRead
        /// <summary>
        /// AWS creds testing.
        /// ACL BucketOwnerRead.
        /// </summary>
        [TestMethod]
        public void AWSCredsACLBucketOwnerRead()
        {
            _input = new Input
            {
                FilePath = $@"{_dir}\AWS",
                ACL = ACLs.BucketOwnerRead,
                FileMask = null,
                UseACL = true,
                S3Directory = "2022/AWSCreds/ACLBucketOwnerRead/",
            };
            _connection = new Connection
            {
                AuthenticationMethod = AuthenticationMethod.AWSCredentials,
                PreSignedURL = null,
                AwsAccessKeyId = _accessKey,
                AwsSecretAccessKey = _secretAccessKey,
                BucketName = _bucketName,
                Region = Region.EuCentral1,
            };
            _options = new Options
            {
                UploadFromCurrentDirectoryOnly = false,
                Overwrite = false,
                PreserveFolderStructure = false,
                ReturnListOfObjectKeys = false,
                DeleteSource = false,
                ThrowErrorIfNoMatch = false,
            };

            var result = AmazonS3.UploadObject(_connection, _input, _options, default);
            Assert.IsNotNull(result.Result.Results);
        }
        #endregion AWSCredsACLBucketOwnerRead

        #region AWSCredsACLBucketOwnerFullControl
        /// <summary>
        /// AWS creds testing.
        /// ACL BucketOwnerFullControl.
        /// </summary>
        [TestMethod]
        public void AWSCredsACLBucketOwnerFullControl()
        {
            _input = new Input
            {
                FilePath = $@"{_dir}\AWS",
                ACL = ACLs.BucketOwnerFullControl,
                FileMask = null,
                UseACL = true,
                S3Directory = "2022/AWSCreds/ACLBucketOwnerFullControl/",
            };
            _connection = new Connection
            {
                AuthenticationMethod = AuthenticationMethod.AWSCredentials,
                PreSignedURL = null,
                AwsAccessKeyId = _accessKey,
                AwsSecretAccessKey = _secretAccessKey,
                BucketName = _bucketName,
                Region = Region.EuCentral1,
            };
            _options = new Options
            {
                UploadFromCurrentDirectoryOnly = false,
                Overwrite = false,
                PreserveFolderStructure = false,
                ReturnListOfObjectKeys = false,
                DeleteSource = false,
                ThrowErrorIfNoMatch = false,
            };

            var result = AmazonS3.UploadObject(_connection, _input, _options, default);
            Assert.IsNotNull(result.Result.Results);
        }
        #endregion AWSCredsACLBucketOwnerFullControl

        #region AWSCredsACLLogDeliveryWrite
        /// <summary>
        /// AWS creds testing.
        /// ACL LogDeliveryWrite.
        /// </summary>
        [TestMethod]
        public void AWSCredsACLLogDeliveryWrite()
        {
            _input = new Input
            {
                FilePath = $@"{_dir}\AWS",
                ACL = ACLs.LogDeliveryWrite,
                FileMask = null,
                UseACL = true,
                S3Directory = "2022/AWSCreds/ACLLogDeliveryWrite/",
            };
            _connection = new Connection
            {
                AuthenticationMethod = AuthenticationMethod.AWSCredentials,
                PreSignedURL = null,
                AwsAccessKeyId = _accessKey,
                AwsSecretAccessKey = _secretAccessKey,
                BucketName = _bucketName,
                Region = Region.EuCentral1,
            };
            _options = new Options
            {
                UploadFromCurrentDirectoryOnly = false,
                Overwrite = false,
                PreserveFolderStructure = false,
                ReturnListOfObjectKeys = false,
                DeleteSource = false,
                ThrowErrorIfNoMatch = false,
            };

            var result = AmazonS3.UploadObject(_connection, _input, _options, default);
            Assert.IsNotNull(result.Result.Results);
        }
        #endregion AWSCredsACLLogDeliveryWrite

        #endregion AWS Creds

        #region HelperMethods

        /// <summary>
        /// Create test files and folders.
        /// </summary>
        [TestInitialize]
        public void Initialize()
        {
            CreateTestFiles();
        }

        /// <summary>
        /// Delete test files and folders.
        /// </summary>
        [TestCleanup]
        public void CleanUp()
        {
            DeleteSourcePath();
        }

        /// <summary>
        /// Create pre-signed URL
        /// </summary>
        /// <param name="key">File's name in S3</param>
        /// <returns></returns>
        private Uri CreatePresignedUrl(string key)
        {
            var region = RegionEndpoint.EUCentral1;
            var client = new AmazonS3Client(_accessKey, _secretAccessKey, region);
            GetPreSignedUrlRequest request = new()
            {
                BucketName = _bucketName,
                Key = key,
                Verb = HttpVerb.PUT,
                Expires = DateTime.UtcNow.AddMinutes(15),

            };
            return new Uri(client.GetPreSignedURL(request));
        }


        /// <summary>
        /// Test files and folders. Location Frends.AmazonS3.UploadObject.Test\bin\Debug\net6.0\AWS
        /// </summary>
        private void CreateTestFiles()
        {
            Directory.CreateDirectory($@"{_dir}\AWS");
            Directory.CreateDirectory($@"{_dir}\AWS\Subfolder");

            File.AppendAllText($@"{_dir}\AWS\test1.txt", "test1");
            File.AppendAllText($@"{_dir}\AWS\Subfolder\subfile.txt", "From subfolder.");
            File.AppendAllText($@"{_dir}\AWS\deletethis_presign.txt", "Resource file deleted. (Presign)");
            File.AppendAllText($@"{_dir}\AWS\deletethis_awscreds.txt", "Resource file deleted. (AWS Creds)");
            File.AppendAllText($@"{_dir}\AWS\overwrite_presign.txt", "Not overwriten. (Presign)");
            File.AppendAllText($@"{_dir}\AWS\overwrite_awscreds.txt", "Not overwriten. (AWS creds)");
        }

        /// <summary>
        /// Delete files from main folder to cause exeption in PreSignedNoFileInTopDirectoryTest().
        /// </summary>
        private void DeleteMainDirectoryFiles()
        {
            File.Delete($@"{_dir}\AWS\test1.txt");
            File.Delete($@"{_dir}\AWS\deletethis_presign.txt");
            File.Delete($@"{_dir}\AWS\deletethis_awscreds.txt");
            File.Delete($@"{_dir}\AWS\overwrite_presign.txt");
            File.Delete($@"{_dir}\AWS\overwrite_awscreds.txt");
        }

        /// <summary>
        /// Delete test files and folders.
        /// </summary>
        private void DeleteSourcePath()
        {
            Directory.Delete($@"{_dir}\AWS", true);
        }
        #endregion HelperMethods

    }
}