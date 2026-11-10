using TalentScout.Models.Entities;
using TalentScout.Repositories.Interfaces;
using TalentScout.Services.Interfaces;

namespace TalentScout.Services.Implementations;

public sealed class TalentProfileService(
    ITalentRepository talentRepository,
    IPhotoStorageService photoStorageService) : ITalentProfileService
{
    public async Task<int> CreateAsync(Talent talent, string primaryPhotoPath, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(talent);
        if (string.IsNullOrWhiteSpace(primaryPhotoPath))
        {
            throw new InvalidOperationException("Для створення анкети потрібно вибрати головну фотографію.");
        }

        var storedPath = await photoStorageService.StoreAsync(primaryPhotoPath, cancellationToken);
        talent.Photos.Add(new TalentPhoto { RelativePath = storedPath, IsPrimary = true, SortOrder = 0 });

        try
        {
            return await talentRepository.AddAsync(talent, cancellationToken);
        }
        catch
        {
            await photoStorageService.DeleteAsync(storedPath, cancellationToken);
            throw;
        }
    }
}
