from sqlalchemy import select, delete, update
from sqlalchemy.ext.asyncio import AsyncSession

from db.models import Ingredient


class IngredientRepository:

    @staticmethod
    async def get_all(session: AsyncSession):
        result = await session.execute(select(Ingredient))
        return result.scalars().all()

    @staticmethod
    async def get_by_id(session: AsyncSession, ingredient_id: int):
        result = await session.execute(
            select(Ingredient).where(Ingredient.id == ingredient_id)
        )
        return result.scalar_one_or_none()

    @staticmethod
    async def create(session: AsyncSession, ingredient_data: dict):
        ingredient = Ingredient(**ingredient_data)
        session.add(ingredient)
        await session.commit()
        await session.refresh(ingredient)
        return ingredient

    @staticmethod
    async def update(session: AsyncSession, ingredient_id: int, new_data: dict):
        await session.execute(
            update(Ingredient)
            .where(Ingredient.id == ingredient_id)
            .values(**new_data)
        )
        await session.commit()

    @staticmethod
    async def delete(session: AsyncSession, ingredient_id: int):
        await session.execute(
            delete(Ingredient).where(Ingredient.id == ingredient_id)
        )
        await session.commit()