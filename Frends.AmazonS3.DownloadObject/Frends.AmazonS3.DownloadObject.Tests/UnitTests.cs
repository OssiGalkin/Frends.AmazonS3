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

    Connection? connection;
    Destination? destination;


    /// <summary>
    /// Create test files and folders before uploading them to S3.
    /// </summary>
    [TestInitialize]
    public async Task Initialize()
    {
        await CreateTestFiles();
    }

    /// <summary>
    /// Delete test files and folders. CleanUp after each test to make sure overwriten file(s) won't make other tests fail.
    /// </summary>
    [TestCleanup]
    public void CleanUp()
    {
        Directory.Delete($@"{_dir}\Download", true);
    }

    /// <summary>
    /// Testfile.txt existed without overwrite before CleanUp().
    /// </summary>
    [TestMethod]
    public void PreSignedURL_Overwrite_NewFile_Test()
    {
        var setS3Key = $"DownloadTest/Testfile.txt";

        connection = new Connection()
        {
            AuthenticationMethod = AuthenticationMethod.PreSignedURL,
            PreSignedURL = CreatePreSignedURL(setS3Key),
        };

        destination = new Destination()
        {
            DestinationDirectory = @$"{_dir}\Download",
            DestinationFilename = "Testfile.txt",
            DestinationFileExistsAction = DestinationFileExistsAction.Overwrite
        };

        var result = AmazonS3.DownloadObject(connection, destination, default);
        Assert.IsNotNull(result.Result.Results);
        Assert.IsFalse(result.Result.Results.Any(x => x.ObjectData.Contains("Download with overwrite complete")));
        Assert.IsTrue(File.Exists(@$"{_dir}\Download\Testfile.txt"));
    }

    /// <summary>
    /// Get filename from PreSignedURL.
    /// Testfile.txt existed without overwrite before CleanUp().
    /// </summary>
    [TestMethod]
    public void PreSignedURL_Overwrite_NoDestinationFilename_Test()
    {
        var setS3Key = $"DownloadTest/Testfile.txt";

        connection = new Connection()
        {
            AuthenticationMethod = AuthenticationMethod.PreSignedURL,
            PreSignedURL = CreatePreSignedURL(setS3Key),
        };

        destination = new Destination()
        {
            DestinationDirectory = @$"{_dir}\Download",
            DestinationFilename = "",
            DestinationFileExistsAction = DestinationFileExistsAction.Overwrite
        };

        var result = AmazonS3.DownloadObject(connection, destination, default);
        Assert.IsNotNull(result.Result.Results);
        Assert.IsFalse(result.Result.Results.Any(x => x.ObjectData.Contains("Download with overwrite complete")));
        Assert.IsTrue(File.Exists(@$"{_dir}\Download\Testfile.txt"));
    }

    /// <summary>
    /// Result informs about overwrite and Overwrite.txt was overwriten.
    /// </summary>
    [TestMethod]
    public void PreSignedURL_Overwrite_Exists_Test()
    {
        var setS3Key = $"DownloadTest/Overwrite.txt";

        connection = new Connection()
        {
            AuthenticationMethod = AuthenticationMethod.PreSignedURL,
            PreSignedURL = CreatePreSignedURL(setS3Key),
        };

        destination = new Destination()
        {
            DestinationDirectory = @$"{_dir}\Download",
            DestinationFilename = "Overwrite.txt",
            DestinationFileExistsAction = DestinationFileExistsAction.Overwrite
        };

        var result = AmazonS3.DownloadObject(connection, destination, default);
        Assert.IsTrue(result.Result.Results.Any(x => x.ObjectData.Contains("Download with overwrite complete")));
        Assert.IsTrue(CompareFiles());
    }

    /// <summary>
    /// Download complete without info.
    /// </summary>
    [TestMethod]
    public void PreSignedURL_Info_NewFile_Test()
    {
        var setS3Key = $"DownloadTest/Testfile.txt";

        connection = new Connection()
        {
            AuthenticationMethod = AuthenticationMethod.PreSignedURL,
            PreSignedURL = CreatePreSignedURL(setS3Key),
        };

        destination = new Destination()
        {
            DestinationDirectory = @$"{_dir}\Download",
            DestinationFilename = "Testfile.txt",
            DestinationFileExistsAction = DestinationFileExistsAction.Info
        };

        var result = AmazonS3.DownloadObject(connection, destination, default);
        Assert.IsNotNull(result.Result.Results);
        Assert.IsFalse(result.Result.Results.Any(x => x.ObjectData.Contains("Download with overwrite complete")));
        Assert.IsTrue(File.Exists(@$"{_dir}\Download\Testfile.txt"));
    }

    /// <summary>
    /// Inform about existing file and skip downloading.
    /// </summary>
    [TestMethod]
    public void PreSignedURL_Info_Exists_Test()
    {
        var setS3Key = $"DownloadTest/Overwrite.txt";

        connection = new Connection()
        {
            AuthenticationMethod = AuthenticationMethod.PreSignedURL,
            PreSignedURL = CreatePreSignedURL(setS3Key),
        };

        destination = new Destination()
        {
            DestinationDirectory = @$"{_dir}\Download",
            DestinationFilename = "Overwrite.txt",
            DestinationFileExistsAction = DestinationFileExistsAction.Info
        };

        var result = AmazonS3.DownloadObject(connection, destination, default);
        Assert.IsTrue(result.Result.Results.Any(x => x.ObjectData.Contains("already exists")));
        Assert.IsTrue(File.Exists(@$"{_dir}\Download\Overwrite.txt"));
        Assert.IsFalse(CompareFiles());
    }

    /// <summary>
    /// Download complete without error.
    /// </summary>
    [TestMethod]
    public void PreSignedURL_Error_NewFile_Test()
    {
        var setS3Key = $"DownloadTest/Testfile.txt";

        connection = new Connection()
        {
            AuthenticationMethod = AuthenticationMethod.PreSignedURL,
            PreSignedURL = CreatePreSignedURL(setS3Key),
        };

        destination = new Destination()
        {
            DestinationDirectory = @$"{_dir}\Download",
            DestinationFilename = "Testfile.txt",
            DestinationFileExistsAction = DestinationFileExistsAction.Error
        };

        var result = AmazonS3.DownloadObject(connection, destination, default);
        Assert.IsNotNull(result.Result.Results);
        Assert.IsFalse(result.Result.Results.Any(x => x.ObjectData.Contains("Download with overwrite complete")));
        Assert.IsTrue(File.Exists(@$"{_dir}\Download\Testfile.txt"));
    }

    /// <summary>
    /// Error while downloading.
    /// </summary>
    [TestMethod]
    public void PreSignedURL_Error_Exists_Test()
    {
        var setS3Key = $"DownloadTest/Overwrite.txt";

        connection = new Connection()
        {
            AuthenticationMethod = AuthenticationMethod.PreSignedURL,
            PreSignedURL = CreatePreSignedURL(setS3Key),
        };

        destination = new Destination()
        {
            DestinationDirectory = @$"{_dir}\Download",
            DestinationFilename = "Overwrite.txt",
            DestinationFileExistsAction = DestinationFileExistsAction.Error
        };

        var result = AmazonS3.DownloadObject(connection, destination, default);
        var ex = Assert.ThrowsExceptionAsync<Exception>(async () => await AmazonS3.DownloadObject(connection, destination, default)).Result;
        Assert.IsTrue(ex.Message.Contains("Error while downloading an object. File"));
        Assert.IsTrue(File.Exists(@$"{_dir}\Download\Overwrite.txt"));
        Assert.IsFalse(CompareFiles());
    }


    /// <summary>
    /// Missing pre-signed URL error.
    /// </summary>
    [TestMethod]
    public void PreSignedURL_MissingURL_Test()
    {
        var setS3Key = $"DownloadTest/Testfile.txt";

        connection = new Connection()
        {
            AuthenticationMethod = AuthenticationMethod.PreSignedURL,
            PreSignedURL = "",
        };

        destination = new Destination()
        {
            DestinationDirectory = @$"{_dir}\Download",
            DestinationFilename = "Testfile.txt",
            DestinationFileExistsAction = DestinationFileExistsAction.Overwrite
        };

        var ex = Assert.ThrowsExceptionAsync<Exception>(async () => await AmazonS3.DownloadObject(connection, destination, default)).Result;
        Assert.IsTrue(ex.Message.Contains("AWS pre-signed URL required."));
    }


    /// <summary>
    /// Missing destination path error.
    /// </summary>
    [TestMethod]
    public void PreSignedURL_DestinationDirectory_Test()
    {
        var setS3Key = $"DownloadTest/Testfile.txt";

        connection = new Connection()
        {
            AuthenticationMethod = AuthenticationMethod.PreSignedURL,
            PreSignedURL = "",
        };

        destination = new Destination()
        {
            DestinationDirectory = "",
            DestinationFilename = "Testfile.txt",
            DestinationFileExistsAction = DestinationFileExistsAction.Overwrite
        };

        var ex = Assert.ThrowsExceptionAsync<Exception>(async () => await AmazonS3.DownloadObject(connection, destination, default)).Result;
        Assert.IsTrue(ex.Message.Contains("Destination required."));
    }


    /// <summary>
    /// AWS creds get all keys. No overwrite.
    /// </summary>
    [TestMethod]
    public async Task AWSCreds_Overwrite_NewFiles_Test()
    {
        Directory.Delete($@"{_dir}\Download", true);

        connection = new Connection()
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
        };

        destination = new Destination()
        {
            DestinationDirectory = @$"{_dir}\Download",
            DestinationFilename = "",
            DestinationFileExistsAction = DestinationFileExistsAction.Overwrite
        };


        var result = await AmazonS3.DownloadObject(connection, destination, default);
        Assert.IsNotNull(result.Results);
        Assert.IsFalse(result.Results.Any(x => x.ObjectData.Contains("Download with overwrite complete")));
        Assert.IsTrue(File.Exists(@$"{_dir}\Download\Overwrite.txt"));
        Assert.IsTrue(File.Exists(@$"{_dir}\Download\Testfile.txt"));
        Assert.IsTrue(File.Exists(@$"{_dir}\Download\DownloadFromCurrentDirectoryOnly.txt"));
    }


    /// <summary>
    /// AWS creds get all keys. Overwrite Overwrite.txt.
    /// </summary>
    [TestMethod]
    public async Task AWSCreds_Overwrite_WithOverwrite_Test()
    {
        connection = new Connection()
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
        };

        destination = new Destination()
        {
            DestinationDirectory = @$"{_dir}\Download",
            DestinationFilename = "",
            DestinationFileExistsAction = DestinationFileExistsAction.Overwrite
        };


        var result = await AmazonS3.DownloadObject(connection, destination, default);
        Assert.IsNotNull(result.Results);
        Assert.IsTrue(result.Results.Any(x => x.ObjectData.Contains("Download with overwrite complete")));
        Assert.IsTrue(File.Exists(@$"{_dir}\Download\Overwrite.txt"));
        Assert.IsTrue(File.Exists(@$"{_dir}\Download\Testfile.txt"));
        Assert.IsTrue(File.Exists(@$"{_dir}\Download\DownloadFromCurrentDirectoryOnly.txt"));
        Assert.IsTrue(CompareFiles());
    }

    /// <summary>
    /// AWS creds get all keys. No info.
    /// </summary>
    [TestMethod]
    public async Task AWSCreds_Info_NoOverwrite_Test()
    {
        Directory.Delete($@"{_dir}\Download", true);

        connection = new Connection()
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
        };

        destination = new Destination()
        {
            DestinationDirectory = @$"{_dir}\Download",
            DestinationFilename = "",
            DestinationFileExistsAction = DestinationFileExistsAction.Info
        };


        var result = await AmazonS3.DownloadObject(connection, destination, default);
        Assert.IsNotNull(result.Results);
        Assert.IsFalse(result.Results.Any(x => x.ObjectData.Contains("Download with overwrite complete")));
        Assert.IsFalse(result.Results.Any(x => x.ObjectData.Contains("was skipped because it already exists at")));
        Assert.IsTrue(File.Exists(@$"{_dir}\Download\Overwrite.txt"));
        Assert.IsTrue(File.Exists(@$"{_dir}\Download\Testfile.txt"));
        Assert.IsTrue(File.Exists(@$"{_dir}\Download\DownloadFromCurrentDirectoryOnly.txt"));
    }


    /// <summary>
    /// AWS creds get all keys. Info about skipping Overwrite.txt.
    /// </summary>
    [TestMethod]
    public async Task AWSCreds_Info_WithInfo_Test()
    {
        connection = new Connection()
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
        };

        destination = new Destination()
        {
            DestinationDirectory = @$"{_dir}\Download",
            DestinationFilename = "",
            DestinationFileExistsAction = DestinationFileExistsAction.Info
        };


        var result = await AmazonS3.DownloadObject(connection, destination, default);
        Assert.IsNotNull(result.Results);
        Assert.IsFalse(result.Results.Any(x => x.ObjectData.Contains("Download with overwrite complete")));
        Assert.IsTrue(result.Results.Any(x => x.ObjectData.Contains("was skipped because it already exists at")));
        Assert.IsTrue(File.Exists(@$"{_dir}\Download\Overwrite.txt"));
        Assert.IsTrue(File.Exists(@$"{_dir}\Download\Testfile.txt"));
        Assert.IsTrue(File.Exists(@$"{_dir}\Download\DownloadFromCurrentDirectoryOnly.txt"));
        Assert.IsFalse(CompareFiles());
    }


    /// <summary>
    /// AWS creds get all keys. No error.
    /// </summary>
    [TestMethod]
    public async Task AWSCreds_Error_NoOverwrite_Test()
    {
        Directory.Delete($@"{_dir}\Download", true);

        connection = new Connection()
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
        };

        destination = new Destination()
        {
            DestinationDirectory = @$"{_dir}\Download",
            DestinationFilename = "",
            DestinationFileExistsAction = DestinationFileExistsAction.Error
        };


        var result = await AmazonS3.DownloadObject(connection, destination, default);
        Assert.IsNotNull(result.Results);
        Assert.IsFalse(result.Results.Any(x => x.ObjectData.Contains("Download with overwrite complete")));
        Assert.IsFalse(result.Results.Any(x => x.ObjectData.Contains("Error while downloading an object")));
        Assert.IsTrue(File.Exists(@$"{_dir}\Download\Overwrite.txt"));
        Assert.IsTrue(File.Exists(@$"{_dir}\Download\Testfile.txt"));
        Assert.IsTrue(File.Exists(@$"{_dir}\Download\DownloadFromCurrentDirectoryOnly.txt"));
    }


    /// <summary>
    /// AWS creds get all keys. Error because of Overwrite.txt.
    /// </summary>
    [TestMethod]
    public async Task AWSCreds_Error_WithError_Test()
    {
        connection = new Connection()
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
        };

        destination = new Destination()
        {
            DestinationDirectory = @$"{_dir}\Download",
            DestinationFilename = "",
            DestinationFileExistsAction = DestinationFileExistsAction.Error
        };


        var ex = Assert.ThrowsExceptionAsync<Exception>(async () => await AmazonS3.DownloadObject(connection, destination, default)).Result;
        Assert.IsTrue(ex.Message.Contains("Error while downloading an object."));
        Assert.IsTrue(File.Exists(@$"{_dir}\Download\Overwrite.txt"));
        Assert.IsFalse(CompareFiles());
    }

    /// <summary>
    /// AWS creds get all keys and delete source. Make sure the files have been downloaded and source is deleted.
    /// </summary>
    [TestMethod]
    public async Task AWSCreds_DeleteSource_Test()
    {
        Directory.Delete($@"{_dir}\Download", true);

        connection = new Connection()
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
        };

        destination = new Destination()
        {
            DestinationDirectory = @$"{_dir}\Download",
            DestinationFilename = "",
            DestinationFileExistsAction = DestinationFileExistsAction.Overwrite
        };


        var result = await AmazonS3.DownloadObject(connection, destination, default);
        Assert.IsNotNull(result.Results);
        Assert.IsFalse(result.Results.Any(x => x.ObjectData.Contains("Download with overwrite complete")));
        Assert.IsTrue(File.Exists(@$"{_dir}\Download\Overwrite.txt"));
        Assert.IsTrue(File.Exists(@$"{_dir}\Download\Testfile.txt"));
        Assert.IsTrue(File.Exists(@$"{_dir}\Download\DownloadFromCurrentDirectoryOnly.txt"));
        Assert.IsTrue(await FileExistsInS3("DownloadTest/testikansio/")); //Make sure the method works
        Assert.IsFalse(await FileExistsInS3("DownloadTest/Testfile.txt"));
    }

    /// <summary>
    /// AWS creds get all keys and delete source. Make sure the files have been downloaded and source is deleted.
    /// </summary>
    [TestMethod]
    public async Task AWSCreds_Pattern_Test()
    {
        Directory.Delete($@"{_dir}\Download", true);

        connection = new Connection()
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
        };

        destination = new Destination()
        {
            DestinationDirectory = @$"{_dir}\Download",
            DestinationFilename = "",
            DestinationFileExistsAction = DestinationFileExistsAction.Overwrite
        };

        var result = await AmazonS3.DownloadObject(connection, destination, default);
        Assert.IsNotNull(result.Results);
        Assert.IsTrue(File.Exists(@$"{_dir}\Download\Testfile.txt"));
        Assert.IsFalse(File.Exists(@$"{_dir}\Download\Overwrite.txt"));
        Assert.IsFalse(File.Exists(@$"{_dir}\Download\DownloadFromCurrentDirectoryOnly.txt"));
    }

    /// <summary>
    /// AWS creds skip sub folder.
    /// </summary>
    [TestMethod]
    public async Task AWSCreds_DownloadFromCurrentDirectoryOnly_Test()
    {
        Directory.Delete($@"{_dir}\Download", true);

        connection = new Connection()
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
        };

        destination = new Destination()
        {
            DestinationDirectory = @$"{_dir}\Download",
            DestinationFilename = "",
            DestinationFileExistsAction = DestinationFileExistsAction.Overwrite
        };


        var result = await AmazonS3.DownloadObject(connection, destination, default);
        Assert.IsNotNull(result.Results);
        Assert.IsTrue(File.Exists(@$"{_dir}\Download\Overwrite.txt"));
        Assert.IsTrue(File.Exists(@$"{_dir}\Download\Testfile.txt"));
        Assert.IsFalse(File.Exists(@$"{_dir}\Download\DownloadFromCurrentDirectoryOnly.txt"));
    }

    /// <summary>
    /// ThrowErrorIfNoMatch = true, 
    /// </summary>
    [TestMethod]
    public async Task AWSCreds_ThrowErrorIfNoMatch_Test()
    {
        connection = new Connection()
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
        };

        destination = new Destination()
        {
            DestinationDirectory = @$"{_dir}\Download",
            DestinationFilename = "",
            DestinationFileExistsAction = DestinationFileExistsAction.Overwrite
        };

        var ex = Assert.ThrowsExceptionAsync<Exception>(async () => await AmazonS3.DownloadObject(connection, destination, default)).Result;
        Assert.IsTrue(ex.Message.Contains("No matches found with search pattern"));
    }

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

    /// <summary>
    /// Create testfiles after each tests to make sure correct files and texts exists.
    /// </summary>
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

    /// <summary>
    /// Make sure that overwriten happened.
    /// </summary>
    /// <returns></returns>
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
        return (response != null && response.S3Objects != null && response.S3Objects.Count > 0);
    }
}
