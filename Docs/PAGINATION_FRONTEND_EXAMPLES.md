# Ejemplos de Uso de la API de Paginación

## 📡 Llamadas desde el Frontend

### JavaScript/TypeScript

```typescript
// ============================================
// 1. BÚSQUEDA PAGINADA BÁSICA
// ============================================
async function searchCities(page = 0, size = 20) {
    const response = await fetch(
        `/api/cities?page=${page}&size=${size}`
    );
    const result = await response.json();
    
    console.log('Items:', result.items);
    console.log('Total pages:', result.totalPages);
    console.log('Has next page:', result.hasNextPage);
    
    return result;
}

// ============================================
// 2. BÚSQUEDA CON FILTROS
// ============================================
async function searchCitiesByCountry(country, page = 0, size = 20) {
    const params = new URLSearchParams({
        country: country,
        page: page.toString(),
        size: size.toString(),
        sortBy: 'Name',
        ascending: 'true'
    });
    
    const response = await fetch(`/api/cities?${params}`);
    const result = await response.json();
    
    return result;
}

// ============================================
// 3. BÚSQUEDA CON ORDENAMIENTO
// ============================================
async function searchCitiesSorted(sortBy = 'Name', ascending = true) {
    const params = new URLSearchParams({
        page: '0',
        size: '20',
        sortBy: sortBy,
        ascending: ascending.toString()
    });
    
    const response = await fetch(`/api/cities?${params}`);
    return await response.json();
}

// ============================================
// 4. NAVEGACIÓN DE PÁGINAS
// ============================================
class CityPaginator {
    private currentPage = 0;
    private pageSize = 20;
    private totalPages = 0;
    
    async loadPage(page: number) {
        const response = await fetch(
            `/api/cities?page=${page}&size=${this.pageSize}`
        );
        const result = await response.json();
        
        this.currentPage = result.pageNumber;
        this.totalPages = result.totalPages;
        
        return result;
    }
    
    async nextPage() {
        if (this.currentPage < this.totalPages - 1) {
            return await this.loadPage(this.currentPage + 1);
        }
        return null;
    }
    
    async previousPage() {
        if (this.currentPage > 0) {
            return await this.loadPage(this.currentPage - 1);
        }
        return null;
    }
    
    async firstPage() {
        return await this.loadPage(0);
    }
    
    async lastPage() {
        return await this.loadPage(this.totalPages - 1);
    }
}

// Uso:
const paginator = new CityPaginator();
const firstPage = await paginator.firstPage();
const nextPage = await paginator.nextPage();

// ============================================
// 5. COMPONENTE REACT CON PAGINACIÓN
// ============================================
import React, { useState, useEffect } from 'react';

interface PagedResult<T> {
    items: T[];
    pageNumber: number;
    pageSize: number;
    totalCount: number;
    totalPages: number;
    hasPreviousPage: boolean;
    hasNextPage: boolean;
}

interface City {
    id: number;
    name: string;
    country: string;
    latitude: number;
    longitude: number;
    createdAt: string;
}

function CityList() {
    const [result, setResult] = useState<PagedResult<City> | null>(null);
    const [page, setPage] = useState(0);
    const [country, setCountry] = useState('');
    const [sortBy, setSortBy] = useState('Name');
    const [ascending, setAscending] = useState(true);

    useEffect(() => {
        loadCities();
    }, [page, country, sortBy, ascending]);

    async function loadCities() {
        const params = new URLSearchParams({
            page: page.toString(),
            size: '20',
            sortBy: sortBy,
            ascending: ascending.toString()
        });
        
        if (country) {
            params.append('country', country);
        }

        const response = await fetch(`/api/cities?${params}`);
        const data = await response.json();
        setResult(data);
    }

    if (!result) return <div>Loading...</div>;

    return (
        <div>
            {/* Filtros */}
            <div className="filters">
                <input
                    type="text"
                    placeholder="Filter by country"
                    value={country}
                    onChange={(e) => {
                        setCountry(e.target.value);
                        setPage(0); // Reset to first page
                    }}
                />
                
                <select 
                    value={sortBy} 
                    onChange={(e) => setSortBy(e.target.value)}
                >
                    <option value="Name">Name</option>
                    <option value="Country">Country</option>
                    <option value="CreatedAt">Created Date</option>
                </select>
                
                <button onClick={() => setAscending(!ascending)}>
                    {ascending ? '↑ Ascending' : '↓ Descending'}
                </button>
            </div>

            {/* Lista de ciudades */}
            <ul>
                {result.items.map(city => (
                    <li key={city.id}>
                        {city.name}, {city.country}
                    </li>
                ))}
            </ul>

            {/* Paginación */}
            <div className="pagination">
                <button 
                    disabled={!result.hasPreviousPage}
                    onClick={() => setPage(page - 1)}
                >
                    Previous
                </button>
                
                <span>
                    Page {result.pageNumber + 1} of {result.totalPages}
                    ({result.totalCount} total cities)
                </span>
                
                <button 
                    disabled={!result.hasNextPage}
                    onClick={() => setPage(page + 1)}
                >
                    Next
                </button>
            </div>
        </div>
    );
}

// ============================================
// 6. COMPONENTE VUE CON PAGINACIÓN
// ============================================
// CityList.vue
<template>
  <div>
    <!-- Filtros -->
    <div class="filters">
      <input
        v-model="filters.country"
        @input="resetPage"
        placeholder="Filter by country"
      />
      
      <select v-model="filters.sortBy">
        <option value="Name">Name</option>
        <option value="Country">Country</option>
        <option value="CreatedAt">Created Date</option>
      </select>
      
      <button @click="toggleSortDirection">
        {{ filters.ascending ? '↑ Ascending' : '↓ Descending' }}
      </button>
    </div>

    <!-- Lista -->
    <ul v-if="result">
      <li v-for="city in result.items" :key="city.id">
        {{ city.name }}, {{ city.country }}
      </li>
    </ul>

    <!-- Paginación -->
    <div v-if="result" class="pagination">
      <button 
        :disabled="!result.hasPreviousPage"
        @click="previousPage"
      >
        Previous
      </button>
      
      <span>
        Page {{ result.pageNumber + 1 }} of {{ result.totalPages }}
        ({{ result.totalCount }} total)
      </span>
      
      <button 
        :disabled="!result.hasNextPage"
        @click="nextPage"
      >
        Next
      </button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, watch, onMounted } from 'vue';

const result = ref(null);
const page = ref(0);
const filters = ref({
  country: '',
  sortBy: 'Name',
  ascending: true
});

async function loadCities() {
  const params = new URLSearchParams({
    page: page.value.toString(),
    size: '20',
    sortBy: filters.value.sortBy,
    ascending: filters.value.ascending.toString()
  });
  
  if (filters.value.country) {
    params.append('country', filters.value.country);
  }

  const response = await fetch(`/api/cities?${params}`);
  result.value = await response.json();
}

function nextPage() {
  page.value++;
}

function previousPage() {
  page.value--;
}

function resetPage() {
  page.value = 0;
}

function toggleSortDirection() {
  filters.value.ascending = !filters.value.ascending;
}

// Watch for changes
watch([page, filters], loadCities, { deep: true });

onMounted(loadCities);
</script>
```

