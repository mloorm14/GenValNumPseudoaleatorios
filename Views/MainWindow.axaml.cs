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

        if (DataContext is VentanaPrincipalViewModel vm)
            vm.EstablecerServicioDialogoArchivo(new ServicioDialogoArchivoAvalonia(this));
    }

    /// <summary>
    /// Las dos listas del sidebar (Generadores / Validadores) muestran su selección con
    /// binding OneWay a "MetodoSeleccionado" para evitar que la lista inactiva, al no encontrar
    /// el ítem recién elegido en su propia colección, reescriba "MetodoSeleccionado" a null
    /// (el clásico bug de dos ListBox compartiendo un único SelectedItem en TwoWay).
    /// Por eso la selección del usuario se reenvía aquí explícitamente al ViewModel.
    /// </summary>
    private void OnNavSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (DataContext is VentanaPrincipalViewModel vm &&
            e.AddedItems.Count > 0 &&
            e.AddedItems[0] is ViewModelBase seleccionado)
        {
            vm.MetodoSeleccionado = seleccionado;
        }
    }

    // ── Implementación del servicio de diálogo, ligada a esta ventana ────────

    private sealed class ServicioDialogoArchivoAvalonia : IServicioDialogoArchivo
    {
        private readonly TopLevel _propietario;

        public ServicioDialogoArchivoAvalonia(TopLevel propietario) => _propietario = propietario;

        public async Task<string?> ElegirRutaGuardarArchivoAsync(string nombreArchivoSugerido)
        {
            var archivo = await _propietario.StorageProvider.SaveFilePickerAsync(
                new FilePickerSaveOptions
                {
                    Title              = "Guardar secuencia de números",
                    SuggestedFileName  = nombreArchivoSugerido,
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

            return archivo?.Path.LocalPath;
        }

        public async Task<string?> ElegirRutaAbrirArchivoAsync()
        {
            var archivos = await _propietario.StorageProvider.OpenFilePickerAsync(
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

            return archivos.Count > 0 ? archivos[0].Path.LocalPath : null;
        }
    }
}
