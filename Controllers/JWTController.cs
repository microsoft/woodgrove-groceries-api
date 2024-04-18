using System.Collections.ObjectModel;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection.Metadata.Ecma335;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Abstractions;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.Tokens;

namespace woodgrove_groceries_api.Controllers;

[ApiController]
[Route("[controller]")]
public class JWTController : ControllerBase
{
    private readonly ILogger<JWTController> _logger;
    private readonly IConfiguration _configuration;
    readonly IAuthorizationHeaderProvider _authorizationHeaderProvider;

    public JWTController(ILogger<JWTController> logger, IConfiguration configuration, IAuthorizationHeaderProvider authorizationHeaderProvider)
    {
        _logger = logger;
        _configuration = configuration;
        _authorizationHeaderProvider = authorizationHeaderProvider;
    }

    [HttpGet(Name = "JWT")]
    public async Task<Dictionary<string, string>> GetAsync()
    {
        Dictionary<string, string> claims = new Dictionary<string, string>();

        try
        {
            string accessToken = Request.Query["token"];

            var handler = new JwtSecurityTokenHandler();

            SecurityToken securityToken = handler.ReadToken(accessToken);
            System.IdentityModel.Tokens.Jwt.JwtSecurityToken jwt = ((System.IdentityModel.Tokens.Jwt.JwtSecurityToken)securityToken);

            foreach (var item in jwt.Claims)
            {
                claims.Add(item.Type, item.Value);
            }

        }
        catch (System.Exception ex)
        {
            claims.Add("error", ex.Message);
        }

        return claims;
    }
}



