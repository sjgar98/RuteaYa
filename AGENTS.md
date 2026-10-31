# RuteaYa — contexto para agentes

TP integrador PW3 2026-2C. Plataforma de transporte colaborativo: un conductor registra **viajes**
habituales (recorridos recurrentes por días de la semana) y el sistema le ofrece **envíos** (paquetes
publicados por otros usuarios) compatibles con su ruta; un **optimizador** elige la combinación de
envíos de mayor ganancia respetando capacidad, peso y desvío. Un mismo usuario puede ser conductor y
remitente (no hay roles).

Este archivo resume el enunciado y el estado del código. **Mantenerlo actualizado** al implementar,
renombrar o eliminar funcionalidades (sobre todo la sección 5, "Estado por funcionalidad").
Es la única fuente de contexto para agentes: `CLAUDE.md` solo lo importa (`@AGENTS.md`), así que
los cambios se hacen acá y no en `CLAUDE.md`.

**Enunciado completo:** `docs/enunciado.md` (transcripción del PDF de la cátedra con definiciones,
reglas y requisitos por pantalla). **Mockups:** `docs/mockups/Pantalla N - <Nombre>.png`, referenciados
desde cada pantalla del enunciado (cada uno con una descripción en texto de lo que muestra). Este archivo
es un resumen: ante dudas sobre el alcance, consultar el enunciado.

---

## 1. Stack y comandos

- .NET 10 (`net10.0`), ASP.NET Core MVC (Razor + Bootstrap 5 + jQuery Validation) y Web API,
  EF Core 10.0.13 + SQL Server, Swashbuckle 10, xUnit. Solución: `RuteaYa.slnx`.
- Puertos (perfil `http`): API `http://localhost:5125` (Swagger en `/swagger`), MVC `http://localhost:5169`.
  El MVC llama a la API usando `ApiUrl` de `RuteaYa.MVC/appsettings*.json`.

```bash
dotnet build RuteaYa.slnx
dotnet run --project RuteaYa.API      # aplica migraciones al arrancar (Program.cs: db.Database.Migrate())
dotnet run --project RuteaYa.MVC
dotnet test RuteaYa.slnx
# Migraciones: el DbContext vive en RuteaYa.Entidades y se configura solo (OnConfiguring)
dotnet ef migrations add <Nombre> --project RuteaYa.Entidades
dotnet ef database update --project RuteaYa.Entidades
# SQL Server en Linux (no hay LocalDB): copiar .env.example a .env y levantar
docker compose --env-file .env up -d
```

- Conexión a BD (`RuteaYaDbContext.ObtenerConexion`): sin `RUTEAYA_DB_HOST` usa LocalDB
  `(localdb)\MSSQLLocalDB/RuteaYa`; con ella usa `RUTEAYA_DB_HOST/PORT/NAME/USER/PASSWORD`.
  **No** hay connection string en appsettings. En Linux hay que exportar las variables de `.env`
  (VS Code lo hace con `envFile` en `.vscode/launch.json`; compound "MVC + API" levanta ambos).
- Claves de Google (opcionales, por `dotnet user-secrets`; ver 6.2): 
`GoogleMaps:ApiKey` en el MVC (mapa) y `Rutas:GoogleApiKey` en la API (distancias). 
Sin claves todo funciona: el mapa se reemplaza por un aviso y las rutas usan el mock.
- CI (`.github/workflows/ci.yml`, en PRs a `main`): restore → build Release → `dotnet ef database update`
  contra SQL Server en contenedor → `dotnet test`.
- Commits: Conventional Commits en español (`feat:`, `fix:`, `chore:`), ramas + PR a `main`.

## 2. Arquitectura

```text
RuteaYa.MVC ──HTTP(JSON)──▶ RuteaYa.API ──▶ RuteaYa.Repositorios ──▶ RuteaYa.Entidades (EF/DbContext)
     │                          │                                          │
     └──────────── RuteaYa.DTOs ◀──────────────────────────────────────────┘ (enums compartidos)
```

| Proyecto | Contenido | Referencia a |
|---|---|---|
| `RuteaYa.DTOs` | DTOs de la API, `Enums/` (`DiaSemana`, `EstadoEnvio`, `TamanoEnvio`), `Validaciones/` (atributos custom) | — |
| `RuteaYa.Entidades` | Entidades EF, `RuteaYaDbContext`, `Migrations/`, `UsuarioPrueba` | DTOs (por los enums) |
| `RuteaYa.Repositorios` | `Interfaces/I*Repositorio` + implementación; único lugar con queries EF | Entidades |
| `RuteaYa.API` | `Controllers/` (finos) + `Servicios/` (lógica de negocio, mapeo entidad→DTO) | Entidades, Repositorios, DTOs |
| `RuteaYa.MVC` | `Controllers/`, `Views/`, `Models/` (ViewModels), `Servicios/` (clientes HTTP de la API), `wwwroot/js` | **solo** DTOs |
| `RuteaYa.Tests` | xUnit; servicios de la API con repositorios falsos en memoria (sin librería de mocks), p. ej. `ViajeServicioTests`, `EnvioServicioTests`, `TransporteServicioTests` | Entidades, Repositorios, API, MVC |

Reglas duras del enunciado:
- El MVC **nunca** referencia Entidades/Repositorios ni toca el DbContext: todo pasa por la API.
- La lógica de negocio vive solo en `RuteaYa.API/Servicios`; no duplicarla en el MVC (el MVC solo
  replica validaciones de formulario en JS para UX).
- Controllers → Services → Repositorios. DTOs en todas las comunicaciones de la API.
- Códigos HTTP correctos (200, 201, 400, 401, 403/404, 409), Swagger documentado (comentarios `///`
  y `[ProducesResponseType]`; el csproj de la API genera el XML), async/await en I/O.

### Flujo típico (ej. "Nuevo viaje")
1. `MVC/Views/Viajes/Nuevo.cshtml` → partial `_FormularioViaje.cshtml` (compartido con `Editar`) + `wwwroot/js/formulario-viaje.js`
   (autocompletado, paradas, validaciones cliente).
2. `MVC/Controllers/ViajesController.Nuevo [POST]` → valida `ModelState` → `MVC/Servicios/ViajeServicio.CrearAsync`.
3. `POST api/viajes` → `API/Controllers/ViajesController.Crear` (`[ApiController]` devuelve 400
   `ValidationProblemDetails` automáticamente) → `API/Servicios/ViajeServicio.CrearAsync` → `ViajeRepositorio.AgregarAsync`.
