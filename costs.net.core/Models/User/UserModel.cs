namespace dnt.core.Models.User
{
    using System;

    public class UserModel
    {
        public Guid Id { get; set; }

        public string Email { get; set; }

        public string FirstName { get; set; }

        public string FullName { get; set; }

        public string LastName { get; set; }

        public string AgencyName { get; set; }

        public string AgencyCountryName { get; set; }

        public CostUserModel CostUserData { get; set; }
    }
}
