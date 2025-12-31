-- Script de inicialización de la base de datos PostgreSQL
-- Este script se ejecuta automáticamente cuando se crea el contenedor de Docker

-- Crear extensiones útiles
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "pg_trgm"; -- Para búsquedas de texto

-- Configurar zona horaria
SET timezone = 'America/New_York';

-- Mensaje de confirmación
DO $$
BEGIN
    RAISE NOTICE 'Base de datos aspnetproject inicializada correctamente';
    RAISE NOTICE 'Extensiones instaladas: uuid-ossp, pg_trgm';
END $$;
