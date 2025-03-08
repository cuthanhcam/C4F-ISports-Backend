using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Dtos.Account;

namespace api.Interfaces
{
    public interface IAuthService
    {
        Task<UserDto> Login(LoginDto loginDto);
        Task<UserDto> Register(RegisterDto registerDto);
    }
}