4. Si la API responde 400, el servicio MVC devuelve `Dictionary<campo, mensajes[]>` y el controller
   los carga en `ModelState` (`AgregarErrores`). Éxito → `TempData["MensajeExito"]` + redirect; el toast lo
   muestra `_Layout.cshtml` en cualquier pantalla (`site.js` lo abre e inicializa los tooltips `data-bs-toggle="tooltip"`).

## 3. Convenciones de código (respetarlas)

- Todo en **español** (clases, métodos, variables, mensajes, comentarios), sin tildes ni ñ en
  identificadores (`TamanoEnvio.Pequeno`, `Miercoles`). Sufijos: `*Servicio`, `*Repositorio`, `*Dto`,
  `*ViewModel`, `*Attribute`; métodos async terminan en `Async` (`ListarPorUsuarioAsync`, `ObtenerPorIdAsync`,
  `AgregarAsync`, `GuardarCambiosAsync`, `CrearAsync`).
- Namespaces con llaves (no file-scoped), campos `_camelCase` inyectados por constructor,
  colecciones inicializadas con `new List<T>()`, strings con `= string.Empty`, navegaciones `= null!`.
- Comentarios breves y solo cuando explican un *por qué*. Pendientes con formato
  `// TODO <Área>: <qué falta>` (áreas actuales: `Servicio de rutas`,
  `Servicio de mapas`, `Paquetes asignados`). Buscar con `grep -rn "TODO" --include=*.cs --include=*.cshtml`.
- Validaciones de entrada: Data Annotations en el DTO con `ErrorMessage` en español (voseo:
  "Seleccioná…"). Validaciones entre campos con atributos de `RuteaYa.DTOs/Validaciones`:
  `[DistintoDe(nameof(X))]`, `[MayorQue(nameof(X))]`, `[DireccionesUnicas(nameof(A), nameof(B))]`,
  `[FechaNoAnteriorAHoy]`.
  Las reglas que dependen de BD (dueño, estado, disponibilidad) van en el servicio de la API.
  En el cliente, las mismas reglas se replican con `$.validator.addMethod` (ver final de
  `formulario-viaje.js` y `formulario-envio.js`).
- Propiedades numéricas obligatorias de DTOs de entrada son nullable (`int?`, `decimal?`, `TimeOnly?`,
  `DateOnly?`) + `[Required]`, y el servicio usa `.Value`.
- Mapeo entidad→DTO: método `private static XDto ADto(X entidad)` dentro del servicio de la API.
- Repositorios: lecturas de listado con `AsNoTracking()`; `ObtenerPorIdAsync` con tracking e `Include`s
  para que el servicio modifique y llame `GuardarCambiosAsync()`.
- Servicios MVC: `HttpClient` tipado registrado con `AddHttpClient<IXServicio, XServicio>(...).AddHttpMessageHandler<TokenHandler>()`;
  rutas relativas `"api/..."`, `GetFromJsonAsync` / `PostAsJsonAsync`.
- Vistas: Bootstrap (cards `border-0 shadow-sm rounded-4`, botones `btn-dark` / `btn-outline-secondary`,
  filtros como pills `rounded-pill`), JS por pantalla en `wwwroot/js/<pantalla>.js` (IIFE, vanilla + jQuery
  solo para validación), CSS por pantalla en `wwwroot/css/site.css` bajo comentario `/* <Pantalla> */`.
- Decimales: convención global `decimal(18,6)` (coordenadas). En formularios, las coordenadas se envían
  con punto y un hidden `__Invariant` con el nombre del campo para que el model binder las lea invariante.
- Enums `: byte` con valores desde 1. `DiaSemana` 1=Lunes…7=Domingo; usar `DateOnly.ADiaSemana()` /
  `DayOfWeek.ADiaSemana()` (no castear `DayOfWeek` directo, Domingo es 0 ahí).
- Textos/formatos repetidos en vistas: extensiones de `MVC/Helpers/EtiquetasHelper.cs` (importado en
  `_ViewImports`): `DiaSemana.Abreviar()` ("Mié"), `EstadoEnvio.Texto()`, `TamanoEnvio.Texto()`,
  `EstadoEnvio.ClaseBadge()` (clases Bootstrap del badge de estado de un envío), `decimal.Moneda()`
  ("$4.000"), `decimal.Numero()` ("3,5"), `DateTime.AHoraLocal()` (convierte un `DateTime` guardado en
  UTC a hora de Argentina, offset fijo -3, antes de mostrarlo — ver 6.3), con cultura es-AR.
- Recursos ajenos: el servicio de la API devuelve `null` si no existe **o** no es del usuario, y el
  controller responde 404 (ver `ViajeServicio.ObtenerDetalleAsync`, `EnvioServicio.ObtenerDetalleAsync`).
- Máquinas de estado (transiciones válidas de un enum de estado): un único lugar estático, nunca
  reimplementar la regla en cada servicio que dispara la transición (ver 4.1, `EnvioEstadoMaquina`).

## 4. Modelo de datos (`RuteaYa.Entidades`)

| Entidad | Campos clave | Relaciones / restricciones |
|---|---|---|
| `Usuario` | Nombre, Apellido, Email, PasswordHash, CreadoEn | Email único. Seed: `UsuarioPrueba` (Id 1, prueba@ruteaya.com / `Prueba123`, hash fijo en `UsuarioPrueba.PasswordHash`) |
| `Viaje` | Origen/Destino (Dir, Lat, Lng), HoraSalida/HoraLlegada (`TimeOnly`), MaxPaquetes, PesoMaxKg, DesvioMaxKm, DistanciaKm, DuracionMin, Activo | N:1 Usuario; 1:N Paradas, Dias, Asignaciones |
| `ViajeDia` | ViajeId, DiaSemana | PK compuesta (ViajeId, DiaSemana) |
| `Parada` | ViajeId, Orden (1..n), Direccion, Lat, Lng | Orden = orden del recorrido |
| `Envio` | Origen/Destino (Dir, Lat, Lng), FechaLimite (`DateOnly`), PesoKg, Tamano, Descripcion, Monto, Estado (default BuscandoConductor), CreadoEn | N:1 Usuario (remitente); 1:0..1 Asignacion; 1:N Historial |
| `Asignacion` | EnvioId, ViajeId, FechaViaje, DesvioKm, TiempoExtraMin, Ganancia, AsignadoEn | **Índice único EnvioId** (un envío se asigna una sola vez → `DbUpdateException` si dos conductores lo toman: mapear a 409). FK a Viaje con `Restrict` (evita doble cascada) |
| `EnvioEstadoHistorial` | EnvioId, EstadoAnterior?, EstadoNuevo, Fecha | Registrar cada transición de estado |

