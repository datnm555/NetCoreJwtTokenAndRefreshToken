using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using TestJwtTokenAndRefreshToken.Dtos;
using TestJwtTokenAndRefreshToken.Models;
using TestJwtTokenAndRefreshToken.Services;

namespace TestJwtTokenAndRefreshToken.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TokensController : ControllerBase
    {
        private readonly AppDbContext _appDbContext;
        private readonly ITokenService _tokenService;

        public TokensController(AppDbContext appDbContext, ITokenService tokenService)
        {
            _appDbContext = appDbContext;
            _tokenService = tokenService;
        }


        [HttpPost]
        [Route("refresh")]
        public IActionResult RefreshToken(TokenDto tokenDto)
        {
            if (tokenDto is null)
            {
                return BadRequest("Invalid client request");
            }
            string accessToken = tokenDto.AccessToken;
            string refreshToken = tokenDto.RefreshToken;

            var principal = _tokenService.GetPrincipalFromExpiredToken(accessToken);
            var userName = principal.Identity.Name;
            var user = _appDbContext.Users.SingleOrDefault(x => x.UserName == userName);
            if (user == null || user.RefreshToken != refreshToken || user.RefreshTokenExpiryTime <= DateTime.Now)
            {
                return BadRequest("Invalid client request");
            }

            var newAccessToken = _tokenService.GenerateAccessToken(principal.Claims);
            var newRefreshToken = _tokenService.GenerateRefreshToken();

            user.RefreshToken = newRefreshToken;
            _appDbContext.SaveChanges();

            return Ok(new AuthenticatedResponse()
            {
                Token = newAccessToken,
                RefreshToken = newRefreshToken
            });
        }

        [HttpPost]
        [Authorize]
        [Route("revoke")]
        public IActionResult RevokeToken()
        {
            var userName = User.Identity.Name;
            var user = _appDbContext.Users.SingleOrDefault(x => x.UserName == userName);
            if (user == null)
            {
                return BadRequest("Invalid client request");
            }

            user.RefreshToken = null;
            _appDbContext.SaveChanges();
            return NoContent();
        }

    }
}
