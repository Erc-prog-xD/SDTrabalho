import os
import requests
import json

API_HOST = os.getenv('API_HOST', 'localhost')
API_PORT = os.getenv('API_PORT', '8080')
BASE_URL = f"http://{API_HOST}:{API_PORT}"
TOKEN_FILE = '/home/client/.token.txt'

def get_token():
    """Retorna o token JWT se existir"""
    if not os.path.exists(TOKEN_FILE):
        return None
    with open(TOKEN_FILE, 'r') as f:
        return f.read().strip()

def save_token(token):
    """Salva token em arquivo"""
    with open(TOKEN_FILE, 'w') as f:
        f.write(token)

def remove_token():
    """Remove token (logout)"""
    if os.path.exists(TOKEN_FILE):
        os.remove(TOKEN_FILE)

def make_request(method, endpoint, data=None):
    """
    Faz requisição autenticada à API.
    Retorna: objeto Response ou None
    """
    token = get_token()
    
    public_endpoints = ['/api/Usuario/Login', '/api/Usuario/Registrar']
    
    if not token and endpoint not in public_endpoints:
        print("Você precisa fazer login primeiro!")
        print("   Execute: python auth.py login <CPF> <SENHA>")
        return None
    
    headers = {'Content-Type': 'application/json'}
    if token:
        headers['Authorization'] = f'Bearer {token}'
    
    url = BASE_URL + endpoint
    
    try:
        if method == 'GET':
            response = requests.get(url, headers=headers, timeout=10)
        elif method == 'POST':
            response = requests.post(url, headers=headers, json=data, timeout=10)
        elif method == 'PUT':
            response = requests.put(url, headers=headers, json=data, timeout=10)
        elif method == 'DELETE':
            response = requests.delete(url, headers=headers, timeout=10)
        else:
            print(f"Método inválido: {method}")
            return None
        
        return response
        
    except requests.exceptions.ConnectionError:
        print(f"Não foi possível conectar à API em {url}")
        return None
    except Exception as e:
        print(f"Erro: {e}")
        return None