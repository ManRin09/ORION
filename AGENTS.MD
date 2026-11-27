# AGENTS.MD — Contexto Operativo para Agentes de Código

Este archivo define la ventana de contexto técnica para cualquier agente de IA o desarrollador que trabaje en el monorepo **Orion**. Toda modificación debe respetar las fronteras arquitectónicas, convenciones y validaciones descritas aquí.

---

## 1. Mapa del Monorepo

| Ruta | Stack | Rol | Estado actual |
| :--- | :--- | :--- | :--- |
| `Api/` | .NET 10, ASP.NET Core, EF Core 10, Npgsql, FluentValidation, BCrypt, JWT | Backend REST API con Clean Architecture | Funcional (`UsersController` + `AuthController`), con pruebas unitarias y dockerizado para Render |
| `Frontend/` | Angular 22.3, TypeScript 6, Tailwind CSS v4, Vitest | SPA con temática RPG 16-bit (`/`, `/login`, `/register`, `/adventurers`, `/auth/callback`) | Conectado a `Orion.Api` mediante un único `AuthService` (`/api/auth` y `/api/users`), `authInterceptor` (Bearer JWT) y `authGuard` |

---

## 2. Ubicación Exacta de Piezas Clave

### Backend (`Api/`)
- **Solución:** `Api/src/Orion.sln`
- **Entrypoint & Pipeline HTTP:** `Api/src/Orion.Api/Program.cs`
- **Controladores:** `Api/src/Orion.Api/Controllers/{UsersController,AuthController}.cs` (`/api/users`, `/api/auth`)
- **Manejo global de excepciones:** `Api/src/Orion.Api/Middlewares/GlobalExceptionMiddleware.cs`
- **Factory de migraciones (Neon direct endpoint):** `Api/src/Orion.Api/Persistence/AppDbContextFactory.cs`
- **Contenedorización Render:** `Api/Dockerfile`, `Api/.dockerignore`
- **Dominio (`Orion.Domain`):**
  - Entidad agregada: `Api/src/Orion.Domain/Entities/User.cs`
  - Enums: `Api/src/Orion.Domain/Enums/{AuthProvider,PlanType,UserRole}.cs`
  - Excepciones: `Api/src/Orion.Domain/Exceptions/{DomainException,EmailAlreadyExistsException,UsernameAlreadyExistsException,UserNotFoundException}.cs`
  - Contrato repositorio: `Api/src/Orion.Domain/Interfaces/IUserRepository.cs`
- **Aplicación (`Orion.Application`):**
  - Registro DI: `Api/src/Orion.Application/DependencyInjection.cs`
  - Casos de uso: `Api/src/Orion.Application/Services/UserService.cs`
  - Interfaces: `Api/src/Orion.Application/Interfaces/{IUserService,IPasswordHasher}.cs`
  - DTOs: `Api/src/Orion.Application/DTOs/Users/{RegisterUserRequest,UpdateUserProfileRequest,UserResponse}.cs`, `Api/src/Orion.Application/DTOs/Common/PagedResponse.cs`
  - Validadores: `Api/src/Orion.Application/Validators/{PasswordRules,ProfileRules,RegisterUserRequestValidator,UpdateUserProfileRequestValidator}.cs`
- **Infraestructura (`Orion.Infrastructure`):**
  - Registro DI: `Api/src/Orion.Infrastructure/DependencyInjection.cs`
  - DbContext y mapeo: `Api/src/Orion.Infrastructure/Persistence/AppDbContext.cs`, `Api/src/Orion.Infrastructure/Config/UserConfiguration.cs`
  - Migraciones: `Api/src/Orion.Infrastructure/Migrations/`
  - Repositorio y hash: `Api/src/Orion.Infrastructure/Repositories/UserRepository.cs`, `Api/src/Orion.Infrastructure/Security/BcryptPasswordHasher.cs`
- **Pruebas (`Orion.UnitTests`):**
  - `Api/tests/Orion.UnitTests/{Security,Services,Validators}/` (xUnit + NSubstitute)

