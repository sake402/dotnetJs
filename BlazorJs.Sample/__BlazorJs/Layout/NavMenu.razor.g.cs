using System;
using static H5.Core.dom;
using BlazorJs.Core;
using Microsoft.AspNetCore.Components;
using BlazorJs.Core.Components;
using BlazorJs.Sample.Layout;
using Microsoft.AspNetCore.Components.Routing;


namespace BlazorJs.Sample.Layout
{
    public partial class NavMenu : ComponentBase
    {

        protected override void BuildRenderTree(IUIFrame __frame0, object __key = null)
        {
            __frame0.Markup("<div class=\"top-row ps-3 navbar navbar-dark\">\r\n    <div class=\"container-fluid\">\r\n        <a class=\"navbar-brand\" href=\"\">MyBlazorWeb</a>\r\n    </div>\r\n</div>", sequenceNumber: -1265218882);
            __frame0.Markup("<input type=\"checkbox\" title=\"Navigation menu\" class=\"navbar-toggler\" />", sequenceNumber: -1265218881);
            __frame0.Element("div", (ref UIElementAttribute __attribute) =>
            {
                __attribute.Set("class", "nav-scrollable");
                __attribute.Set("onclick", "document.querySelector('.navbar-toggler').click()");
            }, (__frame1, __key1) =>
            {
                __frame1.Element("nav", (ref UIElementAttribute __attribute) =>
                {
                    __attribute.Set("class", "flex-column");
                }, (__frame2, __key2) =>
                {
                    __frame2.Element("div", (ref UIElementAttribute __attribute) =>
                    {
                        __attribute.Set("class", "nav-item px-3");
                    }, (__frame3, __key3) =>
                    {
                        __frame3.Component<NavLink>((__component3) =>
                        {
                            __component3["class"] = "nav-link";
                            __component3["href"] = "/";
                            __component3.Match = NavLinkMatch.All;
                            __component3.ChildContent = (__frame4, __key4) =>
                            {
                                __frame4.Markup("<span class=\"bi bi-house-door-fill\" aria-hidden=\"true\"></span>", key: __key4, sequenceNumber: -1265218880);
                                __frame4.Text("Home\r\n            ", key: __key4, sequenceNumber: -1265218879);
                            };
                        }, key: __key3, sequenceNumber: -1265218878);
                    }, key: __key2, sequenceNumber: -1265218877);
                    __frame2.Element("div", (ref UIElementAttribute __attribute) =>
                    {
                        __attribute.Set("class", "nav-item px-3");
                    }, (__frame3, __key3) =>
                    {
                        __frame3.Component<NavLink>((__component3) =>
                        {
                            __component3["class"] = "nav-link";
                            __component3["href"] = "counter";
                            __component3.ChildContent = (__frame4, __key4) =>
                            {
                                __frame4.Markup("<span class=\"bi bi-plus-square-fill\" aria-hidden=\"true\"></span>", key: __key4, sequenceNumber: -1265218876);
                                __frame4.Text("Counter\r\n            ", key: __key4, sequenceNumber: -1265218875);
                            };
                        }, key: __key3, sequenceNumber: -1265218874);
                    }, key: __key2, sequenceNumber: -1265218873);
                    __frame2.Element("div", (ref UIElementAttribute __attribute) =>
                    {
                        __attribute.Set("class", "nav-item px-3");
                    }, (__frame3, __key3) =>
                    {
                        __frame3.Component<NavLink>((__component3) =>
                        {
                            __component3["class"] = "nav-link";
                            __component3["href"] = "weather";
                            __component3.ChildContent = (__frame4, __key4) =>
                            {
                                __frame4.Markup("<span class=\"bi bi-list-nested\" aria-hidden=\"true\"></span>", key: __key4, sequenceNumber: -1265218872);
                                __frame4.Text("Weather\r\n            ", key: __key4, sequenceNumber: -1265218871);
                            };
                        }, key: __key3, sequenceNumber: -1265218870);
                    }, key: __key2, sequenceNumber: -1265218869);
                    __frame2.Element("div", (ref UIElementAttribute __attribute) =>
                    {
                        __attribute.Set("class", "nav-item px-3");
                    }, (__frame3, __key3) =>
                    {
                        __frame3.Component<NavLink>((__component3) =>
                        {
                            __component3["class"] = "nav-link";
                            __component3["href"] = "Sudoku";
                            __component3.ChildContent = (__frame4, __key4) =>
                            {
                                __frame4.Markup("<span class=\"bi bi-list-nested\" aria-hidden=\"true\"></span>", key: __key4, sequenceNumber: -1265218868);
                                __frame4.Text("Sudoku\r\n            ", key: __key4, sequenceNumber: -1265218867);
                            };
                        }, key: __key3, sequenceNumber: -1265218866);
                    }, key: __key2, sequenceNumber: -1265218865);
                    __frame2.Element("div", (ref UIElementAttribute __attribute) =>
                    {
                        __attribute.Set("class", "nav-item px-3");
                    }, (__frame3, __key3) =>
                    {
                        __frame3.Component<NavLink>((__component3) =>
                        {
                            __component3["class"] = "nav-link";
                            __component3["href"] = "Breakout";
                            __component3.ChildContent = (__frame4, __key4) =>
                            {
                                __frame4.Markup("<span class=\"bi bi-list-nested\" aria-hidden=\"true\"></span>", key: __key4, sequenceNumber: -1265218864);
                                __frame4.Text("Breakout\r\n            ", key: __key4, sequenceNumber: -1265218863);
                            };
                        }, key: __key3, sequenceNumber: -1265218862);
                    }, key: __key2, sequenceNumber: -1265218861);
                }, key: __key1, sequenceNumber: -1265218860);
            }, sequenceNumber: -1265218859);
        }

    }
}

