# Banking API - Learning Journal

## 1. Objetivo do projeto

O projeto `banking-api` e uma API bancaria didatica em .NET 8 criada para demonstrar praticas senior de backend em escala reduzida: Clean Architecture, DDD, CQRS, Event Sourcing, Transactional Outbox, idempotencia, concorrencia financeira, autenticacao, autorizacao, observabilidade, testes e CI/CD.

Fato observavel: o README descreve explicitamente que PIX e simulado, que PostgreSQL/Event Store e a fonte de verdade financeira e que read models existem para consulta rapida.

Inferencia: o projeto foi pensado como portfolio tecnico e material de entrevista, nao como produto bancario pronto para operar em producao regulada.

## 2. Estado atual do repositorio

Ultimo commit analisado: `1f3f9a8 feat(api): add FluentValidation request validators`, em `main`.

Estrutura atual:

- `src/Banking.Domain`: value objects, aggregate `Account`, eventos e exceptions de dominio.
- `src/Banking.Application`: contratos de casos de uso, DTOs, comandos, queries, interfaces e exceptions de aplicacao.
- `src/Banking.Infrastructure`: EF Core, PostgreSQL, Event Store, read models, auth persistence, idempotencia, PIX, statements e Outbox.
- `src/Banking.Api`: controllers REST versionados, JWT, Swagger, ProblemDetails, rate limiting, FluentValidation, Serilog, OpenTelemetry e health checks.
- `src/Banking.Worker`: publisher de Outbox com MassTransit/RabbitMQ.
- `tests/Banking.UnitTests`: testes de dominio.
- `tests/Banking.IntegrationTests`: testes de API com Testcontainers para PostgreSQL, Redis e RabbitMQ.
- `tests/Banking.ArchitectureTests`: regras de dependencia com NetArchTest.
- `docs/adr`: decisoes arquiteturais documentadas.
- `performance`: scripts k6 iniciais.

## 3. Como ler o repositorio primeiro

Ordem recomendada para aprender o projeto:

1. Leia `README.md` para entender objetivo, fluxo de requisicao, event flow, endpoints, regras e trade-offs.
2. Leia `plano.md` para ver a intencao inicial da primeira fase.
3. Leia `src/Banking.Domain/ValueObjects/Money.cs` e `src/Banking.Domain/Accounts/Account.cs` para entender as invariantes financeiras.
4. Leia os testes em `tests/Banking.UnitTests` para ver o contrato do dominio.
5. Leia `src/Banking.Application` para mapear comandos, queries e interfaces.
6. Leia `src/Banking.Infrastructure/Persistence/EventStore/PostgresEventStore.cs` para entender expected version e append de eventos.
7. Leia `DepositService`, `TransferService`, `PixService`, `StatementService` e `AccountService` em `src/Banking.Infrastructure`.
8. Leia os controllers em `src/Banking.Api/Controllers` para entender como HTTP vira comando/query.
9. Leia `tests/Banking.IntegrationTests` para ver idempotencia, concorrencia e fluxos reais.
10. Leia `.github/workflows/ci.yml`, `docker-compose.yml` e Dockerfiles para ver validacao e execucao local.

## 4. Metodologia usada

Foram usados README, `plano.md`, estrutura de arquivos, `git log --reverse`, estatisticas de commits, arquivos de dominio, aplicacao, infraestrutura, API, worker, testes, Docker, CI e ADRs.

Comandos executados:

- `git status --short --branch`: branch `main`, sem alteracoes antes do journal.
- `rg --files`: inventario de arquivos do repositorio.
- `git log --reverse --date=iso --pretty=format:'%H %ad %an %s'`: timeline cronologica.
- `git log --reverse --date=short --pretty=format:'COMMIT %h %ad %s' --stat`: arquivos afetados por commit.
- `dotnet --version`: `8.0.421`.
- `dotnet restore`: passou.
- `dotnet build`: primeira tentativa falhou por lock de arquivo causado por execucao paralela de build/testes; repetido isoladamente, passou.
- `dotnet test tests/Banking.UnitTests/Banking.UnitTests.csproj`: 22 testes passaram.
- `dotnet test tests/Banking.ArchitectureTests/Banking.ArchitectureTests.csproj`: 4 testes passaram.
- `dotnet format --verify-no-changes`: passou.
- `dotnet test tests/Banking.IntegrationTests/Banking.IntegrationTests.csproj`: 22 testes passaram.
- `docker compose build`: API e Worker construiram com sucesso.

