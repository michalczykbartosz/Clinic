# ClinicManager

ClinicManager to projekt zaliczeniowy w ASP.NET Core MVC sluzacy do zarzadzania przychodnia medyczna.

Repozytorium zawiera aplikacje webowa, testy automatyczne oraz osobny projekt testow obciazeniowych dla endpointu API aktywnych wizyt.

Aplikacja jest klasyczna aplikacja serwerowa MVC. Warstwa kontrolerow obsluguje widoki Razor oraz jeden endpoint API. Dostep do danych jest realizowany przez Entity Framework Core i serwisy aplikacyjne rejestrowane w kontenerze DI.

## Technologie

- .NET 10
- ASP.NET Core MVC
- ASP.NET Core Identity
- Entity Framework Core
- SQL Server
- NLog
- Mapperly
- NUnit
- NBomber
- QuestPDF

## Struktura repozytorium

```text
Clinic/
+-- ClinicManager/                         # aplikacja MVC
+-- ClinicManager.Tests/                   # testy NUnit
|   +-- ClinicManager.LoadTests/           # test obciazeniowy TECH7
+-- .github/workflows/dotnet-ci.yml        # CI GitHub Actions
+-- Clinic.slnx
```

## Najwazniejsze funkcje aplikacji

Aktualny kod zawiera obsluge:

- rejestracji i logowania uzytkownikow,
- rol `Admin`, `Lekarz`, `Rejestratorka`, `Pacjent`,
- zarzadzania kontami i rolami,
- listy pracownikow,
- pacjentow oraz ich danych, PESEL i numeru ubezpieczenia,
- wyszukiwania pacjentow,
- wizyt, statusow wizyt i platnosci,
- grafiku lekarzy,
- procedur medycznych,
- notatek klinicznych,
- katalogu lekow,
- przypisywania lekow do wizyty,
- dokumentow pacjenta,
- raportow kosztow,
- profilu lekarza z edycja specjalizacji,
- endpointu API dla aktywnych wizyt.

Glowne moduly w aplikacji:

- `PatientsController` - lista pacjentow, szczegoly pacjenta, edycja danych, kartoteka, dane pacjenta zalogowanego jako `Pacjent`.
- `VisitsController` - lista wizyt, tworzenie wizyty, aktualizacja statusu i platnosci.
- `ScheduleController` - grafik lekarzy.
- `ProceduresController` - procedury przypisane do wizyty/pacjenta.
- `ClinicalNoteController` - notatki kliniczne.
- `MedicationController` - katalog lekow.
- `VisitMedicationsController` - przypisywanie lekow do wizyty.
- `PatientDocumentsController` - dokumenty pacjenta.
- `ReportController` - raport kosztow.
- `AdminUsersController` - konta, role i lista pracownikow.
- `DoctorProfileController` - profil lekarza i edycja specjalizacji.
- `VisitsApiController` - endpoint API dla aktywnych wizyt.

## Konfiguracja bazy danych

Domyslny connection string znajduje sie w `ClinicManager/appsettings.json`:

```json
"DefaultConnection": "Server=localhost;Database=ClinicManagerDb;Trusted_Connection=True;TrustServerCertificate=True;"
```

Jesli pracujesz lokalnie na SQL Server LocalDB, connection string mozna zmienic lokalnie na:

```json
"DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=ClinicManagerDb;Trusted_Connection=True;TrustServerCertificate=True;"
```

Po ustawieniu bazy danych uruchom migracje:

```powershell
dotnet ef database update --project ClinicManager
```

Migracje EF Core znajduja sie w katalogu:

```text
ClinicManager/Migrations
```

Projekt korzysta z `ClinicDbContext`, ktory dziedziczy po `IdentityDbContext`, dlatego w tej samej bazie znajduja sie tabele domenowe aplikacji oraz tabele ASP.NET Identity.

## Uruchomienie aplikacji

```powershell
dotnet restore
dotnet build
dotnet run --project ClinicManager
```

Adres lokalny zalezy od konfiguracji launch settings. W projekcie testow obciazeniowych domyslnie uzywany jest adres:

```text
http://localhost:5174
```

Jezeli port jest inny, nalezy uzyc adresu wypisanego przez `dotnet run`.

Typowa kolejnosc uruchomienia lokalnego:

1. ustaw connection string w `ClinicManager/appsettings.json`,
2. uruchom migracje EF Core,
3. uruchom aplikacje przez `dotnet run --project ClinicManager`,
4. wejdz w adres wypisany w terminalu.

## Konta i role

Aplikacja korzysta z ASP.NET Core Identity. Przy starcie aplikacji tworzone sa brakujace role:

- `Admin`
- `Lekarz`
- `Rejestratorka`
- `Pacjent`

Kod probuje nadac role `Admin` uzytkownikowi `admin@wp.pl`, jezeli taki uzytkownik istnieje. W repozytorium nie ma zahardcodowanego hasla administratora.

Role sa uzywane do ograniczania dostepu do czesci widokow i akcji kontrolerow. Przykladowo:

