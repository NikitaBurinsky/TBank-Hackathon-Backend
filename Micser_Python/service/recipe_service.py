from Micser_Python.models.recipe import RecipeCreate, RecipeDB
from Micser_Python.db.recipe_repo import RecipeRepository
from Micser_Python.service.ingredient_service import IngredientService
from Micser_Python.models.ingredient import IngredientCreate
from Micser_Python.models.raw_parsed import RawParsedIngredient, RawParsedRecipe


class RecipeService:

    @staticmethod
    async def create(data: RecipeCreate) -> RecipeDB:
        """
        Создание рецепта:
        - ингредиенты уже должны быть нормализованы (AI2)
        - проверяем что ингредиенты есть в БД (если нет — ошибка)
        - сохраняем рецепт и связываем ингредиенты
        """

        ingredient_amounts = {}

        for title, amount in data.ingredients_amount.items():
            ingredient = await IngredientService.get_by_title(title)
            if not ingredient:
                raise ValueError(f"Ingredient '{title}' not found in DB")

            ingredient_amounts[ingredient.title] = amount

        created = await RecipeRepository.create(
            {
                "title": data.title,
                "instructions": data.instructions,
                "ingredients_amount": ingredient_amounts,
            }
        )

        return created

    @staticmethod
    async def create_from_raw(raw: RawParsedRecipe):
        """
        Полная цепочка работы:
        raw (AI1) → raw ingredient strings → AI2 → normalized → get_or_create → recipe

        Здесь мы не вызываем AI2 — это должен делать твой AI pipeline.
        Здесь мы принимаем уже готовый массив RawParsedIngredient.
        """

        raise NotImplementedError(
            "Эта функция реализуется позже, после AI2 флоу."
        )

    @staticmethod
    async def get(recipe_id: int):
        return await RecipeRepository.get(recipe_id)

    @staticmethod
    async def list_all():
        return await RecipeRepository.list_all()
