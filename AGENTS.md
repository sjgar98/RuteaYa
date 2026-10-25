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
- CI (`.github/workflows/ci.yml`, en PRs a `main`): restore → build Release → `dotnet ef database update`
  contra SQL Server en contenedor → `dotnet test`.
- Commits: Conventional Commits en español (`feat:`, `fix:`, `chore:`), ramas + PR a `main`.

## 2. Arquitectura

```
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
| `RuteaYa.Tests` | xUnit; servicios de la API con repositorios falsos en memoria (sin librería de mocks), p. ej. `ViajeServicioTests` | Entidades, Repositorios, API, MVC |

Reglas duras del enunciado:
- El MVC **nunca** referencia Entidades/Repositorios ni toca el DbContext: todo pasa por la API.
- La lógica de negocio vive solo en `RuteaYa.API/Servicios`; no duplicarla en el MVC (el MVC solo
  replica validaciones de formulario en JS para UX).
- Controllers → Services → Repositorios. DTOs en todas las comunicaciones de la API.
- Códigos HTTP correctos (200, 201, 400, 401, 403/404, 409), Swagger documentado (comentarios `///`
  y `[ProducesResponseType]`; el csproj de la API genera el XML), async/await en I/O.

### Flujo típico (ej. "Nuevo viaje")
1. `MVC/Views/Viajes/Nuevo.cshtml` + `wwwroot/js/nuevo-viaje.js` (autocompletado, paradas, validaciones cliente).
2. `MVC/Controllers/ViajesController.Nuevo [POST]` → valida `ModelState` → `MVC/Servicios/ViajeServicio.CrearAsync`.
3. `POST api/viajes` → `API/Controllers/ViajesController.Crear` (`[ApiController]` devuelve 400
   `ValidationProblemDetails` automáticamente) → `API/Servicios/ViajeServicio.CrearAsync` → `ViajeRepositorio.AgregarAsync`.
4. Si la API responde 400, el servicio MVC devuelve `Dictionary<campo, mensajes[]>` y el controller
   los carga en `ModelState`. Éxito → `TempData["MensajeExito"]` (toast en la vista destino) + redirect.

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
  `[DistintoDe(nameof(X))]`, `[MayorQue(nameof(X))]`, `[DireccionesUnicas(nameof(A), nameof(B))]`.
  Las reglas que dependen de BD (dueño, estado, disponibilidad) van en el servicio de la API.
  En el cliente, las mismas reglas se replican con `$.validator.addMethod` (ver final de `nuevo-viaje.js`).
- Propiedades numéricas obligatorias de DTOs de entrada son nullable (`int?`, `decimal?`, `TimeOnly?`)
  + `[Required]`, y el servicio usa `.Value`.
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
  `_ViewImports`): `DiaSemana.Abreviar()` ("Mié"), `EstadoEnvio.Texto()`, `decimal.Moneda()` ("$4.000"),
  `decimal.Numero()` ("3,5"), con cultura es-AR.
- Recursos ajenos: el servicio de la API devuelve `null` si no existe **o** no es del usuario, y el
  controller responde 404 (ver `ViajeServicio.ObtenerDetalleAsync`).

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
- `IViajeRepositorio`: `ListarPorUsuarioAsync(usuarioId, activo?)`, `ObtenerPorIdAsync` (con Dias, Paradas ordenadas, Asignaciones+Envio), `AgregarAsync`, `GuardarCambiosAsync`.
- `IEnvioRepositorio`: `ListarPorUsuarioAsync(usuarioId, estado?)`, `ListarDisponiblesAsync(desde, usuarioIdExcluido)` (BuscandoConductor, no vencidos, de otros usuarios), `ListarPorIdsAsync`, `ObtenerPorIdAsync` (con Asignacion+Viaje e Historial), `AgregarAsync`, `GuardarCambiosAsync`.
- `IAsignacionRepositorio`: `ListarPorConductorAsync`, `ObtenerPorIdAsync`, `AgregarAsync(List<Asignacion>)` (una o varias en un solo `SaveChanges`).