### Frontend (`Frontend/`)
- **Configuración raíz:** `Frontend/package.json`, `Frontend/angular.json`, `Frontend/tsconfig.json`
- **App shell & rutas:** `Frontend/src/app/{app.ts,app.config.ts,app.routes.ts}`, `Frontend/src/index.html`, `Frontend/src/styles.css`
- **Entornos (`src/environments/`):** `environment.ts` (Render: `https://orion-backend-yar4.onrender.com/api`) y `environment.development.ts` (Local: `http://localhost:5092/api`, reemplazado en `ng serve` mediante `fileReplacements` en `angular.json`)
- **Modelos y datos:** `Frontend/src/app/models/{auth.models.ts,quest-landing.models.ts,user.models.ts}`, `Frontend/src/app/data/quest-landing.data.ts`
- **Servicio HTTP único:** `Frontend/src/app/services/auth.service.ts` (`/api/auth` y CRUD `/api/users`)
- **Interceptores y Guards:** `Frontend/src/app/interceptors/auth.interceptor.ts` (`Authorization: Bearer` + manejo `401`), `Frontend/src/app/guards/auth.guard.ts` (`authGuard`, `guestGuard`)
- **Vistas (lazy-loaded):** `Frontend/src/app/views/{home,login,register,adventurers,auth-callback}/`
- **Componentes:**
  - `Frontend/src/app/components/landing/` (`navbar`, `hero-section`, `battle-arena`, `quest-map`, `zone-card`, `guild-inventory`, `questing-comparison`, `save-point-cta`, `footer`)
  - `Frontend/src/app/components/auth/` (`login-card`, `register-card`, `cipher-requirements`, `familiar-guide-card`)
  - `Frontend/src/app/components/shared/` (`pixel-icon`, `retro-button`)

---

## 3. Reglas de Arquitectura y Convenciones Obligatorias

### Reglas del Backend (.NET 10)
1. **Dirección de dependencias (Clean Architecture):**
   - `Orion.Domain` **no** debe tener dependencias de frameworks externos ni EF Core.
   - `Orion.Application` solo referencia `Orion.Domain` y `FluentValidation`.
   - `Orion.Infrastructure` referencia `Orion.Application` (y transitivamente `Orion.Domain`).
   - `Orion.Api` compone `Orion.Application` + `Orion.Infrastructure`.
2. **Invariantes del agregado `User`:**
   - Constructor privado; instanciar únicamente mediante `User.CreateLocal(...)` o `User.CreateGitHub(...)`.
   - Normalizar `email` y `username` siempre en minúsculas con `User.NormalizeEmail` / `User.NormalizeUsername`.
   - Todas las fechas (`CreatedAt`, `UpdatedAt`, `AcceptedTermsAt`) deben persistirse en **UTC**.
   - Borrado lógico únicamente (`user.Deactivate()` fija `IsActive = false`). `UserConfiguration` aplica `HasQueryFilter(u => u.IsActive)` de forma global, pero `ExistsByEmailAsync` y `ExistsByUsernameAsync` en `UserRepository` deben conservar `.IgnoreQueryFilters()` para impedir colisiones con cuentas desactivadas.
3. **Errores y Validación:**
   - Los endpoints en controladores no llevan bloques `try/catch` manuales para errores de dominio o validación: `GlobalExceptionMiddleware` traduce automáticamente a **RFC 9457 `ProblemDetails`**:
     - `ValidationException` / `DomainException` → `400 Bad Request`
     - `UserNotFoundException` → `404 Not Found`
     - `EmailAlreadyExistsException` / `UsernameAlreadyExistsException` / `DbUpdateException` con `PostgresErrorCodes.UniqueViolation` → `409 Conflict`
     - Excepciones no controladas → `500 Internal Server Error` (solo expone `exception.Message` en `Development`).
4. **Seguridad, JWT y Secretos:**
   - Hashing de contraseñas con `BcryptPasswordHasher` (`BCrypt.Net.BCrypt.EnhancedHashPassword` con `WorkFactor = 12`).
   - JWT configurado vía `IOptions<JwtSettings>` (`Api/src/Orion.Infrastructure/Security/JwtSettings.cs`): usa `Jwt__SecretKey` (>= 32 bytes), `Jwt__Issuer`, `Jwt__Audience` y `Jwt__ExpirationMinutes` en Render (con fallback retrocompatible a `Jwt__Key` y `Jwt__AccessTokenExpirationMinutes`); emite claims `sub`, `email`, `name`, `unique_name`, `role` (`ClaimTypes.Role` + `"role"`) y `jti`.
   - GitHub OAuth configurado vía `IOptions<GitHubOAuthSettings>` (`GitHubOAuth__ClientId`, `GitHubOAuth__ClientSecret`, `GitHubOAuth__CallbackUrl`, `GitHubOAuth__FrontendRedirectUrl`), dejando placeholders vacíos en `appsettings.json`.
   - **Nunca** incluir ni commitear secrets reales en `appsettings.json`, `appsettings.Development.json` ni `.env`. Están excluidos en `.gitignore`, `Api/.gitignore` y `Api/.dockerignore`.
   - `AddInfrastructure` lanza `InvalidOperationException` al arranque si `ConnectionStrings:DefaultConnection` está vacía.
