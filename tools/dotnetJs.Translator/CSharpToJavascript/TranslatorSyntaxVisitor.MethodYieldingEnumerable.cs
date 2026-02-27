//using dotnetJs.Translator.CSharpToJavascript;
//using Microsoft.CodeAnalysis;
//using Microsoft.CodeAnalysis.CSharp;
//using Microsoft.CodeAnalysis.CSharp.Syntax;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace dotnetJs.Translator.CSharpToJavascript
//{
//    public partial class TranslatorSyntaxVisitor
//    {
//        int yieldStateMachineCurrentState;
//        Dictionary<YieldStatementSyntax, (int resumeState, SyntaxNode? resumeNode)>? yieldContext;
//        Dictionary<SyntaxNode, int>? stateStarts;
//        bool StateMachineActive => yieldContext != null;

//        SyntaxNode? GetNodeAfter(SyntaxNode node, bool canReturnParentAfter)
//        {
//            var siblings = node.Parent!.ChildNodes().ToArray();
//            var afterIndex = Array.IndexOf(siblings, node);
//            var after = siblings.ElementAtOrDefault(afterIndex + 1);
//            if (after == null && canReturnParentAfter)
//            {
//                //node is the last
//                return GetNodeAfter(node.Parent, true);
//            }
//            return after;
//        }

//        bool IsControlLoop(SyntaxNode node)
//        {
//            return node.IsKind(SyntaxKind.DoStatement) || node.IsKind(SyntaxKind.WhileStatement) || node.IsKind(SyntaxKind.ForStatement) || node.IsKind(SyntaxKind.ForEachStatement);
//        }

//        SyntaxNode? GetYieldResumeNode(SyntaxNode yieldNode, out bool isLoop)
//        {
//            isLoop = false;
//            var yAfter = GetNodeAfter(yieldNode, false);
//            if (yAfter == null)
//            {
//                //yield is the last
//                yAfter = yieldNode.Parent;
//                while (yAfter.IsKind(SyntaxKind.Block))
//                    yAfter = yAfter.Parent;
//                if (IsControlLoop(yAfter!))
//                {
//                    isLoop = true;
//                }
//                else
//                {
//                    yAfter = GetNodeAfter(yieldNode, true);
//                }
//            }
//            return yAfter;
//        }

//        void TryWrapInYieldingEnumerator(CSharpSyntaxNode node, IEnumerable<TypeParameterSyntax>? typeparameters, Action writeMethodBody)
//        {
//            if (node.DescendantNodes().Any(n => n.IsKind(SyntaxKind.YieldReturnStatement) || n.IsKind(SyntaxKind.YieldBreakStatement)))
//            {
//                var genericArgs = (typeparameters?.Any() ?? false) ? $"({string.Join(", ", typeparameters.Select(p => p.Identifier.ValueText))})" : "";
//                Writer.WriteLine(node, $"return new {_global.GlobalName}.System.YiedingEnumerable{genericArgs}(function()", true);
//                Writer.WriteLine(node, "{", true);

//                //define all state machine defined variables outside of the function
//                var variables = node.DescendantNodes().Where(e => e.IsKind(SyntaxKind.VariableDeclaration) || e.IsKind(SyntaxKind.VariableDeclarator));
//                foreach (var v in variables)
//                {
//                    Visit(v);
//                }

//                var loops = node.DescendantNodes().Where(e => e.IsKind(SyntaxKind.YieldReturnStatement) /*|| e.IsKind(SyntaxKind.YieldBreakStatement)*/);
//                var yields = node.DescendantNodes().Where(e => e.IsKind(SyntaxKind.YieldReturnStatement) /*|| e.IsKind(SyntaxKind.YieldBreakStatement)*/);
//                //Find every nodes following a yield
//                //every node after a yield will start a new machine state
//                //if yield is the last node in its parent, then it resumes at the parent block owner, if such is a control loop
//                yieldContext = new();
//                stateStarts = new();
//                yieldStateMachineCurrentState = 0;
//                int state = 1;
//                foreach (var y in yields)
//                {
//                    var yieldResumeAt = GetYieldResumeNode(y, out var isLoop);
//                    if (yieldResumeAt != node)
//                    {
//                        if (yieldResumeAt != null)
//                        {
//                            if (!stateStarts.TryGetValue(yieldResumeAt, out var sstate))
//                            {
//                                stateStarts.Add(yieldResumeAt, state);
//                            }
//                            else
//                            {
//                                state = sstate;
//                            }
//                        }
//                        yieldContext[(YieldStatementSyntax)y] = (state, yieldResumeAt);
//                    }
//                    state++;
//                }

//                Writer.Write(node, "let $state");
//                Writer.Write(node, " = ");
//                Writer.WriteLine(node, "0;");

//                Writer.WriteLine(node, "return function() /*MoveNext*/");
//                Writer.WriteLine(node, "{");

//                Writer.WriteLine(node, "for(;;)");
//                Writer.WriteLine(node, "{");

//                Writer.Write(node, "switch($state)");
//                Writer.WriteLine(node, "{");

//                Writer.Write(node, "case 0:");
//                Writer.WriteLine(node, "{");

//                VisitChildren(node.ChildNodes());

//                Writer.WriteLine(node, "}"); //end switch

//                Writer.WriteLine(node, "}"); //end for

//                writeMethodBody();

//                Writer.Write(node, "}", true); //end function MoveNext
//                Writer.Write(node, "}", true); //end YiedingEnumerable function

//                Writer.WriteLine(node, "});", true); //end return
//                return;

//            }
//            writeMethodBody();
//        }

//        public override void VisitYieldStatement(YieldStatementSyntax node)
//        {
//            if (node.IsKind(SyntaxKind.YieldBreakStatement))
//            {
//                Writer.WriteLine(node, $"return false;", true);
//            }
//            else
//            {
//                Writer.WriteLine(node, $"$state++;", true);
//                Writer.Write(node, $"$current = ", true);
//                Visit(node.Expression);
//                Writer.WriteLine(node, $";");
//                Writer.WriteLine(node, $"return true;");
//                Writer.WriteLine(node, "}"); //end the case we are in

//                var yieldC = yieldContext![node];

//                if (yieldC.resumeNode != null)
//                {
//                    //start a new state machine case
//                    Writer.Write(node, $"case {yieldC.resumeState}:");
//                    Writer.WriteLine(node, "{");
//                }
//            }
//            //if (node.IsKind(SyntaxKind.YieldBreakStatement))
//            //{
//            //    Writer.WriteLine(node, $"return $break();", true);
//            //}
//            //else
//            //{
//            //    Writer.Write(node, $"$yield(", true);
//            //    Visit(node.Expression);
//            //    Writer.WriteLine(node, $", function($yield, $break)");
//            //    Writer.WriteLine(node, "{", true);
//            //    CurrentClosure.OnClosing += (s, e) =>
//            //    {
//            //        Writer.WriteLine(node, "});", true);
//            //    };
//            //}
//            //base.VisitYieldStatement(node);
//        }
//    }
//}
