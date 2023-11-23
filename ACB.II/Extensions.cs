using System.Text;

namespace ACB.II;

public static class Extensions
{
    /// <summary>
    /// Loads <paramref name="stream"/> into a local <see cref="byte"/> array.
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public static async Task<byte[]> LoadToByteArrayAsync(this Stream stream, CancellationToken ct = default)
    {
        var ms = new MemoryStream();
        await stream.CopyToAsync(ms, ct);
        return ms.ToArray();
    }

    /// <summary>
    /// Loads <paramref name="stream"/> into a <see cref="string"/>
    /// using provided <paramref name="encoding"/> or <see cref="Encoding.UTF8"/> if emitted.
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="encoding"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public static async Task<string> LoadToStringAsync(this Stream stream, Encoding? encoding = null, CancellationToken ct = default)
    {
        encoding ??= Encoding.UTF8;

        return encoding.GetString(await LoadToByteArrayAsync(stream, ct));
    }
}