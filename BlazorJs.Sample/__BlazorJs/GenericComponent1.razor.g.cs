using System;
using static H5.Core.dom;
using BlazorJs.Core;
using Microsoft.AspNetCore.Components;
using BlazorJs.Core.Components;
using BlazorJs.Sample.Layout;
using System.Collections.Generic;
using System.Text;


namespace BlazorJs.Sample
{
    public partial class GenericComponent1<T1, T2> : Microsoft.AspNetCore.Components.ComponentBase
    {

        protected override void BuildRenderTree(IUIFrame __frame0, object __key = null)
        {
            __frame0.Component<GenericComponent2<T2, double>>(null, sequenceNumber: 1089727415);
        }

    }
}

