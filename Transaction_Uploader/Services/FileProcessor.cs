using Transaction_Uploader.Interfaces;
using Transaction_Uploader.Models;

namespace Transaction_Uploader.Services
{
    public class FileProcessor : IFileProcessor
    {
        public async Task<ValidationResult> ProcessFileAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return new ValidationResult { Record = "", ErrorMessage = "Invalid file." };
            }

            using (var stream = file.OpenReadStream())
            {
            }
            return new ValidationResult();
        }
    }
}
