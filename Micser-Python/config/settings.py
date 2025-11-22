import os
from dataclasses import dataclass


@dataclass
class Settings:



    LLM_API_KEY: str = os.getenv("LLM_API_KEY", "sk-or-v1-xxxx")
    LLM_MODEL: str = os.getenv("LLM_MODEL", "kwaipilot/kat-coder-pro:free")

    
    POSTGRES_HOST: str = os.getenv("POSTGRES_HOST", "localhost")
    POSTGRES_PORT: int = int(os.getenv("POSTGRES_PORT", 5432))
    POSTGRES_DB: str = os.getenv("POSTGRES_DB", "recipes")
    POSTGRES_USER: str = os.getenv("POSTGRES_USER", "postgres")
    POSTGRES_PASSWORD: str = os.getenv("POSTGRES_PASSWORD", "postgres")


@property
def postgres_dsn(self) -> str:


    return (
        f"postgresql://{self.POSTGRES_USER}:{self.POSTGRES_PASSWORD}"
        f"@{self.POSTGRES_HOST}:{self.POSTGRES_PORT}/{self.POSTGRES_DB}"
    )

# --- EDAMAMA ---
EDAMAMA_API_KEY: str = os.getenv("EDAMAMA_API_KEY", "demo-key")
EDAMAMA_APP_ID: str = os.getenv("EDAMAMA_APP_ID", "demo-app-id")
EDAMAMA_URL: str = os.getenv("EDAMAMA_URL", "https://api.edamam.com/api/nutrition-data")

# Singleton-like
settings = Settings()