## 5. Premissas, fatos e inferencias

Premissas:

- O historico Git local e a fonte principal da reconstrucao.
- Nao ha `AGENTS.md` no repositorio; essa ausencia foi verificada.
- A stack efetiva e .NET 8 com SDK local `8.0.421`.
- O journal descreve o ultimo estado conhecido localmente, nao um remoto GitHub.

Fatos observaveis:

- O repositorio tem 45 commits.
- Os commits seguem Conventional Commits.
- O dominio foi implementado antes de banco, API e mensageria.
- Testes unitarios vieram logo apos o dominio inicial.
- Testes de integracao e concorrencia foram adicionados depois da API e infraestrutura.
- ADRs foram adicionadas em um commit de documentacao posterior.

Inferencias:

- A arquitetura foi guiada por um plano incremental, porque `plano.md` e a ordem dos commits coincidem na primeira fase.
- A solucao escolheu simplicidade operacional em alguns pontos, como servicos concretos na Infrastructure implementando interfaces da Application, em vez de handlers com pipeline CQRS completo.
- O uso de `SELECT ... FOR UPDATE` no read model de origem em transferencia parece ter sido adicionado para tornar os testes concorrentes deterministas alem do expected version do Event Store.

Quando nao ha evidencia suficiente, este journal usa a frase: "Nao ha evidencia suficiente no historico para afirmar isso com certeza."

## 6. Historia cronologica da implementacao

O projeto comecou por uma solution .NET 8 com cinco projetos de producao e tres projetos de teste. A primeira decisao forte foi separar camadas desde o primeiro commit: API, Application, Domain, Infrastructure e Worker.

Na sequencia, o repositorio recebeu placeholders de documentacao, performance, CI e plano. Isso mostra preocupacao com governanca do projeto antes da implementacao funcional.

O dominio veio primeiro: `Money`, `AccountId`, `UserId`, exceptions, aggregate `Account`, status, eventos e regras de deposito, debito e transferencia. Os testes unitarios foram adicionados logo depois, protegendo a primeira camada critica.

Depois o projeto cresceu para persistencia: contratos de Event Store na Application e implementacao PostgreSQL na Infrastructure. O Event Store passou a usar eventos serializados, aggregate version e constraint unica.

Em seguida entrou autenticacao: contratos de auth, hash de senha, refresh token, JWT, endpoints e migrations. Depois disso vieram contas, depositos, transferencias, statement, PIX, Outbox, Worker, observabilidade, testes de arquitetura, Docker, CI, documentacao, testes de integracao, seeds, OpenAPI, rate limiting e FluentValidation.

Leitura simples e defensavel: o projeto evoluiu em camadas verticais, mas mantendo a direcao arquitetural inicial. A primeira fase foi dominio puro; as fases seguintes criaram funcionalidades completas atravessando Application, Infrastructure, Api e testes.

## 7. Timeline dos commits relevantes

