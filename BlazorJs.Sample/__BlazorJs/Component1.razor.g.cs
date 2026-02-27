using System;
using static H5.Core.dom;
using BlazorJs.Core;
using Microsoft.AspNetCore.Components;
using BlazorJs.Core.Components;
using BlazorJs.Sample.Layout;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Routing;
using System.Reflection;
using System.IO;
using System.Threading;
using Microsoft.AspNetCore.Authorization;
using H5;
using BlazorJs.Core.Components.LiteRouting;


namespace BlazorJs.Sample
{
    public partial class Component1 : Microsoft.AspNetCore.Components.ComponentBase
    {
        public static void RegisterRoute()
        {
            RouteTableFactory.Register<Component1>("/C1", layout: typeof(MainLayout));
            RouteTableFactory.Register<Component1>("/C1/{Property1}", layout: typeof(MainLayout), routeParameterSetter: (component, name, value) =>
            {
                switch(name.ToLower())
                {
                    case "property1":
                        component.Property1 = value;
                        break;
                }
            });
        }

        protected override void InjectServices(IServiceProvider provider)
        {
            MHttp = provider.GetRequiredService<System.Net.Http.HttpClient>();
            Http = provider.GetRequiredService<System.Net.Http.HttpClient>();

        }

            
            
        int prope;
        RenderFragment V1()
        {
            return (__frame0, __key0) =>
            {
                __frame0.Element("span", null, (__frame1, __key1) =>
                {
                    __frame1.Content(prope, key: __key1, sequenceNumber: -2142132206);
                }, sequenceNumber: -2142132205);
            };
        }
        RenderFragment V2()
        {
            return (__frame1, __key1) =>
            {
                    __frame1.Element("span", null, (__frame2, __key2) =>
                    {
                        __frame2.Content(prope, key: __key2, sequenceNumber: -2142132204);
                    }, key: __key1, sequenceNumber: -2142132203);
            };
        }
            
