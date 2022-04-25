using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Frends.AmazonS3.ListObjects.Definitions;

namespace Frends.AmazonS3.ListObjects.Test
{
    [TestClass]
    public class ListObjectsTest
    {
        private readonly string _accessKey = Environment.GetEnvironmentVariable("HiQ_AWSS3Test_AccessKey");
        private readonly string _secretAccessKey = Environment.GetEnvironmentVariable("HiQ_AWSS3Test_SecretAccessKey");
        private readonly string _bucketName = Environment.GetEnvironmentVariable("HiQ_AWSS3Test_BucketName");

        Source? _source = null;
        Options? _options = null;


        /// <summary>
        /// List all objects.
        /// </summary>
        [TestMethod]
        public void ListAllTest()
        {
            _source = new Source
            {
                AwsAccessKeyId = _accessKey,
                AwsSecretAccessKey = _secretAccessKey,
                BucketName = _bucketName,
                Region = Region.EuCentral1
            };

            _options = new Options
            {
                Delimiter = null,
                MaxKeys = 100,
                Prefix = null,
                StartAfter = null
            };

            var result = AmazonS3.ListObjects(_source, _options, default);
            Assert.IsNotNull(result.Result.ObjectList);
        }

        /// <summary>
        /// With StartAfter value. Get objects created after 2022-04-22T00:16:40+02:00.
        /// </summary>
        [TestMethod]
        public void UsingStartAfterTest()
        {
            _source = new Source
            {
                AwsAccessKeyId = _accessKey,
                AwsSecretAccessKey = _secretAccessKey,
                BucketName = _bucketName,
                Region = Region.EuCentral1
            };

            _options = new Options
            {
                Delimiter = null,
                MaxKeys = 100,
                Prefix = null,
                StartAfter = "2022-04-22T00:16:40+02:00"
            };

            var result = AmazonS3.ListObjects(_source, _options, default);
            Assert.IsNotNull(result.Result.ObjectList);
        }

        /// <summary>
        /// With Prefix value. Get objects from 2020/11/23/ locations.
        /// </summary>
        [TestMethod]
        public void UsingPrefixTest()
        {
            _source = new Source
            {
                AwsAccessKeyId = _accessKey,
                AwsSecretAccessKey = _secretAccessKey,
                BucketName = _bucketName,
                Region = Region.EuCentral1
            };

            _options = new Options
            {
                Delimiter = null,
                MaxKeys = 100,
                Prefix = "2020/11/23/",
                StartAfter = null
            };

            var result = AmazonS3.ListObjects(_source, _options, default);
            Assert.IsNotNull(result.Result.ObjectList);
        }

        /// <summary>
        /// Missing AwsAccessKeyId. Authentication fail.
        /// </summary>
        [TestMethod]
        public void MissingKeyTest()
        {
            _source = new Source
            {
                AwsAccessKeyId = null,
                AwsSecretAccessKey = _secretAccessKey,
                BucketName = _bucketName,
                Region = Region.EuCentral1
            };

            _options = new Options
            {
                Delimiter = null,
                MaxKeys = 100,
                Prefix = null,
                StartAfter = null
            };

            var ex = Assert.ThrowsExceptionAsync<Exception>(async () => await AmazonS3.ListObjects(_source, _options, default)).Result;
            Assert.IsTrue(ex.Message.Contains("AWS credentials missing."));
        }
    }
}