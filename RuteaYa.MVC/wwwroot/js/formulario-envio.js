(function () {
    const form = document.getElementById('form-envio');
    if (!form) {
        return;
    }

    const ESPERA_MS = 1000;
    const MIN_CARACTERES = 3;
    const urlDirecciones = form.dataset.urlDirecciones;

    const origen = document.getElementById('OrigenDir');
    const destino = document.getElementById('DestinoDir');
    function mismaDireccion(a, b) {
        return a.trim() !== '' && a.trim().toLowerCase() === b.trim().toLowerCase();
    }
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

    $.validator.addMethod('direccionSeleccionada', function (valor, elemento) {
        return this.optional(elemento) || document.querySelector(elemento.dataset.lat).value !== '';
    });

    $.validator.addMethod('distintoDe', function (valor, elemento, otro) {
        return this.optional(elemento) || !mismaDireccion(valor, $(otro).val());
    });

    $.validator.addMethod('fechaNoAnteriorAHoy', function (valor, elemento) {
        if (!valor) {
            return true;
        }
        const hoy = new Date().toISOString().slice(0, 10);
        return valor >= hoy;
    });

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

        $('#FechaLimite').rules('add', {
            fechaNoAnteriorAHoy: true,
            messages: { fechaNoAnteriorAHoy: 'La fecha no puede ser anterior a hoy.' }
        });
    });
})();