5. **Compatibilidad con Render (Contenedor):**
   - No ejecutar `app.UseHttpsRedirection()` fuera de `app.Environment.IsDevelopment()` (Render termina TLS en su proxy externo y envía HTTP al puerto `10000` o `$PORT`).
   - Mantener `GET /health` ligero (`200 OK`) sin consultar la base de datos en cada probe.
   - Mantener soporte dinámico de `AllowedOrigins` (arreglo o lista separada por `,`/`;`, p. ej. `https://orion-1frh.onrender.com`) junto con `http://localhost:4200` y `https://localhost:4200`, y Swagger condicional (`Development` o `ENABLE_SWAGGER=true`) con esquema OpenAPI **Bearer JWT**.

### Reglas del Frontend (Angular 22 + Tailwind v4)
1. **Estilo de componentes:**
   - Solo componentes **standalone** (sin `NgModules`), usando `ChangeDetectionStrategy.OnPush`.
   - Usar Angular **Signals** (`signal`, `computed`, `toSignal`) e `input()` / `output()` / `inject()`.
   - Formularios con `FormBuilder.nonNullable.group(...)` (`ReactiveFormsModule`).
2. **Reglas estrictas de TypeScript (`Frontend/tsconfig.json`):**
   - `noImplicitOverride`, `noPropertyAccessFromIndexSignature`, `noImplicitReturns`, `noFallthroughCasesInSwitch`, `strictInjectionParameters`, `strictInputAccessModifiers`.
3. **Paridad de validación Frontend ↔ Backend:**
   - Las 4 runas de contraseña en `Frontend/src/app/models/auth.models.ts` (`evaluatePasswordRunes`: mínimo 8 caracteres, carácter especial, mayúscula, minúscula) deben mantenerse sincronizadas con `Api/src/Orion.Application/Validators/PasswordRules.cs`.
   - Nota de contrato actual: en `RegisterUserRequest` el backend usa `MiddleName` y `AcceptTerms` (y opcionalmente `Username`, si es nulo `UserService` genera un slug único), mientras que en el frontend `RegisterCharacterPayload` usa `middleTitle` y `acceptedTerms`. Al conectar el servicio HTTP, mapear explícitamente estos nombres.
4. **Estética y diseño:**
   - Usar los tokens definidos en `@theme` de `Frontend/src/styles.css` (`--color-realm-*`, `--color-pixel-*`, `--font-display`, `--font-pixel`, `--font-mono`) y las clases utilitarias pixel-art existentes (`pixel-btn-primary`, `pixel-btn-secondary`, `pixel-shadow`, `save-point-frame`, `retro-crt-backdrop`, `hero-dot-grid`).

---

## 4. Comandos de Verificación y Trabajo Diario

### Backend (`Api/`)
```bash
# Compilar y ejecutar pruebas unitarias (30 tests con xUnit + NSubstitute)
dotnet test Api/src/Orion.sln

# Ejecutar API localmente (requiere ConnectionStrings:DefaultConnection en appsettings.Development.json o env var)
dotnet run --project Api/src/Orion.Api/Orion.Api.csproj

# Publicar artefacto Release (mismo comando usado en Dockerfile)
dotnet publish Api/src/Orion.Api/Orion.Api.csproj -c Release -o ./publish

# Migraciones EF Core (ejecutar desde Api/)
dotnet tool restore
dotnet ef migrations add <NombreMigracion> --project src/Orion.Infrastructure --startup-project src/Orion.Api
dotnet ef database update --project src/Orion.Infrastructure --startup-project src/Orion.Api
```
- **Nota sobre Neon y migraciones:** `AppDbContextFactory` convierte automáticamente `-pooler.` en el host directo de Neon para evitar bloqueos de PgBouncer durante `dotnet ef`. Si `DefaultConnection` está vacía o contiene `<NEON_`, usa `Host=localhost;Port=5432;Database=orion_design;Username=postgres;Password=postgres`.

### Frontend (`Frontend/`)
```bash
cd Frontend
npm install
npm start        # ng serve en http://localhost:4200
npm run build    # ng build (producción)
npm test         # ng test (Vitest + jsdom)
```

### Checklist antes de entregar cambios
1. Si modificaste `Api/`: ejecuta `dotnet test Api/src/Orion.sln` y verifica `0` errores y `0` pruebas fallidas.
2. Si modificaste `Frontend/`: ejecuta `npm run build` y `npm test` dentro de `Frontend/`.
3. Si tocaste entidades o configuraciones EF Core: crea la migración correspondiente y revisa `InitialCreateUsers.cs` / `AppDbContextModelSnapshot.cs`.
4. Revisa `git status` antes de hacer commit para garantizar que no se incluyan secretos (`appsettings.Development.json`, `.env`) ni carpetas `bin/`, `obj/` o `dist/`.
