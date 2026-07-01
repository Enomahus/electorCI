# ElectorCI CLAUDE.md

## Project Overview

**ElectorCI** - A monorepos with an Angular 21 frontend and ASP.NET Core (.Net 10) backend

- Frontend: `src/front/web-client/`
- Backend: `src/back/`
- E2E tests: `src/front/e2e`
- CI/CD pipelines: `.github/workflows/`

## Development Setup

## Local URLs

| Service  | URL                                       |
| -------- | ----------------------------------------- |
| Frontend | https://localhost:44026/                  |
| Swagger  | http://localhost:44200/swagger/index.html |
| ReDoc    | http://localhost:44200/docs/index.html    |
| Jaeger   | http://localhost:41200/search             |

### Database (SQL Server 2022)

- Host: `localhost:43200`
- User: `sa`
- Password: `Inay@h225`
- Database: `ElectorDb`

### Frontend Commands (from `src/front/web-client/`)

```bash
npm start                # ng serve
npm run lint             # ESLint check
npm run lint-fix         # ESLint auto-fix
npm generate-api-client  # Regenate NSwag API client from backend Swagger
```

### Testing Workflow

### NSwag Workflow (backend + frontend changes)

When a task requires both backend API changes and frontend changes:

1. Implement all backend changes first
2. **Stop and tell the user**: "Backend changes are done. Please run `npm run generate-api-client` from `src/front/web-client/`, then let me know when it's done."
3. **Wait for the user to confirm** the NSwag client has been regenerated
4. Only then implement the frontend changes (the regenerated client will have the updated types/methods)

---

## Backend Architecture

**Vertical Slice Architecture** — each use case is self-contained within a feature folder.

### Project Structure (layers by prefix)

```
00-Web                     # ASP.NET Core entry point
01-ServicesConfiguration   # DI setup
10-Application             # Base application layer
11-Application.Features    # All use cases (the main working area)
12-Application.Common      # Shared application logic
13-Application.Resources   # Resource strings / error codes
20-Infrastructure.*        # Data access, email, external services
Pcea.Core.*                # Authorization subsystem
```

### Adding a New Use Case

Create a folder under `11-Application.Features` for the feature (if not existing), then add a subfolder for the use case containing:

```
MyFeature/
  MyUseCase/
    MyUseCaseController.cs     # API endpoint definition
    MyUseCaseCommand.cs        # (or Query.cs) — model + validator + handler
    MyUseCaseResponse.cs       # Response DTO (optional)
    MyUseCase.md               # Swagger documentation for this use case
```

### Controller Pattern

```csharp
[ExcludeFromCodeCoverage]
[ApiController]
[Route("api/my-feature")]
public class MyUseCaseController : ControllerBase
{
    [HttpPost("my-action")]
    [SwaggerOperation(OperationId = "MyAction", Summary = "Brief description")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(MyUseCaseResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Handle([FromBody] MyUseCaseCommand command, ...)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }
}
```

For GET endpoints, each query param must be declared individually with `[FromQuery]`, then the Query object is manually reconstructed.

### Command / Query Pattern

A single file typically contains the model, validator, and handler:

```csharp
// Model
public record MyUseCaseCommand(string Field1, int Field2) : IRequest<MyUseCaseResponse>;

// Validator (FluentValidation)
public class MyUseCaseCommandValidator : AbstractValidator<MyUseCaseCommand>
{
    public MyUseCaseCommandValidator()
    {
        RuleFor(x => x.Field1).NotEmpty().WithMessage("Field1IsRequired");
        // Use error codes (not human-readable messages) — frontend handles translation
    }
}

// Handler
public class MyUseCaseCommandHandler : IRequestHandler<MyUseCaseCommand, MyUseCaseResponse>
{
    private readonly WritableDbContext _db; // Use ReadOnlyDbContext for Queries

    public MyUseCaseCommandHandler(WritableDbContext db) { _db = db; }

    public async Task<MyUseCaseResponse> Handle(MyUseCaseCommand request, CancellationToken ct)
    {
        using var activity = ActivitySourceLog.CQRS.Start().AddParameters(request);
        // ... business logic
    }
}
```

**Rule:** Commands use `WritableDbContext`, Queries use `ReadOnlyDbContext`.

### Validation Error Response Shape

```json
{
  "data": {
    "code": "Validation",
    "kind": "Validation",
    "additionalData": {
      "FieldName": ["ErrorCode1", "ErrorCode2"]
    }
  }
}
```

---

## Backend Tests

All unit tests live in `src/back/tests/Application.UnitTest/`, organized by feature.

### Test Naming Convention

```
{CommandName}Test_Should{ExpectedResult}_When{Condition}
```

### Test Structure (xUnit + FluentAssertions + NSubstitute)

```csharp
[Fact]
public async Task MyCommandTest_ShouldReturnResult_WhenInputIsValid()
{
    // Arrange
    var (serviceProvider, mocks) = SetupTestServiceProvider(t =>
    {
        t.SomeService().SomeMethod(Arg.Any<string>()).Returns("value");
    });
    var command = new MyUseCaseCommand("value");

    // Act
    var result = await serviceProvider.SendAsync(command);

    // Assert
    result.Should().NotBeNull();
    mocks.SomeService.Received().SomeMethod("value");
}
```

