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
			{
				query = query.Where(e => e.TotalKcal / (e.TotalKcal + e.TotalProtein +e.TotalCarbs +e.TotalFat) >= filters.kcalMin.Value);
			}
			if (filters.kcalMax.HasValue)
			{
				query = query.Where(e => e.TotalKcal / (e.TotalKcal + e.TotalProtein + e.TotalCarbs + e.TotalFat) <= filters.kcalMax.Value);
			}
			// Фильтрация по белкам
			if (filters.proteinPercMin.HasValue)
			{
				query = query.Where(e => e.TotalProtein / (e.TotalKcal + e.TotalProtein + e.TotalCarbs + e.TotalFat) >= filters.proteinPercMin.Value);
			}
			if (filters.proteinPercMax.HasValue)
			{
				query = query.Where(e => e.TotalProtein / (e.TotalKcal + e.TotalProtein + e.TotalCarbs + e.TotalFat) <= filters.proteinPercMax.Value);
			}
			// Фильтрация по жирам
			if (filters.fatPercMin.HasValue)
			{
				query = query.Where(e => e.TotalFat / (e.TotalKcal + e.TotalProtein + e.TotalCarbs + e.TotalFat) >= filters.fatPercMin.Value);
			}
			if (filters.fatPercMax.HasValue)
			{
				query = query.Where(e => e.TotalFat / (e.TotalKcal + e.TotalProtein + e.TotalCarbs + e.TotalFat) <= filters.fatPercMax.Value);
			}
			// Поиск по тексту
			if (!string.IsNullOrWhiteSpace(filters.search))
			{
				string lowQuery = filters.search.ToLower();
				query = query.Where(e =>
					e.Title.ToLower().Contains(lowQuery) || 
					e.Instructions.ToLower().Contains(lowQuery));
			}

			return query;
		}
	}
}
