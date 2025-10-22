namespace PROGA22025.Services
{
    using Microsoft.AspNetCore.Http;
    using System;
    using System.IO;
    using System.Linq;

    public class FileUploadService
    {
        private const long MaxFileSize = 5 * 1024 * 1024; // 5MB in bytes
        private static readonly string[] AllowedExtensions = { ".pdf", ".docx", ".xlsx" };

        /// <summary>
        /// Validates and uploads file to wwwroot/uploads folder
        /// </summary>
        public (bool success, string message, string fileName) UploadFile(IFormFile file)
        {
            try
            {
                // Validation 1: Check if file exists
                if (file == null || file.Length == 0)
                {
                    return (false, "No file selected or file is empty.", null);
                }

                // Validation 2: Check file size (max 5MB)
                if (file.Length > MaxFileSize)
                {
                    return (false, $"File size exceeds 5MB limit. Your file is {file.Length / 1024 / 1024}MB.", null);
                }

                // Validation 3: Check file extension
                string fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!AllowedExtensions.Contains(fileExtension))
                {
                    return (false, $"Invalid file type. Only PDF, DOCX, and XLSX files are allowed.", null);
                }

                // Validation 4: Check file name length
                if (file.FileName.Length > 255)
                {
                    return (false, "File name is too long. Maximum 255 characters.", null);
                }

                // Create uploads folder if it doesn't exist
                string uploadsPath = Path.Combine("wwwroot", "uploads");
                if (!Directory.Exists(uploadsPath))
                {
                    Directory.CreateDirectory(uploadsPath);
                }

                // Generate unique filename to avoid conflicts
                string uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
                string fullPath = Path.Combine(uploadsPath, uniqueFileName);

                // Save file to disk
                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    file.CopyTo(stream);
                }

                return (true, "File uploaded successfully.", uniqueFileName);
            }
            catch (Exception ex)
            {
                return (false, $"Error uploading file: {ex.Message}", null);
            }
        }

        /// <summary>
        /// Downloads file from wwwroot/uploads folder
        /// </summary>
        public (bool success, string message, byte[] fileData, string originalFileName) DownloadFile(string storedFileName, string originalFileName)
        {
            try
            {
                string fullPath = Path.Combine("wwwroot", "uploads", storedFileName);

                if (!File.Exists(fullPath))
                {
                    return (false, "File not found.", null, null);
                }

                byte[] fileData = File.ReadAllBytes(fullPath);

                return (true, "File retrieved successfully.", fileData, originalFileName);
            }
            catch (Exception ex)
            {
                return (false, $"Error downloading file: {ex.Message}", null, null);
            }
        }

        /// <summary>
        /// Gets file extension from filename
        /// </summary>
        public string GetFileExtension(string fileName)
        {
            return Path.GetExtension(fileName);
        }

        /// <summary>
        /// Deletes file from uploads folder (optional - for cleanup)
        /// </summary>
        public bool DeleteFile(string storedFileName)
        {
            try
            {
                string fullPath = Path.Combine("wwwroot", "uploads", storedFileName);

                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }
    }
}
