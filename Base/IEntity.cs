using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HDO.PointOfSale.Entities
{
    public interface IEntity<T>
    {
        T Id { get; set; }

        DateTime DateCreated { get; set; }
        DateTime? DateUpdated { get; set; }
    }

    public interface IEntity : IEntity<Guid>
    {

    }

    public interface IDto<T> : IEntity<T>
    {

    }
}
