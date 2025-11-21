FROM python:3.11-slim

WORKDIR /app

# Устанавливаем системные зависимости
RUN apt-get update && apt-get install -y \
    gcc \
    && rm -rf /var/lib/apt/lists/*

# Копируем requirements.txt если есть, или создаем его
#COPY requirements.txt .

# Устанавливаем Python зависимости
#RUN pip install --no-cache-dir -r requirements.txt

# Копируем исходный код
COPY Micser-Py/Grpc_test/ .

# Генерируем gRPC файлы если нужно (опционально)
#RUN python -m grpc_tools.protoc \
#    -I./protos \
#    --python_out=. \
#    --grpc_python_out=. \
#    ./protos/calc.proto

# Открываем порт для gRPC
EXPOSE 5000

# Запускаем сервер
CMD ["python", "server.py"]