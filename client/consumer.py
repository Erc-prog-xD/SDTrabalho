import os
import json
import logging
import time
import pika
from datetime import datetime

RABBIT_HOST = os.getenv("RABBIT_HOST", "localhost")
RABBIT_PORT = int(os.getenv("RABBIT_PORT", "5672"))
RABBIT_USER = os.getenv("RABBIT_USER", "guest")
RABBIT_PASS = os.getenv("RABBIT_PASS", "guest")

EXCHANGE = os.getenv("RABBIT_EXCHANGE", "notifications.x")
EXCHANGE_TYPE = os.getenv("RABBIT_EXCHANGE_TYPE", "fanout")

QUEUE = os.getenv("RABBIT_QUEUE", "notifications.client.q")

DATA_DIR = "/data"
LOG_FILE = f"{DATA_DIR}/notifications.log"

logging.basicConfig(
    level=logging.INFO,
    format="%(asctime)s %(levelname)s %(message)s"
)

logger = logging.getLogger("consumer")
logger.info(
    "Config RABBIT_HOST=%s RABBIT_PORT=%s RABBIT_USER=%s EXCHANGE=%s QUEUE=%s",
    RABBIT_HOST, RABBIT_PORT, RABBIT_USER, EXCHANGE, QUEUE
)

def ensure_data_dir():
    os.makedirs(DATA_DIR, exist_ok=True)

def process_message(msg: dict):

    notification = {
        "id": msg.get("Id"),
        "appointmentId": msg.get("AppointmentId"),
        "patientId": msg.get("PatientId"),
        "doctorId": msg.get("DoctorId"),
        "status": msg.get("Status"),
        "message": msg.get("Message"),
        "createdAt": msg.get("CreatedAt"),
        "receivedAt": datetime.utcnow().isoformat()
    }
    logger.info(
        "NOTIFICAÇÃO RECEBIDA | Paciente=%s | Consulta=%s | Status=%s | Msg=%s",
        notification["patientId"],
        notification["appointmentId"],
        notification["status"],
        notification["message"]
    )

    os.makedirs("/app/data", exist_ok=True)

    with open("/app/data/notifications.log", "a") as f:
        f.write(json.dumps(msg) + "\n")

    time.sleep(0.3)
    return True


def main():
    ensure_data_dir()

    params = pika.ConnectionParameters(
        host=RABBIT_HOST,
        port=RABBIT_PORT,
        credentials=pika.PlainCredentials(RABBIT_USER, RABBIT_PASS),
        heartbeat=60
    )

    for attempt in range(1, 31):
        try:
            logger.info("Tentando conectar ao RabbitMQ (tentativa %s)...", attempt)
            connection = pika.BlockingConnection(params)
            break
        except pika.exceptions.AMQPConnectionError:
            logger.warning("RabbitMQ indisponível, aguardando...")
            time.sleep(2)
    else:
        raise RuntimeError("Não foi possível conectar ao RabbitMQ")

    channel = connection.channel()

    channel.exchange_declare(exchange=EXCHANGE, exchange_type=EXCHANGE_TYPE, durable=True)
    channel.queue_declare(queue=QUEUE, durable=True)

    channel.queue_bind(queue=QUEUE, exchange=EXCHANGE)

    channel.basic_qos(prefetch_count=1)

    def callback(ch, method, properties, body):
        try:
            msg = json.loads(body.decode("utf-8"))
            process_message(msg)
            ch.basic_ack(delivery_tag=method.delivery_tag)
            logger.info("Mensagem processada com sucesso (Id=%s)", msg.get("Id"))
        except Exception as ex:
            logger.exception("Erro ao processar mensagem: %s", ex)
            ch.basic_nack(delivery_tag=method.delivery_tag, requeue=False)

    channel.basic_consume(queue=QUEUE, on_message_callback=callback)
    logger.info("Consumer pronto. Aguardando notificações... (queue=%s exchange=%s)", QUEUE, EXCHANGE)
    try:
        channel.start_consuming()
    except KeyboardInterrupt:
        logger.info("Encerrando consumer...")
    finally:
        if not connection.is_closed:
            connection.close()

if __name__ == "__main__":
    main()
