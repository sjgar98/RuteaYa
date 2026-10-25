namespace RuteaYa.Repositorios.Interfaces
{
    public interface IRepositorio<T> where T : class
    {
        Task<T?> ObtenerPorIdAsync(int id);
        Task<List<T>> ObtenerTodosAsync();
        Task AgregarAsync(T entidad);
        void Actualizar(T entidad);
        void Eliminar(T entidad);
        Task<int> GuardarCambiosAsync();
    }
}
