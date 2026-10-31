// Oportunidades: al elegir otro viaje se recarga la lista.
(function () {
    const selector = document.getElementById('viajeId');
    if (selector) {
        selector.addEventListener('change', () => selector.form.submit());
    }
})();
