class AIIngredientNormalizer:
    def __init__(self, llm_client):
        self.llm = llm_client

    def normalize(self, ingredients: list[str]) -> list[dict]:
        instructions = "Роль: нормализатор ингредиентов. Приводи к нижнему регистру, убирай вариации."
        prompt = f"Список ингредиентов: {ingredients}\nВерни список JSON с name, amount, unit"
        response = self.llm.ask(instructions, prompt)
        return response