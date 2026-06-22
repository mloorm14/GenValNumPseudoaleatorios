using System.Threading.Tasks;

namespace GenValNumAl.Services;

public interface IFileDialogService
{
    /// <summary>
    /// Muestra el diálogo "Guardar como" y devuelve la ruta elegida, o null si el usuario cancela.
    /// </summary>
    Task<string?> PickSaveFilePathAsync(string defaultFileName);

    /// <summary>
    /// Muestra el diálogo "Abrir archivo" (.txt) y devuelve la ruta elegida, o null si el usuario cancela.
    /// </summary>
    Task<string?> PickOpenFilePathAsync();
}
