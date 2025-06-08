namespace AiReview.Core.LLM;

public static class PromptDatabase
{
    public const string ReviewOutputPrompt = """
                                             Exclude any feedback if no problems are detected.
                                             Exclude any explanations.
                                             Provide tiny quality conclusion rank from 1 to 10 named rank where 10 means good quality.
                                             Force output as proper quoted JSON with mentioned rank,and issues array.
                                             Issues should contain their severity (from Critical to Low), brief description, line numbers as string, category.
                                             Nothing except JSON should be in output.
                                             """;

    public const string DefaultPrompt = """
                                        Act as an expert code reviewer.
                                        Review following C# code and identify issues in areas: 

                                        performance,
                                        I/O concurrency violations,
                                        race conditions,
                                        transaction isolation issues.
                                        Stay compact.
                                        No assumptions. No long explanations
                                        Issues should contain their severity (from Critical to Low), brief description with context, simple category, notes.

                                        Force Exclude issues from : 
                                        readability, blocking I/O usage,
                                        exception handling, unused parameters,
                                        missing usings, division by zero,
                                        static methods, magic numbers,
                                        missing documentation,naming conventions
                                        NullReferenceException,NotImplementedException
                                        incorrect method signatures,
                                        compilation errors, input validation.
                                        """;

    public const string GeneralReviewPrompt = """
                                              Act as an expert code reviewer
                                              Review following C# code

                                              Focus only on directly observable issues:
                                              concurrency, resource management, async usage, security, logic, API misuse, performance, events, serialization, and dynamic execution. 
                                              Identify risks like race conditions, deadlocks, leaks, unsafe patterns, or inefficient practices.
                                              Force exclude potential issues, focus only on real findings.

                                              Exclude issues in:
                                              syntax or compilation errors, input validation, comments in code,
                                              exception handling, unused parameters,
                                              missing usings, division by zero,
                                              static methods, magic numbers.

                                              """;


    public const string LostUpdatePrompt = """
                                           Act as an expert code reviewer
                                           Review following C# code.

                                           Focus only on issues in areas:
                                           read skew, write skew, lost update, deadlocks,
                                           Transaction Isolation Violations, DbContext missusage, 
                                           Entity Tracking Issues, cache staleness, race conditions, lost deletes.

                                           Consider current isolation level, safeguards like atomic operations or concurrency controls.
                                           """;

    public static string BetterNamingPrompt { get; set; } =
    //    "Suggest concise, meaningful .NET-style names for the given C# code snippet. The code may be partial or illustrative. Focus on purpose inference, not syntax validity. force output as a JSON array of name suggestions only.\n";
    "Suggest concise, meaningful names for the code (function or entity). Force output as JSON array only: ";
    //"Based on the following code context, suggest a couple compact, concise, meaningful function names (force output as json array, no extra information): ";
}