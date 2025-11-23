import os
from dataclasses import dataclass

@dataclass
class Settings:
    # NOTE: values below are embedded per user request. These are secrets and
    # will be committed to the repository. Do NOT expose publicly unless you
    # understand the security implications.
    LLM_API_KEY: str = os.getenv(
        "LLM_API_KEY",
        "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",
    )
    LLM_MODEL: str = os.getenv("LLM_MODEL", "kwaipilot/kat-coder-pro:free")

    POSTGRES_HOST: str = os.getenv("POSTGRES_HOST", "2.56.240.190")
    POSTGRES_PORT: int = int(os.getenv("POSTGRES_PORT", "5432"))
    POSTGRES_DB: str = os.getenv("POSTGRES_DB", "dbase")
    POSTGRES_USER: str = os.getenv("POSTGRES_USER", "postgres")
    POSTGRES_PASSWORD: str = os.getenv("POSTGRES_PASSWORD", "redbull")
    # DB echo for SQLAlchemy engine (env: DB_ECHO)
    DB_ECHO: bool = os.getenv("DB_ECHO", "False").lower() in ("1", "true", "yes")
    # Safety: disable all DB write operations by default. Set DB_WRITE_ENABLED=true to allow creates/updates/deletes.
    DB_WRITE_ENABLED: bool = os.getenv("DB_WRITE_ENABLED", "false").lower() in ("1", "true", "yes")

    # Edamama (nutrition) API
    EDAMAMA_API_KEY: str = os.getenv("EDAMAMA_API_KEY", "0f92473c81548bceed35493bfff74d5c")
    EDAMAMA_APP_ID: str = os.getenv("EDAMAMA_APP_ID", "2be90dc0")
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
