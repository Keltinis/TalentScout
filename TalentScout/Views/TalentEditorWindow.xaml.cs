using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using TalentScout.Models.Entities;
using TalentScout.Repositories.Interfaces;
using TalentScout.Services.Interfaces;

namespace TalentScout.Views;

public partial class TalentEditorWindow : Window
{
    private readonly Talent _talent;
    public TalentEditorWindow(Talent talent)
    {
        InitializeComponent(); _talent = talent; DataContext = talent;
    }
    private async void ChangePhoto_Click(object sender, RoutedEventArgs e)
    {
        var picker = ((App)Application.Current).Services.GetRequiredService<IFilePickerService>();
        var path = picker.PickImageFile(); if (path is null) return;
        var storage = ((App)Application.Current).Services.GetRequiredService<IPhotoStorageService>();
        var stored = await storage.StoreAsync(path);
        var photo = _talent.Photos.FirstOrDefault(x => x.IsPrimary) ?? new TalentPhoto { IsPrimary = true, SortOrder = 0 };
        if (photo.Id == 0) _talent.Photos.Add(photo); photo.RelativePath = stored;
    }
    private async void Save_Click(object sender, RoutedEventArgs e)
    {
        await ((App)Application.Current).Services.GetRequiredService<ITalentRepository>().UpdateAsync(_talent);
        DialogResult = true;
    }
}
