using System;
using static H5.Core.dom;
using BlazorJs.Core;
using Microsoft.AspNetCore.Components;
using BlazorJs.Core.Components;
using BlazorJs.Sample.Layout;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BlazorJs.Core.Components.LiteRouting;


namespace BlazorJs.Sample.Pages
{
    public partial class Breakout : Microsoft.AspNetCore.Components.ComponentBase
    {
        public static void RegisterRoute()
        {
            RouteTableFactory.Register<Breakout>("/Breakout");
        }


        protected override void BuildRenderTree(IUIFrame __frame0, object __key = null)
        {
            world = __frame0.Element("canvas", (ref UIElementAttribute __attribute) =>
            {
                __attribute.Set("width", "480");
                __attribute.Set("height", "320");
            }, null, sequenceNumber: -2070903490);
        }

    }
}

