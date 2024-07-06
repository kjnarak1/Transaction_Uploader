using Transaction_Uploader.Models;

namespace Transaction_Uploader.Interfaces
{
    public interface IFileProcessor
    {
        Task<ValidationResult> ProcessFileAsync(IFormFile file);
    }
}
