namespace dnt.core.Models.User
{
    public class UserModel
    {
        public long Id { get; set; }

        public string Username { get; set; }

        public string Email { get; set; }

        public string FullName { get; set; }
        public bool Disabled { get; set; }
        public long RoleId { get; set; }
    }
}
