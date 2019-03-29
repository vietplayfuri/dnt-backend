using System;

namespace dnt.dataAccess.Entity
{
    public abstract class ModifiableEntity : Entity, IModifiable
    {
        public virtual long CreatedById { get; set; }
        public virtual DateTime Created { get; set; }
        public virtual DateTime? Modified { get; set; }
    }
}
