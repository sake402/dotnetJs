namespace System
{
    //[NetJs.NonScriptable]
    public class ForcedPartialBase<T>
    {
        protected extern T THIS
        {
            [NetJs.Name("this")]
            get;
        }
        //protected extern dynamic DynamicThis
        //{
        //    [dotnetJs.Name("this")]
        //    get;
        //}
    }

}
