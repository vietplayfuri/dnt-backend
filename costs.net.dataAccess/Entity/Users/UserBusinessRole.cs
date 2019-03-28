namespace dnt.dataAccess.Entity
{
    public class UserRole : ModifiableEntity
    {
        public virtual long UserId { get; set; }
        public virtual long RoleId { get; set; }
        public virtual Role Role { get; set; }
        public virtual User User { get; set; }
    }
}