For expected exceptions:

```csharp
await FluentActions.Invoking(() => serviceProvider.SendAsync(command))
    .Should().ThrowAsync<MyException>();
```

---

## Frontend Architecture

**Angular 21** with standalone components. API services are auto-generated by **NSwag** from the backend Swagger.

### Signals

Prefer Angular signals over traditional `@Input()` decorators and class properties:

- Use `input()` / `input.required()` instead of `@Input()`
- Use `computed()` instead of getters that derive values from inputs
- Use `signal()` for mutable component state where applicable

After any backend API change, regenerate the API client:

```bash
npm run generate-api-client
```

NSwag config: `src/front/web-client/server.nswag.json`

### Internationalisation (ngx-translate)

**All user-visible text must go through ngx-translate** — never hardcode strings in templates or components.

In templates:

```html
<span>{{ 'MY_FEATURE.MY_KEY' | translate }}</span> <button>{{ 'COMMON.SAVE' | translate }}</button>
```

In components (inject `TranslateService`):

```typescript
constructor(private translate: TranslateService) {}

const label = this.translate.instant('MY_FEATURE.MY_KEY');
```

Translation files are located in `src/front/web-client/src/assets/i18n/`. Add new keys there when introducing any new text only in fr.json file, don't insert in en.json

---

## Git Workflow

### Branch Naming

```
{initials}/{ticket-number}-{2-3-word-description}
# Example: ALRI/55631-simulateur
```

### Commit Messages

Follow [Conventional Commits](https://www.conventionalcommits.org/en/v1.0.0/):

```
feat: add export to PDF
fix: correct validation on budget field
chore: update dependencies
```

### PR Workflow

1. Branch from `origin/develop`
2. Rebase on `origin/develop` before pushing
3. Create PR on GitHub targeting `develop`
4. Requires 1 reviewer approval
5. Prefer **Squash Commit** when completing the PR

### Branch → Environment

- `develop` → Dev auto-deploy
- `qa` → QA auto-deploy
- `master` → Production

---

## Code Style

- **C#:** CSharpier (`.csharpierrc` at root)
- **TypeScript/HTML:** Prettier (`.prettierrc`) + ESLint — 110 char print width, LF, 2-space indent
- Run `npm run lint-fix` before committing frontend changes

---

## Entity Framework Migrations

The project uses Code First with EF8 migrations. When modifying database entities, add a migration in the appropriate infrastructure project.

---

## Observability (Jaeger)

Add tracing to any method with:

```csharp
using var activity = ActivitySourceLog.<Layer>.Start();
// e.g., ActivitySourceLog.CQRS.Start().AddParameters(request);
```

Access traces at http://localhost:41200/search (local dev).

---

## E2E Tests (Playwright)

Tests live in `src/front/e2e/tests/desktop/`. Run with the `playwright` CLI from `src/front/e2e/`.

### Locator priority

Prefer semantic locators in this order — they're more resilient to HTML/CSS refactors:

1. `getByRole` — best for interactive elements (buttons, headings, tabs, inputs via label)
2. `getByLabel` — for form inputs associated with a `<label>`
3. `getByText` — for verifying visible text content
4. `getByPlaceholder` — for inputs without a label
5. CSS class / ID selectors (`.panel-infos`, `#brand`) — only when no semantic alternative exists

### Assertions: verify content, not just presence

**Don't** check only that an element with a given ID is present:

```typescript
// ❌ only proves the element exists in the DOM
await expect(page.locator('#reference')).toBeVisible();
await expect(page.getByRole('heading', { level: 1 })).toBeVisible();
```

**Do** assert the actual text displayed to the user:

```typescript
// ✅ proves the correct content is rendered
await expect(page.getByRole('heading', { level: 1 })).toHaveText('Tester une référence de panneau');
await expect(page.getByLabel('Reference fabricant')).toBeVisible();
await expect(page.locator('.panel-infos')).toContainText('Edilians');
```

Use `toHaveText` for exact matches, `toContainText` when the element may contain additional text.

### Helpers

| Helper                               | Purpose                                                |
| ------------------------------------ | ------------------------------------------------------ |
| `setTestLanguage(test)`              | Forces `fr` locale via `localStorage` before each test |
| `setTestTime(test)`                  | Fixes the clock for deterministic date-dependent tests |
| `selectComboOption(page, id, label)` | Selects an item in a Kendo DropDownList                |
| `getCurrentYearAndTrimester(page)`   | Reads the current trimester from the page              |
| `addTrimesters(year, t, n)`          | Computes a relative trimester offset                   |
| `periodLabel(year, t)`               | Returns the displayed period string for a trimester    |

### Test structure

```typescript
test.describe('Feature name', () => {
  setTestTime(test);
  setTestLanguage(test);

  test('Action and expected result in plain French', async ({ page }) => {
    // Arrange: navigate and set up state
    await page.goto('/route');

    // Act: perform user interaction
    await page.getByRole('button', { name: 'Rechercher' }).click();

    // Assert: verify visible text, not just DOM presence
    await expect(page.getByRole('heading', { level: 1 })).toHaveText('Expected title');
    await expect(page.locator('.result-list')).toContainText('Expected value');
  });
});
```
