using System;
using static H5.Core.dom;
using BlazorJs.Core;
using Microsoft.AspNetCore.Components;
using BlazorJs.Core.Components;
using BlazorJs.Sample.Layout;
using BlazorJs.Core.Components.LiteRouting;


namespace BlazorJs.Sample.Pages
{
    public partial class Home : ComponentBase
    {
        public static void RegisterRoute()
        {
            RouteTableFactory.Register<Home>("/");
        }


        protected override void BuildRenderTree(IUIFrame __frame0, object __key = null)
        {
            __frame0.Component<PageTitle>((__component0) =>
            {

                __component0.ChildContent = (__frame1, __key1) =>
                {
                    __frame1.Text("Home", key: __key1, sequenceNumber: -1658846935);
                };
            }, sequenceNumber: -1658846934);
            __frame0.Markup("<h1>Hello, worlds!</h1>", sequenceNumber: -1658846933);
            __frame0.Text("Welcome to your new app.", sequenceNumber: -1658846932);
            __frame0.Component<Counter>(null, sequenceNumber: -1658846931);
        }

    }
}

