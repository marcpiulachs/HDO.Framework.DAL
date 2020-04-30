using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;

namespace HDO.Application.DAL
{
    /// <summary>
    /// A light weight object mapper for ADO.NET
    /// </summary>
    public static class SqlMapper
    {
        /// <summary>
        /// Returns the first element of a sequence, or a default value if the sequence contains no elements.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="command"></param>
        /// <returns></returns>
        public static T FirstOrDefault<T>(this SqlCommand command)
        {
            return Query<T>(command).FirstOrDefault();
        }

        /// <summary>
        /// Determines whether a sequence contains any elements.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="command"></param>
        /// <returns></returns>
        public static bool Any<T>(this SqlCommand command)
        {
            return Query<T>(command).Any();
        }

        /// <summary>
        /// Determines whether a sequence contains any elements.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="command"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public static bool Any<T>(this SqlCommand command, dynamic param = null)
        {
            return Query<T>(command, param).Any();
        }

        /// <summary>
        /// Returns the number of elements in a sequence.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="command"></param>
        /// <returns></returns>
        public static int Count<T>(this SqlCommand command)
        {
            return Query<T>(command).Count();
        }

        /// <summary>
        /// Execute a query and map it to a list of dynamic objects
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="command"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static IEnumerable<T> Query<T>(this SqlCommand command, Func<T, bool> predicate)
        {
            return Query<T>(command).Where(predicate);
        }

        /// <summary>
        /// Execute a query and map it to a list of dynamic objects
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="command"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public static IEnumerable<T> Query<T>(this SqlCommand command, dynamic param = null)
        {
            foreach (var prop in param.GetType().GetProperties())
            {
                var value = prop.GetValue(param, null);
                
                if (value == null)
                    value = DBNull.Value;

                command.Parameters.AddWithValue("@" + prop.Name, value);
            }

            return Query<T>(command);
        }

        /// <summary>
        /// Execute a query and map it to a list of dynamic objects
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="command"></param>
        /// <returns></returns>
        public static List<T> ExecuteQuery<T>(this SqlCommand command)
        {
            return Query<T>(command).ToList();
        }

        /// <summary>
        /// Execute a query and map it to a list of dynamic objects
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="command"></param>
        /// <returns></returns>
        public static IEnumerable<T> Query<T>(this SqlCommand command)
        {
            if (command.Connection.State == ConnectionState.Closed)
                command.Connection.Open();

            using (var reader = command.ExecuteReader())
            {
                if (reader != null && reader.HasRows)
                {
                    while (reader.Read())
                    {
                        yield return MapEntity<T>(reader);
                    }
                }
            }
        }

        /// <summary>
        /// Execute a query and map it to a list of dynamic objects
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="command"></param>
        /// <param name="mapper"></param>
        /// <returns></returns>
        private static IEnumerable<T> Query<T>(this SqlCommand command, Func<SqlDataReader, T> mapper)
        {
            using (var reader = command.ExecuteReader())
            {
                if (reader != null && reader.HasRows)
                {
                    while (reader.Read())
                    {
                        yield return mapper(reader);
                    }
                }
            }
        }

        /// <summary>
        /// Executes a Transact-SQL statement against the connection and returns the number of rows affected.
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public static int ExecuteNonQuery(this SqlCommand command)
        {
            if (command.Connection.State == ConnectionState.Closed)
                command.Connection.Open();

            return command.ExecuteNonQuery();
        }
        
        /// <summary>
        /// Read value and return default if empty.
        /// </summary>
        /// <typeparam name="T">The type to read</typeparam>
        /// <param name="value">The database field.</param>
        /// <returns>An instance of an object with default values or value from database.</returns>
        public static T Read<T>(object value)
        {
            if (value != DBNull.Value)
            {
                return (T)value;
            }

            return default(T);
        }
        
        /// <summary>
        /// Maps the entity from de <see cref="SqlDataReader"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="reader"></param>
        /// <returns></returns>
        private static T MapEntity<T>(SqlDataReader reader)
        {
            // Creates an instance of the entity class
            T entity = Activator.CreateInstance<T>();
            
            // Reads the datareader and populates the newly created object
            PopulateClass<T>(entity, reader);

            // Returns the database populated entity
            return entity;
        }

        /// <summary>
        /// Encode a variable for use with the SQL LIKE operator.
        /// </summary>
        public static string EncodeForLike(string term)
        {
            return "%" + term.Replace("%", "[%]").Replace("[", "[[]").Replace("]", "[]]") + "%";
        }

