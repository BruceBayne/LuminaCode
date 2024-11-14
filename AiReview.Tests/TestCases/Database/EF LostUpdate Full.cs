/// <summary>
//	MaxRating: 5
/// ReviewPrompt: database
/// </summary>


public class UnitTest1
{
	private static void ResetDatabase()
	{
		using var db = new MyTubeContext();
		var list = db.Visits.ToList();
		foreach (var visit in list)
			visit.VisitCount = 0;
		db.SaveChanges();
	}

	public UnitTest1() => ResetDatabase();


	private static void VisitWebSite()
	{
		using var db = new MyTubeContext();

		using var tran = db.Database.BeginTransaction();

		var entity = db.Visits
			.First(x => x.Url == "http://example.com");
		entity.VisitCount++;

		db.SaveChanges();
		tran.Commit();
	}


	[Fact]
	public void Test1()
	{
		var t1 = Task.Run(() =>
		{
			for (var i = 0; i < 10; i++)
				VisitWebSite();
		});


		var t2 = Task.Run(() =>
		{
			for (var i = 0; i < 10; i++)
				VisitWebSite();
		});

		var t3 = Task.Run(() =>
		{
			for (var i = 0; i < 10; i++)
				VisitWebSite();
		});


		Task.WaitAll(t1, t2, t3);
		AssertCounterValid(30);
	}

	private static void AssertCounterValid(int expected)
	{
		using var db = new MyTubeContext();
		var entity = db.Visits.Single(x => x.Url == "http://example.com");
		entity.VisitCount.Should().Be(expected);
	}
}