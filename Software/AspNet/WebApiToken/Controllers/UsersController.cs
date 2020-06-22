﻿using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using WebApiToken.Entities;
using WebApiToken.Helpers;
using WebApiToken.Models;
using WebApiToken.Services;

namespace WebApiToken.Controllers
{
    //[Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {

        private IUserService _userService;
        private readonly DataContext _context;
        private IMapper _mapper;
        private ApiSettings _apiSettings;

        // Constructor
        public UsersController(DataContext context, IMapper mapper, IUserService userService,
            IOptions<ApiSettings> apiSettings)
        {
            _mapper = mapper;
            _userService = userService;
            _context = context;
            _apiSettings = apiSettings.Value;
        }


        // GET: api/User
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserModel>>> GetUser()
        {

            return NotFound();
            /*
            // Get users
            var users = await _userService.GetAll();

            // Map to model
            var userModels = _mapper.Map<List<UserModel>>(users);

            // Return model
            return userModels;
            */
            // TODO: allow for admins

        }

        // POST: api/User
        [HttpPost]
        public async Task<ActionResult<UserModel>> GetUserProfile([FromBody] ProfileQueryModel model)
        {
            // Get user
            var user = await _userService.GetById(model.Id);

            // Map user
            var userModel = _mapper.Map<UserModel>(user);

            if (user == null)
            {
                return NotFound();
            }

            // Return model
            return userModel;
        }

        // GET: api/User/5
        [HttpGet("{id}")]
        public async Task<ActionResult<UserPublicModel>> GetUserPublic(int id)
        {
            // Get user
            var user = await _userService.GetById(id);

            // Map user
            var userModel = _mapper.Map<UserPublicModel>(user);

            if (user == null)
            {
                return NotFound();
            }

            // Return model
            return userModel;
        }


        // Post: api/users/authenticate
        [AllowAnonymous]
        [HttpPost("authenticate")]
        public async Task<ActionResult> Authenticate([FromBody] LoginModel model)
        {
            
            // Try to authenticate user
            var user = await _userService.Authenticate(model.Username, model.Password);

            // If unsuccessful, return BadRequest
            if (user == null)
                return BadRequest(new { message = "Username or password is incorrect" });

            /*
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_apiSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, user.Id.ToString())
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);
            */

            // Return basic user info and authentication token
            return Ok(new
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Firstname = user.Firstname,
                Lastname = user.Lastname
                //Token = tokenString
            });
        }


        // POST: api/users/register
        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<ActionResult<User>> RegisterUser([FromBody] RegisterModel model)
        {

            // Map model back to user
            //var user = _mapper.Map<User>(model);
            var user = new User
            {
                Username = model.Username,
                Email = model.Email,
                Firstname = model.FirstName,
                Lastname = model.LastName
            };

            // Try to create user
            try
            {

                // Create user
                await _userService.Create(user, model.Password);
                return Ok(new
                {
                    message = $"Successfully registered new user {user.Username}"
                });

            }
            catch (ApiException ex)
            {

                // Return error message if there was an exception
                return BadRequest(new {message = ex.Message});

            }
        }


        // PUT: api/User/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(int id, User user)
        {
            if (id != user.Id)
            {
                return BadRequest();
            }

            _context.Entry(user).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/User/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<User>> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return user;
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }
}
