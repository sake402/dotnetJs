
using NetJs;
using Microsoft.Extensions.Configuration;


[assembly: AssemblyHandle(AssemblyHandleAttribute.MicrosoftExtensionsConfigurationAbstractions)]
[assembly: ReflectableAttribute(false)]
[assembly: Attached(typeof(ConfigurationDebugViewContext), typeof(NonScriptableAttribute))]
[assembly: Attached(typeof(ConfigurationRootExtensions), typeof(NonScriptableAttribute))]
