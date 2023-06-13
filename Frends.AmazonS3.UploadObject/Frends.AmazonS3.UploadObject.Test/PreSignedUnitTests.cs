using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Frends.AmazonS3.UploadObject.Definitions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Frends.AmazonS3.UploadObject.Tests;

[TestClass]
public class PreSignedUnitTests
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
        File.AppendAllText($@"{_dir}\AWS\deletethis_presign.txt", "Resource file deleted. (Presign)");
    }

    [TestCleanup]
    public void CleanUp()
    {
        if (Directory.Exists($@"{_dir}\AWS"))
            Directory.Delete($@"{_dir}\AWS", true);

        using var sw = new StringWriter();
        using var client = new AmazonS3Client(_accessKey, _secretAccessKey, RegionEndpoint.EUCentral1);

        var deleteObjectRequest = new DeleteObjectRequest
        {
            BucketName = _bucketName,
            Key = "Upload2023/PreSigned/UploadTest.txt"
        };
        client.DeleteObjectAsync(deleteObjectRequest);
    }

    [TestMethod]
    public async Task PreSignedUnitTest_UploadObject()
    {
        var setS3Key = $"Upload2023/PreSigned/UploadTest.txt";

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

        var result = await AmazonS3.UploadObject(_connection, _input, default);
        Assert.AreEqual(1, result.UploadedObjects.Count);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(string.Empty, result.DebugLog);
        Assert.IsTrue(result.UploadedObjects.Any(x => x.Contains("deletethis_presign.txt")));
    }

    [TestMethod]
    public async Task PreSignedUnitTest_MissingURL_ThrowExceptionOnErrorResponse_false()
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
            ThrowExceptionOnErrorResponse = false,
        };

        var result = await AmazonS3.UploadObject(_connection, _input, default);
        Assert.AreEqual(0, result.UploadedObjects.Count);
        Assert.IsFalse(result.Success);
        Assert.IsTrue(result.DebugLog.Contains("Invalid URI: The format of the URI could not be determined"));
    }

    [TestMethod]
    public async Task PreSignedUnitTest_MissingURL_ThrowExceptionOnErrorResponse_true()
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
            ThrowExceptionOnErrorResponse = true,
        };

        var ex = await Assert.ThrowsExceptionAsync<UploadException>(async () => await AmazonS3.UploadObject(_connection, _input, default));
        Assert.IsTrue(ex.Message.Contains("Invalid URI: The format of the URI could not be determined"));
    }

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
}