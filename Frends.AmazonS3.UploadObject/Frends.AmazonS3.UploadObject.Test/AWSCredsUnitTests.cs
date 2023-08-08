using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Frends.AmazonS3.UploadObject.Definitions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Frends.AmazonS3.UploadObject.Tests;

[TestClass]
public class AWSCredsUnitTests
{
    public TestContext? TestContext { get; set; }
    private readonly string? _accessKey = Environment.GetEnvironmentVariable("HiQ_AWSS3Test_AccessKey");
    private readonly string? _secretAccessKey = Environment.GetEnvironmentVariable("HiQ_AWSS3Test_SecretAccessKey");
    private readonly string? _bucketName = Environment.GetEnvironmentVariable("HiQ_AWSS3Test_BucketName");
    private readonly string _dir = Path.Combine(Environment.CurrentDirectory);
    Connection? _connection;
    Input? _input;

    [TestInitialize]
    public void Initialize()
    {
        Directory.CreateDirectory($@"{_dir}\AWS");
        Directory.CreateDirectory($@"{_dir}\AWS\Subfolder");

        File.AppendAllText($@"{_dir}\AWS\test1.txt", "test1");
        File.AppendAllText($@"{_dir}\AWS\Subfolder\subfile.txt", "From subfolder.");
        File.AppendAllText($@"{_dir}\AWS\deletethis_awscreds.txt", "Resource file deleted. (AWS Creds)");
        File.AppendAllText($@"{_dir}\AWS\overwrite_presign.txt", "Not overwriten. (Presign)");
        File.AppendAllText($@"{_dir}\AWS\overwrite_awscreds.txt", "Not overwriten. (AWS creds)");
    }

    [TestCleanup]
    public async Task CleanUp()
    {
        if (Directory.Exists($@"{_dir}\AWS"))
            Directory.Delete($@"{_dir}\AWS", true);

        using var client = new AmazonS3Client(_accessKey, _secretAccessKey, RegionEndpoint.EUCentral1);

        var listObjectRequest = new ListObjectsRequest
        {
            BucketName = _bucketName,
        };

        var response = await client.ListObjectsAsync(listObjectRequest);
        var objects = response.S3Objects;

        foreach (var obj in objects)
        {
            var deleteObjectRequest = new DeleteObjectRequest
            {
                BucketName = obj.BucketName,
                Key = obj.Key,
            };
            await client.DeleteObjectAsync(deleteObjectRequest);
        }
    }

    [TestMethod]
    public async Task AWSCreds_Upload()
    {
        _input = new Input
        {
            FilePath = $@"{_dir}\AWS",
            ACL = default,
            FileMask = null,
            UseACL = false,
            S3Directory = "Upload2023/",
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
            UseMultipartUpload = false,
        };

        var result = await AmazonS3.UploadObject(_connection, _input, default);
        Assert.AreEqual(5, result.UploadedObjects.Count);
        Assert.IsTrue(result.Success);
        Assert.IsNotNull(result.DebugLog);
        Assert.IsTrue(result.UploadedObjects.Any(x => x.Contains("deletethis_awscreds.txt")));
    }

    [TestMethod]
    public async Task AWSCreds_Missing_ThrowExceptionOnErrorResponse_False()
    {
        _input = new Input
        {
            FilePath = $@"{_dir}\AWS",
            ACL = default,
            FileMask = null,
            UseACL = false,
            S3Directory = "Upload2023/",
        };
        _connection = new Connection
        {
            AuthenticationMethod = AuthenticationMethod.AWSCredentials,
            PreSignedURL = null,
            AwsAccessKeyId = null,
            AwsSecretAccessKey = "",
            BucketName = _bucketName,
            Region = Region.EuCentral1,
            UploadFromCurrentDirectoryOnly = false,
            Overwrite = false,
            PreserveFolderStructure = false,
            ReturnListOfObjectKeys = false,
            DeleteSource = false,
            ThrowErrorIfNoMatch = false,
            ThrowExceptionOnErrorResponse = false,
            UseMultipartUpload = false,
        };

        var result = await AmazonS3.UploadObject(_connection, _input, default);
        Assert.AreEqual(0, result.UploadedObjects.Count);
        Assert.IsFalse(result.Success);
        Assert.IsTrue(result.DebugLog.Contains("Access Denied"));
    }

    [TestMethod]
    public async Task AWSCreds_Missing_ThrowExceptionOnErrorResponse_True()
    {
        _input = new Input
        {
            FilePath = $@"{_dir}\AWS",
            ACL = default,
            FileMask = null,
            UseACL = false,
            S3Directory = "Upload2023/",
        };
        _connection = new Connection
        {
            AuthenticationMethod = AuthenticationMethod.AWSCredentials,
            PreSignedURL = null,
            AwsAccessKeyId = null,
            AwsSecretAccessKey = "",
            BucketName = _bucketName,
            Region = Region.EuCentral1,
            UploadFromCurrentDirectoryOnly = false,
            Overwrite = false,
            PreserveFolderStructure = false,
            ReturnListOfObjectKeys = false,
            DeleteSource = false,
            ThrowErrorIfNoMatch = false,
            ThrowExceptionOnErrorResponse = true,
            UseMultipartUpload = false,
        };

        var ex = await Assert.ThrowsExceptionAsync<UploadException>(async () => await AmazonS3.UploadObject(_connection, _input, default));
        Assert.IsTrue(ex.DebugLog.Contains("Access Denied"));
    }

