using Microsoft.AspNetCore.Components;

namespace Sikkhaloy.App.Services;

public abstract class LocaleAwareComponent : ComponentBase, IDisposable
{
    [Inject]
    protected AppLocale Locale { get; set; } = default!;

    [CascadingParameter(Name = "AppLang")]
    public string? AppLang { get; set; }

    protected override void OnInitialized()
    {
        Locale.Changed += OnLocaleChanged;
    }

    private void OnLocaleChanged() => _ = InvokeAsync(StateHasChanged);

    public virtual void Dispose()
    {
        Locale.Changed -= OnLocaleChanged;
        GC.SuppressFinalize(this);
    }
}
