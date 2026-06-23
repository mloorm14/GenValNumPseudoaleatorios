using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace GenValNumAl.ViewModels;

public partial class VentanaPrincipalViewModel : ViewModelBase
{
    public ObservableCollection<GeneradorViewModelBase> Generadores { get; } = new()
    {
        new MitadCuadradoViewModel(),
        new CongruencialMixtoViewModel(),
        new CongruencialMultiplicativoViewModel(),
        new FibonacciRetardadoViewModel(),
    };

    public ObservableCollection<ValidacionViewModel> Validadores { get; } = new()
    {
        new ValidacionViewModel(),
    };

    [ObservableProperty]
    private ViewModelBase _metodoSeleccionado;

    public VentanaPrincipalViewModel() =>
        _metodoSeleccionado = Generadores[0];

    /// <summary>
    /// Inyectar el servicio de diálogo de archivos en todos los ViewModels.
    /// Se invoca desde MainWindow.axaml.cs una vez que la ventana está abierta.
    /// </summary>
    public void EstablecerServicioDialogoArchivo(Services.IServicioDialogoArchivo servicio)
    {
        foreach (var generador in Generadores)
            generador.EstablecerServicioDialogoArchivo(servicio);

        foreach (var validador in Validadores)
            validador.EstablecerServicioDialogoArchivo(servicio);
    }
}
