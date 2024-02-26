using System.Security.Cryptography;

namespace PhotoLibraryCleaner.Lib;

public static class FileHelpers
{
    public static bool IsPhoto(this string filePath)
    {
        // Get the file extension in lowercase for case-insensitive comparison
        string extension = Path.GetExtension(filePath);

        // Check against a list of common photo file extensions
        return extension is Constants.FileExtensions.Image.JPEGFileExtension or
            Constants.FileExtensions.Image.JPGFileExtension or
            Constants.FileExtensions.Image.PNGFileExtension or
            Constants.FileExtensions.Image.TIFFFileExtension or
            Constants.FileExtensions.Image.TIFFileExtension or
            Constants.FileExtensions.Image.GIFFileExtension or
            Constants.FileExtensions.Image.BMPFileExtension or
            Constants.FileExtensions.Image.HEIFFileExtension or
            Constants.FileExtensions.Image.HEICFileExtension or
            Constants.FileExtensions.Image.WEBPFileExtension;
    }

    public static bool IsZip(this string filePath)
    {
        // Get the file extension in lowercase for case-insensitive comparison
        string extension = Path.GetExtension(filePath);

        // Check against a list of common .zips - we're not gonna bother with gzip or whatever right now        
        return extension == Constants.FileExtensions.ZipFileExtension;
    }

    private static string GetChecksum(string file)
    {
        using var stream = new BufferedStream(File.OpenRead(file), 1200000);
        using var sha = SHA256.Create();
        byte[] checksum = sha.ComputeHash(stream);
        return BitConverter.ToString(checksum).Replace("-", String.Empty);
    }
}