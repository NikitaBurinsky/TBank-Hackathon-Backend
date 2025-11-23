from pydantic import BaseModel, Field
from typing import Dict


class RecipeBase(BaseModel):
    title: str = Field(..., description="Название блюда")
    instructions: str = Field(..., description="Текст приготовления")


class RecipeIngredients(BaseModel):
    ingredients_amount: Dict[str, float]  # {"овсянка": 20, "рис": 30}


class RecipeCreate(RecipeBase, RecipeIngredients):
    """Модель создания рецепта"""
    pass


class RecipeDB(RecipeBase, RecipeIngredients):
    id: int

