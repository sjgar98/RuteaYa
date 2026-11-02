// Mapa de comparación de rutas (Detalle de oportunidad), con Leaflet y tiles de OpenStreetMap.
(function () {
    'use strict';

    // La API codifica la polilínea con precisión 1e5. Si la ruta aparece corrida o lejos, probar con 6.
    const PRECISION = 5;
    const TILES_URL = 'https://tile.openstreetmap.org/{z}/{x}/{y}.png';
    const ATRIBUCION = '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a>';

    function mostrarEstado(texto) {
        const elemento = document.getElementById('mapa-comparacion-estado');
        if (elemento) {
            elemento.textContent = texto;
        }
    }

    function decodificar(codificado) {
        const factor = Math.pow(10, PRECISION);
        const puntos = [];
        let indice = 0;
        let lat = 0;
        let lng = 0;

        function leerValor() {
            let resultado = 0;
            let desplazamiento = 0;
            let bloque;
            do {
                bloque = codificado.charCodeAt(indice++) - 63;
                resultado |= (bloque & 0x1f) << desplazamiento;
                desplazamiento += 5;
            } while (bloque >= 0x20);
            return (resultado & 1) ? ~(resultado >> 1) : (resultado >> 1);
        }

        while (indice < codificado.length) {
            lat += leerValor();
            lng += leerValor();
            puntos.push([lat / factor, lng / factor]);
        }
        return puntos;
    }

    function pin(posicion, etiqueta, titulo) {
        const icono = L.divIcon({
            className: '',
            html: `<div class="mapa-pin">${etiqueta}</div>`,
            iconSize: [24, 24],
            iconAnchor: [12, 12]
        });
        return L.marker(posicion, { icon: icono, title: titulo });
    }

    function iniciar() {
        const contenedor = document.getElementById('mapa-comparacion');
        const datosVista = document.getElementById('mapa-comparacion-datos');
        if (!contenedor || !datosVista) {
            return;
        }

        try {
            const datos = JSON.parse(datosVista.textContent);
            const habitual = decodificar(datos.habitual);
            const conEnvio = decodificar(datos.conEnvio);
            if (!habitual.length || !conEnvio.length) {
                mostrarEstado('No hay trazado de ruta disponible.');
                return;
            }

            const mapa = L.map(contenedor);
            L.tileLayer(TILES_URL, { maxZoom: 19, attribution: ATRIBUCION }).addTo(mapa);

            // La habitual va más gruesa y por debajo: donde las rutas coinciden se ve solo la azul,
            // y donde se separan (el desvío) se ve el gris de fondo.
            L.polyline(habitual, { color: '#6c757d', weight: 8, opacity: 0.7 }).addTo(mapa);
            L.polyline(conEnvio, { color: '#0d6efd', weight: 4, opacity: 1 }).addTo(mapa);

            pin(habitual[0], 'A', 'Origen').addTo(mapa);
            pin(habitual[habitual.length - 1], 'B', 'Destino').addTo(mapa);

            mapa.fitBounds(L.latLngBounds(habitual.concat(conEnvio)), { padding: [20, 20] });
        } catch (error) {
            console.warn('No se pudo dibujar la comparación de rutas:', error);
            mostrarEstado('No se pudo dibujar el mapa.');
        }
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', iniciar);
    } else {
        iniciar();
    }
})();