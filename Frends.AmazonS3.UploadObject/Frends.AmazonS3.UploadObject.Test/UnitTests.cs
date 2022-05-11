using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon;
using System.IO;
using Frends.AmazonS3.UploadObject.Definitions;

namespace Frends.AmazonS3.UploadObject.Tests;

[TestClass]
public class UnitTests
{
    public TestContext? TestContext { get; set; }

    private readonly string? _accessKey = Environment.GetEnvironmentVariable("HiQ_AWSS3Test_AccessKey");
    private readonly string? _secretAccessKey = Environment.GetEnvironmentVariable("HiQ_AWSS3Test_SecretAccessKey");
    private readonly string? _bucketName = Environment.GetEnvironmentVariable("HiQ_AWSS3Test_BucketName");
    private readonly string _dir = Path.Combine(Environment.CurrentDirectory);


    Connection? _connection;
    Input? _input;

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


    #region PreSigned
    /// <summary>
    /// PreSigned URL testing.
    /// Presigned URL can be used just once and for 1 file.
    /// </summary>
    [TestMethod]
    public void PreSignedUploadTest()
    {
        var setS3Key = $"Upload2022/PreSigned/UploadTest.txt"; //S3 "location".
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
            UploadFromCurrentDirectoryOnly = false,
            Overwrite = false,
            PreserveFolderStructure = false,
            ReturnListOfObjectKeys = false,
            DeleteSource = false,
            ThrowErrorIfNoMatch = false,
        };

