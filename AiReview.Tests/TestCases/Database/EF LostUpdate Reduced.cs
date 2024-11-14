/// <summary>
/// MinRating: 1
/// MaxRating: 6
/// Name: EF Lost update simple
/// HumanReview: [race condition]
/// ReviewPrompt: database
/// </summary>

private static void VisitWebSite()
{
	var db = new MyTubeContext();

	var entity = db.Visits
		.First(x => x.Url == "http://example.com");

	entity.VisitCount++;

	db.SaveChanges();
}



/// <summary>
/// MinRating: 1
/// MaxRating: 6
/// Name : Raw Sql XLOCK Lost update case
/// ReviewPrompt: database
/// </summary>

private static async Task VisitWebSite()
{
	using var transaction = await context.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted);

	var visit = await context.Visits
		.FromSqlRaw("SELECT * FROM [Visits] WITH (XLOCK) WHERE Url = @url",
			new SqlParameter("@url", "http://example.com"))
		.SingleOrDefaultAsync();

	if (visit != null)
	{
		// Step 2: Increment the counter
		visit.VisitCount++;

		// Step 3: Update the row
		context.Visits.Update(visit);
		await context.SaveChangesAsync();
	}

	// Commit the transaction
	await transaction.CommitAsync();
}


/// <summary>
/// MaxRating: 6
/// Name : MongoDB Lost update
/// HumanReview: [lost update]
/// ReviewPrompt: database
/// </summary>
public async Task IncrementCounterAsync(ObjectId documentId)
{
	// Step 1: Read the current value
	var document = await _collection.Find(Builders<BsonDocument>.Filter.Eq("_id", documentId)).FirstOrDefaultAsync();

	if (document == null)
	{
		
		return;
	}

	var currentValue = document["counter"].AsInt32;

	// Step 2: Modify the value
	var newValue = currentValue + 1;


	// Step 3: Write the new value back
	var update = Builders<BsonDocument>.Update.Set("counter", newValue);
	await _collection.UpdateOneAsync(Builders<BsonDocument>.Filter.Eq("_id", documentId), update);


}



/// <summary>
/// MinRating: 5
/// Name : MongoDB Atomic update
/// HumanReview: [~NO~ race condition,~NO~ concurrency,~NO~ violaiton,~NO~ not atomic, ~NO~ lost update]
/// ReviewPrompt: database
/// </summary>
public async Task IncrementCounterAsync(ObjectId documentId)
{
	var update = Builders<BsonDocument>.Update.Inc("counter", 1);

	await _collection.UpdateOneAsync(
		Builders<BsonDocument>.Filter.Eq("_id", documentId),
		update);
}


/// <summary>
/// MinRating: 8
/// Name: EF Atomic update
/// HumanReview: [~NO~ race condition]
/// ReviewPrompt: database
/// </summary>

private static void VisitWebSite()
{
	using (var db = new MyTubeContext())
	{
		db.Visits.ExecuteUpdate(s => s.SetProperty(e => e.VisitCount, e => e.VisitCount + 1));
		db.SaveChanges();
	}
}
