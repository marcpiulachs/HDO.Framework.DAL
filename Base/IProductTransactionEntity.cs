using PuntoVenta.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PuntoVenta.Entidades.Base
{
    public interface IProductTransactionEntity<TEntity> where TEntity : Entity, IProductTransactionLineEntity
    {
        List<TEntity> Lines { get; set; }
    }
}