Migraciones: `20261025012306_Inicial`, `20261025175519_SeedUsuarioPrueba`, `20261025200751_HashUsuarioPrueba`. El seed usa `HasData`
con valores fijos. El enunciado exige **datos iniciales suficientes** para probar todo (falta
sembrar viajes/envíos de ejemplo, p. ej. los de la sección 8.4).

Repositorios existentes (métodos ya disponibles, muchos aún sin uso):
- `IUsuarioRepositorio`: `ObtenerPorIdAsync`, `ObtenerPorEmailAsync`, `ExisteEmailAsync`, `AgregarAsync`.
- `IViajeRepositorio`: `ListarPorUsuarioAsync(usuarioId, activo?)` (con Dias y Asignaciones+Envio, para `TieneEnviosPendientes`), `ObtenerPorIdAsync` (con Dias, Paradas ordenadas, Asignaciones+Envio), `AgregarAsync`, `GuardarCambiosAsync`.
- `IEnvioRepositorio`: `ListarPorUsuarioAsync(usuarioId, estado?)`, `ListarDisponiblesAsync(desde, usuarioIdExcluido)` (BuscandoConductor, no vencidos, de otros usuarios), `ListarPorIdsAsync`, `ObtenerPorIdAsync` (con Asignacion+Viaje e Historial), `AgregarAsync`, `GuardarCambiosAsync`.
- `IAsignacionRepositorio`: `ListarPorConductorAsync` (filtra por `Viaje.UsuarioId`, con `Envio` y `Viaje` incluidos, `AsNoTracking`), `ObtenerPorIdAsync` (con tracking, para cambiar estado), `AgregarAsync(List<Asignacion>)` (una o varias en un solo `SaveChanges`), `GuardarCambiosAsync`.

Todos registrados como `Scoped` en `RuteaYa.API/Program.cs`. Servicios de la API: `IViajeServicio`, `IUsuarioServicio`, `ITokenServicio`, `IRutaServicio` (`RutaServicioGoogle` o `RutaServicioMock`, según `Rutas:Proveedor`; Google va envuelto en `RutaServicioConRespaldo` si `Rutas:RespaldoAlMock` es `true`), `IHistorialServicio`, `IEnvioServicio`, `ITransporteServicio`, `IOportunidadesServicio`.

### 4.1 Máquina de estados de envíos
`RuteaYa.API/Servicios/EnvioEstadoMaquina.cs` es la **única fuente de verdad** para las transiciones
válidas de `EstadoEnvio` (sección 7.2 del enunciado). Es una clase estática (no se registra en DI,
no depende de EF ni de HTTP) que expone:
- `PuedeTransicionar(actual, nuevo)`: consulta sin modificar nada (se usa para calcular flags en los
  DTOs, p. ej. `EnvioDto.PuedeCancelar`, `TransporteDto.PuedeMarcarRetirado/EnCamino/Entregado`).
- `Transicionar(envio, nuevo)`: si la transición es válida, cambia `envio.Estado` y agrega la entrada
  correspondiente a `envio.Historial`; devuelve `false` sin modificar nada si no lo es.

La usan `EnvioServicio.CancelarAsync` (→ `Cancelado`) y `TransporteServicio` (`MarcarRetiradoAsync` /
`MarcarEnCaminoAsync` / `MarcarEntregadoAsync`, con el `Envio` llegando vía `Asignacion.Envio`).
`OportunidadesServicio.AceptarAsync` la usa para `BuscandoConductor → ConductorAsignado`.
Cualquier transición de estado de envío nueva debe agregarse a la tabla de `EnvioEstadoMaquina`,
no reimplementarse en el servicio que la dispara.

### 4.2 Motor de compatibilidad (Oportunidades)
`OportunidadesServicio.EvaluarAsync(viaje, envio, ...)` aplica las reglas en este orden y devuelve el
primer motivo por el que el envío no entra (o ninguno), la `FechaViaje` y la comparación de rutas:
1. Viaje activo y del usuario (si no es del usuario, el servicio devuelve null / `NoEncontrado` → 404).
2. Envío de otro usuario, en `BuscandoConductor` y sin `Asignacion`; `FechaLimite` ≥ hoy.
3. **Fecha:** primera fecha entre hoy y `FechaLimite` que cae en un día del viaje **y** tiene lugar.
4. **Capacidad por salida:** solo cuentan las asignaciones del viaje con esa `FechaViaje` cuyo envío no
   está `Cancelado` (las de otras fechas no ocupan lugar). Paquetes y kg.
5. **Desvío** (`IRutaServicio.CompararConEnvioAsync`) ≤ `DesvioMaxKm`. En el listado hay un prefiltro en línea
   recta (hasta 1,5 × el máximo) para no consultar rutas de más. Si la ruta falla: `LogWarning` y se descarta.
- Compatibilidad = desvío / desvío máximo: ≤ 0,3 "Muy compatible", ≤ 0,5 "Compatible", si no "Mucho desvío";
  `PuntuacionCompatibilidad` = 1 − esa proporción.
- "Hoy" es la fecha de Argentina (UTC − 3), igual que en las vistas.
- `AceptarAsync` vuelve a evaluar todo (las restricciones pueden haber cambiado) y devuelve
  `ResultadoAsignacion` (`ResultadoOperacion` + mensaje para el 409 + `AsignacionDto`).
- Para el Optimizador / Potencial: reusar `EvaluarAsync` (hoy es privado; exponerlo cuando se necesite)
  en vez de reimplementar las reglas.

## 5. Estado por funcionalidad

Leyenda: ✅ hecho · 🟡 parcial/mock · ❌ no existe. Rutas de API sugeridas para lo que falta (mantener REST coherente).

