using TalentScout.Models.Entities;

namespace TalentScout.Services.Interfaces;

public interface ITalentProfileService
{
    Task<int> CreateAsync(Talent talent, string primaryPhotoPath, CancellationToken cancellationToken = default);
}
