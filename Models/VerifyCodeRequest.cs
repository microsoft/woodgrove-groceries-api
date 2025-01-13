
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

public class VerifyCodeRequest
{
    public required string VerificationCode { get; set; }
}

