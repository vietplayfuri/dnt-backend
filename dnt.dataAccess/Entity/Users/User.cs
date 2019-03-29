using System.Collections.Generic;

namespace dnt.dataAccess.Entity
{
    public class User : ModifiableEntity, IUser
    {
        public virtual string Username { get; set; }

        public virtual string Email { get; set; }

        public virtual string Fullname { get; set; }
        public virtual string Password { get; set; }
        public virtual bool Disabled { get; set; }
        public virtual IList<UserRole> UserRoles { get; set; }
    }
}
