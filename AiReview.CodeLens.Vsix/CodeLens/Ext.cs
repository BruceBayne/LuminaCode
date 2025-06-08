using Microsoft.VisualStudio.Language.Intellisense;

namespace AiReview.CodeLens.Vsix.CodeLens;

internal static class Ext
{
    public static bool IsFunctionMethodOrClass(CodeElementKinds kind)
    {
        const CodeElementKinds targetKinds =
            CodeElementKinds.Function | CodeElementKinds.Method | CodeElementKinds.Class;

        // Perform a bitwise AND to check if the given kind matches any of the target kinds
        return (kind & targetKinds) != 0;
    }
}