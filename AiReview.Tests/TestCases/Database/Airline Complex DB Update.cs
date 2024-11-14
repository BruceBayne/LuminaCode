/// <summary>
/// MinRating: 0
///	MaxRating: 10
/// Name: Pace SetAirline Data
/// </summary>

public async Task<AirlineInformationDto> SetAirlineData(SetAirlineDataDto setDataDto)
{
	using var transaction = m_context.StartTransactionScope();

	Airline airline;
	if (setDataDto.IsNew)
	{
		airline = m_mapper.Map<Airline>(setDataDto);
		m_context.Airlines.Add(airline);
	}
	else
	{
		airline = m_context.Airlines.FirstOrDefault(x => x.Code == setDataDto.Code);
		if (airline == null)
			throw new ArgumentOutOfRangeException(nameof(setDataDto.Code));

		m_mapper.Map(setDataDto, airline);
	}

	var weWouldLikeToProcessPictures = setDataDto.Pictures != null;

	if (weWouldLikeToProcessPictures)
	{
		var ids = setDataDto.Pictures.Select(x => x.Value).ToList();

		var weHavePictures = setDataDto.Pictures.Any();

		if (weHavePictures)
		{
			//Check if we're trying to assign images already assigned to other airline
			var changingRootNode = await m_context.AirlinePictures.GetByIds(ids)
				.AnyAsync(x => x.AirlineCode != null && x.AirlineCode != airline.Code);

			if (changingRootNode)
				throw new ArgumentOutOfRangeException(
					nameof(SetAirlineDataDto.Pictures),
					"Some picture already assigned to other airline");
		}

		if (!setDataDto.IsNew)
		{
			//UnAssign existing pictures/usages for current airline
			await m_context.Database.ExecuteSqlInterpolatedAsync(
				@$"UPDATE ""oa"".""AirlinePictures"" SET ""AirlineCode"" = NULL,""Usage""= NULL WHERE ""AirlineCode""= {setDataDto.Code}");
		}

		if (weHavePictures)
		{
			//Direct ToDictionaryAsync call will be executed locally with downloading all Picture Data.
			var tokensWithFileName = (await m_context.AirlinePictures.GetByIds(ids)
				.Select(x => new { x.Id, x.XMin, x.FileName })
				.ToListAsync()).ToDictionary(arg => arg.Id, arg => new { arg.XMin, arg.FileName });

			//Map pictures
			foreach (var picture in setDataDto.Pictures)
			{
				var mapped = m_mapper.Map<AirlinePicture>(picture);
				mapped.XMin = tokensWithFileName[picture.Value].XMin;

				//We want just to include that filename as result, we don't want set it to Modified.
				mapped.FileName = tokensWithFileName[picture.Value].FileName;
				mapped.Airline = airline;
				mapped.Id = picture.Value;
				m_context.AirlinePictures.Attach(mapped);
				m_context.Entry(mapped).Property(x => x.Usage).IsModified = true;
				m_context.Entry(mapped).Property(x => x.AirlineCode).IsModified = true;
			}
		}
	}

	airline.LastModified = m_context.UtcNow;
	await m_context.SaveChangesAsync();
	transaction.Complete();
	var airlineInformationDto = m_mapper.Map<AirlineInformationDto>(airline);
	return airlineInformationDto;
}