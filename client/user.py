import sys
import json
import random
import requests
import logging
import os
from datetime import datetime

API_HOST = os.getenv('API_HOST', 'localhost')
API_PORT = os.getenv('API_PORT', '8080')
API_URL = f"http://{API_HOST}:{API_PORT}/api/Usuario/Registrar"

HEADERS = {
    "Content-Type": "application/json"
}

logging.basicConfig(
    level=logging.INFO,
    format="%(asctime)s %(levelname)s %(message)s"
)
logger = logging.getLogger("users-client")

ROLE_MAP = {
    "PACIENTE": 0,  
    "MEDICO": 1,      
    "RECEPCIONISTA": 2, 
    "ADMIN": 3        
}

def gerar_cpf():
    return str(random.randint(10000000000, 99999999999))

def gerar_email(nome):
    return f"{nome.lower().replace(' ', '_')}@clinica.com"

def gerar_telefone():
    return f"11{random.randint(900000000, 999999999)}"

def erro_uso():
    print("\nUso correto:")
    print("  python users.py criar <NOME> <SENHA> <ROLE>")
    print("\nExemplos:")
    print("  python users.py criar Filipe 123 Admin")
    print("  python users.py criar Joao 123 Paciente")
    print("  python users.py criar Maria 123 Medico")
    print("  python users.py criar Ana 123 Recepcionista")
    print(f"\nAPI URL configurada: {API_URL}")
    print("Use variáveis de ambiente para mudar:")
    print("  API_HOST=host.docker.internal API_PORT=8080 python users.py ...")
    sys.exit(1)

def montar_payload(nome, senha, role_str):
    tipo_usuario = ROLE_MAP[role_str]
    
    payload = {
        "cpf": gerar_cpf(),
        "nome": nome,
        "email": gerar_email(nome),
        "telefone": gerar_telefone(),
        "role": tipo_usuario, 
        "senha": senha 
    }

    if role_str == "PACIENTE":
        payload.update({
            "dataNascimento": "1995-05-10T00:00:00",
            "endereco": "Rua Principal, 123",
            "historicoMedico": "Sem histórico relevante",
            "alergias": "Nenhuma"
        })

    elif role_str == "MEDICO":
        payload.update({
            "crm": f"CRM-CE-{random.randint(10000, 99999)}",
            "especialidade": random.choice(["Clinico Geral", "Cardiologia", "Pediatria", "Ortopedia"])
        })

    elif role_str == "RECEPCIONISTA":
        payload.update({
            "turno": random.choice(["MANHA", "TARDE", "NOITE"])
        })

    return payload

def main():
    if len(sys.argv) != 5:
        erro_uso()

    acao = sys.argv[1].lower()
    nome = sys.argv[2]
    senha = sys.argv[3]
    role_str = sys.argv[4].upper()

    if acao != "criar":
        logger.error("Ação inválida. Use apenas 'criar'.")
        erro_uso()

    if role_str not in ROLE_MAP:
        logger.error("Role inválida. Use: Paciente | Medico | Recepcionista | Admin")
        erro_uso()

    logger.info("Conectando em: %s", API_URL)
    payload = montar_payload(nome, senha, role_str)

    logger.info("Criando usuário %s (%s)", nome, role_str)
    logger.debug("Payload enviado: %s", json.dumps(payload, indent=2))

    try:
        response = requests.post(
            API_URL,
            headers=HEADERS,
            data=json.dumps(payload),
            timeout=10
        )
        
        logger.info("Status HTTP: %s", response.status_code)
        
        if response.status_code == 200:
            print(f"Usuário '{nome}' criado com sucesso!")
            if response.text:
                try:
                    print(json.dumps(response.json(), indent=2, ensure_ascii=False))
                    print(f"CPF gerado: {payload['cpf']}")
                    print("Anote este CPF para fazer login!")
                except:
                    print(f"Resposta: {response.text}")
        else:
            print(f"Erro {response.status_code}: {response.text}")
            logger.error("Erro na resposta: %s", response.text)
            
    except requests.exceptions.ConnectionError as ex:
        logger.error("Erro ao conectar à API: %s", ex)
        print(f"Não foi possível conectar à API em {API_URL}")
        sys.exit(1)
    except Exception as ex:
        logger.error("Erro inesperado: %s", ex)
        print(f"Erro: {ex}")
        sys.exit(1)

if __name__ == "__main__":
    main()