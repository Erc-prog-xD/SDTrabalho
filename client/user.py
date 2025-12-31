#!/usr/bin/env python3
import sys
import json
import requests
import os
import re
import base64
import random
from datetime import datetime
from pathlib import Path
import logging

# =========================================================
# CONFIG
# =========================================================
API_HOST = os.getenv("API_HOST", "localhost")
API_PORT = os.getenv("API_PORT", "8080")
BASE_URL = f"http://{API_HOST}:{API_PORT}/api"

OUTPUT_FILE = os.getenv("USERS_OUTPUT", "/home/client/created_users.json")
TOKEN_FILE = Path("/home/client/.token.txt")

logging.basicConfig(level=logging.INFO, format="%(asctime)s %(levelname)s %(message)s")
logger = logging.getLogger("users-client")

# =========================================================
# ROLE MAP / ENDPOINTS
# =========================================================
ROLE_MAP = {
    "PACIENTE": 0,
    "MEDICO": 1,
    "RECEPCIONISTA": 2,
    "ADMIN": 3
}

ROLE_ENDPOINT = {
    "PACIENTE": "paciente",
    "MEDICO": "Medico",
    "RECEPCIONISTA": "Recepcionista", 
    "ADMIN": "Admin"
}

ROLE_SUFFIX = {
    "PACIENTE": "Paciente",
    "MEDICO": "Medico",
    "RECEPCIONISTA": "Recepcionista",
    "ADMIN": "Admin"
}

# =========================================================
# CAMPOS PERMITIDOS
# =========================================================
VALID_FIELDS_BY_ROLE = {
    "PACIENTE": {"Nome", "Email", "Telefone", "DataNascimento", "Endereco", "HistoricoMedico", "Alergias"},
    "MEDICO": {"Nome", "Email", "Telefone", "CRM", "Especialidade"},
    "RECEPCIONISTA": {"Nome", "Email", "Telefone", "Turno"},
    "ADMIN": {"Nome", "Email", "Telefone"}
}

# =========================================================
# HELPERS
# =========================================================
def usage():
    print("""
Uso:
  python users.py criar <NOME> <SENHA> <ROLE>
  python users.py visualizar <ROLE> <ID>
  python users.py atualizar <ROLE> <ID> campo=valor [campo=valor ...]
  python users.py deletar   <ROLE> <ID>

Exemplo criar:
  python users.py criar "Filipe" "123" "Admin"
""")
    sys.exit(1)

def get_auth_headers():
    if not TOKEN_FILE.exists():
        return None
    token = TOKEN_FILE.read_text().strip()
    return {
        "Authorization": f"Bearer {token}",
        "Content-Type": "application/json"
    }

def parse_jwt(token):
    try:
        payload = token.split(".")[1]
        payload += "=" * (-len(payload) % 4)
        return json.loads(base64.urlsafe_b64decode(payload))
    except Exception:
        return {}

def get_logged_identity():
    headers = get_auth_headers()
    if headers is None:
        return None, None
    token = TOKEN_FILE.read_text().strip()
    claims = parse_jwt(token)
    try:
        return int(claims.get("id")), claims.get("role", "").upper()
    except Exception:
        return None, claims.get("role", "").upper()

def save_created_user(info: dict):
    try:
        path = Path(OUTPUT_FILE)
        path.parent.mkdir(parents=True, exist_ok=True)
        data = json.loads(path.read_text()) if path.exists() else []
        data.append(info)
        path.write_text(json.dumps(data, indent=2, ensure_ascii=False))
    except Exception as ex:
        logger.warning("Não foi possível salvar usuário localmente: %s", ex)

def gerar_cpf():
    # Gera um CPF mock (11 dígitos)
    return str(random.randint(10_000_000_00, 99_999_999_99)).zfill(11)

def gerar_email(nome):
    return f"{nome.lower().replace(' ', '_')}@clinica.com"

