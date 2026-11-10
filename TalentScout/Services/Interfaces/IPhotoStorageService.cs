namespace TalentScout.Services.Interfaces;

public interface IPhotoStorageService
{
    Task<string> StoreAsync(string sourceFilePath, CancellationToken cancellationToken = default);
    Task DeleteAsync(string relativePath, CancellationToken cancellationToken = default);
    string GetAbsolutePath(string relativePath);
}
