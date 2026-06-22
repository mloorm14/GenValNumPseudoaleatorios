using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using GenValNumAl.Services;
using GenValNumAl.ViewModels;

namespace GenValNumAl.Views;

public partial class MainWindow : Window
{
    public MainWindow() => InitializeComponent();

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);

        if (DataContext is MainWindowViewModel vm)
            vm.SetFileDialogService(new AvaloniaFileDialogService(this));
    }

    /// <summary>
    /// Las dos listas del sidebar (Generadores / Validadores) muestran su selección con
    /// binding OneWay a "SelectedMethod" para evitar que la lista inactiva, al no encontrar
    /// el ítem recién elegido en su propia colección, reescriba "SelectedMethod" a null
    /// (el clásico bug de dos ListBox compartiendo un único SelectedItem en TwoWay).
    /// Por eso la selección del usuario se reenvía aquí explícitamente al ViewModel.
    /// </summary>
    private void OnNavSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (DataContext is MainWindowViewModel vm &&
            e.AddedItems.Count > 0 &&
            e.AddedItems[0] is ViewModelBase selected)
        {
            vm.SelectedMethod = selected;
        }
    }

    // ── Implementación del servicio de diálogo, ligada a esta ventana ────────

    private sealed class AvaloniaFileDialogService : IFileDialogService
    {
        private readonly TopLevel _owner;

        public AvaloniaFileDialogService(TopLevel owner) => _owner = owner;

        public async Task<string?> PickSaveFilePathAsync(string defaultFileName)
        {
            var file = await _owner.StorageProvider.SaveFilePickerAsync(
                new FilePickerSaveOptions
                {
                    Title              = "Guardar secuencia de números",
                    SuggestedFileName  = defaultFileName,
                    DefaultExtension   = "txt",
                    FileTypeChoices    = new[]
                    {
                        new FilePickerFileType("Archivo de texto (*.txt)")
                        {
                            Patterns         = new[] { "*.txt" },
                            MimeTypes        = new[] { "text/plain" }
                        }
                    }
                });

            return file?.Path.LocalPath;
        }

        public async Task<string?> PickOpenFilePathAsync()
        {
            var files = await _owner.StorageProvider.OpenFilePickerAsync(
                new FilePickerOpenOptions
                {
                    Title         = "Seleccionar archivo de números",
                    AllowMultiple = false,
                    FileTypeFilter = new[]
                    {
                        new FilePickerFileType("Archivo de texto (*.txt)")
                        {
                            Patterns  = new[] { "*.txt" },
                            MimeTypes = new[] { "text/plain" }
                        }
                    }
                });

            return files.Count > 0 ? files[0].Path.LocalPath : null;
        }
    }
}
