using NetJs;
using NetJs.Translator;
using NetJs.Translator.CSharpToJavascript;
using NetJs.Translator.CSharpToJavascript.SyntaxEmitter;
using NetJs.Translator.RazorToCSharp;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace NetJs.Translator.CSharpToJavascript
{
    public partial class TranslatorSyntaxVisitor : CSharpSyntaxWalker
    {
        GlobalCompilationVisitor _global;
        SyntaxTree _tree;
        List<SemanticModel> _semanticModels = new List<SemanticModel>();
        public IEnumerable<SemanticModel> SemanticModels => _semanticModels;
        CodeSymbol memberAccesChainCurrentType;
        string? currentTypeNamespace;

        public GlobalCompilationVisitor Global => _global;
        public string? CurrentTypeNamespace => currentTypeNamespace;
        public Dictionary<INamedTypeSymbol, ScriptWriter> TypeWriters { get; private set; } = new Dictionary<INamedTypeSymbol, ScriptWriter>(SymbolEqualityComparer.Default);

        //Stack<ScriptWriter> writers = new Stack<ScriptWriter>();
        //ScriptWriter Writer => writers.Peek();
        public ScriptWriter Writer { get; set; } = new ScriptWriter();


        static ISyntaxEmitter[] s_Emitters =
        [
            new PointerCreateSyntaxEmitter(),
            new PointerArrayElementAccessSyntaxEmitter(),
            new PointerDereferenceSyntaxEmitter(),
            new PointerPreIncrementDecrementSyntaxEmitter(),
            new PointerPostIncrementDecrementSyntaxEmitter(),
            new PointerAddSubtractToSelfSyntaxEmitter(),
            new PointerAddSubtractSyntaxEmitter(),
            new CreateIndexSyntaxEmitter(),
            new ArrayRangeToSubArraySyntaxEmitter(),
            new IndexerPostIncrementDecrementSyntaxEmitter(),
            new IndexerPreIncrementDecrementSyntaxEmitter(),
            new SpanRangeToSliceMethodSyntaxEmitter(),
            new IndexerGetItemSyntaxEmitter(),
            new IndexerSetItemSyntaxEmitter(),
            new SystemIndexToSetElementSyntaxEmitter(),
            new SystemIndexToGetElementSyntaxEmitter(),
            new ThisAssignmentSyntaxEmitter(),
            new Utf8StringLiteralToReadOnlySpanOfByteSyntaxEmitter(),
            new StringConstructorSyntaxEmitter(),
        ];
        public TranslatorSyntaxVisitor(GlobalCompilationVisitor global, SyntaxTree tree)
        {
            _global = global;
            _tree = tree;
            _semanticModels.Add(global.Compilation.GetSemanticModel(tree));
            //writers.Push(new ScriptWriter());
        }

        public void VisitNode(CodeNode? node)
        {
            if (node != null)
            {
                if (node.IsT0)
                    Visit(node.AsT0);
                else
                    node.AsT1();
            }
        }

        public override void Visit(SyntaxNode? node)
        {
            if (node != null)
            {
                foreach (var emitter in s_Emitters)
                {
                    if (node.GetType() == emitter.SyntaxType)
                    {
                        if (emitter.TryEmit(node, this))
                            return;
                    }
                }
            }
            base.Visit(node);
        }

        //string CollectStatement(Action _continue)
        //{
        //    var sb = new ScriptWriter();
        //    writers.Push(sb);
        //    _continue();
        //    writers.Pop();
        //    return sb.ToString();
        //}

        //More oftern when we rewrite a node using SYntaxFactory and later we try to do _semanticModel.Get....,
        //We can't do that because the new node is detached not part of the current semantic model
        //This let us replace the said node and put it in a new SyntaxTree with its own dedicated semantic model
        //We have to create its own visitor as well with the new semantic model
        public void ReplaceAndVisit(CSharpSyntaxNode target, CSharpSyntaxNode newNode)
        {
            var mnewNode = (ExpressionStatementSyntax)target.Parent!.ReplaceNode(target, newNode)!;
            Visit(mnewNode.Expression);
            return;

            var syntaxAnnotation = new SyntaxAnnotation("NewNodeTracker");
            newNode = newNode.WithAdditionalAnnotations(syntaxAnnotation);
            var rewriter = new SingleNodeReplacer(target, newNode);
            var result = rewriter.Visit(_tree.GetRoot());
            var replacedNode = result!.DescendantNodes().Where(n => n.HasAnnotation(syntaxAnnotation)).Single();
            if (replacedNode.SyntaxTree != result.SyntaxTree) //did not replace
            {

            }
            var newCompilationUnit = _global.Compilation.AddSyntaxTrees(result.SyntaxTree);
            var newGlobal = _global with { Compilation = newCompilationUnit };
            var newVisitor = new TranslatorSyntaxVisitor(newGlobal, result.SyntaxTree)
            {
                Writer = Writer,
                TypeWriters = TypeWriters,
                alreadyTriedImport = alreadyTriedImport,
                Dependencies = Dependencies,
                closures = closures,
                currentExpressionNamespace = currentExpressionNamespace,
                currentTypeNamespace = currentTypeNamespace,
                importedNamespace = importedNamespace,
                imports = imports,

            };
            newVisitor.Visit(replacedNode);
        }

        public override void VisitCompilationUnit(CompilationUnitSyntax node)
        {
            base.VisitCompilationUnit(node);
        }

        public override void VisitUsingDirective(UsingDirectiveSyntax node)
        {
            var name = node.Name!.ToFullString().Trim();
            if (node.Alias != null)
            {
                if (aliasNamespace.TryGetValue(node.Alias.Name.Identifier.ValueText, out var existingAlias))
                {
                    if (existingAlias == name)
                    {
                        return;
                    }
                }
                aliasNamespace.Add(node.Alias.Name.Identifier.ValueText, name);
            }
            else
            {
                if (node.Parent is NamespaceDeclarationSyntax ns)
                {
                    if (!importedNamespace.Contains(name))
                        importedNamespace.Add(name);
                    name = ns.Name.ToFullString().Trim() + "." + name;
                }
                if (!importedNamespace.Contains(name))
                    importedNamespace.Add(name);
            }
            //we are exporting every module by theri namespace
            //var targetNamespace = global.AllNodes.Where(e => e is NamespaceDeclarationSyntax ns && global.ResolveFullNamespace(ns) == name);
            //var types = targetNamespace.SelectMany(c => c.ChildNodes().OfType<TypeDeclarationSyntax>());
            //Writer.WriteLine(node, $"import {{ {string.Join(",\r\n", types.Select(t => t.Identifier.ValueText))} }} from \"/{name}.js\"");
            //base.VisitUsingDirective(node);
        }

        public override void VisitFileScopedNamespaceDeclaration(FileScopedNamespaceDeclarationSyntax node)
        {
            currentTypeNamespace = node.Name.ToString();
            //base.VisitFileScopedNamespaceDeclaration(node);
        }

        public override void VisitUsingStatement(UsingStatementSyntax node)
        {
            if (node.ToString().Contains("ExecutionContext.SuppressFlow()"))
            {

            }
            if (node.Declaration != null)
            {
                foreach (var variable in node.Declaration.Variables)
                {
                    Writer.WriteLine(node, $"const {variable.Identifier.ValueText} = null;", true);
                }
            }
            else if (node.Expression != null)
            {
                Writer.WriteLine(node, "const $disposable = null;", true);
            }
            Writer.WriteLine(node, "try", true);
            Writer.WriteLine(node, "{", true);
            if (node.Expression != null)
            {
                Writer.Write(node, "$disposable = ", true);
                Visit(node.Expression);
                Writer.WriteLine(node, ";");
            }
            else if (node.Declaration != null)
            {
                Writer.Write(node, "", true);
                Visit(node.Declaration);
                Writer.WriteLine(node, ";");
            }
            //if (node.Expression != null)
            //{
            //    VisitChildren(node.Expression.ChildNodes());
            //}
            if (node.Statement != null)
            {
                //if (node.Statement is BlockSyntax block)
                if (node.Statement.IsKind(SyntaxKind.Block))
                    VisitChildren(node.Statement.ChildNodes());
                else
                    Visit(node.Statement);
            }
            //base.VisitUsingStatement(node);
            Writer.WriteLine(node, "}", true);
            Writer.WriteLine(node, "finally", true);
            Writer.WriteLine(node, "{", true);
            if (node.Expression != null)
            {
                //Writer.WriteLine(node, "$disposable?.System$IDisposable$Dispose();", true);
                WriteMethodInvocation(node, "System.IDisposable.Dispose", lhsExpression: new CodeNode(() =>
                {
                    Writer.Write(node, "$disposable?", true);
                }));
                Writer.WriteLine(node, ";");
            }
            else if (node.Declaration != null)
            {
                foreach (var variable in node.Declaration.Variables)
                {
                    //Writer.WriteLine(node, $"{variable.Identifier.ValueText}?.System$IDisposable$Dispose();", true);
                    WriteMethodInvocation(node, "System.IDisposable.Dispose", lhsExpression: new CodeNode(() =>
                    {
                        Writer.Write(node, $"{variable.Identifier.ValueText}?", true);
                    }));
                    Writer.WriteLine(node, ";");
                }
            }
            Writer.WriteLine(node, "}", true);
        }

        void VisitChildren(IEnumerable<SyntaxNode> nodes, string? separator = null)
        {
            int ix = 0;
            foreach (var node in nodes)
            {
                if (separator != null && ix > 0)
                    Writer.Write(node, separator, false);
                Visit(node);
                ix++;
            }
        }

        public override void VisitNamespaceDeclaration(NamespaceDeclarationSyntax node)
        {
            var addedName = node.Name!.ToFullString().Trim();
            if (addedName?.Length > 0 && currentTypeNamespace?.Length > 0)
            {
                addedName = "." + addedName;
            }
            currentTypeNamespace += addedName;
            //VisitChildren(node.ChildNodes().Where(e => e is not QualifiedNameSyntax && e is not IdentifierNameSyntax));
            VisitChildren(node.ChildNodes().Where(e => !e.IsKind(SyntaxKind.QualifiedName) && !e.IsKind(SyntaxKind.IdentifierName)));
            //base.VisitNamespaceDeclaration(node);
            currentTypeNamespace = currentTypeNamespace.Substring(0, currentTypeNamespace.Length - (addedName?.Length ?? 0));
        }
        public override void VisitDelegateDeclaration(DelegateDeclarationSyntax node)
        {
            //base.VisitDelegateDeclaration(node);
        }

        public override void VisitAccessorDeclaration(AccessorDeclarationSyntax node)
        {
            base.VisitAccessorDeclaration(node);
        }

        public override void VisitExpressionStatement(ExpressionStatementSyntax node)
        {
            Writer.Write(node, "", true);
            base.VisitExpressionStatement(node);
            Writer.WriteLine(node, ";");
        }

        public override void VisitLiteralExpression(LiteralExpressionSyntax node)
        {
            if (node.Token.Text.EndsWith("m"))
            {
                if (TryWriteConstant(node, (ITypeSymbol)_global.GetTypeSymbol("System.Decimal", this/*, out _, out _*/), node))
                    return;
            }
            else if (node.Token.Text.EndsWith("UL"))
            {
                if (TryWriteConstant(node, (ITypeSymbol)_global.GetTypeSymbol("System.UInt64", this/*, out _, out _*/), node))
                    return;
            }
            else if (node.Token.Text.EndsWith("L"))
            {
                if (TryWriteConstant(node, (ITypeSymbol)_global.GetTypeSymbol("System.Int64", this/*, out _, out _*/), node))
                    return;
            }
            else if (node.IsKind(SyntaxKind.DefaultLiteralExpression))
            {
                var type = GetExpressionReturnSymbol(node);
                if (type.TypeSyntaxOrSymbol is ITypeSymbol typeSymbol)
                {
                    Writer.Write(node, $"{_global.GlobalName}.{Constants.DefaultTypeName}({typeSymbol.ComputeOutputTypeName(_global)})");
                    return;
                }
            }
            var txt = node.GetLiteralString(_global);
            Writer.Write(node, txt);
            base.VisitLiteralExpression(node);
        }

        public override void VisitPrefixUnaryExpression(PrefixUnaryExpressionSyntax node)
        {
            if (TryInvokeMethodOperator(node, node.OperatorToken.ValueText, null, node.Operand, [node.Operand]))
                return;
            Writer.Write(node, $"{node.OperatorToken.ValueText}");
            Visit(node.Operand);
            DereferenceIfReference(node.Operand);
            //base.VisitPrefixUnaryExpression(node);
        }

        public override void VisitPostfixUnaryExpression(PostfixUnaryExpressionSyntax node)
        {
            if (!node.IsKind(SyntaxKind.SuppressNullableWarningExpression))
                if (TryInvokeMethodOperator(node, node.OperatorToken.ValueText, null, node.Operand, [node.Operand]))
                    return;
            Visit(node.Operand);
            DereferenceIfReference(node.Operand);
            if (!node.IsKind(SyntaxKind.SuppressNullableWarningExpression))//remove shebang after null and default
                Writer.Write(node, $"{node.OperatorToken.ValueText}");
        }

        public override void VisitBinaryExpression(BinaryExpressionSyntax node)
        {
            if (TryWriteMathBinaryExpression(node, node.OperatorToken.ValueText, node.Left, node.Right))
                return;
            if (TryInvokeMethodOperator(node, node.OperatorToken.ValueText, null, node.Left, [node.Left, node.Right]))
                return;
            var op = node.OperatorToken.ValueText.Trim();
            if (op == "??" && node.Right.IsKind(SyntaxKind.ThrowExpression)/* is ThrowExpressionSyntax*/)
            {
                Writer.Write(node, $"{_global.GlobalName}.$FirstOf(");
                Visit(node.Left);
                Writer.Write(node, ", function(){ ");
                Visit(node.Right);
                Writer.Write(node, " }.bind(this))");
            }
            else if (op == "as")
            {
                Writer.Write(node, $"{_global.GlobalName}.$as(");
                Visit(node.Left);
                Writer.Write(node, ", ");
                Visit(node.Right);
                Writer.Write(node, ")");
            }
            else if (op == "is" && node.Right is not LiteralExpressionSyntax)
            {
                Writer.Write(node, $"{_global.GlobalName}.$is(");
                Visit(node.Left);
                Writer.Write(node, ", ");
                Visit(node.Right);
                Writer.Write(node, ")");
            }
            else
            {
                //Writer.Write(node, $"(");
                Visit(node.Left);
                if (op == "is")
                {
                    if (node.Right is LiteralExpressionSyntax)
                    {
                        op = "==";
                    }
                    else
                    {
                        op = "instanceof";
                    }
                }
                else if (op == "==")
                {
                    op = "===";
                }
                else if (op == "!=")
                {
                    op = "!==";
                }
                Writer.Write(node, $" {op} ");
                Visit(node.Right);
                //Writer.Write(node, $")");
            }
            //base.VisitBinaryExpression(node);
        }

        public override void VisitAwaitExpression(AwaitExpressionSyntax node)
        {
            Writer.Write(node, $"await ");
            WriteMethodInvocation(node, "System.Runtime.CompilerServices.RuntimeHelpers.TaskToPromise", arguments: [node.Expression]);
            //base.VisitAwaitExpression(node);
        }

        public override void VisitParenthesizedExpression(ParenthesizedExpressionSyntax node)
        {
            Writer.Write(node, "(");
            base.VisitParenthesizedExpression(node);
            Writer.Write(node, ")");
        }

        public override void VisitBlock(BlockSyntax node)
        {
            Writer.WriteLine(node, "{", true);
            OpenClosure(node);
            if (!BlockTryHandleJumpLabels(node))
                base.VisitBlock(node);
            CloseClosure();
            Writer.WriteLine(node, "}", true);
        }

        //public override void VisitDeclarationPattern(DeclarationPatternSyntax node)
        //{
        //    base.VisitDeclarationPattern(node);
        //    if (node.Designation != null)
        //    {
        //        Writer.Write($", ");
        //        Visit(node.Designation);
        //    }
        //}

        public override void VisitArgument(ArgumentSyntax node)
        {
            if (node.RefKindKeyword.ValueText == "out" || node.RefKindKeyword.ValueText == "ref" || node.RefKindKeyword.ValueText == "in")
            {
                string? boundIdentifierName = null;
                string? bindToThis = null;
                var iNameMangling = ++Writer.CurrentClosure.NameManglingSeed;
                if (node.Expression is DeclarationExpressionSyntax dec && dec.Designation is SingleVariableDesignationSyntax sv)
                {
                    var boundLocalField = _global.TryGetTypeSymbol(sv, this/*, out _, out _*/);
                    boundIdentifierName = sv.Identifier.ValueText;
                    Writer.InsertInCurrentClosure(node, $"let {boundIdentifierName} = null;", true);
                    if (boundLocalField != null)
                    {
                        CurrentClosure.DefineIdentifierType(boundIdentifierName, CodeSymbol.From(boundLocalField));
                    }
                    else if (node.RefKindKeyword.ValueText == "out" && !dec.Type.IsVar)
                    {
                        CurrentClosure.DefineIdentifierType(boundIdentifierName, CodeSymbol.From(dec.Type, SymbolKind.Local));
                    }
                }
                else if (node.Expression is IdentifierNameSyntax id)
                {
                    var rhs = _global.TryGetTypeSymbol(id, this/*, out var rhs, out var rhsKind*/);
                    var rhsKind = rhs?.Kind;
                    //ISymbol? rhsRefTarget = (declaringType as IParameterSymbol) ??
                    //    (declaringType as IFieldSymbol) ??
                    //    (ISymbol?)(declaringType as ILocalSymbol);
                    //var rhsType = (declaringType as IParameterSymbol)?.Type ??
                    //    (declaringType as ILocalSymbol)?.Type ??
                    //    (declaringType as IFieldSymbol)?.Type ??
                    //    (declaringType as IPropertySymbol)?.Type ?? declaringType as ITypeSymbol;
                    var rhsRefKind = (rhs as IParameterSymbol)?.RefKind ??
                        (rhs as ILocalSymbol)?.RefKind ??
                        (rhs as IFieldSymbol)?.RefKind ??
                        (rhs as IPropertySymbol)?.RefKind;
                    if (rhsKind == SymbolKind.Field || rhsKind == SymbolKind.Local || rhsKind == SymbolKind.Parameter)
                    {
                        if (rhsRefKind == RefKind.Ref || rhsRefKind == RefKind.RefReadOnly || rhsRefKind == RefKind.RefReadOnlyParameter || rhsRefKind == RefKind.Out || rhsRefKind == RefKind.In) //the reference field is already a ref itself. No need to create a new ref
                        {
                            Visit(node.Expression);
                            return;
                        }
                        else
                        {
                            if (rhsKind == SymbolKind.Field)
                            {
                                //While we could be cheking if the accessed field is static.
                                //The "this" in the static method is most likely the prototype of the class itself though
                                //So we expect it to work
                                if (!rhs!.IsStatic)
                                    bindToThis = $"$this{iNameMangling}";
                                var metadata = _global.GetRequiredMetadata(rhs!);
                                boundIdentifierName = metadata.InvocationName ?? rhs!.Name;
                                if (!rhs.IsStatic)
                                {
                                    Writer.InsertInCurrentClosure(node, $"const {bindToThis} = this;", true);
                                    bindToThis += ".";
                                }
                            }
                            else
                            {
                                boundIdentifierName = rhs!.Name;
                            }
                        }
                    }
                    else
                    {
                        //if (declaringType is IParameterSymbol parameter && (parameter.RefKind == RefKind.Ref || parameter.RefKind == RefKind.RefReadOnly || parameter.RefKind == RefKind.RefReadOnlyParameter || parameter.RefKind == RefKind.Out || parameter.RefKind == RefKind.In))
                        //{
                        //    //if the parameter passed to this argument is itself a ref/in/out, no need to create a new reference
                        //    Visit(node.Expression);
                        //    return;
                        //}
                        //if (declaringType is ILocalSymbol local && (local.RefKind == RefKind.Ref || local.RefKind == RefKind.RefReadOnly || local.RefKind == RefKind.RefReadOnlyParameter || local.RefKind == RefKind.Out || local.RefKind == RefKind.In))
                        //{
                        //    //if the parameter passed to this argument is itself a ref/in/out, no need to create a new reference
                        //    Visit(node.Expression);
                        //    return;
                        //}
                        //if (declaringType is IFieldSymbol field && (field.RefKind == RefKind.Ref || field.RefKind == RefKind.RefReadOnly || field.RefKind == RefKind.RefReadOnlyParameter || field.RefKind == RefKind.Out || field.RefKind == RefKind.In))
                        //{
                        //    //if the parameter passed to this argument is itself a ref/in/out, no need to create a new reference
                        //    Visit(node.Expression);
                        //    return;
                        //}
                        boundIdentifierName = id.Identifier.ValueText;
                    }
                }
                else if (node.Expression.IsKind(SyntaxKind.FieldExpression))
                {
                    var containigType = node.FindClosestParent<BaseTypeDeclarationSyntax>() ?? throw new InvalidOperationException("field must be inside a property");
                    var typeSymbol = _global.GetTypeSymbol(containigType, this);
                    var typeMetadata = _global.GetRequiredMetadata(typeSymbol);
                    var containigProperty = node.FindClosestParent<PropertyDeclarationSyntax>() ?? throw new InvalidOperationException("field must be inside a property");
                    var propertyName = containigProperty.Identifier.ValueText;
                    bool isStatic = containigProperty.Modifiers.IsStatic();
                    boundIdentifierName = $"{(!isStatic ? "this" : typeMetadata.InvocationName ?? typeSymbol.Name)}.{propertyName}$";
                }
                else
                {
                    Visit(node.Expression);
                    return;
                }
                var fieldName = $"{bindToThis}{boundIdentifierName}";
                if (boundIdentifierName == "_")//discard
                {
                    Writer.Write(node, $"$.{Constants.DiscardRefName}");
                }
                else
                {
                    var simpleBoundIdentifierName = boundIdentifierName.Split('.').Last();
                    var ix = ++Writer.CurrentClosure.NameManglingSeed;
                    WriteCreateRef(node, fieldName, $"/*{node.RefKindKeyword.ValueText} {boundIdentifierName}*/ const ${node.RefKindKeyword.ValueText}_{simpleBoundIdentifierName}{ix} = ", ";", _readOnly: node.RefKindKeyword.ValueText == "in");
                    Writer.Write(node, $"${node.RefKindKeyword.ValueText}_{simpleBoundIdentifierName}{ix}");
                }
                //Writer.InsertInCurrentClosure($"/*{node.RefKindKeyword.ValueText} {boundIdentifierName}*/ const ${node.RefKindKeyword.ValueText}{iNameMangling} = {{ get value(){{ return {bindToThis}{boundIdentifierName}; }}, set value(v){{ {bindToThis}{boundIdentifierName} = v; }} }};", true);
            }
            //else if (node.RefKindKeyword.ValueText == "in")
            //{
            //}
            else
            {
                //skip namecolon
                //base.VisitArgument(node);
                Visit(node.Expression);
            }
        }

        public override void VisitParenthesizedVariableDesignation(ParenthesizedVariableDesignationSyntax node)
        {
            Writer.Write(node, " [ ");
            int i = 0;
            foreach (var v in node.Variables)
            {
                if (i > 0)
                    Writer.Write(node, ", ");
                Visit(v);
                i++;
            }
            Writer.Write(node, " ]");
            //base.VisitParenthesizedVariableDesignation(node);
        }

        public override void VisitBaseExpression(BaseExpressionSyntax node)
        {
            //if (node.Parent is InvocationExpressionSyntax)//dont insert super keyword into method calls. Can only be used as a dispatch prefix
            //    Writer.Write(node, "this");
            //else
            Writer.Write(node, "super");
            base.VisitBaseExpression(node);
        }

        public override void VisitNameColon(NameColonSyntax node)
        {
            throw new InvalidOperationException("Should not get here. Javascript doesnt do named colon");
            //base.VisitNameColon(node);
            //Writer.Write(node, $" {node.ColonToken.ValueText} ");
        }

        public override void VisitInitializerExpression(InitializerExpressionSyntax node)
        {
            if (node.IsKind(SyntaxKind.ArrayInitializerExpression))
            {
                Writer.Write(node, "[ ", false);
            }
            int i = 0;
            foreach (var n in node.Expressions)
            {
                if (i > 0)
                    Writer.Write(node, ", ");
                Visit(n);
                i++;
            }
            if (node.IsKind(SyntaxKind.ArrayInitializerExpression))
            {
                Writer.Write(node, " ]", false);
            }
            //base.VisitInitializerExpression(node);
        }

        public override void VisitThrowExpression(ThrowExpressionSyntax node)
        {
            Writer.Write(node, $"throw ");
            base.VisitThrowExpression(node);
            if (node.Expression == null) //we must have being inside a catch if throw has no expression
            {
                var _catch = node.FindClosestParent<CatchClauseSyntax>();
                Writer.Write(node, !string.IsNullOrEmpty(_catch?.Declaration?.Identifier.ValueText) ? _catch!.Declaration!.Identifier.ValueText : "$e");
            }
        }


        public override void VisitThrowStatement(ThrowStatementSyntax node)
        {
            Writer.Write(node, $"throw ", true);
            base.VisitThrowStatement(node);
            if (node.Expression == null) //we must have being inside a catch if throw has no expression
            {
                var _catch = node.FindClosestParent<CatchClauseSyntax>();
                Writer.Write(node, !string.IsNullOrEmpty(_catch?.Declaration?.Identifier.ValueText) ? _catch!.Declaration!.Identifier.ValueText : "$e");
            }
            Writer.WriteLine(node, $";");
        }

        public override void VisitConditionalExpression(ConditionalExpressionSyntax node)
        {
            Visit(node.Condition);
            Writer.Write(node, " ? ");
            if (node.WhenTrue.IsKind(SyntaxKind.ThrowExpression)/* is ThrowExpressionSyntax*/)
            {
                Writer.Write(node, $"{_global.GlobalName}.{Constants.Expression}(");
                Writer.Write(node, "function(){ ");
                Visit(node.WhenTrue);
                Writer.Write(node, " }.bind(this))");
            }
            else
            {
                Visit(node.WhenTrue);
            }
            Writer.Write(node, " : ");
            if (node.WhenFalse.IsKind(SyntaxKind.ThrowExpression)/* is ThrowExpressionSyntax*/)
            {
                Writer.Write(node, $"{_global.GlobalName}.{Constants.Expression}(");
                Writer.Write(node, "function(){ ");
                Visit(node.WhenFalse);
                Writer.Write(node, " }.bind(this))");
            }
            else
            {
                Visit(node.WhenFalse);
            }
            //base.VisitConditionalExpression(node);
        }
        public override void VisitElementAccessExpression(ElementAccessExpressionSyntax node)
        {
            //var type = _global.ResolveSymbol(GetExpressionReturnSymbol(node.Expression), this/*, out _, out _*/)?.GetTypeSymbol();
            //if (type != null)
            //{
            //    var propertyIndexers = type.GetMembers("get_Item", _global).Where(e => e is IMethodSymbol p && p.Parameters.Count() == node.ArgumentList.Arguments.Count).Cast<IMethodSymbol>().ToList();
            //    //var propertyIndexers = nt.GetMembers("get_Item", _global).Where(e => e is IPropertySymbol p && p.IsIndexer && p.Parameters.Count() == node.ArgumentList.Arguments.Count && p.GetMethod != null).Cast<IPropertySymbol>().ToList();
            //    var bestIndexer = GetBestOverloadMethod(type, propertyIndexers, null, node.ArgumentList.Arguments, null, out _);
            //    if (bestIndexer != null)
            //    {
            //        bool isExtern = bestIndexer.IsExtern || _global.HasAttribute(bestIndexer, typeof(ExternalAttribute).FullName!, this, false, out _) ||
            //             (bestIndexer.AssociatedSymbol?.IsExtern ?? false) || (bestIndexer.AssociatedSymbol != null && _global.HasAttribute(bestIndexer.AssociatedSymbol, typeof(ExternalAttribute).FullName!, this, false, out _));
            //        bool hasTemplate = bestIndexer.GetTemplateAttribute(_global) != null;
            //        if (!isExtern || hasTemplate)
            //        {
            //            if (WriteMethodInvocation(node, bestIndexer, null, null, node.ArgumentList.Arguments, node.Expression, type, false))
            //                return;
            //        }
            //    }
            //}
            Visit(node.Expression);
            Writer.Write(node, "[");
            foreach (var a in node.ArgumentList.Arguments)
            {
                Visit(a);
            }
            Writer.Write(node, "]");
            //base.VisitElementAccessExpression(node);
        }

        public override void VisitThisExpression(ThisExpressionSyntax node)
        {
            Writer.Write(node, "this");
            //base.VisitThisExpression(node);
        }

        public override void VisitDefaultExpression(DefaultExpressionSyntax node)
        {
            EnsureImported(node.Type);
            //if (node.Type != null)
            //{
            Writer.Write(node, $"{_global.GlobalName}.{Constants.DefaultTypeName}(");
            Visit(node.Type);
            Writer.Write(node, $")");
            //}
            //else
            //{
            //    Writer.Write(node, "null");
            //}
            //base.VisitDefaultExpression(node);
        }

        public override void VisitCheckedExpression(CheckedExpressionSyntax node)
        {
            var dispose = _global.DefinePragma(node.Keyword.ValueText);
            base.VisitCheckedExpression(node);
            dispose.Dispose();
        }

        public override void VisitCheckedStatement(CheckedStatementSyntax node)
        {
            Writer.WriteLine(node, $"//{node.Keyword.ValueText}", true);
            var dispose = _global.DefinePragma(node.Keyword.ValueText);
            base.VisitCheckedStatement(node);
            dispose.Dispose();
        }

        public override void VisitConditionalAccessExpression(ConditionalAccessExpressionSyntax node)
        {
            //This is rewritten, should not get called at all
            Debug.Assert(false);
            //invocation visit will handle the conditional invocation
            //if (node.WhenNotNull is InvocationExpressionSyntax conditionalInvoke)
            {
                var i = ++Writer.CurrentClosure.NameManglingSeed;
                var temporaryIdentifierName = $"$t{i}";
                Writer.InsertInCurrentClosure(node, $"let {temporaryIdentifierName};", true);
                var lhsType = GetExpressionReturnSymbol(node.Expression);
                //var lhsSymbol = GetTypeSymbol(lhsType, out _);
                //var lhsSymbol = GetTypeSymbol(lhsType, out _);
                //VariableDeclarationSyntax variableDeclaration = SyntaxFactory.VariableDeclaration(
                //    SyntaxFactory.ParseTypeName(lhsSymbol!.Name), // Type of the variable
                //    SyntaxFactory.SingletonSeparatedList(
                //        SyntaxFactory.VariableDeclarator(
                //            SyntaxFactory.Identifier(temporaryIdentifierName) // Name of the variable
                //        )
                //    )
                //);
                //LocalDeclarationStatementSyntax localDeclarationStatement = SyntaxFactory.LocalDeclarationStatement(variableDeclaration);
                //var block = node.FindClosest<BlockSyntax>();
                //var newBlock = block.InsertNodesBefore(block.ChildNodes().FirstOrDefault()!, [localDeclarationStatement]);
                ////node.InsertNodesBefore(node, [localDeclarationStatement]);
                //var localField = semanticModel.GetDeclaredSymbol(localDeclarationStatement);
                IDisposable? disposeTemporatyVariable = null;
                //if (lhsSymbol != null)
                //{
                //    var field = SyntaxFactory.LocalDeclarationStatement(SyntaxFactory.VariableDeclaration(SyntaxFactory.ParseTypeName(lhsSymbol.Name), SyntaxFactory.SingletonSeparatedList<VariableDeclaratorSyntax>(SyntaxFactory.VariableDeclarator(temporaryIdentifierName))));
                //    var block = node.FindClosest<BlockSyntax>();
                //    node = (ConditionalAccessExpressionSyntax)node.Parent.InsertNodesBefore(node, [field]);
                //    var localField = GetTypeSymbol(field);
                //    disposeTemporatyVariable = CurrentClosure.DefineIdentifierType(temporaryIdentifierName, CodeType.From(localField));
                //}
                //else
                //{
                disposeTemporatyVariable = CurrentClosure.DefineIdentifierType(temporaryIdentifierName, lhsType with { Kind = SymbolKind.Local });
                //}
                if (false)
                {
                    Writer.WriteLine(node, $"{_global.GlobalName}.{Constants.Expression}(function()");
                    Writer.WriteLine(node, $"{{", true);
                    Writer.Write(node, $"let {temporaryIdentifierName} = ", true);
                    Visit(node.Expression);
                    Writer.WriteLine(node, $";");
                    Writer.WriteLine(node, $"if ({temporaryIdentifierName} != null)", true);
                    Writer.WriteLine(node, $"{{", true);
                    Writer.Write(node, "return ", true);
                }
                else
                {
                    Writer.Write(node, $"(({temporaryIdentifierName} = ");
                    Visit(node.Expression);
                    Writer.Write(node, $") && ");
                }
                ExpressionSyntax Combine(ExpressionSyntax lhs, ExpressionSyntax rhs)
                {
                    if (rhs is InvocationExpressionSyntax conditionalInvoke)
                    {
                        if (conditionalInvoke.Expression is MemberBindingExpressionSyntax mb)
                        {
                            var memberAccess = SyntaxFactory.MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                lhs,
                                SyntaxFactory.Token(SyntaxKind.DotToken), mb.Name);
                            return SyntaxFactory.InvocationExpression(memberAccess, conditionalInvoke.ArgumentList);
                        }
                        else if (conditionalInvoke.Expression is MemberAccessExpressionSyntax ma)
                        {
                            var memberAccess = SyntaxFactory.MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                Combine(lhs, ma.Expression),
                                SyntaxFactory.Token(SyntaxKind.DotToken), ma.Name);
                            return SyntaxFactory.InvocationExpression(memberAccess, conditionalInvoke.ArgumentList);
                        }
                    }
                    else if (rhs is MemberBindingExpressionSyntax member)
                    {
                        return SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            lhs,
                            SyntaxFactory.Token(SyntaxKind.DotToken), member.Name);
                    }
                    else if (rhs is MemberAccessExpressionSyntax ma)
                    {
                        return SyntaxFactory.MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                Combine(lhs, ma.Expression),
                                SyntaxFactory.Token(SyntaxKind.DotToken), ma.Name);
                    }
                    else if (rhs is ConditionalAccessExpressionSyntax cd)
                    {
                        var m = Combine(lhs, cd.Expression);
                        return cd.ReplaceNode(cd.Expression, m);
                    }
                    else if (rhs is ElementAccessExpressionSyntax ae)
                    {
                        var newNode = Combine(lhs, ae.Expression);
                        return ae.ReplaceNode(ae.Expression, newNode);
                    }
                    else if (rhs is ElementBindingExpressionSyntax ab)
                    {
                        var m = SyntaxFactory.ElementAccessExpression(lhs, ab.ArgumentList);
                        return m;
                    }
                    else if (rhs is AssignmentExpressionSyntax asm)
                    {
                        var newNode = Combine(lhs, asm.Left);
                        return asm.ReplaceNode(asm.Left, newNode);
                    }
                    return null;
                }
                ExpressionSyntax next = Combine(SyntaxFactory.IdentifierName($"{temporaryIdentifierName}"), node.WhenNotNull);

                //if (node.WhenNotNull is InvocationExpressionSyntax conditionalInvoke)
                //{
                //    next = Combine(SyntaxFactory.IdentifierName($"{temporaryIdentifierName}"), node.WhenNotNull);
                //    //var memberAccess = SyntaxFactory.MemberAccessExpression(
                //    //    SyntaxKind.SimpleMemberAccessExpression,
                //    //    SyntaxFactory.IdentifierName($"{temporaryIdentifierName}"),
                //    //    SyntaxFactory.Token(SyntaxKind.DotToken), ((MemberBindingExpressionSyntax)conditionalInvoke.Expression).Name);
                //    //next = SyntaxFactory.InvocationExpression(memberAccess, conditionalInvoke.ArgumentList);
                //}
                //else if (node.WhenNotNull is MemberBindingExpressionSyntax member)
                //{
                //    next = Combine(SyntaxFactory.IdentifierName($"{temporaryIdentifierName}"), node.WhenNotNull);
                //    //next = SyntaxFactory.MemberAccessExpression(
                //    //    SyntaxKind.SimpleMemberAccessExpression,
                //    //    SyntaxFactory.IdentifierName($"{temporaryIdentifierName}"),
                //    //    SyntaxFactory.Token(SyntaxKind.DotToken), member.Name);
                //}
                //else if (node.WhenNotNull is ConditionalAccessExpressionSyntax cd)
                //{
                //    var m = Combine(SyntaxFactory.IdentifierName($"{temporaryIdentifierName}"), cd.Expression);
                //    next = cd.ReplaceNode(cd.Expression, m);
                //}
                //node.ReplaceToken(node.OperatorToken, SyntaxFactory.ope($"$loc"));
                Visit(next);
                if (false)
                {
                    Writer.WriteLine(node, $";");
                    Writer.WriteLine(node, $"}}", true);
                    Writer.WriteLine(node, $"return null;", true);
                    Writer.Write(node, $"}}.bind(this))", true);
                }
                else
                {
                    Writer.Write(node, $")");
                }
                disposeTemporatyVariable.Dispose();
            }
            //else
            //{
            //    Visit(node.Expression);
            //    Writer.Write(node, node.OperatorToken.ValueText/*.ToFullString()*/);
            //    Visit(node.WhenNotNull);
            //}
            //base.VisitConditionalAccessExpression(node);
        }

        public override void VisitTupleElement(TupleElementSyntax node)
        {
            base.VisitTupleElement(node);
        }

        public override void VisitTupleExpression(TupleExpressionSyntax node)
        {
            if (node.Parent is AssignmentExpressionSyntax assignment)
            {
                if (assignment.Right == node)
                {
                    //assigning tuple to tuple in an expression like (T start, T end) = (_start, _endExclusive);
                    //should create a simple object desctructured back to the lhs a simple
                    //no need to instantiate a tuple type
                    Writer.Write(node, "{ ");
                    int i = 0;
                    foreach (var e in node.Arguments)
                    {
                        if (i > 0)
                            Writer.Write(node, ", ");
                        Writer.Write(node, "Item");
                        Writer.Write(node, (i + 1).ToString());
                        Writer.Write(node, ": ");
                        Visit(e.Expression);
                        i++;
                    }
                    Writer.Write(node, " }");
                }
                else
                {
                    if (node.Arguments.All(a => a.Expression.IsKind(SyntaxKind.DeclarationExpression)))
                    {
                        Writer.Write(node, "const { ");
                        int i = 0;
                        foreach (var e in node.Arguments)
                        {
                            if (i > 0)
                                Writer.Write(node, ", ");
                            Writer.Write(node, "Item");
                            Writer.Write(node, (i + 1).ToString());
                            Writer.Write(node, ": ");
                            if (e.Expression is DeclarationExpressionSyntax de)
                            {
                                Visit(de.Designation);
                            }
                            else
                            {
                                Visit(e.Expression);
                            }
                            i++;
                        }
                        Writer.Write(node, " }");
                    }
                    else
                    {
                        foreach (var e in node.Arguments)
                        {
                            if (e.Expression is DeclarationExpressionSyntax de)
                            {
                                Visit(de);
                                Writer.WriteLine(node, ";");
                            }
                        }
                        Writer.WriteLine(node, $"{_global.GlobalName}.{Constants.TupleUnPack}(function($tp)");
                        Writer.WriteLine(node, "{", true);
                        int ix = 0;
                        foreach (var arg in node.Arguments)
                        {
                            Writer.Write(node, "", true);
                            WriteVariableAssignment(node, arg.Expression, null, "=", new CodeNode(() =>
                            {
                                Writer.Write(node, $"$tp.Item{(ix + 1)}");
                            }), rhs: _global.TryGetTypeSymbol(arg.Expression, this)?.GetTypeSymbol());
                            //if (arg.Expression is DeclarationExpressionSyntax de)
                            //{
                            //    Visit(de.Designation);
                            //}
                            //else
                            //{
                            //    Visit(arg.Expression);
                            //}
                            //Writer.Write(node, " = ");
                            //Writer.Write(node, "$tp.Item");
                            //Writer.Write(node, ix.ToString());
                            Writer.WriteLine(node, ";");
                            ix++;
                        }
                        Writer.Write(node, "}.bind(this)).$v", true);
                    }
                }
            }
            else
            {
                var tupleStruct = (INamedTypeSymbol)_global.GetTypeSymbol($"System.ValueTuple<{string.Join(",", Enumerable.Range(1, node.Arguments.Count).Select(s => ""))}>", this/*, out _, out _*/);
                var argTypes = node.Arguments.Select(a => _global.ResolveSymbol(GetExpressionReturnSymbol(a), this)?.GetTypeSymbol() ?? throw new InvalidOperationException("Cannot result tuple generic argument")).ToArray();
                tupleStruct = tupleStruct.Construct(argTypes);
                var tupleConstructor = (IMethodSymbol)tupleStruct.GetMembers(".ctor").Where(e => ((IMethodSymbol)e).Parameters.Count() == node.Arguments.Count).Single();
                WriteConstructorCall(node, tupleStruct, tupleConstructor, null, node.Arguments.Select(e => new CodeNode(e)), default);
                //Writer.Write(node, "{ ");
                //int i = 0;
                //foreach (var e in node.Arguments)
                //{
                //    if (i > 0)
                //        Writer.Write(node, ", ");
                //    //if (e.NameColon == null)
                //    //{
                //    Writer.Write(node, $"Item{i + 1}: ");
                //    //}
                //    //The namecolon are syntatic sugar, we still reference them in runtime as ItemX
                //    Visit(e.Expression);
                //    i++;
                //}
                //Writer.Write(node, " }");
            }
            //base.VisitTupleExpression(node);
        }

        public override void VisitTupleType(TupleTypeSyntax node)
        {
            Writer.Write(node, $"System.ValueTuple(");
            int i = 0;
            foreach (var e in node.Elements)
            {
                if (i > 0)
                    Writer.Write(node, ", ");
                Visit(e);
                i++;
            }
            Writer.Write(node, $")");
            //base.VisitTupleType(node);
        }

        public override void VisitLockStatement(LockStatementSyntax node)
        {
            //skip lock, js is single threaded anyway
            Writer.WriteLine(node, "//lock", true);
            Visit(node.Statement);
            //base.VisitLockStatement(node);
        }

        public override void VisitBracketedArgumentList(BracketedArgumentListSyntax node)
        {
            Writer.Write(node, node.OpenBracketToken.ValueText);
            if (node.Arguments.Count > 1)
            {
                Writer.Write(node, "getItem(");
                int i = 0;
                foreach (var a in node.Arguments)
                {
                    if (i > 0)
                        Writer.Write(node, ", ");
                    Visit(a);
                    i++;
                }
                Writer.Write(node, ")");
            }
            else
            {
                int i = 0;
                foreach (var a in node.Arguments)
                {
                    if (i > 0)
                        Writer.Write(node, ", ");
                    Visit(a);
                    i++;
                }
            }
            Writer.Write(node, node.CloseBracketToken.ValueText);
            //base.VisitBracketedArgumentList(node);
        }

        public override void VisitMemberBindingExpression(MemberBindingExpressionSyntax node)
        {
            Writer.Write(node, node.OperatorToken.ValueText);
            Visit(node.Name);
            //base.VisitMemberBindingExpression(node);
        }

        public override void VisitElementBindingExpression(ElementBindingExpressionSyntax node)
        {
            //javascript doesnt support ?[] conditional array access notation, rewrite as ?.[
            if (node.Parent is ConditionalAccessExpressionSyntax cond)
            {
                Writer.Write(cond, ".");
            }
            base.VisitElementBindingExpression(node);
        }

        void WriteTypeOf(CSharpSyntaxNode node, CodeNode typePrototype)
        {
            Writer.Write(node, $"$.{Constants.TypeOf}(");
            VisitNode(typePrototype);
            Writer.Write(node, ")");
        }
        public override void VisitTypeOfExpression(TypeOfExpressionSyntax node)
        {
            WriteTypeOf(node, node.Type);
            //Writer.Write(node, $"$.{Constants.TypeOf}(");
            //Visit(node.Type);
            //Writer.Write(node, ")");
            //base.VisitTypeOfExpression(node);
        }

        public override void VisitFixedStatement(FixedStatementSyntax node)
        {
            Writer.WriteLine(node, "/*fixed*/ ", true);
            OpenClosure(node);
            Writer.WriteLine(node, "{", true);
            Visit(node.Declaration);
            Writer.WriteLine(node, ";");
            Visit(node.Statement);
            Writer.WriteLine(node, "}", true);
            CloseClosure();
            //base.VisitFixedStatement(node);
        }

        public override void VisitWithExpression(WithExpressionSyntax node)
        {
            var type = _global.ResolveSymbol(GetExpressionReturnSymbol(node.Expression), this)!.GetTypeSymbol();
            Writer.Write(node, $"{_global.GlobalName}.{Constants.With}(");
            Visit(node.Expression);
            Writer.WriteLine(node, ", function($clone)");
            Writer.WriteLine(node, "{", true);
            WriteInitializer(node, "$clone", type, node.Initializer.Expressions);
            Writer.Write(node, "})", true);
            //base.VisitWithExpression(node);
        }

        public override void VisitIfDirectiveTrivia(IfDirectiveTriviaSyntax node)
        {
            //base.VisitIfDirectiveTrivia(node);
        }

        public override void VisitElifDirectiveTrivia(ElifDirectiveTriviaSyntax node)
        {
            //base.VisitElifDirectiveTrivia(node);
        }

        public override void VisitElseDirectiveTrivia(ElseDirectiveTriviaSyntax node)
        {
            //base.VisitElseDirectiveTrivia(node);
        }

        public override void VisitEndIfDirectiveTrivia(EndIfDirectiveTriviaSyntax node)
        {
            //base.VisitEndIfDirectiveTrivia(node);
        }

        public override void VisitTypeConstraint(TypeConstraintSyntax node)
        {
            //base.VisitTypeConstraint(node);
        }

        public override void VisitAttribute(AttributeSyntax node)
        {
            //base.VisitAttribute(node);
        }

        public void WrapStatementsInExpression(CSharpSyntaxNode node, Action statementsWriter)
        {
            Writer.WriteLine(node, $"{_global.GlobalName}.{Constants.Expression}(function()");
            Writer.WriteLine(node, $"{{", true);
            statementsWriter();
            Writer.Write(node, $"}})", true);
        }

        public string Build(int formatTabs)
        {
            var importsFromSource = _global.OutputMode.HasFlag(OutputMode.Module) ?
            string.Join("\r\n", imports.Where(e => e.Key.EndsWith(".cs")).Select(i => $"import {{ {string.Join(", ", i.Value)} }} from \"/{_global.Project.GetName()}/{Path.ChangeExtension(Utility.GetRelativePath(_global.Project.GetFolder(), i.Key), "js").Replace("\\", "/")}\"")) : null;
            var importsFromModule = _global.OutputMode.HasFlag(OutputMode.Module) ?
                string.Join("\r\n", imports.Where(e => e.Key.Contains(".dll")).Select(i => $"import {{ {string.Join(", ", i.Value)} }} from \"/{Path.GetFileNameWithoutExtension(i.Key)}.js\"")) : null;
            return (importsFromSource + "\r\n" + importsFromModule + "\r\n" + string.Join("\r\n\r\n", TypeWriters.Values.Select(w => w.Build(formatTabs)))).Trim();
        }

        public override string ToString()
        {
            return Build(0);
        }
    }
}