
import asyncio
import grpc
import os
import sys
from concurrent import futures

# Ensure Micser-Python package dir is on sys.path so imports like
# `service`, `ai`, `db` work when running from the repository root.
_THIS_DIR = os.path.dirname(__file__)
if _THIS_DIR not in sys.path:
    sys.path.insert(0, _THIS_DIR)

import recipe_pb2
import recipe_pb2_grpc

# Import your service layer
# from service.recipe_service import RecipeService
# For safety in this sample, I'll create a tiny in-memory fallback service if real one is absent.
try:
    from service.recipe_service import RecipeService  # user implementation (expected)
except Exception:
    # Minimal fallback service for testing without DB
    class _FakeRecipe:
        def __init__(self, id_, title, instructions, ingredients_amount):
            self.id = id_
            self.title = title
            self.instructions = instructions
            self.ingredients_amount = ingredients_amount

    class RecipeService:
        _STORE = {}
        _ID = 1

        @classmethod
        async def create(cls, data: dict):
            rid = cls._ID
            cls._ID += 1
            obj = _FakeRecipe(rid, data.get("title", ""), data.get("instructions", ""), data.get("ingredients_amount", {}))
            cls._STORE[rid] = obj
            return obj

        @classmethod
        async def get(cls, recipe_id: int):
            return cls._STORE.get(recipe_id)

# Implement the gRPC Servicer using asyncio (grpc.aio)
class RecipeServicer(recipe_pb2_grpc.RecipeServiceServicer):

    async def CreateRecipe(self, request, context):
        # Convert request to dict expected by service
        data = {
            "title": request.title,
            "instructions": request.instructions,
            "ingredients_amount": dict(request.ingredients_amount)
        }

        try:
            recipe = await RecipeService.create(data)
        except Exception as e:
            # On error return gRPC abort
            await context.abort(grpc.StatusCode.INTERNAL, f"create error: {e}")

        # Map returned object to proto response
        resp = recipe_pb2.RecipeResponse(
            id=int(recipe.id),
            title=str(recipe.title),
            instructions=str(recipe.instructions),
            ingredients_amount={k: float(v) for k, v in getattr(recipe, "ingredients_amount", {}).items()}
        )
        return resp

    async def GetRecipe(self, request, context):
        try:
            recipe = await RecipeService.get(request.id)
        except Exception as e:
            await context.abort(grpc.StatusCode.INTERNAL, f"get error: {e}")

        if not recipe:
            await context.abort(grpc.StatusCode.NOT_FOUND, f"recipe {request.id} not found")

        resp = recipe_pb2.RecipeResponse(
            id=int(recipe.id),
            title=str(recipe.title),
            instructions=str(recipe.instructions),
            ingredients_amount={k: float(v) for k, v in getattr(recipe, "ingredients_amount", {}).items()}
        )
        return resp


async def serve(host: str = "[::]", port: int = 50051):
    server = grpc.aio.server()
    recipe_pb2_grpc.add_RecipeServiceServicer_to_server(RecipeServicer(), server)
    bind_addr = f"{host}:{port}"
    server.add_insecure_port(bind_addr)
    print(f"Starting gRPC server on {bind_addr} ...")
    await server.start()
    try:
        await server.wait_for_termination()
    except asyncio.CancelledError:
        await server.stop(grace=None)


if __name__ == "__main__":
    try:
        asyncio.run(serve())
    except KeyboardInterrupt:
        print("Server stopped by user")
