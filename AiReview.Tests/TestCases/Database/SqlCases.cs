/// <summary>
///	MaxRating: 5
/// HumanReview: [sql injection]
/// Name : SqlInjection
/// </summary>

public AuditLog GetByID(int id)
{
	var sql = new StringBuilder();
	sql.AppendLine(
		"SELECT ID, ApplicationID, AuditLogTypeID, AuditLogObjectTypeID, ObjectID, ObjectOrginalState, ObjectNewState, AdditionalData, CreatedBy, CreatedDate FROM dbo.AuditLog WHERE ID =" +
		id);

	AddSqlParameter("@id", id);

	var auditLogItem = GetBySqlQuery<AuditLog>(sql.ToString()).FirstOrDefault();

	return auditLogItem;
}