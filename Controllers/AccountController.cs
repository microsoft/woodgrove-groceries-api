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

        // Get the app settings
        string baseUrl = _configuration.GetSection("WoodgroveGroceriesDownstreamApi:BaseUrl").Value!;
        string[] scopes = _configuration.GetSection("WoodgroveGroceriesDownstreamApi:Scopes").Get<string[]>();

        // Check the scopes application settings
        if (scopes == null)
        {
            return new AccountData("The MyApi:Scopes application setting is misconfigured or missing. Use the array format: [\"Account.Payment\", \"Account.Purchases\"]");
        }

        // Check the base URL application settings
        if (baseUrl == null)
        {
            return new AccountData("The MyApi:BaseUrl application setting is misconfigured or missing. Check out your applications' scope base URL in Microsoft Entra admin center. For example: api://12345678-0000-0000-0000-000000000000");
        }

        // Set the scope full URL (temporary workaround should be fix)
        for (int i = 0; i < scopes.Length; i++)
        {
            scopes[i] = $"{baseUrl}/{scopes[i]}";
        }

        try
        {
            // Acquire an access token to call the downstream API (Payment API).
            accessToken = await _authorizationHeaderProvider.CreateAuthorizationHeaderForUserAsync(scopes);
        }
        catch (System.Exception ex)
        {
            return new AccountData(ex.Message);
        }

        AccountData account = new AccountData();
        account.DisplayName = User.Identity.Name;

        // Simulates a call to a downstream API and return the access token for the downstream API
        account.Payment = new Payment();
        account.Payment.AccessTokenToCallThePaymentAPI = accessToken;
        account.Payment.NameOnCard = User.Identity.Name;
        account.Payment.CardNumber = "123456789000000";
        account.Payment.ExpirationDate = DateTime.Now.AddDays(500).ToShortDateString();
        return account;
    }
}
