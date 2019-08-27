using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using PuntoVenta.Entities;
using DapperLite;

namespace HDO.Application.DAL
{
    public interface IDatabaseContext
    {
        SqlConnection Connection { get; }
        void Dispose();
    }

    public interface IDatabaseContextFactory
    {
        IDatabaseContext Context();
    }

    public class DatabaseContextFactory : IDatabaseContextFactory
    {
        public IDatabaseContext Context()
        {
            return new DatabaseContext();
        }
    }

    public interface IUnitOfWork
    {
        /// <summary>
        /// Gets the context.
        /// </summary>
        IDatabaseContext DataContext { get; }
        SqlTransaction BeginTransaction();

        /// <summary>
        /// The Commit.
        /// </summary>
        /// <returns>
        /// The <see cref="void"/>.
        /// </returns>
        void Commit();

    }

    public interface IRepository<T> where T : IEntity
    {
        int Insert(T entity, string insertSql, SqlTransaction sqlTransaction);
        int Update(T entity, string updateSql, SqlTransaction sqlTransaction);
        int Delete(int id, string deleteSql, SqlTransaction sqlTransaction);
        T GetById(Guid id);
        IEnumerable<T> GetAll(string getAllSql);

        void Add(T entity);
        void Update(T entity);
        void Delete(T entity);

        IEnumerable<T> GetAll();

        T FirstOrDefault(string sql);

        T GetSingleDTO(SqlCommand command);
    }

    public class DatabaseContext : IDatabaseContext
    {
        private readonly string _connectionString;
        private SqlConnection _connection;

        /// <summary>
        /// Get connection string inside constructor.
        /// So when the class will be initialized then connection string will be automatically get from web.config
        /// </summary>
        public DatabaseContext()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["PuntoVentaContext"].ConnectionString;
        }

        /// <summary>
        /// Gets the connection.
        /// </summary>
        public SqlConnection Connection
        {
            get
            {
                if (_connection == null)
                {
                    _connection = new SqlConnection(_connectionString);
                }
                if (_connection.State != ConnectionState.Open)
                {
                    _connection.Open();
                }
                return _connection;
            }
        }