| # | Pantalla / recurso | API | MVC | Estado |
|---|---|---|---|---|
| 1 | Login / Registro / Logout | ✅ `POST api/usuarios/registro` (201 / 400 / 409), `POST api/usuarios/login` (200 con JWT / 401) | ✅ `CuentaController` (Login, Registro, Logout) con `_LayoutCuenta`, cookie con claim `"token"` | ✅ (ver 6.1) |
| 2 | Dashboard | ❌ (resumen: próximos viajes, envíos pendientes, nº oportunidades, potencial del mes) | 🟡 `HomeController.Index` es placeholder | ❌ |
| 3 | Mis viajes | ✅ `GET api/viajes?activo=` | ✅ `Viajes/Index` (filtros Todos/Activos/Inactivos, toast) | 🟡 Ver detalle ✅; Activar/Desactivar ✅ (Desactivar deshabilitado con tooltip si `ViajeDto.TieneEnviosPendientes`; vuelve a la lista con el filtro vía `returnUrl`); botón Oportunidades enlazado a `OportunidadesController.Index` |
| 4 | Nuevo viaje | ✅ `POST api/viajes`, ✅ `GET api/viajes/direcciones?texto=` (lista fija) | ✅ `Viajes/Nuevo` + `_FormularioViaje` + `formulario-viaje.js` (paradas: agregar/quitar/reordenar) | 🟡 Distancia/duración calculadas con `IRutaServicio` (Google si hay key, si no mock; ver 6.2), direcciones de lista fija. Vista previa de la ruta con Google Maps (partial `_MapaRuta` + `mapa-viaje.js`, botón Actualizar ruta) |
| 5 | Detalle / Editar / Activar / Desactivar viaje | ✅ `GET api/viajes/{id}` (`ViajeDetalleDto`: coordenadas, paradas, paquetes asignados; `TieneEnviosPendientes` vive en `ViajeDto`, también en el listado; 404 si no es propio) · ✅ `PUT api/viajes/{id}` (body `CrearViajeDto`; 204 / 400 / 404 / **409 si tiene envíos ConductorAsignado/Retirado/EnCamino**; reemplaza paradas, días por diferencia) · ✅ `POST api/viajes/{id}/desactivar` (204 / 404 / **409 con envíos pendientes**) y `POST api/viajes/{id}/activar` (204 / 404), ambos con `ViajeServicio.CambiarActivoAsync` | ✅ `Viajes/Detalle` (Editar deshabilitado con tooltip si hay envíos pendientes) · ✅ `Viajes/Editar` (mismo form que Nuevo, prepoblado, con vista previa de ruta; GET con pendientes redirige a Detalle) · ✅ `Viajes/Desactivar` y `Viajes/Activar [POST]` (409 → Detalle con alerta `ViewData["Error"]`; Desactivar deshabilitado con tooltip si hay envíos pendientes) | 🟡 Detalle y Editar hechos; Editar y Detalle con mapa de la ruta (Google Maps, partial `_MapaRuta`, ver 6.2); paquetes asignados **mock** (`PaquetesDePrueba()`, `TODO Paquetes asignados` — ahora que Envíos/Asignaciones son reales, falta reemplazar el mock por `viaje.Asignaciones`); Activar/Desactivar ✅ |
| — | Paradas dentro de un viaje | 🟡 vía `PUT api/viajes/{id}` (lista completa en orden) · ❌ endpoints propios `api/viajes/{id}/paradas` | se editan en el form del viaje | 🟡 |
| 6 | Nuevo envío | ✅ `POST api/envios` | ✅ `Envios/Nuevo` + form + `formulario-envio.js` (autocompletado origen/destino reusando `Viajes/BuscarDirecciones`, tamaño como pills) | ✅ |
| 7 | Mis envíos (+ detalle, cancelar) | ✅ `GET api/envios?estado=`, ✅ `GET api/envios/{id}` (con viaje asociado e historial), ✅ `POST api/envios/{id}/cancelar` (204 / 404 / 409) | ✅ `Envios/Index` (filtros por estado, `PuedeCancelar` calculado), `Envios/Detalle` (viaje asociado, historial de estados con hora local) | ✅ |
| 8 | Oportunidades | ✅ `GET api/oportunidades/{viajeId}?ordenarPor=` (`ganancia`, `desvio` o `compatibilidad`) → `OportunidadesViajeDto` (datos del viaje + lista); 404 si el viaje no es propio | ✅ `OportunidadesController.Index(viajeId?, ordenarPor)` (sin viajeId toma el primer viaje activo; selector de viaje + `oportunidades.js`, orden en pills, Aceptar desde la lista) | ✅ Motor de compatibilidad en `OportunidadesServicio.EvaluarAsync` (ver 4.2). Empates: ganancia desc → desvío asc |
| 9 | Detalle de oportunidad | ✅ `GET api/oportunidades/{viajeId}/detalle?envioId=` (404 si el viaje no es propio o el envío no existe, es propio o ya no busca conductor) | ✅ `OportunidadesController.Detalle` + vista con tarjeta "Pago"; si la API da 404 vuelve al listado con "La oportunidad ya no está disponible" | ✅ Ruta habitual vs. con el envío, desvío, tiempos, pago, ganancia por km, capacidad restante (después de aceptar) y fecha del viaje. Si una restricción dejó de cumplirse: `PuedeAceptar = false` + `Motivo`, botón deshabilitado |
| 10 | Optimizador | ❌ `GET api/viajes/{id}/optimizacion` (o `POST`) | ❌ | ❌ |
| 11 | Confirmación / asignación | 🟡 `POST api/oportunidades/{viajeId}/aceptar` (body `AceptarOportunidadDto`) → 201 `AsignacionDto` / 404 / 409 `ProblemDetails` con el motivo en `title` | 🟡 `OportunidadesController.Aceptar [POST]` desde la lista o el Detalle (409 → alerta con el motivo; éxito → toast) | 🟡 Aceptar un envío revalida todo, crea la `Asignacion` (con `FechaViaje`) y pasa el envío a `ConductorAsignado` en un solo `SaveChanges`; `DbUpdateException` (índice único) → 409. Falta: `api/asignaciones` multi-envío (para el Optimizador) y la pantalla de Confirmación |
| 12 | Mis transportes | ✅ `GET api/transportes`, ✅ `POST api/transportes/{asignacionId}/retirado`, `/en-camino`, `/entregado` (204 / 404 / 409) | ✅ `TransportesController.Index` (acciones condicionadas por `PuedeMarcarRetirado/EnCamino/Entregado`) | ✅ |
| 13 | Historial | ✅ `GET api/historial?desde=&hasta=&orden=&estado=` (viajes con envios entregados, orden por fecha/monto) | 🟡 `HistorialController.Index` + `Views/Historial/Index` (KPIs, filtros por fecha/orden, link a detalle de viaje) | 🟡 Falta cubrir estados/tipos adicionales y tests unitarios del servicio |
| 14 | Potencial mensual (req. para 10) | ❌ `GET api/potencial?mes=yyyy-MM&criterio=` (`ganancia`, `desvio` o `equilibrio`) | ❌ | ❌ |
| — | Servicio de rutas/mapas | ✅ `IRutaServicio` con coordenadas (`CalcularRutaAsync`, `CompararConEnvioAsync`) + `RutaServicioGoogle` (Routes API, caché) + `RutaServicioMock`, elegidas por `Rutas:Proveedor`. `RutasController` conserva endpoints de texto (legado) . RutaServicioConRespaldo (decorador): si Google falla responde con el mock y loguea un warning | — | 🟡 Google funcionando y mock sin key o como respaldo si Google falla; lo usan `ViajeServicio` (distancia/duración) y `OportunidadesServicio` (desvío). Falta: borrar los métodos de texto (usan Geocoding, que exige facturación), test de `RutasOpciones.UsarGoogle()` y direcciones reales (hoy lista fija) |
| — | Máquina de estados de envío | ✅ `EnvioEstadoMaquina` (estática, `API/Servicios/`): única fuente de verdad de las transiciones de `EstadoEnvio` (ver 4.1) | — | ✅ Usada por Cancelar (Pantalla 7), aceptar oportunidad (`→ ConductorAsignado`, Pantallas 8-9) y retirado/en camino/entregado (Pantalla 12) |
| — | Layout con sidebar | — | ✅ `_Layout.cshtml` + `_MenuLateral.cshtml` (secciones → `/<Controller>`, activa según el controller actual) + `_MenuUsuario.cshtml` (nombre, email, Cerrar sesión). En celulares el menú es un offcanvas | ✅ Controllers esperados: `Envios` ✅, `Oportunidades` ✅, `Optimizador` ❌, `Transportes` ✅, `Historial` ✅, `Potencial` ❌ (acción `Index`); si se usa otro nombre, cambiarlo en `_MenuLateral` |
| — | Tests unitarios por endpoint | — | — | 🟡 `UsuarioServicio`, `ViajeServicio.ObtenerDetalleAsync`, `CrearAsync` (cálculo de ruta) y `ActualizarAsync`; `EnvioServicioTests` (listar, detalle, cancelar con todas las transiciones de `EnvioEstadoMaquina`, crear) y `TransporteServicioTests` (listar, marcar retirado/en camino/entregado con cobertura de "sin saltear estados") completos. `OportunidadesServicioTests` (propietario, viaje inactivo, envíos propios/no disponibles, fecha/día, capacidad y peso por fecha de salida, desvío, etiquetas, orden y desempate, falla de ruta, detalle con y sin restricciones, aceptar → `ConductorAsignado` + historial, conflictos e índice único); rutas: `RutasTests` (inserción del recorrido, `PolylineCodec`, mock y `GeoUtil`), `RutaServicioGoogleTests` (parseo, caché, errores, comparación y geocoding con `HttpMessageHandler` falso) y `RutaServicioConRespaldoTests` (respaldo al mock y pausa ante fallas de red)|
| — | README (cómo correr y configurar BD desde cero) | — | — | ❌ |

