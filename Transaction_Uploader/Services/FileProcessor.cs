using Transaction_Uploader.Interfaces;
using Transaction_Uploader.Models;

namespace Transaction_Uploader.Services
{
    public class FileProcessor : IFileProcessor
    {
        public async Task<ValidationResult> ProcessFileAsync(IFormFile file)
        {



            return new ValidationResult();
        }
    }
}
