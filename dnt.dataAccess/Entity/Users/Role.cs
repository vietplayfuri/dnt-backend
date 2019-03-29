using System.Collections.Generic;

namespace dnt.dataAccess.Entity
{
    public class Role : ModifiableEntity
    {
        public virtual string Name { get; set; }
        public virtual string Code { get; set; }
        public virtual IList<UserRole> UserRoles { get; set; }
    }
}
