/// <summary>
/// Name : Shared Variable Race Condition
/// MinRating: 1
//	MaxRating: 5
/// </summary>

public void Do()
{

	int number;

	Task t1 = Task.Run(() => {
		for (int j = 0; j < 1_000_000; j++)
		{
			number++;
		}

	});
	Task t2 = Task.Run(() =>
	{
		for (int j = 0; j < 1_000_000; j++)
		{
			number++;
		}
	});

	Task t3 = Task.Run(() =>
	{
		for (int j = 0; j < 1_000_000; j++)
		{
			number++;
		}
	});
	Task.WaitAll(t1, t2, t3);
	Console.WriteLine(number);
}


