using PuntoVenta.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PuntoVenta.Entidades.Base
{
    public abstract class ProductTransactionEntity<TEntity> : Entity, IProductTransactionEntity<TEntity> where TEntity : ProductTransactionLineEntity
    {
        private List<TEntity> _lines;

        public ProductTransactionEntity()
        {
            _lines = new List<TEntity>();
        }

        User User { get; set; }

        public List<TEntity> Lines
        {
            get { return _lines; }
            set { _lines = value; }
        }
    }
}
