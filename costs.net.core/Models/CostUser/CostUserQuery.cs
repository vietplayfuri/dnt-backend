namespace dnt.core.Models.CostUser
{
    using System;
    using System.Collections.Generic;

    public class CostUserQuery : BaseQuery
    {
        public Guid[] BusinessRoleIds { get; set; }

        public string AgencyName { get; set; }

        public string AgencyLabel { get; set; }
        public List<string> AgencyLabels { get; set; }
        public bool MatchLabel { get; set; }
        public Guid AgencyId { get; set; }
        public string[] CostUserGroups { get; set; }
        public Guid CostId { get; set; }
        public string BusinessRoleKey { get; set; }
    }
}
