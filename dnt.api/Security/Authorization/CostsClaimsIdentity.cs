namespace dnt.api.Security.Authorization
{
    using System.Collections.Generic;
    using System.Security.Claims;

    public class CostsClaimsIdentity : ClaimsIdentity
    {
        public CostsClaimsIdentity(IEnumerable<Claim> claims, string userName)
            : base(claims)
        {
            Name = userName;
        }

        public override bool IsAuthenticated => true;

        public override string Name { get; }
    }
}