using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using Amazon.S3;
using Amazon;
using Amazon.S3.Model;
using Frends.AmazonS3.DownloadObject.Definitions;
using Xunit.Sdk;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Frends.AmazonS3.DownloadObject.Tests;

[TestClass]
public class UnitTests
{
    private readonly string? _accessKey = Environment.GetEnvironmentVariable("HiQ_AWSS3Test_AccessKey");
    private readonly string? _secretAccessKey = Environment.GetEnvironmentVariable("HiQ_AWSS3Test_SecretAccessKey");
    private readonly string? _bucketName = Environment.GetEnvironmentVariable("HiQ_AWSS3Test_BucketName");
    private readonly string _dir = Path.Combine(Environment.CurrentDirectory); // .\Frends.AmazonS3.DownloadObject\Frends.AmazonS3.DownloadObject.Test\bin\Debug\net6.0\

    Input? connection;

    [TestInitialize]
    public async Task Initialize()
    {
        await CreateTestFiles();
    }

    [TestCleanup]
    public void CleanUp()
    {
        Directory.Delete($@"{_dir}\Download", true);
    }

    [TestMethod]
    public async Task PreSignedURL_Overwrite_NewFile_Test()
    {
        var setS3Key = $"DownloadTest/Testfile.txt";

        connection = new Input()
        {
            AuthenticationMethod = AuthenticationMethod.PreSignedURL,
            PreSignedURL = CreatePreSignedURL(setS3Key),
            DestinationDirectory = @$"{_dir}\Download",
            DestinationFileExistsAction = DestinationFileExistsAction.Overwrite
        };

        var result = await AmazonS3.DownloadObject(connection, default);
        Assert.IsNotNull(result.Results);
        Assert.AreEqual(true, result.Success);
        Assert.AreEqual(1, result.Results.Count);
        Assert.IsTrue(result.Results.Any(x => x.ObjectName != null));
        Assert.IsTrue(File.Exists(@$"{_dir}\Download\Testfile.txt"));
    }

    [TestMethod]
    public async Task PreSignedURL_Overwrite_NoDestinationFilename_Test()
    {
        var setS3Key = $"DownloadTest/Testfile.txt";

        connection = new Input()
        {
            AuthenticationMethod = AuthenticationMethod.PreSignedURL,
            PreSignedURL = CreatePreSignedURL(setS3Key),
            DestinationDirectory = @$"{_dir}\Download",
            DestinationFileExistsAction = DestinationFileExistsAction.Overwrite
        };

        var result = await AmazonS3.DownloadObject(connection, default);
        Assert.IsNotNull(result.Results);
        Assert.AreEqual(true, result.Success);
        Assert.AreEqual(1, result.Results.Count);
        Assert.IsTrue(result.Results.Any(x => x.ObjectName != null));
        Assert.IsTrue(File.Exists(@$"{_dir}\Download\Testfile.txt"));
    }

    [TestMethod]
    public async Task PreSignedURL_Overwrite_Exists_Test()
    {
        var setS3Key = $"DownloadTest/Overwrite.txt";

        connection = new Input()
        {
            AuthenticationMethod = AuthenticationMethod.PreSignedURL,
            PreSignedURL = CreatePreSignedURL(setS3Key),
            DestinationDirectory = @$"{_dir}\Download",
            DestinationFileExistsAction = DestinationFileExistsAction.Overwrite
        };

        var result = await AmazonS3.DownloadObject(connection, default);
        Assert.IsNotNull(result.Results);
        Assert.AreEqual(true, result.Success);
        Assert.AreEqual(1, result.Results.Count);
        Assert.IsTrue(result.Results.Any(x => x.ObjectName != null));
        Assert.IsTrue(result.Results.Any(x => x.Overwritten.Equals(true)));
        Assert.IsTrue(CompareFiles());
    }

    [TestMethod]
    public async Task PreSignedURL_Info_Exists_Test()
    {
        var setS3Key = $"DownloadTest/Overwrite.txt";

        connection = new Input()
        {
            AuthenticationMethod = AuthenticationMethod.PreSignedURL,
            PreSignedURL = CreatePreSignedURL(setS3Key),
            DestinationDirectory = @$"{_dir}\Download",
            DestinationFileExistsAction = DestinationFileExistsAction.Info
        };

        var result = await AmazonS3.DownloadObject(connection, default);
        Assert.IsNotNull(result.Results);
        Assert.AreEqual(true, result.Success);
        Assert.AreEqual(1, result.Results.Count);
        Assert.IsTrue(result.Results.Any(x => x.ObjectName != null));
        Assert.IsTrue(result.Results.Any(x => x.Info.Contains("Object skipped because file already exists in destination")));
        Assert.IsTrue(File.Exists(@$"{_dir}\Download\Overwrite.txt"));
        Assert.IsFalse(CompareFiles());
    }

