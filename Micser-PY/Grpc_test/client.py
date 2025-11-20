import grpc

import calc_pb2
import calc_pb2_grpc


def run():
    with grpc.insecure_channel("localhost:50051") as channel:
        stub = calc_pb2_grpc.CalculatorStub(channel)
        a = float(input("Enter a number: "))
        b= float(input("Enter b number: "))
        request = calc_pb2.AddRequest(a=a, b=b)
        response = stub.Add(request)
        print("Result from server:", response.result)


if __name__ == "__main__":
    run()
