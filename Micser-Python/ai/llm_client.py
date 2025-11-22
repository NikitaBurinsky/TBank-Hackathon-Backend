import openai
from typing import Optional


class LLMClient:
    def __init__(self, api_key: str, model: str = "kwaipilot/kat-coder-pro:free"):
        # configure global openai client key (works with openai-python)
        openai.api_key = api_key
        self.client = openai
        self.model = model

    def ask(self, instructions: str, prompt: str, max_tokens: Optional[int] = 800) -> str:
        """Ask the configured LLM. Returns text.

        Uses ChatCompletion where available; falls back to Responses API if necessary.
        """
        # Prefer ChatCompletion-like API
        try:
            resp = self.client.ChatCompletion.create(
                model=self.model,
                messages=[
                    {"role": "system", "content": instructions},
                    {"role": "user", "content": prompt},
                ],
                max_tokens=max_tokens,
            )
            return resp.choices[0].message.content
        except Exception:
            # Fallback for newer Responses API
            resp = self.client.responses.create(
                model=self.model,
                instructions=instructions,
                input=prompt,
            )
            return getattr(resp, "output_text", str(resp))
