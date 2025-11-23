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
        model="google/gemma-2-9b-it:free",
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

    demo = """Пшённая каша с тыквой\nПшено 60\nМолоко 200\nСахар 10\nСоль 3\nПшено отварить почти до готовности в воде, затем добавить молоко и довести до мягкости."""
    print(asyncio.run(reciept(demo)))