        /// <summary>
        /// Dispose Connection
        /// </summary>
        public void Dispose()
        {
            if (_connection != null && _connection.State == ConnectionState.Open)
                _connection.Close();
        }
    }

    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        private IDatabaseContextFactory _factory;
        private IDatabaseContext _context;
        public SqlTransaction Transaction { get; private set; }

        /// <summary>
        /// Constructor which will initialize the datacontext factory
        /// </summary>
        /// <param name="factory">datacontext factory</param>
        public UnitOfWork(IDatabaseContextFactory factory)
        {
            _factory = factory;
        }

        /// <summary>
        /// Following method will use to Commit or Rollback memory data into database
        /// </summary>
        public void Commit()
        {
            if (Transaction != null)
            {
                try
                {
                    Transaction.Commit();
                }
                catch (Exception)
                {
                    Transaction.Rollback();
                }
                Transaction.Dispose();
                Transaction = null;
            }
            else
            {
                throw new NullReferenceException("Tryed commit not opened transaction");
            }
        }

        /// <summary>
        /// Define a property of context class
        /// </summary>
        public IDatabaseContext DataContext
        {
            get { return _context ?? (_context = _factory.Context()); }
        }

        /// <summary>
        /// Begin a database transaction
        /// </summary>
        /// <returns>Transaction</returns>
        public SqlTransaction BeginTransaction()
        {
            if (Transaction != null)
            {
                throw new NullReferenceException("Not finished previous transaction");
            }
            Transaction = _context.Connection.BeginTransaction();
            return Transaction;
        }

        /// <summary>
        /// dispose a Transaction.
        /// </summary>
        public void Dispose()
        {
            if (Transaction != null)
            {
                Transaction.Dispose();
            }
            if (_context != null)
            {
                _context.Dispose();
            }
        }
    }

    public abstract class BaseRepository<T> : IRepository<T> where T : IEntity<Guid>, new()
    {
        private SqlConnection _conn;
        protected readonly IUnitOfWork _uow;
        //protected readonly DataReaderMapper<T> mapper = new DataReaderMapper<T>();

        SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["PuntoVentaContext"].ConnectionString);

        protected SqlCeDatabase<Guid> db = null;

        /// <summary>
        /// Initialize the connection
        /// </summary>
        /// <param name="uow">UnitOfWork</param>
        public BaseRepository(IUnitOfWork uow)
        {
            if (uow == null)
                throw new ArgumentNullException("unitOfWork");

            _uow = uow;
            _conn = _uow.DataContext.Connection;

            // The type we pass in (Guid) is the type of the Id column that is assumed to be present in every table.
            db = new SqlCeDatabase<Guid>(conn);
            // Calling Init() automatically generates a table name map, used to map type names to table names.
            // e.g. for the type "Dog", it will first search for a table name == "Dog", then (pluralized) "Dogs"
            db.Init();
        }

        /// <summary>
        /// Base Method for Insert Data
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="insertSql"></param>
        /// <param name="sqlTransaction"></param>
        /// <returns></returns>
        public virtual void Insert(T entity, string insertSql, SqlTransaction sqlTransaction)
        {
            try
            {
                using (var cmd = _conn.CreateCommand())
                {
                    cmd.CommandText = insertSql;
                    cmd.CommandType = CommandType.Text;
                    cmd.Transaction = sqlTransaction;
                    InsertCommandParameters(entity, cmd);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Base Method for Update Data
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="updateSql"></param>
        /// <param name="sqlTransaction"></param>
        /// <returns></returns>
        public virtual void Update(T entity, string updateSql, SqlTransaction sqlTransaction)
        {
            using (var cmd = _conn.CreateCommand())
            {
                cmd.CommandText = updateSql;
                cmd.CommandType = CommandType.Text;
                cmd.Transaction = sqlTransaction;
                UpdateCommandParameters(entity, cmd);
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Base Method for Delete Data
        /// </summary>
        /// <param name="id"></param>
        /// <param name="deleteSql"></param>
        /// <param name="sqlTransaction"></param>
        /// <returns></returns>
        public virtual void Delete(int id, string deleteSql, SqlTransaction sqlTransaction)
        {
            using (var cmd = _conn.CreateCommand())
            {
                cmd.CommandText = deleteSql;
                cmd.CommandType = CommandType.Text;
                cmd.Transaction = sqlTransaction;
                DeleteCommandParameters(id, cmd);
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Base Method for Populate Data by key
        /// </summary>
        /// <param name="id"></param>
        /// <param name="getByIdSql"></param>
        /// <returns></returns>
        public virtual T GetById(Guid Id)
        {
            return db.Get<T>(Id);
        }

        /// <summary>
        /// Base Method for Populate Data by key
        /// </summary>
        /// <param name="id"></param>
        /// <param name="getByIdSql"></param>
        /// <returns></returns>
        public virtual T FirstOrDefault(string sql)
        {
            throw new NotImplementedException();
            //using (var cmd = _conn.CreateCommand())
            //{
            //    cmd.CommandText = sql;
            //    cmd.CommandType = CommandType.Text;
            //    using (SqlDataReader reader = cmd.ExecuteReader())
            //    {
            //        return mapper.MapRowAll(reader);
            //    }

            //}
        }

        /// <summary>
        /// Base Method for Populate All Data
        /// </summary>
        /// <param name="getAllSql"></param>
        /// <returns></returns>
        public IEnumerable<T> GetAll(string getAllSql)
        {
            throw new NotImplementedException();
        }

        protected virtual void InsertCommandParameters(T entity, SqlCommand cmd) { }
        protected virtual void UpdateCommandParameters(T entity, SqlCommand cmd) { }
        protected virtual void DeleteCommandParameters(int id, SqlCommand cmd) { }
        protected virtual void GetByIdCommandParameters(int id, SqlCommand cmd) { }

        int IRepository<T>.Insert(T entity, string insertSql, SqlTransaction sqlTransaction)
        {
            throw new NotImplementedException();
        }

        int IRepository<T>.Update(T entity, string updateSql, SqlTransaction sqlTransaction)
        {
            throw new NotImplementedException();
        }

        int IRepository<T>.Delete(int id, string deleteSql, SqlTransaction sqlTransaction)
        {
            throw new NotImplementedException();
        }


        public void Add(T entity)
        {
            throw new NotImplementedException();
        }

        public void Update(T entity)
        {
            throw new NotImplementedException();
        }

        public void Delete(T entity)
        {
            throw new NotImplementedException();
        }

        public virtual IEnumerable<T> GetAll()
        {
            return db.All<T>();
        }

        // GetSingleDTO
        public T GetSingleDTO(SqlCommand command)
        {
            throw new NotImplementedException();
        }
    }
}
