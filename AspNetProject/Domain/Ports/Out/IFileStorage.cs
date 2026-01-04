using AspNetProject.Domain.ValueObjects;

namespace AspNetProject.Domain.Ports.Out;

/// <summary>
/// Abstracción para el almacenamiento de archivos (S3, Azure Blob, Local Disk).
/// </summary>
public interface IFileStorage
{
    /// <summary>
    /// Sube un archivo al almacenamiento.
    /// </summary>
    /// <param name="fileName">Nombre único del archivo</param>
    /// <param name="content">Contenido binario</param>
    /// <param name="cancellationToken"></param>
    /// <returns>La ruta relativa o URL del archivo almacenado</returns>
    Task<string> UploadAsync(string fileName, Stream content, CancellationToken cancellationToken = default);

    /// <summary>
    /// Descarga un archivo del almacenamiento.
    /// </summary>
    Task<Stream> DownloadAsync(string fileName, CancellationToken cancellationToken = default);
}
