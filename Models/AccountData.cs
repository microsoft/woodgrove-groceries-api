
public class AccountData
{
    public string DisplayName { get; set; }
    public List<Purchase> Purchases { get; set; }
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

