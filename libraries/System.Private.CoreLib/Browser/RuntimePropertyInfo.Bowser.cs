using dotnetJs;

namespace System.Reflection
{
    [dotnetJs.ForcePartial(typeof(RuntimePropertyInfo))]
    internal sealed partial class RuntimePropertyInfo_Partial : ForcedPartialBase<RuntimePropertyInfo>
    {
        internal PropertyModel _model;
        internal RuntimePropertyInfo_Partial(PropertyModel model)
        {
            _model = model;
        }

        [dotnetJs.MemberReplace]
        internal static void get_property_info(RuntimePropertyInfo prop, ref MonoPropertyInfo info, PInfo req_info)
        {
            MonoPropertyInfo minfo = default!;
            minfo.parent = prop.DeclaringType;
            minfo.declaring_type = prop.DeclaringType;
            var model = prop.As<RuntimePropertyInfo_Partial>()._model;
            minfo.get_method = model.GetMethod != null ? new RuntimeMethodInfo(model.GetMethod) : null!;
            minfo.set_method = model.SetMethod != null ? new RuntimeMethodInfo(model.SetMethod) : null!;
            minfo.name = model.Name;
            info = minfo;
        }

        [dotnetJs.MemberReplace]
        internal static Type[] GetTypeModifiers(RuntimePropertyInfo prop, bool optional, int genericArgumentPosition = -1)
        {
            return Type.EmptyTypes;
        }

        [dotnetJs.MemberReplace]
        internal static object get_default_value(RuntimePropertyInfo prop)
        {
            var model = prop.As<RuntimePropertyInfo_Partial>()._model;
            var prototype = AppDomain.GetType(model.PropertyType)!._prototype;
            return prototype![model.Name]!;
        }

        [dotnetJs.MemberReplace]
        internal static int get_metadata_token(RuntimePropertyInfo monoProperty)
        {
            var model = monoProperty.As<RuntimePropertyInfo_Partial>()._model;
            return (int)model.Handle.Value;
        }

        [dotnetJs.MemberReplace]
        private static PropertyInfo internal_from_handle_type(IntPtr event_handle, IntPtr type_handle)
        {
            return (PropertyInfo)AppDomain.GetMember(new ReflectionHandleModel { Value = (uint)event_handle })!;
        }

    }
}
