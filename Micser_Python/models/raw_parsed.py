from pydantic import BaseModel
from typing import List


class RawParsedIngredient(BaseModel):
    """
    результат парсинга ИИ №2:
    - имя в нижнем регистре
    - количество
    - юниты
    """
    name: str
    amount: float
    unit: str


class RawParsedRecipe(BaseModel):
    """
    результат ИИ №1:
    - cooking_method: текст/инструкции
    - raw_ingredients: список строк,
      которые ИИ №2 ещё будет парсить
    """
    cooking_method: str
    raw_ingredients: List[str]

