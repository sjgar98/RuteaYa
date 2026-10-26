# PW3 - 2026 - 2C — TP Integrador — RuteaYa

**Plataforma de transporte colaborativo**

*ASP.NET Core MVC + Web API + Entity Framework Core + SQL Server*

> Transcripción del enunciado oficial (`enunciado.pdf`). Los mockups de cada pantalla están en
> [`mockups/`](mockups/) y se referencian desde la sección 6.

## 1. Descripción general

RuteaYa es una plataforma que permite aprovechar viajes habituales realizados en automóvil para
transportar pequeños paquetes. Los conductores registran recorridos que realizan habitualmente y el
sistema busca envíos compatibles con esas rutas. Las personas que necesitan enviar un paquete pueden
publicarlo indicando origen, destino, fecha, peso y monto ofrecido.

Un mismo usuario puede actuar como conductor y como remitente. El objetivo principal del sistema es
encontrar oportunidades de transporte y ayudar al conductor a seleccionar la combinación de paquetes
que le permita obtener la mayor ganancia posible respetando las restricciones de su viaje.

## 2. Objetivos del trabajo

- Desarrollar una aplicación web utilizando ASP.NET Core MVC.
- Implementar una Web API REST en C#.
- Utilizar Entity Framework Core para la persistencia.
- Utilizar SQL Server como base de datos.
- Separar la interfaz MVC de la lógica de acceso a datos mediante la Web API.
- Trabajar con entidades relacionadas, DTOs, validaciones y reglas de negocio.
- Integrar un servicio externo de rutas/mapas o implementar una alternativa equivalente provista por la cátedra.
- Implementar un algoritmo que permita optimizar las oportunidades de transporte.

## 3. Arquitectura y requisitos técnicos

- La solución deberá estar compuesta, como mínimo, por un proyecto MVC y un proyecto Web API.
- La aplicación MVC NO deberá acceder directamente al DbContext ni a Entity Framework, deberá haber al
  menos una capa de lógica o servicio.
- Se deberán utilizar DTOs para las comunicaciones de la API cuando corresponda.
- La API deberá utilizar correctamente los códigos HTTP (200, 201, 400, 404, 409, etc.).
- La API deberá estar documentada mediante Swagger/OpenAPI.
- Se deberán implementar validaciones tanto del lado del servidor como, cuando resulte apropiado, en la interfaz MVC.
- La solución deberá utilizar una arquitectura que separe Controllers, Services y acceso a datos.

## 4. Usuarios y acceso

La aplicación deberá permitir registrar usuarios e iniciar sesión. No se exige que el usuario tenga un rol
fijo: un mismo usuario puede publicar envíos y registrar viajes.

| Dato | Obligatorio | Validaciones |
|---|---|---|
| Nombre | Sí | No vacío. |
| Apellido | Sí | No vacío. |
| Email | Sí | Formato válido y único. |
| Contraseña | Sí | Longitud mínima y requisitos definidos por el grupo/cátedra. |
| Confirmación | Sí | Debe coincidir con la contraseña. |

## 5. Estados principales

### Estado de un envío

- Buscando conductor
- Conductor asignado
- Retirado
- En camino
- Entregado
- Cancelado

### Estado de un viaje

- Activo
- Inactivo

### Reglas generales de estados

- No se podrá modificar libremente una operación que ya haya finalizado.
- Un envío entregado no podrá volver a estado pendiente.
- Un envío cancelado no podrá ser asignado a un conductor.
- Una transición de estado inválida deberá devolver un error apropiado.

## 6. Pantallas y funcionalidades

> **Importante:** Las pantallas son a modo de ejemplo, deben armar su propio diseño que respete la funcionalidad.

### Pantalla 1 — Login / Registro

![Mockup Pantalla 1 — Login / Registro](<mockups/Pantalla 1 - Login - Registro.png>)

*Mockup: panel izquierdo oscuro con "RuteaYa" y el lema "Convertí tus viajes habituales en una
oportunidad. Compartí el espacio libre de tus recorridos y ganá dinero llevando paquetes."; a la derecha
el formulario "Ingresar" (Email, Contraseña, botón Ingresar) y el enlace "¿Todavía no tenés una cuenta? Crear cuenta".*

**Funcionalidad**

- Acceso público.
- Permite iniciar sesión y registrar nuevos usuarios.

**Campos / información**

- Login: Email, Contraseña.
- Registro: Nombre, Apellido, Email, Contraseña, Confirmación de contraseña.

