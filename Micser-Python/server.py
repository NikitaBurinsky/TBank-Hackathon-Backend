import asyncio
import grpc
import os
import sys

# Ensure package imports work when running from repo root
_THIS_DIR = os.path.dirname(__file__)
if _THIS_DIR not in sys.path:
	sys.path.insert(0, _THIS_DIR)

import recipe_pb2
import recipe_pb2_grpc

from ai.parser_ai1 import reciept as llm_reciept
from ai.normalizer_ai2 import ingredients_nutrition as llm_ingredients


class ReceiptServicer(recipe_pb2_grpc.ReceiptServiceServicer):
	async def GetReceiptInfo(self, request, context):
		query = request.receiptQuery or ""
		try:
			# Parse recipe (LLM)
			recipe_json = await llm_reciept(query)
			# Produce nutrition list (LLM)
			ingredients_json = await llm_ingredients(recipe_json)
		except Exception as e:
			await context.abort(grpc.StatusCode.INTERNAL, f"LLM error: {e}")

		return recipe_pb2.ReceiptResponse(ingredientsJson=ingredients_json, receiptJson=recipe_json)


async def serve(host: str = "[::]", port: int = 50051):
	server = grpc.aio.server()
	recipe_pb2_grpc.add_ReceiptServiceServicer_to_server(ReceiptServicer(), server)
	bind_addr = f"{host}:{port}"
	server.add_insecure_port(bind_addr)
	print(f"Starting gRPC server on {bind_addr} ...")
	await server.start()
	try:
		await server.wait_for_termination()
	except asyncio.CancelledError:
		await server.stop(grace=None)


if __name__ == '__main__':
	try:
		asyncio.run(serve())
	except KeyboardInterrupt:
		print("Server stopped by user")

