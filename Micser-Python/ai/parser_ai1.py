import json
from typing import Any
from models.raw_parsed import RawParsedRecipe


class AIParserRaw:
    def __init__(self, llm_client):
        self.llm = llm_client

    async def parse(self, text: str) -> Any:
        """Parse raw recipe text into a RawParsedRecipe-like dict.

        Returns parsed JSON dict or raises ValueError when parsing fails.
        """
        instructions = (
            "Роль: минимальный парсер рецептов. "
            "Извлеки метод приготовления и список сырых ингредиентов."
        )
        prompt = (
            f"Текст рецепта:\n{text}\n\n"
            "Верни строго JSON с полями: cooking_method (string), raw_ingredients (array of strings)."
        )

        raw = await self.llm.ask(instructions, prompt)

        # Try to parse JSON using client's helper when available
        try:
            parsed = self.llm.parse_json(raw)
        except Exception:
            # Fallback: try plain json.loads
            try:
                parsed = json.loads(raw)
            except Exception as e:
                raise ValueError(f"Parser AI produced non-JSON output: {e}\nRaw: {raw}")

        # Validate minimal shape
        if not isinstance(parsed, dict) or "cooking_method" not in parsed or "raw_ingredients" not in parsed:
            raise ValueError(f"Parsed output missing required fields: {parsed}")

        # Optionally coerce into Pydantic model
        try:
            return RawParsedRecipe(**parsed)
        except Exception:
            return parsed