## 6. Mocks y pendientes transversales

### 6.1 Autenticación
- API: JWT firmado con `Jwt:Key` (`appsettings.json`, vence a los `Jwt:ExpiracionMinutos`), emitido por
  `TokenServicio` con los claims NameIdentifier (id), Name y Email. **Todo endpoint exige token** por la
  `FallbackPolicy` de `Program.cs`, salvo `[AllowAnonymous]` (`UsuariosController`). En cada acción:
  `var usuarioId = User.ObtenerUsuarioId();` (`API/Extensiones/ClaimsPrincipalExtensiones.cs`).
  **No usar más `UsuarioPrueba.Id` en controllers.** Swagger tiene botón *Authorize*: hacer login y pegar el `token`.
- Contraseñas: `PasswordHasher<Usuario>` (PBKDF2 con salt). El email se guarda en minúsculas y sin espacios.
  Reglas: mínimo 8 caracteres con al menos una letra y un número (`RegistroUsuarioDto`).
  Login incorrecto → 401 genérico "Email o contraseña incorrectos."; email repetido → 409.
- MVC: cookie (`LoginPath = "/Cuenta/Login"`) que guarda el JWT como claim `"token"` y vence junto con él;
  `TokenHandler` lo agrega como `Authorization: Bearer`. También hay `FallbackPolicy`: toda pantalla
  exige sesión salvo `CuentaController` y los archivos estáticos. Nombre del usuario en vistas: `User.Identity.Name`.

### 6.2 Servicio de rutas / mapas

**Servicio en la API (`IRutaServicio`)**
- Interfaz con coordenadas: `CalcularRutaAsync(recorrido)` (origen, paradas y destino en orden) y
  `CompararConEnvioAsync(recorridoHabitual, retiro, entrega)` (distancia y tiempo habitual vs. con el envío,
  desvío y tiempo adicional). Completa solo datos de ruta: el monto, peso, capacidad y ganancia por km los
  compone `OportunidadesServicio`.
- Implementaciones en `API/Servicios/Rutas/`: `RutaServicioGoogle` (Routes API por REST, sin tráfico, caché en
  memoria por coordenadas) y `RutaServicioMock` (valores simulados). Helpers: `ComparadorRutas`,
  `InsercionRecorrido` (elige dónde van el retiro y la entrega, retiro antes que entrega, por menor largo en
  línea recta), `GeoUtil` (Haversine y prefiltros) y `PolylineCodec`.
- Selección por configuración: `Rutas:Proveedor` = `Auto` (default: Google si hay key, si no mock) |
  `Google` (falla al arrancar si falta la key) | `Mock`. La key del servidor va en user-secrets de la API:
  `dotnet user-secrets set "Rutas:GoogleApiKey" "<clave>" --project RuteaYa.API`. `Rutas:RespaldoAlMock` (default `true`) 
  activa el respaldo al mock cuando se usa Google. El arranque loguea `Servicio de rutas: Mock | Google (con respaldo al mock) | 
  Google (sin respaldo)` (con IIS Express se ve en Salida → Depurar).
