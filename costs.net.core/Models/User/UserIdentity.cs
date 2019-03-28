namespace dnt.core.Models.User
{
    using System;

    /// <summary>
    /// Represents an end user or a System user.
    /// </summary>
    public class UserIdentity : IEquatable<UserIdentity>
    {
        public long Id { get; set; }

        public string GdamUserId { get; set; }

        public string Email { get; set; }

        public string FirstName { get; set; }

        public string FullName { get; set; }

        public string LastName { get; set; }

        public Guid AgencyId { get; set; }

        public Guid ModuleId { get; set; }

        public BuType BuType { get; set; }

        public string IpAddress { get; set; }

        public string Username { get { return Email; } }

        public bool Equals(UserIdentity other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Id.Equals(other.Id);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return Equals((UserIdentity) obj);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}
