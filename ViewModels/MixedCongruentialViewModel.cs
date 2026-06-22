using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using GenValNumAl.Models;
using GenValNumAl.Services;

namespace GenValNumAl.ViewModels;

public partial class MixedCongruentialViewModel : GeneratorViewModelBase
{
    public override string Title => "Congruencial Mixto";

    // Hull-Dobell válido: gcd(3,16)=1, (5-1)=4 divisible por 2 y por 4 (ya que 4|16)
    [ObservableProperty] private string _x0 = "7";
    [ObservableProperty] private string _a  = "5";
    [ObservableProperty] private string _c  = "3";
    [ObservableProperty] private string _m  = "16";
    [ObservableProperty] private string _n  = "15";

    private readonly MixedCongruentialService _service = new();

    protected override IEnumerable<GeneratedNumber> ExecuteGenerate()
    {
        if (!long.TryParse(X0.Trim(), out long x0))
            throw new ValidationException("Error: 'X₀' debe ser un número entero no negativo.");
        if (!long.TryParse(A.Trim(), out long a))
            throw new ValidationException("Error: 'a' debe ser un número entero positivo.");
        if (!long.TryParse(C.Trim(), out long c))
            throw new ValidationException("Error: 'c' debe ser un número entero no negativo.");
        if (!long.TryParse(M.Trim(), out long m))
            throw new ValidationException("Error: 'm' debe ser un número entero positivo.");
        if (!int.TryParse(N.Trim(), out int n))
            throw new ValidationException("Error: 'n' debe ser un número entero positivo.");

        return _service.Generate(x0, a, c, m, n);
    }
}
