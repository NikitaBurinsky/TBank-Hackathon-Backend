from sqlalchemy.ext.asyncio import AsyncSession, create_async_engine
from sqlalchemy.orm import sessionmaker

from Micser_Python.config.settings import settings

engine = create_async_engine(
    settings.postgres_dsn,
    echo=settings.db_echo,
    future=True
)

async_session_maker = sessionmaker(
    engine,
    class_=AsyncSession,
    expire_on_commit=False
)