**Validaciones y reglas**

- Email con formato válido.
- Email único.
- Campos obligatorios.
- Contraseña y confirmación coincidentes.
- Credenciales incorrectas deben informar un error sin revelar información innecesaria.

**Acciones / accesos**

- Ingresar.
- Crear cuenta.
- Cerrar sesión desde el menú del usuario.

### Pantalla 2 — Dashboard

![Mockup Pantalla 2 — Dashboard](<mockups/Pantalla 2 - Dashboard.png>)

*Mockup: menú lateral (Inicio, Mis viajes, Mis envíos, Oportunidades, Optimizador, Mis transportes,
Historial; abajo "Juan Pérez · Cerrar sesión"). Encabezado "Hola, Juan — Esto pasa con tus viajes y
envíos." con botones Nuevo envío / Nuevo viaje. Contadores: Próximos viajes 2, Envíos pendientes 2,
Oportunidades 5, Potencial del mes $84.500 (destacado en verde). Tarjeta "Próximos viajes": Belgrano →
Lomas de Zamora (Lun, Mié, Vie · 08:00 – 08:55, Activo) y Palermo → San Isidro (Vie · 18:30 – 19:20,
Activo). Tarjeta "Envíos pendientes": Plaza Italia → Obelisco (Zapatillas · $4.000, Buscando conductor)
y Recoleta → Quilmes (Documentos · $3.500, Conductor asignado), con botón "Ver oportunidades".*

**Funcionalidad**

- Pantalla principal del usuario autenticado.
- Debe resumir viajes, envíos, oportunidades y potencial de ganancias.

**Campos / información**

- No requiere formulario.

**Validaciones y reglas**

- Solo accesible para usuarios autenticados.

### Pantalla 3 — Mis viajes

![Mockup Pantalla 3 — Mis viajes](<mockups/Pantalla 3 - Mis viajes.png>)

*Mockup: "Mis viajes — Tus recorridos habituales." con botón Nuevo viaje; filtros en pills Activos /
Inactivos / Todos. Lista: Belgrano → Lomas de Zamora (Lun, Mié, Vie · 08:00 – 08:55) y Palermo → San
Isidro (Vie · 18:30 – 19:20), cada uno con badge Activo y botones Ver detalle, Oportunidades, Desactivar.*

**Funcionalidad**

- Lista exclusivamente los viajes creados por el usuario autenticado.
- Permite consultar, modificar y desactivar viajes.

**Campos / información**

- Filtros: Activos, Inactivos, Todos.

**Validaciones y reglas**

- No mostrar viajes de otros usuarios.
- Solo viajes propios pueden editarse o desactivarse.

**Acciones / accesos**

- Ver detalle (luego se puede hacer click en Editar).
- Oportunidades.
- Desactivar.

### Pantalla 4 — Nuevo viaje

![Mockup Pantalla 4 — Nuevo viaje](<mockups/Pantalla 4 - Nuevo viaje.png>)

*Mockup: "Nuevo viaje — Registrá un recorrido habitual." Campos: Origen ("¿Desde dónde salís?"), Destino
("¿A dónde llegás?"), Paradas intermedias (opcional, separadas por coma), Días de la semana (botones
circulares L M X J V S D), Hora de salida, Hora de llegada, Paquetes máximos (3), Peso máximo (kg) (10),
Desvío máximo (km) (3). Botones Cancelar / Guardar viaje.*

**Funcionalidad**

- Permite registrar un recorrido habitual.
- Debe calcular y mostrar la ruta cuando se disponga del servicio de mapas/rutas.
- Permite agregar paradas intermedias opcionales.

**Campos / información**

- Origen.
- Destino.
- Días de la semana.
- Hora de salida.
- Hora estimada de llegada.
- Cantidad máxima de paquetes.
- Peso máximo total.
- Paradas intermedias opcionales.

**Validaciones y reglas**

- Origen obligatorio.
- Destino obligatorio.
- Origen y destino diferentes.
- Al menos un día seleccionado.
- Hora válida.
- Capacidad mayor que cero.
- Peso máximo mayor que cero.
- No repetir paradas.
- Una parada no puede coincidir con origen o destino.
- Las paradas deben mantener un orden.

**Acciones / accesos**

- Guardar viaje.
- Cancelar.
- Calcular/actualizar ruta.
- Agregar/eliminar/reordenar paradas.

### Pantalla 5 — Detalle del viaje

![Mockup Pantalla 5 — Detalle del viaje](<mockups/Pantalla 5 - Detalle del viaje.png>)

