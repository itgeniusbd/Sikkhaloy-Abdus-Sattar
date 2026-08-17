using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Sikkhaloy.Web;

public static class WebRender
{
    public static readonly IComponentRenderMode Mode = new InteractiveServerRenderMode(prerender: false);
}
