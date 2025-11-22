from sqlalchemy import select, delete, update
from sqlalchemy.ext.asyncio import AsyncSession

from db.models import Recipe


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
        recipe = Recipe(**recipe_data)
        session.add(recipe)
        await session.commit()
        await session.refresh(recipe)
        return recipe

    @staticmethod
    async def update(session: AsyncSession, recipe_id: int, new_data: dict):
        await session.execute(
            update(Recipe)
            .where(Recipe.id == recipe_id)
            .values(**new_data)
        )
        await session.commit()

    @staticmethod
    async def delete(session: AsyncSession, recipe_id: int):
        await session.execute(
            delete(Recipe).where(Recipe.id == recipe_id)
        )
        await session.commit()