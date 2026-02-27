using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;
using BlazorJs.Core;
using System.Net.Http.Json;
using System.Text.Json;
using static H5.Core.dom;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using System.Reflection;
using System.IO;
using System.Threading;
using Microsoft.AspNetCore.Authorization;
using H5;

//namespace System
//{
//    public struct DateOnly
//    {
//    }
//}

namespace BlazorJs.Sample
{
    class ABC
    {
        [Name("B")]
        public ABC A()
        {
            return this;
        }
        [Template("B({0}, {1})")]
        public ABC AB(int a, int b)
        {
            return this;
        }
    }
    //public abstract class HTMLCanvasElement
    //{
    //    public abstract System.String getContext();

    //    public abstract BlazorJs.Sample.HTMLCanvasElement getContext2();

    //    protected HTMLCanvasElement() : base()
    //    {
    //    }
    //}
    //public interface Interface0
    //{
    //    void Add<T>(Action<T> attributeBuilder = null) where T : IComponent
    //    {
    //        var tt = new int[1];
    //        tt.ForEach((a) => a = a + 1);
    //    }
    //    void A0()
    //    {
    //        if (P0 == 1)
    //        {

    //        }
    //        else if (P0 == 2)
    //        {

    //        }
    //        else
    //        {

    //        }
    //    }
    //    int P0 { get; set; }
    //}

    //public interface Interface1 //: Interface0
    //{
    //    public event EventHandler<LocationChangedEventArgs> LocationChanged
    //    {
    //        add
    //        {
    //        }
    //        remove
    //        {
    //        }
    //    }
    //    void ABC()
    //    {
    //        AAA = AAA as int;
    //        AAA = _ => (__, ___) =>
    //        {
    //            //throw exception;
    //            // ExceptionDispatchInfo.Capture(exception).Throw();
    //        };
    //    }
    //    RenderFragment<int> AAA { get; set; }
    //    public static implicit operator string(int i) => null;
    //    public event EventHandler OnDisposed;
    //    Delegate BB()
    //    {
    //        return AB;
    //        void AB()
    //        {

    //        }
    //    }
    //    string this[string key]
    //    {
    //        set
    //        {
    //            A(default);
    //        }
    //        get
    //        {

    //        }
    //    }
    //    void A(int i)
    //    {
    //        this.abc = 1;
    //        var arr = new int[] { 1, 2, 3 };
    //        var eventName = arr[0];
    //        if (eventName is double)
    //        {

    //        }
    //        var o = 1 > 2 ? 1 : 2;
    //        new MutationObserverInit
    //        {
    //            childList = true,
    //            attributeFilter = new string[] { "href" },
    //            subtree = true
    //        };
    //        int start = 0;
    //        while (start < 10)
    //            start++;
    //    }
    //    //int P { get; set; }
    //}
    public partial class Component1 : ComponentBase//, Interface1
    {
        //public int WriteTimeout
        //{
        //    get => throw new InvalidOperationException("net_http_content_readonly_stream");
        //    set => throw new InvalidOperationException("net_http_content_readonly_stream");
        //}
        //private static ReadOnlySpan<byte> UTF8Preamble => new byte[] { 0xEF, 0xBB, 0xBF };
        //public long Length => throw new NotSupportedException();
        //string FormatValueAsString(int value)
        //    => value.ToString();
        //private static readonly Dictionary<(Type ModelType, string FieldName), PropertyInfo> _propertyInfoCache = new Dictionary<(Type ModelType, string FieldName), PropertyInfo>();
        //Dictionary<(int, int), string> aa = new Dictionary<(int, int), string>();
        //int Length => throw new Exception();
        Action a;
        async Task AA()
        {
            var sum = 1.Add(2).Add(3).Add(4);
            ABC a;
            a.A().AB(1, 2).C();
            var ab = new ABC();
            ab.AB(1, 2);
            //Dictionary<Type, IAuthorizeData[]> mm = new Dictionary<Type, IAuthorizeData[]>();
            //            return (0, 1);
            //(CancellationTokenSource cts, bool disposeCts, CancellationTokenSource pendingRequestsCts) = PrepareCancellationTokenSource(cancellationToken);
            //var useElement = parent?[0];
            //            IEnumerable<PropertyInfo> properties = from property in instance.GetType().GetProperties()
            //                                                   from property2 in property.GetProperties()
            //                                                  //groupby property.Name
            //                                       where !property.GetIndexParameters().Any()
            //                                       select property2;

            //a?.Target;
            //a?[0];
            //switch (a)
            //{
            //    case Func<int, Task> funcEventArgs:
            //        {

            //        }
            //        break;
            //}
            //var (errorKey, error) = new KeyValuePair<string, string>();
            //using (Stream contentStream = await Task.FromResult<Stream?>(null))            
            //    await JsonSerializer.DeserializeAsync(contentStream, null, default);
            //this.a = delegate
            //        {
            //        };
            //try
            //{
            //}
            //catch (ArgumentException)
            //{
            //}
            //const string pattern = @"^((([a-z]|\d|[!#\$%&'\*\+\-\/=\?\^_`{\|}~]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+(\.([a-z]|\d|[!#\$%&'\*\+\-\/=\?\^_`{\|}~]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+)*)|((\x22)((((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(([\x01-\x08\x0b\x0c\x0e-\x1f\x7f]|\x21|[\x23-\x5b]|[\x5d-\x7e]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(\\([\x01-\x09\x0b\x0c\x0d-\x7f]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF]))))*(((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(\x22)))@((([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.)+(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.?$";
            //HTMLElement scrollContainer = null;
            //(scrollContainer ?? document.documentElement).style["overflowAnchor"] = "none";
            //var cacheKey = (ModelType: this.GetType(), this);
            //RequestCascadingParameter<Task<int>>(e => { });
        }
        //void Interface1.A()
        //{

        //}
        public Component1(int i) : base()
        {

        }
        void Add<T>(Action<T> attributeBuilder = null) where T : IComponent
        {
        }
        protected override async Task OnInitializedAsync()
        {
            descriptor = await Http.GetFromJsonAsync<BlazorWasmAppDescriptor>("https://sake.org.ng/wasm.app.json");
            html = await Http.GetStringAsync("https://google.com");
            await base.OnInitializedAsync();
        }
        void DO(int a)
        {
            field1 = field1 + 1;
            var ff = field1.ToString();
        }
        HTMLElement input = null;
        int field1;
        int field2;
        RenderFragment view;
        HttpClient _http;
        [Inject] public HttpClient MHttp { get => _http; set => _http = value; }
        [Inject] public HttpClient Http { get; set; }
        public string Property1 { get; set; }
        void Clicked1() => field1++;
        void Clicked()
        {
            field1++;
        }

        MarkupString html;
        BlazorWasmAppDescriptor descriptor;
        public class BlazorWasmAppFile
        {
            public string Path { get; set; }
            public string Hash { get; set; }
            public long Size { get; set; }
            public DateTime DateModified { get; set; }
            public override string ToString()
            {
                return Path;
            }
            public override int GetHashCode()
            {
                return Path.GetHashCode();
            }

            public override bool Equals(object obj)
            {
                if (obj is BlazorWasmAppFile f)
                {
                    return f.Path == Path;
                }
                return base.Equals(obj);
            }
        }
        public partial class BlazorWasmAppDescriptor
        {
            //public DateTime BuildTime { get; set; }
            public string Version { get; set; }
            public long Size { get; set; }
            public IEnumerable<BlazorWasmAppFile> Files { get; set; }
        }
    }
}
