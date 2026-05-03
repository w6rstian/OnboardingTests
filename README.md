# Onboarding System - Aplikacja do Zarządzania Onboardingiem Pracowników

System wspomagający proces onboardingu nowych pracowników, umożliwiający zarządzanie kursami, zadaniami, spotkaniami (kalendarz) oraz komunikację (czat).

## Wymagania i Środowisko

Projekt został zbudowany przy użyciu następujących technologii i wersji:

- **IDE:** Visual Studio 2022
- **Framework:** .NET 8.0
- **Baza danych:** Entity Framework Core
- **Frontend:** ASP.NET Core MVC, Vanilla JS, Bootstrap 5
- **Główne biblioteki:**
  - FullCalendar 6 (Kalendarz)
  - SignalR (Czat w czasie rzeczywistym)
  - Chart.js (Statystyki)
- **Wersje narzędzi testowych:**
  - `Microsoft.Playwright.Xunit`: 1.59.0
  - `FakeItEasy`: 9.0.1
  - `FluentAssertio`: 8.8.0
  - `xunit`: 2.5.3 (SDK: 17.8.0)
  - `Microsoft.NET.Test.Sdk`: 17.8.0

## Uruchomienie Aplikacji

1.  **Inicjalizacja bazy danych:**
    W Package Manager Console (Visual Studio) lub CLI:
    ```bash
    dotnet ef database update --project Onboarding
    # lub w VS:
    Update-Database
    ```

2.  **Uruchomienie projektu:**
    - **CLI:** `dotnet run --project Onboarding`
    - **Visual Studio:** `Ctrl + F5`

    Aplikacja domyślnie dostępna pod adresem: `https://localhost:7231`

## Testowanie

### Testy Jednostkowe (Unit Tests)
Testy logiki biznesowej i kontrolerów:
```bash
dotnet test --filter Category=Unit
```

### Testy End-to-End (E2E) z Playwright
1.  **Instalacja przeglądarek Playwright (wymagane raz):**
    ```bash
    pwsh bin/Debug/net8.0/playwright.ps1 install
    ```
2.  **Uruchomienie:**
    - Upewnij się, że aplikacja działa (`Ctrl + F5`).
    - W Visual Studio: `Ctrl + R, T` lub przez CLI: `dotnet test OnboardingXUnitTests`

### Pokrycie kodu (Code Coverage)
1.  **Uruchomienie testów z kolekcją danych:**
    ```bash
    dotnet test --collect:"XPlat Code Coverage"
    ```
2.  **Generowanie raportu HTML:**
    ```bash
    dotnet tool restore
    dotnet reportgenerator -reports:"**/TestResults/**/coverage.cobertura.xml" -targetdir:"coveragereport" -reporttypes:Html
    ```

## Konta Testowe (Seed Data)

Po uruchomieniu aplikacji dostępne są następujące konta:
- **Admin:** `admin@mail.com` / `AdminPassword123!`
- **HR:** `hr@mail.com` / `HrPassword123!`
- **Buddy:** `buddy1@mail.com` / `BuddyPassword123!`
- **Nowy Pracownik:** `nowy1@mail.com` / `NowyPassword123!`
