using System.IO;
//using CodeLineWriter = System.IO.StringWriter;

namespace dotnetJs.Translator.CSharpToJavascript
{
    public class CodeLineWriter
    {
        StringWriter internalWriter = new StringWriter();
        char lastChar;
        string? lastWord;
        public LinkedListNode<CodeLineWriter> Node { get; set; } = default!;
        public CodeLineWriter? RedirectInsertBefore { get; set; }
        void ValidateChar(char firstChar)
        {
            if (lastChar == '(' && firstChar == ',')
                throw new InvalidOperationException("Syntax would not be valid");
            if (lastChar == '(' && firstChar == '=')
                throw new InvalidOperationException("Syntax would not be valid");
            if (lastChar == '(' && (firstChar == '>' || firstChar == '<' || firstChar == '='))
                throw new InvalidOperationException("Syntax would not be valid");
            if ((lastChar == '(' || lastChar == ',') && firstChar == '.')
                throw new InvalidOperationException("Syntax would not be valid");
        }

        void ValidateWord(string word)
        {
            if (lastWord == "return" && word == "throw")
                throw new InvalidOperationException("Syntax would not be valid");
            if (lastWord == "throw" && word == ";")
                throw new InvalidOperationException("Syntax would not be valid");
            //if (lastChar == '.' && word == "this")
            //throw new InvalidOperationException("Syntax would not be valid");
        }

        public void Write(char value)
        {
            ValidateChar(value);
            internalWriter.Write(value);
            lastChar = value;
        }
        public void Write(string value)
        {
            if (value.Length == 0)
                return;
            ValidateChar(value[0]);
            ValidateWord(value.Trim().Split([' '], StringSplitOptions.RemoveEmptyEntries).FirstOrDefault());
            internalWriter.Write(value);
            var trimmedValue = value.Trim();
            if (trimmedValue.Length > 0)
            {
                lastChar = trimmedValue[trimmedValue.Length - 1];
                lastWord = trimmedValue.Split([' '], StringSplitOptions.RemoveEmptyEntries).LastOrDefault();
            }
        }

        public bool StartsWith(string value)
        {
            return internalWriter.ToString().TrimStart().StartsWith(value);
        }

        public override string ToString()
        {
            return internalWriter.ToString();
        }
    }
}