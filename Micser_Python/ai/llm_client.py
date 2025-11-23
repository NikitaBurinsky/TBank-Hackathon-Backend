from openai import OpenAI
import asyncio
import json
import re
from typing import Optional, Any


def _strip_code_blocks(text: str) -> str:
    """Remove common code fences and surrounding ```json markers."""
    if not text:
        return text
    # remove triple-backtick blocks and single backticks
    text = re.sub(r"```(?:json)?\n", "", text, flags=re.IGNORECASE)
    text = text.replace("```", "")
    text = text.replace("`", "")
    return text.strip()


class LLMClient:
    """Light wrapper around OpenAI client using `openai.OpenAI`.

    Provides async `ask` and a `parse_json` helper.
    """

    def __init__(self, api_key: str, model: str = "kwaipilot/kat-coder-pro:free", base_url: Optional[str] = None):
        # Create OpenAI client instance from new SDK
        kwargs = {}
        if base_url:
            kwargs["base_url"] = base_url
        self.client = OpenAI(api_key=api_key, **kwargs)
        self.model = model

    async def ask(self, instructions: str, prompt: str, max_tokens: Optional[int] = 800) -> str:
        """Ask the configured LLM and return the textual reply.

        Uses `client.chat.completions.create(...)` and runs the blocking call in a
        thread so it can be awaited from async code.
        """

        def _call_sync():
            messages = [
                {"role": "system", "content": instructions},
                {"role": "user", "content": prompt},
            ]

            resp = self.client.chat.completions.create(
                model=self.model,
                messages=messages,
                max_tokens=max_tokens,
            )

            # Response shape varies by SDK; try common access patterns
            try:
                choice = resp.choices[0]
                msg = getattr(choice, "message", None)
                if msg is not None:
                    return getattr(msg, "content", msg.get("content") if isinstance(msg, dict) else str(choice))
                return getattr(choice, "text", choice.get("text") if isinstance(choice, dict) else str(choice))
            except Exception:
                return str(resp)

        loop = asyncio.get_running_loop()
        text = await loop.run_in_executor(None, _call_sync)
        return text

    def parse_json(self, text: str) -> Any:
        """Try to parse JSON from LLM output robustly.

        Strips code fences and attempts json.loads. Raises ValueError on failure.
        """
        text = _strip_code_blocks(text)
        idxs = [i for i in (text.find("{"), text.find("[")) if i != -1]
        if idxs:
            first = min(idxs)
            if first > 0:
                text = text[first:]
        try:
            return json.loads(text)
        except Exception as e:
            raise ValueError(f"Failed to parse JSON from LLM response: {e}\nRaw: {text}")