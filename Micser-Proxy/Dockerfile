FROM nginx:latest

# Добавим проверку
RUN echo "=== BEFORE COPY ===" && \
    ls -la /etc/nginx/ && \
    echo "==================="

COPY nginx.conf /etc/nginx/nginx.conf

RUN echo "=== AFTER COPY ===" && \
    cat /etc/nginx/nginx.conf && \
    echo "=================="

RUN rm -f /etc/nginx/conf.d/default.conf

EXPOSE 80 443
CMD ["nginx", "-g", "daemon off;"]