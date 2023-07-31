using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;
using Frends.AmazonS3.DeleteBucket.Definitions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace Frends.AmazonS3.DeleteBucket.Tests;

[TestClass]
public class AWSCredsUnitTests
{
    private readonly string? _accessKey = Environment.GetEnvironmentVariable("HiQ_AWSS3Test_AccessKey");
    private readonly string? _secretAccessKey = Environment.GetEnvironmentVariable("HiQ_AWSS3Test_SecretAccessKey");
    private Connection _connection = new();
    private readonly string _bucketName = "ritteambuckettest";

    [TestInitialize]
    public async Task Init()
    {
        _connection = new Connection
        {
            AwsAccessKeyId = _accessKey,
            AwsSecretAccessKey = _secretAccessKey,
            Region = Region.EuCentral1,
            BucketName = _bucketName,
        };

        using IAmazonS3 s3Client = new AmazonS3Client(_accessKey, _secretAccessKey, RegionEndpoint.EUCentral1);
        if (!await AmazonS3Util.DoesS3BucketExistV2Async(s3Client, _bucketName))
            await s3Client.PutBucketAsync(_bucketName);
    }

    [TestCleanup]
    public async Task CleanUp()
    {
        try
        {
            using IAmazonS3 s3Client = new AmazonS3Client(_connection.AwsAccessKeyId, _connection.AwsSecretAccessKey, RegionEndpoint.EUCentral1);
            if (await AmazonS3Util.DoesS3BucketExistV2Async(s3Client, _bucketName))
            {
                var request = new DeleteBucketRequest
                {
                    BucketName = _bucketName
                };

                await s3Client.DeleteBucketAsync(request);
            }
        }
        catch (AmazonS3Exception ex)
        {
            throw new AmazonS3Exception(ex.Message);
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }

    [TestMethod]
    public async Task DeleteBucket_SuccessTest()
    {
        var result = await AmazonS3.DeleteBucket(_connection, default);
        Assert.IsTrue(result.Success);
        Assert.IsNull(result.Data);
    }

    [TestMethod]
    public async Task DeleteBucket_BucketAlreadyExistsTest()
    {
        var result = await AmazonS3.DeleteBucket(_connection, default);
        Assert.IsTrue(result.Success);
        Assert.IsNull(result.Data);

        var result2 = await AmazonS3.DeleteBucket(_connection, default);
        Assert.IsTrue(result2.Success);
        Assert.AreEqual("Bucket to be deleted, does not exist.", result2.Data);
    }

    [TestMethod]
    public async Task DeleteBucket_ExceptionHandlingTest()
    {
        var connection = new Connection
        {
            AwsSecretAccessKey = "foobar",
            AwsAccessKeyId = "foobar",
            BucketName = _bucketName,
            Region = Region.EuCentral1,
        };

        var ex = await Assert.ThrowsExceptionAsync<AmazonS3Exception>(() => AmazonS3.DeleteBucket(connection, default));
        Assert.IsNotNull(ex.InnerException);
    }
}