// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Comportamiento común a todas las pantallas.
(function () {
    const toastExito = document.getElementById('toast-exito');
    if (toastExito) {
        bootstrap.Toast.getOrCreateInstance(toastExito).show();
    }

    for (const elemento of document.querySelectorAll('[data-bs-toggle="tooltip"]')) {
        bootstrap.Tooltip.getOrCreateInstance(elemento);
    }
})();
