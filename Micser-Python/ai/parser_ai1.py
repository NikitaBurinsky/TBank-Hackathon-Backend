class AIParserRaw:

    def __init__(self, llm_client):
        self.llm = llm_client

    def parse(self, text: str) -> dict:
        instructions = "Роль: минимальный парсер рецептов. Максимально сохраняй контекст."
        prompt = f"Текст рецепта: {text}\nВерни JSON: cooking_method_str, raw_str_ingredients"
        response = self.llm.ask(instructions, prompt)
        return response