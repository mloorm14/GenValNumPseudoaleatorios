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
public partial class ValidacionViewModel : ViewModelBase
{
    public string Titulo => "Validación Estadística";

    public ObservableCollection<OpcionPruebaValidacion> PruebasDisponibles { get; } = new()
    {
        new OpcionPruebaValidacion { Nombre = "Prueba de Medias",                     Prueba = new PruebaMedias() },
        new OpcionPruebaValidacion { Nombre = "Prueba de Varianza",                   Prueba = new PruebaVarianza() },
        new OpcionPruebaValidacion { Nombre = "Prueba de Uniformidad (Chi-Cuadrado)", Prueba = new PruebaUniformidadChiCuadrado(), RequiereIntervalos = true },
        new OpcionPruebaValidacion { Nombre = "Prueba Kolmogorov-Smirnov",            Prueba = new PruebaKolmogorovSmirnov() },
        new OpcionPruebaValidacion { Nombre = "Corridas Arriba y Abajo",              Prueba = new PruebaCorridasArribaAbajo() },
        new OpcionPruebaValidacion { Nombre = "Corridas Arriba y Abajo de la Media",  Prueba = new PruebaCorridasArribaAbajoMedia() },
        new OpcionPruebaValidacion { Nombre = "Prueba de Poker",                      Prueba = new PruebaPoker() },
    };

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(MostrarIntervalos))]
    private OpcionPruebaValidacion _pruebaSeleccionada;

    [ObservableProperty]
    private string _alpha = "0.05";

    [ObservableProperty]
    private string _intervalos = "10";

    [ObservableProperty]
    private string _nombreArchivo = "(ningún archivo cargado)";

    [ObservableProperty]
    private int _cantidadDatos;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TieneError))]
    private string _mensajeError = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanValidar))]
    private bool _tieneDatos;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanExportar))]
    private string _reporteTexto = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanValidar))]
    [NotifyPropertyChangedFor(nameof(CanExportar))]
    private bool _estaOcupado;

    private List<double>? _datos;

    public bool TieneError => !string.IsNullOrEmpty(MensajeError);
    public bool MostrarIntervalos => PruebaSeleccionada?.RequiereIntervalos ?? false;
    public bool CanValidar => TieneDatos && !EstaOcupado;
    public bool CanExportar => !string.IsNullOrEmpty(ReporteTexto) && !EstaOcupado;

    public ValidacionViewModel() =>
        _pruebaSeleccionada = PruebasDisponibles[0];

    // ── Servicio de archivo (inyectado desde la vista) ───────────────────────

    private IServicioDialogoArchivo? _servicioDialogoArchivo;

    public void EstablecerServicioDialogoArchivo(IServicioDialogoArchivo servicio) =>
        _servicioDialogoArchivo = servicio;

    // ── Comandos ─────────────────────────────────────────────────────────────

    [RelayCommand]
    private async Task CargarArchivoAsync()
    {
        if (_servicioDialogoArchivo is null) return;

        MensajeError = string.Empty;

        try
        {
            string? ruta = await _servicioDialogoArchivo.ElegirRutaAbrirArchivoAsync();
            if (ruta is null) return;

            EstaOcupado = true;
            _datos = await Task.Run(() => AnalizadorArchivoNumeros.Analizar(File.ReadAllText(ruta)));
            NombreArchivo = Path.GetFileName(ruta);
            CantidadDatos = _datos.Count;
            TieneDatos = true;
            ReporteTexto = string.Empty;
        }
        catch (ExcepcionValidacion ex)
        {
            MensajeError = ex.Message;
            TieneDatos = false;
        }
        catch (System.Exception ex)
        {
            MensajeError = $"No se pudo leer el archivo: {ex.Message}";
            TieneDatos = false;
        }
        finally
        {
            EstaOcupado = false;
        }
    }

    [RelayCommand]
    private async Task ValidarAsync()
    {
        MensajeError = string.Empty;

        if (_datos is null || _datos.Count == 0)
        {
            MensajeError = "Primero debe cargar un archivo con números.";
            return;
        }

        if (PruebaSeleccionada is null)
        {
            MensajeError = "Debe seleccionar una prueba de validación.";
            return;
        }

        if (!double.TryParse(Alpha, NumberStyles.Float, CultureInfo.InvariantCulture, out double alpha) ||
            alpha <= 0 || alpha >= 1)
        {
            MensajeError = "El valor de Alpha (α) debe ser un número entre 0 y 1.";
            return;
        }

        int intervalos = 10;
        if (PruebaSeleccionada.RequiereIntervalos &&
            (!int.TryParse(Intervalos, NumberStyles.Integer, CultureInfo.InvariantCulture, out intervalos) || intervalos < 2))
        {
            MensajeError = "El número de intervalos (k) debe ser un entero mayor o igual a 2.";
            return;
        }

        EstaOcupado = true;
        ReporteTexto = string.Empty;
        try
        {
            var parametros = new ParametrosValidacion { Alpha = alpha, Intervalos = intervalos };
            var prueba = PruebaSeleccionada.Prueba;
            var datos = _datos;

            var resultado = await Task.Run(() => prueba.Ejecutar(datos, parametros));
            ReporteTexto = resultado.Reporte;
        }
        catch (ExcepcionValidacion ex)
        {
            MensajeError = ex.Message;
        }
        catch (System.Exception ex)
        {
            MensajeError = $"Error inesperado: {ex.Message}";
        }
        finally
        {
            EstaOcupado = false;
        }
    }

    [RelayCommand]
    private async Task ExportarAsync()
    {
        if (_servicioDialogoArchivo is null || string.IsNullOrEmpty(ReporteTexto)) return;

        MensajeError = string.Empty;

        try
        {
            string? ruta = await _servicioDialogoArchivo.ElegirRutaGuardarArchivoAsync("reporte_validacion.txt");
            if (ruta is null) return;

            await File.WriteAllTextAsync(ruta, ReporteTexto);
        }
        catch (System.Exception ex)
        {
            MensajeError = $"No se pudo guardar el reporte: {ex.Message}";
        }
    }
}
