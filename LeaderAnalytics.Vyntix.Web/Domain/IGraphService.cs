using LeaderAnalytics.Vyntix.Web.Model;
using Microsoft.Graph;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LeaderAnalytics.Vyntix.Web.Domain
{
    public interface IGraphService
    {
        Task<User> CreateUser(User user);
        Task<User> CreateUser(UserRecord userRecord);
        Task DeleteUser(string id);
        Task<List<User>> FindUser(string id);
        Task<List<User>> GetAllUsers();
        Task<List<UserRecord>> GetDelegateUsers(string ownerID);
        Task<UserRecord> GetUserByEmailAddress(string email);
        Task<UserRecord> GetUserRecordByID(string id);
        Task UpdateUser(UserRecord user);
        Task UpdateUser(User user);
        Task<bool> VerifyUser(string userID);
    }
}