        var result = AmazonS3.UploadObject(_connection, _input, default);
        Assert.IsNotNull(result.Result.Results);
    }

    /// <summary>
    /// PreSigned URL testing. Missing URL and ends up to exception before checking files etcc.
    /// </summary>
    [TestMethod]
    public void PreSignedMissingTest()
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
            UploadFromCurrentDirectoryOnly = false,
            Overwrite = false,
            PreserveFolderStructure = false,
            ReturnListOfObjectKeys = false,
            DeleteSource = false,
            ThrowErrorIfNoMatch = false,
        };

        var ex = Assert.ThrowsExceptionAsync<Exception>(async () => await AmazonS3.UploadObject(_connection, _input, default)).Result;
        Assert.IsTrue(ex.Message.Contains("AWS pre-signed URL required."));
    }

    /// <summary>
    /// Presigned URL with UploadFromCurrentDirectoryOnly and ThrowErrorIfNoMatch = true
    /// Presigned URL can be used just once and for 1 file upload -> DeleteMainDirectoryFiles() makes sure that only subdirectory contains the file and top directory is empty.
    /// </summary>
    [TestMethod]
    public void PreSignedNoFileInTopDirectoryTest()
    {
        File.Delete($@"{_dir}\AWS\test1.txt");
        File.Delete($@"{_dir}\AWS\deletethis_presign.txt");
        File.Delete($@"{_dir}\AWS\deletethis_awscreds.txt");
        File.Delete($@"{_dir}\AWS\overwrite_presign.txt");
        File.Delete($@"{_dir}\AWS\overwrite_awscreds.txt");

        var setS3Key = $"Upload2022/PreSigned/PreSignedNoFileInTopDirectoryTest.txt"; //Location where file will be uploaded.

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
            UploadFromCurrentDirectoryOnly = true,
            ThrowErrorIfNoMatch = true,
            Overwrite = false,
            PreserveFolderStructure = false,
            ReturnListOfObjectKeys = false,
            DeleteSource = false,
        };

        var ex = Assert.ThrowsExceptionAsync<Exception>(async () => await AmazonS3.UploadObject(_connection, _input, default)).Result;
        Assert.IsTrue(ex.Message.Contains("No files match the filemask within supplied path."));
    }

    /// <summary>
    /// Presigned URL with FilePath not found.
    /// </summary>
    [TestMethod]
    public void PreSignedNoPathTest()
    {
        var setS3Key = $"Upload2022/PreSigned/PreSignedNoPath.txt"; //Location where file will be uploaded. This file should not exists in S3.
        _input = new Input
        {
            FilePath = "c:/nofileshere/",
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
            UploadFromCurrentDirectoryOnly = true,
            ThrowErrorIfNoMatch = true,
            Overwrite = false,
            PreserveFolderStructure = false,
            ReturnListOfObjectKeys = false,
            DeleteSource = false,
        };

        var ex = Assert.ThrowsExceptionAsync<Exception>(async () => await AmazonS3.UploadObject(_connection, _input, default)).Result;
        Assert.IsTrue(ex.Message.Contains("Source path not found."));
    }

    /// <summary>
    /// Presigned URL can be used just once and for 1 file so only 1 file will be uploaded and deleted.
    /// Also testing FileMask.
    /// Test asserts that file has been uploaded (result ok) and file doesn't exists in source directory but you can comment CleanUp() and check it out manually.
    /// </summary>
    [TestMethod]
    public void PreSignedDeleteSourceFileMaskTest()
    {
        var fileName = "deletethis_presign.txt";
        var setS3Key = $"Upload2022/PreSigned/{fileName}"; //Location where file will be uploaded. test1.txt should be deleted from source directory.
        _input = new Input
        {
            FilePath = $@"{_dir}\AWS",
            ACL = default,
            FileMask = "deletethis_presign.txt",
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
            DeleteSource = true,
            UploadFromCurrentDirectoryOnly = false,
            ThrowErrorIfNoMatch = false,
            Overwrite = false,
            PreserveFolderStructure = false,
            ReturnListOfObjectKeys = false,
        };

        var result = AmazonS3.UploadObject(_connection, _input, default);
        Assert.IsNotNull(result.Result.Results);
        Assert.IsTrue(!File.Exists($@"{_dir}\AWS\{fileName}"));
    }
    #endregion PreSigned

    #region AWS Creds

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
            S3Directory = "Upload2022/AWSCreds/UploadTest/",
        };
        _connection = new Connection
        {
            AuthenticationMethod = AuthenticationMethod.AWSCredentials,
            PreSignedURL = null,
            AwsAccessKeyId = _accessKey,
            AwsSecretAccessKey = _secretAccessKey,
            BucketName = _bucketName,
            Region = Region.EuCentral1,
            UploadFromCurrentDirectoryOnly = false,
            Overwrite = false,
            PreserveFolderStructure = false,
            ReturnListOfObjectKeys = false,
            DeleteSource = false,
            ThrowErrorIfNoMatch = false,
        };

        var result = AmazonS3.UploadObject(_connection, _input, default);
        Assert.IsNotNull(result.Result.Results);
    }

    /// <summary>
    /// AWS creds missing. Ends up to exception before checking files etcc.
    /// </summary>
    [TestMethod]
    public void AWSCredsMissingTest()
    {
        _input = new Input
        {
            FilePath = $@"{_dir}\AWS",
            ACL = default,
            FileMask = null,
            UseACL = false,
            S3Directory = "Upload2022/AWSCreds/ThisShouldntExistsInS3_AWSCredsMissing/",
        };
        _connection = new Connection
        {
            AuthenticationMethod = AuthenticationMethod.AWSCredentials,
            PreSignedURL = null,
            AwsAccessKeyId = null,
            AwsSecretAccessKey = _secretAccessKey,
            BucketName = _bucketName,
            Region = Region.EuCentral1,
            UploadFromCurrentDirectoryOnly = false,
            Overwrite = false,
            PreserveFolderStructure = false,
            ReturnListOfObjectKeys = false,
            DeleteSource = false,
            ThrowErrorIfNoMatch = false,
        };

        var ex = Assert.ThrowsExceptionAsync<Exception>(async () => await AmazonS3.UploadObject(_connection, _input, default)).Result;
        Assert.IsTrue(ex.Message.Contains("AWS Access Key Id and Secret Access Key required."));
    }

    /// <summary>
    /// AWS creds testing with UploadFromCurrentDirectoryOnly = true. S3 locations shouldn't contain subfile.txt.
    /// </summary>
    [TestMethod]
    public void AWSCredsUploadFromCurrentDirectoryOnlyTest()
    {
        _input = new Input
        {
            FilePath = $@"{_dir}\AWS",
            ACL = default,
            FileMask = null,
            UseACL = false,
            S3Directory = "Upload2022/AWSCreds/AWSCredsUploadFromCurrentDirectoryOnly/",
        };
        _connection = new Connection
        {
            AuthenticationMethod = AuthenticationMethod.AWSCredentials,
            PreSignedURL = null,
            AwsAccessKeyId = _accessKey,
            AwsSecretAccessKey = _secretAccessKey,
            BucketName = _bucketName,
            Region = Region.EuCentral1,
            UploadFromCurrentDirectoryOnly = true,
            Overwrite = false,
            PreserveFolderStructure = false,
            ReturnListOfObjectKeys = false,
            DeleteSource = false,
            ThrowErrorIfNoMatch = false,
        };

        var result = AmazonS3.UploadObject(_connection, _input, default);
        Assert.IsNotNull(result.Result.Results);
    }

    /// <summary>
    /// AWS creds testing with Overwrite = true. Run this test 2 times so 2nd time will be overwrite.
    /// </summary>
    [TestMethod]
    public void AWSCredsOverwriteTest()
    {
        _input = new Input
        {
            FilePath = $@"{_dir}\AWS",
            ACL = default,
            FileMask = null,
            UseACL = false,
            S3Directory = "Upload2022/AWSCreds/Overwrite/",
        };
        _connection = new Connection
        {
            AuthenticationMethod = AuthenticationMethod.AWSCredentials,
            PreSignedURL = null,
            AwsAccessKeyId = _accessKey,
            AwsSecretAccessKey = _secretAccessKey,
            BucketName = _bucketName,
            Region = Region.EuCentral1,
            Overwrite = true,
            UploadFromCurrentDirectoryOnly = false,
            PreserveFolderStructure = false,
            ReturnListOfObjectKeys = false,
            DeleteSource = false,
            ThrowErrorIfNoMatch = false,
        };

        var result = AmazonS3.UploadObject(_connection, _input, default);
        Assert.IsNotNull(result.Result.Results);
    }

    /// <summary>
    /// AWS creds testing with PreserveFolderStructure = true. 
    /// </summary>
    [TestMethod]
    public void AWSCredsPreserveFolderStructureTest()
    {
        _input = new Input
        {
            FilePath = $@"{_dir}\AWS",
            ACL = default,
            FileMask = null,
            UseACL = false,
            S3Directory = "Upload2022/AWSCreds/PreserveFolderStructure/",
        };
        _connection = new Connection
        {
            AuthenticationMethod = AuthenticationMethod.AWSCredentials,
            PreSignedURL = null,
            AwsAccessKeyId = _accessKey,
            AwsSecretAccessKey = _secretAccessKey,
            BucketName = _bucketName,
            Region = Region.EuCentral1,
            PreserveFolderStructure = true,
            Overwrite = false,
            UploadFromCurrentDirectoryOnly = false,
            ReturnListOfObjectKeys = false,
            DeleteSource = false,
            ThrowErrorIfNoMatch = false,
        };

        var result = AmazonS3.UploadObject(_connection, _input, default);
        Assert.IsNotNull(result.Result.Results);
    }

    /// <summary>
    /// AWS creds testing with PreserveFolderStructure = true. Check result.
    /// </summary>
    [TestMethod]
    public void AWSCredsReturnListOfObjectKeysTest()
    {
        _input = new Input
        {
            FilePath = $@"{_dir}\AWS",
            ACL = default,
            FileMask = null,
            UseACL = false,
            S3Directory = "Upload2022/AWSCreds/AWSCredsReturnListOfObjectKeys/",
        };
        _connection = new Connection
        {
            AuthenticationMethod = AuthenticationMethod.AWSCredentials,
            PreSignedURL = null,
            AwsAccessKeyId = _accessKey,
            AwsSecretAccessKey = _secretAccessKey,
            BucketName = _bucketName,
            Region = Region.EuCentral1,
            ReturnListOfObjectKeys = true,
            PreserveFolderStructure = false,
            Overwrite = true,
            UploadFromCurrentDirectoryOnly = false,
            DeleteSource = false,
            ThrowErrorIfNoMatch = false,
        };

        var result = AmazonS3.UploadObject(_connection, _input, default);
        Assert.IsNotNull(result.Result.Results);
    }

    /// <summary>
    /// AWS creds testing with DeleteSource = true + FileMask.
    /// Also testing FileMask.
    /// Test asserts that file has been uploaded (result ok) and file doesn't exists in source directory but you can comment CleanUp() and check it out manually.
    /// </summary>
    [TestMethod]
    public void AWSCredsDeleteSourceFileMaskTest()
    {
        var fileName = "deletethis_awscreds.txt";
        _input = new Input
        {
            FilePath = $@"{_dir}\AWS",
            ACL = default,
            FileMask = fileName,
            UseACL = false,
            S3Directory = "Upload2022/AWSCreds/DeleteSource/",
        };
        _connection = new Connection
        {
            AuthenticationMethod = AuthenticationMethod.AWSCredentials,
            PreSignedURL = null,
            AwsAccessKeyId = _accessKey,
            AwsSecretAccessKey = _secretAccessKey,
            BucketName = _bucketName,
            Region = Region.EuCentral1,
            DeleteSource = true,
            ReturnListOfObjectKeys = false,
            PreserveFolderStructure = false,
            Overwrite = false,
            UploadFromCurrentDirectoryOnly = false,
            ThrowErrorIfNoMatch = false,
        };

        var result = AmazonS3.UploadObject(_connection, _input, default);
        Assert.IsNotNull(result.Result.Results);
        Assert.IsTrue(!File.Exists($@"{_dir}\AWS\{fileName}"));
    }

    /// <summary>
    /// AWS creds testing with AWSCredsThrowErrorIfNoMatch = true.
    /// </summary>
    [TestMethod]
    public void AWSCredsThrowErrorIfNoMatchTest()
    {
        _input = new Input
        {
            FilePath = $@"{_dir}\AWS",
            ACL = default,
            FileMask = "notafile*",
            UseACL = false,
            S3Directory = "Upload2022/AWSCreds/ThisShouldntExistsInS3_AWSCredsThrowErrorIfNoMatch/",
        };
        _connection = new Connection
        {
            AuthenticationMethod = AuthenticationMethod.AWSCredentials,
            PreSignedURL = null,
            AwsAccessKeyId = _accessKey,
            AwsSecretAccessKey = _secretAccessKey,
            BucketName = _bucketName,
            Region = Region.EuCentral1,
            ThrowErrorIfNoMatch = true,
            DeleteSource = false,
            ReturnListOfObjectKeys = false,
            PreserveFolderStructure = false,
            Overwrite = false,
            UploadFromCurrentDirectoryOnly = false,
        };

        var ex = Assert.ThrowsExceptionAsync<Exception>(async () => await AmazonS3.UploadObject(_connection, _input, default)).Result;
        Assert.IsTrue(ex.Message.Contains("No files match the filemask within supplied path."));
    }

    /// <summary>
    /// AWS creds testing.
    /// ACL Private.
    /// </summary>
    [TestMethod]
    public void AWSCredsACLPrivateTest()
    {
        _input = new Input
        {
            FilePath = $@"{_dir}\AWS",
            ACL = ACLs.Private,
            FileMask = null,
            UseACL = true,
            S3Directory = "Upload2022/AWSCreds/ACLPrivate/",
        };
        _connection = new Connection
        {
            AuthenticationMethod = AuthenticationMethod.AWSCredentials,
            PreSignedURL = null,
            AwsAccessKeyId = _accessKey,
            AwsSecretAccessKey = _secretAccessKey,
            BucketName = _bucketName,
            Region = Region.EuCentral1,
            UploadFromCurrentDirectoryOnly = false,
            Overwrite = false,
            PreserveFolderStructure = false,
            ReturnListOfObjectKeys = false,
            DeleteSource = false,
            ThrowErrorIfNoMatch = false,
        };

        var result = AmazonS3.UploadObject(_connection, _input, default);
        Assert.IsNotNull(result.Result.Results);
    }

    /// <summary>
    /// AWS creds testing.
    /// ACL PublicRead.
    /// </summary>
    [TestMethod]
    public void AWSCredsACLPublicReadTest()
    {
        _input = new Input
        {
            FilePath = $@"{_dir}\AWS",
            ACL = ACLs.PublicRead,
            FileMask = null,
            UseACL = true,
            S3Directory = "Upload2022/AWSCreds/ACLPublicRead/",
        };
        _connection = new Connection
        {
            AuthenticationMethod = AuthenticationMethod.AWSCredentials,
            PreSignedURL = null,
            AwsAccessKeyId = _accessKey,
            AwsSecretAccessKey = _secretAccessKey,
            BucketName = _bucketName,
            Region = Region.EuCentral1,
            UploadFromCurrentDirectoryOnly = false,
            Overwrite = false,
            PreserveFolderStructure = false,
            ReturnListOfObjectKeys = false,
            DeleteSource = false,
            ThrowErrorIfNoMatch = false,
        };

        var result = AmazonS3.UploadObject(_connection, _input, default);
        Assert.IsNotNull(result.Result.Results);
    }

    /// <summary>
    /// AWS creds testing.
    /// ACL PublicReadWrite.
    /// </summary>
    [TestMethod]
    public void AWSCredsACLPublicReadWritedTest()
    {
        _input = new Input
        {
            FilePath = $@"{_dir}\AWS",
            ACL = ACLs.PublicReadWrite,
            FileMask = null,
            UseACL = true,
            S3Directory = "Upload2022/AWSCreds/ACLPublicReadWrite/",
        };
        _connection = new Connection
        {
            AuthenticationMethod = AuthenticationMethod.AWSCredentials,
            PreSignedURL = null,
            AwsAccessKeyId = _accessKey,
            AwsSecretAccessKey = _secretAccessKey,
            BucketName = _bucketName,
            Region = Region.EuCentral1,
            UploadFromCurrentDirectoryOnly = false,
            Overwrite = false,
            PreserveFolderStructure = false,
            ReturnListOfObjectKeys = false,
            DeleteSource = false,
            ThrowErrorIfNoMatch = false,
        };

        var result = AmazonS3.UploadObject(_connection, _input, default);
        Assert.IsNotNull(result.Result.Results);
    }

    /// <summary>
    /// AWS creds testing.
    /// ACL AuthenticatedRead.
    /// </summary>
    [TestMethod]
    public void AWSCredsACLAuthenticatedReadTest()
    {
        _input = new Input
        {
            FilePath = $@"{_dir}\AWS",
            ACL = ACLs.AuthenticatedRead,
            FileMask = null,
            UseACL = true,
            S3Directory = "Upload2022/AWSCreds/ACLAuthenticatedRead/",
        };
        _connection = new Connection
        {
            AuthenticationMethod = AuthenticationMethod.AWSCredentials,
            PreSignedURL = null,
            AwsAccessKeyId = _accessKey,
            AwsSecretAccessKey = _secretAccessKey,
            BucketName = _bucketName,
            Region = Region.EuCentral1,
            UploadFromCurrentDirectoryOnly = false,
            Overwrite = false,
            PreserveFolderStructure = false,
            ReturnListOfObjectKeys = false,
            DeleteSource = false,
            ThrowErrorIfNoMatch = false,
        };

        var result = AmazonS3.UploadObject(_connection, _input, default);
        Assert.IsNotNull(result.Result.Results);
    }

    /// <summary>
    /// AWS creds testing.
    /// ACL BucketOwnerRead.
    /// </summary>
    [TestMethod]
    public void AWSCredsACLBucketOwnerReadTest()
    {
        _input = new Input
        {
            FilePath = $@"{_dir}\AWS",
            ACL = ACLs.BucketOwnerRead,
            FileMask = null,
            UseACL = true,
            S3Directory = "Upload2022/AWSCreds/ACLBucketOwnerRead/",
        };
        _connection = new Connection
        {
            AuthenticationMethod = AuthenticationMethod.AWSCredentials,
            PreSignedURL = null,
            AwsAccessKeyId = _accessKey,
            AwsSecretAccessKey = _secretAccessKey,
            BucketName = _bucketName,
            Region = Region.EuCentral1,
            UploadFromCurrentDirectoryOnly = false,
            Overwrite = false,
            PreserveFolderStructure = false,
            ReturnListOfObjectKeys = false,
            DeleteSource = false,
            ThrowErrorIfNoMatch = false,
        };

        var result = AmazonS3.UploadObject(_connection, _input, default);
        Assert.IsNotNull(result.Result.Results);
    }

    /// <summary>
    /// AWS creds testing.
    /// ACL BucketOwnerFullControl.
    /// </summary>
    [TestMethod]
    public void AWSCredsACLBucketOwnerFullControlTest()
    {
        _input = new Input
        {
            FilePath = $@"{_dir}\AWS",
            ACL = ACLs.BucketOwnerFullControl,
            FileMask = null,
            UseACL = true,
            S3Directory = "Upload2022/AWSCreds/ACLBucketOwnerFullControl/",
        };
        _connection = new Connection
        {
            AuthenticationMethod = AuthenticationMethod.AWSCredentials,
            PreSignedURL = null,
            AwsAccessKeyId = _accessKey,
            AwsSecretAccessKey = _secretAccessKey,
            BucketName = _bucketName,
            Region = Region.EuCentral1,
            UploadFromCurrentDirectoryOnly = false,
            Overwrite = false,
            PreserveFolderStructure = false,
            ReturnListOfObjectKeys = false,
            DeleteSource = false,
            ThrowErrorIfNoMatch = false,
        };

        var result = AmazonS3.UploadObject(_connection, _input, default);
        Assert.IsNotNull(result.Result.Results);
    }

    /// <summary>
    /// AWS creds testing.
    /// ACL LogDeliveryWrite.
    /// </summary>
    [TestMethod]
    public void AWSCredsACLLogDeliveryWriteTest()
    {
        _input = new Input
        {
            FilePath = $@"{_dir}\AWS",
            ACL = ACLs.LogDeliveryWrite,
            FileMask = null,
            UseACL = true,
            S3Directory = "Upload2022/AWSCreds/ACLLogDeliveryWrite/",
        };
        _connection = new Connection
        {
            AuthenticationMethod = AuthenticationMethod.AWSCredentials,
            PreSignedURL = null,
            AwsAccessKeyId = _accessKey,
            AwsSecretAccessKey = _secretAccessKey,
            BucketName = _bucketName,
            Region = Region.EuCentral1,
            UploadFromCurrentDirectoryOnly = false,
            Overwrite = false,
            PreserveFolderStructure = false,
            ReturnListOfObjectKeys = false,
            DeleteSource = false,
            ThrowErrorIfNoMatch = false,
        };

        var result = AmazonS3.UploadObject(_connection, _input, default);
        Assert.IsNotNull(result.Result.Results);
    }

    #endregion AWS Creds


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
    /// Delete test files and folders.
    /// </summary>
    private void DeleteSourcePath()
    {
        Directory.Delete($@"{_dir}\AWS", true);
    }

}
