using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.PixelFormats;
using System.Security.Cryptography;
using Microsoft.AspNetCore.StaticFiles;

namespace AnnualHealthCheckJs.Controllers
{
    public class ImageController : Controller
    {
        private const int MaxDimension = 1000;
        private const int Size = 80;
        private static string[] AllowedMimeTypes = new[] { "image/jpeg", "image/png", "image/gif" };

        private readonly IHostingEnvironment _environment;

        public ImageController(IHostingEnvironment environment)
        {
            _environment = environment;
        }

        public IActionResult UnionLogo()
        {
            var imagePath = $"imgs/Union-Bank-logo.png";

            var mimeType = GetContentType(imagePath);
            if (Array.IndexOf(AllowedMimeTypes, mimeType) < 0)
            {
                return BadRequest("Disallowed image format");
            }

            // Locate source image on disk
            var fileInfo = _environment.WebRootFileProvider.GetFileInfo(imagePath);
            if (!fileInfo.Exists)
            {
                return NotFound();
            }

            var eTag = GenerateETag(Encoding.UTF8.GetBytes($"{fileInfo.LastModified.ToString("s")}-{fileInfo.Length}"));
            HttpContext.Response.Headers["ETag"] = eTag;

            var match = HttpContext.Request.Headers["If-None-Match"].FirstOrDefault();
            if (eTag == match)
            {
                return StatusCode(304);
            }

            byte[] data;
            using (var outputStream = new MemoryStream())
            {
                using (var inputStream = fileInfo.CreateReadStream())
                using (Image<Rgba32> image = Image.Load<Rgba32>(inputStream))
                {
                    image.Mutate(x => x.Resize(ScaleWidth(image.Height, image.Width), Size));
                    image.SaveAsPng(outputStream);
                }

                data = outputStream.ToArray();
            }
            return File(data, mimeType, fileInfo.Name);
        }

        public IActionResult AnchorhmoLogo()
        {
            var imagePath = $"imgs/anchor_logo_new.png";

            var mimeType = GetContentType(imagePath);
            if (Array.IndexOf(AllowedMimeTypes, mimeType) < 0)
            {
                return BadRequest("Disallowed image format");
            }

            // Locate source image on disk
            var fileInfo = _environment.WebRootFileProvider.GetFileInfo(imagePath);
            if (!fileInfo.Exists)
            {
                return NotFound();
            }

            var eTag = GenerateETag(Encoding.UTF8.GetBytes($"{fileInfo.LastModified.ToString("s")}-{fileInfo.Length}"));
            HttpContext.Response.Headers["ETag"] = eTag;

            var match = HttpContext.Request.Headers["If-None-Match"].FirstOrDefault();
            if (eTag == match)
            {
                return StatusCode(304);
            }

            byte[] data;
            using (var outputStream = new MemoryStream())
            {
                using (var inputStream = fileInfo.CreateReadStream())
                using (Image<Rgba32> image = Image.Load<Rgba32>(inputStream))
                {
                    image.Mutate(x => x.Resize(ScaleWidth(image.Height, image.Width), Size));
                    image.SaveAsPng(outputStream);
                }

                data = outputStream.ToArray();
            }
            return File(data, mimeType, fileInfo.Name);
        }

        public IActionResult RodingLogo()
        {
            var imagePath = $"imgs/roding_logo_new.png";

            var mimeType = GetContentType(imagePath);
            if (Array.IndexOf(AllowedMimeTypes, mimeType) < 0)
            {
                return BadRequest("Disallowed image format");
            }

            // Locate source image on disk
            var fileInfo = _environment.WebRootFileProvider.GetFileInfo(imagePath);
            if (!fileInfo.Exists)
            {
                return NotFound();
            }

            var eTag = GenerateETag(Encoding.UTF8.GetBytes($"{fileInfo.LastModified.ToString("s")}-{fileInfo.Length}"));
            HttpContext.Response.Headers["ETag"] = eTag;

            var match = HttpContext.Request.Headers["If-None-Match"].FirstOrDefault();
            if (eTag == match)
            {
                return StatusCode(304);
            }

            byte[] data;
            using (var outputStream = new MemoryStream())
            {
                using (var inputStream = fileInfo.CreateReadStream())
                using (Image<Rgba32> image = Image.Load<Rgba32>(inputStream))
                {
                    image.Mutate(x => x.Resize(ScaleWidth(image.Height, image.Width), Size));
                    image.SaveAsPng(outputStream);
                }

                data = outputStream.ToArray();
            }
            return File(data, mimeType, fileInfo.Name);
        }


