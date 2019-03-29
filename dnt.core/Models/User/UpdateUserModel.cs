namespace dnt.core.Models.User
{
    using System;
    using System.Collections.Generic;

    public class UpdateUserModel
    {
        public int? ApprovalLimit { get; set; }
        public string EmailOverride { get; set; }
        public Guid? NotificationBudgetRegionId { get; set; }
        public List<AccessDetail> AccessDetails { get; set; }
    }

    public class AccessDetail
    {
        public Guid? ObjectId { get; set; }
        public Guid? OriginalObjectId { get; set; }
        public string ObjectType { get; set; }
        public string LabelName { get; set; }
        public Guid? LabelId { get; set; }
        public Guid BusinessRoleId { get; set; }
    }
}