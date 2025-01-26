using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using RecipePlatform.RecipeManagementService.Contracts.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipePlatform.RecipeManagementService.Application.Services
{
    public class CloudinaryService : ICloudinaryService
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryService(string cloudName, string apiKey, string apiSecret)
        {
            var account = new Account(cloudName, apiKey, apiSecret);
            _cloudinary = new Cloudinary(account);
        }

        // Upload a single image to Cloudinary and return the image URL
        public async Task<string> UploadImageAsync(IFormFile file)
        {
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, file.OpenReadStream())
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);
            return uploadResult?.SecureUrl?.ToString();
        }

        // Upload multiple images and return a list of image URLs
        public async Task<List<string>> UploadImagesAsync(List<IFormFile> files)
        {
            var imageUrls = new List<string>();
            foreach (var file in files)
            {
                var url = await UploadImageAsync(file);
                if (!string.IsNullOrEmpty(url))
                {
                    imageUrls.Add(url);
                }
            }
            return imageUrls;
        }
    }
}
