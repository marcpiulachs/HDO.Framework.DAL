using PuntoVenta.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PuntoVenta.Entidades.Base
{
    public interface IProductTransactionLineEntity: IEntity
    {
        Product Product { get; set; }
        decimal Quantity { get; set; }
    }
}
