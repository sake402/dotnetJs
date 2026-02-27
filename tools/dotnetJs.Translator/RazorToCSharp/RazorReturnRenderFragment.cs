namespace dotnetJs.Translator.RazorToCSharp
{
    public class RazorReturnRenderFragment : RazorXmlNode
    {
        public RazorXmlNode _return;

        public RazorReturnRenderFragment(RazorXmlNode @return, RazorXmlNode? parent) : base(parent)
        {
            _return = @return;
        }

        public override string GenerateCode(int tabDepth, int parameterDepth, ComponentCodeGenerationContext context)
        {
            return @$"{GetCodeFormatTabs(tabDepth)}return (__frame{parameterDepth}, __key{parameterDepth}) =>
{GetCodeFormatTabs(tabDepth)}{{
{_return.GenerateCode(tabDepth+1, parameterDepth, context)}
{GetCodeFormatTabs(tabDepth)}}};";
        }
    }
}