## 🔧 Ejemplos con cURL

```bash
# 1. Búsqueda básica (primera página, 20 elementos)
curl "http://localhost:5000/api/cities?page=0&size=20"

# 2. Búsqueda con filtro por país
curl "http://localhost:5000/api/cities?country=Colombia&page=0&size=20"

# 3. Búsqueda con ordenamiento
curl "http://localhost:5000/api/cities?sortBy=Name&ascending=true&page=0&size=20"

# 4. Búsqueda con múltiples filtros
curl "http://localhost:5000/api/cities?country=Colombia&nameContains=Bog&sortBy=Name&ascending=true&page=0&size=10"

# 5. Segunda página
curl "http://localhost:5000/api/cities?page=1&size=20"

# 6. Obtener todas (solo si < 1000 registros)
curl "http://localhost:5000/api/cities/all"

# 7. Estadísticas por país
curl "http://localhost:5000/api/cities/statistics/Colombia"
```

## 📊 Respuesta JSON Ejemplo

```json
{
  "items": [
    {
      "id": 1,
      "name": "Bogotá",
      "country": "Colombia",
      "latitude": 4.7110,
      "longitude": -74.0721,
      "createdAt": "2024-01-15T10:30:00Z"
    },
    {
      "id": 2,
      "name": "Medellín",
      "country": "Colombia",
      "latitude": 6.2442,
      "longitude": -75.5812,
      "createdAt": "2024-01-16T14:20:00Z"
    }
  ],
  "pageNumber": 0,
  "pageSize": 20,
  "totalCount": 150,
  "totalPages": 8,
  "hasPreviousPage": false,
  "hasNextPage": true,
  "isFirstPage": true,
  "isLastPage": false,
  "previousPageNumber": null,
  "nextPageNumber": 1
}
```

