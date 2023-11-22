using System.Text;
using Microsoft.JSInterop;
using Microsoft.VisualBasic;

namespace ACB.II.JsInterop;

[RegisterScoped]
public class DownloadJsInterop(IJSRuntime jsRuntime) : JsInteropBase(jsRuntime)
{
    protected override string JsFilePath => "js/download.js";

    public ValueTask DownloadAsync(string content, string fileName)
    {
        var ms = new MemoryStream(Encoding.UTF8.GetBytes(content));

        return DownloadAsync(ms, fileName);
    }

    public async ValueTask DownloadAsync(Stream stream, string fileName)
    {
        var module = await GetModuleAsync();
        DotNetStreamReference streamRef = new(stream);

        await module.InvokeVoidAsync("downloadFile", streamRef, fileName);
    }
}