| Commit | Data | Intencao provavel | Mudanca feita | Arquivos principais | Verificacao associada | Impacto arquitetural |
|---|---:|---|---|---|---|---|
| `32390cf` | 2026-05-28 | Criar base da solution | Projetos, references e `.gitignore` | `Banking.sln`, `src/*`, `tests/*` | Build esperado da estrutura | Define Clean Architecture fisica |
| `4186016` | 2026-05-28 | Criar contexto de repo | README inicial, plano, docs, performance, editorconfig | `README.md`, `plano.md`, `.editorconfig` | Nao funcional | Governanca e direcao inicial |
| `b7e7d19` | 2026-05-28 | Modelar dinheiro e ids | `Money`, `AccountId`, `UserId`, exceptions | `Money.cs`, ids, exceptions | Build | Valor monetario protegido no dominio |
| `9078506` | 2026-05-28 | Abrir conta e lifecycle | `Account`, status, eventos lifecycle | `Account.cs`, `AccountOpened` | Build | Aggregate root nasce event-sourced |
| `30b4d3f` | 2026-05-28 | Regras de movimentacao | deposito, debito, credito e exceptions | `Account.cs`, eventos financeiros | Build | Invariantes ficam no dominio |
| `3d1019b` | 2026-05-28 | Transferencia no dominio | `TransferTo`, evento e erro mesma conta | `Account.cs`, `TransferCompleted` | Build | Transferencia vira comportamento de aggregate |
| `1ee894e` | 2026-05-28 | Testar Money | Testes do value object | `MoneyTests.cs` | Unit tests | Contrato de arredondamento/validacao |
| `ebad3b3` | 2026-05-28 | Testar Account | lifecycle, deposito, debito, eventos | `AccountTests.cs` | Unit tests | Protege invariantes centrais |
| `41a244a` | 2026-05-28 | Testar transferencia | transferencia valida/invalida | `TransferTests.cs` | Unit tests | Protege regra de saldo e mesma conta |
| `bc71297` | 2026-05-28 | Criar porta de persistencia | `IEventStore`, envelope, metadata, clock | `src/Banking.Application/Abstractions` | Build | Application define contrato, infra implementa |
| `30ab5ba` | 2026-05-28 | Persistir eventos | DbContext e PostgreSQL Event Store | `PostgresEventStore.cs`, configs | Build | PostgreSQL vira fonte tecnica da verdade |
| `64feba3` | 2026-05-28 | Ligar host e migration | connection strings e migration inicial | `Program.cs`, migrations | Build | API/Worker conhecem Infra por DI |
| `1be15f2` | 2026-05-29 | Contratos de auth | requests, result, roles, exceptions | `src/Banking.Application/Auth` | Build | Auth entra como caso de uso |
| `a1aec32` | 2026-05-29 | Persistir credenciais | hash PBKDF2, refresh hash, JWT factory | `AuthService.cs`, records auth | Build | Seguranca minima realista |
| `12249bf` | 2026-05-29 | Expor auth HTTP | controller e JWT bearer | `AuthController.cs`, `Program.cs` | Build | API passa a ter autenticacao |
| `2bfa504` | 2026-05-29 | Migrar auth | tabelas users/refresh tokens e config JWT | migrations, appsettings | Build | Auth torna-se persistido |
| `dba3c0f` | 2026-05-29 | Reconstituir aggregate | `Account.Rehydrate` | `Account.cs` | Build | Event Sourcing fica utilizavel |
| `0e4bee7` | 2026-05-29 | Contratos de contas | commands/queries/DTOs | `Application/Accounts` | Build | CQRS por contratos |
| `a505bf0` | 2026-05-29 | Read model de contas | tabela/projecao de contas | `AccountReadModel*`, migration | Build | Consulta separada da fonte de verdade |
| `5074665` | 2026-05-29 | Endpoints de contas | service e controller | `AccountService.cs`, `AccountsController.cs` | Build | Primeiro fluxo vertical de negocio |
| `8e0119e` | 2026-05-29 | Contratos de deposito | command/response/interface | `Application/Deposits` | Build | Deposito modelado como use case |
| `790c6a7` | 2026-05-29 | Idempotencia persistida | tabela, hash e constraint | `IdempotencyRecord*`, migration | Build | Requisicoes financeiras ficam reexecutaveis |
| `0517fea` | 2026-05-29 | Deposito idempotente | controller, service, event append, statement | `DepositService.cs`, `DepositsController.cs` | Build | Combina dominio, Event Store e read models |
| `9251d9f` | 2026-05-29 | Contratos de transferencia | command/query/response/interface | `Application/Transfers` | Build | Transferencia como caso de uso |
| `ff89a98` | 2026-05-29 | Read model de transfers | tabela e migration | `TransferReadModel*` | Build | Consulta de transferencia separada |
| `87145b3` | 2026-05-29 | Transferencia idempotente | service, controller, expected version | `TransferService.cs` | Build | Regra critica chega ao fluxo HTTP |
| `ee01954` | 2026-05-29 | Contratos de extrato | query/DTO/interface | `Application/Statements` | Build | Statement vira read side |
| `2c4115e` | 2026-05-29 | Read model de extrato | tabela e config | `StatementEntryReadModel*` | Build | Extrato otimizado para query |
| `cf078ed` | 2026-05-29 | Query paginada | Dapper e atualizacao de statement | `StatementService.cs`, `StatementController.cs` | Build | Read side usa tecnologia propria |
| `6e68546` | 2026-05-29 | Contratos PIX | commands/queries/DTOs | `Application/Pix` | Build | PIX entra como simulacao |
| `c8c4b8a` | 2026-05-29 | Persistencia PIX | tabela, uniqueness e migration | `PixKeyRecord*` | Build | Chave PIX unica no banco |
| `e2c20e2` | 2026-05-29 | Pagamento PIX | PIX payment como transferencia | `PixService.cs`, `PixController.cs` | Build | Reuso de regra financeira |
| `18ed82f` | 2026-05-29 | Outbox persistente | records, writer, integration events | `OutboxWriter.cs`, migrations | Build | Mensageria desacoplada da transacao |
| `e8a7bb6` | 2026-05-29 | Enfileirar eventos | deposit/transfer/pix gravam Outbox | services financeiros | Build | Publicacao direta e evitada |
| `49cb711` | 2026-05-29 | Worker de Outbox | background worker com MassTransit | `OutboxPublisherWorker.cs` | Build | Resiliencia de broker |
| `7d17daf` | 2026-05-29 | Observabilidade | Serilog, correlation, OTEL, health | `Program.cs`, middleware | Build | Operabilidade basica |
| `c34be53` | 2026-05-29 | Testes arquiteturais | NetArchTest rules | `CleanArchitectureTests.cs` | Architecture tests | Boundaries passam a ser verificadas |
| `7add03a` | 2026-05-29 | Docker Compose | API, Worker, Postgres, Redis, RabbitMQ | `docker-compose.yml`, Dockerfiles | Docker build esperado | Execucao local integrada |
| `ecd632b` | 2026-05-29 | CI | restore/build/format/tests/docker build | `.github/workflows/ci.yml` | CI localmente equivalente | Automatiza qualidade |
| `f81dc8f` | 2026-05-29 | Docs finais | README, ADRs, `.http`, k6 | `README.md`, `docs/adr/*` | Revisao documental | Explica decisoes e trade-offs |
| `a105c1e` | 2026-05-29 | Fixture de integracao | Testcontainers e health test | `BankingApiFactory.cs` | Integration health test | Dependencias reais nos testes |
| `6df8177` | 2026-05-29 | Testes avancados | auth, accounts, deposits, transfers, pix, outbox, concorrencia | `tests/Banking.IntegrationTests/*` | Integration tests | Valida comportamento end-to-end |
| `6159498` | 2026-05-29 | Polimento de API | Swagger, rate limiting, seeds, docker ajustes | `Program.cs`, seeder, compose | Build/integ | Melhora experiencia local |
| `1f3f9a8` | 2026-05-30 | Validacao de entrada | FluentValidation para requests | validators, tests auth | Tests | Valida input na borda |

