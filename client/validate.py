#!/usr/bin/env python3
import sys
import os
import json
import requests
import jwt
import logging

# ================= CONFIG =================

VALIDATION_HOST = os.getenv("VALIDATION_HOST", "host.docker.internal")
VALIDATION_PORT = os.getenv("VALIDATION_PORT", "9000")
BASE_URL = f"http://{VALIDATION_HOST}:{VALIDATION_PORT}"

TOKEN_FILE = "/home/client/.token.txt"

logging.basicConfig(
    level=logging.INFO,
    format="%(asctime)s INFO %(message)s"
)

def verificar_login():
    if not os.path.exists(TOKEN_FILE):
        print("Você precisa fazer login primeiro.")
        print("Execute: python auth.py login <CPF> <SENHA>")
        return False
    return True


def get_token():
    if not os.path.exists(TOKEN_FILE):
        return None
    with open(TOKEN_FILE, "r") as f:
        return f.read().strip()


def obter_info_usuario():
    token = get_token()
    if not token:
        return None
    try:
        return jwt.decode(token, options={"verify_signature": False})
    except Exception as e:
        print(f"Erro ao decodificar token: {e}")
        return None


def validar_role_convênio():
    info = obter_info_usuario()
    if not info:
        return False

    role = info.get("role")
    if role not in ["Admin", "Medico", "Recepcionista"]:
        print(f"Role '{role}' não pode validar convênio.")
        return False

    return True


def validar_role_pagamento_privado():
    info = obter_info_usuario()
    if not info:
        return False

    role = info.get("role")
    if role != "Paciente":
        print(f"Role '{role}' não pode autorizar pagamento privado.")
        return False

    return True


def request_post(endpoint, payload):
    if not verificar_login():
        return None

    token = get_token()
    headers = {
        "Authorization": f"Bearer {token}",
        "Content-Type": "application/json"
    }

    url = BASE_URL + endpoint
    logging.info(f"POST {url}")

    try:
        response = requests.post(url, headers=headers, json=payload, timeout=10)
        print(f"Status HTTP: {response.status_code}")
        return response
    except Exception as e:
        print(f"Erro ao chamar API: {e}")
        return None


def processar_resposta(response):
    if not response:
        return

    try:
        data = response.json()
    except Exception:
        print("Resposta inválida:", response.text)
        return

    if response.status_code != 200:
        print("Erro HTTP:", data.get("message"))
        return

    sucesso = data.get("success")
    if sucesso is None:
        sucesso = data.get("approved")

    if sucesso:
        print("Sucesso:", data.get("message"))
    else:
        print("Falha:", data.get("message"))

    print(json.dumps(data, indent=2, ensure_ascii=False))

def health_insurance(appointment_id, patient_id, insurance, procedure):
    if not verificar_login():
        return
    if not validar_role_convênio():
        return

    payload = {
        "appointmentId": int(appointment_id),
        "patientId": int(patient_id),
        "insuranceName": insurance,
        "procedure": procedure
    }

    response = request_post("/api/validation/health-insurance", payload)
    processar_resposta(response)


def private_payment(appointment_id, patient_id, amount, method):
    if not verificar_login():
        return
    if not validar_role_pagamento_privado():
        return

    payload = {
        "appointmentId": int(appointment_id),
        "patientId": int(patient_id),
        "amount": float(amount),
        "paymentMethod": method
    }

    response = request_post("/api/validation/private-payment", payload)
    processar_resposta(response)

def help():
    print("=" * 55)
    print("VALIDAÇÃO DE PAGAMENTO / CONVÊNIO")
    print("=" * 55)
    print("")
    print("Convênio (Admin | Médico | Recepcionista):")
    print("  python validate.py health-insurance <AGENDAMENTO_ID> <PACIENTE_ID> <CONVENIO> <PROCEDIMENTO>")
    print("")
    print("Pagamento privado (Paciente):")
    print("  python validate.py private-payment <AGENDAMENTO_ID> <PACIENTE_ID> <VALOR> <METODO>")
    print("  Métodos aceitos: PIX | CREDITO | DEBITO")
    print("=" * 55)

def main():
    if len(sys.argv) < 2:
        help()
        return

    comando = sys.argv[1].lower()

    if comando == "health-insurance":
        if len(sys.argv) < 6:
            help()
            return
        _, _, appt, patient, ins, proc = sys.argv
        health_insurance(appt, patient, ins, proc)

    elif comando == "private-payment":
        if len(sys.argv) < 6:
            help()
            return
        _, _, appt, patient, amount, method = sys.argv
        private_payment(appt, patient, amount, method)

    elif comando in ["help", "--help", "-h"]:
        help()

    else:
        print(f"Comando desconhecido: {comando}")
        help()

if __name__ == "__main__":
    main()