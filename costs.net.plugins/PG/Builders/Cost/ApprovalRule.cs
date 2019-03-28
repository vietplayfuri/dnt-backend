namespace costs.net.plugins.PG.Builders.Cost
{
    using core.Builders.Response;
    using dataAccess.Entity;
    using System.Collections.Generic;

    public class ApprovalRule
    {
        public bool CostConsultantIpmAllowed { get; set; }

        public bool IpmApprovalEnabled { get; set; }

        public bool BrandApprovalEnabled { get; set; }

        public bool HasExternalIntegration { get; set; }

        public string BudgetRegion { get; set; }

        public string ContentType { get; set; }

        public decimal TotalCostAmount { get; set; }

        public CostType CostType { get; set; }

        public List<ApprovalModel> Approvals()
        {
            var approvals = new List<ApprovalModel>();
            if (IpmApprovalEnabled)
            {
                var validBusinessRoles = new List<string> { Constants.BusinessRole.Ipm };

                if (CostConsultantIpmAllowed)
                {
                    validBusinessRoles.Add(Constants.BusinessRole.CostConsultant);
                }

                approvals.Add(new ApprovalModel
                {
                    Type = ApprovalType.IPM,
                    Status = ApprovalStatus.New,
                    ValidBusinessRoles = validBusinessRoles.ToArray()
                });
            }

            if (!BrandApprovalEnabled)
            {
                return approvals;
            }

            var approval = new ApprovalModel
            {
                Type = ApprovalType.Brand,
                Status = ApprovalStatus.New,
                ValidBusinessRoles = new[] { Constants.BusinessRole.BrandManager },
            };

            if (HasExternalIntegration)
            {
                approval.Members.Add(new ApprovalMemberModel
                {
                    Email = ApprovalMemberModel.BrandApprovalUserEmail,
                    IsExternal = true // This approval member should be hidden from UI because it is 'fake' approval member that represents Coupa system
                });
            }
            approvals.Add(approval);

            return approvals;
        }
    }
}