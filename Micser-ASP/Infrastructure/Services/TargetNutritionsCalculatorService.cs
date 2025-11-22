using tbank_back_web.Core.Identity.User;

namespace tbank_back_web.Infrastructure.Services
{
	public static class NutritionCalculator
	{
		public class NutritionTargets
		{
			public int TargetKcal { get; set; }
			public float TargetProtein { get; set; }
			public float TargetFat { get; set; }
			public float TargetCarbs { get; set; }
		}

		public static NutritionTargets CalculateDailyNutrition(BaseApplicationUser user)
		{
			// Расчет базового метаболизма (BMR) по формуле Миффлина-Сан Жеора
			float bmr = CalculateBMR(user);

			// Учет уровня активности
			float activityMultiplier = GetActivityMultiplier(user.ActivityLevel);
			float tdee = bmr * activityMultiplier;

			// Округление до целых калорий
			int targetKcal = (int)Math.Round(tdee);

			// Расчет макронутриентов (стандартные рекомендации)
			float targetProtein = CalculateProtein((float)user.Weight, targetKcal);
			float targetFat = CalculateFat(targetKcal);
			float targetCarbs = CalculateCarbs(targetKcal, targetProtein, targetFat);

			return new NutritionTargets
			{
				TargetKcal = targetKcal,
				TargetProtein = (float)Math.Round(targetProtein, 1),
				TargetFat = (float)Math.Round(targetFat, 1),
				TargetCarbs = (float)Math.Round(targetCarbs, 1)
			};
		}

		private static float CalculateBMR(BaseApplicationUser user)
		{
			// Формула Миффлина-Сан Жеора
			if (user.Gender?.ToLower() == "male" || user.Gender?.ToLower() == "мужской")
			{
				return (10f * (float)user.Weight) + (6.25f * (float)user.Heigth) - (5f * (float)user.Age) + 5f;
			}
			else // female или по умолчанию
			{
				return (10f * (float)user.Weight) + (6.25f * (float)user.Heigth) - (5f * (float)user.Age) - 161f;
			}
		}

		private static float GetActivityMultiplier(BaseApplicationUser.ActivityLevelE? activityLevel)
		{
			return activityLevel switch
			{
				BaseApplicationUser.ActivityLevelE.VeryLow => 1.2f,    // Сидячий образ жизни
				BaseApplicationUser.ActivityLevelE.Low => 1.375f,      // Легкая активность 1-3 раза в неделю
				BaseApplicationUser.ActivityLevelE.Medium => 1.55f,    // Умеренная активность 3-5 раз в неделю
				BaseApplicationUser.ActivityLevelE.MediumHigh => 1.725f, // Высокая активность 6-7 раз в неделю
				BaseApplicationUser.ActivityLevelE.VeryHigh => 1.9f,   // Очень высокая активность
				_ => 1.2f
			};
		}

		private static float CalculateProtein(float weight, int calories)
		{
			// Рекомендация: 1.6-2.2 г белка на кг веса для активных людей
			// Используем среднее значение 1.8 г/кг
			float proteinPerKg = 1.8f;
			float proteinGrams = weight * proteinPerKg;

			// Альтернативный расчет: 20-30% от общей калорийности
			float proteinFromCalories = (calories * 0.25f) / 4f; // 25% калорий из белка

			// Возвращаем среднее значение
			return (proteinGrams + proteinFromCalories) / 2f;
		}

		private static float CalculateFat(int calories)
		{
			// Рекомендация: 20-35% от общей калорийности
			// Используем 25% калорий из жиров
			return (calories * 0.25f) / 9f; // 1 г жира = 9 ккал
		}

		private static float CalculateCarbs(int calories, float protein, float fat)
		{
			// Оставшиеся калории идут на углеводы
			float proteinCalories = protein * 4f;
			float fatCalories = fat * 9f;
			float remainingCalories = calories - proteinCalories - fatCalories;

			return remainingCalories / 4f; // 1 г углеводов = 4 ккал
		}
	}
}
