using tbank_back_web.Controllers.Finder.Models.Dto;

public class SearchRecipesRequestModel
{
	public int? kcalMax { get; set; }
	public int? kcalMin { get; set; }
	public float? proteinPercMax { get; set; }
	public float? proteinPercMin { get; set; }
	public float? fatPercMax { get; set; }
	public float? fatPercMin { get; set; }
	public string? search { get; set; }
}
