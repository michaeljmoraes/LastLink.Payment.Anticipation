# 🚀 **LastLink Payment – Anticipation Service (V1)**

### **Clean Architecture • .NET 8.0.416 • Angular 17.3.8 • SQLite • EF Core 8**

---

# ⚡ **Executive Summary (For Evaluators)**

A concise, decision-ready overview.

### **Technology**

* Backend: **.NET 8.0.416**, EF Core 8, SQLite, Clean Architecture, DDD
* Frontend: **Angular 17.3.8**
* Testing: **NUnit + Moq + FluentAssertions**, Postman E2E
* Architecture: Domain-driven, infrastructure-isolated, layered

### **Business Capabilities (V1)**

* Create anticipation requests
* Minimum value enforcement (R$100)
* One pending request per creator
* Approve / Reject
* List by creator
* Simulation endpoint
* Full unit + E2E coverage

### **Roadmap**

* **V2:** Docker, gateway failover, retry, idempotency
* **V3:** JWT, Logging, Observability, Circuit Breaker
* **POC:** Redis Streams (event-driven async processing)

---

# ⚙️ **1. Quick Start (Backend + Frontend)**

## **Clone the repository**

```bash
git clone https://github.com/michaeljmoraes/LastLink.Payment.Anticipation.git
cd LastLink.Payment.Anticipation
```

---

## **Backend — start in < 30 seconds**

```bash
dotnet restore .\backend\src

dotnet ef database update --project backend/src/LastLink.Payment.Anticipation.Infrastructure --startup-project backend/src/LastLink.Payment.Anticipation.Api

cd backend/src/LastLink.Payment.Anticipation.Api
dotnet run --launch-profile "LastLink.Api.Local(HTTP)"
```

API
`http://localhost:5274/api/v1/anticipations`

Swagger
`http://localhost:5274/swagger`

---

## **Frontend — start in < 20 seconds**

```bash
cd frontend/lastlink-payment-front
npm install
ng serve --open
```

UI
`http://localhost:4200/`

---

# 📁 **2. Essential Project Structure**

```text
LastLink.Payment.Anticipation
├── backend/
│   ├── src/
│   │   ├── LastLink.Payment.Anticipation.Api/              (Controllers, filters, DI setup)
│   │   ├── LastLink.Payment.Anticipation.Application/      (Services, DTOs, interfaces)
│   │   ├── LastLink.Payment.Anticipation.Domain/           (Entities, rules, exceptions)
│   │   └── LastLink.Payment.Anticipation.Infrastructure/   (DbContext, EF configs, repositories)
│   │
│   └── tests/
│       └── LastLink.Payment.Anticipation.Tests/            (NUnit + Moq + FluentAssertions)
│
├── frontend/
│   └── lastlink-payment-front/
│       └── src/
│           ├── app/
│           │   ├── features/
│           │   │   └── anticipation/
│           │   │       ├── create/
│           │   │       └── list/
│           │   ├── layout/
│           │   └── shared/
│           │
│           ├── assets/
│           └── environments/
│
├── docs/                                                   (Architecture, ADRs, full documentation)
├── e2e/
│   └── postman/                                            (Complete E2E suite with traceability)
│
└── global.json                                             (.NET SDK pin = 8.0.416)
```

---

# 📘 3. Detailed Documentation

Full architecture, diagrams, domain rules, API documentation, ADRs, testing strategy and roadmap:

➡️ **[docs/DETAILS.md](docs/DETAILS.md)**

---

# 📘 4. ROADMAP Documentation

For the full, versioned roadmap, see:  
➡️ **[docs/DETAILS.md](docs/ROADMAP.md)**


---

# 👤 **Author**

**Michael Jackson Moraes**
Software Engineer — .NET, DDD, Payments, Angular

