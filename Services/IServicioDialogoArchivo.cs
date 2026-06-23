using System.Threading.Tasks;

namespace GenValNumAl.Services;

public interface IServicioDialogoArchivo
{
    /// <summary>
    /// Mostrar el diálogo "Guardar como" y devolver la ruta elegida, o null si el usuario cancela.
    /// </summary>
    Task<string?> ElegirRutaGuardarArchivoAsync(string nombreArchivoSugerido);

    /// <summary>
    /// Mostrar el diálogo "Abrir archivo" (.txt) y devolver la ruta elegida, o null si el usuario cancela.
    /// </summary>
    Task<string?> ElegirRutaAbrirArchivoAsync();
}
