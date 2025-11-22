# client.py
# Uploaded file (local path): file:///mnt/data/4288e4f8-12e3-4cc5-b950-ced1c2b7ac1f.png
#
# Async gRPC client example for RecipeService.
# Run: python client.py

import asyncio
import grpc

import recipe_pb2
import recipe_pb2_grpc

async def create_and_get_recipe(host: str = "localhost", port: int = 50051):
    addr = f"{host}:{port}"
    # create an insecure async channel
    async with grpc.aio.insecure_channel(addr) as channel:
        stub = recipe_pb2_grpc.RecipeServiceStub(channel)

        # Example CreateRecipe call
        create_req = recipe_pb2.RecipeRequest(
            title="Пример: овсяная каша",
            instructions="Смешать ингредиенты, нагреть 5 минут.",
            ingredients_amount={"овсянка": 50.0, "молоко": 200.0}
        )

        try:
            create_resp = await stub.CreateRecipe(create_req, timeout=10.0)
            print("CreateRecipe response:", create_resp)
        except grpc.RpcError as e:
            print("CreateRecipe failed:", e)
            return

        recipe_id = create_resp.id

        # Example GetRecipe call
        try:
            get_req = recipe_pb2.GetRecipeRequest(id=recipe_id)
            get_resp = await stub.GetRecipe(get_req, timeout=5.0)
            print("GetRecipe response:", get_resp)
        except grpc.RpcError as e:
            print("GetRecipe failed:", e)

if __name__ == "__main__":
    asyncio.run(create_and_get_recipe())
