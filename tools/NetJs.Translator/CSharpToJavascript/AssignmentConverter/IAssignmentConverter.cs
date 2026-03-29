using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace NetJs.Translator.CSharpToJavascript.AssignmentConverter
{
    public interface IAssignmentConverter
    {
        Type ExpressionType { get; }
        bool CanConvertTo(TranslatorSyntaxVisitor visitor, INamedTypeSymbol lhsType, CSharpSyntaxNode rhsExpression);
        void WriteAssignment(TranslatorSyntaxVisitor visitor, INamedTypeSymbol lhsType, CSharpSyntaxNode rhsExpression);
    }

}
