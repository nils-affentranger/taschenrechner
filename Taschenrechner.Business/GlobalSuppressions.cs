// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Major Code Smell", "S3928:Parameter names used into ArgumentException constructors should match an existing one ", Justification = "The parameter name 'token.Type' is correctly used to ensure clear debugging and error handling.", Scope = "member", Target = "~M:Taschenrechner.Business.Calculator.GetCurrentCalculation~System.String")]
[assembly: SuppressMessage("Major Code Smell", "S2589:Boolean expressions should not be gratuitous", Justification = "The boolean expression is necessary to prevent runtime errors and ensure correct logic.", Scope = "member", Target = "~M:Taschenrechner.Business.Calculator.AddCharacter(System.String)~System.Boolean")]