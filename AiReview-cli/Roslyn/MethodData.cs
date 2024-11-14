using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace LuminaCode_cli.Roslyn;


public enum MethodType
{ }



public sealed class MethodData
{
	public string ToHash() => $"{CRC32.Compute(ToAnalyze):X}";

	public ClassDeclarationSyntax ClassDeclaration { get; init; }
	public MethodDeclarationSyntax MethodDeclaration { get; init; }
	public string ClassName { get; init; }
	public string MethodName { get; init; }
	public string ReturnType { get; init; }
	public string Modifiers { get; init; }
	public string FullMethodSignature { get; init; }
	public string SourceFilePath { get; init; }
	public string FullMethodText { get; init; }
	public string FullClassText { get; init; }

	
	public string ToAnalyze => string.IsNullOrEmpty(FullMethodText) ? FullClassText : FullMethodText;

	public string EntityType => string.IsNullOrEmpty(FullMethodText) ? "class" : "method";
	public string EntityName => EntityType == "class" ? ClassName : MethodName;
	public string NavPath => $"{Path.GetFileName(SourceFilePath)}/{EntityType}/{EntityName}";
}