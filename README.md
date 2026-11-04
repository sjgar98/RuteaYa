# RuteaYa

Plataforma de transporte colaborativo (TP integrador PW3 2026-2C). Un conductor registra sus **viajes** habituales y
el sistema le ofrece **envíos** publicados por otros usuarios que le quedan de paso. Un **optimizador** elige la
combinación de envíos que más le conviene, respetando la capacidad, el peso y el desvío máximo del viaje. Un mismo
usuario puede ser conductor y remitente.

- **Stack:** .NET 10, ASP.NET Core MVC (Razor + Bootstrap 5) y Web API, EF Core 10 + SQL Server, Swagger y xUnit.
- **Mapas y rutas:** Leaflet + OpenStreetMap, OSRM y Photon. No hace falta ninguna API key.

## Arquitectura

```text
RuteaYa.MVC ──HTTP/JSON──▶ RuteaYa.API ──▶ RuteaYa.Repositorios ──▶ RuteaYa.Entidades (EF Core / SQL Server)
     └──────────────── RuteaYa.DTOs (contratos compartidos) ◀─────────────┘
```

- **`RuteaYa.MVC`**: la interfaz web. Solo conoce los DTOs y le pide todo a la API.
- **`RuteaYa.API`**: controllers finos y la lógica de negocio en `Servicios/`. Autenticación por JWT.
- **`RuteaYa.Repositorios`**: las consultas EF.
- **`RuteaYa.Entidades`**: entidades, `DbContext`, migraciones y datos iniciales.
- **`RuteaYa.Tests`**: tests unitarios de los servicios, con repositorios falsos en memoria.

## Requisitos