Todos registrados como `Scoped` en `RuteaYa.API/Program.cs`. Solo existe `IViajeServicio` en la API.

## 5. Estado por funcionalidad

Leyenda: ✅ hecho · 🟡 parcial/mock · ❌ no existe. Rutas de API sugeridas para lo que falta (mantener REST coherente).

| # | Pantalla / recurso | API | MVC | Estado |
|---|---|---|---|---|
| 1 | Login / Registro / Logout | ✅ `POST api/usuarios/registro` (201 / 400 / 409), `POST api/usuarios/login` (200 con JWT / 401) | ✅ `CuentaController` (Login, Registro, Logout) con `_LayoutCuenta`, cookie con claim `"token"` | ✅ (ver 6.1) |
| 2 | Dashboard | ❌ (resumen: próximos viajes, envíos pendientes, nº oportunidades, potencial del mes) | 🟡 `HomeController.Index` es placeholder | ❌ |
| 3 | Mis viajes | ✅ `GET api/viajes?activo=` | ✅ `Viajes/Index` (filtros Todos/Activos/Inactivos, toast) | 🟡 Ver detalle ✅; los botones Oportunidades / Desactivar apuntan a acciones que **no existen** |
| 4 | Nuevo viaje | ✅ `POST api/viajes`, ✅ `GET api/viajes/direcciones?texto=` (lista fija) | ✅ `Viajes/Nuevo` + `nuevo-viaje.js` (paradas: agregar/quitar/reordenar) | 🟡 Distancia/duración = 0 (sin servicio de rutas), direcciones mock, sin mapa |
| 5 | Detalle / Editar / Desactivar viaje | ✅ `GET api/viajes/{id}` (`ViajeDetalleDto`: paradas + paquetes asignados; 404 si no es propio) · ❌ `PUT api/viajes/{id}`, `PATCH api/viajes/{id}/desactivar` (o `POST`) | ✅ `Viajes/Detalle` · ❌ `Viajes/Editar`, `Viajes/Desactivar [POST]` | 🟡 Detalle hecho; paquetes asignados **mock** (`PaquetesDePrueba()`, `TODO Paquetes asignados`); sin mapa; Editar/Desactivar ❌ |
| — | Paradas dentro de un viaje | ❌ `POST/PUT/DELETE api/viajes/{id}/paradas[/{paradaId}]` (o vía PUT del viaje) | se editan en el form del viaje | ❌ |
| 6 | Nuevo envío | ❌ `POST api/envios` | ❌ `Envios/Nuevo` | ❌ (enum `TamanoEnvio` y entidad ya existen) |
| 7 | Mis envíos (+ detalle, cancelar) | ❌ `GET api/envios?estado=`, `GET api/envios/{id}`, `POST api/envios/{id}/cancelar` | ❌ `Envios/Index`, `Envios/Detalle` | ❌ |
| 8 | Oportunidades | ❌ `GET api/viajes/{id}/oportunidades?orden=` (`ganancia`, `desvio` o `compatibilidad`) | ❌ `OportunidadesController.Index(viajeId)` (ya enlazado desde Mis viajes) | ❌ |
| 9 | Detalle de oportunidad | ❌ `GET api/viajes/{id}/oportunidades/{envioId}` | ❌ | ❌ |
| 10 | Optimizador | ❌ `GET api/viajes/{id}/optimizacion` (o `POST`) | ❌ | ❌ |
| 11 | Confirmación / asignación | ❌ `POST api/asignaciones` (viajeId, fecha, envioIds[]) → 201 / 409 | ❌ | ❌ |
| 12 | Mis transportes | ❌ `GET api/transportes`, `POST api/transportes/{asignacionId}/estado` (o `/retirado`, `/en-camino`, `/entregado`) | ❌ | ❌ |
| 13 | Historial | ❌ `GET api/historial?desde=&hasta=&estado=` | ❌ | ❌ |
| 14 | Potencial mensual (req. para 10) | ❌ `GET api/potencial?mes=yyyy-MM&criterio=` (`ganancia`, `desvio` o `equilibrio`) | ❌ | ❌ |
| — | Servicio de rutas/mapas | ❌ interfaz (p. ej. `IRutasServicio`) + impl. mock + impl. real opcional | — | ❌ |
| — | Layout con sidebar | — | ✅ `_Layout.cshtml` + `_MenuLateral.cshtml` (secciones → `/<Controller>`, activa según el controller actual) + `_MenuUsuario.cshtml` (nombre, email, Cerrar sesión). En celulares el menú es un offcanvas | ✅ Controllers esperados: `Envios`, `Oportunidades`, `Optimizador`, `Transportes`, `Historial`, `Potencial` (acción `Index`); si se usa otro nombre, cambiarlo en `_MenuLateral` |
| — | Tests unitarios por endpoint | — | — | 🟡 solo `ViajeServicio.ObtenerDetalleAsync` |
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

