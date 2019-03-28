namespace dnt.api.Security.Token
{
    using System.Collections.Generic;

    public static class AccountInMemory
    {
        public static IList<Account> ArrayAccount = new List<Account>();

        static AccountInMemory()
        {
            ArrayAccount.Add(new Account
            {
                Id = long.MaxValue,
                FullName = "Butter Ngo",
                UserName = "admin",
                Password = "123456"
            });
        }
    }
}
