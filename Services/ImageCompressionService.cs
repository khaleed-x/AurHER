using AurHER.Services.Interfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

namespace AurHER.Services
{
    public class ImageCompressionService : IImageCompressionService
    {
        public async Task<byte[]> CompressAndResizeAsync(IFormFile file, int maxWidth = 800, 
        int maxHeight = 800, int quality = 75, CancellationToken cancellationToken = default)
        {
            using var stream = file.OpenReadStream();
            using var image = await Image.LoadAsync(stream, cancellationToken );
            
             if (image.Width > 5000 || image.Height > 5000) return;

            // Resize if needed (maintain aspect ratio)
            if (image.Width > maxWidth || image.Height > maxHeight)
            {
                image.Mutate(x => x.Resize(new ResizeOptions
                {
                    Mode = ResizeMode.Max,
                    Size = new Size(maxWidth, maxHeight)
                }));
            }
            
         
            //remove metadata(reduce size)
             image.Metadata.ExifProfile = null;

                // Encode as WebP with specified quality
            var encoder = new WebpEncoder
            {
                Quality = quality,
                Method =WebpEncodingMethod.BestCompression,
                FileFormat = WebpFileFormatType.Lossy
            };
            
            using var outputStream = new MemoryStream();
            await image.SaveAsync(outputStream, encoder);
            return outputStream.ToArray();
        }
    }
}