using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GenValNumAl.Models;
using GenValNumAl.Services;
using GenValNumAl.Services.Validation;

namespace GenValNumAl.ViewModels;

/// <summary>
/// ViewModel del módulo de Validación Estadística: carga una muestra desde .txt,
/// ejecuta la prueba seleccionada en segundo plano y permite exportar el reporte.
/// </summary>
public partial class ValidationViewModel : ViewModelBase
{
    public string Title => "Validación Estadística";

    public ObservableCollection<ValidationTestOption> PruebasDisponibles { get; } = new()
    {
        new ValidationTestOption { Nombre = "Prueba de Medias",                     Prueba = new MeanTest() },
        new ValidationTestOption { Nombre = "Prueba de Varianza",                   Prueba = new VarianceTest() },
        new ValidationTestOption { Nombre = "Prueba de Uniformidad (Chi-Cuadrado)", Prueba = new ChiSquareUniformityTest(), RequiereIntervalos = true },
        new ValidationTestOption { Nombre = "Prueba Kolmogorov-Smirnov",            Prueba = new KolmogorovSmirnovTest() },
        new ValidationTestOption { Nombre = "Corridas Arriba y Abajo",              Prueba = new RunsUpDownTest() },
        new ValidationTestOption { Nombre = "Corridas Arriba y Abajo de la Media",  Prueba = new RunsUpDownMeanTest() },
        new ValidationTestOption { Nombre = "Prueba de Poker",                      Prueba = new PokerTest() },
    };

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(MostrarIntervalos))]
    private ValidationTestOption _pruebaSeleccionada;

    [ObservableProperty]
    private string _alpha = "0.05";

    [ObservableProperty]
    private string _intervalos = "10";

    [ObservableProperty]
    private string _nombreArchivo = "(ningún archivo cargado)";

    [ObservableProperty]
    private int _cantidadDatos;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanValidar))]
    private bool _hasData;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanExportar))]
    private string _reporteTexto = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanValidar))]
    [NotifyPropertyChangedFor(nameof(CanExportar))]
    private bool _isBusy;

    private List<double>? _datos;

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);
    public bool MostrarIntervalos => PruebaSeleccionada?.RequiereIntervalos ?? false;
    public bool CanValidar => HasData && !IsBusy;
    public bool CanExportar => !string.IsNullOrEmpty(ReporteTexto) && !IsBusy;

    public ValidationViewModel() =>
        _pruebaSeleccionada = PruebasDisponibles[0];

    // ── Servicio de archivo (inyectado desde la vista) ───────────────────────

    private IFileDialogService? _fileDialogService;

    public void SetFileDialogService(IFileDialogService service) =>
        _fileDialogService = service;

    // ── Comandos ─────────────────────────────────────────────────────────────

    [RelayCommand]
    private async Task CargarArchivoAsync()
    {
        if (_fileDialogService is null) return;

        ErrorMessage = string.Empty;

        try
        {
            string? ruta = await _fileDialogService.PickOpenFilePathAsync();
            if (ruta is null) return;

            IsBusy = true;
            _datos = await Task.Run(() => NumberFileParser.Parse(File.ReadAllText(ruta)));
            NombreArchivo = Path.GetFileName(ruta);
            CantidadDatos = _datos.Count;
            HasData = true;
            ReporteTexto = string.Empty;
        }
        catch (ValidationException ex)
        {
            ErrorMessage = ex.Message;
            HasData = false;
        }
        catch (System.Exception ex)
        {
            ErrorMessage = $"No se pudo leer el archivo: {ex.Message}";
            HasData = false;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task ValidarAsync()
    {
        ErrorMessage = string.Empty;

        if (_datos is null || _datos.Count == 0)
        {
            ErrorMessage = "Primero debe cargar un archivo con números.";
            return;
        }

        if (PruebaSeleccionada is null)
        {
            ErrorMessage = "Debe seleccionar una prueba de validación.";
            return;
        }

        if (!double.TryParse(Alpha, NumberStyles.Float, CultureInfo.InvariantCulture, out double alpha) ||
            alpha <= 0 || alpha >= 1)
        {
            ErrorMessage = "El valor de Alpha (α) debe ser un número entre 0 y 1.";
            return;
        }

        int intervalos = 10;
        if (PruebaSeleccionada.RequiereIntervalos &&
            (!int.TryParse(Intervalos, NumberStyles.Integer, CultureInfo.InvariantCulture, out intervalos) || intervalos < 2))
        {
            ErrorMessage = "El número de intervalos (k) debe ser un entero mayor o igual a 2.";
            return;
        }

        IsBusy = true;
        ReporteTexto = string.Empty;
        try
        {
            var parametros = new ValidationParameters { Alpha = alpha, Intervalos = intervalos };
            var prueba = PruebaSeleccionada.Prueba;
            var datos = _datos;

            var resultado = await Task.Run(() => prueba.Ejecutar(datos, parametros));
            ReporteTexto = resultado.Reporte;
        }
        catch (ValidationException ex)
        {
            ErrorMessage = ex.Message;
        }
        catch (System.Exception ex)
        {
            ErrorMessage = $"Error inesperado: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task ExportarAsync()
    {
        if (_fileDialogService is null || string.IsNullOrEmpty(ReporteTexto)) return;

        ErrorMessage = string.Empty;

        try
        {
            string? ruta = await _fileDialogService.PickSaveFilePathAsync("reporte_validacion.txt");
            if (ruta is null) return;

            await File.WriteAllTextAsync(ruta, ReporteTexto);
        }
        catch (System.Exception ex)
        {
            ErrorMessage = $"No se pudo guardar el reporte: {ex.Message}";
        }
    }
}