*Mockup: "Detalle del viaje — Belgrano → Lomas de Zamora" con botones Volver, Editar, Desactivar, Ver
oportunidades. Tarjeta "Recorrido": Estado Activo, Días Lun, Mié, Vie, Horarios 08:00 – 08:55, Capacidad
3 paquetes, Peso máximo 10 kg, Desvío máximo 5 km. Tarjeta "Paquetes asignados": Zapatillas (Retirado ·
Fecha límite 20/10/2026), $4.000, botón Ver paquete.*

**Funcionalidad**

- Muestra información completa del viaje y su recorrido.
- Debe visualizar el mapa y las paradas cuando corresponda.

**Campos / información**

- Origen.
- Destino.
- Días.
- Horarios.
- Capacidad.
- Peso máximo.
- Paradas.
- Paquetes asignados.

**Validaciones y reglas**

- Solo accesible al propietario del viaje.

**Acciones / accesos**

- Editar.
- Desactivar.
- Ver oportunidades.
- Consultar paquetes asignados.

### Pantalla 6 — Nuevo envío

![Mockup Pantalla 6 — Nuevo envío](<mockups/Pantalla 6 - Nuevo envio.png>)

*Mockup: "Nuevo envío — Publicá un paquete que necesita ser transportado." Campos: Origen ("¿Dónde lo
retiran?"), Destino ("¿A dónde llega?"), Fecha, Peso (kg), Monto ofrecido ($), Tamaño (pills Pequeño /
Mediano / Grande), Descripción ("Ej: zapatillas, documentos"). Botones Cancelar / Publicar envío.*

**Funcionalidad**

- Permite publicar un paquete que necesita ser transportado.

**Campos / información**

- Origen.
- Destino.
- Fecha.
- Peso.
- Tamaño: pequeño/mediano/grande.
- Descripción.
- Monto ofrecido.

**Validaciones y reglas**

- Origen y destino obligatorios.
- Origen y destino diferentes.
- Fecha no anterior a la fecha actual.
- Peso mayor que cero.
- Tamaño obligatorio.
- Descripción obligatoria.
- Monto ofrecido mayor que cero.

**Acciones / accesos**

- Publicar envío.
- Cancelar.

### Pantalla 7 — Mis envíos

![Mockup Pantalla 7 — Mis envíos](<mockups/Pantalla 7 - Mis envios.png>)

*Mockup: "Mis envíos — Seguí el estado de lo que enviaste." con botón Nuevo envío; filtros Todos /
Buscando conductor / Conductor asignado / Entregado / Cancelado. Lista: Plaza Italia → Obelisco
(Zapatillas · $4.000, Buscando conductor, Ver detalle + Cancelar), Recoleta → Quilmes (Documentos ·
$3.500, Conductor asignado, Ver detalle + Cancelar), Avellaneda → Palermo (Repuesto · $2.500, Entregado,
solo Ver detalle).*

**Funcionalidad**

- Lista los envíos creados por el usuario.
- Permite consultar su estado y acciones disponibles.

**Campos / información**

- Filtros por estado.
- Fecha.
- Origen/destino.

**Validaciones y reglas**

- Solo mostrar envíos propios.
- Las acciones disponibles dependen del estado.

**Acciones / accesos**

- Ver detalle.
- Cancelar cuando corresponda.
- Consultar viaje asociado.

### Pantalla 8 — Oportunidades

![Mockup Pantalla 8 — Oportunidades](<mockups/Pantalla 8 - Oportunidades.png>)

*Mockup: "Oportunidades — Envíos compatibles con Belgrano → Lomas de Zamora." con botón Optimizar
combinación; "Ordenar por" Ganancia / Desvío / Compatibilidad. Lista (cada fila con Ver detalle y
Aceptar): Palermo → Avellaneda (8 kg · desvío 3 km, Mucho desvío, $7.000), Recoleta → Quilmes (6 kg ·
desvío 2 km, Compatible, $5.500), Caballito → Lomas (2 kg · desvío 1 km, Muy compatible, $4.000),
Morón → Lomas (4 kg · desvío 1 km, Muy compatible, $3.500), Barrancas → Lomas (1 kg · desvío 0.5 km,
Muy compatible, $2.500).*

**Funcionalidad**

- Permite al conductor seleccionar uno de sus viajes y buscar envíos compatibles.
- Es una de las pantallas centrales del sistema.

**Campos / información**

- Viaje seleccionado.
- Filtros de orden/prioridad opcionales.

