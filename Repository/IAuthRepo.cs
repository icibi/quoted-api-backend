using backend.Model;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace backend.Repository
{
    public interface IAuthRepo
    {
        Task<UserDTO> ValidateUserAsync(string username, string password);
        Task<bool> SignupAsync([FromBody] SignupDTO signupRequest);
        Task<bool> PatchUserAsync(string Uid, UserUpdateDTO updatedUser);
        Task<UserUpdateDTO> GetUserById(string Uid);
        Task<bool> DeleteUserById(string Uid);
    }
}
