using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace dotnetJs.Translator.RazorToCSharp
{
    public partial class RazorComponentParser
    {
        string razorCode;
        int position;

        public RazorComponentParser(string razorCode)
        {
            this.razorCode = razorCode;
        }

        public char Current => At(position);
        public char Next => At(position + 1);
        public bool EndOfFile => position >= razorCode.Length;
        public ReadOnlySpan<char> Remaining => position < razorCode.Length ? razorCode.AsSpan().Slice(position) : ReadOnlySpan<char>.Empty;
        public bool IsStartingElementNode => Current == '<' && Next != '/';
        public bool IsClosingElementNode => Current == '<' && Next == '/';

        public ReadOnlySpan<char> StringAt(int i)
        {
            return i < razorCode.Length ? razorCode.AsSpan().Slice(i) : ReadOnlySpan<char>.Empty;
        }

        public char At(int i)
        {
            if (i < razorCode.Length)
                return razorCode[i];
            return '\0';
        }

        public bool CompareAt(int start, string with, bool skipWhiteSpaces = false)
        {
            int with_i = 0;
            for (int i = 0; ; i++)
            {
                if (skipWhiteSpaces && IsWhiteSpace(At(start + i)))
                {
                    continue;
                }
                if (At(start + i) != with[with_i])
                    return false;
                with_i++;
                if (with_i >= with.Length)
                    break;
            }
            return true;
        }

        public bool IsWhiteSpace(int where) => " \t\r\n".Contains(At(where));
        public bool IsRazorOpeningComment(int where) => At(where) == '@' && At(where + 1) == '*';

        static string[] cSharpBlockStatementsStart = new[] { "if", "do", "while", "for", "foreach", "code" };
        static string[] cSharpContinueBlockStatementsStart = new[] { "else", "else if" };
        public bool IsCodeBlockStart(int where, RazorXmlNode? parentNode, RazorCodeBlock? lastCodeBlock, out int codeBlockStart, out int codeBlockEnd)
        {
            where = SkipWhiteSpaceAndComments(where);
            char c = At(where);
            if (c == '<')
            {
                codeBlockStart = -1;
                codeBlockEnd = -1;
                return false;
            }
            //codeBlockStart = where;
            bool isAt = c == '@';
            if (isAt)
            {
                where = SkipWhiteSpace(where);
            }
            bool isStartBlock = isAt && (At(where + 1) == '{' || cSharpBlockStatementsStart.Any(c =>
            {
                if (CompareAt(where + 1, c))
                {
                    var startLen = c.Length;
                    int braceOpen = SkipWhiteSpaceAndComments(where + 1 + startLen);
                    var next = At(braceOpen);
                    if (next == '{' || next == '(')
                    {
                        return true;
                    }
                }
                return false;
            }));
            bool parentIsRazorCodeBlock = parentNode is RazorCodeBlock codeBlock;
            if (parentIsRazorCodeBlock)
            {
                if (At(where) == '{')
                    isStartBlock = true;
                else if (cSharpBlockStatementsStart.Any(statement => CompareAt(where, statement)))
                {
                    isStartBlock = true;
                }
                else
                {
                    var methodMatch = Regex.Match(
                        Window(where, where + Math.Min(razorCode.Length - 1 - where, 512)).ToString(),
                        "^[A-Za-z0-9-_<>]+\\s+[A-Za-z_]+<?.?>?\\(?.*\\)?\\s*({|(=>)).?");
                    if (methodMatch.Success) //match a method/property signature
                    {
                        isStartBlock = true;
                    }
                }
            }
            bool isContinueBlock = lastCodeBlock?.ContinueBlock != null && lastCodeBlock.ContinueBlock.Any(b => CompareAt(where, b));
            if (isStartBlock || isContinueBlock) //@{ or @if ()
            {
                while (true)
                {
                    c = At(where);
                    if (c == '\0')
                        break;
                    if (parentIsRazorCodeBlock && c == ';') //we found end of statement, not a block
                        break;
                    if (c == '"')
                    {
                        where = FindQuoteEnd(where, true);
                    }
                    else if (c == '{')
                    {
                        codeBlockStart = where;
                        codeBlockEnd = FindCurlyBracketEnd(where);
                        return true;
                    }
                    else if (c == '=' && At(where + 1) == '>')//match method bodied expression =>
                    {
                        codeBlockStart = where;
                        codeBlockEnd = FindSemiColon(where);
                        return true;
                    }
                    else
                    {
                        where++;
                    }
                }
            }
            codeBlockStart = -1;
            codeBlockEnd = -1;
            return false;
        }

        int ReadUntil(int start, char breakChar)
        {
            int where = start;
            while (true)
            {
                char c = At(where);
                if (c == '\0')
                    return -1;
                if (c == breakChar)
                    return where - 1;
                where++;
            }
        }

        int ReadUntilAny(int start, string breakChars)
        {
            int where = start;
            while (true)
            {
                char c = At(where);
                if (c == '\0')
                    return -1;
                if (breakChars.Contains(c))
                    return where - 1;
                where++;
            }
        }

        int ReadWhile(int start, Func<char, int, bool> breakDelegate)
        {
            int where = start;
            while (true)
            {
                char c = At(where);
                if (c == '\0')
                    return -1;
                if (!breakDelegate(c, where))
                    return where - 1;
                where++;
            }
        }


        int FindFormatEnd(int start)
        {
            if (At(start) == '{')
            {
                int where = start + 1;
                while (true)
                {
                    char c = At(where);
                    if (c == '(')
                    {
                        where = FindBracketEnd(where);
                        where++;
                    }
                    else if (c == '}')
                        return where;
                    else
                    {
                        where++;
                    }
                }
            }
            throw new InvalidOperationException("Exprected {");
        }

        int FindQuoteEnd(int start, bool allowFormat)
        {
            if (At(start) == '"')
            {
                int where = start + 1;
                while (true)
                {
                    char c = At(where);
                    if (c == '\0')
                        return -1;
                    else if (c == '(')
                    {
                        where = FindBracketEnd(where);
                    }
                    else if (c == '{' && allowFormat)
                    {
                        where = FindFormatEnd(where);
                    }
                    else if (c == '"')
                    {
                        if (At(where - 1) != '\\') //if escaped dont break yet
                        {
                            return where;
                        }
                    }
                    where++;
                }
            }
            throw new InvalidOperationException("Not a quote");
        }

        int FindBracketEnd(int start)
        {
            if (At(start) == '(')
            {
                int where = start + 1;
                int depth = 0;
                while (true)
                {
                    char c = At(where);
                    if (c == '\0')
                        return -1;
                    if (c == '"') //if we encounter a quote within c#, its a string skip to its end
                    {
                        where = FindQuoteEnd(where, At(where - 1) == '$');
                        if (where < 0)
                            return where;
                    }
                    else if (c == '(')
                        depth++;
                    else if (c == ')')
                    {
                        if (depth > 0)
                            depth--;
                        else
                            return where;
                    }
                    where++;
                }
            }
            throw new InvalidOperationException("Not a brace");
        }

        int FindCurlyBracketEnd(int start)
        {
            if (At(start) == '{')
            {
                int where = start + 1;
                int depth = 0;
                while (true)
                {
                    char c = At(where);
                    if (c == '\0')
                        throw new InvalidOperationException("No matching curly brace");
                    /*if (c == '"') //if we encounter a quote within c#, its a string skip to its end
                    {
                        where = FindQuoteEnd(where, At(where - 1) == '$');
                        if (where < 0)
                            return where;
                    }
                    else */
                    if (c == '{')
                        depth++;
                    else if (c == '}')
                    {
                        if (depth > 0)
                            depth--;
                        else
                            return where;
                    }
                    where++;
                }
            }
            throw new InvalidOperationException("Not a brace");
        }

        int FindSemiColon(int start)
        {
            int where = start;
            bool isInQuote = false;
            while (true)
            {
                char c = At(where);
                if (c == '\0')
                    throw new InvalidOperationException("No semi colon found");
                if (c == '"')
                    isInQuote = !isInQuote;
                if (c == ';' && !isInQuote)
                    return where;
                where++;
            }
        }

        int FindAngleEnd(int start)
        {
            if (At(start) == '<')
            {
                int where = start + 1;
                int depth = 0;
                while (true)
                {
                    char c = At(where);
                    if (c == '\0')
                        throw new InvalidOperationException("No matching curly brace");
                    if (c == '<')
                        depth++;
                    else if (c == '>')
                    {
                        if (depth > 0)
                            depth--;
                        else
                            return where;
                    }
                    where++;
                }
            }
            throw new InvalidOperationException("Not an angle");
        }

        int FindRazorCommentEnd(int start)
        {
            if (At(start) == '@' && At(start + 1) == '*')
            {
                int where = start + 2;
                while (true)
                {
                    char c = At(where);
                    if (c == '*' && At(where + 1) == '@')
                    {
                        return where + 1;
                    }
                    where++;
                }
            }
            throw new InvalidOperationException("Expected @*");
        }

        int FindHtmlCommentEnd(int start)
        {
            if (At(start) == '<' && At(start + 1) == '!')
            {
                int where = start + 2;
                while (true)
                {
                    char c = At(where);
                    if (c == '-' && At(where + 1) == '>')
                    {
                        return where + 1;
                    }
                    where++;
                }
            }
            throw new InvalidOperationException("Expected @*");
        }
        //<br />
        //<p></p>
        int FindHtmlEnd(int start)
        {
            if (At(start) == '<')
            {
                int stateStartIndex = 0;
                ReadOnlySpan<char> currentTag = ReadOnlySpan<char>.Empty;
                HtmlParseState state = HtmlParseState.StartOpenend;
                int where = start + 1;
                bool IsInCode() => state.HasFlag(HtmlParseState.Code);
                HtmlParseState StateNoFlags() => state & ~HtmlParseState.Flags;
                bool IsInAttributeValue() => StateNoFlags() == HtmlParseState.AttributeValue;
                while (true)
                {
                    char c = At(where);
                    if (c == '\0')
                        throw new InvalidOperationException("Malformed html");
                    if (c == '/' && At(where + 1) == '>')
                    {
                        return where + 1;
                    }
                    else if ((StateNoFlags() == HtmlParseState.StartOpenend ||
                        StateNoFlags() == HtmlParseState.TagName ||
                        StateNoFlags() == HtmlParseState.TagNamed ||
                        StateNoFlags() == HtmlParseState.AttributeKey ||
                        StateNoFlags() == HtmlParseState.AttributeKeyed ||
                        StateNoFlags() == HtmlParseState.AttributeValue ||
                        StateNoFlags() == HtmlParseState.AttributeValued) && c == '>')
                    {
                        if (StateNoFlags() == HtmlParseState.TagName || StateNoFlags() == HtmlParseState.TagNamed)
                        {
                            currentTag = Window(stateStartIndex, where - 1);
                        }
                        if (IsSelfClosingTag(currentTag))
                            return where;
                        stateStartIndex = where;
                        state = HtmlParseState.StartClosed;
                    }
                    else if (StateNoFlags() == HtmlParseState.StartClosed && c == '@' && IsCodeBlockStart(where, null, null, out var codeBlockStart, out var codeBlockEnd))
                    {
                        where = codeBlockEnd;
                    }
                    else if (StateNoFlags() == HtmlParseState.StartClosed && c == '<' && At(where + 1) == '/')
                    {
                        stateStartIndex = where;
                        state = HtmlParseState.EndOpened;
                        where++;
                    }
                    else if (StateNoFlags() == HtmlParseState.EndOpened && c == '>')
                    {
                        return where;
                    }
                    else if (!IsInCode() && !IsInAttributeValue() && c == '<')
                    {
                        where = FindHtmlEnd(where);
                        if (where < 0)
                            throw new InvalidOperationException("Malformed html");
                    }
                    else if (StateNoFlags() == HtmlParseState.StartOpenend && c != ' ')
                    {
                        stateStartIndex = where;
                        state = HtmlParseState.TagName;
                    }
                    else if (StateNoFlags() == HtmlParseState.TagName && c == ' ')
                    {
                        currentTag = Window(stateStartIndex, where - 1);
                        stateStartIndex = where;
                        state = HtmlParseState.TagNamed;
                    }
                    else if ((StateNoFlags() == HtmlParseState.TagNamed || StateNoFlags() == HtmlParseState.AttributeValued) && c != ' ')
                    {
                        stateStartIndex = where;
                        if (c == '@')
                        {
                            state = HtmlParseState.AttributeKey | HtmlParseState.Code;
                        }
                        else
                        {
                            state = HtmlParseState.AttributeKey;
                        }
                    }
                    else if (StateNoFlags() == HtmlParseState.AttributeKey && c == '=')
                    {
                        stateStartIndex = where;
                        var flag = state & HtmlParseState.Flags;
                        state = HtmlParseState.AttributeKeyed | flag;
                    }
                    else if (StateNoFlags() == HtmlParseState.AttributeKeyed && (c == '"' || c != ' '))
                    {
                        stateStartIndex = where;
                        var flag = state & HtmlParseState.Flags;
                        if (c == '@')
                        {
                            state = HtmlParseState.AttributeValue | HtmlParseState.Code | flag;
                            where++;
                        }
                        else if (c == '"')
                        {
                            if (At(where + 1) == '@')
                            {
                                state = HtmlParseState.AttributeValue | HtmlParseState.Code | HtmlParseState.Quoted | flag;
                                where++;
                            }
                            else
                            {
                                state = HtmlParseState.AttributeValue | HtmlParseState.Quoted | flag;
                            }
                        }
                        else
                        {
                            state = HtmlParseState.AttributeValue | flag;
                        }
                    }
                    else if (StateNoFlags() == HtmlParseState.AttributeValue && (state.HasFlag(HtmlParseState.Quoted) && c == '"' || !state.HasFlag(HtmlParseState.Quoted) && c == ' '))
                    {
                        stateStartIndex = where;
                        var flag = state & HtmlParseState.Flags;
                        state = HtmlParseState.AttributeValued | flag;
                    }
                    where++;
                }
            }
            throw new InvalidOperationException("Expected @*");
        }

        ReadOnlySpan<char> Window(int start, int end)
        {
            if (end == start - 1)
                return ReadOnlySpan<char>.Empty;
            if (end < start)
            {
                throw new InvalidOperationException("Invalid range");
            }
            return razorCode.AsSpan(start, end - start + 1);
        }

        ReadOnlyMemory<char> WindowAsMemory(int start, int end)
        {
            if (end == start - 1)
                return ReadOnlyMemory<char>.Empty;
            if (end < start)
            {
                throw new InvalidOperationException("Invalid range");
            }
            return razorCode.AsMemory(start, end - start + 1);
        }

        public int SkipRazorComment(int start)
        {
            int where = start;
            if (IsRazorOpeningComment(where))
            {
                where = FindRazorCommentEnd(where) + 1;
            }
            return where;
        }

        public int SkipWhiteSpace(int start)
        {
            int where = start;
            while (" \t\r\n".Contains(At(where)))
            {
                where++;
            }
            return where;
        }

        public int SkipWhiteSpaceAndComments(int start)
        {
            int where = start;
            while (IsRazorOpeningComment(where) || IsWhiteSpace(where))
            {
                if (IsRazorOpeningComment(where))
                    where = SkipRazorComment(where);
                if (IsWhiteSpace(where))
                    where = SkipWhiteSpace(where);
            }
            return where;
        }

        public ReadOnlySpan<char> GetWord(string breakChars = " \r\n=<>/\\")
        {
            position = SkipWhiteSpace(position);
            int start = position;
            int end = ReadUntilAny(position, breakChars);
            position = end + 1;
            return Window(start, end);
        }

        public ReadOnlySpan<char> GetWhile(Func<char, int, bool> breakDelegate)
        {
            position = SkipWhiteSpace(position);
            int start = position;
            int end = ReadWhile(position, breakDelegate);
            position = end + 1;
            return Window(start, end);
        }

        public ReadOnlySpan<char> GetExpression()
        {
            position = SkipWhiteSpace(position);
            int where = position;
            int start = position;
            while (true)
            {
                char c = At(where);
                if (c == '(')
                {
                    where = FindBracketEnd(where);
                }
                else if (c == '<' || IsWhiteSpace(where))
                {
                    break;
                }
                where++;
            }
            position = where;
            return Window(start, where - 1);
        }

        public ReadOnlySpan<char> GetTypeName()
        {
            position = SkipWhiteSpace(position);
            int where = position;
            int start = position;
            while (true)
            {
                char c = At(where);
                if (c == '<')
                {
                    where = FindAngleEnd(where);
                    where++;
                    break;
                }
                else if (char.IsWhiteSpace(c))
                    break;
                where++;
            }
            position = where;
            return Window(start, where - 1);
        }



        //ReadOnlySpan<char> ReadCSharpExpression()
        //{
        //    if (Current == '@')
        //    {
        //        int start = position + 1;
        //        int end = ReadUntilAny(start, " ");
        //        position = end + 1;
        //        return Window(start, end);
        //    }
        //    return ReadOnlySpan<char>.Empty;
        //}

        List<RazorTextBaseNode> ParseTextContentNodes(RazorXmlNode? outerParentNode)
        {
            List<RazorTextBaseNode> nodes = new List<RazorTextBaseNode>();
            int where = position;
            int start = position;
            while (true)
            {
                char c = At(where);
                if (c == '<' || c == '\0')
                {
                    if (where > start)
                    {
                        var text = Window(start, where - 1).Trim(['\r', '\n']);
                        if (text.Length > 0)
                        {
                            nodes.Add(new RazorTextNode(text.ToString(), outerParentNode));
                        }
                    }
                    break;
                }
                else if (c == '}' && outerParentNode is RazorCodeBlock)
                {
                    break;
                }
                else if (c == '@')
                {
                    if (where > start)
                    {
                        var text = Window(start, where - 1).Trim(['\r', '\n']);
                        if (text.Length > 0)
                        {
                            nodes.Add(new RazorTextNode(text.ToString(), outerParentNode));
                        }
                    }
                    if (IsCodeBlockStart(where, outerParentNode, null, out var _, out var _))
                    {
                        break;
                    }
                    //if (At(where + 1) == '(')
                    //{
                    //    var braceEnd = FindBracketEnd(where + 1);
                    //    var value = Window(where + 1, braceEnd);
                    //    nodes.Add(new RazorBindingNode(value.ToString(), outerParentNode));
                    //    position = braceEnd + 1;
                    //    start = where = braceEnd + 1;
                    //}
                    //else
                    {
                        position = where + 1;
                        var value = GetExpression();
                        start = where = position;
                        nodes.Add(new RazorBindingNode(value.ToString(), outerParentNode));
                        //add a space delimiter after the binding expression
                        //nodes.Add(new RazorTextNode(" ", outerParentNode));
                    }
                    continue;
                }
                where++;
            }
            position = where;
            return nodes;
        }

        static string[] SelfClosingTags = new string[] { "input", "br" };

        bool IsSelfClosingTag(ReadOnlySpan<char> tag)
        {
            tag = tag.Trim();
            foreach (var t in SelfClosingTags)
                if (tag.SequenceEqual(t.AsSpan()))
                    return true;
            return false;
        }

        IEnumerable<RazorXmlElementNode> ParseElementNodes(RazorXmlNode? outerParentNode)
        {
            List<RazorXmlElementNode> nodes = new List<RazorXmlElementNode>();
            position = SkipWhiteSpaceAndComments(position);
            while (IsStartingElementNode)
            {
                int startHtml = position;
                var endHtml = FindHtmlEnd(position);
                var rawHtml = WindowAsMemory(startHtml, endHtml);
                position++;
                var tagName = GetWord();
                var node = new RazorXmlElementNode(tagName.ToString(), rawHtml, outerParentNode);
                nodes.Add(node);
                position = SkipWhiteSpaceAndComments(position);
                while (Current != '>' && Current != '/')
                {
                    var attrName = GetWord();
                    position = SkipWhiteSpace(position);
                    string? value = null;
                    if (Current == '=')
                    {
                        position++;
                        if (Current == '"')
                        {
                            var endQuote = FindQuoteEnd(position, false);
                            value = Window(position + 1, endQuote - 1).ToString();
                            position = endQuote + 1;
                        }
                        else
                        {
                            value = GetExpression().ToString();
                        }
                    }
                    node.Children.Add(new RazorXmlElementAttribute(attrName.ToString(), value, node));
                    position = SkipWhiteSpace(position);
                }
                position = SkipWhiteSpaceAndComments(position);
                if (Current == '/' && Next == '>') //closed
                {
                    position += 2;
                    position = SkipWhiteSpaceAndComments(position);
                    continue;
                }
                if (Current == '>')
                {
                    position++;
                    if (SelfClosingTags.Contains(node.TagName))
                    {
                        position = SkipWhiteSpaceAndComments(position);
                        continue;
                    }
                }
                position = SkipWhiteSpaceAndComments(position);
                var innerNodes = ParseNodes(node);
                node.Children.AddRange(innerNodes);
                position = SkipWhiteSpaceAndComments(position);
                if (Current == '<')
                {
                    if (Next == '/') //closing
                    {
                        position += 2;
                        var closingTag = GetWord().ToString();
                        if (closingTag != node.TagName)
                        {
                            throw new InvalidOperationException($"Expected </{node.TagName}");
                        }
                        if (Current != '>')
                        {
                            throw new InvalidOperationException("Expected >");
                        }
                        position++;
                    }
                    else
                    {
                        throw new InvalidOperationException("Expected </");
                    }
                }
                else
                {
                    throw new InvalidOperationException("Expected <");
                }
                position = SkipWhiteSpaceAndComments(position);
            }
            return nodes;
        }

        RazorCodeBlock ParseCodeBlock(int codeBlockStart, int codeBlockEnd, RazorCodeBlock? lastCodeBlock, RazorXmlNode? outerParentNode)
        {
            var codeOpening = At(codeBlockStart);
            if ((Current == '@' ||
                outerParentNode is RazorCodeBlock ||
                (lastCodeBlock?.ContinueBlock?.Any(b => CompareAt(position, b)) ?? false)) && (codeOpening == '{' || codeOpening == '=' && At(codeBlockStart + 1) == '>'))
            {
                var codeOpeningToken = Window(position + (Current == '@' ? 1 : 0), codeBlockStart - 1).Trim().ToString();
                var block = new RazorCodeBlock(codeOpeningToken, "", outerParentNode);
                if (codeOpening == '=')/*=>*/
                {

                }
                var codeStart = codeBlockStart + 1 + (codeOpening == '='/*=>*/ ? 1 : 0);
                //var codeEntry = Window(codeStart, codeBlockEnd - 1);
                int where = codeStart;
                bool statementStarted = false;
                where = SkipWhiteSpaceAndComments(where);
                int start = where;
                void CollectStatement()
                {
                    if (where > start)
                    {
                        var code = Window(start, where - 1).Trim();
                        if (code.Length > 0)
                        {
                            block.Children.Add(new RazorCSharpStatement(code.ToString(), block));
                        }
                    }
                }
                while (where < codeBlockEnd - 1)
                {
                    if (!statementStarted && IsCodeBlockStart(where, block, null, out var _codeBlockStart, out var _codeBlockEnd)) //code block inside code block
                    {
                        position = where;
                        var codeBlocks = ParseCodeBlocks(block);
                        //var codeBlock = ParseCodeBlock(_codeBlockStart, _codeBlockEnd, lastCodeBlock, block);
                        block.Children.AddRange(codeBlocks);
                        position = SkipWhiteSpaceAndComments(position);
                        where = start = position;
                    }
                    else
                    {
                        char c = At(where);
                        if (c == '<' && At(where + 1) != '/' && !statementStarted)
                        {
                            CollectStatement();
                            position = where;
                            var nodes = ParseElementNodes(block);
                            block.Children.AddRange(nodes);
                            position = SkipWhiteSpaceAndComments(position);
                            start = where = position;
                            statementStarted = false;
                            continue;
                        }
                        else if (c == '@' && !statementStarted)
                        {
                            CollectStatement();
                            position = where;
                            if (Next == '<')
                            {
                                position++;
                                var nodes = ParseElementNodes(block);
                                if (codeOpening == '=' && At(codeBlockStart + 1) == '>') //method bodied expression must use return
                                {
                                    var _return = new RazorReturnRenderFragment(nodes.Single(), block);
                                    block.Children.Add(_return);
                                }
                                else
                                {
                                    block.Children.AddRange(nodes);
                                }
                            }
                            else
                            {
                                var nodes = ParseTextContentNodes(block);
                                block.Children.AddRange(nodes);
                            }
                            start = where = position;
                            continue;
                        }
                        else if (statementStarted && c == '@' && At(where + 1) == '<'/* && CompareAt(where, "@<text>") */&& (CompareAt(start, "RenderFragment") || CompareAt(start, "return"))) //handle RenderFragment view = @<whatever/>; and return @<text>
                        {
                            //CollectStatement();
                            var endHtml = FindHtmlEnd(where + 1);
                            var rawHtml = WindowAsMemory(where + 1, endHtml);
                            position = where + 1;
                            var renderFragment = new RazorCSharpRenderFragmentStatement(Window(start, where - 1).ToString(), rawHtml, block);
                            block.Children.Add(renderFragment);
                            IEnumerable<RazorXmlNode> nodes = ParseElementNodes(renderFragment);
                            position = SkipWhiteSpaceAndComments(position);
                            if (Current != ';')
                            {
                                throw new InvalidOperationException("; expected");
                            }
                            position++;
                            position = SkipWhiteSpaceAndComments(position);
                            start = where = position;
                            //if (nodes.Count() == 1 && ((RazorXmlElementNode)nodes.Single()).TagName == "text") //uses a defaut <text> element that should not be rendered
                            //{
                            //    nodes = ((RazorXmlElementNode)nodes.Single()).Children;
                            //    foreach (var node in nodes)
                            //    {
                            //        node.Parent = renderFragment; //since we are removing the <text> node, switch the parent to RenderFragment view = ...
                            //    }
                            //}
                            renderFragment.Children.AddRange(nodes);
                        }
                        else if (statementStarted && c == ';') //end of code statement
                        {
                            where++;
                            CollectStatement();
                            position = where;
                            position = SkipWhiteSpaceAndComments(position);
                            start = where = position;
                            statementStarted = false;
                            continue;
                        }
                        else
                        {
                            statementStarted = true;
                        }
                        where++;
                    }
                }
                if (where > start && where < codeBlockEnd)
                {
                    var code = Window(start, where).Trim();
                    if (code.Length > 0)
                    {
                        block.Children.Add(new RazorCSharpStatement(code.ToString(), block));
                    }
                }
                position = codeBlockEnd + 1;
                return block;
            }
            throw new InvalidOperationException("Expected {");
        }

        IEnumerable<RazorXmlNode> ParseCodeBlocks(RazorXmlNode? outerParentNode)
        {
            List<RazorXmlNode> nodes = new List<RazorXmlNode>();
            position = SkipWhiteSpaceAndComments(position);
            RazorCodeBlock? lastCodeBlock = null; //to detect else
            while (IsCodeBlockStart(position, outerParentNode, lastCodeBlock, out var codeBlockStart, out var codeBlockEnd))
            {
                var codeBlock = ParseCodeBlock(codeBlockStart, codeBlockEnd, lastCodeBlock, outerParentNode);
                lastCodeBlock = codeBlock;
                nodes.Add(codeBlock);
                position = SkipWhiteSpaceAndComments(position);
            }
            return nodes;
        }

        //IEnumerable<RazorCSharpCodeBlock> ParseCodeBlocks(RazorXmlNode? outerParentNode)
        //{
        //    List<RazorCSharpCodeBlock> nodes = new List<RazorCSharpCodeBlock>();
        //    position = SkipWhiteSpaceAndComments(position);
        //    while (Current == '@' && CompareAt(position+1, "code"))
        //    {
        //        int codeBlockStart = ReadUntil(position, '{');
        //        var codeBlockEnd = FindCurlyBracketEnd(codeBlockStart);

        //        position = SkipWhiteSpaceAndComments(position);
        //    }
        //    return nodes;
        //}

        IEnumerable<RazorXmlNode> ParseNodes(RazorXmlNode? outerParentNode)
        {
            List<RazorXmlNode> nodes = new List<RazorXmlNode>();
            position = SkipWhiteSpaceAndComments(position);
            while (!EndOfFile && !IsClosingElementNode)
            {
                position = SkipWhiteSpaceAndComments(position);
                nodes.AddRange(ParseCodeBlocks(outerParentNode));
                //position = SkipWhiteSpaceAndComments(position);
                //nodes.AddRange(ParseCodeBlocks(outerParentNode));
                position = SkipWhiteSpaceAndComments(position);
                nodes.AddRange(ParseElementNodes(outerParentNode));
                position = SkipWhiteSpaceAndComments(position);
                nodes.AddRange(ParseTextContentNodes(outerParentNode));
                if (outerParentNode == null)
                    break;
                position = SkipWhiteSpaceAndComments(position);
            }
            return nodes;
        }

        public IEnumerable<RazorXmlNode> ParseRootNodes()
        {
            List<RazorXmlNode> nodes = new List<RazorXmlNode>();
            position = SkipWhiteSpaceAndComments(position);
            while (Current != '\0')
            {
                nodes.AddRange(ParseNodes(null));
            }
            return nodes;
        }

        public RazorComponent Parse()
        {
            RazorComponent result = new RazorComponent();
            position = SkipWhiteSpaceAndComments(position);
            bool _break = false;
            while (Current == '@' && !_break)
            {
                if (Next == '*')
                {
                    position = FindRazorCommentEnd(position);
                    position++;
                    continue;
                }
                if (IsCodeBlockStart(position, null, null, out _, out _))
                    break;
                int rewind = position;
                var @specifier = GetWord();
                //var @value = GetWord("\r\n \t");
                switch (specifier.ToString())
                {
                    case "@inherits":
                        if (result.Inherit != null)
                        {
                            throw new InvalidOperationException("Can only have one inherit atribute");
                        }
                        result.Inherit = new RazorInherit(GetTypeName().ToString());
                        break;
                    case "@namespace":
                        if (result.Inherit != null)
                        {
                            throw new InvalidOperationException("Can only have one inherit atribute");
                        }
                        result.Namespace = new RazorNamespace(GetTypeName().ToString());
                        break;
                    case "@layout":
                        if (result.Layout != null)
                        {
                            throw new InvalidOperationException("Can only have one layout atribute");
                        }
                        result.Layout = new RazorLayout(GetTypeName().ToString());
                        break;
                    case "@page":
                        position = SkipWhiteSpace(position);
                        if (At(position) == '"')
                        {
                            position++;
                            var route = GetWhile((e, i) => e != '"');
                            position++;
                            result.Routes.Add(new RazorPage(route.ToString()));
                        }
                        else
                        {
                            throw new InvalidOperationException("Route path must be quoted");
                        }
                        break;
                    case "@using":
                        result.Usings.Add(new RazorUsing(GetWord().ToString()));
                        break;
                    case "@typeparam":
                        result.TemplateTypes.Add(new RazorTemplateTypeName(GetWord().ToString()));
                        break;
                    case "@inject":
                        var type = GetTypeName();
                        var name = GetWord();
                        result.Injects.Add(new RazorInject(type.ToString(), name.ToString()));
                        break;
                    case "@attribute":
                        var att = GetWord();
                        result.Attributes.Add(new RazorAttribute(att.ToString()));
                        break;
                    default:
                        position = rewind;
                        _break = true;
                        break;
                        //throw new NotImplementedException($"Declaration for {specifier} not implemented");
                }
                position = SkipWhiteSpaceAndComments(position);
            }

            result.RootNodes.AddRange(ParseRootNodes());

            //var code = result.Generate();

            return result;
        }
    }
}
