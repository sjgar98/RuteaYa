// Formulario de viaje (Nuevo y Editar): autocompletado de direcciones, lista de paradas intermedias y validaciones entre campos.
(function () {
    const form = document.getElementById('form-viaje');
    if (!form) {
        return;
    }

    const ESPERA_MS = 1000;
    const MIN_CARACTERES = 3;
    const urlDirecciones = form.dataset.urlDirecciones;

    const origen = document.getElementById('OrigenDir');
    const destino = document.getElementById('DestinoDir');

    // Mismo criterio que el atributo DistintoDe de la API.
    function mismaDireccion(a, b) {
        return a.trim() !== '' && a.trim().toLowerCase() === b.trim().toLowerCase();
    }

    // Busca direcciones mientras se escribe (con espera) y las muestra en un desplegable bajo el input.
    // onSeleccion recibe la dirección elegida; onEdicion avisa que el texto cambió y la selección anterior ya no vale.
    function autocompletar(input, onSeleccion, onEdicion) {
        const lista = input.parentElement.querySelector('.autocompletar-lista');
        let temporizador;

        function cerrar() {
            lista.classList.add('d-none');
            lista.replaceChildren();
        }

        function mostrarMensaje(texto) {
            const item = document.createElement('li');
            item.className = 'list-group-item small text-muted';
            item.textContent = texto;
            lista.replaceChildren(item);
            lista.classList.remove('d-none');
        }

        function mostrar(direcciones) {
            if (direcciones.length === 0) {
                mostrarMensaje('Sin resultados.');
                return;
            }

            lista.replaceChildren(...direcciones.map(direccion => {
                const item = document.createElement('button');
                item.type = 'button';
                item.className = 'list-group-item list-group-item-action small';
                item.textContent = direccion.direccion;
                // Evita que el blur del input cierre la lista antes del click.
                item.addEventListener('mousedown', e => e.preventDefault());
                item.addEventListener('click', () => {
                    input.value = direccion.direccion;
                    cerrar();
                    onSeleccion(direccion);
                });
                return item;
            }));
            lista.classList.remove('d-none');
        }

        async function buscar(texto) {
            try {
                const respuesta = await fetch(`${urlDirecciones}?texto=${encodeURIComponent(texto)}`);
                if (!respuesta.ok) {
                    throw new Error(respuesta.statusText);
                }

                const direcciones = await respuesta.json();
                // Si se siguió escribiendo, esta respuesta ya no corresponde.
                if (texto === input.value.trim()) {
                    mostrar(direcciones);
                }
            } catch {
                mostrarMensaje('No se pudieron buscar direcciones.');
            }
        }

        input.addEventListener('input', () => {
            onEdicion();
            clearTimeout(temporizador);

            const texto = input.value.trim();
            if (texto.length < MIN_CARACTERES) {
                cerrar();
                return;
            }

            temporizador = setTimeout(() => buscar(texto), ESPERA_MS);
        });
        input.addEventListener('blur', cerrar);
        input.addEventListener('keydown', e => {
            if (e.key === 'Escape') {
                cerrar();
            } else if (e.key === 'Enter') {
                e.preventDefault();
            }
        });
    }

    function validarSiTieneValor(input) {
        if (input.value) {
            $(input).valid();
        }
    }

    // Origen y destino: la dirección elegida completa las coordenadas ocultas.
    for (const input of [origen, destino]) {
        const lat = document.querySelector(input.dataset.lat);
        const lng = document.querySelector(input.dataset.lng);

        autocompletar(input,
            direccion => {
                lat.value = direccion.lat;
                lng.value = direccion.lng;
                validarSiTieneValor(origen);
                validarSiTieneValor(destino);
            },
            () => {
                lat.value = '';
                lng.value = '';
            });
    }

    // Paradas intermedias: se envían como Paradas[i].Direccion/Lat/Lng en el orden de la lista
    // (__Invariant hace que las coordenadas se lean con punto decimal en el servidor).
    const paradas = JSON.parse(document.getElementById('paradas-iniciales').textContent);
    const busquedaParada = document.getElementById('parada-busqueda');
    const botonAgregar = document.getElementById('parada-agregar');
    const errorParada = document.getElementById('parada-error');
    const listaParadas = document.getElementById('paradas-lista');
    let paradaSeleccionada = null;

    function boton(texto, titulo, clases, onClick, deshabilitado) {
        const elemento = document.createElement('button');
        elemento.type = 'button';
        elemento.className = clases;
        elemento.textContent = texto;
        elemento.title = titulo;
        elemento.setAttribute('aria-label', titulo);
        elemento.disabled = !!deshabilitado;
        elemento.addEventListener('click', onClick);
        return elemento;
    }

    function oculto(nombre, valor) {
        const elemento = document.createElement('input');
        elemento.type = 'hidden';
        elemento.name = nombre;
        elemento.value = valor;
        return elemento;
    }

    function mover(indice, desplazamiento) {
        const [parada] = paradas.splice(indice, 1);
        paradas.splice(indice + desplazamiento, 0, parada);
        dibujarParadas();
    }

    function dibujarParadas() {
        listaParadas.replaceChildren(...paradas.map((parada, i) => {
            const texto = document.createElement('span');
            texto.className = 'flex-grow-1 small';
            texto.textContent = `${i + 1}. ${parada.direccion}`;

            const item = document.createElement('li');
            item.className = 'd-flex align-items-center gap-2 py-1 border-bottom';
            item.append(
                boton('−', 'Quitar parada', 'btn btn-link link-danger text-decoration-none fw-bold p-0 px-1', () => {
                    paradas.splice(i, 1);
                    dibujarParadas();
                }),
                texto,
                boton('↑', 'Subir parada', 'btn btn-outline-secondary btn-sm py-0', () => mover(i, -1), i === 0),
                boton('↓', 'Bajar parada', 'btn btn-outline-secondary btn-sm py-0', () => mover(i, 1), i === paradas.length - 1),
                oculto(`Paradas[${i}].Direccion`, parada.direccion),
                oculto(`Paradas[${i}].Lat`, parada.lat),
                oculto(`Paradas[${i}].Lng`, parada.lng),
                oculto('__Invariant', `Paradas[${i}].Lat`),
                oculto('__Invariant', `Paradas[${i}].Lng`));
            return item;
        }));
    }

    function limpiarSeleccionParada() {
        paradaSeleccionada = null;
        botonAgregar.disabled = true;
        errorParada.textContent = '';
    }

    autocompletar(busquedaParada,
        direccion => {
            paradaSeleccionada = direccion;
            botonAgregar.disabled = false;
        },
        limpiarSeleccionParada);

    botonAgregar.addEventListener('click', () => {
        if (!paradaSeleccionada) {
            return;
        }

        const usadas = [origen.value, destino.value, ...paradas.map(p => p.direccion)];
        if (usadas.some(u => mismaDireccion(u, paradaSeleccionada.direccion))) {
            errorParada.textContent = 'La parada no puede repetirse ni coincidir con el origen o el destino.';
            return;
        }

        paradas.push(paradaSeleccionada);
        busquedaParada.value = '';
        limpiarSeleccionParada();
        dibujarParadas();
    });

    dibujarParadas();

    // Validaciones entre campos (en el servidor: DistintoDe, MayorQue y las coordenadas [Required] de CrearViajeDto).
    $.validator.addMethod('direccionSeleccionada', function (valor, elemento) {
        return this.optional(elemento) || document.querySelector(elemento.dataset.lat).value !== '';
    });

    $.validator.addMethod('distintoDe', function (valor, elemento, otro) {
        return this.optional(elemento) || !mismaDireccion(valor, $(otro).val());
    });

    // Las horas llegan como "HH:mm", así que se pueden comparar como texto.
    $.validator.addMethod('horaPosteriorA', function (valor, elemento, otro) {
        const otroValor = $(otro).val();
        return this.optional(elemento) || !otroValor || valor > otroValor;
    });

    // Después de que jquery.validate.unobtrusive arme el validador del form.
    $(function () {
        $(origen).rules('add', {
            direccionSeleccionada: true,
            messages: { direccionSeleccionada: 'Seleccioná un origen de la lista.' }
        });

        $(destino).rules('add', {
            direccionSeleccionada: true,
            distintoDe: '#OrigenDir',
            messages: {
                direccionSeleccionada: 'Seleccioná un destino de la lista.',
                distintoDe: 'El destino debe ser distinto del origen.'
            }
        });

        $('#HoraLlegada').rules('add', {
            horaPosteriorA: '#HoraSalida',
            messages: { horaPosteriorA: 'La hora de llegada debe ser posterior a la de salida.' }
        });

        $('#HoraSalida').on('change', () => validarSiTieneValor(document.getElementById('HoraLlegada')));
    });
})();
