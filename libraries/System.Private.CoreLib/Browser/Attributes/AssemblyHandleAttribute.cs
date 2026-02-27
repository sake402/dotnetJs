using System;

namespace dotnetJs
{
    [External]
    [NonScriptable]
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
    public sealed class AssemblyHandleAttribute : Attribute
    {
        //Handle 1 is reserved for runtime
        public const int RuntimeHandle = 1;
        public const int SystemPrivateCoreLib = 1;
        public const int MicrosoftExtensionsPrimitives = 2;
        public const int MicrosoftExtensionsDependencyInjectionAbstractions = 3;
        public const int MicrosoftExtensionsConfigurationAbstractions = 4;
        public const int MicrosoftExtensionsLoggingAbstractions = 5;
        public const int SystemCollections = 6;
        public const int SystemLinq = 7;
        public AssemblyHandleAttribute(uint handle) { }
    }
}