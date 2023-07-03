﻿using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Security.Claims;
using TimeTracker.Area.Identity.Models;
using System.Net.Http;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using TimeTracker.Repositories;

namespace TimeTracker.Area.Identity
{
    [Area("Identity")]
    public class IdentityController : Controller
    {
        [HttpPost]
        public async Task<IActionResult> Login([FromBody] Login UserLogData, [FromServices] IConfiguration configuration, [FromServices] IUserRepository userRepository)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid data");
            }

            var user = await userRepository.GetUserByCredentials(UserLogData.LoginOrEmail, UserLogData.Password);

            if (user == null)
            {
                return BadRequest("User does not exist");
            }

            var permissions = await userRepository.GetPermissions(user.Id);

            var jwt = new JwtSecurityToken(
            issuer: configuration["JWT:Author"],
            audience: configuration["JWT:Audience"],
            claims: new[] {
            
            new Claim("LoginOrEmail", UserLogData.LoginOrEmail),
            new Claim("CRUDUsers",permissions.CRUDUsers.ToString()),
            new Claim("ViewUsers",permissions.ViewUsers.ToString()),
            new Claim("EditPermiters",permissions.EditPermiters.ToString()),
            new Claim("ImportExcel",permissions.ImportExcel.ToString()),
            new Claim("ControlPresence",permissions.ControlPresence.ToString()),
            new Claim("ControlDayOffs",permissions.ControlDayOffs.ToString())
            },
            expires: DateTime.UtcNow.Add(TimeSpan.FromDays(365)),
            signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(configuration["JWT:Key"]), SecurityAlgorithms.HmacSha256));

            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

            var response = new { access_token = encodedJwt };

            //HttpContext.Response.Cookies.Append("gdfg", "gdfdgdf", new()
            //{
            //    SameSite = SameSiteMode.None,
            //    Secure = true,
            //});

            return Json(response);
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Ok();
        }
    }
}
