using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using PawRescue.Application.DTOs;
using PawRescue.Application.Interfaces;

namespace PawRescue.Infrastructure.Storage;

/// <summary>
/// POLYMORPHISM: two interchangeable implementations of IImageStorageService.
/// Chosen in DependencyInjection.cs by Storage:Provider.
/// </summary>
public class CloudinaryImageStorageService : IImageStorageService
{
    private readonly Cloudinary _cloudinary;
    private readonly string _rootFolder;

    public CloudinaryImageStorageService(IOptions<StorageOptions> options)
    {
        var s = options.Value.Cloudinary;
        _cloudinary = new Cloudinary(new Account(s.CloudName, s.ApiKey, s.ApiSecret)) { Api = { Secure = true } };
        _rootFolder = s.RootFolder;
    }

    public async Task<StoredImage> UploadAsync(ImageUpload image, string folder, CancellationToken ct = default)
    {
        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(image.FileName, image.Content),
            Folder = $"{_rootFolder}/{folder}",
            // Resize huge phone photos server-side to save bandwidth on the map/list screens.
            Transformation = new Transformation().Width(1600).Crop("limit").Quality("auto")
        };

        var result = await _cloudinary.UploadAsync(uploadParams, ct);
        if (result.Error is not null)
            throw new InvalidOperationException($"Image upload failed: {result.Error.Message}");

        return new StoredImage(result.SecureUrl.ToString(), result.PublicId);
    }

    public async Task DeleteAsync(string storageKey, CancellationToken ct = default)
        => await _cloudinary.DestroyAsync(new DeletionParams(storageKey));
}

/// <summary>Development storage: saves to wwwroot/uploads so the app works without a cloud account.</summary>
public class LocalImageStorageService : IImageStorageService
{
    private readonly string _rootPath;
    private readonly string _publicBaseUrl;

    public LocalImageStorageService(IWebHostEnvironment env, IOptions<StorageOptions> options)
    {
        var webRoot = env.WebRootPath ?? Path.Combine(env.ContentRootPath, "wwwroot");
        _rootPath = Path.Combine(webRoot, "uploads");
        _publicBaseUrl = options.Value.PublicBaseUrl.TrimEnd('/');
    }

    public async Task<StoredImage> UploadAsync(ImageUpload image, string folder, CancellationToken ct = default)
    {
        var ext = Path.GetExtension(image.FileName);
        if (string.IsNullOrEmpty(ext)) ext = ".jpg";

        var key = $"{folder}/{Guid.NewGuid():N}{ext.ToLowerInvariant()}";
        var fullPath = Path.Combine(_rootPath, key.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

        await using var fs = File.Create(fullPath);
        await image.Content.CopyToAsync(fs, ct);

        return new StoredImage($"{_publicBaseUrl}/uploads/{key}", key);
    }

    public Task DeleteAsync(string storageKey, CancellationToken ct = default)
    {
        var fullPath = Path.Combine(_rootPath, storageKey.Replace('/', Path.DirectorySeparatorChar));
        if (File.Exists(fullPath)) File.Delete(fullPath);
        return Task.CompletedTask;
    }
}
