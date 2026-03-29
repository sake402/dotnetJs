namespace System
{
    public class ForcedPartialBase<T>
    {
        protected extern T This
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
