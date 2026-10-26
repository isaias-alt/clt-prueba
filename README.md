# CLT Prueba: API de Usuarios, Direcciones y Divisas

API REST en .NET con Minimal API, Entity Framework Core (SQLite), FluentValidation y CQRS, con seguridad por API Key y documentación en Swagger.

## Versiones y paquetes

- **.NET 10** (SDK 10.0, `net10.0`)
- Entity Framework Core 10 (SQLite)
- FluentValidation 12
- Swashbuckle.AspNetCore 10 (Swagger UI)

## Cómo correr el proyecto

Requisito: tener instalado el [SDK de .NET 10](https://dotnet.microsoft.com/download).

```bash
git clone https://github.com/isaias-alt/clt-prueba.git
cd clt-prueba/src/CltPrueba.Api
dotnet run
```

La aplicación queda en **http://localhost:5205** y abre Swagger en **http://localhost:5205/swagger/index.html** (la ruta `/` redirige ahí).

También se puede abrir la solución `CltPrueba.sln` en Rider o Visual Studio y ejecutar el proyecto `CltPrueba.Api`.

## Base de datos y migraciones

La base es un archivo SQLite (`app.db`) que se crea solo, en la carpeta desde la que se ejecuta el proyecto, la primera vez que arranca la aplicación. Al iniciar se aplican automáticamente las migraciones pendientes (`Database.Migrate()`), que crean las tablas y cargan las monedas iniciales (PYG y USD). No hay que ejecutar nada más.

Las migraciones están versionadas en `src/CltPrueba.Api/Migrations`. Para crear o aplicar migraciones a mano, desde la raíz del repositorio:

```bash
dotnet tool restore
dotnet restore
dotnet ef migrations add NombreDeLaMigracion --project src/CltPrueba.Api
dotnet ef database update --project src/CltPrueba.Api
```

`dotnet tool restore` instala la versión de `dotnet-ef` fijada en `.config/dotnet-tools.json` y `dotnet restore` descarga los paquetes del proyecto (necesario antes de usar `dotnet ef` en un clon nuevo).

Para empezar con una base vacía, basta con borrar `app.db` (y los archivos `app.db-shm` y `app.db-wal`, si existen) y volver a ejecutar.

## API Key

Toda la API requiere el header `X-API-KEY`. Si falta o es incorrecto, responde **401 Unauthorized**. Quedan fuera del chequeo Swagger (`/swagger`) y la redirección de la raíz `/`.

La clave de prueba está en `src/CltPrueba.Api/appsettings.json` (clave `ApiKey`):

```
X-API-KEY: clt-prueba-dev-key
```

Ejemplo:

```bash
curl -i http://localhost:5205/users -H "X-API-KEY: clt-prueba-dev-key"
```

En Swagger, el botón **Authorize** permite ingresar la clave una vez y se envía en todas las pruebas.

## Endpoints

| Método | Ruta | Descripción | Respuestas |
|---|---|---|---|
| POST | `/users` | Crea un usuario (`name`, `email`, `password` opcional) | 201, 400, 409 (email duplicado) |
| GET | `/users` | Lista usuarios, filtro opcional `?isActive=true\|false` | 200 |
| GET | `/users/{id}` | Obtiene un usuario | 200, 404 |
| PUT | `/users/{id}` | Modifica `name`, `email` e `isActive` | 200, 400, 404, 409 |
| DELETE | `/users/{id}` | Elimina un usuario y sus direcciones | 204, 404 |
| POST | `/users/bulk` | Carga masiva de usuarios (en paralelo) | 200 con reporte, 400 |
| POST | `/users/{userId}/addresses` | Crea una dirección para un usuario | 201, 400, 404 |
| GET | `/users/{userId}/addresses` | Lista las direcciones de un usuario | 200, 404 |
| PUT | `/addresses/{id}` | Modifica una dirección | 200, 400, 404 |
| DELETE | `/addresses/{id}` | Elimina una dirección | 204, 404 |
| GET | `/currencies` | Lista las monedas | 200 |
| POST | `/currencies` | Crea una moneda (`code`, `name`, `rateToBase`) | 201, 400, 409 (código duplicado) |
| POST | `/currency/convert` | Convierte un monto entre dos monedas | 200, 400, 404 |

Todas las respuestas con error de validación son **400** con el detalle por campo. Las contraseñas se guardan hasheadas y nunca se devuelven.

### Ejemplos

Crear un usuario:

```bash
curl -i -X POST http://localhost:5205/users \
  -H "X-API-KEY: clt-prueba-dev-key" -H "Content-Type: application/json" \
  -d '{ "name": "Juan", "email": "juan@test.com" }'
```

Crear una dirección:

```bash
curl -i -X POST http://localhost:5205/users/1/addresses \
  -H "X-API-KEY: clt-prueba-dev-key" -H "Content-Type: application/json" \
  -d '{ "street": "Calle Falsa 123", "city": "Asunción", "country": "Paraguay", "zipCode": "9999" }'
```

Convertir divisas:

```bash
curl -i -X POST http://localhost:5205/currency/convert \
  -H "X-API-KEY: clt-prueba-dev-key" -H "Content-Type: application/json" \
  -d '{ "fromCurrencyCode": "USD", "toCurrencyCode": "PYG", "amount": 100 }'
```

## Carga masiva (bulk)

El archivo de prueba está en **`sample-data/bulk_users_test.json`** (los cuatro usuarios del enunciado). Desde la raíz del repositorio:

```bash
curl -i -X POST http://localhost:5205/users/bulk \
  -H "X-API-KEY: clt-prueba-dev-key" -H "Content-Type: application/json" \
  --data-binary @sample-data/bulk_users_test.json
```

La respuesta indica cuántos se crearon y cuáles fallaron, con el motivo y la posición (`index`, desde 0) de cada uno:

```json
{
  "total": 4,
  "createdCount": 4,
  "failedCount": 0,
  "created": [ { "id": 1, "name": "Ana Gómez", "email": "ana.gomez@test.com", "isActive": true } ],
  "failed": []
}
```

Si se ejecuta dos veces, la segunda vez los cuatro aparecen en `failed` porque el email ya existe. Un segundo archivo, `sample-data/bulk_users_test_with_errors.json`, mezcla usuarios válidos con casos de fallo (email inválido, nombre vacío, email repetido en el mismo arreglo, email ya existente, contraseña corta e ítem vacío) para ver el reporte de errores. Para que el caso del email ya existente falle, hay que ejecutar antes `bulk_users_test.json`.

## Monedas iniciales

La migración carga dos monedas: **PYG = 1** (base) y **USD = 5765** (cotización de referencia del Banco Central del Paraguay del 23/10/2026, redondeada a guaraníes enteros). `RateToBase` indica cuántos guaraníes vale una unidad de la moneda.

## Qué quedó implementado

- CRUD de Users, con filtro `isActive`, hash de contraseña y email único.
- CRUD de Addresses relacionadas a un User (1:N), con 404 si el usuario no existe.
- Currencies: listado, alta con validación y conversión de divisas.
- Seguridad por API Key (`X-API-KEY`) con respuesta 401.
- Entity Framework Core con SQLite, migraciones versionadas aplicadas al iniciar y `IDbContextFactory<AppDbContext>` en lugar de un `DbContext` inyectado.
- FluentValidation para crear/editar usuario, crear/editar dirección, crear moneda, conversión y carga masiva.
- CQRS con commands y queries (un handler por cada uno).
- Procesamiento en paralelo con `Task.WhenAll`: la conversión de divisas obtiene ambas monedas a la vez y la carga masiva procesa los usuarios en paralelo, cada operación con su propio `DbContext`.
- Endpoint de carga masiva `POST /users/bulk` con JSON de prueba.
- Swagger con botón Authorize.

## Qué no está implementado

- Pruebas automatizadas.
- Autenticación de usuarios (login): la contraseña se guarda hasheada, pero no hay endpoint de acceso.
- `PUT`/`DELETE` de monedas ni `GET` por id de monedas y direcciones (no figuran en el enunciado).

## Notas

- **Ejemplo de conversión del enunciado:** el enunciado muestra `100 USD` => `730000 PYG`, que corresponde a un USD = 7300. Con la cotización cargada (USD = 5765) la misma petición devuelve `576500`. Para reproducir el ejemplo exacto, basta con cargar el USD con 7300.
- **SQLite y escrituras:** SQLite admite una sola escritura a la vez, por lo que en la carga masiva las escrituras se serializan, el paralelismo y el límite de concurrencia (`Bulk:MaxDegreeOfParallelism` en `appsettings.json`) están pensados para un motor de base de datos que soporte escrituras concurrentes, como PostgreSQL o SQL Server.

## Estructura del proyecto

```
CltPrueba.sln
.config/dotnet-tools.json        herramienta dotnet-ef
sample-data/                     JSON de prueba para el bulk
src/CltPrueba.Api/
  Api/                           endpoints (Minimal API)
  Application/
    Users/{Commands,Queries,Dto}
    Addresses/{Commands,Queries,Dto}
    Currencies/{Commands,Queries,Dto}
    CurrencyConversion/
    Common/                      Result<T>
  Domain/                        entidades User, Address, Currency
  Infrastructure/                AppDbContext y middleware de API Key
  Migrations/                    migraciones de EF Core
  Program.cs
```