    [TestMethod]
    public async Task PreSignedURL_Error_NewFile_Test()
    {
        var setS3Key = $"DownloadTest/Testfile.txt";

        connection = new Input()
        {
            AuthenticationMethod = AuthenticationMethod.PreSignedURL,
            PreSignedURL = CreatePreSignedURL(setS3Key),
            DestinationDirectory = @$"{_dir}\Download",
            DestinationFileExistsAction = DestinationFileExistsAction.Error
        };

        var result = await AmazonS3.DownloadObject(connection, default);
        _ = await Assert.ThrowsExceptionAsync<Exception>(async () => await AmazonS3.DownloadObject(connection, default));
        Assert.IsTrue(File.Exists(@$"{_dir}\Download\Testfile.txt"));
    }

    [TestMethod]
    public async Task PreSignedURL_MissingURL_Test()
    {
        var setS3Key = $"DownloadTest/Testfile.txt";

        connection = new Input()
        {
            AuthenticationMethod = AuthenticationMethod.PreSignedURL,
            PreSignedURL = "",
            DestinationDirectory = @$"{_dir}\Download",
            DestinationFileExistsAction = DestinationFileExistsAction.Overwrite
        };

        var ex = await Assert.ThrowsExceptionAsync<Exception>(async () => await AmazonS3.DownloadObject(connection, default));
        Assert.IsTrue(ex.Message.Contains("AWS pre-signed URL required."));
    }

    [TestMethod]
    public async Task PreSignedURL_MissingDestinationDirectory_Test()
    {
        var setS3Key = $"DownloadTest/Testfile.txt";

        connection = new Input()
        {
            AuthenticationMethod = AuthenticationMethod.PreSignedURL,
            PreSignedURL = "",
            DestinationDirectory = "",
            DestinationFileExistsAction = DestinationFileExistsAction.Overwrite
        };

        var ex = await Assert.ThrowsExceptionAsync<Exception>(async () => await AmazonS3.DownloadObject(connection, default));
        Assert.IsTrue(ex.Message.Contains("Destination required."));
    }

    [TestMethod]
    public async Task AWSCreds_Overwrite_NewFiles_Test()
    {
        Directory.Delete($@"{_dir}\Download", true);

        connection = new Input()
        {
            AuthenticationMethod = AuthenticationMethod.AWSCredentials,
            AwsAccessKeyId = _accessKey,
            AwsSecretAccessKey = _secretAccessKey,
            BucketName = _bucketName,
            Region = Region.EuCentral1,
            S3Directory = "DownloadTest/",
            SearchPattern = "*",
            DeleteSourceObject = false,
            DownloadFromCurrentDirectoryOnly = false,
            ThrowErrorIfNoMatch = false,
            DestinationDirectory = @$"{_dir}\Download",
            DestinationFileExistsAction = DestinationFileExistsAction.Overwrite
        };

        var result = await AmazonS3.DownloadObject(connection, default);
        Assert.IsNotNull(result.Results);
        Assert.AreEqual(true, result.Success);
        Assert.AreEqual(3, result.Results.Count);
        Assert.IsTrue(result.Results.Any(x => x.ObjectName != null));
        Assert.IsTrue(File.Exists(@$"{_dir}\Download\DownloadFromCurrentDirectoryOnly.txt"));
    }

    [TestMethod]
    public async Task AWSCreds_Overwrite_WithOverwrite_Test()
    {
        connection = new Input()
        {
            AuthenticationMethod = AuthenticationMethod.AWSCredentials,
            AwsAccessKeyId = _accessKey,
            AwsSecretAccessKey = _secretAccessKey,
            BucketName = _bucketName,
            Region = Region.EuCentral1,
            S3Directory = "DownloadTest/",
            SearchPattern = "*",
            DeleteSourceObject = false,
            DownloadFromCurrentDirectoryOnly = false,
            ThrowErrorIfNoMatch = false,
            DestinationDirectory = @$"{_dir}\Download",
            DestinationFileExistsAction = DestinationFileExistsAction.Overwrite
        };

        var result = await AmazonS3.DownloadObject(connection, default);
        Assert.IsNotNull(result.Results);
        Assert.AreEqual(true, result.Success);
        Assert.AreEqual(3, result.Results.Count);
        Assert.IsTrue(result.Results.Any(x => x.ObjectName != null));
        Assert.IsTrue(File.Exists(@$"{_dir}\Download\Overwrite.txt"));
        Assert.IsTrue(File.Exists(@$"{_dir}\Download\Testfile.txt"));
        Assert.IsTrue(File.Exists(@$"{_dir}\Download\DownloadFromCurrentDirectoryOnly.txt"));
        Assert.IsTrue(CompareFiles());
    }

