namespace AspNetProject.Domain.ValueObjects;

/// <summary>
/// Value Object para especificar criterios de paginación y ordenamiento.
/// Similar a Pageable de Spring Data.
/// </summary>
public class PageRequest
{
    private const int MaxPageSize = 100;
    private const int DefaultPageSize = 10;

    /// <summary>
    /// Número de página (0-indexed)
    /// </summary>
    public int PageNumber { get; }
    
    /// <summary>
    /// Tamaño de la página
    /// </summary>
    public int PageSize { get; }
    
    /// <summary>
    /// Campo por el cual ordenar
    /// </summary>
    public string? SortBy { get; }
    
    /// <summary>
    /// Dirección del ordenamiento (true = ascendente, false = descendente)
    /// </summary>
    public bool SortAscending { get; }
    
    /// <summary>
    /// Cantidad de elementos a saltar (calculado)
    /// </summary>
    public int Skip => PageNumber * PageSize;

    public PageRequest(
        int pageNumber = 0,
        int pageSize = DefaultPageSize,
        string? sortBy = null,
        bool sortAscending = true)
    {
        // Validaciones
        if (pageNumber < 0)
            throw new ArgumentException("Page number must be >= 0", nameof(pageNumber));
        
        if (pageSize <= 0)
            throw new ArgumentException("Page size must be > 0", nameof(pageSize));
        
        if (pageSize > MaxPageSize)
            throw new ArgumentException($"Page size must be <= {MaxPageSize}", nameof(pageSize));

        PageNumber = pageNumber;
        PageSize = pageSize;
        SortBy = sortBy;
        SortAscending = sortAscending;
    }

    /// <summary>
    /// Crea una solicitud de paginación por defecto (página 0, 10 elementos)
    /// </summary>
    public static PageRequest Default() => new();

    /// <summary>
    /// Crea una solicitud de paginación sin ordenamiento
    /// </summary>
    public static PageRequest Of(int pageNumber, int pageSize)
    {
        return new PageRequest(pageNumber, pageSize);
    }

    /// <summary>
    /// Crea una solicitud de paginación con ordenamiento
    /// </summary>
    public static PageRequest Of(int pageNumber, int pageSize, string sortBy, bool ascending = true)
    {
        return new PageRequest(pageNumber, pageSize, sortBy, ascending);
    }

    /// <summary>
    /// Crea una nueva solicitud para la siguiente página
    /// </summary>
    public PageRequest NextPage()
    {
        return new PageRequest(PageNumber + 1, PageSize, SortBy, SortAscending);
    }

    /// <summary>
    /// Crea una nueva solicitud para la página anterior
    /// </summary>
    public PageRequest PreviousPage()
    {
        if (PageNumber == 0)
            return this;
        
        return new PageRequest(PageNumber - 1, PageSize, SortBy, SortAscending);
    }
}