## 8. Decisao por decisao

Clean Architecture fisica:

- Feito: cinco projetos de producao e tres de testes.
- Por que provavelmente: isolar dominio, casos de uso, adaptadores e borda HTTP.
- Alternativas: monolito em um projeto, modular monolith por folders, vertical slices.
- Trade-off: boundaries compilaveis ajudam, mas adicionam boilerplate e referencias cruzadas permitidas em API/Worker.

Dominio puro:

- Feito: `Account`, `Money`, ids, eventos e exceptions em `Banking.Domain`.
- Por que provavelmente: proteger invariantes financeiras sem depender de banco ou HTTP.
- Alternativas: regras nos services ou stored procedures.
- Trade-off: dominio fica testavel, mas precisa de rehidratacao e mapeamento para persistencia.

Event Sourcing:

- Feito: eventos append-only em `event_store` com aggregate version.
- Por que provavelmente: auditoria e concorrencia otimista.
- Alternativas: saldo mutavel em tabela transacional, ledger contabil de dupla entrada.
- Trade-off: boa demonstracao de auditabilidade, mas serializar `AssemblyQualifiedName` acopla eventos ao nome/classe .NET.

CQRS:

- Feito: commands/queries/DTOs na Application e read models na Infrastructure.
- Por que provavelmente: separar escrita auditavel de leitura otimizada.
- Alternativas: CRUD direto via EF, MediatR com handlers na Application, vertical slices.
- Trade-off: conceito aparece, mas os "handlers" reais sao services concretos na Infrastructure; isso simplifica, porem mistura orquestracao de caso de uso com adaptador de persistencia.