    [TestMethod]
    public async Task AWSCreds_Info_NoOverwrite_Test()
    {
        Directory.Delete($@"{_dir}\Download", true);

        connection = new Input()
        {
            AuthenticationMethod = AuthenticationMethod.AWSCredentials,
            AwsAccessKeyId = _accessKey,
            AwsSecretAccessKey = _secretAccessKey,
            BucketName = _bucketName,
            Region = Region.EuCentral1,
            S3Directory = "DownloadTest/",
            SearchPattern = "*",
            DeleteSourceObject = false,
            DownloadFromCurrentDirectoryOnly = false,
            ThrowErrorIfNoMatch = false,
            DestinationDirectory = @$"{_dir}\Download",
            DestinationFileExistsAction = DestinationFileExistsAction.Info
        };

        var result = await AmazonS3.DownloadObject(connection, default);
        Assert.IsNotNull(result.Results);
        Assert.AreEqual(true, result.Success);
        Assert.AreEqual(3, result.Results.Count);
        Assert.IsTrue(result.Results.Any(x => x.ObjectName != null));
        Assert.IsTrue(File.Exists(@$"{_dir}\Download\Overwrite.txt"));
        Assert.IsTrue(File.Exists(@$"{_dir}\Download\Testfile.txt"));
        Assert.IsTrue(File.Exists(@$"{_dir}\Download\DownloadFromCurrentDirectoryOnly.txt"));
    }

    [TestMethod]
    public async Task AWSCreds_Info_WithInfo_Test()
    {
        connection = new Input()
        {
            AuthenticationMethod = AuthenticationMethod.AWSCredentials,
            AwsAccessKeyId = _accessKey,
            AwsSecretAccessKey = _secretAccessKey,
            BucketName = _bucketName,
            Region = Region.EuCentral1,
            S3Directory = "DownloadTest/",
            SearchPattern = "*",
            DeleteSourceObject = false,
            DownloadFromCurrentDirectoryOnly = false,
            ThrowErrorIfNoMatch = false,
            DestinationDirectory = @$"{_dir}\Download",
            DestinationFileExistsAction = DestinationFileExistsAction.Info
        };

        var result = await AmazonS3.DownloadObject(connection, default);
        Assert.IsNotNull(result.Results);
        Assert.AreEqual(true, result.Success);
        Assert.AreEqual(3, result.Results.Count);
        Assert.IsTrue(result.Results.Any(x => x.ObjectName != null));
        Assert.IsTrue(File.Exists(@$"{_dir}\Download\Overwrite.txt"));
        Assert.IsTrue(File.Exists(@$"{_dir}\Download\Testfile.txt"));
        Assert.IsTrue(File.Exists(@$"{_dir}\Download\DownloadFromCurrentDirectoryOnly.txt"));
        Assert.IsFalse(CompareFiles());
    }

    [TestMethod]
    public async Task AWSCreds_Error_NoOverwrite_Test()
    {
        Directory.Delete($@"{_dir}\Download", true);

        connection = new Input()
        {
            AuthenticationMethod = AuthenticationMethod.AWSCredentials,
            AwsAccessKeyId = _accessKey,
            AwsSecretAccessKey = _secretAccessKey,
            BucketName = _bucketName,
            Region = Region.EuCentral1,
            S3Directory = "DownloadTest/",
            SearchPattern = "*",
            DeleteSourceObject = false,
            DownloadFromCurrentDirectoryOnly = false,
            ThrowErrorIfNoMatch = false,
            DestinationDirectory = @$"{_dir}\Download",
            DestinationFileExistsAction = DestinationFileExistsAction.Error
        };

        var result = await AmazonS3.DownloadObject(connection, default);
        Assert.IsNotNull(result.Results);
        Assert.AreEqual(true, result.Success);
        Assert.AreEqual(3, result.Results.Count);
        Assert.IsTrue(result.Results.Any(x => x.ObjectName != null));
        Assert.IsTrue(File.Exists(@$"{_dir}\Download\Overwrite.txt"));
        Assert.IsTrue(File.Exists(@$"{_dir}\Download\Testfile.txt"));
        Assert.IsTrue(File.Exists(@$"{_dir}\Download\DownloadFromCurrentDirectoryOnly.txt"));
    }

