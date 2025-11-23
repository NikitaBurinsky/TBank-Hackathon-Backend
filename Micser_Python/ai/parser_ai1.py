import json
from typing import Optional

from Micser_Python.ai.llm_client import ask, parse_json


_SYSTEM_PROMPT = (
    "You are a JSON-extraction assistant. Given a free-form recipe text, "
    "return valid JSON with the following shape:\n"
    "{\n  \"title\": string,\n  \"instructions\": string,\n  \"ingridients\": [{\"title\": string, \"amount\": number}]\n}\n"
    "Only output JSON and nothing else. Be strict about keys and types."
)


async def reciept(text: str, model: Optional[str] = None) -> str:
    """Ask the LLM to convert free-form recipe text into the restricted JSON.

    Returns a JSON string (pretty-printed) or raises if parsing failed.
    """
    prompt = (
        "Convert the following recipe text into the JSON shape requested.\n\n" + text
    )

    out = await ask(prompt, system=_SYSTEM_PROMPT, model=model)
    parsed = parse_json(out)
    if parsed is None:
        raise RuntimeError('LLM did not return valid JSON for recipe')
    return json.dumps(parsed, ensure_ascii=False, indent=2)


if __name__ == '__main__':
    import asyncio

    demo = """Пшённая каша с тыквой\nПшено 60\nМолоко 200\nСахар 10\nСоль 3\nПшено отварить почти до готовности в воде, затем добавить молоко и довести до мягкости."""
    print(asyncio.run(reciept(demo)))
