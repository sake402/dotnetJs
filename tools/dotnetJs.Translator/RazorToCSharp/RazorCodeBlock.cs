using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dotnetJs.Translator.RazorToCSharp
{
    public enum BlockType
    {
        None,
        IF,
        ELSEIF,
        ELSE,
        DO,
        WHILE,
        FOR,
        FOREACH
    }

    /// <summary>
    /// Abstract a c# code brace { }
    /// </summary>
    public class RazorCodeBlock : RazorXmlHasChildrenNode
    {
        public RazorCodeBlock(string blockStart, string blockEnd, RazorXmlNode? parentNode) : base(parentNode)
        {
            BlockStart = blockStart;
            BlockEnd = blockEnd;
        }

        public string BlockStart { get; }
        public string BlockEnd { get; }

        public bool IsCodeBlock => BlockStart == "code";

        public BlockType BlockType
        {
            get
            {
                if (BlockStart.StartsWith("if"))
                    return BlockType.IF;
                if (BlockStart.StartsWith("else if"))
                    return BlockType.ELSEIF;
                if (BlockStart.StartsWith("else"))
                    return BlockType.ELSE;
                if (BlockStart.StartsWith("do"))
                    return BlockType.DO;
                if (BlockStart.StartsWith("while"))
                    return BlockType.WHILE;
                if (BlockStart.StartsWith("for"))
                    return BlockType.FOR;
                if (BlockStart.StartsWith("foreach"))
                    return BlockType.FOREACH;
                return BlockType.None;
            }
        }

        public string[]? ContinueBlock
        {
            get
            {
                switch (BlockType)
                {
                    case BlockType.IF:
                        return new[] { "else", "else if " };
                    case BlockType.ELSEIF:
                        return new[] { "else" };
                }
                return null;
            }
        }

        public override string ToString()
        {
            return @$"{ToStringFormatTabs}@{BlockStart}
{ToStringFormatTabs}{{
{base.ToString()}
{ToStringFormatTabs}}}{BlockEnd}";
        }

        public override string GenerateCode(int tabDepth, int parameterDepth, ComponentCodeGenerationContext context)
        {
            bool noCodeBlock = string.IsNullOrEmpty(BlockStart) || BlockStart == "code";
            return $@"{GetCodeFormatTabs(tabDepth)}{(noCodeBlock ? "" : BlockStart)}
{GetCodeFormatTabs(tabDepth)}{(noCodeBlock ? "" : "{")}
{string.Join("\r\n", Children.Select(s => s.GenerateCode(tabDepth, parameterDepth, context)))}
{GetCodeFormatTabs(tabDepth)}{(noCodeBlock ? "" : "}")}{BlockEnd}";
        }
    }
}