- [.NET 10 SDK](https://dotnet.microsoft.com/download).
- SQL Server, de una de estas dos formas:
  - **Windows:** LocalDB (viene con Visual Studio). No hay que configurar nada.
  - **Linux / macOS:** Docker, para levantar SQL Server en un contenedor.
- Opcional, para manejar migraciones a mano: `dotnet tool install --global dotnet-ef`.

## Cómo correrlo desde cero

### 1. Base de datos

**Windows con LocalDB:** no hay que hacer nada. Si no está definida la variable `RUTEAYA_DB_HOST`, la aplicación usa
`(localdb)\MSSQLLocalDB` con la base `RuteaYa`.

**Linux / macOS con Docker:**

1. Copiá el archivo de ejemplo de variables:

   ```bash
   cp .env.example .env
   ```

2. Editá `.env` y poné una contraseña en `RUTEAYA_DB_PASSWORD`. Tiene que cumplir la política de SQL Server: al menos
   8 caracteres, con mayúsculas, minúsculas, números y símbolos.

3. Levantá el contenedor:

   ```bash
   docker compose --env-file .env up -d
   ```

4. Exportá las variables en la terminal donde vas a correr la API:

   ```bash
   set -a; . ./.env; set +a
   ```

   En VS Code no hace falta: `.vscode/launch.json` las lee del `.env`.

### 2. API

```bash
dotnet run --project RuteaYa.API --launch-profile http
```

Al arrancar, la API aplica todas las migraciones, incluidos los datos de prueba. No hace falta correr
`dotnet ef database update`.

- API: <http://localhost:5125>
- Swagger: <http://localhost:5125/swagger>. Para probar los endpoints:
  1. Hacé login con `POST api/usuarios/login`.
  2. Copiá el `token` de la respuesta.
  3. Pegalo en **Authorize**.

### 3. Aplicación web (en otra terminal)

```bash
dotnet run --project RuteaYa.MVC --launch-profile http
```

Abrí <http://localhost:5169>. El MVC llama a la API usando el `ApiUrl` de `RuteaYa.MVC/appsettings.Development.json`
(`http://localhost:5125/`). Si corrés la API con otro perfil, ajustá ese valor.

En VS Code, la configuración compuesta **"MVC + API"** levanta las dos aplicaciones juntas.

## Usuarios y datos de prueba

La migración `SeedDatosDePrueba` carga dos usuarios. Los dos usan la contraseña `Prueba123`.

| Usuario | Email | Qué tiene |
|---|---|---|
| Usuario Prueba (conductor) | `prueba@ruteaya.com` | **V1** Belgrano → Lomas de Zamora (Lun/Mié/Vie, 3 paquetes, 10 kg, 5 km): activo y sin envíos.<br>**V2** Palermo → San Isidro (Vie): lleno, con un envío retirado y otro en camino.<br>**V3** Avellaneda → Palermo: inactivo, con 3 envíos entregados en septiembre y octubre de 2026.<br>Además, envíos propios: uno entregado, uno con conductor asignado, uno buscando conductor y uno cancelado. |
| María Gómez (remitente) | `remitente@ruteaya.com` | Publica los envíos que se le ofrecen al conductor: 6 buscando conductor, 2 en curso, 3 entregados y 1 cancelado.<br>Maneja **V4** Recoleta → Quilmes (Sáb), que lleva envíos del conductor. |

Qué se puede probar con cada usuario:

- **Con `prueba@ruteaya.com`:**
  - **Oportunidades / Optimizador de V1:** los envíos 1 a 5 de María reproducen el ejemplo de la sección 12 del
    enunciado. La combinación recomendada es Caballito → Lomas + Palermo → Lomas + Barracas → Lomas: **$12.000 y 9 kg**.
    Plaza Italia → San Isidro queda descartado por desvío. Desde ahí, **Revisar y reservar** abre la Confirmación.
  - **V2:** Oportunidades avisa que el viaje está lleno, y Editar / Desactivar están bloqueados porque tiene envíos
    pendientes.
  - **Mis transportes:** se pueden marcar retirado, en camino y entregado.
  - **Historial:** viajes realizados, envíos propios entregados y cancelados, con filtros por fecha y por estado.
  - **Potencial mensual** y **Dashboard**.
- **Con `remitente@ruteaya.com`:** Mis envíos en todos los estados (se puede cancelar los que buscan conductor) y
  Mis transportes de V4.

Los desvíos se calculan con el servicio de rutas que esté activo, así que los km pueden variar un poco respecto
del enunciado. Las fechas límite de los envíos disponibles son hasta el 30/06/2027.

## Configuración opcional

Todo funciona sin configuración extra. Estas claves están en `RuteaYa.API/appsettings.json`:

| Clave | Valores | Default |
|---|---|---|
| `Rutas:Proveedor` | `Auto` (Google si hay key, si no OSRM), `Osrm`, `Google`, `Mock` | `Auto` |
| `Rutas:RespaldoAlMock` | si el proveedor falla, responde con valores simulados | `true` |
| `Direcciones:Proveedor` | `Photon` (OpenStreetMap) o `Fijo` (lista de 12 barrios) | `Photon` |
| `Direcciones:RespaldoAlFijo` | si Photon falla, usa la lista fija | `true` |

Para usar Google Routes en lugar de OSRM (opcional):

```bash
dotnet user-secrets set "Rutas:GoogleApiKey" "<clave>" --project RuteaYa.API
```

Los user-secrets solo se cargan en Development.

## Tests

```bash
dotnet test RuteaYa.slnx
```

Cubren la lógica de cada endpoint: viajes, envíos, oportunidades, confirmación de varios envíos, optimizador,
transportes, historial, potencial, dashboard, usuarios, rutas, direcciones y validaciones de los DTOs. Usan
repositorios falsos: no necesitan base de datos.

## Migraciones

El `DbContext` vive en `RuteaYa.Entidades` y toma la conexión de las variables `RUTEAYA_DB_*`. Por eso hay que
exportarlas antes de correr estos comandos:

```bash
dotnet ef migrations add <Nombre> --project RuteaYa.Entidades
dotnet ef database update --project RuteaYa.Entidades
```

## Problemas frecuentes

- **La API no arranca y muestra un error de conexión a SQL Server:**
  - Revisá que el contenedor esté levantado (`docker compose ps`).
  - Revisá que las variables `RUTEAYA_DB_*` estén exportadas en esa terminal.
  - La primera vez, SQL Server tarda unos segundos en aceptar conexiones.
- **`SocketException ... localhost:XXXX` en el MVC:** la API no está corriendo, o `ApiUrl` apunta a otro puerto.
  Ojo: los user-secrets del MVC pisan a `appsettings*.json`.
- **La primera carga de Dashboard u Oportunidades tarda unos segundos:** el servidor público de OSRM admite 1
  consulta por segundo. Después queda en caché por 24 horas, hasta reiniciar la API. Antes de una demo conviene
  abrirlas una vez.
- **`MSB3027` (archivo bloqueado) al compilar:** la API o el MVC siguen corriendo. Detenelos y volvé a compilar.
- **Ya tenías una base local con viajes o envíos cargados a mano:** la migración de datos de prueba inserta registros
  con Ids fijos (del 1 en adelante) y choca con los existentes. En ese caso, borrá la base
  (`dotnet ef database drop --project RuteaYa.Entidades`) y dejá que la API la vuelva a crear.
