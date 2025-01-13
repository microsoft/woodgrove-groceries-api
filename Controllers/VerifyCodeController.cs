using Azure.Communication.Email;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Identity.Abstractions;
using Microsoft.Identity.Web;

namespace woodgrove_groceries_api.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class VerifyCodeController : ControllerBase
{
    private readonly ILogger<VerifyCodeController> _logger;
    private readonly IConfiguration _configuration;
    private readonly IMemoryCache _memoryCache;

    public VerifyCodeController(ILogger<VerifyCodeController> logger, IConfiguration configuration, IMemoryCache memoryCache)
    {
        _logger = logger;
        _configuration = configuration;
        _memoryCache = memoryCache;
    }

    [HttpPost(Name = "VerifyCode")]
    public async Task<VerifyCodeResponse> OnPostAsync([FromBody] VerifyCodeRequest request)
    {
        // Check the user object ID
        if (User == null || User.GetObjectId() == null)
        {
            return new VerifyCodeResponse("Error: User object ID is null");
        }

        string userID = User.GetObjectId()!;
        VerifyCodeResponse response = new VerifyCodeResponse();
        response.ValidationPassed = false;

        // Try to get the cache object for the current user
        if (_memoryCache.TryGetValue(userID, out AuthMethod cachedAuthMethod))
        {
            // Increase the number of user tries
            cachedAuthMethod.Validations++;

            if (IsAboveThreshold(cachedAuthMethod))
            {
                return new VerifyCodeResponse("You have reached the maximum number of allowed verifications.");
            }

            // TBD: Compare Hash 
            if (request.VerificationCode == cachedAuthMethod.VerificationCode)
            {
                response.ValidationPassed = true;
                response.AuthType = cachedAuthMethod.AuthType;
                response.AuthValue = cachedAuthMethod.AuthValue;

                // Update the verification code with a random value and save it back to the cache
                cachedAuthMethod.VerificationCode = new Guid().ToString();
                var cacheEntryOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromHours(1));
                _memoryCache.Set(userID, cachedAuthMethod, cacheEntryOptions);
            }
            else
            {
                var cacheEntryOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromHours(1));
                _memoryCache.Set(userID, cachedAuthMethod, cacheEntryOptions);
            }
        }

        return response;
    }

    private bool IsAboveThreshold(AuthMethod authMethod)
    {
        // Get app settings
        int maxRetry = _configuration.GetValue<int>("AppSettings:MaxRetry", 3);

        // Check if the user's validation in the last hour reached the threshold
        return authMethod.Validations > maxRetry;
    }
}


