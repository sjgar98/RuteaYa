(function () {
    'use strict';

    const CENTRO_POR_DEFECTO = { lat: -34.6037, lng: -58.3816 };
    let mapa;
    let Route;
    let polilineas = [];
    let marcadores = [];
    let numeroPedido = 0;

    function coordenada(idLat, idLng) {
        const lat = parseFloat(document.querySelector(idLat)?.value);
        const lng = parseFloat(document.querySelector(idLng)?.value);
        return Number.isFinite(lat) && Number.isFinite(lng) ? { lat, lng } : null;
    }

    function leerParadas() {
        return Array.from(document.querySelectorAll('#paradas-lista li'))
            .map(li => {
                const lat = parseFloat(li.querySelector('input[name$=".Lat"]')?.value);
                const lng = parseFloat(li.querySelector('input[name$=".Lng"]')?.value);
                return Number.isFinite(lat) && Number.isFinite(lng) ? { lat, lng } : null;
            })
            .filter(Boolean);
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
        const origen = coordenada('#OrigenLat', '#OrigenLng');
        const destino = coordenada('#DestinoLat', '#DestinoLng');
        const paradas = leerParadas();
        const pedido = ++numeroPedido;

        limpiar();
        if (!origen || !destino) {
            return;
        }

        try {
            const { routes } = await Route.computeRoutes({
                origin: origen,
                destination: destino,
                intermediates: paradas,
                travelMode: 'DRIVING',
                fields: ['path', 'viewport']
            });

            
            if (pedido !== numeroPedido || !routes?.length) {
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
        } catch (error) {
            console.warn('No se pudo calcular la ruta:', error);
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

        document.getElementById('form-viaje').addEventListener('ruta:cambio', calcularRuta);
        calcularRuta(); 
    };
})();