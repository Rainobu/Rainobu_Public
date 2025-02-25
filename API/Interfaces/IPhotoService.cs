using System.Threading.Tasks;
using API.Entities;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;

namespace API.Interfaces
{
    public interface IPhotoService
    {
        Task<ImageUploadResult> AddPhotoAsync(IFormFile file);
        Task<ImageUploadResult> AddBannerAsync(IFormFile file);
        Task<DeletionResult> DeletePhotoAsync(string publicId);

    }
}