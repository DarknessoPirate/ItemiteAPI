using System.Collections.Immutable;

namespace Domain.Enums;

public class SupportedFileTypes
{
    public const long MaxFileSize = 50 * 1024 * 1024; // 50MB
    
    public static ImmutableDictionary<FileType, string[]> MimeTypes = new Dictionary<FileType, string[]>
    {
        { FileType.Image, ["image/jpeg", "image/png","image/webp"] },
        { FileType.Video, ["video/mp4", "video/webm"] },
    }.ToImmutableDictionary();
    
    
    public static FileType GetFileTypeFromMimeType(string mimeType)
    {
        foreach (var kvp in MimeTypes.Where(kvp => kvp.Value.Contains(mimeType)))
        {
            return kvp.Key;
        }

        return FileType.Unsupported;
    }
    
    public static bool IsSupportedMimeType(string mimeType)
    {
        return MimeTypes.Values.Any(types => types.Contains(mimeType));
    }
}
