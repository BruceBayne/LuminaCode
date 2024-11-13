namespace AiReview.Tests
{

	internal static class CodeCollections
	{

		public const string MinSample =
			"static void Main(string[] args, int a)\r\n {\r\n     Console.WriteLine(\"Hello, World!\");\r\n     Console.WriteLine(\"Hello, World!\");\r\n }\r\n";


		public const string RaceConditionSample = """

		                                          
		                                          
		                                          public void Do()
		                                          {
		                                          
		                                          int number;
		                                          
		                                          Task t1 = Task.Run(() => { 
		                                          for (int j = 0; j < 1_000_000; j++) { 
		                                          number++; }
		                                          
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
		                                          
		                                          """;	
		
		
		public const string EFLostUpdate = """

		                                          
		                                          private static void VisitWebSite()
		                                          {
		                                              var db = new MyTubeContext();
		                                          
		                                              var entity = db.Visits
		                                                  .First(x => x.Url == "http://example.com");
		                                          
		                                              entity.VisitCount++;
		                                          
		                                              db.SaveChanges();
		                                          }
		                                          
		                                          """;		
		
		public const string ChatGptGeneratedPoorCode = """

		                                          
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
		                                          
		                                          """;	
		
		
		
		public const string ClassBased_EFLostUpdate = """

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
		                                          
		                                          """;

	}
}