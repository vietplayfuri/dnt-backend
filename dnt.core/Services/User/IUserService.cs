namespace dnt.core.Services.User
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Models;
    using Models.Response;
    using Models.User;

    public interface IUserService
    {
        Task<bool> IsCorrectPassword(string username, string password);
        Task<List<UserModel>> Get();
        Task<UserModel> Get(long userId);
        Task<UserModel> Get(string username);
        Task<UserModel> Create(UserInsertModel userInsertModel);
        Task<UserModel> Update(UserInsertModel userInsertModel);
    }
}
