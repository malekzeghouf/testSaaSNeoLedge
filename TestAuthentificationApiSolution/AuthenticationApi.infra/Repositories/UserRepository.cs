using AuthenticationApi.Application.DTO;
using AuthenticationApi.Application.Interfaces;
using AuthenticationApi.Domain.Entities;
using AuthenticationApi.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using SharedLibrary.Responses;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace AuthenticationApi.Infrastructure.Repositories
{
    public class UserRepository (AuthenticationDbContext context , IConfiguration config, MinioService minioService) : IUser
    {
        private readonly ILogger<MinioService> _logger;


        public async Task<User> GetUserByEmail(string email)
        {
            var user = await context.Users.FirstOrDefaultAsync(u => u.Email == email);
            return user is null ? null! : user!;

        }
        public async Task<GetUserDTO> GetUser(int userId)
        {
            var user = await context.Users.FindAsync(userId);
            return user is not null ? new GetUserDTO
                (
                user.Id!,
                user.Name!,
                user.Email!,
                user.Role!
                ) : null!;
        }

        public async Task<Response> Login(LoginDTO loginDTO)
        {
            
            
            var getUser = await GetUserByEmail(loginDTO.Email);
            if (getUser is null)
            {
                return new Response(false, "Invalid credentials");
            }
             bool verifPwd = BCrypt.Net.BCrypt.Verify (loginDTO.Password,getUser.Password);
            if (!verifPwd) {
                return new Response(false, "Invalid credentials");
            }
            string token = GenerateToken(getUser, config);
            return new Response(true, token);

        }

        private static string GenerateToken(User getUser, IConfiguration config)
        {
            var key= Encoding.UTF8.GetBytes(config.GetSection("Authentication:Key").Value!);
            var securityKey = new SymmetricSecurityKey(key);
            var credentials = new SigningCredentials(securityKey,SecurityAlgorithms.HmacSha256);
            var claims = new List<Claim>
            {
                new(ClaimTypes.Name,getUser.Name!),
                new(ClaimTypes.Email,getUser.Email!),
                new (ClaimTypes.Role,getUser.Role!)
            };
            var token = new JwtSecurityToken(
                issuer: config["Authentication:Issuer"],
                audience: config["Authentication:Audience"],
                claims: claims,
                expires: null,
                signingCredentials: credentials
                );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /* public async Task<Response> Register(UserDTO userDTO)
         {
             var getUser = await GetUserByEmail (userDTO.Email);
             if (getUser is not null)
             {
                 return new Response(false, $"you cannot use this email for registration");

             }
             var result = context.Users.Add(
                 new User()
                 {
                     Name = userDTO.Name,
                     Email = userDTO.Email,
                     Password = BCrypt.Net.BCrypt.HashPassword(userDTO.Password),
                     Role = "User",
                 });
             await context.SaveChangesAsync();
             try
             {
                 await minioService.CreateBucketAsync(userDTO.Name);
             }
             catch (Exception ex)
             {
                 // Log the error but don't fail the registration
                 // You can log the exception here if needed
                 Console.WriteLine($"Failed to create MinIO bucket: {ex.Message}");
             }

             return result.Entity.Id > 0 ? new Response(true, "User registred successfully") :
                 new Response(false, "Invalid data provided "); 
         } */
        public async Task<Response> Register(UserDTO userDTO)
        {
            var getUser = await GetUserByEmail(userDTO.Email);
            if (getUser is not null)
            {
                return new Response(false, $"You cannot use this email for registration.");
            }

            var result = context.Users.Add(
                new User()
                {
                    Name = userDTO.Name,
                    Email = userDTO.Email,
                    Password = BCrypt.Net.BCrypt.HashPassword(userDTO.Password),
                    Role = "User",
                });

            await context.SaveChangesAsync();

            try
            {
                var sanitizedBucketName = userDTO.Name.ToLower().Replace(" ", "-");
                await minioService.CreateBucketAsync(sanitizedBucketName);
                await minioService.UploadKeysAsync(userDTO.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to create MinIO bucket for user {userDTO.Name}");
                throw; // Re-throw the exception
            }

            return result.Entity.Id > 0 ? new Response(true, "User registered successfully.") :
                new Response(false, "Invalid data provided.");
        }
    }
}
