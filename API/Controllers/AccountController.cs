using API.Data;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using API.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Security.Cryptography;
using API.DTOs;
using System;
using API.Interfaces;
using System.Linq;
using AutoMapper;
using Microsoft.AspNetCore.Identity;

namespace API.Controllers
{
    public class AccountController : BaseApiController
    {

        private readonly ITokenService _tokenService;

        private readonly IMapper _mapper;

        private readonly UserManager<AppUser> _userManager;

        private readonly SignInManager<AppUser> _signInManager;

        public AccountController(ITokenService tokenService, IMapper mapper, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager)
        {

            _tokenService = tokenService;

            _mapper = mapper;

            _userManager = userManager;

            _signInManager = signInManager;
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login (LoginDto loginDto)
        {
            var user=await _userManager.Users
                .Include(u => u.Photos)
                .SingleOrDefaultAsync(u => u.UserName == loginDto.Username.ToLower());

            var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);
            if(!result.Succeeded)
            {
                return Unauthorized();
            }

            if(user == null)
            {
                return Unauthorized("Invalid username.");
            }

            return new UserDto
            {
                Username = user.UserName,   
                Token = await _tokenService.CreateToken(user),
                PhotoUrl = user.Photos.FirstOrDefault(p => p.IsMain)?.Url,
                KnownAs = user.KnownAs,
                Gender = user.Gender
            };
        }


        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register (RegisterDto registerDto)
        {
            if(await UserExists(registerDto.Username))
            {
                return BadRequest("Username is taken.");
            }

            var user = _mapper.Map<AppUser>(registerDto);

            user.UserName = (registerDto.Username).ToLower();

            var result = await _userManager.CreateAsync(user, registerDto.Password);

            if(!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            var roleResult = await _userManager.AddToRoleAsync(user, "Member");

            if(!roleResult.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return new UserDto
            {
                Username = user.UserName,
                Token = await _tokenService.CreateToken(user),
                KnownAs = user.KnownAs, 
                Gender = user.Gender
            };
        }

        public async Task<bool> UserExists(string username)
        {
            return await _userManager.Users.AnyAsync(u => u.UserName == username.ToLower());
        }
    }
}