## 🎯 Mejores Prácticas Frontend

### 1. **Debouncing en Filtros**
```typescript
import { debounce } from 'lodash';

const debouncedSearch = debounce(async (searchTerm: string) => {
    await searchCities(searchTerm);
}, 300);

// Uso en input
<input onChange={(e) => debouncedSearch(e.target.value)} />
```

### 2. **Loading States**
```typescript
const [loading, setLoading] = useState(false);

async function loadCities() {
    setLoading(true);
    try {
        const result = await fetch('/api/cities?page=0&size=20');
        setResult(await result.json());
    } finally {
        setLoading(false);
    }
}
```

### 3. **Error Handling**
```typescript
async function loadCities() {
    try {
        const response = await fetch('/api/cities?page=0&size=20');
        
        if (!response.ok) {
            const error = await response.json();
            throw new Error(error.error || 'Failed to load cities');
        }
        
        return await response.json();
    } catch (error) {
        console.error('Error loading cities:', error);
        // Mostrar mensaje al usuario
    }
}
```

### 4. **Cache con React Query**
```typescript
import { useQuery } from '@tanstack/react-query';

function useCities(page: number, country?: string) {
    return useQuery({
        queryKey: ['cities', page, country],
        queryFn: async () => {
            const params = new URLSearchParams({
                page: page.toString(),
                size: '20'
            });
            if (country) params.append('country', country);
            
            const response = await fetch(`/api/cities?${params}`);
            return response.json();
        },
        keepPreviousData: true, // Mantener datos mientras carga la nueva página
        staleTime: 5 * 60 * 1000 // Cache por 5 minutos
    });
}

// Uso:
function CityList() {
    const [page, setPage] = useState(0);
    const { data, isLoading, error } = useCities(page);
    
    // ...
}
```

## 🚀 Optimizaciones

### 1. **Prefetch de Páginas**
```typescript
// Precargar la siguiente página
async function prefetchNextPage(currentPage: number) {
    const nextPage = currentPage + 1;
    fetch(`/api/cities?page=${nextPage}&size=20`);
}
```

### 2. **Infinite Scroll**
```typescript
function useInfiniteScroll() {
    const [items, setItems] = useState([]);
    const [page, setPage] = useState(0);
    const [hasMore, setHasMore] = useState(true);

    async function loadMore() {
        const response = await fetch(`/api/cities?page=${page}&size=20`);
        const result = await response.json();
        
        setItems(prev => [...prev, ...result.items]);
        setHasMore(result.hasNextPage);
        setPage(prev => prev + 1);
    }

    return { items, loadMore, hasMore };
}
```
