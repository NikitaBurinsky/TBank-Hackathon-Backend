import os
import re
import json
import asyncio
from typing import Optional

LLM_API_KEY = "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"
LLM_MODEL ="kwaipilot/kat-coder-pro:free"


async def ask(prompt: str, system: Optional[str] = None, max_tokens: int = 512, model: Optional[str] = None) -> str:
    """Ask the LLM for a completion. Async wrapper.

    - If `OPENAI_API_KEY` is present and `openai` is installed, use OpenAI.
    - Otherwise raise a clear RuntimeError instructing how to configure an API key.
    """
    api_key = LLM_API_KEY
    model = LLM_MODEL

    # run OpenAI call in thread since openai-python is sync in many versions
    def _call():
        openai.api_key = LLM_API_KEY
        messages = []
        if system:
            messages.append({"role": "system", "content": system})
        messages.append({"role": "user", "content": prompt})

        resp = openai.ChatCompletion.create(
            model=model,
            messages=messages,
            max_tokens=max_tokens,
            temperature=0.0,
        )
        # pick text
        choices = resp.get('choices') or []
        if not choices:
            return ''
        return choices[0].get('message', {}).get('content', '')

    loop = asyncio.get_event_loop()
    return await loop.run_in_executor(None, _call)


def parse_json(text: str):
    """Extract the first JSON object/array from text and return parsed Python value."""
    if not text:
        return None

    # find first { ... } or [ ... ] balancing braces roughly
    # crude approach: find first { or [ and then attempt json.loads on increasing windows
    m = re.search(r"([\[{])", text)
    if not m:
        return None
    start = m.start(1)
    for end in range(len(text), start, -1):
        try:
            candidate = text[start:end]
            return json.loads(candidate)
        except Exception:
            continue
    # fallback: try to directly json.loads whole text
    try:
        return json.loads(text)
    except Exception:
        return None
