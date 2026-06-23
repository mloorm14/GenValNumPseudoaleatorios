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
/// Gestiona resultados, errores, exportación y el ciclo generar → mostrar.
/// </summary>
public abstract partial class GeneradorViewModelBase : ViewModelBase
{
    // ── Propiedades comunes ──────────────────────────────────────────────────

    /// <summary>Nombre del método, mostrado en la barra lateral.</summary>
    public abstract string Titulo { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TieneError))]
    private string _mensajeError = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PuedeExportar))]
    private bool _tieneResultados;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TieneCiclo))]
    private string? _mensajeCiclo;

    public bool TieneError   => !string.IsNullOrEmpty(MensajeError);
    public bool PuedeExportar => TieneResultados;
    public bool TieneCiclo   => !string.IsNullOrEmpty(MensajeCiclo);

    public ObservableCollection<NumeroGenerado> Resultados { get; } = new();

    // ── Servicio de archivo (inyectado desde la vista) ───────────────────────

    private IServicioDialogoArchivo? _servicioDialogoArchivo;

    public void EstablecerServicioDialogoArchivo(IServicioDialogoArchivo servicio) =>
        _servicioDialogoArchivo = servicio;

    // ── Comandos ─────────────────────────────────────────────────────────────

    [RelayCommand]
    public void Generar()
    {
        MensajeError = string.Empty;
        MensajeCiclo = null;
        Resultados.Clear();
        TieneResultados = false;

        try
        {
            var resultado = EjecutarGeneracion();
            foreach (var item in resultado.Numeros)
                Resultados.Add(item);

            TieneResultados = Resultados.Count > 0;
            MensajeCiclo = resultado.Ciclo?.Mensaje;
        }
        catch (ExcepcionValidacion ex)
        {
            MensajeError = ex.Message;
        }
        catch (OverflowException)
        {
            MensajeError =
                "Error de desbordamiento aritmético: los parámetros producen valores " +
                "demasiado grandes para un entero de 64 bits. Reduzca 'a', 'm' o el número de dígitos.";
        }
        catch (Exception ex)
        {
            MensajeError = $"Error inesperado: {ex.Message}";
        }
    }

    [RelayCommand]
    public async Task ExportarAsync()
    {
        if (_servicioDialogoArchivo is null || !TieneResultados) return;

        MensajeError = string.Empty;

        try
        {
            string? ruta = await _servicioDialogoArchivo.ElegirRutaGuardarArchivoAsync("numeros_aleatorios.txt");
            if (ruta is null) return;

            await File.WriteAllLinesAsync(ruta,
                Resultados.Select(r => r.Un.ToString("F10")));
        }
        catch (Exception ex)
        {
            MensajeError = $"No se pudo exportar el archivo: {ex.Message}";
        }
    }

    // ── Punto de extensión ───────────────────────────────────────────────────

    /// <summary>
    /// Implementado por cada subclase. Puede lanzar <see cref="ExcepcionValidacion"/>.
    /// </summary>
    protected abstract ResultadoGeneracion EjecutarGeneracion();
}
