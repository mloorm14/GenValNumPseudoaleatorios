using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GenValNumAl.Models;
using GenValNumAl.Services;

namespace GenValNumAl.ViewModels;

/// <summary>
/// Base compartida por los cuatro generadores.
/// Maneja resultados, errores, exportación y el ciclo generar → mostrar.
/// </summary>
public abstract partial class GeneratorViewModelBase : ViewModelBase
{
    // ── Propiedades comunes ──────────────────────────────────────────────────

    /// <summary>Nombre del método (mostrado en la barra lateral).</summary>
    public abstract string Title { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanExport))]
    private bool _hasResults;

    public bool HasError  => !string.IsNullOrEmpty(ErrorMessage);
    public bool CanExport => HasResults;

    public ObservableCollection<GeneratedNumber> Results { get; } = new();

    // ── Servicio de archivo (inyectado desde la vista) ───────────────────────

    private IFileDialogService? _fileDialogService;

    public void SetFileDialogService(IFileDialogService service) =>
        _fileDialogService = service;

    // ── Comandos ─────────────────────────────────────────────────────────────

    [RelayCommand]
    public void Generate()
    {
        ErrorMessage = string.Empty;
        Results.Clear();
        HasResults = false;

        try
        {
            foreach (var item in ExecuteGenerate())
                Results.Add(item);

            HasResults = Results.Count > 0;
        }
        catch (ValidationException ex)
        {
            ErrorMessage = ex.Message;
        }
        catch (OverflowException)
        {
            ErrorMessage =
                "Error de desbordamiento aritmético: los parámetros producen valores " +
                "demasiado grandes para un entero de 64 bits. Reduzca 'a', 'm' o el número de dígitos.";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error inesperado: {ex.Message}";
        }
    }

    [RelayCommand]
    public async Task ExportAsync()
    {
        if (_fileDialogService is null || !HasResults) return;

        ErrorMessage = string.Empty;

        try
        {
            string? path = await _fileDialogService.PickSaveFilePathAsync("numeros_aleatorios.txt");
            if (path is null) return;

            await File.WriteAllLinesAsync(path,
                Results.Select(r => r.Un.ToString("F10")));
        }
        catch (Exception ex)
        {
            ErrorMessage = $"No se pudo exportar el archivo: {ex.Message}";
        }
    }

    // ── Punto de extensión ───────────────────────────────────────────────────

    /// <summary>
    /// Implementado por cada subclase.  Puede lanzar <see cref="ValidationException"/>.
    /// </summary>
    protected abstract System.Collections.Generic.IEnumerable<GeneratedNumber> ExecuteGenerate();
}
