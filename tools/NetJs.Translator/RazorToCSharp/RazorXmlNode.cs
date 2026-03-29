using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetJs.Translator.RazorToCSharp
{

    public abstract class RazorXmlNode
    {
        protected RazorXmlNode(RazorXmlNode? parent)
        {
            Parent = parent;
        }

        public RazorXmlNode? Parent { get; set; }

        bool IsInRazorCodeBlock
        {
            get
            {
                var parent = Parent;
                while (parent != null)
                {
                    bool isRazorCodeBlock = parent is RazorCodeBlock block && (block.BlockStart == "code" || string.IsNullOrEmpty(block.BlockStart));
                    if (isRazorCodeBlock)
                        return true;
                    parent = parent.Parent;
                }
                return false;
            }
        }

        int? _depth;
        public int Depth
        {
            get
            {
                if (_depth != null)
                    return _depth.Value;
                var parent = Parent;
                int depth = 0;
                while (parent != null)
                {
                    bool isRazorCodeBlock = parent is RazorCodeBlock block && block.BlockStart == "code";
                    if (!isRazorCodeBlock)
                        depth++;
                    parent = parent.Parent;
                }
                _depth = depth;
                return depth;
            }
        }

        protected string ToStringFormatTabs
        {
            get
            {
                string t = "";
                int depth = Depth;
                while (depth-- > 0)
                {
                    t += "    ";
                }
                return t;
            }
        }

        protected string GetCodeFormatTabs(int addTabs)
        {
            string t = "";
            int depth = Depth + addTabs + (IsInRazorCodeBlock ? 2 : 3);
            while (depth-- > 0)
            {
                t += "    ";
            }
            return t;
        }


        public abstract string GenerateCode(int tabDepth, int parameterDepth, ComponentCodeGenerationContext context);
    }
}
