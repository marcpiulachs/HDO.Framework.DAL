using HDO.Application.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
    public abstract class DBContext
    {
        protected readonly IUnitOfWork uow;

        public DBContext (string name)
        {
            uow = UnitOfWorkFactory.Create(name);
        }

        public IUnitOfWork UnitOfWork
        {
            get { return uow; }
        }
    }
}
