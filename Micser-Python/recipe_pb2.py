"""
Minimal development stubs for `recipe_pb2` used by the example server/client.

WARNING: These are NOT real protobuf-generated classes and DO NOT provide a
real gRPC transport. They only exist so the example server/client code can be
imported and used for lightweight local testing. For production or real gRPC
calls, generate proper modules with `grpc_tools.protoc` from the .proto files.
"""
from typing import Dict, Any


class _BaseMessage:
    def __init__(self, **kwargs):
        for k, v in kwargs.items():
            setattr(self, k, v)

    def __repr__(self) -> str:  # pragma: no cover - convenience only
        fields = ", ".join(f"{k}={v!r}" for k, v in self.__dict__.items())
        return f"{self.__class__.__name__}({fields})"


class RecipeRequest(_BaseMessage):
    # expected fields: title (str), instructions (str), ingredients_amount (dict)
    pass


class GetRecipeRequest(_BaseMessage):
    # expected fields: id (int)
    pass


class RecipeResponse(_BaseMessage):
    # expected fields: id (int), title (str), instructions (str), ingredients_amount (dict)
    pass
