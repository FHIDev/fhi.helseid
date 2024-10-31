using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Fhi.HelseId.Web.Hpr
{
    public interface IUserInfoApi
    {
        [HttpGet("/connect/userinfo")]
        Task<UserInfoResponse> GetUserInfo(string id);
    }
}