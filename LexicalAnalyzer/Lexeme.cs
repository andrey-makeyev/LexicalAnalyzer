namespace LexicalAnalyzer
{
    public class Lexeme
    {
        public readonly LexemeEnumeration type;
        public readonly int index;
        public readonly string lexeme;
        public Lexeme(int index, LexemeEnumeration type, string lexeme)
        {
            this.type = type;
            this.index = index;
            this.lexeme = lexeme;
        }
    }
}