Idempotencia:

- Feito: `idempotency_records` com hash, payload e status code.
- Por que provavelmente: permitir retries seguros.
- Alternativas: cache Redis, unique operation id no evento, middleware idempotente.
- Trade-off: robusto para persistencia, mas ha risco de corrida se duas requisicoes iguais entrarem simultaneamente antes da insercao; a constraint unica ajuda, mas o tratamento de excecao de unique violation para idempotencia concorrente nao esta evidente nos services.

Concorrencia:

- Feito: expected version no Event Store e `SELECT ... FOR UPDATE` na conta origem durante transferencia.
- Por que provavelmente: garantir que saldo nao fique negativo nos cenarios concorrentes.
- Alternativas: serializable isolation, advisory locks, Redis lock auxiliar, ledger com constraints.
- Trade-off: passa nos testes concorrentes e e simples de entender, mas o lock no read model torna o read model parte do mecanismo de escrita.

Outbox:

- Feito: mensagens gravadas na mesma transacao e publicadas por Worker.
- Por que provavelmente: evitar publish direto no broker dentro da transacao de negocio.
- Alternativas: MassTransit EF Outbox, CDC, publicar sincrono apos commit.
- Trade-off: implementacao didatica e clara, mas ainda nao ha claim concorrente de mensagens com `SKIP LOCKED`, DLQ formal ou propagacao rica de headers.

Autenticacao e autorizacao:

- Feito: JWT, refresh token com hash, roles `Customer` e `Admin`, ownership nos services.
- Por que provavelmente: demonstrar seguranca minima realista.
- Alternativas: OpenID Connect externo, Identity, Keycloak/Auth0.
- Trade-off: bom para portfolio; para producao faltam MFA, rotacao de chaves, politicas mais fortes e armazenamento seguro de secrets.

Observabilidade:

- Feito: Serilog, correlation id, OpenTelemetry console exporter, health checks.
- Por que provavelmente: mostrar operabilidade sem subir stack completa.
- Alternativas: OTLP Collector, Prometheus/Grafana, dashboards.
- Trade-off: simples de rodar localmente; insuficiente para producao sem backend de traces/metrics.

Testcontainers:

- Feito: testes de integracao sobem PostgreSQL, Redis e RabbitMQ.
- Por que provavelmente: testar com dependencias reais.
- Alternativas: SQLite/in-memory, mocks, docker compose externo.
- Trade-off: mais confianca e mais custo de execucao.

## 9. Pros e contras arquiteturais

Pros:

- Dominio financeiro isolado e bem coberto por testes unitarios.
- Event Store com expected version e constraint unica.
- Testes concorrentes validam o requisito mais importante.
- Outbox evita acoplamento direto entre transacao de negocio e RabbitMQ.
- Boundaries principais sao verificadas por NetArchTest.
- Docker, CI, README, ADRs e `.http` tornam o projeto facil de demonstrar.

