using System;
using System.Collections.Generic;
using System.Text;

namespace dnt.api.Security.Token
{
    public class Account
    {
        public long Id { get; set; }
        public string FullName { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Token { get; internal set; }
    }
}
