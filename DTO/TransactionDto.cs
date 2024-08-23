public class TransactionDto
{
    public DateTime Date { get; set; }
    public string Action { get; set; } 
    // public string User { get; set; } 
    public decimal RemainingBalance { get; set; }
    public string From { get; set; } 
    public string To { get; set; }
    public decimal Amount { get; set; }
}
