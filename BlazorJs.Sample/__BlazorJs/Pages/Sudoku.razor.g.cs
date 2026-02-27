using System;
using static H5.Core.dom;
using BlazorJs.Core;
using Microsoft.AspNetCore.Components;
using BlazorJs.Core.Components;
using BlazorJs.Sample.Layout;
using Microsoft.AspNetCore.Components.Forms;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BlazorJs.Core.Components.LiteRouting;


namespace BlazorJs.Sample.Pages
{
    public partial class Sudoku : Microsoft.AspNetCore.Components.ComponentBase
    {
        public static void RegisterRoute()
        {
            RouteTableFactory.Register<Sudoku>("/Sudoku");
        }


        protected override void BuildRenderTree(IUIFrame __frame0, object __key = null)
        {
            __frame0.Element("div", (ref UIElementAttribute __attribute) =>
            {
                __attribute.Set("class", "ltroot flex");
            }, (__frame1, __key1) =>
            {
                __frame1.Component<EditForm>((__component1) =>
                {
                    __component1.Model = this;
                    __component1["class"] = "bg-card mg-a";
                    __component1.OnSubmit = EventCallback.Factory.Create<Microsoft.AspNetCore.Components.Forms.EditContext>(this, (Action)CreateBoard);
                    __component1.ChildContent = (context) => (__frame2, __key2) =>
                    {
                        if (size == 0 || boards == null)
                        {
                            __frame2.Markup("<h3>Enter board size</h3>", key: __key2, sequenceNumber: 1863524173);
                            __frame2.Component<InputNumber<int>>((__component2) =>
                            {
                                var bindGetValue2 = size;
                                __component2.Value = bindGetValue2;
                                __component2.ValueChanged = EventCallback.Factory.CreateInferred(this, __value => size = __value, bindGetValue2);
                                __component2.ValueExpression = () => size;
                            }, key: __key2, sequenceNumber: 1863524174);
                            __frame2.Element("button", (ref UIElementAttribute __attribute) =>
                            {
                                __attribute.Set("type", "submit");
                                __attribute.Set("class", "mgx bg-primary");
                                __attribute.Set("@onclick", EventCallback.Factory.Create(this, (Action)CreateBoard));
                            }, (__frame3, __key3) =>
                            {
                                __frame3.Text("Continue", key: __key3, sequenceNumber: 1863524175);
                            }, key: __key2, sequenceNumber: 1863524176);
                        }
                        else
                        {
                            __frame2.Element("table", null, (__frame3, __key3) =>
                            {
                                for (int _y = 0; _y < size; _y++)
                                {
                                    var y = _y;
                                    __frame3.Element("tr", null, (__frame4, __key4) =>
                                    {
                                        for (int _x = 0; _x < size; _x++)
                                        {
                                            var x = _x;
                                            var board = boards[y, x];
                                            __frame4.Element("td", null, (__frame5, __key5) =>
                                            {
                                                __frame5.Component<InputNumber<int?>>((__component5) =>
                                                {
                                                    var bindGetValue5 = board.Entry;
                                                    __component5.Value = bindGetValue5;
                                                    __component5.ValueChanged = EventCallback.Factory.CreateInferred(this, (__value) =>
                                                    {
                                                        board.Entry = __value;
                                                        Validate(x, y);
                                                    }, bindGetValue5);
                                                    __component5.ValueExpression = () => board.Entry;

                                                    __component5["readonly"] = (board.IsFixed);
                                                    __component5["class"] = ($"text-center bd-0 wx-04 hx-04 {(board.IsFixed ? " bg-dark-01" : board.HasError ? " bg-error-01" : board.Entry > 0 ? " bg-success-01": " bg-primary-01")}");
                                                }, key: __key5, sequenceNumber: 1863524177);
                                            }, key: x, sequenceNumber: 1863524178);
                                        }
                                    }, key: y, sequenceNumber: 1863524179);
                                }
                            }, key: __key2, sequenceNumber: 1863524180);
                        }
                    };
                }, key: __key1, sequenceNumber: 1863524181);
            }, sequenceNumber: 1863524182);
        }

    }
}