**Validaciones y reglas**

- El viaje debe pertenecer al usuario.
- Solo considerar envíos disponibles.
- Respetar fecha/día.
- Respetar capacidad y peso.
- Respetar las reglas de compatibilidad de rutas.
- No ofrecer envíos cancelados o ya asignados.

**Acciones / accesos**

- Listar oportunidades.
- Ordenar por ganancia, desvío o compatibilidad.
- Ver detalle.
- Aceptar una oportunidad.

### Pantalla 9 — Detalle de oportunidad

![Mockup Pantalla 9 — Detalle de oportunidad](<mockups/Pantalla 9 - Detalle de oportunidad.png>)

*Mockup: "Detalle de oportunidad — Palermo → Avellaneda" con botón Volver. Tarjeta "Ruta habitual vs.
con el envío": Distancia habitual 18,4 km, Distancia con el envío 21,4 km, Desvío 3 km, Tiempo habitual
55 min, Tiempo adicional +9 min, Peso 8 kg, Capacidad restante 2 kg · 2 paquetes. Tarjeta verde "Pago":
$7.000, "$2.333 por km adicional", botón Aceptar envío.*

**Funcionalidad**

- Explica por qué un envío es compatible con un viaje.
- Debe comparar la ruta habitual con la ruta necesaria para transportar el paquete.

**Campos / información**

- Distancia habitual.
- Distancia con el envío.
- Desvío.
- Tiempo habitual.
- Tiempo adicional.
- Pago.
- Peso.
- Capacidad restante.
- Ganancia por km adicional.

**Validaciones y reglas**

- El cálculo debe ser consistente con las reglas de la API.
- No permitir aceptar si las restricciones dejaron de cumplirse.

**Acciones / accesos**

- Aceptar envío.
- Volver.

### Pantalla 10 — Optimizador de oportunidades

![Mockup Pantalla 10 — Optimizador de oportunidades](<mockups/Pantalla 10 - Optimizador de oportunidades.png>)

*Mockup: "Optimizador de oportunidades — Evaluamos todas las combinaciones y elegimos la de mayor
ganancia." con botón Calcular de nuevo. Barra de resumen: Viaje Belgrano → Lomas de Zamora; Paquetes
3 de 3; Peso (kg) 9 de 10; Desvío (km) 3.5 de 5 (barras de progreso). Tarjeta verde "Combinación
recomendada": $12.000, "Envíos A + B + E · 9 kg · 3.5 km de desvío", botón Confirmar combinación.
Tarjeta "Otras combinaciones": A + D (10 kg · 4 km, $11.000), A + C + E (7 kg · 2.5 km, $10.000),
A + B + D (16 kg · 6 km, badge rojo "Supera 10 kg"), B + C + D (18 kg · 6 km, "Supera 10 kg"); nota
"En empate de ganancia gana la de menor desvío."*

**Funcionalidad**

- Debe analizar múltiples envíos compatibles y encontrar una combinación de oportunidades que maximice
  la ganancia del conductor respetando las restricciones del viaje.
- Debe mostrar claramente la combinación recomendada.

**Campos / información**

- Viaje.
- Capacidad máxima.
- Peso máximo.
- Desvío máximo.
- Conjunto de envíos candidatos.

**Validaciones y reglas**

- No superar cantidad máxima de paquetes.
- No superar peso máximo.
- No superar desvío máximo.
- Respetar fechas y días.
- Respetar origen/destino y orden del recorrido.
- Un mismo envío no puede formar parte de dos combinaciones aceptadas.

**Acciones / accesos**

- Calcular combinación óptima.
- Mostrar ganancia total.
- Mostrar peso total.
- Mostrar desvío.
- Mostrar paquetes seleccionados.
- Mostrar otras combinaciones o motivos de descarte cuando sea posible.
- Permitir confirmar la combinación recomendada.

### Pantalla 11 — Confirmación de selección

![Mockup Pantalla 11 — Confirmación de selección](<mockups/Pantalla 11 - Confirmacion de seleccion.png>)

*Mockup: "Confirmar selección — Revisá el resumen antes de reservar." Tarjeta del viaje Belgrano → Lomas
de Zamora con los paquetes: Caballito → Lomas (2 kg · 1 km, $4.000), Recoleta → Quilmes (6 kg · 2 km,
$5.500), Barrancas → Lomas (1 kg · 0.5 km, $2.500). Contadores: Ganancia total $12.000 (verde), Peso
total 9 kg, Desvío total 3.5 km, Paquetes 3. Botones Volver / Confirmar y reservar.*

