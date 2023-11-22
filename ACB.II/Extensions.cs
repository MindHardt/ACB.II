using System.Text;

namespace ACB.II;

public static class Extensions
{
    public static async Task<byte[]> LoadToByteArrayAsync(this Stream stream)
    {
        var ms = new MemoryStream();
        await stream.CopyToAsync(ms);
        return ms.ToArray();
    }

    public static async Task<string> LoadToStringAsync(this Stream stream, Encoding? encoding = null)
    {
        encoding ??= Encoding.UTF8;

        return encoding.GetString(await stream.LoadToByteArrayAsync());
    }
}