namespace dnt.core.Models.User
{
    using System;
    using System.Collections.Generic;

    public class CostUserModel : ICostUserModel
    {
        public Guid Id { get; set; }

        public string Email { get; set; }

        public int? ApprovalLimit { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string FullName { get; set; }

        public Guid PrimaryCurrency { get; set; }

        public bool IsPlatformAdmin { get; set; }

        public bool CanCreateCost { get; set; }

        public string EmailOverride { get; set; }

        public Guid? NotificationBudgetRegionId { get; set; }
    }
}
