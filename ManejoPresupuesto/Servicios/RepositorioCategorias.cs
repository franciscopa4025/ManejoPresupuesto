using Dapper;
using ManejoPresupuesto.Models;
using System.Data.SqlClient;

namespace ManejoPresupuesto.Servicios
{

    public interface IRepositorioCategorias
    {
        Task Actualizar(Categoria categoria);
        Task Crear(Categoria categoria);
        Task<int> Contar(int usuarioId);
        Task Borrar(int id);
        Task<IEnumerable<Categoria>> Obtener(int usuarioId,PaginacionViewModel paginacion);
        Task<Categoria> ObtenerPorId(int id, int usuarioId);
        Task<IEnumerable<Categoria>> Obtener(int usuarioId, TipoOperacion tipoOperacionId);
    }
    public class RepositorioCategorias:IRepositorioCategorias
    {
        private readonly string connectionString;
        public RepositorioCategorias(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");

        }


        public async Task Crear(Categoria categoria)
        {
            using var connection = new SqlConnection(connectionString);
            var id = await connection.QuerySingleAsync<int>(@"INSERT INTO CATEGORIAS (Nombre,TipoOperacionId,UsuarioID) 
                                Values (@Nombre,@TipoOperacionId,@UsuarioId); 
                                SELECT SCOPE_IDENTITY();", categoria);
            categoria.Id = id;
        }

        public async Task<IEnumerable<Categoria>> Obtener(int usuarioId, PaginacionViewModel paginacion)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<Categoria>(
                @$"SELECT * 
                  FROM Categorias 
                  WHERE UsuarioId = @usuarioId
                  ORDER BY Nombre
                  OFFSET {paginacion.RecordsASaltar} ROWS FETCH NEXT {paginacion.RecordsPorPagina} 
                    ROWS ONLY", new { usuarioId });
        }

        public async Task<int> Contar(int usuarioId)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM Categorias WHERE UsuarioId = @usuarioId", new { usuarioId }
                );
        }
        public async Task<IEnumerable<Categoria>> Obtener(int usuarioId, TipoOperacion tipoOperacionId)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<Categoria>(
                @"SELECT * 
            FROM Categorias 
            WHERE UsuarioId = @usuarioId AND TipoOperacionId = @tipoOperacionId",
                new { usuarioId, tipoOperacionId });
        }

        public async Task<Categoria> ObtenerPorId(int id, int usuarioId)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryFirstOrDefaultAsync<Categoria>(
                        @"Select * FROM Categorias WHERE Id = @Id AND UsuarioId = @UsuarioId",
                        new { id, usuarioId });
        }

        public async Task Actualizar(Categoria categoria)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync(@"UPDATE Categorias 
                    SET Nombre = @Nombre, TipoOperacionId = @TipoOperacionID
                    WHERE Id = @Id", categoria);
        }

        public async Task Borrar(int id)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync("DELETE Categorias WHERE Id = @Id", new { id });
        }

    }
}
