# HealthAndMed

O **HealthAndMed** é um sistema de gestão para ambientes de saúde, desenvolvido com .NET 8. Ele oferece uma API REST que possibilita o gerenciamento de agendamentos, cadastro de usuários (pacientes, médicos e administradores), gestão de agendas dos médicos, entre outras funcionalidades.

Esta API foi desenvolvida seguindo as melhores práticas, utilizando o Entity Framework Core para acesso a dados, conta com testes unitários e de integração, e possui uma infraestrutura preparada para ser executada em containers com Kubernetes, com monitoração via Prometheus e visualização via Grafana.

---

## Sumário

- [Características](#características)
- [Estrutura do Projeto](#estrutura-do-projeto)
- [Principais Tecnologias](#principais-tecnologias)
- [Endpoints da API e Autenticação](#endpoints-da-api-e-autenticação)
- [Uso do Swagger](#uso-do-swagger)
- [Infraestrutura em Kubernetes](#infraestrutura-em-kubernetes)
- [Executando a API Localmente](#executando-a-api-localmente)
- [Testes](#testes)
- [CI/CD](#cicd)
- [Licença](#licença)

---

## Características

- **API REST** para gerenciamento de:
  - Agendamentos de consultas.
  - Cadastro e gerenciamento de usuários (pacientes, médicos e administradores).
  - Agendas dos médicos.
- **Autenticação** via Basic Authentication.
- **Documentação interativa** com Swagger.
- **Infraestrutura containerizada** com Docker e deploy em Kubernetes.
- **Monitoramento** dos endpoints via Prometheus e visualização dos dados com Grafana.
- Testes unitários e de integração utilizando xUnit e Moq.

---

## Estrutura do Projeto

A estrutura do projeto está organizada da seguinte forma:

```
.
├── .github
│   └── workflows
│       └── pipeline-ci.yml       # Pipeline CI/CD com GitHub Actions
├── HealthAndMed.API              # Projeto da API
│   ├── Authentication            # Implementação da Basic Authentication
│   ├── Controllers               # Endpoints (Appointment, Doctor, DoctorsSchedule, User)
│   ├── Extensions                # Validações customizadas (ex.: CustomEmailAddress)
│   ├── Models                    # Modelos para entrada/saída de dados
│   ├── Repository                # Implementações dos repositórios e interfaces
│   ├── appsettings*.json         # Configurações de ambiente
│   ├── Dockerfile                # Dockerfile para containerização
│   ├── HealthAndMed.API.csproj
│   ├── HealthAndMed.API.http     # Arquivo para testes com HTTP
│   └── Program.cs                # Configuração da aplicação (.NET 8)
├── HealthAndMed.Core              # DTOs, entidades e regras de negócio
├── HealthAndMed.Infraestructure   # Configuração do DbContext e mapeamentos (Entity Framework)
├── HealthAndMed.Tests             # Testes unitários e de integração
├── k8s-configs                    # Arquivos YAML para deployment no Kubernetes
├── prometheus                   # Configuração do Prometheus
├── docker-compose.yml             # Configuração para execução via Docker Compose
├── HealthAndMed.sln               # Solução do Visual Studio
└── README.md                      # Este arquivo
```

---

## Principais Tecnologias

- **.NET 8**: Framework para desenvolvimento da API.
- **Entity Framework Core**: Acesso e mapeamento dos dados.
- **Swagger (Swashbuckle.AspNetCore)**: Documentação interativa dos endpoints.
- **Docker**: Containerização da aplicação.
- **Kubernetes (minikube)**: Orquestração e deploy dos containers.
- **Prometheus & Grafana**: Monitoramento e visualização de métricas.
- **xUnit e Moq**: Testes unitários e de integração.

---

## Endpoints da API e Autenticação

### Autenticação

A API utiliza **Basic Authentication** para proteger os endpoints críticos. A autenticação funciona da seguinte maneira:

- **Cabeçalho HTTP:**  
  Envie o cabeçalho `Authorization` com o valor no formato:  
  ```
  Basic <base64(username:password)>
  ```
- **Exceções:**  
  Endpoints como `/metrics`, `/health`, `/readiness` e `/api/live/ws` são acessíveis sem autenticação para facilitar a monitoração.

### Principais Endpoints

#### **Agendamentos (Appointment)**

- **GET /api/Appointment**  
  Lista os agendamentos. A resposta varia de acordo com o perfil do usuário:
  - **Administrador:** Retorna todos os agendamentos.
  - **Médico:** Retorna apenas os agendamentos do médico autenticado.
  - **Paciente:** Retorna os agendamentos do paciente autenticado.

- **POST /api/Appointment**  
  Cria um novo agendamento, verificando se a agenda selecionada está disponível (não bloqueada).

- **PUT /api/Appointment/{id}**  
  Atualiza um agendamento existente (restrito ao administrador).

- **PUT /api/Appointment/{id}/cancel**  
  Permite que o paciente (ou administrador) cancele um agendamento, desde que esteja no status "Pendente" ou "Agendado".

- **PUT /api/Appointment/{id}/status**  
  Atualiza o status de um agendamento (para "Aceito" ou "Recusado"). Geralmente, utilizado por médicos e administradores.

#### **Médicos (Doctor)**

- **GET /api/Doctor**  
  Busca médicos com filtros opcionais (nome, especialidade, número CRM ou ID).

- **GET /api/Doctor/with-future-schedules**  
  Lista os médicos que possuem agendas futuras disponíveis.

#### **Agendas dos Médicos (DoctorsSchedule)**

- **GET /api/DoctorsSchedule**  
  Lista as agendas dos médicos. Se o usuário autenticado for um médico, mostra apenas suas agendas; caso contrário, retorna todas (para administradores).

- **POST /api/DoctorsSchedule**  
  Cria uma nova agenda para um médico.

- **PUT /api/DoctorsSchedule/{id}**  
  Atualiza uma agenda existente.

- **DELETE /api/DoctorsSchedule/{id}**  
  Exclui uma agenda (restrito a administradores).

#### **Usuários (User)**

- **GET /api/User**  
  Lista todos os usuários (apenas para administradores).

- **POST /api/User**  
  Cria um novo usuário. Para médicos, exige campos adicionais como Número CRM, Especialidade e Valor da Consulta.

- **PUT /api/User/{id}**  
  Atualiza um usuário existente.

- **DELETE /api/User/{id}**  
  Exclui um usuário (apenas para administradores).

---

## Uso do Swagger

A documentação interativa da API está integrada com o Swagger. Para acessar:

1. Execute a aplicação.
2. Navegue até: [http://localhost:5159/swagger](http://localhost:5159/swagger)  
   *(ou a URL/porta configurada conforme o ambiente)*.
3. Utilize o botão **Authorize** para inserir as credenciais (Basic Auth) e testar os endpoints diretamente pela interface.

---

## Infraestrutura em Kubernetes

A aplicação está preparada para ser executada em um cluster Kubernetes. Na pasta `k8s-configs` você encontra os arquivos YAML que definem os seguintes recursos:

- **Deployments**:  
  - `healthandmed-api-deployment.yaml`: Deploy da API HealthAndMed.
  - `prometheus-deployment.yaml`: Deploy do Prometheus para coleta de métricas.
  - `grafana-deployment.yaml`: Deploy do Grafana para visualização das métricas.

- **Services**:  
  - `healthandmed-api-service.yaml`: Serviço para expor a API (NodePort).
  - `prometheus-service.yaml`: Serviço para acessar o Prometheus.
  - `grafana-service.yaml`: Serviço para acessar o Grafana.

- **ConfigMaps**:  
  - `healthandmed-api-configmap.yaml`: Configuração da API, incluindo connection string e níveis de log.
  - `prometheus-configmap.yaml`: Configuração do Prometheus (arquivo `prometheus.yml`).

- **RBAC e ServiceAccount**:  
  - `prometheus-clusterrole.yaml` e `prometheus-clusterbinding.yaml` definem as permissões para o Prometheus.

- **Volumes Persistentes (PV/PVC)**:  
  - Arquivos para o Grafana e outros serviços que necessitam persistência de dados.

### Comando de Verificação

Após realizar o deploy no cluster (por exemplo, usando `kubectl apply -f k8s-configs/ -R`), você pode verificar os recursos com:

```bash
kubectl get pods,svc,deployment,pv,pvc -n healthmed
```

---

## Executando a API Localmente

### Pré-requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [Docker](https://www.docker.com/)
- [Minikube](https://minikube.sigs.k8s.io/docs/start/) e [kubectl](https://kubernetes.io/docs/tasks/tools/)

### Rodando com Docker Compose

Utilize o arquivo `docker-compose.yml` para levantar os containers da API, Prometheus e Grafana:

```bash
docker-compose up --build
```

A API será exposta na porta `8080` e você poderá acessar o Swagger via `http://localhost:8080/swagger`.

### Rodando via Minikube

Se você preferir usar o Minikube, após aplicar os arquivos YAML (deploy):

```bash
kubectl apply -f k8s-configs/ -R
```

Use o comando abaixo para expor os serviços via Minikube:

```bash
minikube service healthandmed-api-service -n healthmed
```

---

## Testes

A solução conta com testes unitários e de integração. Para executá-los, utilize:

```bash
dotnet test
```

---

## CI/CD

O projeto utiliza o GitHub Actions para integração contínua e deploy:
- O arquivo `.github/workflows/pipeline-ci.yml` define as etapas de build, teste e deploy (publicação da imagem Docker no Docker Hub).

---

## Licença

[Informe aqui a licença do projeto, ex.: MIT, Apache, etc.]

---

## Considerações Finais

- A API utiliza **Basic Authentication**. Certifique-se de enviar o cabeçalho `Authorization` com as credenciais codificadas em Base64 para acessar os endpoints protegidos.
- Os endpoints de monitoração (e.g., `/metrics`, `/health`) são acessíveis sem autenticação.
- A infraestrutura em Kubernetes permite escalabilidade e alta disponibilidade, além de integração com Prometheus e Grafana para monitoramento de métricas.

---