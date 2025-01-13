
using System.Text.Json.Serialization;

public class SendCodeResponse
{
    public SendCodeResponse()
    {
        
    }

    public SendCodeResponse(string error)
    {
        this.Error = error;
    }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Error { get; set; }
}

