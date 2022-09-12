using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TestJwtTokenAndRefreshToken.Dtos;
using TestJwtTokenAndRefreshToken.Models;
using TestJwtTokenAndRefreshToken.Services;

namespace TestJwtTokenAndRefreshToken.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthsController : ControllerBase
    {

        private readonly AppDbContext _appDbContext;
        private readonly ITokenService _tokenService;

        public AuthsController(AppDbContext appDbContext, ITokenService tokenService)
        {
            _appDbContext = appDbContext;
            _tokenService = tokenService;
        }



        [HttpPost, Route("login")]
        public IActionResult Login([FromBody] User userLogin)
        {
            if (userLogin is null)
            {
                return BadRequest("Invalid client request");
            }

            var user = _appDbContext.Users.FirstOrDefault(u =>
                (u.UserName == userLogin.UserName) && (u.Password == userLogin.Password));
            if (user is null)
                return Unauthorized();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, userLogin.UserName),
                new Claim(ClaimTypes.Role, "Manager")
            };
            var accessToken = _tokenService.GenerateAccessToken(claims);
            var refreshToken = _tokenService.GenerateRefreshToken();

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.Now.AddDays(7);

            _appDbContext.SaveChanges();

            return Ok(new AuthenticatedResponse
            {
                Token = accessToken,
                RefreshToken = refreshToken
            });
        }
    }
}