- Respaldo (`RutaServicioConRespaldo`, decorador registrado en `Program.cs`): si Google lanza `RutaNoDisponibleException`, 
  falla la red, hay timeout o la respuesta viene rota, responde con el mock y deja un `LogWarning` 
  (con el mock, distancias, desvíos y tiempos son simulados). Ante fallas de red o timeout pausa Google durante 60 s para no 
  esperar el timeout en cada llamada; si es solo una ruta no encontrada o una key inválida, no pausa. 
  No tapa la cancelación del llamador ni los errores de programación. Para probar que Google anda de verdad, 
  poner `Rutas:RespaldoAlMock = false`.
- La key del servidor es **distinta** de la del mapa: sin restricción de sitios web y solo con Routes API.
  **No usar Geocoding API** (exige facturación; el proyecto de Google es un Maps Demo Project sin cuenta de
  facturación, así que no se puede cobrar). Por eso los métodos de texto (`GeocodificarDireccionAsync`,
  `CalcularRutaConParadasAsync`, `CalcularRutaAlternativaEnvioAsync`, `CalcularDesvioAsync`,
  `ObtenerDetalleOportunidadAsync`) son legado y fallan con Google: usar siempre los de coordenadas.
- `ViajeServicio` calcula `Viaje.DistanciaKm/DuracionMin` al crear y editar con `CalcularRutaAsync`, pasando
  las coordenadas del recorrido completo en orden. `OportunidadesServicio` usa `CompararConEnvioAsync`.
- Direcciones: `ViajeServicio.BuscarDireccionesAsync` filtra una lista fija de 12 barrios (con coordenadas) y
  también la usa Nuevo envío (`Viajes/BuscarDirecciones`).

**Mapa en el navegador**
- Partial `Views/Shared/_MapaRuta.cshtml` (la usan `_FormularioViaje` en Nuevo/Editar y `Viajes/Detalle`) +
  `wwwroot/js/mapa-viaje.js`, con Maps JavaScript API y Routes API (`Route.computeRoutes`; la
  `DirectionsService` legacy no está habilitada en proyectos nuevos). En el formulario lee las coordenadas de
  los hidden inputs que completa `formulario-viaje.js` y se actualiza con el evento `ruta:cambio`
  (`avisarCambioRuta()`) o con el botón "Actualizar ruta"; en Detalle toma los datos de `ViajeDetalleDto`
  (`ViewData["ViajeMapa"]`, JSON en `#mapa-datos`). Las paradas respetan el orden elegido. La polilínea se
  calcula en el navegador, no en la API.
- Key del navegador: `GoogleMaps:ApiKey` (config del MVC), en user-secrets del MVC
  (`dotnet user-secrets set "GoogleMaps:ApiKey" "<clave>" --project RuteaYa.MVC`). Restringida por referrer
  HTTP (`localhost:*`) y a Maps JavaScript API + Routes API. Sin key, la partial muestra "Mapa no disponible"
  en lugar del mapa y el resto del formulario funciona.

**Pendiente**
- Tests de `RutasOpciones.UsarGoogle()`.
- Con `Rutas:RespaldoAlMock = false` y Google caído, Oportunidades muestra la lista vacía: cada envío sin ruta
  se descarta (queda un `LogWarning` por envío en `OportunidadesServicio`).
- Borrar los métodos de texto de `IRutaServicio` y migrar `RutasController`.
- Buscador de direcciones real (hoy lista fija).

### 6.3 Zona horaria
- Todo lo que se guarda como fecha/hora de evento (`Envio.CreadoEn`, `EnvioEstadoHistorial.Fecha`,
  `Asignacion.AsignadoEn`) se escribe en **UTC** (`DateTime.UtcNow`) — es la convención correcta para
  guardar en base, independiente de dónde corra el servidor. `EnvioEstadoMaquina.Transicionar` también
  usa `DateTime.UtcNow` para cada entrada de historial.
- **Las vistas son responsables de convertir a hora local antes de mostrar.** `EtiquetasHelper.AHoraLocal()`
  convierte un `DateTime` UTC a hora de Argentina con un offset fijo de -3 (Argentina no tiene horario de
  verano desde 2009, por eso no hace falta resolver el TZ por nombre — evita diferencias entre el ID de
  Windows y el de Linux/Docker). Usar siempre `@fecha.AHoraLocal().ToString("dd/MM/yyyy HH:mm")` en las
  vistas, nunca `@fecha.ToString(...)` directo sobre un campo UTC (se ve adelantado 3 horas).
- Pendiente: revisar si `Viaje.HoraSalida`/`HoraLlegada` (`TimeOnly`, hora del recorrido, no un instante)
  necesitan el mismo tratamiento — por ahora no, porque no son un timestamp sino un horario de reloj fijo
  del viaje.

## 7. Requisitos del enunciado (resumen fiel)

Fuente completa: `docs/enunciado.md` (y mockups en `docs/mockups/`).

### 7.1 Usuarios
Registro: Nombre y Apellido no vacíos, Email formato válido y **único**, Contraseña con longitud mínima
(y requisitos que defina el grupo), Confirmación igual a la contraseña. Login: Email + Contraseña;
credenciales incorrectas sin revelar cuál falló. Todas las pantallas salvo login/registro requieren
autenticación. Cerrar sesión desde el menú del usuario.

### 7.2 Estados
- Envío: `BuscandoConductor → ConductorAsignado → Retirado → EnCamino → Entregado`; `Cancelado`.
- Viaje: Activo / Inactivo (desactivar, no borrar).
- Transiciones válidas (derivadas del enunciado y mockups, implementadas en `EnvioEstadoMaquina`, ver 4.1):
  - `BuscandoConductor → ConductorAsignado` (al confirmar asignación) · `→ Cancelado` (remitente).
  - `ConductorAsignado → Retirado` (conductor) · `→ Cancelado` (remitente; el mockup lo permite).
  - `Retirado → EnCamino → Entregado` (conductor). No se puede entregar sin haber retirado.
  - `Entregado` y `Cancelado` son finales: no se modifican ni vuelven a pendiente; un cancelado no se asigna.
- Transición inválida → error apropiado (sugerido **409 Conflict** con mensaje; 400 si el dato es inválido).
  Registrar cada cambio en `EnvioEstadoHistorial`.

