using tbank_back_web.Core.Data_Entities.Business;
using tbank_back_web.Infrastructure.DbContext;

public class NutrientsSummarizerService
{
	ApplicationDbContext db;
	public NutrientsSummarizerService(ApplicationDbContext db)
	{
		this.db = db;
	}

	public IQueryable<IngredientEntity> GetIngredientEntities(ReceiptEntity receiptEntity)
	{
		return db.Ingredients.Where(e => receiptEntity.IngredientsAmount.Keys.Contains(e.Title));
	}


	public float GetProteinPercentage(IQueryable<IngredientEntity> ingredients)
	{
		float allProtein = ingredients.Sum(e => e.Protein).Value;
		return allProtein / GetAllGramms(ingredients);
	}

	public float GetFatPercentage(IQueryable<IngredientEntity> ingredients) { 
		return ingredients.Sum(e => e.Fat).Value / GetAllGramms(ingredients);
	}
	public float GetCarbsPercentage(IQueryable<IngredientEntity> ingredients)
	{
		return ingredients.Sum(e => e.Carbs).Value / GetAllGramms(ingredients);
	}
	public int GetTotalKcal(IQueryable<IngredientEntity> ingredients) { 
		return ingredients.Sum(e => e.Kcal).Value;
	}
	public float GetAllGramms(IQueryable<IngredientEntity> ingredients) { 
		return ingredients.Sum(e => e.Kcal + e.Carbs + e.Fat + e.Protein).Value;
	}

	public float GetFatSumm(ReceiptEntity receipt)
	=>		GetIngredientEntities(receipt).Sum(e => e.Fat).Value;
	public int GetKcalSumm(ReceiptEntity receipt) => GetIngredientEntities(receipt).Sum(e => e.Kcal).Value;
	public float GetProteinSumm(ReceiptEntity receipt) => GetIngredientEntities(receipt).Sum(e => e.Protein).Value;
	public float GetCarbsSumm(ReceiptEntity receipt) => GetIngredientEntities(receipt).Sum(e => e.Carbs).Value;


}