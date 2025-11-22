import openai


class LLMClient:
    def __init__(self, api_key: str, model: str = "kwaipilot/kat-coder-pro:free"):
        self.client = openai.OpenAI(api_key=api_key)
        self.model = model


def ask(self, instructions: str, prompt: str) -> str:
    response = self.client.responses.create(
        model=self.model,
        instructions=instructions,
        input=prompt,
        text=True
    )
    return response.output_text
