using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace GenValNumAl.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public ObservableCollection<GeneratorViewModelBase> Generadores { get; } = new()
    {
        new MidSquareViewModel(),
        new MixedCongruentialViewModel(),
        new MultiplicativeCongruentialViewModel(),
        new LaggedFibonacciViewModel(),
    };

    public ObservableCollection<ValidationViewModel> Validadores { get; } = new()
    {
        new ValidationViewModel(),
    };

    [ObservableProperty]
    private ViewModelBase _selectedMethod;

    public MainWindowViewModel() =>
        _selectedMethod = Generadores[0];

    /// <summary>
    /// Inyecta el servicio de diálogo de archivos en todos los ViewModels.
    /// Llamado desde MainWindow.axaml.cs una vez que la ventana está abierta.
    /// </summary>
    public void SetFileDialogService(Services.IFileDialogService service)
    {
        foreach (var generador in Generadores)
            generador.SetFileDialogService(service);

        foreach (var validador in Validadores)
            validador.SetFileDialogService(service);
    }
}
