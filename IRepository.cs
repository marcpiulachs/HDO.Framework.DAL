﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace HDO.Application.DAL
{
    public interface IRepository<T> where T : class, new()
    {
        /// <summary>
        /// Gets all entities
        /// </summary>        
        /// <returns>All entities</returns>
        IEnumerable<T> GetAll();

        /// <summary>
        /// Gets all entities matching the predicate
        /// </summary>
        /// <param name="predicate">The filter clause</param>
        /// <returns>All entities matching the predicate</returns>
        IEnumerable<T> GetAll(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Set based on where condition
        /// </summary>
        /// <param name="predicate">The predicate</param>
        /// <returns>The records matching the given condition</returns>
        IQueryable<T> Where(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Finds an entity matching the predicate
        /// </summary>
        /// <param name="predicate">The filter clause</param>
        /// <returns>An entity matching the predicate</returns>
        IEnumerable<T> Find(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Determines if there are any entities matching the predicate
        /// </summary>
        /// <param name="predicate">The filter clause</param>
        /// <returns>True if a match was found</returns>
        bool Any(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Returns the first entity that matches the predicate
        /// </summary>
        /// <param name="predicate">The filter clause</param>
        /// <returns>An entity matching the predicate</returns>
        T First(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Returns the last entity that matches the predicate
        /// </summary>
        /// <param name="predicate">The filter clause</param>
        /// <returns>An entity matching the predicate</returns>
        T Last(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Returns the first entity that matches the predicate else null
        /// </summary>
        /// <param name="predicate">The filter clause</param>
        /// <returns>An entity matching the predicate else null</returns>
        T FirstOrDefault(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Adds a given entity to the context
        /// </summary>
        /// <param name="entity">The entity to add to the context</param>
        void Add(T entity);

        /// <summary>
        /// Updates a given entity to the context
        /// </summary>
        /// <param name="entity">The entity to update to the context</param>
        void Update(T entity);

        /// <summary>
        /// Deletes a given entity from the context
        /// </summary>
        /// <param name="entity">The entity to delete</param>
        void Delete(T entity);

        /// <summary>
        /// Attaches a given entity to the context
        /// </summary>
        /// <param name="entity">The entity to attach</param>
        void Attach(T entity);

        void Detach(T entity);

        IEnumerable<T> Get(
            Expression<Func<T, bool>> filter = null,
            Func<IQueryable<T>,
                IOrderedQueryable<T>> orderBy = null,
            List<Expression<Func<T, object>>>
                includeProperties = null,
            int? page = null,
            int? pageSize = null);

        /// <summary>
        /// Execute stored procedures and dynamic sql
        /// </summary>
        void SqlQuery(string sql, params object[] parameters);
    }
}