﻿using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Taschenrechner.WinForms {

    internal class Calculator {
        private List<Token> currentCalculation;

        public bool AddCharacter(string character) {
            

            if (IsValidCharacter(character)) {
                if (IsOperator(character)) {
                    if (currentCalculation.Count > 0 && currentCalculation[currentCalculation.Count - 1].Type == Token.TokenType.Operator) {
                        currentCalculation[currentCalculation.Count - 1] = new Token(character);
                    }
                    else {
                        currentCalculation.Add(new Token(character));
                    }
                }
                else if (double.TryParse(character, NumberStyles.Any, CultureInfo.InvariantCulture, out double number)) {
                    if (currentCalculation.Count > 0 && currentCalculation[currentCalculation.Count - 1].Type == Token.TokenType.Number) {
                        double newValue = currentCalculation[currentCalculation.Count - 1].Number * 10 + number;
                        currentCalculation[currentCalculation.Count - 1] = new Token(newValue);
                    }
                    else {
                        currentCalculation.Add(new Token(number));
                    }
                }
                return true;
            }
            return false;
        }

        public Calculator() {
            currentCalculation = new List<Token>();
        }
        public void Clear() {
            currentCalculation.Clear();
        }

        public double Evaluate() {
            string postfix = ConvertToPostfix(currentCalculation);
            return EvaluatePostfix(postfix);
        }

        public string GetCurrentCalculation() {
            List<string> parts = new List<string>();
            foreach (var token in currentCalculation) {
                parts.Add(token.ToString());
            }
            return string.Join(" ", parts);
        }

        private bool IsValidCharacter(string character) {
            return !string.IsNullOrEmpty(character);
        }

        private bool IsOperator(string character) {
            return character == "+" || character == "-" || character == "*" || character == "/";
        }

        private int GetPrecedence(string op) {
            return op == "+" || op == "-" ? 1 : 2;
        }

        private string ConvertToPostfix(List<Token> infixTokens) {
            Stack<string> stack = new Stack<string>();
            List<string> output = new List<string>();

            foreach (var token in infixTokens) {
                if (token.Type == Token.TokenType.Number) {
                    output.Add(token.Number.ToString(CultureInfo.InvariantCulture));
                }
                else if (token.Type == Token.TokenType.Operator) {
                    while (stack.Count > 0 && IsOperator(stack.Peek()) && GetPrecedence(token.Operator) <= GetPrecedence(stack.Peek())) {
                        output.Add(stack.Pop());
                    }
                    stack.Push(token.Operator);
                }
            }

            while (stack.Count > 0) {
                output.Add(stack.Pop());
            }

            return string.Join(" ", output);
        }

        private double EvaluatePostfix(string postfix) {
            Stack<double> stack = new Stack<double>();
            string[] tokens = postfix.Split(' ');

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
                default:
                    throw new InvalidOperationException("Invalid operator");
            };
        }
    }
}