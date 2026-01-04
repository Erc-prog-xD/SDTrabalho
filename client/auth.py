import sys
import requests
import os

API_HOST = os.getenv('API_HOST', 'localhost')
API_PORT = os.getenv('API_PORT', '8080')
BASE_URL = f"http://{API_HOST}:{API_PORT}"
TOKEN_FILE = '/home/client/.token.txt'

def login(cpf, senha):
    """Faz login e salva o token JWT"""
    print(f"Tentando login com CPF: {cpf}")
    url = f"{BASE_URL}/api/Usuario/Login"
    payload = {"cpf": cpf, "senha": senha}
    
    try:
        response = requests.post(url, json=payload, timeout=10)
        print(f"Status API: {response.status_code}")
        
        data = response.json()
        print(f"Resposta API: {data}")
        
        if data.get('status') == True:
            token = data['dados']
            with open(TOKEN_FILE, 'w') as f:
                f.write(token)
            print("Login realizado com sucesso!")
            print(f"Token salvo em: {TOKEN_FILE}")
            print(f"Token (início): {token[:30]}...")
            return True
        else:
            print(f"Falha no login: {data.get('mensage', 'Erro desconhecido')}")
            return False
    except Exception as e:
        print(f"Erro de conexão: {e}")
        print(f"   Verifique se o backend está rodando em {BASE_URL}")
        return False

def logout():
    """Remove o token (logout)"""
    if os.path.exists(TOKEN_FILE):
        os.remove(TOKEN_FILE)
        print("Logout realizado. Token removido.")
    else:
        print("Você já não está autenticado.")

def status():
    """Verifica status da autenticação"""
    if os.path.exists(TOKEN_FILE):
        with open(TOKEN_FILE, 'r') as f:
            token = f.read().strip()
        print(f"AUTENTICADO")
        print(f"Token (início): {token[:30]}...")
        print(f"Tamanho do token: {len(token)} caracteres")
    else:
        print("NÃO AUTENTICADO")
        print("   Execute: python auth.py login <CPF> <SENHA>")

def mostrar_ajuda():
    """Mostra ajuda completa"""
    print("=" * 50)
    print("SISTEMA DE AUTENTICAÇÃO")
    print("=" * 50)
    print("\nCOMANDOS DISPONÍVEIS:")
    print("  python auth.py login <CPF> <SENHA>")
    print("  python auth.py logout")
    print("  python auth.py status")
    print("\nEXEMPLOS:")
    print("  python auth.py login 12345678900 senha123")
    print("  python auth.py status")
    print("  python auth.py logout")
    print("=" * 50)

def main():
    if len(sys.argv) < 2:
        mostrar_ajuda()
        return
    
    comando = sys.argv[1].lower()
    
    # Processa cada comando CORRETAMENTE
    if comando == "login":
        if len(sys.argv) != 4:
            print("Uso incorreto!")
            print("Correto: python auth.py login <CPF> <SENHA>")
            print("Exemplo: python auth.py login 12345678900 senha123")
        else:
            login(sys.argv[2], sys.argv[3])
    
    elif comando == "logout":
        logout()
    
    elif comando == "status":
        status()
    
    elif comando in ["help", "ajuda", "--help", "-h"]:
        mostrar_ajuda()
    
    else:
        print(f"Comando desconhecido: '{comando}'")
        print("Comandos válidos: login, logout, status, help")
        mostrar_ajuda()

if __name__ == "__main__":
    main()