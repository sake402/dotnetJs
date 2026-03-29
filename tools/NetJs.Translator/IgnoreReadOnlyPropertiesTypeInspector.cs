//using Microsoft.CodeAnalysis;
//using Microsoft.CodeAnalysis.CSharp;
//using YamlDotNet.Serialization;
//using YamlDotNet.Serialization.TypeInspectors;

//public class IgnoreReadOnlyPropertiesTypeInspector : TypeInspectorSkeleton
//{
//    private readonly ITypeInspector _innerTypeInspector;

//    public IgnoreReadOnlyPropertiesTypeInspector(ITypeInspector innerTypeInspector)
//    {
//        _innerTypeInspector = innerTypeInspector;
//    }

//    public override string GetEnumName(Type enumType, string name)
//    {
//        throw new NotImplementedException();
//    }

//    public override string GetEnumValue(object enumValue)
//    {
//        throw new NotImplementedException();
//    }

//    public override IEnumerable<IPropertyDescriptor> GetProperties(Type type, object container)
//    {
//        return _innerTypeInspector.GetProperties(type, container)
//            .Where(p => p.CanWrite); // Only include writable properties
//    }
//}