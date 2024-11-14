/// <summary>
/// MinRating: 7
/// MaxRating: 10	
/// Models: [a,b,c]
/// </summary>

public IQueryable<RouteNode> GetFilteredNodesExcludingOnesFromOrders()
{
	var entities = GetAsQueryable();
	const string nodeFromOrderCreatorUserName = "spider";
	entities = entities.Where(e => e.UpdatedBy != nodeFromOrderCreatorUserName);
	return entities;
}



/// <summary>
/// Name : SetTaxWeight
/// MinRating: 7
/// MaxRating: 10	
/// </summary>
public static void SetTaxWeight(SearchCriterionCalculationDTO criterion, IReadOnlyCollection<CalculationDTO> calculations)
{
	foreach (var calculation in calculations)
	{
		SetTaxWeight(criterion, calculation);
	}
}


/// <summary>
/// Name : NotImplementedException
/// MinRating: 1
/// HumanReview: [not implemented]
/// </summary>
public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
{
	throw new NotImplementedException();
}



/// <summary>
/// Name : DoubleOrCheck
/// MinRating: 3
/// MaxRating: 8
/// HumanReview: [redundant]
/// </summary>
private static bool HasContent(OrderLine ol) =>
	!string.IsNullOrWhiteSpace(ol.MaterialName) ||
	!string.IsNullOrWhiteSpace(ol.MaterialName) ||
	(ol.MaterialQuantity.HasValue && ol.MaterialQuantity.Value > 0);



/// <summary>
/// Name : ThreadSleep
/// MinRating: 3
/// MaxRating: 5
/// HumanReview: [blocking operation]
/// </summary>

protected virtual void SetProgressMonitorMessage(string identifier, int percent, string message,
	bool warning = false)
{
	new ProgressMonitorService().SetProgressMonitorMessage(identifier, percent, message, warning);
	Thread.Sleep(1000);
}
