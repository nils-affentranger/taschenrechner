using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Taschenrechner.Business {

    public class Calculator {
        private bool lastActionWasEvaluation;
        private readonly List<Token> currentCalculation;

        public Calculator() {
            currentCalculation = new List<Token>();
        }

        public bool AddCharacter(string character) {
            if (IsValidCharacter(character)) {
                if (lastActionWasEvaluation) {
                    if (IsOperator(character) || IsParenthesis(character)) {
                        lastActionWasEvaluation = false;
                    }
                    else {
                        currentCalculation.Clear();
                        lastActionWasEvaluation = false;
                    }
                }
                if (IsOperator(character)) {
                    if (!currentCalculation.Any()) {
                        return false;
                    }
                    if (currentCalculation.Count > 0 && currentCalculation[currentCalculation.Count - 1].Type == Token.TokenType.Operator) {
                        currentCalculation[currentCalculation.Count - 1] = new Token(character, true);
                    }
                    else {
                        currentCalculation.Add(new Token(character, true));
                    }
                }
                else if (IsParenthesis(character)) {
                    var lastToken = currentCalculation[currentCalculation.Count - 1];
                    if (lastToken.Type == Token.TokenType.Number && character == "(") {
                        currentCalculation.Add(new Token("*", true));
                    }
                    currentCalculation.Add(new Token(character, false, true));
                }
                else {
                    if (currentCalculation.Count > 0 && currentCalculation[currentCalculation.Count - 1].Type == Token.TokenType.Number) {
                        var lastToken = currentCalculation[currentCalculation.Count - 1];
                        var newNumberString = lastToken.NumberString + character;
                        currentCalculation[currentCalculation.Count - 1] = new Token(newNumberString);
                    }
                    else {
                        currentCalculation.Add(new Token(character));
                    }
                }
                return true;
            }
            return false;
        }

        public bool AddDecimalPoint() {
            if (currentCalculation.Count > 0 && currentCalculation[currentCalculation.Count - 1].Type == Token.TokenType.Number) {
                var lastToken = currentCalculation[currentCalculation.Count - 1];
                if (!lastToken.NumberString.Contains(".")) {
                    var newNumberString = lastToken.NumberString + ".";
                    currentCalculation[currentCalculation.Count - 1] = new Token(newNumberString);
                    return true;
                }
            }
            else {
                currentCalculation.Add(new Token("0."));
                return true;
            }
            return false;
        }

        public bool Backspace() {
            if (currentCalculation.Any() && !lastActionWasEvaluation) {
                if (GetCurrentCalculation().Length >= 1) {
                    var lastToken = currentCalculation[currentCalculation.Count - 1];
                    if (lastToken.Type == Token.TokenType.Number) {
                        string invariantNumberString = double.Parse(lastToken.NumberString, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture);
                        if (invariantNumberString.Length == 1) {
                            currentCalculation.Remove(lastToken);
                        }
                        else {
                            var newNumberString = lastToken.NumberString.Remove(lastToken.NumberString.Length - 1);
                            currentCalculation[currentCalculation.Count - 1] = new Token(newNumberString);
                        }
                    }
                    else if (lastToken.Type == Token.TokenType.Operator) {
                        currentCalculation.Remove(lastToken);
                    }
                    else if (lastToken.Type == Token.TokenType.Parenthesis) {
                        currentCalculation.Remove(lastToken);
                    }
                    else throw new Exception();
                }
                else {
                    CE();
                }
                return true;
            }
            else {
                Clear();
                return false;
            }
        }

        public bool CE() {
            if (currentCalculation.Any()) {
                currentCalculation.RemoveAt(currentCalculation.Count - 1);
                return true;
            }
            else { return false; }
        }

        public void Clear() {
            currentCalculation.Clear();
        }

        public string Evaluate() {
            string postfix = ConvertToPostfix(currentCalculation);
            double result = EvaluatePostfix(postfix);
            Clear();
            currentCalculation.Add(new Token(result));
            lastActionWasEvaluation = true;
            return FormatNumber(result);
        }

        public string GetCurrentCalculation() {
            StringBuilder sb = new StringBuilder();
            foreach (var token in currentCalculation) {
                switch (token.Type) {
                    case Token.TokenType.Number:
                        sb.Append(token.NumberString); break;
                    case Token.TokenType.Operator:
                        sb.Append(token.Operator); break;
                    case Token.TokenType.Parenthesis:
                        sb.Append(token.Parenthesis); break;
                    default:
                        throw new ArgumentOutOfRangeException();
                        ;
                }
            }
            return sb.ToString();
        }

        public bool ToggleSign() {
            if (currentCalculation.Count > 0 && currentCalculation[currentCalculation.Count - 1].Type == Token.TokenType.Number) {
                var lastNumber = currentCalculation[currentCalculation.Count - 1].Number;
                currentCalculation[currentCalculation.Count - 1] = new Token(-lastNumber);
                return true;
            }
            return false;
        }

        private double ApplyOperator(string op, double left, double right) {
            switch (op) {
                case "+":
                    return left + right;

                case "-":
                    return left - right;

                case "*":
                    return left * right;

                case "/":
                    return left / right;

                case "^":
                    return (double)Math.Pow((double)left, (double)right);

                default:
                    throw new InvalidOperationException("Invalid operator");
            }
        }

        public string ConvertToPostfix(List<Token> infixTokens) {
            Stack<string> stack = new Stack<string>();
            List<string> output = new List<string>();

            foreach (var token in infixTokens) {
                if (token.Type == Token.TokenType.Number) {
                    output.Add(token.NumberString);
                }
                else if (token.Type == Token.TokenType.Operator) {
                    while (stack.Count > 0 && IsOperator(stack.Peek()) && GetPrecedence(token.Operator) <= GetPrecedence(stack.Peek())) {
                        output.Add(stack.Pop());
                    }
                    stack.Push(token.Operator);
                }
                else if (token.Type == Token.TokenType.Parenthesis) {
                    if (token.Parenthesis == "(") {
                        stack.Push(token.Parenthesis);
                    }
                    else {
                        while (stack.Count > 0 && stack.Peek() != "(") {
                            output.Add(stack.Pop());
                        }
                        stack.Pop();
                    }
                }
            }

            while (stack.Count > 0) {
                output.Add(stack.Pop());
            }

            return string.Join(" ", output);
        }

        private double EvaluatePostfix(string postfix) {
            Stack<double> stack = new Stack<double>();
            string[] tokens = postfix.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string token in tokens) {
                if (double.TryParse(token, NumberStyles.Any, CultureInfo.InvariantCulture, out double number)) {
                    stack.Push(number);
                }
                else if (IsOperator(token)) {
                    double right = stack.Pop();
                    double left = stack.Pop();
                    stack.Push(ApplyOperator(token, left, right));
                }
            }

            return stack.Pop();
        }

        private string FormatNumber(double number) {
            bool useScientific = Math.Abs(number) >= 1e15 || Math.Abs(number) < 1e-2;
            string format = useScientific ? "E" : "N";
            int decimalPlaces = GetDecimalPlaces(number);
            var nfi = new NumberFormatInfo { NumberGroupSeparator = "'", NumberDecimalDigits = decimalPlaces };
            string formattedNumber = number.ToString(format, nfi);
            return formattedNumber;
        }

        private static int GetDecimalPlaces(double number) {
            string str = number.ToString("G", CultureInfo.InvariantCulture);
            int index = str.IndexOf('.');
            if (index == -1) {
                return 0;
            }
            else return str.Length - index - 1;
        }

        private int GetPrecedence(string op) {
            return op == "+" || op == "-" ? 1 : 2;
        }

        private bool IsOperator(string character) {
            return character == "+" || character == "-" || character == "*" || character == "/" || character == "^";
        }

        private bool IsParenthesis(string character) {
            return character == "(" || character == ")";
        }

        private bool IsValidCharacter(string character) {
            return !string.IsNullOrEmpty(character);
        }
    }
}