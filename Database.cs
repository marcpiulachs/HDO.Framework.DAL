using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
    public class Db : IDbConnection, IDb
    {
        public static DbConnection conn;

        public DbProviderFactory DbProviderFactory
        {
            get;
            set;
        }

        public ConnectionStringSettings ConnectionStringSetting
        {
            get;
            set;
        }

        public Db(DbProviderFactory dbProviderFactory, ConnectionStringSettings connectionStringSettings)
        {
            DbProviderFactory = dbProviderFactory;
            ConnectionStringSetting = connectionStringSettings;
            ChangeDatabase(ConnectionStringSetting.ConnectionString);
        }


        #region IDbConnection 成员

        public IDbTransaction BeginTransaction(IsolationLevel il)
        {
            return conn.BeginTransaction(il);
        }

        public IDbTransaction BeginTransaction()
        {
            return conn.BeginTransaction();
        }

        public void ChangeDatabase(string databaseName)
        {
            conn = DbProviderFactory.CreateConnection();
            conn.ConnectionString = databaseName;
        }

        public void Close()
        {
            if (conn.State == ConnectionState.Open)
                conn.Close();
        }

        public string ConnectionString
        {
            get
            {
                return conn.ConnectionString;
            }
            set
            {
                conn.ConnectionString = value;
            }
        }

        public int ConnectionTimeout
        {
            get { return conn.ConnectionTimeout; }
        }

        public IDbCommand CreateCommand()
        {
            return conn.CreateCommand();
        }


        public IDbDataAdapter CreateDataAdapter()
        {
            return DbProviderFactory.CreateDataAdapter();
        }

        string IDbConnection.Database
        {
            get { return conn.Database; }
        }

        public void Open()
        {
            if (conn.State != ConnectionState.Open)
                conn.Open();

        }

        public ConnectionState State
        {
            get { return conn.State; }
        }

        #endregion

        #region IDisposable 成员

        public void Dispose()
        {
            conn.Close();
            conn.Dispose();
        }

        #endregion

        #region Command

        /// <summary>
        /// 执行查询，返回影响的行数
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public int ExecuteNonQuery(string sql)
        {
            using (IDbCommand com = CreateCommand())
            {
                com.CommandText = sql;
                return com.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// 执行查询，返回影响的行数
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public int ExecuteNonQuery(string sql, List<DbParameter> parameters)
        {
            using (IDbCommand com = CreateCommand())
            {
                com.CommandText = sql;
                if (parameters == null || parameters.Count == 0)
                    return 0;
                foreach (DbParameter parameter in parameters)
                    com.Parameters.Add(parameter);
                if (State != ConnectionState.Open)
                    throw new Exception("Database not opened");
                return com.ExecuteNonQuery();
            }
        }



        /// <summary>
        /// 执行查询返回第一行第一列的数据
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public object ExecuteScalar(string sql)
        {
            using (IDbCommand com = CreateCommand())
            {
                com.CommandText = sql;
                if (State != ConnectionState.Open)
                    throw new Exception("Database not opened");
                return com.ExecuteScalar();
            }
        }

        /// <summary>
        /// 执行查询返回第一行第一列的数据
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public object ExecuteScalar(string sql, List<DbParameter> parameters)
        {
            using (IDbCommand com = CreateCommand())
            {
                com.CommandText = sql;
                if (parameters == null || parameters.Count == 0)
                    return null;
                foreach (DbParameter parameter in parameters)
                    com.Parameters.Add(parameter);
                if (State != ConnectionState.Open)
                    throw new Exception("Database not opened");
                return com.ExecuteScalar();
            }
        }

        #endregion

        #region Tools

        public DbParameter GetParameter(string name, object value)
        {
            DbParameter dbParameter = DbProviderFactory.CreateParameter();
            dbParameter.ParameterName = name;
            dbParameter.Value = value;
            return dbParameter;
        }
        #endregion

        #region Query

        /// <summary>
        /// 将表转换为实体
        /// </summary>
        /// <typeparam name="T">实体类</typeparam>
        /// <param name="dt">数据</param>
        /// <returns></returns>
        public List<T> Query<T>(string sql) where T : class, new()
        {
            using (IDbCommand com = CreateCommand())
            {
                com.CommandText = sql;
                if (State != ConnectionState.Open)
                    throw new Exception("Database not opened");
                IDataReader dataReader = com.ExecuteReader();
                return FillModels<T>(dataReader);
            }
        }

        /// <summary>
        /// 将表转换为实体
        /// </summary>
        /// <typeparam name="T">实体类</typeparam>
        /// <param name="dt">数据</param>
        /// <returns></returns>
        public List<T> Query<T>(string sql, List<DbParameter> parameters) where T : class, new()
        {
            using (IDbCommand com = CreateCommand())
            {
                com.CommandText = sql;
                if (parameters == null || parameters.Count == 0)
                    return null;
                foreach (DbParameter parameter in parameters)
                    com.Parameters.Add(parameter);
                if (State != ConnectionState.Open)
                    throw new Exception("Database not opened");
                IDataReader dataReader = com.ExecuteReader();
                return FillModels<T>(dataReader);
            }
        }

        private List<T> FillModels<T>(IDataReader dr)
        {
            using (dr)
            {
                List<string> field = new List<string>(dr.FieldCount);
                for (int i = 0; i < dr.FieldCount; i++)
                {
                    field.Add(dr.GetName(i).ToLower());
                }
                List<T> list = new List<T>();
                while (dr.Read())
                {
                    T model = Activator.CreateInstance<T>();
                    foreach (PropertyInfo property in model.GetType().GetProperties(BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.Instance))
                    {
                        if (field.Contains(property.Name.ToLower()))
                        {
                            if (dr[property.Name] != DBNull.Value)
                                property.SetValue(model, Convert.ChangeType(dr[property.Name], property.PropertyType), null);
                        }
                    }
                    list.Add(model);
                }
                return list;
            }
        }
        #endregion
    }
}
