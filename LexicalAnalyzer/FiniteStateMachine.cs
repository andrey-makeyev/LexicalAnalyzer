using System;
using System.Collections.Generic;
using System.IO;

namespace LexicalAnalyzer
{
    /// <summary>
    /// Получение Id новой лексемы, заданной ее типом. Добавление новой лексемы в таблицу лексем.
    /// </summary>
    public class FiniteStateMachine
    {
        private string lexemeCashingBuffer = "";
        private char currentChar;
        private StringReader currentStringReader;
        private List<string> currentKeywords;
        private List<string> currentDelimiters;
        private enum States { START, DIGIT, LITERAL, DELIMITER, FINISH, IDENTIFIER, ERROR, DOUBLE_DELIMITER_ASSIGN_SYMBOL, DOUBLE_DELIMITER_NOT_EQUAL_SYMBOL, DOUBLE_DELIMITER_RANGE_SYMBOL }
        private States currentState;
        public List<string> IdentifiersTable { get; private set; }
        public List<string> LiteralsTable { get; private set; }
        public List<string> KeywordsTable { get; private set; }
        public List<Lexeme> Lexemes { get; private set; }
        public string Message { get; private set; }
        public FiniteStateMachine(List<string> keywords, List<string> delimiters)
        {
            currentKeywords = keywords;
            currentDelimiters = delimiters;
            KeywordsTable = new List<string>();
            KeywordsTable.AddRange(currentKeywords);
            KeywordsTable.AddRange(currentDelimiters);
            Lexemes = new List<Lexeme>();
            IdentifiersTable = new List<string>();
            LiteralsTable = new List<string>();
        }
        private void Log(string message) => Message += message;
        private bool IsKeyword(string buffer) => KeywordsTable.Contains(buffer);
        private void ClearBuffer() => lexemeCashingBuffer = "";
        private void AddBuffer(char symbol) => lexemeCashingBuffer += symbol;
        private void GetNextChar()
        {
            int next = currentStringReader.Read();
            if (next != -1)
            {
                currentChar = (char)next;
            }
            else
            {
                currentState = States.FINISH;
                Log("Program finish");
            }
        }
        private int GetLexemeId(string buffer, LexemeEnumeration type)
        {
            if (type == LexicalAnalyzer.LexemeEnumeration.KEY)
            {
                return KeywordsTable.IndexOf(buffer);
            }
            else
            {
                var table = type == LexicalAnalyzer.LexemeEnumeration.IDN ? IdentifiersTable : LiteralsTable;
                if (table.IndexOf(buffer) == -1)
                {
                    table.Add(buffer);
                }
                return table.IndexOf(buffer);
            }
        }
        private void AddLexem(string buffer, LexemeEnumeration type)
        {
            var id = GetLexemeId(buffer, type);
            var lex = new Lexeme(id, type, buffer);
            Lexemes.Add(lex);
        }
        public void Analyze(string text)
        {
            Log("Initializing..");
            currentStringReader = new StringReader(text);
            while (currentState != States.FINISH)
            {
                switch (currentState)
                {
                    case States.START:
                        CursorOnStart();
                        break;
                    case States.LITERAL:
                        CursorOnLiteral();
                        break;
                    case States.IDENTIFIER:
                        CursorOnId();
                        break;
                    case States.DIGIT:
                        CursorOnDigit();
                        break;
                    case States.DELIMITER:
                        CursorOnDelimiter();
                        break;
                    case States.DOUBLE_DELIMITER_ASSIGN_SYMBOL:
                        CursorOnDoubleDelimiterAssignSymbol();
                        break;
                    case States.DOUBLE_DELIMITER_NOT_EQUAL_SYMBOL:
                        CursorOnDoubleDelimiterNotEqualSymbol();
                        break;
                    case States.DOUBLE_DELIMITER_RANGE_SYMBOL:
                        CursorOnDoubleDelimiterRangeSymbol();
                        break;
                    case States.ERROR:
                        CursorOnError();
                        break;
                }
            }
        }
        private void CursorOnError()
        {
            currentState = States.FINISH;
            Log("Program error...");
            Console.WriteLine("\n -------------Character not recognized!-------------\n");
        }
        private void CursorOnDoubleDelimiterNotEqualSymbol()
        {
            if (currentChar == '>')
            {
                AddBuffer(currentChar);
                AddLexem(lexemeCashingBuffer, LexicalAnalyzer.LexemeEnumeration.KEY);
                ClearBuffer();
                GetNextChar();
            }
            else
            {
                AddLexem(lexemeCashingBuffer, LexicalAnalyzer.LexemeEnumeration.KEY);
            }
            currentState = States.START;
        }
        private void CursorOnDoubleDelimiterAssignSymbol()
        {
            if (currentChar == '=')
            {
                AddBuffer(currentChar);
                AddLexem(lexemeCashingBuffer, LexicalAnalyzer.LexemeEnumeration.KEY);
                ClearBuffer();
                GetNextChar();
            }
            else
            {
                AddLexem(lexemeCashingBuffer, LexicalAnalyzer.LexemeEnumeration.KEY);
            }
            currentState = States.START;
        }
        private void CursorOnDoubleDelimiterRangeSymbol()
        {
            if (currentChar == '.')
            {
                AddBuffer(currentChar);
                AddLexem(lexemeCashingBuffer, LexicalAnalyzer.LexemeEnumeration.KEY);
                ClearBuffer();
                GetNextChar();
            }
            else
            {
                AddLexem(lexemeCashingBuffer, LexicalAnalyzer.LexemeEnumeration.KEY);
            }
            currentState = States.START;
        }
        private void CursorOnDelimiter()
        {
            ClearBuffer();
            AddBuffer(currentChar);

            if (GetLexemeId(lexemeCashingBuffer, LexicalAnalyzer.LexemeEnumeration.KEY) != -1)
            {
                AddLexem(lexemeCashingBuffer, LexicalAnalyzer.LexemeEnumeration.KEY);
                currentState = States.START;
                GetNextChar();
            }
            else
            {
                currentState = States.ERROR;
            }
        }
        private void CursorOnDigit()
        {
            if (char.IsDigit(currentChar))
            {
                AddBuffer(currentChar);
                GetNextChar();
            }
            else
            {
                AddLexem(lexemeCashingBuffer, LexicalAnalyzer.LexemeEnumeration.LIT);
                currentState = States.START;
            }
        }
        private void CursorOnId()
        {
            if (char.IsLetterOrDigit(currentChar))
            {
                AddBuffer(currentChar);
                GetNextChar();
            }
            else
            {
                var type = IsKeyword(lexemeCashingBuffer) ? LexicalAnalyzer.LexemeEnumeration.KEY : LexicalAnalyzer.LexemeEnumeration.IDN;
                AddLexem(lexemeCashingBuffer, type);
                currentState = States.START;
            }
        }
        private void CursorOnLiteral()
        {
            if (currentChar == '\'')
            {
                if (lexemeCashingBuffer.Length == 0)
                {
                    currentState = States.ERROR;
                }
                else
                {
                    AddLexem(lexemeCashingBuffer, LexicalAnalyzer.LexemeEnumeration.LIT);
                    currentState = States.START;
                    GetNextChar();
                }
            }
            else
            {
                AddBuffer(currentChar);
                GetNextChar();
            }
        }
        private void CursorOnStart()
        {
            ClearBuffer();
            if (currentChar == ' ' || currentChar == '\n' || currentChar == '\t' || currentChar == '\0' || currentChar == '\r')
                GetNextChar();
            else if (char.IsLetter(currentChar))
            {
                AddBuffer(currentChar);
                currentState = States.IDENTIFIER;
                GetNextChar();
            }
            else if (char.IsDigit(currentChar))
            {
                AddBuffer(currentChar);
                GetNextChar();
                currentState = States.DIGIT;
            }
            else if (currentChar == ':')
            {
                currentState = States.DOUBLE_DELIMITER_ASSIGN_SYMBOL;
                AddBuffer(currentChar);
                GetNextChar();
            }
            else if (currentChar == '<')
            {
                currentState = States.DOUBLE_DELIMITER_NOT_EQUAL_SYMBOL;
                AddBuffer(currentChar);
                GetNextChar();
            }
            else if (currentChar == '.')
            {
                currentState = States.DOUBLE_DELIMITER_RANGE_SYMBOL;
                AddBuffer(currentChar);
                GetNextChar();
            }
            else if (currentChar == '\'')
            {
                currentState = States.LITERAL;
                GetNextChar();
            }
            else
            {
                currentState = States.DELIMITER;
            }
        }
    }
}