    [TestMethod]
    public async Task AWSCreds_UploadFromCurrentDirectoryOnly()
    {
        _input = new Input
        {
            FilePath = $@"{_dir}\AWS",
            ACL = default,
            FileMask = null,
            UseACL = false,
            S3Directory = "Upload2023/",
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
            UseMultipartUpload = false,
        };

        var result = await AmazonS3.UploadObject(_connection, _input, default);
        Assert.AreEqual(4, result.UploadedObjects.Count);
        Assert.IsTrue(result.Success);
        Assert.IsNotNull(result.DebugLog);
        Assert.IsTrue(result.UploadedObjects.Any(x => x.Contains("deletethis_awscreds.txt")));
    }

    [TestMethod]
    public async Task AWSCreds_Overwrite()
    {
        _input = new Input
        {
            FilePath = $@"{_dir}\AWS",
            ACL = default,
            FileMask = null,
            UseACL = false,
            S3Directory = "Upload2023/",
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
            UseMultipartUpload = false,
        };

        var result = await AmazonS3.UploadObject(_connection, _input, default);
        Assert.AreEqual(5, result.UploadedObjects.Count);
        Assert.IsTrue(result.Success);
        Assert.IsNotNull(result.DebugLog);
        Assert.IsTrue(result.UploadedObjects.Any(x => x.Contains("deletethis_awscreds.txt")));
    }

    [TestMethod]
    public async Task AWSCreds_PreserveFolderStructure()
    {
        _input = new Input
        {
            FilePath = $@"{_dir}\AWS",
            ACL = default,
            FileMask = null,
            UseACL = false,
            S3Directory = "Upload2023/",
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
            UseMultipartUpload = false,
        };

        var result = await AmazonS3.UploadObject(_connection, _input, default);
        Assert.AreEqual(5, result.UploadedObjects.Count);
        Assert.IsTrue(result.Success);
        Assert.IsNotNull(result.DebugLog);
        Assert.IsTrue(result.UploadedObjects.Any(x => x.Contains("deletethis_awscreds.txt")));
    }

    [TestMethod]
    public async Task AWSCreds_ReturnListOfObjectKeys()
    {
        _input = new Input
        {
            FilePath = $@"{_dir}\AWS",
            ACL = default,
            FileMask = null,
            UseACL = false,
            S3Directory = "Upload2023/",
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
            UseMultipartUpload = false,
        };

        var result = await AmazonS3.UploadObject(_connection, _input, default);
        Assert.AreEqual(5, result.UploadedObjects.Count);
        Assert.IsFalse(result.UploadedObjects.Any(x => x.Contains("C:")));
        Assert.IsTrue(result.Success);
        Assert.IsNotNull(result.DebugLog);
        Assert.IsTrue(result.UploadedObjects.Any(x => x.Contains("deletethis_awscreds.txt")));
    }

    [TestMethod]
    public async Task AWSCreds_DeleteSourceFile_Mask()
    {
        var fileName = "deletethis_awscreds.txt";
        _input = new Input
        {
            FilePath = $@"{_dir}\AWS",
            ACL = default,
            FileMask = fileName,
            UseACL = false,
            S3Directory = "Upload2023/",
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
            UseMultipartUpload = false,
        };

        var result = await AmazonS3.UploadObject(_connection, _input, default);
        Assert.AreEqual(1, result.UploadedObjects.Count);
        Assert.IsTrue(result.Success);
        Assert.IsNotNull(result.DebugLog);
        Assert.IsTrue(result.UploadedObjects.Any(x => x.Contains("deletethis_awscreds.txt")));
        Assert.IsFalse(File.Exists($@"{_dir}\AWS\{fileName}"));
    }

    [TestMethod]
    public async Task AWSCreds_ThrowErrorIfNoMatch()
    {
        _input = new Input
        {
            FilePath = $@"{_dir}\AWS",
            ACL = default,
            FileMask = "notafile*",
            UseACL = false,
            S3Directory = "Upload2023/",
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
            ThrowExceptionOnErrorResponse = false,
            UseMultipartUpload = false,
        };

        var ex = await Assert.ThrowsExceptionAsync<Exception>(async () => await AmazonS3.UploadObject(_connection, _input, default));
        Assert.IsTrue(ex.Message.Contains($"No files match the filemask '{_input.FileMask}' within supplied path."));
    }

    [TestMethod]
    public async Task AWSCreds_ACLs()
    {
        var acls = new List<ACLs>() { ACLs.Private, ACLs.PublicRead, ACLs.PublicReadWrite, ACLs.AuthenticatedRead, ACLs.BucketOwnerRead, ACLs.BucketOwnerFullControl, ACLs.LogDeliveryWrite };

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
            UseMultipartUpload = false,
        };

        foreach (var acl in acls)
        {
            _input = new Input
            {
                FilePath = $@"{_dir}\AWS",
                ACL = acl,
                FileMask = null,
                UseACL = true,
                S3Directory = "Upload2023/",
            };

            var result = await AmazonS3.UploadObject(_connection, _input, default);
            Assert.AreEqual(5, result.UploadedObjects.Count);
            Assert.IsTrue(result.Success);
            Assert.IsNotNull(result.DebugLog);
            Assert.IsTrue(result.UploadedObjects.Any(x => x.Contains("deletethis_awscreds.txt")));

            await CleanUp();
            Initialize();
        }
    }
}