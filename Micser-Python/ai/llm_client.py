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

        Uses `client.chat.completions.create(...)` (no extra reasoning) and runs
        the blocking call in a thread so it can be awaited from async code.
        """

        def _call_sync():
            # Build messages list
            messages = [
                {"role": "system", "content": instructions},
                {"role": "user", "content": prompt},
            ]

            # Call the Chat Completions endpoint
            resp = self.client.chat.completions.create(
                model=self.model,
                messages=messages,
                max_tokens=max_tokens,
            )

            # Response shape: resp.choices[0].message.content or dict-like
            try:
                choice = resp.choices[0]
                # some SDKs return nested objects
                msg = getattr(choice, "message", None)
                if msg is not None:
                    return getattr(msg, "content", msg.get("content") if isinstance(msg, dict) else str(choice))
                return getattr(choice, "text", choice.get("text") if isinstance(choice, dict) else str(choice))
            except Exception:
                # As a last resort return stringified response
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
from OpenAi import openai
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
        import openai
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
            def __init__(self, api_key: str, model: str = "kwaipilot/kat-coder-pro:free"):
                # Configure openai client key (compatible with openai-python)
                openai.api_key = api_key
                self.client = openai
                self.model = model

            async def ask(self, instructions: str, prompt: str, max_tokens: Optional[int] = 800) -> str:
                """Ask the configured LLM and return the textual reply.

                This runs the blocking OpenAI client in a thread so callers can await it.
                """
                def _call_sync():
                    # Prefer ChatCompletion API if available
                    try:
                        resp = self.client.ChatCompletion.create(
                            model=self.model,
                            messages=[
                                {"role": "system", "content": instructions},
                                {"role": "user", "content": prompt},
                            ],
                            max_tokens=max_tokens,
                        )
                        # Some libs return nested structures
                        choice = resp.choices[0]
                        # older/newer SDK shape
                        msg = getattr(choice, "message", None)
                        if msg:
                            return getattr(msg, "content", str(choice))
                        return getattr(choice, "text", str(choice))
                    except Exception:
                        # Fallback to Responses API
                        resp = self.client.responses.create(
                            model=self.model,
                            instructions=instructions,
                            input=prompt,
                        )
                        return getattr(resp, "output_text", str(resp))

                loop = asyncio.get_running_loop()
                text = await loop.run_in_executor(None, _call_sync)
                return text

            def parse_json(self, text: str) -> Any:
                """Try to parse JSON from LLM output robustly.

                Strips code fences and attempts json.loads. Raises ValueError on failure.
                """
                text = _strip_code_blocks(text)
                # Heuristic: find first { or [ and parse from there
                idxs = [i for i in (text.find("{"), text.find("[")) if i != -1]
                if idxs:
                    first = min(idxs)
                    if first > 0:
                        text = text[first:]
                try:
                    return json.loads(text)
                except Exception as e:
                    raise ValueError(f"Failed to parse JSON from LLM response: {e}\nRaw: {text}")
