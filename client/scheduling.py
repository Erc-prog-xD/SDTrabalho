import sys
import json
import requests
import os
import jwt

API_HOST = os.getenv('API_HOST', 'localhost')
API_PORT = os.getenv('API_PORT', '8080')
BASE_URL = f"http://{API_HOST}:{API_PORT}"
TOKEN_FILE = '/home/client/.token.txt'

def verificar_login():
    """Verifica se o usuario esta logado"""
    if not os.path.exists(TOKEN_FILE):
        print("ERRO: Voce precisa fazer login primeiro!")
        print("   Execute: python auth.py login <CPF> <SENHA>")
        return False
    return True

def get_token():
    """Retorna o token JWT"""
    if not os.path.exists(TOKEN_FILE):
        return None
    with open(TOKEN_FILE, 'r') as f:
        return f.read().strip()

def obter_info_usuario():
    """Obtem informacoes do usuario a partir do token JWT"""
    token = get_token()
    if not token:
        return None
    
    try:
        decoded = jwt.decode(token, options={"verify_signature": False})
        return decoded
    except Exception as e:
        print(f"Erro ao decodificar token: {e}")
        return None

def validar_permissao_paciente(paciente_id, usuario_info):
    """Valida se o usuario tem permissao para acessar dados do paciente"""
    if not usuario_info:
        return False
    
    role = usuario_info.get('role')
    usuario_id = usuario_info.get('id')
    
    if role in ['Admin', 'Medico', 'Recepcionista']:
        return True
    
    if role == 'Paciente':
        return str(usuario_id) == str(paciente_id)
    
    return False

def request(method, endpoint, data=None):
    """Faz requisicao autenticada"""
    if not verificar_login():
        return None
    
    token = get_token()
    if not token:
        print("Token nao encontrado. Faca login novamente.")
        return None
    
    headers = {
        'Authorization': f'Bearer {token}',
        'Content-Type': 'application/json'
    }
    
    url = BASE_URL + endpoint
    
    try:
        if method == 'GET':
            response = requests.get(url, headers=headers, timeout=10)
        elif method == 'POST':
            response = requests.post(url, headers=headers, json=data, timeout=10)
        elif method == 'PATCH':
            response = requests.patch(url, headers=headers, json=data, timeout=10)
        elif method == 'DELETE':
            response = requests.delete(url, headers=headers, timeout=10)
        else:
            print(f"Metodo invalido: {method}")
            return None
        
        return response
        
    except requests.exceptions.ConnectionError:
        print(f"Nao foi possivel conectar a API em {url}")
        print("   Verifique se o backend esta rodando.")
        return None
    except Exception as e:
        print(f"Erro: {e}")
        return None

def processar_resposta(response):
    """Processa a resposta da API e trata erros"""
    if not response:
        return None
    
    try:
        data = response.json()
        
        if isinstance(data, dict):
            if 'status' in data and not data['status']:
                print(f"ERRO: {data.get('mensage', 'Erro desconhecido')}")
                return None
            
            if 'dados' in data and data['dados'] is None and data.get('mensage'):
                print(f"AVISO: {data['mensage']}")
                return None
        
        return data
    except json.JSONDecodeError:
        print(f"Resposta invalida da API: {response.text}")
        return None


def criar(patient_id, doctor_id, specialty, datetime_str):
    """Cria um novo agendamento"""
    if not verificar_login():
        return
    
    usuario_info = obter_info_usuario()
    if not usuario_info:
        print("Nao foi possivel obter informacoes do usuario.")
        return
    
    if not validar_permissao_paciente(patient_id, usuario_info):
        role = usuario_info.get('role', 'Desconhecido')
        if role == 'Paciente':
            print("ERRO: Pacientes so podem criar agendamentos para si mesmos.")
        else:
            print(f"ERRO: Usuario com role '{role}' nao tem permissao para criar agendamento.")
        return
    
    print(f"Criando agendamento...")
    print(f"   Paciente: {patient_id}")
    print(f"   Medico: {doctor_id}")
    print(f"   Especialidade: {specialty}")
    print(f"   Data/Hora: {datetime_str}")
    
    payload = {
        "patientId": int(patient_id),
        "doctorId": int(doctor_id),
        "specialty": specialty,
        "datetime": datetime_str
    }
    
    response = request('POST', '/api/scheduling', payload)
    
    if response:
        print(f"Status HTTP: {response.status_code}")
        
        data = processar_resposta(response)
        
        if data:
            if isinstance(data, dict) and data.get('status') == True:
                print("Agendamento criado com sucesso!")
        else:
            print(f"Resposta: {response.text}")
    else:
        print("Nao houve resposta da API.")

