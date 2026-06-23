using CommunityToolkit.Mvvm.ComponentModel;
using GenValNumAl.Models;
using GenValNumAl.Services;

namespace GenValNumAl.ViewModels;

public partial class CongruencialMixtoViewModel : GeneradorViewModelBase
{
    public override string Titulo => "Congruencial Mixto";

    // Hull-Dobell válido: mcd(3,16)=1, (5-1)=4 divisible por 2 y por 4 (ya que 4|16)
    [ObservableProperty] private string _x0 = "7";
    [ObservableProperty] private string _a  = "5";
    [ObservableProperty] private string _c  = "3";
    [ObservableProperty] private string _m  = "16";
    [ObservableProperty] private string _n  = "15";

    private readonly ServicioCongruencialMixto _servicio = new();

    protected override ResultadoGeneracion EjecutarGeneracion()
    {
        if (!long.TryParse(X0.Trim(), out long x0))
            throw new ExcepcionValidacion("Error: 'X₀' debe ser un número entero no negativo.");
        if (!long.TryParse(A.Trim(), out long a))
            throw new ExcepcionValidacion("Error: 'a' debe ser un número entero positivo.");
        if (!long.TryParse(C.Trim(), out long c))
            throw new ExcepcionValidacion("Error: 'c' debe ser un número entero no negativo.");
        if (!long.TryParse(M.Trim(), out long m))
            throw new ExcepcionValidacion("Error: 'm' debe ser un número entero positivo.");
        if (!int.TryParse(N.Trim(), out int n))
            throw new ExcepcionValidacion("Error: 'n' debe ser un número entero positivo.");

        return _servicio.Generar(x0, a, c, m, n);
    }
}
