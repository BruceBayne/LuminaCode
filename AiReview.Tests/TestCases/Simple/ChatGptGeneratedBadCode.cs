/// <summary>
/// MinRating: 1
//	MaxRating: 5,
/// </summary>

public void PoorQualityCode()
{
	// Bad: Hardcoded values, no meaningful names
	int a = 10, b = 20, c = 30;

	// Bad: Magic numbers, no validation or explanation
	if (a + b == c)
	{
		// Bad: Randomly returns values without logic
		return a * b;
	}

	// Bad: Nested logic with side effects and no separation of concerns
	if (a > b)
	{
		Console.WriteLine("a is greater"); // Bad: Console output in a method, mixing logic with side effects
		a++; // Bad: Mutating variables directly without purpose
	}

	// Bad: No error handling or exception management
	int result = a / 0; // Bad: Division by zero, no try-catch, will throw exception

	// Bad: No meaningful method return or outcome
	return; // Bad: Unclear what this method is supposed to achieve
}
