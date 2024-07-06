namespace Transaction_Uploader.Models
{
    public class Transaction
    {
        public string TransactionId { get; set; }
        public decimal Amount { get; set; }
        public string CurrencyCode { get; set; }
        public DateTime TransactionDate { get; set; }
        public string Status { get; set; }
    }
    public class Currency
    {
        public string cc { get; set; }
        public string symbol { get; set; }
        public string name { get; set; }
        public string DisplayCurrencyInfo()
        {
            return $"{cc} : {name}({symbol})";
        }
    }
}