**Funcionalidad**

- Muestra un resumen antes de reservar uno o varios envíos.

**Campos / información**

- Viaje.
- Paquetes seleccionados.
- Ganancia total.
- Peso total.
- Desvío total.
- Cantidad de paquetes.

**Validaciones y reglas**

- Volver sin modificar.
- Confirmar solo si todos los envíos siguen disponibles.
- Si otro usuario tomó un envío previamente, informar conflicto y recalcular o cancelar la operación.

**Acciones / accesos**

- Confirmar y reservar.
- Volver.

### Pantalla 12 — Mis transportes

![Mockup Pantalla 12 — Mis transportes](<mockups/Pantalla 12 - Mis transportes.png>)

*Mockup: "Mis transportes — Paquetes que aceptaste llevar." Fila: Zapatillas (Plaza Italia → Obelisco ·
$4.000), badge Retirado, botones Ver viaje, Ver envío y Marcar en camino (acción principal según el estado).*

**Funcionalidad**

- Permite al conductor gestionar paquetes que aceptó transportar.

**Campos / información**

- Viaje.
- Paquete.
- Estado.
- Ganancia.
- Origen/destino.

**Validaciones y reglas**

- Las acciones deben respetar las transiciones de estado.
- No marcar como entregado un paquete que no fue retirado.

**Acciones / accesos**

- Marcar retirado.
- Marcar en camino.
- Marcar entregado.
- Consultar detalle.

### Pantalla 13 — Historial

![Mockup Pantalla 13 — Historial](<mockups/Pantalla 13 - Historial.png>)

*Mockup: "Historial — Operaciones anteriores." Contadores: Viajes realizados 18, Envíos transportados 41,
Ganancia efectiva $126.500 (verde). Lista: Belgrano → Lomas de Zamora (Septiembre · 3 paquetes, $18.000),
Palermo → San Isidro (Septiembre · 2 paquetes, $9.500), Avellaneda → Palermo (Entregado · Repuesto, $2.500).*

**Funcionalidad**

- Permite consultar operaciones anteriores del usuario.

**Campos / información**

- Viajes realizados.
- Envíos transportados.
- Ganancias.
- Filtros por fecha/estado.

**Validaciones y reglas**

- No mostrar información privada de otros usuarios.

**Acciones / accesos**

- Consultar.
- Filtrar.
- Ver detalle.

### Pantalla 14 — Potencial de ganancias mensual (REQUISITO PARA 10)

*Sin mockup.*

**Funcionalidad**

- Debe estimar cuánto podría ganar potencialmente el conductor durante un mes utilizando sus viajes
  recurrentes y los envíos disponibles.
- Debe analizar los viajes futuros y aplicar el optimizador a las oportunidades disponibles.

**Campos / información**

- Mes.
- Criterio: maximizar ganancias / minimizar desvíos / equilibrio.
- Opcionalmente límites definidos por el sistema.

**Validaciones y reglas**

- Distinguir claramente ganancia potencial de ganancia efectivamente obtenida.
- No contar envíos incompatibles.
- No duplicar un envío en distintos viajes.
- Respetar capacidad, peso, fechas y desvíos.

**Acciones / accesos**

- Mostrar viajes analizados.
- Mostrar paquetes posibles.
- Mostrar ganancia potencial.
- Mostrar promedio por viaje.
- Mostrar ganancia por semana.
- Mostrar mejor recorrido.
- Permitir cambiar criterio de optimización.

## 7. Reglas de negocio generales

- Un usuario no puede modificar viajes o envíos pertenecientes a otro usuario.
- Un conductor no puede aceptar un envío que ya fue asignado.
- Un envío solo puede ser asignado una vez.
- Un viaje no puede superar su capacidad de paquetes.
- Un viaje no puede superar su peso máximo.
- Las operaciones deben respetar los estados definidos.
- El cálculo de compatibilidad debe considerar el recorrido del viaje y el recorrido necesario para
  transportar el paquete.
- El optimizador debe maximizar la ganancia respetando todas las restricciones.
- En caso de empate de ganancia, se deberá preferir la alternativa con menor desvío.

## 8. Integración con rutas y mapas

La aplicación podrá utilizar un servicio de rutas externo, por ejemplo Google Maps Platform (puede ser
otro), para calcular recorridos, distancias y tiempos. La integración deberá estar encapsulada detrás de
una abstracción de servicio, de modo que la lógica de negocio no dependa directamente del proveedor.

