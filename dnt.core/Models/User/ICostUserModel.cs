namespace dnt.core.Models.User
{
    using System;

    public interface ICostUserModel
    {
        Guid Id { get; }

        string Email { get; }

        string FirstName { get; }

        string LastName { get; }

        Guid PrimaryCurrency { get; set; }

        bool IsPlatformAdmin { get; set; }

        bool CanCreateCost { get; set; }
    }
}
