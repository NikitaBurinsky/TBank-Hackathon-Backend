"""
Minimal development stubs for `recipe_pb2_grpc` used by the example server/client.

These stubs provide small helpers so `server.py` and `client.py` can be imported
and used for simple, in-process testing. They DO NOT implement real gRPC
transport. For real networked RPC calls generate the proper code using
`grpc_tools.protoc` from the .proto definition.
"""
from typing import Any

# Store the registered servicer instance when add_RecipeServiceServicer_to_server
# is called by the example server. This allows very small local tests when the
# server and client run in the same interpreter, but it does NOT provide an RPC
# transport between processes.
_REGISTERED_SERVICER = None


class RecipeServiceServicer:
    """Base class for servicer (kept for symmetry with generated code)."""
    pass


def add_RecipeServiceServicer_to_server(servicer: RecipeServiceServicer, server: Any) -> None:
    """Register servicer for local/in-process test usage.

    This does not hook into grpc internals. It's intentionally minimal: it
    stores the servicer so tests can access it. For real gRPC, generate real
    code and use the gRPC runtime's add_* function.
    """
    global _REGISTERED_SERVICER
    _REGISTERED_SERVICER = servicer


class RecipeServiceStub:
    """A tiny stubbed client-side helper.

    NOTE: This is NOT a real network stub. If you create the stub with a
    `channel` and call its async methods they will raise unless you replace the
    implementation to call a real gRPC channel. This class exists so importers
    expecting `RecipeServiceStub(channel)` won't fail during local import.
    """

    def __init__(self, channel: Any):
        self._channel = channel

    async def CreateRecipe(self, request: Any, timeout: float | None = None):
        raise RuntimeError("RecipeServiceStub is a development stub and cannot perform network RPCs.")

    async def GetRecipe(self, request: Any, timeout: float | None = None):
        raise RuntimeError("RecipeServiceStub is a development stub and cannot perform network RPCs.")
