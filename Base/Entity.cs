using System;
using System.ComponentModel;

namespace HDO.PointOfSale.Entities
{
    public abstract class Entity : IEntity, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private Guid _id;
        private DateTime _dateCreated;
        private DateTime? _dateUpdated;
        
        public Entity()
        {
            _id = Guid.NewGuid();
            _dateCreated = DateTime.Now;
            _dateUpdated = DateTime.Now;
        }

        protected void NotifyProperty(string name)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(name));
        }

        public virtual Guid Id
        {
            get { return _id; }
            set
            {
                _id = value;
                NotifyProperty("Id");
            }
        }

        public virtual DateTime DateCreated
        {
            get { return _dateCreated; }
            set
            {
                _dateCreated = value;
                NotifyProperty("DateCreated");
            }
        }

        public virtual DateTime? DateUpdated
        {
            get { return _dateUpdated; }
            set
            {
                _dateUpdated = value;
                NotifyProperty("DateUpdated");
            }
        }

        public override bool Equals(object obj)
        {
            Entity entity = obj as Entity;
            
            if (entity == null)
                return false;

            return entity.Id == Id;
        }

        public override int GetHashCode()
        {
            return this._id.GetHashCode();
        }
    }
}
