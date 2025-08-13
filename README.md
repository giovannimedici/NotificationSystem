# 📨 NotificationSystem

Projeto de exemplo para praticar mensageria com **RabbitMQ** utilizando **.NET** (API + Worker) com containers Docker.

---

## 📌 Estrutura do Projeto

NotificationSystem/
├── NotificationSystem.Api # API para cadastro de usuários e envio de mensagens ao RabbitMQ
├── NotificationSystem.Worker # Worker que consome mensagens da fila e processa as notificações
├── NotificationSystem.Shared # Classes compartilhadas entre API e Worker
├── docker-compose.yml # Orquestração de containers (API, Worker e RabbitMQ)
└── README.md

yaml
Copiar
Editar

---

## 🚀 Tecnologias Utilizadas

- [.NET 8](https://dotnet.microsoft.com/)
- [RabbitMQ](https://www.rabbitmq.com/)
- [MongoDB](https://www.mongodb.com/) (armazenamento de usuários)
- [Docker](https://www.docker.com/)
- [Docker Compose](https://docs.docker.com/compose/)

---

## ⚙️ Pré-requisitos

Antes de rodar o projeto, você precisa ter instalado:

- [Docker](https://www.docker.com/get-started)
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (apenas se for rodar localmente sem Docker)

---

## 📂 Configuração do Ambiente

O projeto utiliza `docker-compose` para subir:

1. **RabbitMQ** (com painel de administração em `http://localhost:15672`)
2. **NotificationSystem.Api** (API .NET acessível em `http://localhost:5001`)
3. **NotificationSystem.Worker** (processador de mensagens)
4. **MongoDB** (banco para usuários)

---

## ▶️ Executando o Projeto

### 1️⃣ Clonar o repositório
```bash
git clone https://github.com/seuusuario/NotificationSystem.git
cd NotificationSystem
2️⃣ Subir os containers
bash
Copiar
Editar
docker-compose up --build
3️⃣ Acessar os serviços
API: http://localhost:5001

RabbitMQ Management: http://localhost:15672
Login: guest | Senha: guest

MongoDB: localhost:27017

📬 Testando a API
Criar um usuário
bash
Copiar
Editar
POST http://localhost:5001/api/users
Content-Type: application/json

{
  "name": "João Silva",
  "email": "joao@example.com"
}
📌 Ao criar o usuário:

Ele é salvo no MongoDB

Uma mensagem é enviada para a fila do RabbitMQ

O Worker consome e processa a notificação

🛠 Estrutura da Comunicação
mermaid
Copiar
Editar
flowchart LR
    A[API - Cria Usuário] -->|Publica Mensagem| B[(RabbitMQ)]
    B -->|Consome Mensagem| C[Worker - Processa Notificação]
    A -->|Salva Dados| D[(MongoDB)]
📄 Licença
Este projeto é apenas para fins de estudo e prática. Sinta-se à vontade para adaptar e utilizar como base para seus próprios projetos.