Contras:

- Casos de uso ficam em services da Infrastructure; isso reduz pureza da Application.
- Controllers repetem tratamento de excecoes e extracao de claims.
- Event serialization por `AssemblyQualifiedName` dificulta evolucao/versionamento de eventos.
- Read model de contas participa do lock de transferencia, aproximando consulta e consistencia de escrita.
- Redis esta na arquitetura e nos testes, mas ainda nao parece usado como cache/rate limiting distribuido/idempotency fast lookup.
- O Worker nao evidencia controle forte contra duas instancias publicarem a mesma mensagem ao mesmo tempo.

## 10. Problemas enfrentados no caminho

Fatos observaveis no historico:

- Nao ha commits de reversao.
- O commit `6df8177` mistura muitos ajustes pequenos em controllers, migrations, services e testes para estabilizar integracao e concorrencia.
- O projeto ganhou testes de integracao depois das funcionalidades, nao antes.

Problema observado durante este journal:

- `dotnet build` falhou uma vez com `CS2012` porque build e testes foram executados em paralelo e disputaram `Banking.Domain.dll`. Reexecutado isoladamente, o build passou. Impacto: nenhum problema estrutural do projeto; foi uma falha de procedimento de validacao.

Inferencias:

- Os testes concorrentes provavelmente expuseram necessidade de ajustes em `TransferService` e `StatementService`, porque o commit que adiciona esses testes tambem altera esses services.
- Nao ha evidencia suficiente no historico para afirmar que houve bugs especificos antes dos ajustes, apenas que houve estabilizacao junto com testes.

## 11. Evolucao dos testes

Os testes evoluiram em tres ondas:

1. Testes unitarios logo apos o dominio: `MoneyTests`, `AccountTests`, `TransferTests`.
2. Testes de arquitetura apos a implementacao dos modulos principais: regras de dependencia e controllers sem DbContext.
3. Testes de integracao apos API, infraestrutura e Docker: auth, accounts, deposits, transfers, PIX, statement, outbox e concorrencia.

Resultado atual:

- Unitarios: 22 passaram.
- Arquitetura: 4 passaram.
- Integracao: 22 passaram.

## 12. Sinais reais de TDD

Ha sinal parcial de TDD na primeira fase porque os commits de dominio e testes estao muito proximos e em unidades pequenas. Mesmo assim, os testes foram commitados depois das implementacoes (`feat(domain)` antes de `test(domain)`), entao nao ha evidencia suficiente para afirmar TDD estrito red-green-refactor.

## 13. Onde nao ha evidencia de TDD

Nao ha evidencia de TDD para API, infraestrutura, auth, Outbox, Worker, observabilidade e FluentValidation. A leitura mais simples e que as funcionalidades foram implementadas primeiro e os testes de integracao foram adicionados depois para cobrir os fluxos principais.

## 14. Quais testes protegem quais decisoes

- `MoneyTests`: protegem validacao de valor negativo, zero permitido e operacoes monetarias.
- `AccountTests`: protegem abertura ativa, saldo zero, deposito, debito, conta inativa e eventos.
- `TransferTests`: protegem transferencia valida, saldo insuficiente, mesma conta e eventos.
- `CleanArchitectureTests`: protegem dependencias Domain/Application/Infrastructure/API e impedem controllers de acessar DbContext diretamente.
- `AuthApiTests`: protegem registro, login, refresh e validacao de entrada.
- `DepositsApiTests`: protegem deposito e idempotencia.
- `TransfersApiTests`: protegem transferencia e consulta por id.
- `ConcurrentTransfersTests`: protegem saldo nao negativo com 2 transferencias de 80 e 10 transferencias de 30.
- `OutboxTests`: protegem gravacao de mensagens pendentes.
- `StatementApiTests`: protegem extrato paginado/read model.
- `PixApiTests`: protegem chave PIX e pagamento simulado.

## 15. Mapa das boundaries atuais

