from openai import OpenAI

def ingredients_nutrition(str_rec):
    _SYSTEM_PROMPT = ("""[
{
    "title": "Хлеб ржаной",
    "measurementUnit": "g",
    "kcal": 210,
    "protein": 6,
    "fat": 1.1,
    "carbs": 40
  },
{
    "title": "яйца",
    "measurementUnit": "pcs",
    "kcal": 210,
    "protein": 6,
    "fat": 1.1,
    "carbs": 40
  },
{
    "title": "молоко",
    "measurementUnit": "ml",
    "kcal": 210,
    "protein": 6,
    "fat": 1.1,
    "carbs": 40
  },
]"""
)
    client = OpenAI(
        base_url="https://openrouter.ai/api/v1",
        api_key="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",)

    response=client.chat.completions.create(
        model="tngtech/deepseek-r1t2-chimera:free",
        messages=[
            {
                "role": "user",
                "content": f"ты должен привести это {str_rec} к такому формату{_SYSTEM_PROMPT} ВАЖНО не выводи ниаких объяснений, только JSON без лишних выделений и тд. НИ В КОЕМ СЛУЧАЕ НЕ ВЫВОДИ ДОПОЛНИТЕЛЬНЫЙ ТЕКСТ по типу (вот ваш Джейсон без лишней информации..) ТОЛЬКО ФОРМАТ КОТОРЫЙ Я ЗАПРОСИЛ ТЕКСТОМ, первый и последний символ это открытие списка Джейсонов и закрытие"

            }

        ],
    temperature=0)
    return f"[{response.choices[0].message.text.split('[', 1)[1].split(']', 1)[0]}]"


if 1:
    print("Testing ingredients nutrition normalizer...")

    demo = {
        "title": "Пшённая каша",
        "instructions": "...",
        "ingridients": [
            {"title": "Пшено (сухое)", "amount": 60},
            {"title": "Молоко 2.5%", "amount": 200},
        ]
    }
    print(ingredients_nutrition(demo))