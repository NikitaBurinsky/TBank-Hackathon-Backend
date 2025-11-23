from typing import Optional

from sqlalchemy import select, delete, update
from sqlalchemy.ext.asyncio import AsyncSession

# No SQLAlchemy ORM models shipped in this lightweight layout.
# Keep this module import-safe and explicit: mark ORM as unavailable.
Recipe = None
_ORM_AVAILABLE = False

from Micser_Python.config.settings import settings
from Micser_Python.db.connector import async_session_maker


class RecipeRepository:

    @staticmethod
    async def get_all(session: Optional[AsyncSession] = None):
        if not _ORM_AVAILABLE:
            raise RuntimeError("ORM models are not available (Micser_Python.db.models).")

        if session is None:
            async with async_session_maker() as s:
                result = await s.execute(select(Recipe))
                return result.scalars().all()

        result = await session.execute(select(Recipe))
        return result.scalars().all()

    # convenience alias used by service layer
    @staticmethod
    async def list_all():
        return await RecipeRepository.get_all()

    @staticmethod
    async def get_by_id(session: Optional[AsyncSession] = None, recipe_id: int = None):
        # support (session, id) and (id) signatures
        if recipe_id is None and isinstance(session, int):
            recipe_id = session
            session = None

        if not _ORM_AVAILABLE:
            raise RuntimeError("ORM models are not available (Micser_Python.db.models).")

        if session is None:
            async with async_session_maker() as s:
                result = await s.execute(
                    select(Recipe).where(Recipe.id == recipe_id)
                )
                return result.scalar_one_or_none()

        result = await session.execute(
            select(Recipe).where(Recipe.id == recipe_id)
        )
        return result.scalar_one_or_none()

    @staticmethod
    async def get(recipe_id: int):
        return await RecipeRepository.get_by_id(recipe_id)

    @staticmethod
    async def create(session: Optional[AsyncSession] = None, recipe_data: dict = None):
        # allow calling create(data) without session
        if recipe_data is None and session is not None and not isinstance(session, AsyncSession):
            recipe_data = session
            session = None

        if not settings.db_write_enabled:
            raise RuntimeError(
                "DB writes are disabled (DB_WRITE_ENABLED=false). Enable explicitly to allow create operations."
            )

        if not _ORM_AVAILABLE:
            raise RuntimeError("ORM models are not available (Micser_Python.db.models).")

        if session is None:
            async with async_session_maker() as s:
                recipe = Recipe(**recipe_data)
                s.add(recipe)
                await s.commit()
                await s.refresh(recipe)
                return recipe

        recipe = Recipe(**recipe_data)
        session.add(recipe)
        await session.commit()
        await session.refresh(recipe)
        return recipe

    @staticmethod
    async def update(session: Optional[AsyncSession] = None, recipe_id: int = None, new_data: dict = None):
        if new_data is None and isinstance(session, int):
            new_data = recipe_id
            recipe_id = session
            session = None

        if not settings.db_write_enabled:
            raise RuntimeError(
                "DB writes are disabled (DB_WRITE_ENABLED=false). Enable explicitly to allow update operations."
            )

        if not _ORM_AVAILABLE:
            raise RuntimeError("ORM models are not available (Micser_Python.db.models).")

        if session is None:
            async with async_session_maker() as s:
                await s.execute(
                    update(Recipe)
                    .where(Recipe.id == recipe_id)
                    .values(**new_data)
                )
                await s.commit()
                return

        await session.execute(
            update(Recipe)
            .where(Recipe.id == recipe_id)
            .values(**new_data)
        )
        await session.commit()

    @staticmethod
    async def delete(session: Optional[AsyncSession] = None, recipe_id: int = None):
        if recipe_id is None and isinstance(session, int):
            recipe_id = session
            session = None

        if not settings.db_write_enabled:
            raise RuntimeError(
                "DB writes are disabled (DB_WRITE_ENABLED=false). Enable explicitly to allow delete operations."
            )

        if not _ORM_AVAILABLE:
            raise RuntimeError("ORM models are not available (Micser_Python.db.models).")

        if session is None:
            async with async_session_maker() as s:
                await s.execute(
                    delete(Recipe).where(Recipe.id == recipe_id)
                )
                await s.commit()
                return

        await session.execute(
            delete(Recipe).where(Recipe.id == recipe_id)
        )
        await session.commit()