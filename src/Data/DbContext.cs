using System.Data;
using Dapper;
using Npgsql;

namespace Mistral.Data
{
    public interface IDbContext : IDisposable
    {

        IDbConnection DbConnection { get; }

        /// <summary>
        /// Gets the transaction created with BeginTransaction.
        /// </summary>
        IDbTransaction? Transaction { get; }

        /// <summary>
        /// Begins a new transaction (if supported by the DbContext)
        /// </summary>
        void BeginTransaction();

        /// <summary>
        /// Commits the current transaction (if supported by the DbContext)
        /// </summary>
        void Commit();

        /// <summary>
        /// Executes a query against the database
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        Task<int> ExecuteNonQueryAsync(string sql, params IDbDataParameter[] dbParameters);

        /// <summary>
        /// Executes a query against the database using Dapper to substitute model values.
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        Task<int> ExecuteNonQueryAsync(string sql, object? param = null);

        /// <summary>
        /// Executes a query against the database
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        Task<T?> ExecuteScalarAsync<T>(string sql, object? param = null);

        /// <summary>
        /// Executes a query and maps the result to a strongly typed list.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        Task<IEnumerable<T>> QueryAsync<T>(string sql, object? param = null);

        /// <summary>
        /// Executes a query and maps the result to a strongly typed single object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        Task<T> QuerySingleAsync<T>(string sql, object? param = null);

        /// <summary>
        /// Executes a query and maps the result to a strongly typed single object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        Task<T?> QuerySingleOrDefaultAsync<T>(string sql, object? param = null);


        /// <summary>
        /// Rolls back the current transaction (if supported by the DbContext)
        /// </summary>
        void Rollback();

    }

    //public abstract class DbContext : IDbContext
    //{


    //    /// <summary>
    //    /// Begins a new transaction (if supported by the DbContext)
    //    /// </summary>
    //    public abstract void BeginTransaction();

    //    /// <summary>
    //    /// Commits the current transaction (if supported by the DbContext)
    //    /// </summary>
    //    public abstract void Commit();

    //    public virtual void Dispose()
    //    {

    //    }

    //    /// <summary>
    //    /// Executes a query against the database
    //    /// </summary>
    //    /// <param name="sql"></param>
    //    /// <param name="parameters"></param>
    //    public abstract Task<int> ExecuteNonQueryAsync(string sql, params IDbDataParameter[] dbParameters);

    //    /// <summary>
    //    /// Executes a query against the database using Dapper to substitute model values.
    //    /// </summary>
    //    /// <param name="sql"></param>
    //    /// <param name="param"></param>
    //    public abstract Task<int> ExecuteNonQueryAsync(string sql, object? param = null);

    //    /// <summary>
    //    /// Executes a query against the database
    //    /// </summary>
    //    /// <param name="sql"></param>
    //    /// <param name="parameters"></param>
    //    public abstract Task<T> ExecuteScalarAsync<T>(string sql, object? param = null);

    //    /// <summary>
    //    /// Executes a query and maps the result to a strongly typed list.
    //    /// </summary>
    //    /// <typeparam name="T"></typeparam>
    //    /// <param name="sql"></param>
    //    /// <param name="param"></param>
    //    /// <returns></returns>
    //    public abstract Task<IEnumerable<T>> QueryAsync<T>(string sql, object? param = null);

    //    /// <summary>
    //    /// Executes a query and maps the result to a strongly typed single object.
    //    /// </summary>
    //    /// <typeparam name="T"></typeparam>
    //    /// <param name="sql"></param>
    //    /// <param name="param"></param>
    //    /// <returns></returns>
    //    public abstract Task<T> QuerySingleAsync<T>(string sql, object? param = null);

    //    /// <summary>
    //    /// Executes a query and maps the result to a strongly typed single object.
    //    /// </summary>
    //    /// <typeparam name="T"></typeparam>
    //    /// <param name="sql"></param>
    //    /// <param name="param"></param>
    //    /// <returns></returns>
    //    public abstract Task<T> QuerySingleOrDefaultAsync<T>(string sql, object? param = null);

    //    /// <summary>
    //    /// Rolls back the current transaction (if supported by the DbContext)
    //    /// </summary>
    //    public abstract void Rollback();

    //}

    public class DbContext : IDbContext
    {
        private readonly string _connString;
        private NpgsqlConnection _conn;

        public DbContext(string connectionString)
        {
            this.Id = Guid.NewGuid();
            _connString = connectionString;

            var builder = new NpgsqlDataSourceBuilder(connectionString);
            builder.UseVector();
            var dataSource = builder.Build();
            _conn = dataSource.OpenConnection();
        }

        public IDbConnection DbConnection => _conn;

        /// <summary>
        /// Gets the transaction created with BeginTransaction.
        /// </summary>
        public IDbTransaction? Transaction { get; protected set; }


        /// <summary>
        /// Gets/sets the unique identifier of the DbContext - useful for debugging but serves no other practical purpose
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Begins a new transaction (if supported by the DbContext)
        /// </summary>
        public void BeginTransaction()
        {
            this.Transaction = _conn.BeginTransaction(IsolationLevel.Serializable);
        }

        /// <summary>
        /// Commits the current transaction (if supported by the DbContext)
        /// </summary>
        public void Commit()
        {
            if (this.Transaction == null)
            {
                throw new InvalidOperationException("Transaction has not been started");
            }
            this.Transaction.Commit();
        }

        public void Dispose()
        {
            if (_conn != null)
            {
                _conn.Dispose();
            }
        }

        /// <summary>
        /// Executes a query and maps the result to a strongly typed list.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public Task<IEnumerable<T>> QueryAsync<T>(string sql, object? param = null)
        {
            return _conn.QueryAsync<T>(sql, param, this.Transaction);
        }

        /// <summary>
        /// Executes a query and maps the result to a strongly typed single object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public Task<T> QuerySingleAsync<T>(string sql, object? param = null)
        {
            return _conn.QuerySingleAsync<T>(sql, param, this.Transaction);
        }

        /// <summary>
        /// Executes a query and maps the result to a strongly typed single object, returning the default value if the object 
        /// is not found.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public Task<T?> QuerySingleOrDefaultAsync<T>(string sql, object? param = null)
        {
            return _conn.QuerySingleOrDefaultAsync<T>(sql, param, this.Transaction);
        }

        /// <summary>
        /// Executes a query against the database
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        public Task<int> ExecuteNonQueryAsync(string sql, params IDbDataParameter[] dbParameters)
        {
            using (var cmd = new NpgsqlCommand(sql, _conn))
            {
                cmd.Parameters.AddRange(dbParameters);
                return cmd.ExecuteNonQueryAsync();
            }
        }

        /// <summary>
        /// Executes a query against the database
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        public Task<T?> ExecuteScalarAsync<T>(string sql, object? param = null)
        {
            return _conn.ExecuteScalarAsync<T>(sql, param, this.Transaction);
        }

        /// <summary>
        /// Executes a query against the database using Dapper to substitute model values.
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        public Task<int> ExecuteNonQueryAsync(string sql, object? param = null)
        {
            return _conn.ExecuteAsync(sql, param, this.Transaction);
        }

        /// <summary>
        /// Rolls back the current transaction (if supported by the DbContext)
        /// </summary>
        public void Rollback()
        {
            if (this.Transaction == null)
            {
                throw new InvalidOperationException("Transaction has not been started");
            }
            this.Transaction.Rollback();
        }

    }
}
