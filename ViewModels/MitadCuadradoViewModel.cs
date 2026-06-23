using CommunityToolkit.Mvvm.ComponentModel;
using GenValNumAl.Models;
using GenValNumAl.Services;

namespace GenValNumAl.ViewModels;

public partial class MitadCuadradoViewModel : GeneradorViewModelBase
{
    public override string Titulo => "Mitad del Cuadrado";

    [ObservableProperty] private string _x0      = "1234";
    [ObservableProperty] private string _digitos = "4";
    [ObservableProperty] private string _n       = "10";

    private readonly ServicioMitadCuadrado _servicio = new();

    protected override ResultadoGeneracion EjecutarGeneracion()
    {
        if (!long.TryParse(X0.Trim(), out long x0))
            throw new ExcepcionValidacion("Error: 'X₀' debe ser un número entero positivo.");
        if (!int.TryParse(Digitos.Trim(), out int digitos))
            throw new ExcepcionValidacion("Error: 'Dígitos' debe ser un número entero positivo par.");
        if (!int.TryParse(N.Trim(), out int n))
            throw new ExcepcionValidacion("Error: 'n' debe ser un número entero positivo.");

        return _servicio.Generar(x0, digitos, n);
    }
}