        public IActionResult Logo(string id)
        {
            string imagePath = $"images/unknown_file.png";

            if (!string.IsNullOrEmpty(id))
            {
                imagePath = $"uploads/logos/{id}.png";
            }

            var mimeType = GetContentType(imagePath);
            if (Array.IndexOf(AllowedMimeTypes, mimeType) < 0)
            {
                return BadRequest("Disallowed image format");
            }

            // Locate source image on disk
            var fileInfo = _environment.WebRootFileProvider.GetFileInfo(imagePath);
            if (!fileInfo.Exists)
            {
                return NotFound();
            }

            var eTag = GenerateETag(Encoding.UTF8.GetBytes($"{fileInfo.LastModified.ToString("s")}-{fileInfo.Length}"));
            HttpContext.Response.Headers["ETag"] = eTag;

            var match = HttpContext.Request.Headers["If-None-Match"].FirstOrDefault();
            if (eTag == match)
            {
                return StatusCode(304);
            }

            byte[] data;
            using (var outputStream = new MemoryStream())
            {
                using (var inputStream = fileInfo.CreateReadStream())
                using (Image<Rgba32> image = Image.Load<Rgba32>(inputStream))
                {
                    image.Mutate(x => x.Resize(ScaleWidth(image.Height, image.Width), Size));
                    image.SaveAsPng(outputStream);
                }

                data = outputStream.ToArray();
            }
            return File(data, mimeType, fileInfo.Name);
        }

        public IActionResult Signature(string id)
        {
            string imagePath = $"images/unknown_file.png";

            if (!string.IsNullOrEmpty(id))
            {
                imagePath = $"uploads/signatures/{id}.png";
            }

            var mimeType = GetContentType(imagePath);
            if (Array.IndexOf(AllowedMimeTypes, mimeType) < 0)
            {
                return BadRequest("Disallowed image format");
            }

            // Locate source image on disk
            var fileInfo = _environment.WebRootFileProvider.GetFileInfo(imagePath);
            if (!fileInfo.Exists)
            {
                return NotFound();
            }

            var eTag = GenerateETag(Encoding.UTF8.GetBytes($"{fileInfo.LastModified.ToString("s")}-{fileInfo.Length}"));
            HttpContext.Response.Headers["ETag"] = eTag;

            var match = HttpContext.Request.Headers["If-None-Match"].FirstOrDefault();
            if (eTag == match)
            {
                return StatusCode(304);
            }

            byte[] data;
            using (var outputStream = new MemoryStream())
            {
                using (var inputStream = fileInfo.CreateReadStream())
                using (Image<Rgba32> image = Image.Load<Rgba32>(inputStream))
                {
                    image.Mutate(x => x.Resize(ScaleWidth(image.Height, image.Width), Size));
                    image.SaveAsPng(outputStream);
                }

                data = outputStream.ToArray();
            }
            return File(data, mimeType, fileInfo.Name);
        }


        private string GetContentType(string path)
        {
            string result;
            return new FileExtensionContentTypeProvider().TryGetContentType(path, out result) ? result : null;
        }

        private string GenerateETag(byte[] data)
        {
            string ret = string.Empty;

            using (var md5 = MD5.Create())
            {
                var hash = md5.ComputeHash(data);
                string hex = BitConverter.ToString(hash);
                ret = hex.Replace("-", "");
            }

            return ret;
        }

        private int ScaleWidth(int height, int width)
        {
            var scaledWidth = (double)Size / (double)height * (double)width;

            return (int)Math.Round(scaledWidth);
        }

    }
}