def gerar_telefone():
    return f"11{random.randint(900_000_000, 999_999_999)}"

# =========================================================
# VALIDAÇÕES
# =========================================================
def is_deleted_response(body):
    if not isinstance(body, dict):
        return True
    msg = (body.get("mensage") or "").lower()
    if body.get("status") is False:
        return True
    if any(k in msg for k in ["delet", "inativ", "não encontrado", "nao encontrado"]):
        return True
    return False

def validar_campo(role, campo, valor):
    if campo not in VALID_FIELDS_BY_ROLE[role]:
        return f"Campo '{campo}' não permitido para {role.lower()}."
    if campo == "Telefone" and (not valor.isdigit() or len(valor) not in (10, 11)):
        return "Telefone inválido."
    if campo == "Email" and not re.match(r"^[^@]+@[^@]+\.[^@]+$", valor):
        return "Email inválido."
    if campo == "CRM" and not re.match(r"^CRM-[A-Z]{2}-\d{4,6}$", valor):
        return "CRM inválido."
    if campo == "Turno" and valor.upper() not in ("MANHA", "TARDE", "NOITE"):
        return "Turno inválido."
    if campo == "DataNascimento":
        try:
            datetime.fromisoformat(valor)
        except ValueError:
            return "DataNascimento inválida."
    return None

# =========================================================
# ACTIONS
# =========================================================
def action_criar(nome, senha, role_str):
    """
    Criação simples (mock): não precisa estar logado.
    Usa endpoint /Usuario/Registrar e preenche campos mock dependendo da role.
    No final imprime o CPF gerado para login.
    """
    role_key = role_str.upper()
    if role_key not in ROLE_MAP:
        print("Role inválida.")
        sys.exit(1)

    payload = {
        "Cpf": gerar_cpf(),
        "Nome": nome,
        "Email": gerar_email(nome),
        "Telefone": gerar_telefone(),
        "Role": ROLE_MAP[role_key],
        "Senha": senha
    }

    if role_key == "PACIENTE":
        payload.update({
            "DataNascimento": "1995-05-10T00:00:00",
            "Endereco": "Rua Principal, 123",
            "HistoricoMedico": "Sem histórico relevante",
            "Alergias": "Nenhuma"
        })
    elif role_key == "MEDICO":
        payload.update({
            "CRM": f"CRM-CE-{random.randint(10000, 99999)}",
            "Especialidade": random.choice(["Clinico Geral", "Cardiologia", "Pediatria", "Ortopedia"])
        })
    elif role_key == "RECEPCIONISTA":
        payload.update({
            "Turno": random.choice(["MANHA", "TARDE", "NOITE"])
        })

    url = f"{BASE_URL}/Usuario/Registrar"
    headers = {"Content-Type": "application/json"}
    try:
        resp = requests.post(url, headers=headers, json=payload)
    except Exception as ex:
        print("Erro ao conectar com a API:", ex)
        sys.exit(1)

    # Mostrar status e resposta para transparência
    print("Status HTTP:", resp.status_code)
    try:
        body = resp.json()
        print(json.dumps(body, indent=2, ensure_ascii=False))
    except Exception:
        print(resp.text)

    # Sempre mostrar o CPF para que o usuário possa realizar login
    cpf = payload.get("Cpf")
    print("CPF do usuário criado (use para login):", cpf)

    save_created_user({
        "nome": nome,
        "cpf": cpf,
        "role": role_key,
        "createdAt": datetime.utcnow().isoformat()
    })