    [TestMethod]
    public async Task AWSCreds_Error_WithError_Test()
    {
        connection = new Input()
        {
            AuthenticationMethod = AuthenticationMethod.AWSCredentials,
            AwsAccessKeyId = _accessKey,
            AwsSecretAccessKey = _secretAccessKey,
            BucketName = _bucketName,
            Region = Region.EuCentral1,
            S3Directory = "DownloadTest/",
            SearchPattern = "*",
            DeleteSourceObject = false,
            DownloadFromCurrentDirectoryOnly = false,
            ThrowErrorIfNoMatch = false,
            DestinationDirectory = @$"{_dir}\Download",
            DestinationFileExistsAction = DestinationFileExistsAction.Error
        };

        var ex = await Assert.ThrowsExceptionAsync<Exception>(async () => await AmazonS3.DownloadObject(connection, default));
        Assert.IsTrue(ex.Message.Contains("already exists in"));
        Assert.IsTrue(File.Exists(@$"{_dir}\Download\Overwrite.txt"));
        Assert.IsFalse(CompareFiles());
    }

    [TestMethod]
    public async Task AWSCreds_DeleteSource_Test()
    {
        Directory.Delete($@"{_dir}\Download", true);

        connection = new Input()
        {
            AuthenticationMethod = AuthenticationMethod.AWSCredentials,
            AwsAccessKeyId = _accessKey,
            AwsSecretAccessKey = _secretAccessKey,
            BucketName = _bucketName,
            Region = Region.EuCentral1,
            S3Directory = "DownloadTest/",
            SearchPattern = "*",
            DeleteSourceObject = true,
            DownloadFromCurrentDirectoryOnly = false,
            ThrowErrorIfNoMatch = false,
            DestinationDirectory = @$"{_dir}\Download",
            DestinationFileExistsAction = DestinationFileExistsAction.Overwrite
        };

        var result = await AmazonS3.DownloadObject(connection, default);
        Assert.IsNotNull(result.Results);
        Assert.AreEqual(true, result.Success);
        Assert.AreEqual(3, result.Results.Count);
        Assert.IsTrue(result.Results.Any(x => x.ObjectName != null));
        Assert.IsTrue(File.Exists(@$"{_dir}\Download\Overwrite.txt"));
        Assert.IsTrue(File.Exists(@$"{_dir}\Download\Testfile.txt"));
        Assert.IsTrue(File.Exists(@$"{_dir}\Download\DownloadFromCurrentDirectoryOnly.txt"));
        Assert.IsFalse(await FileExistsInS3("DownloadTest/testikansio/")); //Folder will be deleted if all files inside have been deleted.
    }

    [TestMethod]
    public async Task AWSCreds_Pattern_Test()
    {
        Directory.Delete($@"{_dir}\Download", true);

        connection = new Input()
        {
            AuthenticationMethod = AuthenticationMethod.AWSCredentials,
            AwsAccessKeyId = _accessKey,
            AwsSecretAccessKey = _secretAccessKey,
            BucketName = _bucketName,
            Region = Region.EuCentral1,
            S3Directory = "DownloadTest/",
            SearchPattern = "Testfi*",
            DeleteSourceObject = false,
            DownloadFromCurrentDirectoryOnly = false,
            ThrowErrorIfNoMatch = false,
            DestinationDirectory = @$"{_dir}\Download",
            DestinationFileExistsAction = DestinationFileExistsAction.Overwrite
        };

        var result = await AmazonS3.DownloadObject(connection, default);
        Assert.IsNotNull(result.Results);
        Assert.AreEqual(true, result.Success);
        Assert.AreEqual(1, result.Results.Count);
        Assert.IsTrue(result.Results.Any(x => x.ObjectName != null));
        Assert.IsTrue(File.Exists(@$"{_dir}\Download\Testfile.txt"));
        Assert.IsFalse(File.Exists(@$"{_dir}\Download\Overwrite.txt"));
        Assert.IsFalse(File.Exists(@$"{_dir}\Download\DownloadFromCurrentDirectoryOnly.txt"));
    }