### 7.3 Reglas de negocio generales
- Nadie modifica ni ve viajes/envíos de otro usuario (404 o 403; se sugiere 404 para no revelar existencia).
- Un envío se asigna una sola vez; no aceptar envíos ya asignados o cancelados.
- Un viaje no supera su cantidad máxima de paquetes ni su peso máximo (contando asignaciones ya
  existentes para esa fecha), ni el desvío máximo.
- Compatibilidad considera recorrido del viaje y recorrido necesario para el paquete
  (origen/destino del envío respetando el orden del recorrido: el retiro antes que la entrega).
- Fecha: el envío debe poder hacerse en una fecha en que el viaje ocurre (día de la semana del viaje,
  fecha ≥ hoy y ≤ `FechaLimite` del envío). No ofrecer envíos propios.
- Optimizador maximiza ganancia; **empate → menor desvío**.
- Ganancia por km adicional = Monto / desvío.

### 7.4 Validaciones por pantalla
- **Viaje** (ya en `CrearViajeDto`): origen y destino obligatorios y distintos, ≥1 día, horas válidas
  (llegada > salida, mismo día), MaxPaquetes > 0, PesoMaxKg > 0, DesvioMaxKm > 0, paradas sin repetir,
  no iguales a origen/destino, con orden. Editar/desactivar solo viajes propios; Detalle solo el dueño.
- **Envío** (ya en `CrearEnvioDto`): origen y destino obligatorios y distintos, fecha no anterior a hoy
  (`[FechaNoAnteriorAHoy]`), peso > 0, tamaño obligatorio (Pequeño/Mediano/Grande), descripción
  obligatoria, monto > 0.
- **Mis envíos**: filtros por estado (Todos, Buscando conductor, Conductor asignado, Entregado, Cancelado),
  muestra fecha y origen/destino; acciones según estado (Cancelar solo si no está retirado/finalizado;
  ver viaje asociado). ✅ implementado (`EnvioDto.PuedeCancelar` calculado con `EnvioEstadoMaquina`).
- **Oportunidades**: viaje del usuario, solo envíos disponibles, respetar fecha/día, capacidad, peso y
  compatibilidad de ruta; ordenar por ganancia, desvío o compatibilidad (etiquetas tipo
  "Muy compatible / Compatible / Mucho desvío" según desvío relativo al máximo); ver detalle; aceptar.
- **Detalle de oportunidad**: distancia habitual, distancia con el envío, desvío, tiempo habitual,
  tiempo adicional, pago, peso, capacidad restante (kg y paquetes), ganancia por km adicional.
  Cálculo consistente con la API; no aceptar si las restricciones dejaron de cumplirse (revalidar en el POST).
- **Optimizador**: muestra viaje, uso de paquetes/peso/desvío vs. máximos, combinación recomendada
  (ganancia, peso, desvío, paquetes) y otras combinaciones con motivo de descarte ("Supera 10 kg").
  Confirmar la recomendada lleva a Confirmación.
- **Confirmación**: resumen (viaje, paquetes, ganancia total, peso total, desvío total, cantidad).
  Volver sin modificar. Confirmar solo si todos siguen disponibles; si otro usuario tomó alguno →
  informar conflicto (409) y recalcular o cancelar. Todo en una transacción (`AgregarAsync(List<Asignacion>)`).
- **Mis transportes**: viaje, paquete, estado, ganancia, origen/destino; acciones Marcar retirado /
  en camino / entregado respetando transiciones. ✅ implementado (`TransporteDto` con los tres flags
  `PuedeMarcarRetirado/EnCamino/Entregado` calculados con `EnvioEstadoMaquina`); ver viaje y ver envío.
- **Historial**: viajes realizados, envíos transportados, ganancia **efectiva** (solo entregados);
  filtros por fecha/estado; sin datos privados de otros usuarios.
- **Dashboard**: saludo, contadores (próximos viajes, envíos pendientes, oportunidades, potencial del mes),
  listas de próximos viajes y envíos pendientes, accesos a Nuevo envío / Nuevo viaje / Ver oportunidades.

### 7.5 Optimizador (sección 12) — caso de prueba esperado
Restricciones: máx. 3 paquetes, 10 kg, 5 km de desvío.

| Envío | Pago | Peso | Desvío |
|---|---|---|---|
| A | 4.000 | 2 | 1,0 |
| B | 5.500 | 6 | 2,0 |
| C | 3.500 | 4 | 1,0 |
| D | 7.000 | 8 | 3,0 |
| E | 2.500 | 1 | 0,5 |

Resultado esperado: **A + B + E = $12.000, 9 kg, 3,5 km**. Otras: A+D $11.000 (10 kg, 4 km),
A+C+E $10.000; A+B+D y B+C+D superan 10 kg. En el enunciado el desvío de la combinación es la **suma**
de desvíos individuales. Con pocos candidatos alcanza con enumerar subconjuntos (2ⁿ, podando por
cantidad/peso/desvío); es una mochila multidimensional. La cátedra tiene **tests ocultos** del
optimizador: mantenerlo como lógica pura (sin EF, sin HTTP) en un servicio fácilmente testeable,
con desempate determinístico (ganancia desc, desvío asc).

### 7.6 Potencial mensual (sección 13, requisito para el 10)
Para un mes y criterio (maximizar ganancia / minimizar desvío / equilibrio): tomar los viajes activos
del usuario, calcular las fechas del mes que caen en sus días (desde hoy si es el mes actual), buscar
oportunidades por viaje-fecha, aplicar el optimizador a cada una **sin repetir un envío entre viajes**,
acumular. Mostrar: viajes analizados, paquetes posibles, ganancia potencial, promedio por viaje,
ganancia por semana, mejor recorrido, y detalle que justifique. Distinguir claramente **potencial**
(estimado) de **efectiva** (historial).

### 7.7 Calidad y entrega
Tests unitarios de la lógica principal de cada endpoint (servicios con repositorios falsos/mocks),
migraciones EF, Swagger documentado, manejo de errores y HTTP apropiados, README para correr desde
un entorno limpio, datos iniciales de prueba. Se entrega por MIeL con link al repo (acceso para
pablokuko, juizmariano, MarceWey) y respuestas a: nota esperada, qué fue lo más complicado,
cómo se repartieron el trabajo, qué aprendieron. Defensa en llamada con demo.

