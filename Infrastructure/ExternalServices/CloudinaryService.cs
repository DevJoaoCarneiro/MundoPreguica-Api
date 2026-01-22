using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.ExternalServices
{
    public class CloudinaryService : IImageUploadService
    {
        private readonly Cloudinary _cloudinary;
        private readonly ILogger<CloudinaryService> _logger;

        public CloudinaryService(IConfiguration config, ILogger<CloudinaryService> logger)
        {
            var account = new Account(
            config["Cloudinary:CloudName"],
            config["Cloudinary:ApiKey"],
            config["Cloudinary:ApiSecret"]
            );
            _logger = logger;
            _cloudinary = new Cloudinary(account);
        }
        public async Task<string> UploadImageAsync(IFormFile file)
        {
            try
            {
                _logger.LogInformation("Iniciando upload da imagem: {FileName}", file?.FileName);

                if (file == null || file.Length == 0)
                {
                    _logger.LogWarning("Tentativa de upload com arquivo nulo ou vazio.");
                    return null;
                }

                using var stream = file.OpenReadStream();

                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(file.FileName, stream),
                    Folder = "catalogo-produtos",
                    UseFilename = true,
                    UniqueFilename = true,
                    Transformation = new Transformation().Quality("auto").FetchFormat("auto")
                };

                var uploadResult = await _cloudinary.UploadAsync(uploadParams);

                if (uploadResult.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    _logger.LogError("Cloudinary retornou erro: {Message}", uploadResult.Error?.Message);
                    return null; 
                }

                _logger.LogInformation("Upload concluído com sucesso. URL: {Url}", uploadResult.SecureUrl);
                return uploadResult.SecureUrl.ToString();
            }
            catch (Exception ex)
            {

                _logger.LogCritical(ex, "Falha crítica na comunicação com o Cloudinary.");
                return null;
            }
        }
    }
}
