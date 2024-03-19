﻿using bobs_burger_api.Models.Users;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using static bobs_burger_api.Models.DTO;
using bobs_burger_api.Services;
using bobs_burger_api.Repository;

namespace bobs_burger_api.Endpoints
{
    public static class AuthEndpoints
    {
        public static void ConfigureAuthEndpoints(this WebApplication app)
        {
            var taskGroup = app.MapGroup("auth");
            taskGroup.MapPost("/register", Register);
            taskGroup.MapPost("/login", Login);
        }
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async static Task<IResult> Register(RegisterDto registerPayload, UserManager<ApplicationUser> userManager)
        {
            if (registerPayload.Email == null) 
                return TypedResults.BadRequest("Email is required.");
            if (registerPayload.Password == null) 
                return TypedResults.BadRequest("Password is required.");
            var result = await userManager.CreateAsync(
            new ApplicationUser
            {
                UserName = registerPayload.Email,
                Email = registerPayload.Email,
                Role = UserRole.User
            },
            registerPayload.Password!
            );
            if (result.Succeeded)
            {
                return TypedResults.Created($"/auth/", new
                RegisterResponseDto(registerPayload.Email, UserRole.User));
            }
            return Results.BadRequest(result.Errors);
        }

        public async static Task<IResult> Login(LoginDto loginPayload, UserManager<ApplicationUser> userManager, TokenService tokenService)
        {
            if (loginPayload.Email == null) 
                return TypedResults.BadRequest("Email is required.");
            if (loginPayload.Password == null) 
                return TypedResults.BadRequest("Password is required.");
            var user = await
            userManager.FindByEmailAsync(loginPayload.Email!);
            if (user == null)
            {
                return TypedResults.BadRequest("Invalid email or password.");
            }
            // check the password matches
            var isPasswordValid = await
            userManager.CheckPasswordAsync(user, loginPayload.Password);
            if (!isPasswordValid)
            {
                return TypedResults.BadRequest("Invalid email or password.");
            }
            // create a token
            var token = tokenService.CreateToken(user);
            // return the response
            return TypedResults.Ok(new AuthResponseDto(token, user.Email, user.Role));
        }
    }
}