        protected override void BuildRenderTree(IUIFrame __frame0, object __key = null)
        {
            __frame0.Markup("<div>ShoudUseMarkup</div>", sequenceNumber: -2142132274);
            __frame0.Element("a", (ref UIElementAttribute __attribute) =>
            {
                __attribute.Set("href", "C2");
            }, (__frame1, __key1) =>
            {
                __frame1.Markup("<span>Goto \r\n        \"\r\n        C2</span>", key: __key1, sequenceNumber: -2142132273);
                __frame1.Content(field2, key: __key1, sequenceNumber: -2142132272);
                __frame1.Text(" DEF", key: __key1, sequenceNumber: -2142132271);
            }, sequenceNumber: -2142132270);
            __frame0.Element("div", (ref UIElementAttribute __attribute) =>
            {
                __attribute.Set("@attributes", this.As<Dictionary<string, object>>());
            }, null, sequenceNumber: -2142132269);
            __frame0.Component<GenericComponent1<int, string>>((__component0) =>
            {
                __component0.Set("@attributes", this.As<Dictionary<string, object>>());
            }, sequenceNumber: -2142132268);
            __frame0.Component<CascadingValue<Component1>>((__component0) =>
            {
                __component0.Name = "C1";
                __component0.Value = this;
                __component0.IsFixed = true;
                __component0.ChildContent = (__frame1, __key1) =>
                {
                    __frame1.Component<Component2>((__component1) =>
                    {
                        __component1.Property1 = "1";
                        __component1.Property2 = 1;
                        __component1.ChildContent = (a) => (__frame2, __key2) =>
                        {
                            __frame2.Text("Component2.1 ", key: __key2, sequenceNumber: -2142132267);
                            __frame2.Content(a, key: __key2, sequenceNumber: -2142132266);
                            __frame2.Text("        ", key: __key2, sequenceNumber: -2142132265);
                            __frame2.Component<Component2>((__component2) =>
                            {
                                __component2.Property1 = "1";
                                __component2.Property2 = 1;
                                __component2.ChildContent = (aa) => (__frame3, __key3) =>
                                {
                                    __frame3.Text("Component2.Component2 ", key: __key3, sequenceNumber: -2142132264);
                                    __frame3.Content(a, key: __key3, sequenceNumber: -2142132263);
                                    __frame3.Text(" ", key: __key3, sequenceNumber: -2142132262);
                                    __frame3.Content(aa, key: __key3, sequenceNumber: -2142132261);
                                    __frame3.Text(" DEF\r\n        ", key: __key3, sequenceNumber: -2142132260);
                                };
                            }, key: __key2, sequenceNumber: -2142132259);
                        };
                    }, key: __key1, sequenceNumber: -2142132258);
                };
            }, sequenceNumber: -2142132257);
            __frame0.Component<Component2>((__component0) =>
            {
                __component0.Property1 = "1";
                __component0.Property2 = 1;
                __component0.ChildContent = (i) => (__frame1, __key1) =>
                {
                        __frame1.Text("Component2.2\r\n    ", key: __key1, sequenceNumber: -2142132256);
                };
                __component0.Property3 = (__frame1, __key1) =>
                {
                        __frame1.Text("Component2.Property3\r\n    ", key: __key1, sequenceNumber: -2142132255);
                };
                __component0.Property4 = (i) => (__frame1, __key1) =>
                {
                        __frame1.Text("Component2.Property4\r\n    ", key: __key1, sequenceNumber: -2142132254);
                };
            }, sequenceNumber: -2142132253);
            if ((field1 & 1) == 0)
            {
                input = __frame0.Element("input", (ref UIElementAttribute __attribute) =>
                {
                    var bindGetValue1 = field2;
                    __attribute.Set("value", bindGetValue1);
                    __attribute.Set("@onchange", EventCallback.Factory.CreateBinder(this, __value => field2 = __value, bindGetValue1));
                }, null, sequenceNumber: -2142132252);
            }
            __frame0.Element("button", (ref UIElementAttribute __attribute) =>
            {
                __attribute.Set("@onclick", EventCallback.Factory.Create(this, (Action)Clicked, EventCallbackFlags.StopPropagation | EventCallbackFlags.PreventDefault));
            }, (__frame1, __key1) =>
            {
                __frame1.Text("Click Me", key: __key1, sequenceNumber: -2142132251);
            }, sequenceNumber: -2142132250);
            for (int _i = 0; _i < 10; _i++)
            {
                __frame0.Frame((__frame1, __key1) =>
                {
                        __frame0.Content(_i, sequenceNumber: -2142132249);
                }, key: _i, sequenceNumber: -2142132248);
                var i = _i;
                __frame0.Element("div", (ref UIElementAttribute __attribute) =>
                {
                    __attribute.Set("class", $"abc {field1} {field2} {i}");
                }, (__frame1, __key1) =>
                {
                    __frame1.Content((i + "."), key: __key1, sequenceNumber: -2142132247);
                    __frame1.Text(" ABC ", key: __key1, sequenceNumber: -2142132246);
                    __frame1.Content(i, key: __key1, sequenceNumber: -2142132245);
                    __frame1.Text(" ", key: __key1, sequenceNumber: -2142132244);
                    __frame1.Content(field1, key: __key1, sequenceNumber: -2142132243);
                    __frame1.Text(" ", key: __key1, sequenceNumber: -2142132242);
                    __frame1.Content(field2, key: __key1, sequenceNumber: -2142132241);
                    __frame1.Text("    ", key: __key1, sequenceNumber: -2142132240);
                }, key: i, sequenceNumber: -2142132239);
            }
            __frame0.Element("div", (ref UIElementAttribute __attribute) =>
            {
                __attribute.Set("class", "def");
            }, (__frame1, __key1) =>
            {
                __frame1.Text("ABC ", key: __key1, sequenceNumber: -2142132238);
                __frame1.Content(field1, key: __key1, sequenceNumber: -2142132237);
                __frame1.Text("    ", key: __key1, sequenceNumber: -2142132236);
                __frame1.Content(view, key: __key1, sequenceNumber: -2142132235);
                __frame1.Text("    ", key: __key1, sequenceNumber: -2142132234);
                for (int _i = 0; _i < 10; _i++)
                {
                    var i = _i;
                    __frame1.Element("div", (ref UIElementAttribute __attribute) =>
                    {
                        __attribute.Set("class", $"abc {field1} {field2} {i}");
                    }, (__frame2, __key2) =>
                    {
                        __frame2.Content(i, key: __key2, sequenceNumber: -2142132233);
                        __frame2.Text(" . ABC ", key: __key2, sequenceNumber: -2142132232);
                        __frame2.Content(i, key: __key2, sequenceNumber: -2142132231);
                        __frame2.Text(" ", key: __key2, sequenceNumber: -2142132230);
                        __frame2.Content(field1, key: __key2, sequenceNumber: -2142132229);
                        __frame2.Text(" ", key: __key2, sequenceNumber: -2142132228);
                        __frame2.Content(field2, key: __key2, sequenceNumber: -2142132227);
                        __frame2.Text("        ", key: __key2, sequenceNumber: -2142132226);
                    }, key: i, sequenceNumber: -2142132225);
                }
            }, sequenceNumber: -2142132224);
            for (int _i = 0; _i < 10; _i++)
            {
                var i = _i;
                __frame0.Element("div", (ref UIElementAttribute __attribute) =>
                {
                    __attribute.Set("class", $"abc {field1} {field2} {i}");
                }, (__frame1, __key1) =>
                {
                    __frame1.Content(i, key: __key1, sequenceNumber: -2142132223);
                    __frame1.Text(" . DEF ", key: __key1, sequenceNumber: -2142132222);
                    __frame1.Content(field1, key: __key1, sequenceNumber: -2142132221);
                    __frame1.Text(" ", key: __key1, sequenceNumber: -2142132220);
                    __frame1.Content(field2, key: __key1, sequenceNumber: -2142132219);
                    __frame1.Text("    ", key: __key1, sequenceNumber: -2142132218);
                }, key: i, sequenceNumber: -2142132217);
            }
            
            
            RenderFragment view2 = (__frame1, __key1) =>
            {
                    __frame1.Element("span", null, (__frame2, __key2) =>
                    {
                        __frame2.Content(prope, key: __key2, sequenceNumber: -2142132216);
                    }, key: __key1, sequenceNumber: -2142132215);
            };
            
            __frame0.Content(view2, sequenceNumber: -2142132214);
            if (descriptor != null)
            {
                __frame0.Element("h1", null, (__frame1, __key1) =>
                {
                    __frame1.Text("Version: ", key: __key1, sequenceNumber: -2142132213);
                    __frame1.Content(descriptor.Version, key: __key1, sequenceNumber: -2142132212);
                }, sequenceNumber: -2142132211);
                __frame0.Element("h1", null, (__frame1, __key1) =>
                {
                    __frame1.Text("Size: ", key: __key1, sequenceNumber: -2142132210);
                    __frame1.Content(descriptor.Size, key: __key1, sequenceNumber: -2142132209);
                }, sequenceNumber: -2142132208);
            }
            __frame0.Content(html, sequenceNumber: -2142132207);
        }

    }
}

