
using System.Text.Json.Serialization;

public class VerifyCodeResponse
{
    public VerifyCodeResponse()
    {
        ValidationPassed = false;
    }

    public VerifyCodeResponse(string error)
    {
        this.Error = error;
        ValidationPassed = false;
    }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Error { get; set; }

    public bool ValidationPassed { get; set; } = false;

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string AuthValue { get; set; }

    public AuthMethodType AuthType { get; set; }
}

