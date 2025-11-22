import os
from dataclasses import dataclass
from pathlib import Path

# Optional: load local .env file placed in the same folder (Micser-Python/config/.env.local)
try:
    from dotenv import load_dotenv
    _env_path = Path(__file__).parent / ".env.local"
    if _env_path.exists():
        load_dotenv(_env_path)
except Exception:
    # python-dotenv is optional for production; env vars can be provided by the environment
    pass


@dataclass
class Settings:
    LLM_API_KEY: str = os.getenv("LLM_API_KEY", "sk-or-v1-xxxx")
    LLM_MODEL: str = os.getenv("LLM_MODEL", "kwaipilot/kat-coder-pro:free")

    POSTGRES_HOST: str = os.getenv("POSTGRES_HOST", "localhost")
    POSTGRES_PORT: int = int(os.getenv("POSTGRES_PORT", 5432))
    POSTGRES_DB: str = os.getenv("POSTGRES_DB", "recipes")
    POSTGRES_USER: str = os.getenv("POSTGRES_USER", "postgres")
    POSTGRES_PASSWORD: str = os.getenv("POSTGRES_PASSWORD", "postgres")
    # DB echo for SQLAlchemy engine (env: DB_ECHO)
    DB_ECHO: bool = os.getenv("DB_ECHO", "False").lower() in ("1", "true", "yes")
    # Safety: disable all DB write operations by default. Set DB_WRITE_ENABLED=true to allow creates/updates/deletes.
    DB_WRITE_ENABLED: bool = os.getenv("DB_WRITE_ENABLED", "false").lower() in ("1", "true", "yes")

    # Edamama (nutrition) API
    EDAMAMA_API_KEY: str = os.getenv("EDAMAMA_API_KEY", "demo-key")
    EDAMAMA_APP_ID: str = os.getenv("EDAMAMA_APP_ID", "demo-app-id")
    EDAMAMA_URL: str = os.getenv("EDAMAMA_URL", "https://api.edamam.com/api/nutrition-data")

    @property
    def postgres_dsn(self) -> str:
        # Use asyncpg driver for SQLAlchemy async engine
        return (
            f"postgresql+asyncpg://{self.POSTGRES_USER}:{self.POSTGRES_PASSWORD}"
            f"@{self.POSTGRES_HOST}:{self.POSTGRES_PORT}/{self.POSTGRES_DB}"
        )

    @property
    def db_echo(self) -> bool:
        return self.DB_ECHO

        @property
        def db_write_enabled(self) -> bool:
            return self.DB_WRITE_ENABLED


# Singleton-like
settings = Settings()
