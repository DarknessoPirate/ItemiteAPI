namespace Domain.DTOs.File;

public class FileWrapper(string fileName, long fileLength, string contentType, Stream fileStream)
{
    public string FileName => !string.IsNullOrWhiteSpace(fileName) 
    ? fileName 
    : Guid.NewGuid().ToString("N")[..12];
    public long Length => fileLength;
    public string ContentType => contentType;
    public double FileSizeInMB => Math.Round(fileLength / (1024.0 * 1024.0), 2);
    public Stream FileStream => fileStream;
}