        /// <summary>
        /// Returns the column names for the given <see cref="IDataRecord"/>.
        /// </summary>
        /// <param name="reader">The <see cref="IDataRecord"/>.</param>
        /// <returns>An array containing the column names.</returns>
        private static IEnumerable<string> GetColumnNames(IDataRecord reader)
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                yield return reader.GetName(i);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <param name="property"></param>
        /// <param name="value"></param>
        private static void SetPropertyValue<T>(T entity, string property, object value)
        {
            // Contains the property setter for the property to be set.
            PropertyInfo propertyInfo = null;

            // The object to set the property
            object target = entity;

            if (entity == null)
                throw new ArgumentNullException("entity");

            if (string.IsNullOrEmpty(property))
                throw new ArgumentNullException("property");

            // If the value is null then we do nothing and return.
            if ((value == DBNull.Value) || (value == null))
                return;

            if (property.Contains('.'))
            {
                string[] propertyNames = property.Split('.');

                for (var i = 0; i < propertyNames.Length - 1; i++)
                {
                    string propertyName = propertyNames[i];

                    propertyInfo = target.GetType().GetProperty(propertyName);

                    if (propertyInfo != null)
                    {
                        target = propertyInfo.GetValue(target, null);
                    }

                    if (target == null)
                        throw new KeyNotFoundException(string.Format("Property '{0}' on class '{1}' is set to a null object, an instance of an object is required.", propertyInfo.Name, entity.GetType().Name));
                }

                propertyInfo = target.GetType().GetProperty(propertyNames.Last());
            }
            else
            {
                propertyInfo = target.GetType().GetProperty(property);
            }

            if (value == DBNull.Value)
                return;

            if (propertyInfo == null)
                throw new KeyNotFoundException(string.Format("Column '{0}' in DataReader does not match any property", property));

            if (!propertyInfo.CanRead)
                throw new KeyNotFoundException(string.Format("Property '{0}' is readonly", property));

            if (!propertyInfo.CanWrite)
                throw new KeyNotFoundException(string.Format("Property '{0}' is readonly", property));

            Type columnType = value.GetType();
            Type actualType = Nullable.GetUnderlyingType(propertyInfo.PropertyType) ?? propertyInfo.PropertyType;

            
            // Check for a directly assignable type
            if (actualType == columnType || actualType.Equals(columnType))
            {
                propertyInfo.SetValue(target, value, null);
            }
            else
            {
                if (actualType.IsSubclassOf(typeof(Enum)))
                {
                    value = Enum.Parse(actualType, value.ToString());
                    //propertyInfo.SetValue(target, value, null);
                }
                else
                {
                    value = Convert.ChangeType(value, propertyInfo.PropertyType);
                    //propertyInfo.SetValue(target, value, null);
                }

                propertyInfo.SetValue(target, value, null);
            }
        }

        /// <summary>
        /// Populate a references type from an IDataRecord by matching column names to property names.
        /// </summary>
        private static void PopulateClass<T>(T entity, IDataRecord record)
        {
            if (!Validate(record))
                return;

            // Gets all the column names from the data reader.
            var columnNames = GetColumnNames(record);

            // Only set properties which match column names in the result.
            foreach (string columnName in columnNames)
            {
                //string colName = columnName;
                object value = record[columnName];

                // Sets the property value to the value from the database
                SetPropertyValue<T>(entity, columnName, value);
            }
        }

        /// <summary>
        /// Fills the public properties of a class from a DataRow where the name
        /// of the property matches a column name from that DataRow.
        /// </summary>
        /// <param name="row">A IDataRecord that contains the data.</param>
        /// <returns>A class of type T with its public properties set to the
        ///      data from the matching columns in the DataRow.</returns>
        private static T MapEntity<T>(IDataRecord row) where T : class, new()
        {
            T result = new T();
            Type classType = typeof(T);

            // Defensive programming, make sure there are properties to set,
            //   and columns to set from and values to set from.
            if ((row.FieldCount < 1) || (classType.GetProperties().Length < 1))
            {
                return result;
            }

            foreach (PropertyInfo property in classType.GetProperties())
            {
                // Gets all the column names from the data reader.
                var columnNames = GetColumnNames(row);

                // Only set properties which match column names in the result.
                foreach (string columnName in columnNames)
                {
                    // Skip if Property name and ColumnName do not match
                    if (property.Name != columnName)
                        continue;

                    // This would throw if we tried to convert it below
                    if (row[columnName] == DBNull.Value)
                        continue;

                    object newValue;

                    // If type is of type System.Nullable, do not attempt to convert the value
                    if (IsNullable(property.PropertyType))
                    {
                        newValue = row[property.Name];
                    }
                    else
                    {   // Convert row object to type of property
                        newValue = Convert.ChangeType(row[columnName], property.PropertyType);
                    }

                    // This is what sets the class properties of the class
                    property.SetValue(result, newValue, null);
                }
            }

            return result;
        }

        /// <summary>
        /// Checks a DataTable for empty rows, columns or null.
        /// </summary>
        /// <param name="record">The DataTable to check.</param>
        /// <returns>True if DataTable has data, false if empty or null.</returns>
        public static bool Validate(IDataRecord record)
        {
            if (record == null)
                return false;

            if (record.FieldCount == 0)
                return false;

            return true;
        }

        /// <summary>
        /// Checks if type is nullable, Nullable<T> or its reference is nullable.
        /// </summary>
        /// <param name="type">Type to check for nullable.</param>
        /// <returns>True if type is nullable, false if it is not.</returns>
        private static bool IsNullable(Type type)
        {
            if (!type.IsValueType)
                return true; // ref-type

            if (Nullable.GetUnderlyingType(type) != null)
                return true; // Nullable<T>

            return false; // value-type
        }
    }
}