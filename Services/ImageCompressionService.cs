// Validate file type
var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
var extension = Path.GetExtension(file.FileName).ToLower();
if (!allowedExtensions.Contains(extension)) return false;