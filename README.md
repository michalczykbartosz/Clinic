# 🏥 ClinicManager 2.0

System zarządzania przychodnią medyczną. Projekt zaliczeniowy z ASP.NET Core 10.

## ⚙️ CI/CD (GitHub Actions)
W repozytorium skonfigurowany jest automatyczny workflow (plik `dotnet-ci.yml`), który uruchamia się przy każdym pushu lub Pull Requeście do gałęzi `main`.

Wykonuje on następujące kroki:
1. Pobranie najnowszego kodu.
2. Konfiguracja środowiska .NET 10.
3. `dotnet build` - kompilacja aplikacji.
4. `dotnet test` - weryfikacja za pomocą testów jednostkowych (NUnit).