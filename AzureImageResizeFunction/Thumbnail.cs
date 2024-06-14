using System.Drawing.Imaging;
using System.Drawing;
using Azure.Storage.Blobs;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Drawing.Drawing2D;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp;

namespace AzureImageResizeFunction
{
    public class Thumbnail
    {
        private readonly ILogger<Thumbnail> _logger;
        private readonly BlobServiceClient _blobServiceClient;

        public Thumbnail(ILogger<Thumbnail> logger, BlobServiceClient blobServiceClient)
        {
            _logger = logger;
            _blobServiceClient = blobServiceClient;
        }

        [Function(nameof(Thumbnail))]
        public async Task Run([BlobTrigger("%IMAGES_CONTAINER_NAME%/{name}", Connection = "AZURE_SA_CS")] Stream stream, string name)
        {
            //using var blobStreamReader = new StreamReader(stream);
            //var content = await blobStreamReader.ReadToEndAsync();

            //try
            //{
            //    _logger.LogInformation($"C# Blob trigger function Processed blob\n Name: {name}");

            //    using var image = Image.FromStream(stream);

            //    int thumbnailWidth = Convert.ToInt32(Environment.GetEnvironmentVariable("THUMBNAIL_WIDTH"));
            //    //int thumbnailHeight = (int)((float)thumbnailWidth / image.Width * image.Height);
            //    int thumbnailHeight = Convert.ToInt32(Environment.GetEnvironmentVariable("THUMBNAIL_WIDTH"));

            //    using var thumbnail = new Bitmap(thumbnailWidth, thumbnailHeight);
            //    using var graphics = Graphics.FromImage(thumbnail);

            //    // Set the interpolation mode for high quality resizing
            //    graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            //    graphics.CompositingQuality = CompositingQuality.HighQuality;
            //    graphics.SmoothingMode = SmoothingMode.HighQuality;
            //    graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

            //    graphics.DrawImage(image, 0, 0, thumbnailWidth, thumbnailHeight);

            //    var destinationContainerName = Environment.GetEnvironmentVariable("THUMBNAIL_CONTAINER_NAME");
            //    var destinationContainer = _blobServiceClient.GetBlobContainerClient(destinationContainerName);
            //    var destinationBlob = destinationContainer.GetBlobClient(name);

            //    using var thumbnailStream = new MemoryStream();
            //    thumbnail.Save(thumbnailStream, ImageFormat.Jpeg);
            //    thumbnailStream.Position = 0;

            //    await destinationBlob.UploadAsync(thumbnailStream, overwrite: true);
            //    _logger.LogInformation($"Thumbnail uploaded to {destinationContainerName} successfully.");
            //}
            //catch (Exception ex)
            //{
            //    _logger.LogError($"Error processing blob {name}: {ex.Message}");
            //}

            try
            {
                _logger.LogInformation($"C# Blob trigger function Processed blob\n Name: {name}");

                using var image = SixLabors.ImageSharp.Image.Load(stream);

                // Compression quality (0-100, where 100 is best quality)
                int compressionQuality = 50; // Adjust as needed

                var encoder = new JpegEncoder
                {
                    Quality = compressionQuality // Set the JPEG compression quality
                };

                var destinationContainerName = Environment.GetEnvironmentVariable("THUMBNAIL_CONTAINER_NAME");
                var destinationContainer = _blobServiceClient.GetBlobContainerClient(destinationContainerName);
                var destinationBlob = destinationContainer.GetBlobClient(name);

                using var compressedStream = new MemoryStream();
                image.Save(compressedStream, encoder); // Save image with specified encoder

                compressedStream.Position = 0;
                await destinationBlob.UploadAsync(compressedStream, overwrite: true);

                _logger.LogInformation($"Compressed image uploaded to {destinationContainerName} successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error processing blob {name}: {ex.Message}");
            }
        }
    }
}
