import grpc
from concurrent import futures

import calc_pb2
import calc_pb2_grpc




class CalculatorService(calc_pb2_grpc.CalculatorServicer):
    def Add(self, request, context):
        result = request.a + request.b
        print(f"Got request: a={request.a}, b={request.b}, result={result}")
        return calc_pb2.AddReply(result=result)


def serve():
    server = grpc.server(futures.ThreadPoolExecutor(max_workers=10))
    calc_pb2_grpc.add_CalculatorServicer_to_server(CalculatorService(), server)
    server.add_insecure_port("[::]:50051")
    print("gRPC server listening on 50051...")
    server.start()
    server.wait_for_termination()


if __name__ == "__main__":
    serve()