    [TestMethod]
    public async Task AWSCreds_DownloadFromCurrentDirectoryOnly_Test()
    {
        Directory.Delete($@"{_dir}\Download", true);

        connection = new Input()
        {
            AuthenticationMethod = AuthenticationMethod.AWSCredentials,
            AwsAccessKeyId = _accessKey,
            AwsSecretAccessKey = _secretAccessKey,
            BucketName = _bucketName,
            Region = Region.EuCentral1,
            S3Directory = "DownloadTest/",
            SearchPattern = "*",
            DeleteSourceObject = false,
            DownloadFromCurrentDirectoryOnly = true,
            ThrowErrorIfNoMatch = false,
            DestinationDirectory = @$"{_dir}\Download",
            DestinationFileExistsAction = DestinationFileExistsAction.Overwrite
        };

        var result = await AmazonS3.DownloadObject(connection, default);
        Assert.IsNotNull(result.Results);
        Assert.AreEqual(true, result.Success);
        Assert.AreEqual(2, result.Results.Count);
        Assert.IsTrue(result.Results.Any(x => x.ObjectName != null));
        Assert.IsTrue(File.Exists(@$"{_dir}\Download\Overwrite.txt"));
        Assert.IsTrue(File.Exists(@$"{_dir}\Download\Testfile.txt"));
        Assert.IsFalse(File.Exists(@$"{_dir}\Download\DownloadFromCurrentDirectoryOnly.txt"));
    }

    [TestMethod]
    public async Task AWSCreds_ThrowErrorIfNoMatch_Test()
    {
        connection = new Input()
        {
            AuthenticationMethod = AuthenticationMethod.AWSCredentials,
            AwsAccessKeyId = _accessKey,
            AwsSecretAccessKey = _secretAccessKey,
            BucketName = _bucketName,
            Region = Region.EuCentral1,
            S3Directory = "DownloadTest/",
            SearchPattern = "NoFile",
            DeleteSourceObject = false,
            DownloadFromCurrentDirectoryOnly = false,
            ThrowErrorIfNoMatch = true,
            DestinationDirectory = @$"{_dir}\Download",
            DestinationFileExistsAction = DestinationFileExistsAction.Overwrite
        };

        var ex = await Assert.ThrowsExceptionAsync<Exception>(async () => await AmazonS3.DownloadObject(connection, default));
        Assert.IsTrue(ex.Message.Contains("No matches found with search pattern"));
    }

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
        return client.GetPreSignedURL(request);
    }

    private async Task<bool> CreateTestFiles()
    {
        var file1 = $@"{_dir}\Download\Overwrite.txt";
        var file2 = $@"{_dir}\DownloadTestFiles\Overwrite.txt";
        var file3 = $@"{_dir}\DownloadTestFiles\Testfile.txt";
        var file4 = $@"{_dir}\DownloadTestFiles\DownloadFromCurrentDirectoryOnly\DownloadFromCurrentDirectoryOnly.txt";

        var files = new List<string> { file1, file2, file3, file4 };

        Directory.CreateDirectory($@"{_dir}\Download");
        Directory.CreateDirectory($@"{_dir}\DownloadTestFiles\DownloadFromCurrentDirectoryOnly");
        File.AppendAllText(file1, "To Be Overwriten");
        File.AppendAllText(file2, $"Overwrite complete {DateTime.UtcNow}");
        File.AppendAllText(file3, $"Test {DateTime.UtcNow}");
        File.AppendAllText(file4, $"This should exists if DownloadFromCurrentDirectoryOnly = true.  {DateTime.UtcNow}");

        return await UploadFileToS3(files);
    }

    private async Task<bool> UploadFileToS3(List<string> files)
    {
        var client = new AmazonS3Client(_accessKey, _secretAccessKey, RegionEndpoint.EUCentral1);

        foreach (var x in files)
        {
            var putObjectRequest = new PutObjectRequest
            {
                BucketName = _bucketName,
                Key = x.Contains("DownloadFromCurrentDirectoryOnly") ? "DownloadTest/DownloadFromCurrentDirectoryOnly/DownloadFromCurrentDirectoryOnly.txt" : $"DownloadTest/{Path.GetFileName(x)}",
                FilePath = x,
            };
            await client.PutObjectAsync(putObjectRequest);
        }
        return true;
    }

    private bool CompareFiles()
    {
        string mainFile = File.ReadAllText($@"{_dir}\Download\Overwrite.txt");
        return mainFile.Contains("Overwrite complete") && !mainFile.Contains("To Be Overwriten");
    }

    private async Task<bool> FileExistsInS3(string key)
    {
        var client = new AmazonS3Client(_accessKey, _secretAccessKey, RegionEndpoint.EUCentral1);

        var request = new ListObjectsRequest
        {
            BucketName = _bucketName,
            Prefix = key,
        };
        ListObjectsResponse response = await client.ListObjectsAsync(request);
        client.Dispose();
        return (response != null && response.S3Objects != null && response.S3Objects.Count > 0);
    }
}