- Domain: regras financeiras puras e eventos. Nao referencia Application, Infrastructure ou API.
- Application: contratos de entrada/saida e portas. Referencia Domain.
- Infrastructure: implementa portas, usa EF Core, Dapper, Event Store, Outbox, Auth e read models.
- Api: autentica, valida, autoriza na borda, chama interfaces de Application, traduz exceptions para ProblemDetails.
- Worker: consome Outbox e publica via MassTransit.

Observacao importante: a Application contem interfaces e DTOs, mas a orquestracao principal dos casos de uso esta em `Banking.Infrastructure`. Isso ainda respeita as referencias compilaveis, mas nao e a forma mais pura de Clean Architecture.

## 16. Checklist de boundaries para futuras features

- A regra de negocio pertence ao dominio?
- O controller apenas valida HTTP, extrai usuario e chama uma interface?
- A Application expoe contrato sem depender de EF, Dapper, MassTransit ou ASP.NET?
- A Infrastructure implementa persistencia ou integracao sem vazar detalhes para API?
- O fluxo financeiro exige `Idempotency-Key`?
- O evento financeiro entra no Event Store antes do read model ser confiado?
- A publicacao externa passa pela Outbox?
- O teste unitario cobre a invariante do dominio?
- O teste de integracao cobre o fluxo HTTP e persistencia real?
- O teste de arquitetura continua passando?

## 17. Como adicionar a proxima feature

Exemplo para uma nova operacao financeira:

1. Adicionar comportamento/evento no dominio se houver nova invariante.
2. Adicionar command/response/interface em `Banking.Application`.
3. Implementar service em `Banking.Infrastructure`, usando Event Store, idempotencia, read models e Outbox se necessario.
4. Criar migration/read model se houver consulta.
5. Expor controller em `Banking.Api` com `/api/v1`, authorization e FluentValidation.
6. Adicionar testes unitarios para dominio.
7. Adicionar testes de integracao para idempotencia, ownership e concorrencia quando aplicavel.
8. Atualizar README, `.http` e ADR se houver decisao nova.

## 18. Onde a arquitetura e simples de proposito

- PIX e simulado, sem Banco Central.
- Worker de Outbox tem loop simples com batch, retries e status.
- OpenTelemetry usa console exporter.
- Rate limiting e local ao processo.
- Read models sao atualizados no fluxo da aplicacao, nao por pipeline assincrono completo de projecoes.
- Auth e proprio do projeto, sem provedor OIDC externo.

Essas escolhas reduzem escopo e deixam o projeto executavel localmente.

## 19. Onde a solucao nao basta para producao

- Falta provedor de identidade real, rotacao de chaves, MFA e gestao de secrets.
- Falta observabilidade operacional com collector, metric backend, alertas e dashboards.
- Falta estrategia robusta de versionamento de eventos.
- Falta ledger contabil completo de dupla entrada.
- Falta resiliencia avancada no Worker para multiplas instancias, DLQ formal e claim atomico de mensagens.
- Falta hardening de idempotencia sob corrida exatamente simultanea.
- Falta politica de retencao/expiracao efetiva para idempotency records.
- Falta auditoria administrativa completa.
- Falta modelagem regulatoria real de PIX/Open Banking.

## 20. Limites tecnicos, riscos e dividas

Bloqueantes para producao:

- Segredos JWT aparecem em appsettings/compose para desenvolvimento; producao exigiria secrets manager.
- Event type por `AssemblyQualifiedName` cria risco em renomeacoes e deploys com versoes diferentes.
- Outbox worker nao demonstra lock/claim seguro para multiplas instancias.

Importantes:

- Tratamento de excecoes repetido em controllers.
- Casos de uso em Infrastructure reduzem isolamento da Application.
- Redis presente, mas ainda nao usado como parte real das otimizacoes descritas.
- Idempotencia precisa de tratamento explicito de unique violation em corrida.

Melhorias futuras:

- Introduzir middleware/filtro global de exception handling.
- Usar MediatR ou handlers explicitos se a intencao for CQRS mais reconhecivel.
- Adicionar testes para logout/revogacao e ownership em todos os endpoints.
- Adicionar teste de duas instancias do Worker ou claim de outbox.

