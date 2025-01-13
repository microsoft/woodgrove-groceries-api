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
public class SendCodeController : ControllerBase
{
    private readonly ILogger<SendCodeController> _logger;
    private readonly IConfiguration _configuration;
    private readonly IMemoryCache _memoryCache;
    Random random = new Random();

    public SendCodeController(ILogger<SendCodeController> logger, IConfiguration configuration, IMemoryCache memoryCache)
    {
        _logger = logger;
        _configuration = configuration;
        _memoryCache = memoryCache;
    }


    [HttpPost(Name = "SendCode")]
    public async Task<SendCodeResponse> OnPostAsync([FromBody] SendCodeRequest request)
    {
        // Check the user object ID
        if (User == null || User.GetObjectId() == null)
        {
            return new SendCodeResponse("Error: User object ID is null");
        }

        string userID = User.GetObjectId()!;
        AuthMethod authMethod = null;

        // Try to get the cache object for the current user
        if (_memoryCache.TryGetValue(userID, out AuthMethod cachedAuthMethod))
        {
            // Get the value from the cache
            authMethod = cachedAuthMethod;
        }

        // If the cache is null
        if (authMethod == null)
        {
            // Init a new one
            authMethod = new AuthMethod();
            authMethod.UID = userID;
        }

        // Set the values
        authMethod.AuthType = request.AuthType;
        authMethod.AuthValue = request.AuthValue;
        authMethod.MessagesSent++;
        // Reset the validations to zero
        authMethod.Validations = 0;
        authMethod.VerificationCode = random.Next(112461, 989746).ToString();

        // Check if the user's validation in the last hour reached the threshold
        if (IsAboveThreshold(authMethod))
        {
            return new SendCodeResponse("You have reached the number of verification code you can send. Please wait an hour and try again.");
        }

        // Save data in cache
        // TBD: Hash the pass code before adding it to the cache
        var cacheEntryOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromHours(1));
        _memoryCache.Set(userID, authMethod, cacheEntryOptions);

        if (authMethod.AuthType == AuthMethodType.EmailMfa || authMethod.AuthType == AuthMethodType.SignInEmail)
        {
            try
            {
                await SendEmailAsync(authMethod);
            }
            catch (System.Exception ex)
            {
                return new SendCodeResponse(ex.Message);
            }

        }

        return new SendCodeResponse();
    }

    private bool IsAboveThreshold(AuthMethod authMethod)
    {
        // Get app settings
        int userThreshold = _configuration.GetValue<int>("AppSettings:UserThreshold", 3);
        int AppThreshold = _configuration.GetValue<int>("AppSettings:AppThreshold", 60);

        // Check if the user's validation in the last hour reached the threshold
        return authMethod.MessagesSent > userThreshold;
    }

    private async Task SendEmailAsync(AuthMethod authMethod)
    {
        string emailConnectionString = _configuration.GetSection("AppSettings:Email:ConnectionString").Value!;
        string emailSender = _configuration.GetSection("AppSettings:Email:Sender").Value!;

        try
        {
            var emailClient = new EmailClient(emailConnectionString);

            var subject = "Your Woodgrove account verification code";
            var htmlContent = @$"<html><body>
            <div style='background-color: #1F6402!important; padding: 15px'>
                <table>
                <tbody>
                    <tr>
                        <td colspan='2' style='padding: 0px;font-family: &quot;Segoe UI Semibold&quot;, &quot;Segoe UI Bold&quot;, &quot;Segoe UI&quot;, &quot;Helvetica Neue Medium&quot;, Arial, sans-serif;font-size: 17px;color: white;'>Woodgrove Groceries live demo</td>
                    </tr>
                    <tr>
                        <td colspan='2' style='padding: 15px 0px 0px;font-family: &quot;Segoe UI Light&quot;, &quot;Segoe UI&quot;, &quot;Helvetica Neue Medium&quot;, Arial, sans-serif;font-size: 35px;color: white;'>Your Woodgrove verification code</td>
                    </tr>
                    <tr>
                        <td colspan='2' style='padding: 25px 0px 0px;font-family: &quot;Segoe UI&quot;, Tahoma, Verdana, Arial, sans-serif;font-size: 14px;color: white;'> To access <span style='font-family: &quot;Segoe UI Bold&quot;, &quot;Segoe UI Semibold&quot;, &quot;Segoe UI&quot;, &quot;Helvetica Neue Medium&quot;, Arial, sans-serif; font-size: 14px; font-weight: bold; color: white;'>Woodgrove Groceries</span>'s app, please copy and enter the code below into the sign-up or sign-in page. This code is valid for 30 minutes. </td>
                    </tr>
                    <tr>
                        <td colspan='2' style='padding: 25px 0px 0px;font-family: &quot;Segoe UI&quot;, Tahoma, Verdana, Arial, sans-serif;font-size: 14px;color: white;'>Your account verification code:</td>
                    </tr>
                    <tr>
                        <td style='padding: 0px;font-family: &quot;Segoe UI Bold&quot;, &quot;Segoe UI Semibold&quot;, &quot;Segoe UI&quot;, &quot;Helvetica Neue Medium&quot;, Arial, sans-serif;font-size: 25px;font-weight: bold;color: white;padding-top: 5px;'>
                        {authMethod.VerificationCode}</td>
                        <td rowspan='3' style='text-align: center;'>
                            <img src='https://woodgrovedemo.com/custom-email/shopping.png' style='border-radius: 50%; width: 100px'>
                        </td>
                    </tr>
                    <tr>
                        <td style='padding: 25px 0px 0px;font-family: &quot;Segoe UI&quot;, Tahoma, Verdana, Arial, sans-serif;font-size: 14px;color: white;'> If you didn't request a code, you can ignore this email. </td>
                    </tr>
                    <tr>
                        <td style='padding: 25px 0px 0px;font-family: &quot;Segoe UI&quot;, Tahoma, Verdana, Arial, sans-serif;font-size: 14px;color: white;'> Best regards, </td>
                    </tr>
                    <tr>
                        <td>
                            <img src='https://woodgrovedemo.com/Company-branding/headerlogo.png' height='20'>
                        </td>
                        <td style='font-family: &quot;Segoe UI&quot;, Tahoma, Verdana, Arial, sans-serif;font-size: 14px;color: white; text-align: center;'>
                            <a href='https://woodgrovedemo.com/Privacy' style='color: white; text-decoration: none;'>Privacy Statement</a>
                        </td>
                    </tr>
                </tbody>
                </table>
            </div>
            </body></html>";


            EmailSendOperation emailSendOperation = await emailClient.SendAsync(
                Azure.WaitUntil.Started,
                emailSender,
                authMethod.AuthValue,
                subject,
                htmlContent);

        }
        catch (System.Exception ex)
        {
            throw ex;
        }
    }
}
