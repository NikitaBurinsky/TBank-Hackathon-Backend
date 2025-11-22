using Microsoft.AspNetCore.Mvc;
using System.Linq;
using tbank_back_web.Core.Data_Entities.Business;
using static tbank_back_web.Controllers.Finder.FinderController;

namespace tbank_back_web.Controllers.Finder.Filtration
{
	public static class ApplyFilterQueryableExtension
	{
		public static IQueryable<ReceiptEntity> ApplySearchFilters(
			this IQueryable<ReceiptEntity> query,
			SearchRecipesRequestModel filters,
			 NutrientsSummarizerService nutritionSummary)
		{
			if (filters == null) return query;
		
			// Фильтрация по калориям
			if (filters.kcalMin.HasValue)
				query = query.Where(e => nutritionSummary.GetTotalKcal(nutritionSummary.GetIngredientEntities(e)) >= filters.kcalMin.Value);

			if (filters.kcalMax.HasValue)
				query = query.Where(e => nutritionSummary.GetTotalKcal(nutritionSummary.GetIngredientEntities(e)) <= filters.kcalMax.Value);

			// Фильтрация по белкам
			if (filters.proteinPercMin.HasValue)
				query = query.Where(e => nutritionSummary.GetProteinPercentage(nutritionSummary.GetIngredientEntities(e)) >= filters.proteinPercMin.Value);

			if (filters.proteinPercMax.HasValue)
				query = query.Where(e => nutritionSummary.GetProteinPercentage(nutritionSummary.GetIngredientEntities(e)) <= filters.proteinPercMax.Value);

			// Фильтрация по жирам
			if (filters.fatPercMin.HasValue)
				query = query.Where(e => nutritionSummary.GetFatPercentage(nutritionSummary.GetIngredientEntities(e)) >= filters.fatPercMin.Value);

			if (filters.fatPercMax.HasValue)
				query = query.Where(e => nutritionSummary.GetFatPercentage(nutritionSummary.GetIngredientEntities(e)) <= filters.fatPercMax.Value);

			// Поиск по тексту
			if (!string.IsNullOrWhiteSpace(filters.search))
			{
				query = query.Where(e =>
					e.Title.Contains(filters.search) || 
					e.Instructions.Contains(filters.search));
			}

			return query;
		}
	}
}
