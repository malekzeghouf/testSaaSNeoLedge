using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Minio.DataModel.Args;
using Minio;
using System.Linq;
using System.Threading.Tasks;
using System;
using AuthenticationApi.Infrastructure.Services;
using System.Text;

public class MinioService
{
    private readonly IMinioClient _minioClient;

    private readonly ILogger<MinioService> _logger;

    public MinioService(IConfiguration config, ILogger<MinioService> logger)
    {
        var endpoint = config["Minio:Endpoint"];
        var accessKey = config["Minio:AccessKey"];
        var secretKey = config["Minio:SecretKey"];

        if (string.IsNullOrEmpty(endpoint)) throw new ArgumentNullException(nameof(endpoint));
        if (string.IsNullOrEmpty(accessKey)) throw new ArgumentNullException(nameof(accessKey));
        if (string.IsNullOrEmpty(secretKey)) throw new ArgumentNullException(nameof(secretKey));

        _minioClient = new MinioClient()
            .WithEndpoint(endpoint)
            .WithCredentials(accessKey, secretKey)
            .WithSSL(false)
            .Build();

        _logger = logger;
    }

    public async Task TestMinioConnection()
    {
        try
        {
            _logger.LogInformation("Testing MinIO connection...");

            var buckets = await _minioClient.ListBucketsAsync();
            _logger.LogInformation($"MinIO connection successful! Found {buckets.Buckets.Count} buckets.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to MinIO.");
            throw;
        }
    }

    public async Task CreateBucketAsync(string bucketName)
    {
        if (string.IsNullOrEmpty(bucketName))
            throw new ArgumentNullException(nameof(bucketName));

        bucketName = bucketName.ToLower().Replace(" ", "-"); // Normalisation

        if (!IsValidBucketName(bucketName))
            throw new ArgumentException("Invalid bucket name.");

        try
        {
            _logger.LogInformation($"Checking if bucket '{bucketName}' exists...");
            var args = new BucketExistsArgs().WithBucket(bucketName);
            bool found = await _minioClient.BucketExistsAsync(args);

            if (!found)
            {
                _logger.LogInformation($"Creating bucket: {bucketName}");
                var makeBucketArgs = new MakeBucketArgs().WithBucket(bucketName);
                await _minioClient.MakeBucketAsync(makeBucketArgs);
                _logger.LogInformation($"Bucket '{bucketName}' created successfully.");
            }
            else
            {
                _logger.LogInformation($"Bucket '{bucketName}' already exists.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error creating bucket '{bucketName}'.");
            throw;
        }
    }

    private bool IsValidBucketName(string bucketName)
    {
        return !string.IsNullOrEmpty(bucketName) &&
               bucketName.All(c => char.IsLetterOrDigit(c) || c == '-') &&
               bucketName.Length >= 3 && bucketName.Length <= 63;
    }


    public async Task UploadKeysAsync(string username)
    {
        var keyService = new KeyService();
        var (publicKey, privateKey) = keyService.GenerateRsaKeys();

        // Ensure the public bucket exists
        await EnsureBucketExistsAsync("pubkeys");

        // Upload public key to "pubkeys" bucket
        await UploadKeyAsync("pubkeys", $"{username}_public.pem", publicKey);

        // Upload private key to user's bucket
        await UploadKeyAsync(username, $"{username}_private.pem", privateKey);

        _logger.LogInformation($"Keys for user '{username}' uploaded successfully.");
    }

    private async Task EnsureBucketExistsAsync(string bucketName)
    {
        var found = await _minioClient.BucketExistsAsync(new BucketExistsArgs().WithBucket(bucketName));
        if (!found)
        {
            await _minioClient.MakeBucketAsync(new MakeBucketArgs().WithBucket(bucketName));
            _logger.LogInformation($"Bucket '{bucketName}' created.");
        }
    }

    private async Task UploadKeyAsync(string bucketName, string objectName, string content)
    {
        var bytes = Encoding.UTF8.GetBytes(content);
        using var stream = new MemoryStream(bytes);

        await _minioClient.PutObjectAsync(new PutObjectArgs()
            .WithBucket(bucketName)
            .WithObject(objectName)
            .WithStreamData(stream)
            .WithObjectSize(stream.Length)
            .WithContentType("text/plain"));

        _logger.LogInformation($"Uploaded '{objectName}' to bucket '{bucketName}'.");
    }


}
