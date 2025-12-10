# 📘 LastLink Payment — Roadmap (Official Version Sequence)

This roadmap reflects the planned evolution of the Anticipation Service according to the project delivery strategy defined for the technical challenge.

Each version builds on top of V1 without reworking existing behaviors.

---

# 🚀 V1 — MVP Core (Delivered)

A complete implementation of all business rules defined in the challenge:

- Create anticipation request  
- Minimum amount validation (R$100)  
- Only one pending request per creator  
- Fee calculation (5%)  
- Approve / Reject flows  
- List by creator  
- Simulation endpoint  
- SQLite + EF Core  
- Angular UI  
- NUnit + Moq + FluentAssertions  
- Postman E2E suite  
- Clean Architecture + DDD  

**No Docker is used in this version.**

---

# 🧱 V2 — Docker + Gateway Failover + Retry (Next)

This version introduces real operational elements while keeping the core MVP unchanged.

### 🔑 Capabilities
- Docker / docker-compose environment  
- Integration layer for payment gateways (REST mocked)  
- **Gateway A (primary)**  
- **Gateway B (fallback)**  
- Failover orchestration logic in the Application Layer  
- Retry policies (exponential backoff)  
- Timeout enforcement  
- Logging for gateway interactions  
- Idempotency via `requestId`  
- Circuit breaker (optional for this version)  

### 🎯 Objective
Demonstrate resilience, fault-tolerance and integration maturity.

---

# 🔐 V3 — Authentication + Observability + Circuit Breaker

Focus on production-readiness and platform governance.

### 🔑 Capabilities
- JWT authentication (access + refresh tokens)
- Role separation (creator, admin)
- Route protection
- Structured logging (Serilog)
- CorrelationId management
- Request tracing (OpenTelemetry)
- Metrics collection (Prometheus)
- Grafana dashboards
- Circuit breaker (if not implemented in V2)

### 🎯 Objective
Bring the platform to a real payment-service operational level.

---

# 🧵 POC — Event-Driven Architecture with Redis Streams (Independent)

This proof-of-concept does *not* interfere with the core challenge but showcases architectural maturity.

### 🔑 Capabilities
- Redis Streams message bus  
- Consumer Groups (`gateway-priority`, `gateway-fallback`)  
- Asynchronous orchestrator  
- Stateless workers for each gateway  
- Automatic redelivery using XPENDING / XCLAIM  
- Event-based status updates  
- Optional SignalR broadcast to update the UI  
- Docker orchestrated environment  

### 🎯 Objective
Demonstrate mastery of modern distributed architecture without complicating the primary solution.

---

# 📌 Notes

- V1 remains intentionally simple and aligned to the challenge.  
- V2, V3, and the POC represent **scalable, enterprise-grade capabilities** commonly used in payment gateways, acquirers and processors.  
- The roadmap is incremental and does not break backwards compatibility.