def listar_paciente(patient_id):
    """Lista agendamentos de um paciente especifico"""
    if not verificar_login():
        return
    
    usuario_info = obter_info_usuario()
    if not usuario_info:
        print("Nao foi possivel obter informacoes do usuario.")
        return
    
    if not validar_permissao_paciente(patient_id, usuario_info):
        role = usuario_info.get('role', 'Desconhecido')
        if role == 'Paciente':
            print("ERRO: Pacientes so podem visualizar seus proprios agendamentos.")
        else:
            print(f"ERRO: Usuario com role '{role}' nao tem permissao para visualizar agendamentos deste paciente.")
        return
    
    print(f"Buscando agendamentos do paciente {patient_id}...")
    
    response = request('GET', f'/api/scheduling/patient/{patient_id}')
    
    if response:
        print(f"Status HTTP: {response.status_code}")
        
        data = processar_resposta(response)
        
        if data:
            if isinstance(data, list):
                if len(data) > 0:
                    print(f"Encontrados {len(data)} agendamento(s):")
                    for i, agendamento in enumerate(data, 1):
                        print(f"\nAgendamento {i}:")
                        print(f"   ID: {agendamento.get('id', 'N/A')}")
                        print(f"   Medico ID: {agendamento.get('doctorId', 'N/A')}")
                        print(f"   Especialidade: {agendamento.get('specialty', 'N/A')}")
                        print(f"   Data/Hora: {agendamento.get('datetime', 'N/A')}")
                        print(f"   Status: {agendamento.get('status', 'N/A')}")
                else:
                    print("Nenhum agendamento encontrado para este paciente.")
            else:
                print("Resposta inesperada da API.")
                print(json.dumps(data, indent=2, ensure_ascii=False))
    else:
        print("Nao houve resposta da API.")

def listar_medico(doctor_id):
    """Lista agendamentos de um medico especifico"""
    if not verificar_login():
        return
    
    usuario_info = obter_info_usuario()
    if not usuario_info:
        print("Nao foi possivel obter informacoes do usuario.")
        return
    
    # Valida se e medico ou tem permissao
    role = usuario_info.get('role')
    if role not in ['Medico', 'Admin', 'Recepcionista']:
        print(f"ERRO: Usuario com role '{role}' nao tem permissao para visualizar agendamentos de medicos.")
        return
    
    # Se for medico, so pode ver seus proprios agendamentos
    if role == 'Medico':
        usuario_id = usuario_info.get('id')
        if str(usuario_id) != str(doctor_id):
            print("ERRO: Medicos so podem visualizar seus proprios agendamentos.")
            return
    
    print(f"Buscando agendamentos do medico {doctor_id}...")
    
    response = request('GET', f'/api/scheduling/doctor/{doctor_id}')
    
    if response:
        print(f"Status HTTP: {response.status_code}")
        
        data = processar_resposta(response)
        
        if data:
            if isinstance(data, list):
                if len(data) > 0:
                    print(f"Encontrados {len(data)} agendamento(s):")
                    for i, agendamento in enumerate(data, 1):
                        print(f"\nAgendamento {i}:")
                        print(f"   ID: {agendamento.get('id', 'N/A')}")
                        print(f"   Paciente ID: {agendamento.get('patientId', 'N/A')}")
                        print(f"   Especialidade: {agendamento.get('specialty', 'N/A')}")
                        print(f"   Data/Hora: {agendamento.get('datetime', 'N/A')}")
                        print(f"   Status: {agendamento.get('status', 'N/A')}")
                else:
                    print("Nenhum agendamento encontrado para este medico.")
            else:
                print("Resposta inesperada da API.")
                print(json.dumps(data, indent=2, ensure_ascii=False))
        # Se data for None, o erro ja foi mostrado em processar_resposta
    else:
        print("Nao houve resposta da API.")

def atualizar_status(appointment_id, new_status):
    """Atualiza status de um agendamento"""
    if not verificar_login():
        return
    
    # Obtem informacoes do usuario logado
    usuario_info = obter_info_usuario()
    if not usuario_info:
        print("Nao foi possivel obter informacoes do usuario.")
        return
    
    # Valida permissao (somente funcionarios podem atualizar status)
    role = usuario_info.get('role')
    if role not in ['Medico', 'Recepcionista', 'Admin']:
        print(f"ERRO: Usuario com role '{role}' nao tem permissao para atualizar status de agendamentos.")
        return
    
    print(f"Atualizando status do agendamento {appointment_id} para '{new_status}'...")
    
    try:
        status_int = int(new_status)
    except ValueError:
        print(f"ERRO: newStatus deve ser um numero inteiro")
        print("Valores comuns: 0=Agendado, 1=Confirmado, 2=Cancelado, 3=Concluido")
        return
    
    response = request('PATCH', f'/api/scheduling/{appointment_id}/status/{status_int}')
    
    if response:
        print(f"Status HTTP: {response.status_code}")
        
        data = processar_resposta(response)
        
        if data:
            if isinstance(data, dict) and data.get('status') == True:
                print("Status atualizado com sucesso!")
            # Se data for None, o erro ja foi mostrado em processar_resposta
        else:
            print(f"Resposta: {response.text}")
    else:
        print("Nao houve resposta da API.")

