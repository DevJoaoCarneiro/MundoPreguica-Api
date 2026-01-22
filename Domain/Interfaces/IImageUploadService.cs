using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Interfaces
{
    public interface IImageUploadService
    {
        Task<string> UploadImageAsync(IFormFile file);
    }
}
