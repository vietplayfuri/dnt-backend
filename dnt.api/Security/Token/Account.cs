using dnt.core.Models.User;
using System;
using System.Collections.Generic;
using System.Text;

namespace dnt.api.Security.Token
{
    public class Account : UserModel
    {
        public string Token { get; internal set; }
    }
}
