// Mapa del recorrido (Nuevo/Editar viaje y Detalle del viaje), con Leaflet y tiles de OpenStreetMap.
(function () {
    'use strict';

    const CENTRO_POR_DEFECTO = [-34.6037, -58.3816];
    // La API codifica la polilínea con precisión 1e5. Si la ruta aparece corrida o lejos, probar con 6.
    const PRECISION = 5;
    const TILES_URL = 'https://tile.openstreetmap.org/{z}/{x}/{y}.png';
    const ATRIBUCION = '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a>';

    let mapa;
    let capas;
    let urlRuta;
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

        // Formulario: coordenadas ocultas que completa formulario-viaje.js.
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

    // Única parte que depende del contrato de Viajes/CalcularRuta: GET con "puntos=lat,lng;lat,lng;..." y respuesta con "polyline".
    async function pedirTrazado(puntos) {
        const lista = puntos.map(p => `${p.lat},${p.lng}`).join(';');
        const respuesta = await fetch(`${urlRuta}?puntos=${encodeURIComponent(lista)}`);
        if (!respuesta.ok) {
            throw new Error(`Viajes/CalcularRuta respondió ${respuesta.status}`);
        }

        const resultado = await respuesta.json();
        if (resultado.polyline) {
            return decodificar(resultado.polyline);
        }
        return (resultado.puntosRecorrido || []).map(p => [p.latitud ?? p.lat, p.longitud ?? p.lng]);
    }

    async function calcularRuta() {
        const { origen, destino, paradas } = leerRecorrido();
        const pedido = ++numeroPedido;

        capas.clearLayers();
        if (!origen || !destino) {
            mostrarEstado('Elegí origen y destino de la lista para ver la ruta.');
            return;
        }

        const puntos = [origen, ...paradas, destino];
        const latLngs = puntos.map(p => [p.lat, p.lng]);

        // Los marcadores se dibujan enseguida; el trazado llega después del servidor.
        pin(latLngs[0], 'A', 'Origen').addTo(capas);
        paradas.forEach((p, i) => pin([p.lat, p.lng], String(i + 1), `Parada ${i + 1}`).addTo(capas));
        pin(latLngs[latLngs.length - 1], 'B', 'Destino').addTo(capas);
        mapa.fitBounds(L.latLngBounds(latLngs), { padding: [30, 30] });

        mostrarEstado('Calculando ruta…');
        try {
            const trazado = await pedirTrazado(puntos);
            if (pedido !== numeroPedido) {
                return;
            }
            if (!trazado.length) {
                throw new Error('La respuesta no trae trazado.');
            }

            L.polyline(trazado, { color: '#0d6efd', weight: 5, opacity: 0.9 }).addTo(capas);
            mapa.fitBounds(L.latLngBounds(trazado), { padding: [30, 30] });
            mostrarEstado('');
        } catch (error) {
            console.warn('No se pudo calcular el trazado:', error);
            if (pedido === numeroPedido) {
                L.polyline(latLngs, { color: '#6c757d', weight: 3, dashArray: '6 8' }).addTo(capas);
                mostrarEstado('No se pudo calcular el trazado: se muestra una línea recta entre los puntos.');
            }
        }
    }

    function iniciar() {
        const contenedor = document.getElementById('map');
        if (!contenedor) {
            return;
        }
        urlRuta = contenedor.dataset.urlRuta;

        mapa = L.map(contenedor).setView(CENTRO_POR_DEFECTO, 12);
        L.tileLayer(TILES_URL, { maxZoom: 19, attribution: ATRIBUCION }).addTo(mapa);
        capas = L.layerGroup().addTo(mapa);

        // Solo existen en el formulario.
        document.getElementById('form-viaje')?.addEventListener('ruta:cambio', calcularRuta);
        document.getElementById('ruta-calcular')?.addEventListener('click', calcularRuta);

        calcularRuta(); // Detalle y Editar ya traen datos
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', iniciar);
    } else {
        iniciar();
    }
})();