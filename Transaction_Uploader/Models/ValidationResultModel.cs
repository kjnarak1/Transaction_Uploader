namespace Transaction_Uploader.Models
{
    public class ValidationResult
    {
        public bool IsValid { get; set; } = false;
        public string ErrorMessage { get; set; }
    }
}
