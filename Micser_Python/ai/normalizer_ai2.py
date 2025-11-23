import json
from typing import Optional

from Micser_Python.ai.llm_client import ask, parse_json


_SYSTEM_PROMPT = (
    "You are a nutrition assistant. Given a JSON receipt with `ingridients` (title+amount), "
    "return a JSON array of objects for each ingredient with keys: \n"
    "title, measurementUnit, kcal, protein, fat, carbs\n"
    "measurementUnit should be 'g' or 'ml' or 'pieces'. kcal/protein/fat/carbs are numeric (per 100g/ml).\n"
    "Only output valid JSON (an array) and nothing else."
)


async def ingredients_nutrition(restricted_receipt_json: str, model: Optional[str] = None) -> str:
    prompt = (
        "Input JSON:\n" + restricted_receipt_json + "\n\nProduce the nutrition array as described."
    )
    out = await ask(prompt, system=_SYSTEM_PROMPT, model=model)
    parsed = parse_json(out)
    if parsed is None:
        raise RuntimeError('LLM did not return valid JSON for nutrition array')
    return json.dumps(parsed, ensure_ascii=False, indent=2)


if __name__ == '__main__':
    import asyncio
    demo = {
        "title": "Пшённая каша",
        "instructions": "...",
        "ingridients": [
            {"title": "Пшено (сухое)", "amount": 60},
            {"title": "Молоко 2.5%", "amount": 200},
        ]
    }
    print(asyncio.run(ingredients_nutrition(json.dumps(demo, ensure_ascii=False))))
