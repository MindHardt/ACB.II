﻿using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Web;
using ACB.II.JsInterop;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace ACB.II.Components.Pages;

public partial class Minnies
{
    private const int MaxImageSize = 1024 * 10; // 10KB
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        WriteIndented = false
    };

    private string? _originalSvg;
    private string? _preparedSvg;
    private readonly List<Minnie> _minnies = new();

    private IBrowserFile? _uploadedFile;
    private string _uploadedName = string.Empty;

    [Inject]
    public HttpClient HttpClient { get; set; } = null!;
    [Inject]
    public DownloadJsInterop DownloadJsInterop { get; set; } = null!;

    private async Task CreateMinnie()
    {
        if (_uploadedFile is null || string.IsNullOrWhiteSpace(_uploadedName))
        {
            return;
        }

        var image = await _uploadedFile.RequestImageFileAsync("jpg", 128, 256);
        await using var imageStream = image.OpenReadStream();
        var ms = new MemoryStream();
        await imageStream.CopyToAsync(ms);

        var base64 = Convert.ToBase64String(ms.ToArray());
        var minnie = new Minnie(_uploadedName, base64, MinnieSize.Default);

        _minnies.Add(minnie);
    }

    private void UploadMinnieFile(InputFileChangeEventArgs e)
        => _uploadedFile = e.File;

    private async Task ImportMinnies(InputFileChangeEventArgs e)
    {
        const int fileSize = 0xFFFFFF; // 16MB
        await using var contentStream = e.File.OpenReadStream(fileSize);
        var ms = new MemoryStream();
        await contentStream.CopyToAsync(ms);

        var json = Encoding.UTF8.GetString(ms.ToArray());
        var importedMinnies = JsonSerializer.Deserialize<Minnie[]>(json, JsonOptions)!;

        _minnies.AddRange(importedMinnies);
    }

    private void DeleteMinnie(Minnie minnie)
    {
        _minnies.Remove(minnie);
        StateHasChanged();
    }

    private void CloneMinnie(Minnie minnie)
    {
        var index = _minnies.IndexOf(minnie);
        var clone = minnie with { };

        _minnies.Insert(index, clone);
        StateHasChanged();
    }

    protected override async Task OnInitializedAsync()
    {
        _originalSvg = TransformOriginalSvg(await HttpClient.GetStringAsync("/svg/minnies.svg"));

        await base.OnInitializedAsync();
    }

    private const string DefaultPortraitBase64 = // 1 pixel of 0xFFFFFF in jpg
        "/9j/4AAQSkZJRgABAQEAYABgAAD/4QBoRXhpZgAATU0AKgAAAAgABAEaAAUAAAABAAAAPgEbAAUAAAABAAAARgEoAAMAAAABAAIAAAExAAIAAA" +
        "ARAAAATgAAAAAAAABgAAAAAQAAAGAAAAABcGFpbnQubmV0IDUuMC4xMQAA/9sAQwABAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEB" +
        "AQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEB/9sAQwEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQ" +
        "EBAQEBAQEBAQEBAQEBAQEB/8AAEQgAAQABAwESAAIRAQMRAf/EAB8AAAEFAQEBAQEBAAAAAAAAAAABAgMEBQYHCAkKC//EALUQAAIBAwMCBAMF" +
        "BQQEAAABfQECAwAEEQUSITFBBhNRYQcicRQygZGhCCNCscEVUtHwJDNicoIJChYXGBkaJSYnKCkqNDU2Nzg5OkNERUZHSElKU1RVVldYWVpjZG" +
        "VmZ2hpanN0dXZ3eHl6g4SFhoeIiYqSk5SVlpeYmZqio6Slpqeoqaqys7S1tre4ubrCw8TFxsfIycrS09TV1tfY2drh4uPk5ebn6Onq8fLz9PX2" +
        "9/j5+v/EAB8BAAMBAQEBAQEBAQEAAAAAAAABAgMEBQYHCAkKC//EALURAAIBAgQEAwQHBQQEAAECdwABAgMRBAUhMQYSQVEHYXETIjKBCBRCka" +
        "GxwQkjM1LwFWJy0QoWJDThJfEXGBkaJicoKSo1Njc4OTpDREVGR0hJSlNUVVZXWFlaY2RlZmdoaWpzdHV2d3h5eoKDhIWGh4iJipKTlJWWl5iZ" +
        "mqKjpKWmp6ipqrKztLW2t7i5usLDxMXGx8jJytLT1NXW19jZ2uLj5OXm5+jp6vLz9PX29/j5+v/aAAwDAQACEQMRAD8A/v4ooA//2Q==";

    private static string TransformOriginalSvg(string svg)
    {
        var i = 1;
        return DefaultPortraitRegex.Replace(svg, _ => $"${{avatar_{i++}}}");
    }

    private string PrepareSvg()
    {
        if (_originalSvg is null)
        {
            return string.Empty;
        }
        var processed = PreparationRegex().Replace(_originalSvg, match =>
        {
            var name = match.Groups["Name"].Value;

            var index = int.Parse(name[(name.IndexOf('_') + 1)..]) - 1;
            var minnie = _minnies.Count > index
                ? _minnies[index]
                : null;

            if (minnie is null)
            {
                return string.Empty;
            }

            return name.StartsWith("name")
                ? HttpUtility.HtmlEncode(minnie.Name)
                : minnie.AvatarBase64;
        });
        return processed;
    }

    [GeneratedRegex(@"\$\{(?<Name>[a-z_0-9]+)\}")]
    private static partial Regex PreparationRegex();
    private static Regex DefaultPortraitRegex => new(Regex.Escape(DefaultPortraitBase64));

    private async Task DownloadJson()
    {
        var json = JsonSerializer.Serialize(_minnies, JsonOptions);
        await DownloadJsInterop.DownloadAsync(json, "minnies.json");
    }

    private async Task DownloadSvg()
    {
        if (_preparedSvg is null)
        {
            return;
        }

        await DownloadJsInterop.DownloadAsync(_preparedSvg, "minnies.svg");
    }
}

public record Minnie(string Name, string AvatarBase64, MinnieSize Size);

public enum MinnieSize
{
    Default,
    Big
}