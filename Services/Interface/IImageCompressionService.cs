namespace AurHER.Services.Interfaces
{
    public interface IImageCompressionService
    {
        Task<byte[]?> CompressAndResizeAsync(IFormFile file, int maxWidth = 800, 
        int maxHeight = 800, int quality = 75,
        CancellationToken cancellationToken = default);
    }
}