## 8. Recetas

### 8.1 Agregar una funcionalidad de punta a punta
1. **DTOs** (`RuteaYa.DTOs`): `Crear<X>Dto` (entrada, con Data Annotations) y `<X>Dto` (salida).
2. **Repositorio** (si hace falta un query nuevo): método en `Interfaces/I<X>Repositorio.cs` + implementación.
3. **Servicio API**: método en `API/Servicios/Interfaces/I<X>Servicio.cs` + `API/Servicios/<X>Servicio.cs`;
   registrar `AddScoped` en `API/Program.cs` si es nuevo. Errores de negocio: las lecturas devuelven `null` (→ 404);
   las operaciones que modifican devuelven `ResultadoOperacion` (`API/Servicios/ResultadoOperacion.cs`:
   `Exito` / `NoEncontrado` / `Conflicto`) y el controller lo traduce a 2xx / 404 / 409 (ver `ViajesController.Actualizar`).
   Si la operación es una transición de estado, no reimplementar la regla: pasar por la máquina de
   estados correspondiente (ver 4.1).
4. **Controller API**: acción con `///` summary, `<response code>` y `[ProducesResponseType]` para
   cada código; usuario con `User.ObtenerUsuarioId()`.
5. **Servicio MVC**: método en `MVC/Servicios/Interfaces/I<X>Servicio.cs` + implementación con `HttpClient`;
   si es nuevo, `AddHttpClient<...>().AddHttpMessageHandler<TokenHandler>()` en `MVC/Program.cs`.
   Traducir 400 a `Dictionary<string,string[]>` como `ViajeServicio.CrearAsync`; 404/409 a resultados que la vista pueda mostrar.
6. **Controller + vista MVC** (+ ViewModel en `Models/` si la vista combina datos, + JS en `wwwroot/js`).
7. **Tests** en `RuteaYa.Tests` del servicio de la API.
8. Actualizar la tabla de la sección 5 de este archivo.

### 8.2 Cambiar el modelo
Editar entidad / `RuteaYaDbContext.OnModelCreating` → `dotnet ef migrations add <NombreEnPascal> --project RuteaYa.Entidades`
→ revisar la migración generada → la API la aplica al arrancar. No editar migraciones ya mergeadas en `main`.

### 8.3 Eliminar una funcionalidad
Recorrer la misma cadena en orden inverso (vista/JS/CSS → controller MVC → servicio MVC + registro →
controller API → servicio API + registro → métodos de repositorio sin otros usos → DTOs → entidad +
migración). Buscar referencias con `grep -rn "<Nombre>" --include=*.cs --include=*.cshtml --include=*.js`,
incluidos enlaces `asp-action`/`asp-controller` en otras vistas, y actualizar la sección 5.

### 8.4 Datos de ejemplo de los mockups (útiles para seed y tests)
Salen de `docs/mockups/` (descritos en texto en `docs/enunciado.md`, sección 6).
Usuario "Juan Pérez". Viajes: Belgrano → Lomas de Zamora (Lun, Mié, Vie 08:00–08:55, 3 paquetes, 10 kg,
5 km, 18,4 km / 55 min); Palermo → San Isidro (Vie 18:30–19:20). Envíos: Plaza Italia → Obelisco
(Zapatillas $4.000), Recoleta → Quilmes (Documentos $3.500/$5.500), Avellaneda → Palermo (Repuesto $2.500,
Entregado), Palermo → Avellaneda ($7.000, 8 kg), Caballito → Lomas ($4.000, 2 kg), Morón → Lomas
($3.500, 4 kg), Barrancas → Lomas ($2.500, 1 kg). Todas esas direcciones existen en `DireccionesDePrueba()`.

## 9. Gotchas

- `RuteaYa.API/Program.cs` crea un `RuteaYaDbContext` suelto y ejecuta `Migrate()` antes de construir
  la app: si la BD no está accesible, la API no arranca.
- `appsettings.json` del MVC apunta a `https://localhost:7134/` (perfil https de la API); Development usa
  `http://localhost:5125/`. Correr ambos con el mismo esquema.
- Oportunidades está en `api/oportunidades/{viajeId}` (los nombres de ruta pueden variar según el enunciado);
  cualquier cambio de ruta requiere ajustar `RuteaYa.MVC/Servicios/OportunidadesServicio.cs`.
- Bootstrap 5.3 no tiene `text-bg-*-subtle`: para badges suaves usar `bg-*-subtle text-*-emphasis`.
- `InicializadorDatos` está comentado en `API/Program.cs` y sus envíos son todos `Entregado` y sin coordenadas:
  para probar Oportunidades hay que crear envíos con otro usuario (no se ofrecen envíos propios).
- Todas las fechas/horas de evento se guardan en UTC; las vistas que las muestran deben pasar por
  `DateTime.AHoraLocal()` o se ven adelantadas 3 horas respecto a la hora de Argentina (ver 6.3).
- `ListarDisponiblesAsync` solo filtra por estado/fecha/usuario: capacidad, peso, día y desvío se
  calculan en el servicio.
- Al cambiar estados usar entidades con tracking (`ObtenerPorIdAsync`), no las de listados `AsNoTracking`.
- `RuteaYa.MVC.csproj.user` está versionado con perfil Docker activo; el Dockerfile solo construye el MVC.
- Los user-secrets del MVC pisan a `appsettings*.json`. Si guardaste `ApiUrl` ahí, hay que
  actualizarlo al cambiar de perfil de ejecución de la API (otro puerto), o el MVC queda
  apuntando a un puerto sin API (`SocketException ... localhost:XXXX`).
- Sin `GoogleMaps:ApiKey` el mapa se reemplaza por un aviso; sin `Rutas:GoogleApiKey` la API usa el mock (modo `Auto`). Con `Rutas:Proveedor = Google` y sin key, la API no arranca.
- Los user-secrets de la API solo se cargan en `Development`. Las dos claves de Google son distintas: la del servidor nunca va en el MVC.
- `dotnet build` falla con MSB3027 ("archivo bloqueado por IIS Express") si la API o el MVC siguen corriendo: detené la depuración antes de compilar.
- Con `Rutas:RespaldoAlMock = true` (default) una key inválida o una caída de Google no se nota en pantalla: Oportunidades responde con valores del mock. Mirar el log (`RutaServicioConRespaldo: Warning`) para saber si se está usando el respaldo.
