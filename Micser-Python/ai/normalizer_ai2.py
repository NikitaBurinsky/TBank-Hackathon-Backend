import json
from typing import Any, List
from models.raw_parsed import RawParsedIngredient


class AIIngredientNormalizer:
    def __init__(self, llm_client):
        self.llm = llm_client

    async def normalize(self, ingredients: List[str]) -> Any:
        """Normalize a list of raw ingredient strings into structured objects.

        Returns a list of dicts with keys: name, amount, unit. Tries to coerce into
        `RawParsedIngredient` when possible.
        """
        instructions = (
            "Роль: нормализатор ингредиентов. "
            "Приводи к нижнему регистру, старайся вернуть name, amount (float), unit (string)."
        )
        prompt = (
            f"Список ингредиентов: {ingredients}\n\n"
            "Верни строго JSON-массив объектов с полями: name, amount, unit. "
            "Например: [{\"name\": \"овсянка\", \"amount\": 50, \"unit\": \"g\"}]"
        )

        raw = await self.llm.ask(instructions, prompt)

        try:
            parsed = self.llm.parse_json(raw)
        except Exception:
            try:
                parsed = json.loads(raw)
            except Exception as e:
                raise ValueError(f"Normalizer AI produced non-JSON output: {e}\nRaw: {raw}")

        # Basic validation/coercion
        if not isinstance(parsed, list):
            raise ValueError(f"Normalizer output expected a list, got: {type(parsed)} -> {parsed}")

        coerced = []
        for item in parsed:
            if isinstance(item, dict) and "name" in item:
                try:
                    coerced_item = RawParsedIngredient(
                        name=item.get("name"),
                        amount=float(item.get("amount", 0) or 0),
                        unit=str(item.get("unit", "g")),
                    )
                    coerced.append(coerced_item)
                except Exception:
                    coerced.append(item)
            else:
                coerced.append(item)

        return coerced