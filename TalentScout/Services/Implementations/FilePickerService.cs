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
            Filter = "Файли зображень (*.jpg;*.jpeg;*.png;*.webp;*.bmp)|*.jpg;*.jpeg;*.png;*.webp;*.bmp|Усі файли (*.*)|*.*",
            DefaultExt = ".jpg",
            CheckFileExists = true,
            Multiselect = false
        };

        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }
}
