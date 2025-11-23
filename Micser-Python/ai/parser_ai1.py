from openai import OpenAI

LLM_API_KEY = "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"
LLM_MODEL = "google/gemma-2-9b-it:free",

def reciept(str_rec):
    client = OpenAI(
        base_url="https://openrouter.ai/api/v1",
        api_key="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")

    _SYSTEM_PROMPT = str({
        "title": "Пшённая каша",
        "instructions": "...",
        "ingridients": [
            {"title": "Пшено (сухое)", "amount": 60},
            {"title": "Молоко 2.5%", "amount": 200},
        ]
    })

    response=client.chat.completions.create(
        model="tngtech/deepseek-r1t2-chimera:free",
        messages=[
            {
                "role": "user",
                "content": f"ты должен привести это {str_rec} к такому формату{_SYSTEM_PROMPT}. НИ В КОЕМ СЛУЧАЕ НЕ ВЫВОДИ ДОПОЛНИТЕЛЬНЫЙ ТЕКСТ по типу (вот ваш Джейсон без лишней информации..) ТОЛЬКО ФОРМАТ КОТОРЫЙ Я ЗАПРОСИЛ ТЕКСТОМ, первый и последний символ это открытие списка Джейсонов и закрытие"

            }

        ],
    temperature=0)
    return f"[{response.choices[0].message.text.split('[', 1)[1].split(']', 1)[0]}]"