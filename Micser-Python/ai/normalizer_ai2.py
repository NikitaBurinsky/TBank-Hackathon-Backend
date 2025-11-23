
from openai import OpenAI


_SYSTEM_PROMPT = (
    "You are a nutrition assistant. Given a JSON receipt with `ingridients` (title+amount), "
    "return a JSON array of objects for each ingredient with keys: \n"
    "title, measurementUnit, kcal, protein, fat, carbs\n"
    "measurementUnit should be 'g' or 'ml' or 'pieces'. kcal/protein/fat/carbs are numeric (per 100g/ml).\n"
    "Only output valid JSON (an array) and nothing else."
)

def ingredients_nutrition(str_rec):
    client = OpenAI(
        base_url="https://openrouter.ai/api/v1",
        api_key="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",)

    response=client.chat.completions.create(
        model="kwaipilot/kat-coder-pro:free",
        messages=[
            {
                "role": "user",
                "content": f"ты должен привести это {str_rec} к такому формату{_SYSTEM_PROMPT}"

            }

        ],
    temperature=0)
    return response.choices[0].message


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
