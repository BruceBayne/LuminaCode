/// <summary>
/// MinRating: 7
//	MaxRating: 10
//	ExpectedOutput: "ABC"
/// </summary>


private void Setup_AdditionalLabel_AdditionalLabel()
{
	CreateMap<AdditionalLabel, AdditionalLabel>()
		.ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(x => DateTime.Now))
		.ForMember(dest => dest.UpdatedDate, opt => opt.MapFrom(x => DateTime.Now))
		.ForMember(dest => dest.ID, opt => opt.Ignore())
		.ForMember(dest => dest.Additional, opt => opt.Ignore())
		.ForMember(dest => dest.Label, opt => opt.Ignore());
}