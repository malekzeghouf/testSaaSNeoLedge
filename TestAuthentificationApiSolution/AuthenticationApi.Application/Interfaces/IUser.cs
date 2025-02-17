using AuthenticationApi.Application.DTO;
using SharedLibrary.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthenticationApi.Application.Interfaces
{
    public interface IUser
    {
        Task<Response>Register(UserDTO userDTO);
        Task<Response> Login(LoginDTO loginDTO);
        Task<GetUserDTO> GetUser(int userId);

    }
}
