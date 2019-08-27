using System;
using System.Collections;
using System.Collections.Generic;
//using System.Data.Entity;
//using System.Data.Entity.Validation;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HDO.Application.DAL
{
    /// <summary>
    /// The Entity Framework implementation of IUnitOfWork
    /// </summary>
    public sealed class UnitOfWork : IUnitOfWork
    {
        /// <summary>
        /// The DbContext
        /// </summary>
        //private DbContext _dbContext;

        /// <summary>
        /// List of <see cref="IRepository"/> instances.
        /// </summary>
        private Hashtable _repositories;

        /// <summary>
        /// Initializes a new instance of the UnitOfWork class.
        /// </summary>
        /// <param name="context">The object context</param>
        //public UnitOfWork(DbContext context)
        //{
        //    _dbContext = context;
        //}

        /// <summary>
        /// Saves all pending changes
        /// </summary>
        /// <returns>The number of objects in an Added, Modified, or Deleted state</returns>
        public int Commit()
        {
            try
            {
                throw new NotImplementedException();
                // Your code...
                // Could also be before try if you know the exception occurs in SaveChanges

                //return _dbContext.SaveChanges();
            }
            catch (Exception e)
            {
                //foreach (var eve in e.EntityValidationErrors)
                //{
                //    Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                //        eve.Entry.Entity.GetType().Name, eve.Entry.State);
                //    foreach (var ve in eve.ValidationErrors)
                //    {
                //        Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                //            ve.PropertyName, ve.ErrorMessage);
                //    }
                //}
                throw;
            }
        }

        /// <summary>
        /// Disposes the current object
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Gets a <see cref="IRepository"/> instance.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IRepository<T> Repository<T>() where T : class, new()
        {
            throw new NotImplementedException();

            //if (_repositories == null)
            //    _repositories = new Hashtable();

            //var type = typeof(T).Name;

            //if (!_repositories.ContainsKey(type))
            //{
            //    var repositoryType = typeof(GenericRepository<>);

            //    var repositoryInstance =
            //        Activator.CreateInstance(repositoryType
            //                .MakeGenericType(typeof(T)), _dbContext);

            //    _repositories.Add(type, repositoryInstance);
            //}

            //return (IRepository<T>)_repositories[type];
        }

        /// <summary>
        /// Disposes all external resources.
        /// </summary>
        /// <param name="disposing">The dispose indicator.</param>
        private void Dispose(bool disposing)
        {
            if (disposing)
            {
            }
        }
    }
}
