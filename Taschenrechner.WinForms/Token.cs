using System.Globalization;
using System;

namespace Taschenrechner {

    public class Token {

        public enum TokenType {
            Number,
            Operator,
            Parenthesis
        }

        public TokenType Type { get; }
        public string NumberString { get; }

        public double Number {
            get {
                if (double.TryParse(NumberString, NumberStyles.Any, CultureInfo.InvariantCulture, out double result)) {
                    return result;
                }
                return 42;
            }
        }

        public string Operator { get; }
        public string Parenthesis { get; }

        public Token(double number) {
            Type = TokenType.Number;
            NumberString = Convert.ToString(number);
        }

        public Token(string numberString) {
            Type = TokenType.Number;
            NumberString = numberString;
        }

        public Token(string operatorSymbol, bool isOperator) {
            Type = TokenType.Operator;
            Operator = operatorSymbol;
        }

        public Token(string parenthesisSymbol, bool isParenthesis, bool isParenthesisToken) {
            Type = TokenType.Parenthesis;
            Parenthesis = parenthesisSymbol;
        }
    }
}