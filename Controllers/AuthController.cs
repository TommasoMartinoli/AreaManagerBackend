using Microsoft.AspNetCore.Mvc;
using ADLoginAPI.Services;
using ADLoginAPI.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using ADLoginAPI.Models;
using System;
using ADLoginAPI.DTO;
using Microsoft.Data.SqlClient;

namespace ADLoginAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ActiveDirectoryService _adService;
        private readonly ApplicationDbContext _contextApp;

        public AuthController(ApplicationDbContext contextApp)
        {
            _adService = new ActiveDirectoryService("ZG");
            _contextApp = contextApp;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginModel loginModel)
        {
            var (isAuthenticated, firstName, lastName) = _adService.AuthenticateUser(loginModel.Username, loginModel.Password);

            if (isAuthenticated)
            {
                string managerLogin = $"MICROAREA\\{lastName}";

                var myPICEmployeeQuery = @"
                    SELECT PICEmployee,EMail,Name,Surname
                    FROM PAI_EmployeesMaster
                    WHERE CustomerCode = '0110G081'
                      AND LOWER(Surname) = @lastName";

                var myUserData = _contextApp.PAI_EmployeesMaster
                    .FromSqlRaw(myPICEmployeeQuery, new SqlParameter("@lastName", lastName.ToLower()))
                    .Select(e => new { e.PICEmployee, e.EMail, e.Name, e.Surname })
                    .FirstOrDefault();

                var teamRolesQuery = @"
                    SELECT RoleID,Role 
                    FROM PAI_FnEnumerateManagedRoles('Microarea SpA', @managerLogin) 
                    ORDER BY Role";

                var teamRoles = _contextApp.PAI_TeamRolesFunction
                    .FromSqlRaw(teamRolesQuery, new SqlParameter("@managerLogin", managerLogin))
                    .ToList();

                return Ok(new
                {
                    message = "Login successful",
                    teamRoles,
                    myUserData
                });
            }
            else
            {
                return Unauthorized(new { message = "Invalid credentials" });
            }
        }


        [HttpPost("logout")]
        public IActionResult Logout()
        {
            return Ok(new { message = "Logout successful" });
        }
    }

    public class LoginModel
    {
        public required string Username { get; set; }
        public required string Password { get; set; }
    }
}