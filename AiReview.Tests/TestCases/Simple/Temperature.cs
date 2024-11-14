/// <summary>
/// MinRating: 6
/// </summary>


private async Task<Temperature?> GetDeviceTemperature(PatDevice device)
{
	var isTemperatureAvailable =
		driverManager.TryLookupCapability<ITemperatureCapability>(device, out var provider);

	if (!isTemperatureAvailable || provider == null) return default;

	try
	{
		return await provider.GetCurrentTemperature(device);
	}
	catch (Exception e)
	{
		log.Error(e, $"Couldn't receive temperature from device: {device}.");
		return default;
	}
}