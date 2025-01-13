
using System.Text.Json.Serialization;

public enum AuthMethodType
{
    SignInEmail,
    EmailMfa,
    PhoneMfa
}

public class AuthMethod
{
    public string UID { get; set; }
    public string AuthValue { get; set; }
    public AuthMethodType AuthType { get; set; }
    public string VerificationCode { get; set; }
    public int MessagesSent { get; set; }
    public int Validations { get; set; }
}

