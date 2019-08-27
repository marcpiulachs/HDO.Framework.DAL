using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Reflection;

using HDO.PointOfSale.Entities;

namespace HDO.Application.DAL
{
    public abstract class Repository<T> : IRepository<T> where T : IEntity, new()
    {
        protected readonly IUnitOfWork _uow;

        public Repository(IUnitOfWork uow)
        {
            _uow = uow;
        }

        protected string TableName
        {
            get { return string.Format("[DBO].[{0}]", typeof(T).Name); }
        }

        public virtual bool ExistsById(Guid id)
        {
            try
            {
                var sql = string.Format("SELECT COUNT(*) FROM {0} WHERE (Id = @id)", TableName);

                using (var cmd = _uow.CreateCommand())
                {
                    cmd.CommandText = sql;
                    cmd.Parameters.AddWithValue("@id", id);

                    return cmd.FirstOrDefault<int>() > 0;
                }
            }
            catch (Exception e)
            {
                throw new ApplicationException("An erorr has occurred with the SELECT.", e);
            }
        }

        /// <summary>
        /// Gets a single instance of a type by specifying the row Id.
        /// </summary>
        /// <returns>A specific instance of the specified type, or the default value for the type.</returns>
        public virtual T GetById(Guid id)
        {
            try
            {
                var sql = string.Format("SELECT * FROM {0} WHERE (Id = @id)", TableName);

                using (var cmd = _uow.CreateCommand())
                {
                    cmd.CommandText = sql;
                    cmd.Parameters.AddWithValue("@id", id);

                    return cmd.FirstOrDefault<T>();
                }
            }
            catch (Exception e)
            {
                throw new ApplicationException("An erorr has occurred with the SELECT.", e);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerable<T> GetAll()
        {
            try
            {
                var sql = string.Format("SELECT * FROM {0}", TableName);

                using (var cmd = _uow.CreateCommand())
                {
                    cmd.CommandText = sql;

                    return cmd.Query<T>();
                }
            }
            catch (Exception e)
            {
                throw new ApplicationException("An erorr has occurred with the SELECT.", e);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public virtual IEnumerable<T> GetAll(Func<T, bool> predicate)
        {
            return GetAll().Where(predicate);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entity"></param>
        public virtual void Add(T entity)
        {
            throw new NotImplementedException();
        }

        //public virtual void Update(T entity)
        //{
        //    throw new NotImplementedException();
        //}


        /// <summary>
        /// 
        /// </summary>
        /// <param name="entity"></param>
        public virtual void Delete(T entity)
        {
            try
            {
                var sql = string.Format("DELETE FROM {0} WHERE (Id = @id)", TableName);

                using (var cmd = _uow.CreateCommand())
                {
                    cmd.CommandText = sql;
                    cmd.Parameters.AddWithValue("@id", entity.Id);

                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception e)
            {
                throw new ApplicationException("An erorr has occurred with the DELETE.", e);
            }
        }

        /// <summary>
        /// Inspired by Dapper.Rainbow.
        /// </summary>
        public virtual void Insert(T entity)
        {
            int result = 0;

            // Entity parameters
            var paramNames = GetParamNames(entity);

            var sql = new StringBuilder();
            sql.AppendFormat("INSERT INTO [{0}]", typeof(T).Name);
            sql.AppendLine("(");
            for (var i = 0; i < paramNames.Count(); i++)
            {
                var column = paramNames.ElementAt(i);
                sql.AppendFormat("\n[{0}]", column);
                if (i != paramNames.Count() - 1)
                {
                    sql.Append(",");
                }
            }
            
            sql.AppendLine(")");
            sql.AppendLine("\nVALUES");
            sql.AppendLine("(");

            for (var i = 0; i < paramNames.Count(); i++)
            {
                var column = paramNames.ElementAt(i);
                sql.AppendFormat("\n'{0}'", GetPropertyValue<T>(entity, column));
                if (i != paramNames.Count() - 1)
                {
                    sql.Append(",");
                }
            }
            sql.AppendLine(")");

            using (var cmd = _uow.CreateCommand())
            {
                cmd.CommandText = sql.ToString();

                // Execute the SQL query
                result = cmd.ExecuteNonQuery();
            }

            if (result <= 0)
                throw new ApplicationException("Return value of INSERT should be greater than 0. An erorr has occurred with the INSERT.");
        }

        /// <summary>
        /// Inspired by Dapper.Rainbow.
        /// </summary>
        public virtual void Update(T entity)
        {
            int result = 0;
            
            // Entity parameters
            var paramNames = GetParamNames(entity);

            var sql = new StringBuilder();
            sql.AppendFormat("UPDATE [{0}] SET", TableName);

            for (var i = 0; i < paramNames.Count(); i++)
            {
                var column = paramNames.ElementAt(i);

                if (column != "Id")
                {
                    sql.AppendFormat("\n[{0}] = '{1}'", column, GetPropertyValue<T>(entity, column));
                    if (i != paramNames.Count() - 1)
                    {
                        sql.Append(",");
                    }
                }
            }

            sql.AppendLine(" WHERE (Id = @Id)");

            using (var cmd = _uow.CreateCommand())
            {
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddWithValue("@Id", entity.Id);
                
                // Execute the SQL query
                result = cmd.ExecuteNonQuery();
            }

            if (result <= 0)
                throw new ApplicationException("Return value of UPDATE should be greater than 0. An erorr has occurred with the UPDATE.");
        }

        public virtual void AddOrUpdate(T entity)
        {
            var exists = ExistsById(entity.Id);

            if (exists)
            {
                Update(entity);
            }
            else
            {
                Insert(entity);
            }
        }

        /// <summary>
        /// Modified from the Dapper.Rainbow source.
        /// </summary>
        private static IEnumerable<string> GetParamNames<TEntity>(TEntity entity)
        {
            return entity.GetType().GetProperties(BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.Public).Select(prop => prop.Name);
        }

        /// <summary>
        /// Function to get the Property Value
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="entity"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        private static string GetPropertyValue<TEntity>(TEntity entity, string propertyName)
        {
            return entity.GetType().GetProperty(propertyName).GetValue(entity, null) as string;
        }
    }

    public interface IRepository<T> where T : IEntity
    {
        T GetById(Guid id);
        IEnumerable<T> GetAll();
        void Add(T entity);
        void Update(T entity);
        void Delete(T entity);
    }

    /// <summary>
    /// 
    /// </summary>
    public interface IUnitOfWork: IDisposable
    {
        /// <summary>
        /// Gets the context.
        /// </summary>
        SqlCommand CreateCommand();

        /// <summary>
        /// Gets the context.
        /// </summary>
        /// <param name="commandText"></param>
        /// <returns></returns>
        SqlCommand CreateCommand(string commandText);

        /// <summary>
        /// The Commit.
        /// </summary>
        /// <returns>
        /// The <see cref="void"/>.
        /// </returns>
        void SaveChanges();
    }

    /// <summary>
    /// 
    /// </summary>
    public class UnitOfWorkFactory
    {
        public static IUnitOfWork Create()
        {
            var connection = new SqlConnection(ConnectionString);
            connection.Open();

            return new AdoDotNetUnitOfWork(connection, true);
        }

        public static IUnitOfWork CreateWtihConnectionName(string connectionName)
        {
            var connectionString = ConfigurationManager.ConnectionStrings[connectionName].ConnectionString;
            var connection = new SqlConnection(connectionString);
            connection.Open();

            return new AdoDotNetUnitOfWork(connection, true);
        }

        public static IUnitOfWork CreateWtihConnectionString(string connectionString)
        {
            var connection = new SqlConnection(connectionString);
            connection.Open();

            return new AdoDotNetUnitOfWork(connection, true);
        }

        private static string ConnectionString
        {
            get { return ConfigurationManager.ConnectionStrings["PuntoVentaContext"].ConnectionString; }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class AdoDotNetUnitOfWork : IUnitOfWork
    {
        private SqlConnection _connection;
        private SqlTransaction _transaction;
        private bool _ownsConnection;

        public AdoDotNetUnitOfWork(SqlConnection connection, bool ownsConnection)
        {
            _connection = connection;
            _ownsConnection = ownsConnection;
            _transaction = connection.BeginTransaction();
        }

        public SqlCommand CreateCommand()
        {
            var command = _connection.CreateCommand();
            command.Transaction = _transaction;
            return command;
        }

        public SqlCommand CreateCommand(string commandText)
        {
            var command = _connection.CreateCommand();
            command.Transaction = _transaction;
            command.CommandText = commandText;
            return command;
        }

        /// <summary>
        /// Persist changes
        /// </summary>
        public void SaveChanges()
        {
            if (_transaction == null)
            {
                throw new InvalidOperationException(
                    "Transaction have already been commited. Check your transaction handling.");
            }

            _transaction.Commit();
            _transaction = null;
        }

        public void Dispose()
        {
            if (_transaction != null)
            {
                _transaction.Rollback();
                _transaction = null;
            }

            if (_connection != null && _ownsConnection)
            {
                _connection.Close();
                _connection = null;
            }
        }
    }
}