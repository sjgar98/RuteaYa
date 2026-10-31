(function () {
    'use strict';

    const CENTRO_POR_DEFECTO = { lat: -34.6037, lng: -58.3816 };
    let mapa;
    let Route;
    let polilineas = [];
    let marcadores = [];
    let numeroPedido = 0;

    function mostrarEstado(texto) {
        const elemento = document.getElementById('mapa-estado');
        if (elemento) {
            elemento.textContent = texto;
        }
    }

    function punto(lat, lng) {
        const latitud = parseFloat(lat);
        const longitud = parseFloat(lng);
        return Number.isFinite(latitud) && Number.isFinite(longitud) ? { lat: latitud, lng: longitud } : null;
    }

    function leerRecorrido() {
        // Detalle: los datos vienen incrustados en la vista.
        const datosVista = document.getElementById('mapa-datos');
        if (datosVista) {
            const datos = JSON.parse(datosVista.textContent);
            return {
                origen: punto(datos.origen.lat, datos.origen.lng),
                destino: punto(datos.destino.lat, datos.destino.lng),
                paradas: datos.paradas.map(p => punto(p.lat, p.lng)).filter(Boolean)
            };
        }

        const valor = selector => document.querySelector(selector)?.value;
        return {
            origen: punto(valor('#OrigenLat'), valor('#OrigenLng')),
            destino: punto(valor('#DestinoLat'), valor('#DestinoLng')),
            paradas: Array.from(document.querySelectorAll('#paradas-lista li'))
                .map(li => punto(li.querySelector('input[name$=".Lat"]')?.value,
                                 li.querySelector('input[name$=".Lng"]')?.value))
                .filter(Boolean)
        };
    }

    function limpiar() {
        polilineas.forEach(p => p.setMap(null));
        marcadores.forEach(m => m.setMap(null));
        polilineas = [];
        marcadores = [];
    }

    function agregarMarcador(posicion, etiqueta, titulo) {
        marcadores.push(new google.maps.Marker({ map: mapa, position: posicion, label: etiqueta, title: titulo }));
    }

    async function calcularRuta() {
        const { origen, destino, paradas } = leerRecorrido();
        const pedido = ++numeroPedido;

        limpiar();
        if (!origen || !destino) {
            mostrarEstado('Elegí origen y destino de la lista para ver la ruta.');
            return;
        }

        mostrarEstado('Calculando ruta…');
        try {
            const { routes } = await Route.computeRoutes({
                origin: origen,
                destination: destino,
                intermediates: paradas,
                travelMode: 'DRIVING',
                fields: ['path', 'viewport']
            });

           
            if (pedido !== numeroPedido) {
                return;
            }
            if (!routes?.length) {
                mostrarEstado('No se encontró una ruta para este recorrido.');
                return;
            }

            const ruta = routes[0];
            polilineas = ruta.createPolylines();
            polilineas.forEach(p => p.setMap(mapa));

            agregarMarcador(origen, 'A', 'Origen');
            paradas.forEach((p, i) => agregarMarcador(p, String(i + 1), `Parada ${i + 1}`));
            agregarMarcador(destino, 'B', 'Destino');

            if (ruta.viewport) {
                mapa.fitBounds(ruta.viewport);
            }
            mostrarEstado('');
        } catch (error) {
            console.warn('No se pudo calcular la ruta:', error);
            if (pedido === numeroPedido) {
                mostrarEstado('No se pudo calcular la ruta. Probá de nuevo con "Actualizar ruta".');
            }
        }
    }

    window.initMapaViaje = async function () {
        const contenedor = document.getElementById('map');
        if (!contenedor) {
            return;
        }

        const { Map } = await google.maps.importLibrary('maps');
        await google.maps.importLibrary('marker');
        ({ Route } = await google.maps.importLibrary('routes'));

        mapa = new Map(contenedor, {
            zoom: 12,
            center: CENTRO_POR_DEFECTO,
            mapTypeControl: false,
            streetViewControl: false
        });

       
        document.getElementById('form-viaje')?.addEventListener('ruta:cambio', calcularRuta);
        document.getElementById('ruta-calcular')?.addEventListener('click', calcularRuta);

        calcularRuta();
    };
})();