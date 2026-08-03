# Gym Management System

A production-oriented gym operations platform built on ASP.NET Core 10 with a layered architecture, JWT authentication, recurring subscription automation, Stripe payment flows, email confirmation, SQL Server persistence, and a lightweight static web client for both admins and members.

## Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Architecture](#architecture)
- [Project Structure](#project-structure)
- [Tech Stack](#tech-stack)
- [Why This Project Is Strong](#why-this-project-is-strong)
- [Security](#security)
- [Performance](#performance)
- [API](#api)
- [Database](#database)
- [Docker](#docker)
- [Environment Variables](#environment-variables)
- [Running the Project](#running-the-project)
- [Configuration](#configuration)
- [Testing](#testing)
- [Logging and Monitoring](#logging-and-monitoring)
- [CI/CD](#cicd)
- [Architecture Diagram](#architecture-diagram)
- [Future Improvements](#future-improvements)
- [Contributing](#contributing)

## Overview

Gym Management System is designed to support the day-to-day workflows of a gym business:

- managing members and coaches
- creating and administering plans
- creating, renewing, freezing, scheduling, and expiring subscriptions
- tracking attendance
- handling pending and paid invoices
- supporting card payment flows through Stripe
- issuing JWT access tokens and refresh tokens
- sending email confirmation links during registration

The project targets two primary users:

- gym administrators and coaches who manage operations
- members who consume subscriptions, review payment state, and access their own profile/payment history

At a business level, it solves the common problem of scattered gym administration by centralizing membership, billing, attendance, and subscription lifecycle rules in one system.

## Features

### Membership and Coach Management

- Create, update, and soft-delete members
- Create, update, and deactivate coaches
- Upload and update profile images
- Fetch current member profile and member-specific views
- Role-aware access rules for self-service and admin workflows

### Plans and Subscriptions

- Create, update, list, and deactivate plans
- Create subscriptions from plans
- Renew subscriptions from the latest member subscription
- Activate, schedule, cancel, freeze, unfreeze, and expire subscriptions
- Recurring Hangfire jobs for scheduled activation, expiration, and unfreeze workflows
- Snapshot plan pricing into subscriptions and payments

### Payments

- Create pending payments when subscriptions are created or renewed
- Admin-driven payment completion
- Stripe payment intent creation
- Stripe webhook processing for external payment confirmation
- Member payment history and admin payment listing

### Identity and Authentication

- JWT access tokens
- Refresh token flow
- Email confirmation flow
- Resend confirmation email support
- Admin/member/coach role model backed by ASP.NET Core Identity

### Attendance and Dashboard

- Member check-in with subscription validation
- One-check-in-per-day guard
- Attendance history per member
- Admin dashboard statistics for members, subscriptions, attendance, and revenue

### Frontend Assets

- Static admin UI under `src/Gym.Client/admin-ui`
- Static member UI under `src/Gym.Client/member-ui`
- Stripe-based member payment UI support in the member frontend

## Architecture

The solution uses a layered architecture with strong Clean Architecture and CQRS characteristics:

- `Gym.Domain` contains the business model, invariants, errors, enums, and core state transitions.
- `Gym.Application` contains use cases organized by feature, with commands, queries, DTOs, mappers, validators, and MediatR handlers.
- `Gym.Infrastructure` implements persistence, identity integration, external email, Stripe, EF Core mappings, migrations, and background job wiring.
- `Gym.Api` exposes the HTTP API, OpenAPI/Swagger configuration, exception handling, authentication, authorization, and middleware composition.
- `Gym.Client` contains static HTML/CSS/JavaScript clients.

CQRS is implemented at the feature level:

- commands mutate state
- queries return view-specific DTOs
- MediatR dispatches both

Cross-cutting behavior is implemented with MediatR pipeline behaviors already wired in application startup:

- caching behavior
- performance behavior
- unhandled exception behavior

## Project Structure

```text
Gym System/
├─ src/
│  ├─ Gym.Api/
│  │  ├─ Controllers/
│  │  ├─ Contracts/
│  │  ├─ Infrastructure/
│  │  ├─ OpenApi/
│  │  ├─ Properties/
│  │  ├─ appsettings*.json
│  │  └─ Program.cs
│  ├─ Gym.Application/
│  │  ├─ Common/
│  │  ├─ Features/
│  │  │  ├─ Attendances/
│  │  │  ├─ Coaches/
│  │  │  ├─ Dashboard/
│  │  │  ├─ Identity/
│  │  │  ├─ Members/
│  │  │  ├─ Payments/
│  │  │  ├─ Plans/
│  │  │  └─ Subscriptions/
│  │  └─ DependencyInjection.cs
│  ├─ Gym.Client/
│  │  ├─ admin-ui/
│  │  └─ member-ui/
│  ├─ Gym.Domain/
│  │  ├─ Attendance/
│  │  ├─ Coaches/
│  │  ├─ Common/
│  │  ├─ Identity/
│  │  ├─ Members/
│  │  ├─ Payments/
│  │  ├─ People/
│  │  ├─ Plans/
│  │  └─ Subscriptions/
│  └─ Gym.Infrastructure/
│     ├─ BackgroundJobs/
│     ├─ Data/
│     ├─ Email/
│     ├─ Identity/
│     ├─ Payments/
│     └─ Settings/
├─ tests/
│  ├─ Gym.Api.IntegrationTests/
│  ├─ Gym.Application.SubcutaneousTests/
│  ├─ Gym.Application.UnitTests/
│  ├─ Gym.Domain.UnitTests/
│  └─ Gym.Tests.Common/
├─ .github/workflows/
├─ Dockerfile
├─ docker-compose.yml
├─ Directory.Build.props
├─ Directory.Packages.props
└─ GymSystem.slnx
```

### Folder Responsibilities

| Path | Responsibility |
|---|---|
| `src/Gym.Domain` | Domain entities, errors, enums, and business rules |
| `src/Gym.Application` | Application use cases, DTOs, MediatR handlers, validators, and behaviors |
| `src/Gym.Infrastructure` | EF Core, Identity, email, Stripe, background jobs, migrations, and external integrations |
| `src/Gym.Api` | HTTP API surface, middleware, versioning, OpenAPI, and startup composition |
| `src/Gym.Client` | Static admin/member frontends |
| `tests/Gym.Domain.UnitTests` | Domain rule and state-transition tests |
| `tests/Gym.Application.UnitTests` | Mapper and behavior unit tests |
| `tests/Gym.Application.SubcutaneousTests` | End-to-end application-layer tests with real DI and SQLite |
| `tests/Gym.Api.IntegrationTests` | API integration test project scaffold |
| `tests/Gym.Tests.Common` | Shared builders/helpers used by tests |

## Tech Stack

| Area | Technology |
|---|---|
| Language | C# |
| Runtime | .NET 10 |
| Web Framework | ASP.NET Core Web API |
| API Versioning | `Asp.Versioning.Mvc`, `Asp.Versioning.Mvc.ApiExplorer` |
| Application Dispatching | MediatR |
| Validation Library | FluentValidation |
| Database | SQL Server |
| ORM | Entity Framework Core |
| Identity | ASP.NET Core Identity |
| Authentication | JWT Bearer Tokens |
| Authorization | Role-based and policy-based authorization |
| Token Lifecycle | Refresh tokens stored in SQL Server |
| Caching | `Microsoft.Extensions.Caching.Hybrid` |
| Logging | Serilog |
| Log Sink / Viewer | Seq |
| Background Jobs | Hangfire |
| Payments | Stripe |
| Email | Brevo SMTP API integration |
| API Docs | OpenAPI + Swagger UI + Scalar |
| Testing | xUnit |
| Mocking | NSubstitute |
| Subcutaneous Test DB | EF Core SQLite |
| Containerization | Docker + Docker Compose |
| CI | GitHub Actions |
| Static Frontend | HTML, CSS, JavaScript |
| Build Centralization | `Directory.Build.props`, `Directory.Packages.props` |
| Style Enforcement | StyleCop Analyzers + `.editorconfig` |

## Why This Project Is Strong

### Clean separation of concerns

The project keeps domain logic, use cases, infrastructure concerns, and HTTP delivery separated into dedicated projects, which reduces coupling and keeps business rules testable.

### Real CQRS implementation

Commands and queries are split by feature under the application layer, which keeps handlers focused and makes the use-case surface easy to navigate.

### Rich domain rules

Subscription, payment, attendance, member, coach, and plan behavior is enforced inside the domain model rather than being scattered across controllers.

### Result pattern

The solution consistently returns structured success/error results instead of throwing for ordinary business failures, which improves API predictability.

### Background automation

Recurring jobs automatically activate scheduled subscriptions, expire old active subscriptions, and unfreeze eligible subscriptions, which aligns the system with real gym lifecycle operations.

### Built-in payment workflow

Subscription creation and renewal create pending payment records, and the app supports both internal payment completion and Stripe payment intent/webhook flows.

### Strong authentication model

JWT access tokens, refresh tokens, email confirmation, and ASP.NET Core Identity roles give the project a solid foundation for real user accounts.

### Caching support at the application layer

Hybrid cache behavior is integrated into MediatR, allowing selected queries to participate in cached execution while preserving the handler model.

### High test coverage across layers

The repository includes domain unit tests, application unit tests, and subcutaneous application tests with a realistic DI/container setup.

### DevOps-ready packaging

The project includes Docker, Docker Compose, GitHub Actions, centralized package management, and analyzer configuration.

## Security

Implemented security features found in the codebase:

- JWT bearer authentication
- refresh token flow stored in the database
- ASP.NET Core Identity user and role management
- role-based authorization for admin/member/coach access
- policy-based authorization for same-member / same-coach / same-member-or-admin scenarios
- email confirmation flow for member registration
- Stripe webhook signature validation through configured webhook secret
- password rules configured through Identity

## Performance

Performance-oriented patterns currently present:

- Hybrid cache behavior for cached queries
- paginated query endpoints across major resources
- `AsNoTracking()` usage in read queries
- feature-scoped DTO projections instead of returning entities directly
- recurring jobs delegated to background processing rather than synchronous request paths

## API

### Base URLs

| Environment | Base URL |
|---|---|
| Local HTTP | `http://localhost:5194` |
| Local HTTPS | `https://localhost:7022` |
| Docker Compose | `http://localhost:8080` |

### API Documentation

In `Development`, the API exposes:

- OpenAPI documents for `v1` and `v2`
- Swagger UI
- Scalar API Reference

### Versioning

URL segment versioning is enabled:

```text
/api/v1/...
/api/v2/...
```

### Endpoint Groups

| Area | Sample Endpoints |
|---|---|
| Identity | `POST /api/v1/Identity/token/generate`, `POST /api/v1/Identity/token/refresh-token`, `POST /api/v1/Identity`, `POST /api/v2/Identity`, `GET /api/v1/Identity/confirm-email` |
| Members | `GET /api/v1/members`, `GET /api/v1/members/active`, `GET /api/v1/members/me`, `PUT /api/v1/members/{id}`, `DELETE /api/v1/members/{id}` |
| Coaches | `GET /api/v1/coaches`, `GET /api/v1/coaches/{id}`, `POST /api/v1/coaches`, `PUT /api/v1/coaches/{id}/image` |
| Plans | `GET /api/v1/plans`, `GET /api/v1/plans/{id}`, `POST /api/v1/plans`, `DELETE /api/v1/plans/{id}` |
| Subscriptions | `GET /api/v1/subscriptions`, `GET /api/v1/subscriptions/member/{id}`, `POST /api/v1/subscriptions`, `POST /api/v1/subscriptions/renew`, `PUT /api/v1/subscriptions/{subscriptionId}/freeze` |
| Payments | `GET /api/v1/payments`, `GET /api/v1/payments/member/{id}`, `POST /api/v1/payments/Pay`, `POST /api/v1/payments/{paymentId}/stripe-intent`, `POST /api/v1/payments/webhook` |
| Attendance | `GET /api/v1/attendances`, `GET /api/v1/attendances/{id}/history`, `POST /api/v1/attendances/{id}/check-in` |
| Dashboard | `GET /api/v1/dashboard/stats` |
| Images | `POST /api/v1/images/UploadImage` |

### Authentication

Most endpoints require a bearer token:

```http
Authorization: Bearer <access-token>
```

## Database

### Engine

- SQL Server
- EF Core code-first migrations

### Main Entities

- Members
- Coaches
- People
- PersonImages
- Plans
- Subscriptions
- Payments
- Attendances
- RefreshTokens
- ASP.NET Core Identity tables

### Key Relationships

- `Member` and `Coach` reference `Person`
- `Person` references `PersonImage`
- `Subscription` links `Member` to `Plan`
- `Payment` links to `Subscription`
- `Attendance` links to `Member`
- `RefreshToken` links to `AppUser`

### Query Filters

The infrastructure layer applies query filters for:

- soft-deleted members
- inactive coaches

### Migrations

Migrations are stored in:

```text
src/Gym.Infrastructure/Data/Migrations
```

### Seed Data

During development startup, the API initializer:

- applies migrations
- ensures `Admin`, `Coach`, and `Member` roles exist
- seeds an initial admin user

Current seeded admin credentials from code:

- email: `A@gmail.com`
- username: `Ahmedsaleh`
- password: `123456`

## Docker

### Services in `docker-compose.yml`

| Service | Purpose |
|---|---|
| `gym.api` | ASP.NET Core API container built from the root `Dockerfile` |
| `gym.db` | SQL Server 2022 database |
| `seq` | Centralized structured log viewer for Serilog |

### Container Architecture

- `gym.api` depends on SQL Server health and Seq startup
- uploaded images are persisted through a named volume
- SQL data is persisted through a named volume
- Seq data is persisted through a named volume

## Environment Variables

### Runtime Variables Used by Docker Compose

| Variable | Required | Description |
|---|---|---|
| `EMAIL_API_KEY` | Yes | Brevo API key used for confirmation emails |
| `EMAIL_FROM_EMAIL` | Yes | Sender email address used by the email provider |
| `JWT_SECRET` | Yes | JWT signing secret for token generation/validation |
| `STRIPE_SECRET_KEY` | Yes | Stripe secret key for server-side payment intent creation |
| `STRIPE_PUBLISHABLE_KEY` | Yes | Stripe publishable key used by the frontend |
| `STRIPE_WEBHOOK_SECRET` | Yes | Stripe webhook signing secret |

### Configuration Keys Used by the Application

| Variable | Required | Description |
|---|---|---|
| `ConnectionStrings__DefaultConnection` | Yes | SQL Server connection string |
| `JwtSettings__Issuer` | Yes | JWT issuer |
| `JwtSettings__Audience` | Yes | JWT audience |
| `JwtSettings__Secret` | Yes | JWT signing secret |
| `Email__ApiKey` | Yes | Email API key |
| `Email__FromEmail` | Yes | Sender email |
| `Email__FromName` | Recommended | Sender display name |
| `Stripe__SecretKey` | Yes | Stripe secret key |
| `Stripe__PublishableKey` | Yes | Stripe publishable key |
| `Stripe__WebhookSecret` | Yes | Stripe webhook secret |
| `Serilog__WriteTo__1__Args__serverUrl` | Optional | Seq server URL override |

### `.env.example`

This repository now includes a safe template:

```bash
cp .env.example .env
```

## Running the Project

### Docker

1. Clone the repository.
2. Copy the example environment file:

```bash
cp .env.example .env
```

3. Fill the placeholder values.
4. Start the stack:

```bash
docker compose up --build
```

5. Open:

- API: `http://localhost:8080`
- Seq: `http://localhost:8081`

### Local Development

1. Restore dependencies:

```bash
dotnet restore GymSystem.slnx
```

2. Build the solution:

```bash
dotnet build GymSystem.slnx
```

3. Make sure SQL Server is available and update `ConnectionStrings:DefaultConnection` if needed.
4. Provide required Stripe and email settings through user secrets, environment variables, or `appsettings`.
5. Run the API:

```bash
dotnet run --project src/Gym.Api/Gym.Api.csproj
```

6. In development mode, the API will:

- apply migrations
- seed roles
- seed the initial admin account
- expose Swagger/OpenAPI/Scalar

### Static Clients

The frontend in `src/Gym.Client` is plain HTML/CSS/JavaScript. It can be served by any static file server or opened/adapted during local development.

## Configuration

Important configuration sources:

- `src/Gym.Api/appsettings.json`
- `src/Gym.Api/appsettings.Development.json`
- `.env`
- environment variables passed through Docker Compose
- `src/Gym.Api/Properties/launchSettings.json`
- `Directory.Build.props`
- `Directory.Packages.props`
- `.editorconfig`

Current notable defaults:

- local HTTP profile: `http://localhost:5194`
- local HTTPS profile: `https://localhost:7022`
- permissive CORS policy named `AllowAll`
- Serilog console + Seq logging

## Testing

### Test Projects

| Project | Purpose |
|---|---|
| `Gym.Domain.UnitTests` | Domain rules and state transitions |
| `Gym.Application.UnitTests` | Behaviors and mapper tests |
| `Gym.Application.SubcutaneousTests` | Real application-layer integration with DI and SQLite |
| `Gym.Api.IntegrationTests` | API integration test project |
| `Gym.Tests.Common` | Shared test builders and helpers |

### Run Tests

Run all test layers individually:

```bash
dotnet test tests/Gym.Domain.UnitTests/Gym.Domain.UnitTests.csproj
dotnet test tests/Gym.Application.UnitTests/Gym.Application.UnitTests.csproj
dotnet test tests/Gym.Application.SubcutaneousTests/Gym.Application.SubcutaneousTests.csproj
dotnet test tests/Gym.Api.IntegrationTests/Gym.Api.IntegrationTests.csproj
```

## Logging and Monitoring

The project currently includes:

- Serilog request logging
- console logging
- Seq sink integration
- Hangfire dashboard middleware

In Docker Compose, Seq is exposed at:

```text
http://localhost:8081
```

## CI/CD

GitHub Actions workflow:

```text
.github/workflows/build-test.yml
```

It performs:

- checkout
- .NET 10 setup
- restore
- release build
- domain tests
- application unit tests
- subcutaneous tests
- integration tests

## Architecture Diagram

```text
                +----------------------+
                |   Gym.Client (UI)    |
                | admin-ui / member-ui |
                +----------+-----------+
                           |
                           v
                +----------------------+
                |       Gym.Api        |
                | Controllers, Auth,   |
                | OpenAPI, Middleware  |
                +----------+-----------+
                           |
                           v
                +----------------------+
                |   Gym.Application    |
                | Commands / Queries   |
                | Behaviors / DTOs     |
                +----------+-----------+
                           |
                           v
                +----------------------+
                |      Gym.Domain      |
                | Entities, Rules,     |
                | Errors, Enums        |
                +----------+-----------+
                           |
                           v
                +----------------------+
                |  Gym.Infrastructure  |
                | EF Core, Identity,   |
                | Stripe, Email, Jobs  |
                +----------+-----------+
                           |
          +----------------+----------------+
          |                |                |
          v                v                v
    +-----------+    +-----------+    +-----------+
    | SQLServer |    |   Stripe  |    |   Brevo   |
    +-----------+    +-----------+    +-----------+
                           |
                           v
                      +---------+
                      |  Seq    |
                      +---------+
```

## Future Improvements

Based on the current architecture, the most realistic next steps are:

- add a validation pipeline so FluentValidation validators are executed automatically
- tighten the current permissive CORS policy for production
- add a production-ready frontend build/deployment strategy for `Gym.Client`
- add richer API integration test coverage
- externalize seeded admin credentials and other dev-only defaults
- add health checks for API dependencies
- add observability beyond Seq, such as metrics/tracing

## Contributing

1. Create a feature branch.
2. Keep changes aligned with the current layered architecture.
3. Prefer adding or updating tests alongside behavior changes.
4. Follow the existing MediatR feature organization under `Gym.Application/Features`.
5. Keep package versions centralized in `Directory.Packages.props`.
6. Run build and tests before opening a PR:

```bash
dotnet build GymSystem.slnx
dotnet test tests/Gym.Domain.UnitTests/Gym.Domain.UnitTests.csproj
dotnet test tests/Gym.Application.UnitTests/Gym.Application.UnitTests.csproj
dotnet test tests/Gym.Application.SubcutaneousTests/Gym.Application.SubcutaneousTests.csproj
```

No license file was found in the repository at the time of writing, so a license section is intentionally omitted.
