from Micser_Python.models.ingredient import IngredientCreate, IngredientDB
from Micser_Python.db.ingredient_repo import IngredientRepository


class IngredientService:

    @staticmethod
    async def normalize_name(name: str) -> str:
        """
        Приведение к нижнему регистру, удаление скобок, лишних пробелов.
        То, что делает AI2, но дополнительно перестраховываемся.
        """
        name = name.strip().lower()
        if "(" in name:
            name = name.split("(")[0].strip()
        return name

    @staticmethod
    async def get_or_create(data: IngredientCreate) -> IngredientDB:
        """
        Основная логика:
        - нормализация имени
        - поиск в БД
        - если нет → создание
        """

        normalized_title = await IngredientService.normalize_name(data.title)

        existing = await IngredientRepository.get_by_title(normalized_title)
        if existing:
            return existing

        # создаём новую запись
        created = await IngredientRepository.create(
            {
                "title": normalized_title,
                "protein": data.protein,
                "fat": data.fat,
                "carbs": data.carbs,
                "kcal": data.kcal,
                "measurement_unit": data.measurement_unit,
            }
        )

        return created

    @staticmethod
    async def get_by_title(title: str):
        normalized_title = await IngredientService.normalize_name(title)
        return await IngredientRepository.get_by_title(normalized_title)

    @staticmethod
    async def list_all():
        return await IngredientRepository.list_all()
