using System;
using System.IO;
using System.Web;

namespace RecipeShare.Models.Services
{
    public class FileService
    {
        private readonly string _rootPath;
        private readonly string _uploadDir;

        public FileService(string serverMapPath)
        {
            _rootPath = serverMapPath;
            _uploadDir = Path.Combine(serverMapPath, "Uploads");
            if (!Directory.Exists(_uploadDir))
                Directory.CreateDirectory(_uploadDir);
        }

        public string SaveFile(HttpPostedFileBase file, string subfolder = "")
        {
            if (file == null || file.ContentLength == 0) return null;

            var folder = string.IsNullOrEmpty(subfolder) ? _uploadDir : Path.Combine(_uploadDir, subfolder);
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

            var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
            var path = Path.Combine(folder, fileName);
            file.SaveAs(path);

            return string.IsNullOrEmpty(subfolder)
                ? $"/Uploads/{fileName}"
                : $"/Uploads/{subfolder}/{fileName}";
        }

        public void DeleteFile(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath)) return;
            var trimmed = relativePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            var fullPath = Path.Combine(_rootPath, trimmed);
            if (File.Exists(fullPath)) File.Delete(fullPath);
        }
    }
}
