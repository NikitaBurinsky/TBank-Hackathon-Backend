from openai import OpenAI

LLM_API_KEY = "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"
LLM_MODEL = "kwaipilot/kat-coder-pro:free"

_SYSTEM_PROMPT = (
    "You are a JSON-extraction assistant. Given a free-form recipe text, "
    "return valid JSON with the following shape:\n"
    "{\n  \"title\": string,\n  \"instructions\": string,\n  \"ingridients\": [{\"title\": string, \"amount\": number}]\n}\n"
    "Only output JSON and nothing else. Be strict about keys and types."
)


def reciept(str_rec):
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

    demo = """Пшённая каша с тыквой\nПшено 60\nМолоко 200\nСахар 10\nСоль 3\nПшено отварить почти до готовности в воде, затем добавить молоко и довести до мягкости."""
    print(asyncio.run(reciept(demo)))
