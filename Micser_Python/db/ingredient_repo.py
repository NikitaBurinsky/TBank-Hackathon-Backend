from typing import Optional

from sqlalchemy import select, delete, update
from sqlalchemy.ext.asyncio import AsyncSession

# No SQLAlchemy ORM models shipped in this lightweight layout.
# Keep this module import-safe and explicit: set ORM flag to False.
Ingredient = None
_ORM_AVAILABLE = False

from Micser_Python.config.settings import settings
from Micser_Python.db.connector import async_session_maker


class IngredientRepository:

    @staticmethod
    async def get_all(session: Optional[AsyncSession] = None):
        """Return all Ingredient records. If `session` is not provided, a session is created.
        """
        if not _ORM_AVAILABLE:
            raise RuntimeError(
                "ORM models are not available (Micser_Python.db.models).\n"
                "Define SQLAlchemy models or adjust the repository to use your storage layer."
            )

        if session is None:
            async with async_session_maker() as s:
                result = await s.execute(select(Ingredient))
                return result.scalars().all()

        result = await session.execute(select(Ingredient))
        return result.scalars().all()

    # convenience alias used by service layer
    @staticmethod
    async def list_all():
        return await IngredientRepository.get_all()

    @staticmethod
    async def get_by_id(session: Optional[AsyncSession] = None, ingredient_id: int = None):
        """Get ingredient by id. Call as `get_by_id(session, id)` or `get_by_id(id=id)`.
        If `session` is omitted, pass `ingredient_id` as first positional arg.
        """
        # Support both signatures: (session, id) and (id=...)
        if ingredient_id is None and isinstance(session, int):
            ingredient_id = session
            session = None

        if not _ORM_AVAILABLE:
            raise RuntimeError("ORM models are not available (Micser_Python.db.models).")

        if session is None:
            async with async_session_maker() as s:
                result = await s.execute(
                    select(Ingredient).where(Ingredient.id == ingredient_id)
                )
                return result.scalar_one_or_none()

        result = await session.execute(
            select(Ingredient).where(Ingredient.id == ingredient_id)
        )
        return result.scalar_one_or_none()

    @staticmethod
    async def get_by_title(title: str):
        """Convenience method used by services: returns Ingredient or None by title."""
        if not _ORM_AVAILABLE:
            raise RuntimeError("ORM models are not available (Micser_Python.db.models).")

        async with async_session_maker() as s:
            result = await s.execute(select(Ingredient).where(Ingredient.title == title))
            return result.scalar_one_or_none()

    @staticmethod
    async def create(session: Optional[AsyncSession] = None, ingredient_data: dict = None):
        """Create ingredient. Can be called as `create(data)` or `create(session, data)`.
        """
        # Allow calling create(data) without session
        if ingredient_data is None and session is not None and not isinstance(session, AsyncSession):
            ingredient_data = session
            session = None

        if not settings.db_write_enabled:
            raise RuntimeError(
                "DB writes are disabled (DB_WRITE_ENABLED=false). Enable explicitly to allow create operations."
            )

        if not _ORM_AVAILABLE:
            raise RuntimeError("ORM models are not available (Micser_Python.db.models).")

        if session is None:
            async with async_session_maker() as s:
                ingredient = Ingredient(**ingredient_data)
                s.add(ingredient)
                await s.commit()
                await s.refresh(ingredient)
                return ingredient

        ingredient = Ingredient(**ingredient_data)
        session.add(ingredient)
        await session.commit()
        await session.refresh(ingredient)
        return ingredient

    @staticmethod
    async def update(session: Optional[AsyncSession] = None, ingredient_id: int = None, new_data: dict = None):
        """Update ingredient. Supports both (session, id, new_data) and (id, new_data) signatures."""
        # Normalize signature: if new_data is None, shift args
        if new_data is None and isinstance(session, int):
            new_data = ingredient_id
            ingredient_id = session
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
                    update(Ingredient)
                    .where(Ingredient.id == ingredient_id)
                    .values(**new_data)
                )
                await s.commit()
                return

        await session.execute(
            update(Ingredient)
            .where(Ingredient.id == ingredient_id)
            .values(**new_data)
        )
        await session.commit()

    @staticmethod
    async def delete(session: Optional[AsyncSession] = None, ingredient_id: int = None):
        """Delete ingredient. Supports (session, id) and (id) signatures."""
        if ingredient_id is None and isinstance(session, int):
            ingredient_id = session
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
                    delete(Ingredient).where(Ingredient.id == ingredient_id)
                )
                await s.commit()
                return

        await session.execute(
            delete(Ingredient).where(Ingredient.id == ingredient_id)
        )
        await session.commit()