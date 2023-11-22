using Microsoft.JSInterop;

namespace ACB.II.JsInterop;

public abstract class JsInteropBase : IAsyncDisposable
{
    /// <summary>
    /// The path of the .js file from the local wwwroot.
    /// The leading '/' symbol is not required.
    /// </summary>
    protected abstract string JsFilePath { get; }
    
    /// <summary>
    /// The underlying <see cref="IJSRuntime"/> for using
    /// default js functions.
    /// </summary>
    protected IJSRuntime Runtime { get; }

    /// <summary>
    /// The underlying <see cref="IJSRuntime"/> object, wrapped for lazy evaluation.
    /// </summary>
    private readonly Lazy<Task<IJSObjectReference>> _moduleTask;
    
    /// <summary>
    /// Gets the imported javascript module.
    /// </summary>
    /// <returns></returns>
    protected Task<IJSObjectReference> GetModuleAsync() => _moduleTask.Value;

    protected JsInteropBase(IJSRuntime jsRuntime)
    {
        Runtime = jsRuntime;
        var normalizedFileName = JsFilePath.StartsWith('/')
            ? JsFilePath
            : $"/{JsFilePath}";
        _moduleTask = new Lazy<Task<IJSObjectReference>>(() => 
            Runtime.InvokeAsync<IJSObjectReference>("import", normalizedFileName).AsTask());
    }
    
    public async ValueTask DisposeAsync()
    {
        if (_moduleTask.IsValueCreated)
        {
            IJSObjectReference module = await _moduleTask.Value;
            await module.DisposeAsync();
        }
    }
    
    /// <summary>
    /// The default javascript alert function that displays message in a floating window.
    /// </summary>
    /// <param name="message">The additional info displayed.</param>
    /// <returns></returns>
    public ValueTask AlertAsync(string message)
        => Runtime.InvokeVoidAsync("alert", message);

    /// <summary>
    /// The default javascript confirm function that asks for user confirmation in a floating window.
    /// </summary>
    /// <param name="message">The additional info displayed.</param>
    /// <returns><see langword="true"/> if user agrees, otherwise <see langword="false"/>.</returns>
    public ValueTask<bool> ConfirmAsync(string message)
        => Runtime.InvokeAsync<bool>("confirm", message);
    
    /// <summary>
    /// The default javascript prompt function that asks for user input in a floating window.
    /// </summary>
    /// <param name="message">The additional info displayed.</param>
    /// <returns>A value provided by user.</returns>
    public ValueTask<string> PromptAsync(string message)
        => Runtime.InvokeAsync<string>("prompt", message);
}