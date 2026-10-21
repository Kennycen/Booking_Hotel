using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Options;

public interface IImageStorage
{
    Task<IReadOnlyList<string>> UploadAsync(IReadOnlyList<IFormFile> files, CancellationToken cancellationToken);
}

public sealed class CloudinaryImageStorage : IImageStorage
{
    private readonly CloudinaryOptions _options;

    public CloudinaryImageStorage(IOptions<CloudinaryOptions> options)
    {
        _options = options.Value;
    }

    public async Task<IReadOnlyList<string>> UploadAsync(IReadOnlyList<IFormFile> files, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_options.CloudName))
        {
            throw new InvalidOperationException("Cloudinary is not configured.");
        }

        var cloudinary = new Cloudinary(new Account(_options.CloudName, _options.ApiKey, _options.ApiSecret));
        var urls = new List<string>();

        foreach (var file in files)
        {
            await using var stream = file.OpenReadStream();
            var result = await cloudinary.UploadAsync(new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = "hotel-rooms"
            });

            if (result.Error is not null || result.SecureUrl is null)
            {
                throw new InvalidOperationException(result.Error?.Message ?? "Image upload failed.");
            }

            urls.Add(result.SecureUrl.ToString());
        }

        return urls;
    }
}