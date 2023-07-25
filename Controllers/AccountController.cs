using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Abstractions;
using Microsoft.Identity.Web;

namespace woodgrove_groceries_api.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class AccountController : ControllerBase
{
    private readonly ILogger<AccountController> _logger;
    private readonly IConfiguration _configuration;
    readonly IAuthorizationHeaderProvider _authorizationHeaderProvider;

    public AccountController(ILogger<AccountController> logger, IConfiguration configuration, IAuthorizationHeaderProvider authorizationHeaderProvider)
    {
        _logger = logger;
        _configuration = configuration;
        _authorizationHeaderProvider = authorizationHeaderProvider;
    }

    [HttpGet(Name = "Account")]
    public async Task<AccountData> GetAsync()
    {
        string accessToken = string.Empty;
        // Acquire the access token.
        string baseUrl = _configuration.GetSection("WoodgroveGroceriesDownstreamApi:BaseUrl").Value!;
        string[] scopes = _configuration.GetSection("WoodgroveGroceriesDownstreamApi:Scopes").Get<string[]>();
        var x = User.Claims;
        if (scopes == null)
        {
            throw new Exception("The MyApi:Scopes application setting is misconfigured or missing. Use the array format: [\"Account.Payment\", \"Account.Purchases\"]");
        }
        else if (baseUrl == null)
        {
            throw new Exception("The MyApi:BaseUrl application setting is misconfigured or missing. Check out your applications' scope base URL in Microsoft Entra admin center. For example: api://12345678-0000-0000-0000-000000000000");
        }
        else
        {
            for (int i = 0; i < scopes.Length; i++)
            {
                scopes[i] = $"{baseUrl}/{scopes[i]}";
            }

            try
            {
                accessToken = await _authorizationHeaderProvider.CreateAuthorizationHeaderForUserAsync(scopes);
            }
            catch (System.Exception ex)
            {
                if (ex.GetType().ToString().Contains("MicrosoftIdentityWebChallengeUserException"))
                {
                    throw new Exception("The token cache does not contain the token to access the web APIs. To get the access token, sign-out and sign-in again.");
                }
                else
                {
                    throw ex;
                }

            }
        }

        AccountData account = new AccountData();
        account.DisplayName = User.Identity.Name;

        //
        account.Payment = new Payment();
        account.Payment.AccessTokenToCallThePaymentAPI = accessToken;
        account.Payment.NameOnCard = User.Identity.Name;
        account.Payment.CardNumber = "123456789000000";
        account.Payment.ExpirationDate  = DateTime.Now.AddDays(500).ToShortDateString();
        return account;
    }
}