def action_visualizar(role_str, user_id):
    role = role_str.upper()
    logged_id, logged_role = get_logged_identity()
    if logged_role is None:
        print("Operação bloqueada. Faça login primeiro.")
        sys.exit(1)
    if logged_role != "ADMIN" and user_id != logged_id:
        print("Você só pode visualizar seu próprio perfil.")
        sys.exit(1)

    endpoint = ROLE_ENDPOINT[role]
    suffix = ROLE_SUFFIX[role]
    url = f"{BASE_URL}/{endpoint}/VisualizarPerfil{suffix}"
    resp = requests.get(url, headers=get_auth_headers(), params={"id": user_id})
    print("Status HTTP:", resp.status_code)
    body = resp.json() if resp.text else {}
    if is_deleted_response(body):
        print("Usuário deletado ou inexistente. Operação bloqueada.")
        sys.exit(1)
    returned_id = body.get("dados", {}).get("id")
    if returned_id != user_id:
        print("Role ou ID inválido.")
        sys.exit(1)
    print(json.dumps(body, indent=2, ensure_ascii=False))

def action_atualizar(role_str, user_id, pairs):
    role = role_str.upper()
    logged_id, logged_role = get_logged_identity()
    if logged_role is None:
        print("Operação bloqueada. Faça login primeiro.")
        sys.exit(1)
    if logged_role != "ADMIN" and user_id != logged_id:
        print("Não é permitido atualizar outro usuário.")
        sys.exit(1)

    payload = {"Id": user_id}
    for p in pairs:
        if "=" not in p:
            print(f"Formato inválido: '{p}', use campo=valor.")
            sys.exit(1)
        campo, valor = p.split("=", 1)
        campo = campo.strip()
        valor = valor.strip()
        erro = validar_campo(role, campo, valor)
        if erro:
            print(erro)
            sys.exit(1)
        payload[campo] = valor

    endpoint = ROLE_ENDPOINT[role]
    suffix = ROLE_SUFFIX[role]
    url = f"{BASE_URL}/{endpoint}/AtualizarPerfil{suffix}"
    resp = requests.put(url, headers=get_auth_headers(), json=payload)
    print("Status HTTP:", resp.status_code)
    body = resp.json() if resp.text else {}
    if is_deleted_response(body):
        print("Usuário deletado. Atualização bloqueada.")
        sys.exit(1)
    if resp.status_code != 200:
        print("Falha na atualização:", body)
        sys.exit(1)
    print("Atualização realizada.")

def action_deletar(role_str, user_id):
    role = role_str.upper()
    logged_id, logged_role = get_logged_identity()
    if logged_role is None:
        print("Operação bloqueada. Faça login primeiro.")
        sys.exit(1)
    if logged_role != "ADMIN" and user_id != logged_id:
        print("Não é permitido deletar outro usuário.")
        sys.exit(1)

    endpoint = ROLE_ENDPOINT[role]
    suffix = ROLE_SUFFIX[role]
    url = f"{BASE_URL}/{endpoint}/DeletarPerfil{suffix}"

    resp = requests.delete(url, headers=get_auth_headers(), params={"id": user_id})
    print("Status HTTP:", resp.status_code)
    body = resp.json() if resp.text else {}
    if resp.status_code != 200:
        print("Falha ao deletar:", body)
        sys.exit(1)
    print("Perfil deletado com sucesso.")

# =========================================================
# MAIN
# =========================================================
def main():
    if len(sys.argv) < 2:
        usage()
    cmd = sys.argv[1].lower()
    if cmd == "criar":
        if len(sys.argv) != 5:
            print("Uso: python users.py criar <NOME> <SENHA> <ROLE>")
            sys.exit(1)
        _, _, nome, senha, role = sys.argv
        action_criar(nome, senha, role)
    elif cmd == "visualizar":
        if len(sys.argv) != 4:
            usage()
        _, _, role, uid = sys.argv
        action_visualizar(role, int(uid))
    elif cmd == "atualizar":
        if len(sys.argv) < 5:
            usage()
        _, _, role, uid, *pairs = sys.argv
        action_atualizar(role, int(uid), pairs)
    elif cmd == "deletar":
        if len(sys.argv) != 4:
            usage()
        _, _, role, uid = sys.argv
        action_deletar(role, int(uid))
    else:
        usage()

if __name__ == "__main__":
    main()