- Calcular la ruta entre origen y destino.
- Considerar las paradas intermedias cuando existan.
- Calcular la distancia del viaje habitual.
- Calcular la distancia de una alternativa incorporando origen y destino del paquete.
- Calcular el desvío producido por transportar el paquete.
- La solución deberá contemplar una alternativa mock/ficticia para pruebas o desarrollo cuando no
  exista una API key disponible.

## 9. Web API — requisitos mínimos

La implementación deberá exponer endpoints equivalentes a los siguientes. Los nombres pueden variar si
se mantiene una API REST coherente.

| Recurso | Operaciones mínimas |
|---|---|
| Usuarios | Registro / autenticación |
| Viajes | GET, GET por ID, POST, PUT, desactivar |
| Paradas | Crear/modificar/eliminar dentro de un viaje |
| Envíos | GET, GET por ID, POST, cancelar |
| Oportunidades | Buscar oportunidades para un viaje |
| Optimización | Calcular mejor combinación |
| Asignaciones | Aceptar/confirmar oportunidades |
| Transportes | Actualizar estados |
| Historial | Consultar operaciones |
| Potencial mensual | Calcular resumen mensual |

## 10. Calidad técnica

- Código organizado y mantenible.
- Debe haber **Tests Unitarios** que confirmen el correcto funcionamiento de la lógica principal de cada endpoint.
- Uso adecuado de nombres y responsabilidades.
- No duplicar lógica de negocio entre MVC y API.
- No acceder a la base de datos desde MVC.
- Uso adecuado de async/await en operaciones I/O.
- Manejo de errores y respuestas HTTP apropiadas.
- Validación de datos de entrada.
- Migraciones de Entity Framework o mecanismo equivalente definido por la cátedra.
- Swagger correctamente documentado.
- README con instrucciones para ejecutar el sistema y configurar la base de datos.

## 11. Entrega y pruebas

- La solución deberá poder ejecutarse desde un entorno limpio siguiendo las instrucciones del README.
- La base de datos deberá poder generarse mediante migraciones o script entregado.
- Se deberán incluir datos iniciales suficientes para probar las funcionalidades.
- Se podrán utilizar tests automatizados provistos por la cátedra para validar reglas de negocio y el optimizador.
- La cátedra podrá utilizar casos de prueba adicionales no visibles para los alumnos.
- Se deberá enviar la entrega por MIeL, compartiendo el link al repositorio (y acceso al repo para los
  usuarios `pablokuko`, `juizmariano`, `MarceWey`) y contestando las preguntas:
  - ¿Qué nota creen que deben sacarse? ¿Qué fue lo más complicado? ¿Cómo se organizaron para repartirse
    el trabajo? ¿Qué cosas aprendieron durante el desarrollo de la aplicación?
- El día de entrega se hará una llamada con cada grupo, donde un integrante del grupo presentará su
  pantalla y mostrará la funcionalidad, los profesores harán preguntas confirmando la participación de
  cada alumno.

## 12. Consideraciones para el optimizador

El objetivo del optimizador no es solamente filtrar envíos. Debe evaluar distintas combinaciones posibles y
seleccionar una solución que maximice la ganancia respetando las restricciones.

Ejemplo:

| Envío | Pago | Peso | Desvío |
|---|---|---|---|
| A | $4.000 | 2 kg | 1,0 km |
| B | $5.500 | 6 kg | 2,0 km |
| C | $3.500 | 4 kg | 1,0 km |
| D | $7.000 | 8 kg | 3,0 km |
| E | $2.500 | 1 kg | 0,5 km |

Si el viaje permite como máximo 3 paquetes, 10 kg y 5 km de desvío, el sistema deberá evaluar las
combinaciones posibles y seleccionar la alternativa que maximice la ganancia sin violar las restricciones.

## 13. Funcionalidad de excelencia — Potencial de ganancias

Para obtener la máxima calificación, el sistema deberá extender el optimizador para analizar múltiples
viajes recurrentes durante un mes y estimar una ganancia potencial.

- Considerar los viajes recurrentes del usuario.
- Determinar qué fechas del mes corresponden a cada viaje.
- Buscar oportunidades disponibles para cada viaje.
- Aplicar el optimizador a cada viaje.
- Evitar asignar el mismo envío a más de un viaje.
- Acumular la ganancia potencial.
- Permitir cambiar el criterio de optimización.
- Mostrar resultados agregados y detalle suficiente para justificar la estimación.
