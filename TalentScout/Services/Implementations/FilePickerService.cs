using Microsoft.Win32;
using TalentScout.Services.Interfaces;

namespace TalentScout.Services.Implementations;

public sealed class FilePickerService : IFilePickerService
{
    public string? PickImageFile()
    {
        var dialog = new OpenFileDialog
        {
            Title = "Оберіть головне фото таланту",
            Filter = "Зображення|*.jpg;*.jpeg;*.png;*.webp",
            Multiselect = false
        };

        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }
}
