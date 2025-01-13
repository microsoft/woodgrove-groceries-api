
using System.Text.Json.Serialization;

public class SendCodeRequest
{
    public required string AuthValue { get; set; }
    public required AuthMethodType AuthType { get; set; }
}

