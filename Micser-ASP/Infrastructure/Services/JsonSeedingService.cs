using Microsoft.Extensions.Hosting;
using System.Text.Json;
using tbank_back_web.Core.Data_Entities.Business;

public class JsonSeedingService
{
	private readonly IWebHostEnvironment _environment;

	public JsonSeedingService(IWebHostEnvironment environment)
	{
		_environment = environment;
	}

	public async Task<List<IngredientEntity>> ReadIngredientsFromJsonAsync()
	{
		var filePath = Path.Combine(_environment.ContentRootPath, "ingseeding.json");

		if (!File.Exists(filePath))
		{
			throw new FileNotFoundException($"Ingredients JSON file not found: {filePath}");
		}

		var json = await File.ReadAllTextAsync(filePath);
		var options = new JsonSerializerOptions
		{
			PropertyNameCaseInsensitive = true,
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase
		};

		var ingredients = JsonSerializer.Deserialize<List<IngredientEntity>>(json, options);
		return ingredients ?? new List<IngredientEntity>();
	}

	public async Task<List<ReceiptEntity>> ReadReceiptsFromJsonAsync()
	{
		var filePath = Path.Combine(_environment.ContentRootPath, "rseeding.json");

		if (!File.Exists(filePath))
		{
			throw new FileNotFoundException($"Receipts JSON file not found: {filePath}");
		}

		var json = await File.ReadAllTextAsync(filePath);
		var options = new JsonSerializerOptions
		{
			PropertyNameCaseInsensitive = true,
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase
		};

		var receipts = JsonSerializer.Deserialize<List<ReceiptEntity>>(json, options);
		return receipts ?? new List<ReceiptEntity>();
	}

	// Комбинированный метод для чтения обоих файлов
	public async Task<(List<IngredientEntity> ingredients, List<ReceiptEntity> receipts)> ReadAllDataAsync()
	{
		var ingredientsTask = ReadIngredientsFromJsonAsync();
		var receiptsTask = ReadReceiptsFromJsonAsync();

		await Task.WhenAll(ingredientsTask, receiptsTask);

		return (await ingredientsTask, await receiptsTask);
	}
}