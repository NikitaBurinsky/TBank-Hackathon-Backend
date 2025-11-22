import requests


class EdamamaAPI:
    def __init__(self, api_key: str, app_id: str, url: str = "https://api.edamam.com/api/nutrition-data"):
        self.api_key = api_key
        self.app_id = app_id
        self.url = url

    def get_nutrition_for(self, ingredient_name: str) -> dict:

        """
        Получает БЖУ на 1 грамм продукта.


        Edamama отдаёт инфу за 100 грамм → переводим в 1 грамм.
        """

        params = {
            "app_id": self.app_id,
            "app_key": self.api_key,
            "ingr": f"100g {ingredient_name}"
        }

        response = requests.get(self.url, params=params)

        if response.status_code != 200:
            raise Exception(f"Edamama API error: {response.status_code} => {response.text}")

        data = response.json()

        # БЖУ и калории за 100 грамм
        protein_100 = data.get("totalNutrients", {}).get("PROCNT", {}).get("quantity", 0)
        fat_100 = data.get("totalNutrients", {}).get("FAT", {}).get("quantity", 0)
        carbs_100 = data.get("totalNutrients", {}).get("CHOCDF", {}).get("quantity", 0)
        kcal_100 = data.get("calories", 0)

        # Пересчёт на 1 грамм
        protein = protein_100 / 100
        fat = fat_100 / 100
        carbs = carbs_100 / 100
        kcal = kcal_100 / 100

        return {
            "Title": ingredient_name,
            "Protein": round(protein, 4),
            "Fat": round(fat, 4),
            "Carbs": round(carbs, 4),
            "Kcal": round(kcal, 4),
            "MeasurementUnit": "g"
        }
