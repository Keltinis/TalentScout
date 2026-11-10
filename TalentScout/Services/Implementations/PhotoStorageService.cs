using System.IO;
using TalentScout.Services.Interfaces;

namespace TalentScout.Services.Implementations;

public sealed class PhotoStorageService : IPhotoStorageService
{
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp"
    };

    private readonly string _photosDirectory = Path.Combine(AppContext.BaseDirectory, "Assets", "Photos");

    public async Task<string> StoreAsync(string sourceFilePath, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceFilePath);
        if (!File.Exists(sourceFilePath))
        {
            throw new FileNotFoundException("Файл фото не знайдено.", sourceFilePath);
        }

        var extension = Path.GetExtension(sourceFilePath);
        if (!AllowedExtensions.Contains(extension))
        {
            throw new InvalidOperationException("Підтримуються лише JPG, PNG та WEBP-файли.");
        }

        Directory.CreateDirectory(_photosDirectory);
        var fileName = $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
        var destinationPath = Path.Combine(_photosDirectory, fileName);
        await using var source = File.OpenRead(sourceFilePath);
        await using var destination = File.Create(destinationPath);
        await source.CopyToAsync(destination, cancellationToken);
        return Path.Combine("Assets", "Photos", fileName);
    }

    public Task DeleteAsync(string relativePath, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var filePath = GetAbsolutePath(relativePath);
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        return Task.CompletedTask;
    }

    public string GetAbsolutePath(string relativePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(relativePath);
        var root = Path.GetFullPath(AppContext.BaseDirectory);
        var path = Path.GetFullPath(Path.Combine(root, relativePath));
        if (!path.StartsWith(root, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Шлях до фото виходить за межі папки застосунку.");
        }

        return path;
    }
}
