from sqlalchemy import select, delete, update
from sqlalchemy.ext.asyncio import AsyncSession

from Micser_Python.db.models import Recipe
from Micser_Python.config.settings import settings


class RecipeRepository:

    @staticmethod
    async def get_all(session: AsyncSession):
        result = await session.execute(select(Recipe))
        return result.scalars().all()

    @staticmethod
    async def get_by_id(session: AsyncSession, recipe_id: int):
        result = await session.execute(
            select(Recipe).where(Recipe.id == recipe_id)
        )
        return result.scalar_one_or_none()

    @staticmethod
    async def create(session: AsyncSession, recipe_data: dict):
        if not settings.db_write_enabled:
            raise RuntimeError(
                "DB writes are disabled (DB_WRITE_ENABLED=false). Enable explicitly to allow create operations."
            )

        recipe = Recipe(**recipe_data)
        session.add(recipe)
        await session.commit()
        await session.refresh(recipe)
        return recipe

    @staticmethod
    async def update(session: AsyncSession, recipe_id: int, new_data: dict):
        if not settings.db_write_enabled:
            raise RuntimeError(
                "DB writes are disabled (DB_WRITE_ENABLED=false). Enable explicitly to allow update operations."
            )

        await session.execute(
            update(Recipe)
            .where(Recipe.id == recipe_id)
            .values(**new_data)
        )
        await session.commit()

    @staticmethod
    async def delete(session: AsyncSession, recipe_id: int):
        if not settings.db_write_enabled:
            raise RuntimeError(
                "DB writes are disabled (DB_WRITE_ENABLED=false). Enable explicitly to allow delete operations."
            )

        await session.execute(
            delete(Recipe).where(Recipe.id == recipe_id)
        )
        await session.commit()