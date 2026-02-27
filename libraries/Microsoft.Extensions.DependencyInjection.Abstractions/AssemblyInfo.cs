
using dotnetJs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;


[assembly: AssemblyHandle(AssemblyHandleAttribute.MicrosoftExtensionsDependencyInjectionAbstractions)]
[assembly: Attached(typeof(ServiceKeyAttribute), typeof(NonScriptableAttribute))]
[assembly: Attached(typeof(FromKeyedServicesAttribute), typeof(NonScriptableAttribute))]
[assembly: Attached(typeof(ServiceLifetime), typeof(InlineConstAttribute))]
[assembly: ReflectableAttribute(false)]

