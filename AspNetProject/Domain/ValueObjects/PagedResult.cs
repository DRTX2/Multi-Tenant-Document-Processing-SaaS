namespace AspNetProject.Domain.ValueObjects;

/// <summary>
/// Value Object genérico para representar resultados paginados.
/// Similar a Page<T> de Spring Data.
/// 
/// DISEÑO: Usamos record para inmutabilidad y comparación por valor.
/// </summary>
/// <typeparam name="T">Tipo de los elementos en la página</typeparam>
public record PagedResult<T>
{
    /// <summary>
    /// Elementos de la página actual.
    /// </summary>
    public required IReadOnlyList<T> Items { get; init; }
    
    /// <summary>
    /// Número de página actual (0-indexed, como Spring Boot).
    /// </summary>
    public required int PageNumber { get; init; }
    
    /// <summary>
    /// Tamaño de página (cantidad de elementos por página).
    /// </summary>
    public required int PageSize { get; init; }
    
    /// <summary>
    /// Total de elementos en todas las páginas.
    /// </summary>
    public required int TotalCount { get; init; }
    
    /// <summary>
    /// Total de páginas.
    /// </summary>
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    
    /// <summary>
    /// Indica si hay página anterior.
    /// </summary>
    public bool HasPreviousPage => PageNumber > 0;
    
    /// <summary>
    /// Indica si hay página siguiente.
    /// </summary>
    public bool HasNextPage => PageNumber < TotalPages - 1;
    
    /// <summary>
    /// Indica si esta es la primera página.
    /// </summary>
    public bool IsFirstPage => PageNumber == 0;
    
    /// <summary>
    /// Indica si esta es la última página.
    /// </summary>
    public bool IsLastPage => PageNumber >= TotalPages - 1;
    
    /// <summary>
    /// Número de la página anterior (si existe).
    /// </summary>
    public int? PreviousPageNumber => HasPreviousPage ? PageNumber - 1 : null;
    
    /// <summary>
    /// Número de la página siguiente (si existe).
    /// </summary>
    public int? NextPageNumber => HasNextPage ? PageNumber + 1 : null;

    /// <summary>
    /// Crea un resultado paginado vacío.
    /// </summary>
    public static PagedResult<T> Empty(int pageNumber = 0, int pageSize = 10)
    {
        return new PagedResult<T>
        {
            Items = Array.Empty<T>(),
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = 0
        };
    }

    /// <summary>
    /// Transforma los elementos de la página a otro tipo.
    /// Útil para mapear de entidades a DTOs.
    /// </summary>
    public PagedResult<TResult> Map<TResult>(Func<T, TResult> mapper)
    {
        var mappedItems = Items.Select(mapper).ToList();
        return new PagedResult<TResult>
        {
            Items = mappedItems,
            PageNumber = PageNumber,
            PageSize = PageSize,
            TotalCount = TotalCount
        };
    }
}
