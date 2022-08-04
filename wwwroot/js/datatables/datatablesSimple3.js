window.addEventListener('DOMContentLoaded', event => {
    // Simple-DataTables
    // https://github.com/fiduswriter/Simple-DataTables/wiki

    const datatablesSimples = document.getElementById('datatablesSimples');
    if (datatablesSimples) {
        new simpleDatatables.DataTable(datatablesSimples);
    }
});