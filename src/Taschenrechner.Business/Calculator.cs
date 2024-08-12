﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Taschenrechner.WinForms {

    public class Calculator {
        private bool lastActionWasEvaluation;
        private readonly List<Token> currentCalculation;
        private readonly List<string> history = new List<string>(6);
        private string historyString;

        public string HistoryString {
            get {
                return string.Join("\r\n", history);
            }
        }

        public Calculator() {
            currentCalculation = new List<Token>();
        }

        public bool AddCharacter(string character) {
            if (!IsValidCharacter(character)) {
                return false;
            }

            if (lastActionWasEvaluation && !IsOperator(character)) {
                currentCalculation.Clear();
            }

            lastActionWasEvaluation = false;

            var lastToken = currentCalculation.LastOrDefault();

            if (IsOperator(character)) {
                if (lastToken == null || lastToken.Type == Token.TokenType.Operator) {
                    return false;
                }
                currentCalculation.Add(new Token(character, true));
                return true;
            }

            if (IsParenthesis(character)) {
                if (lastToken?.Type == Token.TokenType.Number && character == "(") {
                    currentCalculation.Add(new Token("*", true));
                }
                currentCalculation.Add(new Token("*", true));
                return true;
            }

            if (lastToken?.Type == Token.TokenType.Number) {
                currentCalculation[currentCalculation.Count - 1] = new Token(lastToken.NumberString + character);
            }
            else {
                currentCalculation.Add(new Token(character));
            }

            return true;
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
            if (!currentCalculation.Any() || lastActionWasEvaluation) {
                Clear();
                return false;
            }

            var lastToken = currentCalculation[currentCalculation.Count - 1];

            if (lastToken.Type == Token.TokenType.Number) {
                var newNumberString = lastToken.NumberString.Remove(lastToken.NumberString.Length - 1);
                if (string.IsNullOrEmpty(newNumberString)) {
                    currentCalculation.RemoveAt(currentCalculation.Count - 1);
                }
                else {
                    currentCalculation[currentCalculation.Count - 1] = new Token(newNumberString);
                }
            }
            else {
                currentCalculation.RemoveAt(currentCalculation.Count - 1);
            }

            return true;
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
            AppendHistory(FormatNumber(result));
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
                }
            }
            return sb.ToString();
        }

        public void AppendHistory(string result) {
            history.Insert(0, result);
            if (history.Count > 6) {
                history.RemoveAt(6);
            }
            historyString = string.Join("\r\n", history);
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
                    return Math.Pow(left, right);

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