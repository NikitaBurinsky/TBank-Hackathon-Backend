from pydantic import BaseModel, Field
from typing import Literal


class IngredientBase(BaseModel):
    title: str = Field(..., description="Название продукта в нормализованном виде")
    protein: float = Field(..., description="Белки на 1 грамм")
    fat: float = Field(..., description="Жиры на 1 грамм")
    carbs: float = Field(..., description="Углеводы на 1 грамм")
    kcal: float = Field(..., description="Калории на 1 грамм")
    measurement_unit: Literal["g", "ml", "pieces"]


class IngredientCreate(IngredientBase):
    """Модель для создания ингредиента в БД"""
    pass


class IngredientDB(IngredientBase):
    """Модель ингредиента, как он хранится в БД"""
    id: int