### 6.2 Servicio de rutas / mapas (no implementado)
- `ViajeServicio.BuscarDireccionesAsync` filtra una lista fija de 12 barrios de CABA/GBA
  (`DireccionesDePrueba()`), sin distinguir tildes/mayúsculas.
- `Viaje.DistanciaKm` / `DuracionMin` se guardan en 0.
- Requisito: abstracción de servicio (la lógica no depende del proveedor), con **implementación mock
  obligatoria** cuando no hay API key (p. ej. distancia Haversine × factor + velocidad promedio) y
  opcionalmente una real (Google Maps, OSRM, etc.), elegida por configuración en `Program.cs`.
  Debe: ruta origen→destino con paradas, distancia habitual, distancia con el envío
  (origen viaje → … → origen envío → destino envío → … → destino viaje), desvío = diferencia, y tiempo adicional.
  Mover ahí también la búsqueda de direcciones.

## 7. Requisitos del enunciado (resumen fiel)

### 7.1 Usuarios
Registro: Nombre y Apellido no vacíos, Email formato válido y **único**, Contraseña con longitud mínima
(y requisitos que defina el grupo), Confirmación igual a la contraseña. Login: Email + Contraseña;
credenciales incorrectas sin revelar cuál falló. Todas las pantallas salvo login/registro requieren
autenticación. Cerrar sesión desde el menú del usuario.

### 7.2 Estados
- Envío: `BuscandoConductor → ConductorAsignado → Retirado → EnCamino → Entregado`; `Cancelado`.
- Viaje: Activo / Inactivo (desactivar, no borrar).
- Transiciones válidas (derivadas del enunciado y mockups):
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
- **Envío**: origen y destino obligatorios y distintos, fecha no anterior a hoy, peso > 0, tamaño
  obligatorio (Pequeño/Mediano/Grande), descripción obligatoria, monto > 0.
- **Mis envíos**: filtros por estado (Todos, Buscando conductor, Conductor asignado, Entregado, Cancelado),
  muestra fecha y origen/destino; acciones según estado (Cancelar solo si no está retirado/finalizado;
  ver viaje asociado).
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
  en camino / entregado respetando transiciones; ver viaje y ver envío.
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
   registrar `AddScoped` en `API/Program.cs` si es nuevo. Errores de negocio: devolver un resultado que el
   controller traduzca a 404/409 (o excepciones de dominio propias capturadas en el controller /
   un filtro); mantener un único criterio en todo el proyecto.
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
- Las acciones MVC `Viajes/Editar`, `Viajes/Desactivar`, `OportunidadesController` y `Envios/Detalle`
  están enlazadas desde `Views/Viajes/Index.cshtml` y `Views/Viajes/Detalle.cshtml` pero aún no existen (404).
- `ListarDisponiblesAsync` solo filtra por estado/fecha/usuario: capacidad, peso, día y desvío se
  calculan en el servicio.
- Al cambiar estados usar entidades con tracking (`ObtenerPorIdAsync`), no las de listados `AsNoTracking`.
- `RuteaYa.MVC.csproj.user` está versionado con perfil Docker activo; el Dockerfile solo construye el MVC.