## 21. Resultado das revisoes de qualidade

Revisao estrutural:

- Boundaries principais estao boas e testadas.
- Controllers nao acessam `BankingDbContext` diretamente.
- Dominio esta livre de dependencias externas.
- Trade-off principal: Application e mais "contratos" que "orquestracao".

Revisao da stack:

- .NET 8, EF Core/Npgsql, Dapper, MassTransit, Testcontainers, NetArchTest, FluentValidation, Serilog e OpenTelemetry estao coerentes com o objetivo.
- Pacotes estao em versoes modernas para o periodo do projeto.
- Redis esta configurado/testado como dependencia, mas sua utilizacao funcional ainda e limitada ou nao evidente.

Checks executados:

- `dotnet restore`: sucesso.
- `dotnet build`: sucesso apos repetir isoladamente.
- `dotnet format --verify-no-changes`: sucesso.
- `dotnet test tests/Banking.UnitTests/Banking.UnitTests.csproj`: 22/22.
- `dotnet test tests/Banking.ArchitectureTests/Banking.ArchitectureTests.csproj`: 4/4.
- `dotnet test tests/Banking.IntegrationTests/Banking.IntegrationTests.csproj`: 22/22.
- `docker compose build`: sucesso.

## 22. Ajustes recomendados

Bloqueantes:

- Para producao, trocar secrets locais por secrets manager e configuracao segura.
- Criar versionamento estavel de tipos de evento, sem depender de `AssemblyQualifiedName`.
- Tornar o Worker seguro para multiplas instancias com claim atomico, por exemplo `FOR UPDATE SKIP LOCKED` ou status transition controlada.

Importantes:

- Centralizar ProblemDetails/exception mapping.
- Levar orquestracao de casos de uso para Application ou assumir explicitamente no README que a Application define contratos e a Infrastructure implementa services.
- Tratar unique violation de idempotencia em corrida simultanea e retornar resposta deterministica.
- Adicionar testes para logout, revogacao de refresh token e ownership negativa em mais endpoints.

Melhorias futuras:

- Usar OTLP exporter e documentar uma stack local de observabilidade.
- Adicionar testes k6 executaveis em pipeline opcional.
- Adicionar ADR especifica sobre services na Infrastructure versus handlers na Application.
- Melhorar DLQ/retry/backoff do Worker.

Esteticos/opcionais:

- Reduzir duplicacao de `GetUserId`/`GetRole` nos controllers.
- Padronizar nomes de testes que mencionam transfer dentro de `DepositsApiTests`.
- Expandir `docs/diagrams` com arquivos Mermaid separados alem dos diagramas no README.

## 23. Resultado final no ultimo commit analisado

No commit `1f3f9a8`, o projeto esta funcional como demonstracao senior de backend .NET 8. Ele compila, tem formatacao verificada, possui testes unitarios, de arquitetura e integracao passando, executa com Docker Compose, documenta decisoes em README/ADRs e cobre o requisito mais critico: concorrencia nao pode gerar saldo negativo.

O resultado nao e um banco pronto para producao, e isso e uma qualidade do escopo: o README deixa claro o carater educacional, os trade-offs e os futuros incrementos. A base atual e forte para entrevista e aprendizado porque mostra nao apenas endpoints, mas tambem consistencia financeira, idempotencia, auditoria por eventos, Outbox e testes com dependencias reais.

## Addendum: local execution evidence

On 2026-07-12, the project was run again from the checked revision using
Docker Compose. Health and Swagger returned HTTP 200. A real API flow
registered a user, opened two accounts, deposited 100.00, replayed the same
idempotency key with the same response, transferred 40.00, and returned a
source balance of 60.00 in the statement. The Outbox contained two messages in
`Processed` status after the Worker reconnected to RabbitMQ.

The full command set, environment, ports, results, and startup observation are
recorded in `docs/local-run-evidence.md`.
