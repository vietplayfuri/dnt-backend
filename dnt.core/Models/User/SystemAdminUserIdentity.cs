
namespace dnt.core.Models.User
{
    using dnt.dataAccess.Entity;

    public class SystemAdminUserIdentity : UserIdentity
    {
        public SystemAdminUserIdentity(User systemUser)
        {
            Email = systemUser.Email;
            Id = systemUser.Id;
            IpAddress = "System";
        }
    }
}
