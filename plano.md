# Banking API — Plano de Implementacao

## Objetivo

Criar uma Banking API didatica em .NET 8 para demonstrar Clean Architecture, DDD, CQRS, Event Sourcing, Transactional Outbox, idempotencia, concorrencia, seguranca, observabilidade, testes e CI/CD.

## Stack Planejada

- .NET 8
- ASP.NET Core Web API
- PostgreSQL
- EF Core
- Dapper
- Redis
- RabbitMQ
- MassTransit
- JWT Bearer e Refresh Token
- FluentValidation
- Serilog
- OpenTelemetry
- Docker e Docker Compose
- GitHub Actions
- xUnit, FluentAssertions, Testcontainers e NetArchTest
- k6

## Parte 1

Implementar primeiro:

- estrutura da solution;
- projetos por camada;
- referencias entre projetos;
- documentacao inicial;
- dominio puro;
- testes unitarios do dominio;
- commits atomicos.

Nao implementar ainda:

- banco de dados;
- migrations;
- controllers reais;
- autenticacao;
- Redis;
- RabbitMQ;
- Outbox;
- testes de integracao reais.

## Ordem de Commits da Parte 1

1. `chore: create banking solution structure`
2. `chore: add repository docs and tooling placeholders`
3. `feat(domain): add money and identifier value objects`
4. `feat(domain): add account aggregate lifecycle`
5. `feat(domain): add account money movement rules`
6. `feat(domain): add transfer domain behavior`
7. `test(domain): add money value object tests`
8. `test(domain): add account aggregate tests`
9. `test(domain): add transfer behavior tests`

## Decisoes da Parte 1

- Conta nova comeca ativa.
- Saldo inicial e zero.
- `Money.Zero` e valido para saldo.
- Operacoes financeiras exigem valor maior que zero.
- Conta inativa nao movimenta dinheiro.
- Debito nunca pode deixar saldo negativo.
- Transferencia para a mesma conta e invalida.
- Aggregate registra eventos nao commitados e incrementa version ao aplicar eventos.

