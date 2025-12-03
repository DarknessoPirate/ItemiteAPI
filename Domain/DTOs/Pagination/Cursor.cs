using System.Buffers.Text;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json;

namespace Domain.DTOs.Pagination;

public class Cursor
{
    public DateTime DateSent { get; set; }
    public int LastId { get; set; }

    public static string Encode(DateTime dateSent, int lastId)
    {
        var cursor = new Cursor { DateSent = dateSent, LastId = lastId };
        string json = JsonSerializer.Serialize(cursor);
        return Base64Url.EncodeToString(Encoding.UTF8.GetBytes(json));
    }

    public static Cursor? Decode(string? cursor)
    {
        if (string.IsNullOrEmpty(cursor))
            return null;

        try
        {
            string json = Encoding.UTF8.GetString(Base64Url.DecodeFromChars(cursor));
            return JsonSerializer.Deserialize<Cursor>(json);
        }
        catch
        {
            return null;
        }
    }
}