- pacjent moze korzystac z widokow przeznaczonych dla pacjenta,
- lekarz ma dostep do profilu lekarza i funkcji medycznych,
- rejestratorka obsluguje pacjentow i wizyty,
- admin zarzadza kontami i rolami.

Dokladne reguly dostepu sa zdefiniowane atrybutami `[Authorize]` w kontrolerach.

## Logowanie zdarzen

Projekt uzywa NLog. Konfiguracja znajduje sie w:

```text
ClinicManager/nlog.config
```

Logowanie jest podlaczone przez standardowy interfejs `ILogger<T>`.

W `Program.cs` skonfigurowano NLog jako provider logowania oraz globalna obsluge nieobsluzonych wyjatkow w pipeline HTTP.

## API

Projekt wystawia endpoint API dla aktywnych wizyt:

```http
GET /api/visits/active
```

Endpoint jest obslugiwany przez `ClinicManager/Controllers/Api/VisitsApiController.cs` i korzysta z danych wizyt, pacjentow oraz lekarzy.

OpenAPI jest mapowane w `Program.cs` przez:

```csharp
app.MapOpenApi();
```

Endpoint zwraca dane zlozone z wizyt, pacjentow i lekarzy. Jest wykorzystywany przez projekt testu obciazeniowego TECH7.

## Testy

Testy automatyczne sa w projekcie:

```text
ClinicManager.Tests/ClinicManager.Tests.csproj
```

Uruchomienie testow:

```powershell
dotnet test ClinicManager.Tests/ClinicManager.Tests.csproj
```

W CI uzywana jest konfiguracja Release:

```powershell
dotnet test --no-build --configuration Release
```

W projekcie testowym znajduja sie testy kontrolerow, serwisow i DTO. Czesc testow korzysta z SQLite in-memory przez pomocnicze klasy w katalogu:

```text
ClinicManager.Tests/TestSupport
```

Projekt `ClinicManager.Tests.csproj` celowo wyklucza katalog `ClinicManager.LoadTests`, poniewaz load testy sa osobnym projektem i nie powinny byc kompilowane jako zwykle testy NUnit.

## Test obciazeniowy TECH7

Projekt testu obciazeniowego znajduje sie tutaj:

```text
ClinicManager.Tests/ClinicManager.LoadTests
```

Test uzywa NBomber i wykonuje zapytania do:

```http
GET /api/visits/active
```

Parametry z kodu:

- 50 rownoleglych uzytkownikow,
- 100 iteracji na kopie scenariusza,
- raporty NBomber w formatach HTML, Markdown i CSV,
- dodatkowy raport PDF generowany przez QuestPDF.

Uruchomienie przy dzialajacej aplikacji:

```powershell
dotnet run --project ClinicManager.Tests/ClinicManager.LoadTests -- --base-url http://localhost:5174
```

Mozna tez podac katalog raportow:

```powershell
dotnet run --project ClinicManager.Tests/ClinicManager.LoadTests -- --base-url http://localhost:5174 --report-folder ./load-test-reports
```

Domyslnie projekt korzysta z:

```text
CLINIC_BASE_URL=http://localhost:5174
```

oraz zapisuje raporty do katalogu `reports` wzgledem katalogu uruchomieniowego aplikacji testu, jezeli nie podano `--report-folder`.

Wygenerowany PDF ma nazwe w formacie:

```text
tech7-active-visits-yyyyMMdd-HHmmss.pdf
```

Test obciazeniowy wymaga, zeby aplikacja `ClinicManager` byla uruchomiona przed startem load testu.

## CI/CD

Workflow GitHub Actions znajduje sie w:

```text
.github/workflows/dotnet-ci.yml
```

Uruchamia sie dla pushy i pull requestow do `main`. Kroki workflow:

1. checkout repozytorium,
2. instalacja .NET 10,
3. `dotnet restore`,
4. `dotnet build --no-restore --configuration Release`,
5. `dotnet test --no-build --configuration Release`.

## Pliki przesylane przez uzytkownikow

Dokumenty pacjentow sa przechowywane w katalogu:

```text
ClinicManager/wwwroot/uploads/patient-documents
```

W repozytorium znajduje sie plik `.gitkeep`, zeby katalog byl obecny po sklonowaniu projektu.

## Uwagi developerskie

- Mapperly jest uzywany w mapperach w katalogu `ClinicManager/Mappers`.
- Widoki Razor sa w katalogu `ClinicManager/Views`.
- Modele domenowe sa w katalogu `ClinicManager/Models`.
- DTO sa w katalogu `ClinicManager/DTOs`.
- Serwisy aplikacyjne sa w katalogu `ClinicManager/Services`.
- Pliki statyczne znajduja sie w `ClinicManager/wwwroot`.

Znane ostrzezenia przy buildzie moga dotyczyc paczek NuGet raportowanych przez `dotnet restore/build`, np. ostrzezen bezpieczenstwa lub zgodnosci paczek. Nie sa one ukrywane w README, bo wynik zalezy od aktualnych advisory NuGet oraz wersji SDK.
