(function () {
    'use strict';

    const CENTRO_POR_DEFECTO = { lat: -34.6037, lng: -58.3816 };

    function mostrarEstado(texto) {
        const elemento = document.getElementById('mapa-comparacion-estado');
        if (elemento) {
            elemento.textContent = texto;
        }
    }

    window.initMapaComparacion = async function () {
        const contenedor = document.getElementById('mapa-comparacion');
        const datosVista = document.getElementById('mapa-comparacion-datos');
        if (!contenedor || !datosVista) {
            return;
        }

        try {
            const datos = JSON.parse(datosVista.textContent);

            const { Map } = await google.maps.importLibrary('maps');
            await google.maps.importLibrary('marker');
            const { encoding } = await google.maps.importLibrary('geometry');

            const habitual = encoding.decodePath(datos.habitual);
            const conEnvio = encoding.decodePath(datos.conEnvio);
            if (!habitual.length || !conEnvio.length) {
                mostrarEstado('No hay trazado de ruta disponible.');
                return;
            }

            const mapa = new Map(contenedor, {
                zoom: 12,
                center: CENTRO_POR_DEFECTO,
                mapTypeControl: false,
                streetViewControl: false
            });

            // La habitual va más gruesa y por debajo: donde las rutas coinciden se ve solo la azul,
            // y donde se separan (el desvío) se ve el gris de fondo.
            new google.maps.Polyline({
                map: mapa, path: habitual, strokeColor: '#6c757d', strokeOpacity: 0.7, strokeWeight: 8, zIndex: 1
            });
            new google.maps.Polyline({
                map: mapa, path: conEnvio, strokeColor: '#0d6efd', strokeOpacity: 1, strokeWeight: 4, zIndex: 2
            });

            new google.maps.Marker({ map: mapa, position: habitual[0], label: 'A', title: 'Origen' });
            new google.maps.Marker({ map: mapa, position: habitual[habitual.length - 1], label: 'B', title: 'Destino' });

            const limites = new google.maps.LatLngBounds();
            habitual.concat(conEnvio).forEach(p => limites.extend(p));
            mapa.fitBounds(limites);
        } catch (error) {
            console.warn('No se pudo dibujar la comparación de rutas:', error);
            mostrarEstado('No se pudo dibujar el mapa.');
        }
    };
})();