def cancelar(appointment_id):
    """Cancela/remove um agendamento"""
    if not verificar_login():
        return
    
    print(f"Cancelando agendamento {appointment_id}...")
    
    response = request('DELETE', f'/api/scheduling/{appointment_id}')
    
    if response:
        print(f"Status HTTP: {response.status_code}")
        
        data = processar_resposta(response)
        
        if data:
            if isinstance(data, dict) and data.get('status') == True:
                print("Agendamento cancelado com sucesso!")
            # Se data for None, o erro ja foi mostrado em processar_resposta
        else:
            print(f"Resposta: {response.text}")
    else:
        print("Nao houve resposta da API.")

# ================== MAIN ==================

def mostrar_ajuda():
    """Mostra ajuda completa"""
    print("=" * 60)
    print("SISTEMA DE AGENDAMENTOS")
    print("=" * 60)
    print("\nCOMANDOS DISPONIVEIS:")
    print("\nCRIAR AGENDAMENTO:")
    print("  python scheduling.py criar <PACIENTE_ID> <MEDICO_ID> <ESPECIALIDADE> <DATETIME>")
    print('  Exemplo: python scheduling.py criar 1 1 "Cardiologia" "2026-01-10T14:00:00"')
    
    print("\nLISTAR AGENDAMENTOS:")
    print("  python scheduling.py paciente <PACIENTE_ID>")
    print("  python scheduling.py medico <MEDICO_ID>")
    print("  Exemplo: python scheduling.py paciente 1")
    
    print("\nATUALIZAR STATUS:")
    print("  python scheduling.py status <AGENDAMENTO_ID> <NOVO_STATUS>")
    print('  Exemplo: python scheduling.py status 1 "1"')
    print("  Status (valores inteiros): 0=Agendado, 1=Confirmado, 2=Cancelado, 3=Concluido")
    
    print("\nCANCELAR AGENDAMENTO:")
    print("  python scheduling.py cancelar <AGENDAMENTO_ID>")
    print("  Exemplo: python scheduling.py cancelar 1")
    
    print("\nPERMISSOES:")
    print("  - Pacientes: so podem criar/listar seus proprios agendamentos")
    print("  - Medicos: podem listar seus agendamentos e atualizar status")
    print("  - Recepcionistas/Admins: podem gerenciar todos os agendamentos")
    
    print("\nPRE-REQUISITO: Faca login primeiro!")
    print("  python auth.py login <CPF> <SENHA>")
    print("=" * 60)

def main():
    if len(sys.argv) < 2:
        mostrar_ajuda()
        return
    
    comando = sys.argv[1].lower()
    
    if comando == "criar":
        if len(sys.argv) < 6:
            print("Uso incorreto!")
            print("Correto: python scheduling.py criar <PACIENTE_ID> <MEDICO_ID> <ESPECIALIDADE> <DATETIME>")
            print('Exemplo: python scheduling.py criar 1 1 "Cardiologia" "2026-01-10T14:00:00"')
            return
        
        patient_id = sys.argv[2]
        doctor_id = sys.argv[3]
        specialty = sys.argv[4]
        datetime_str = sys.argv[5]
        
        criar(patient_id, doctor_id, specialty, datetime_str)
    
    elif comando == "paciente":
        if len(sys.argv) != 3:
            print("Uso incorreto!")
            print("Correto: python scheduling.py paciente <PACIENTE_ID>")
            print("Exemplo: python scheduling.py paciente 1")
            return
        
        listar_paciente(sys.argv[2])
    
    elif comando == "medico":
        if len(sys.argv) != 3:
            print("Uso incorreto!")
            print("Correto: python scheduling.py medico <MEDICO_ID>")
            print("Exemplo: python scheduling.py medico 1")
            return
        
        listar_medico(sys.argv[2])
    
    elif comando == "status":
        if len(sys.argv) != 4:
            print("Uso incorreto!")
            print("Correto: python scheduling.py status <AGENDAMENTO_ID> <NOVO_STATUS>")
            print('Exemplo: python scheduling.py status 1 "1"')
            print('Status (valores inteiros): 0=Agendado, 1=Confirmado, 2=Cancelado, 3=Concluido')
            return
        
        atualizar_status(sys.argv[2], sys.argv[3])
    
    elif comando == "cancelar":
        if len(sys.argv) != 3:
            print("Uso incorreto!")
            print("Correto: python scheduling.py cancelar <AGENDAMENTO_ID>")
            print("Exemplo: python scheduling.py cancelar 1")
            return
        
        cancelar(sys.argv[2])
    
    elif comando in ["help", "ajuda", "--help", "-h"]:
        mostrar_ajuda()
    
    else:
        print(f"Comando desconhecido: '{comando}'")
        mostrar_ajuda()

if __name__ == "__main__":
    main()