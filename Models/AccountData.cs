
using System.Text.Json.Serialization;

public class AccountData
{
    public AccountData(){

    }

    public AccountData(string error){
        this.Error = error;
    }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string Error { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string DisplayName { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<Purchase> Purchases { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Payment Payment { get; set; }
}

public class Payment
{

    public string AccessTokenToCallThePaymentAPI { get; set; }
    public string CardNumber { get; set; }
    public string NameOnCard { get; set; }
    public string ExpirationDate { get; set; }
}

public class Purchase
{
    public string date { get; set; }
    public string ID { get; set; }
    public int items { get; set; }
    public double total { get; set; }
}

