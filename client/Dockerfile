FROM python:3.11-slim

WORKDIR /home/client

COPY requirements.txt .
RUN pip install --no-cache-dir -r requirements.txt

COPY *.py .
COPY help.sh .
COPY auth.py .
COPY token_utils.py .
COPY scheduling.py .

RUN chmod +x help.sh

ENV API_HOST=host.docker.internal
ENV API_PORT=8080
ENV RABBIT_HOST=host.docker.internal
ENV RABBIT_PORT=5672
ENV RABBIT_USER=guest
ENV RABBIT_PASS=guest

CMD ["/bin/bash"]