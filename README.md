<div align="center">
  <h1>🏆 Tournament System (GFlow)</h1>
  <p><em>Uniwersalna platforma do tworzenia i zarządzania turniejami (e-sport, gry planszowe i inne).</em></p>

  <!-- Badges -->

<a href="https://react.dev/"><img src="https://img.shields.io/badge/React-19-61DAFB?logo=react&logoColor=black&style=for-the-badge" alt="React" /></a>
<a href="https://dotnet.microsoft.com/"><img src="https://img.shields.io/badge/.NET-9.0-512BD4?logo=dotnet&logoColor=white&style=for-the-badge" alt=".NET" /></a>
<a href="https://www.postgresql.org/"><img src="https://img.shields.io/badge/PostgreSQL-336791?logo=postgresql&logoColor=white&style=for-the-badge" alt="PostgreSQL" /></a>
<a href="https://github.com/AliasMaster/tournament-system/blob/main/LICENSE"><img src="https://img.shields.io/badge/License-MIT-green.svg?style=for-the-badge" alt="License" /></a>

</div>

<br />

## 📖 Opis Projektu

**Tournament System (GFlow)** to kompleksowa aplikacja dla organizatorów i graczy, którzy potrzebują intuicyjnego narzędzia do automatycznego generowania drabinek turniejowych, śledzenia wyników i archiwizacji historii rozgrywek. Rozwiązuje problem ręcznego i żmudnego zarządzania turniejami, udostępniając czytelną, interaktywną platformę.

<br />

## 📑 Spis Treści

- [✨ Główne Funkcjonalności](#-główne-funkcjonalności)
- [💻 Stack Technologiczny](#-stack-technologiczny)
- [🚀 Instalacja i uruchomienie](#-instalacja-i-uruchomienie)
- [⚙️ Zmienne środowiskowe](#️-zmienne-środowiskowe)
- [💡 Sposób użycia](#-sposób-użycia)
- [👥 Zespół](#-zespół)

<br />

## ✨ Główne Funkcjonalności

- 🔐 **Autoryzacja (JWT):** Rejestracja i logowanie użytkowników.
- 👥 **Zarządzanie uczestnikami:** Dodawanie zawodników indywidualnych oraz całych drużyn.
- 🌳 **Drabinki turniejowe:** Automatyczne generowanie drabinek w różnych systemach rozgrywek.
- 👁️ **Interaktywna wizualizacja:** Przejrzysty podgląd struktury turnieju.
- ⏱️ **Wyniki na żywo:** Szybkie wprowadzanie i prezentacja zaktualizowanych wyników.
- 📚 **Historia:** Przechowywanie historycznych danych o turniejach i meczach.
- 🌍 **Wielojęzyczność:** Obsługa tłumaczeń interfejsu (i18next).

<br />

## 💻 Stack Technologiczny

### Frontend

- **React 19**
- **TypeScript**
- **Vite**
- **React Router** & **React i18next**

### Backend

- **.NET 9.0 (C#)** – Clean Architecture
- **Entity Framework Core**
- **PostgreSQL / SQLite** (Baza danych)
- **JWT** (System.IdentityModel.Tokens.Jwt)

<br />

## 🚀 Instalacja i uruchomienie

Sklonuj repozytorium na swój dysk:

```bash
git clone https://github.com/AliasMaster/tournament-system.git
cd tournament-system
```

### Uruchomienie Backendu (.NET)

1. Przejdź do folderu backendu:
   ```bash
   cd backend
   ```
2. Skonfiguruj bazę danych (jeśli to wymagane) – domyślnie używany jest plik `appsettings.json`.
3. Uruchom serwer używając wbudowanego skryptu (Windows):
   ```powershell
   .\run.ps1
   ```
   _lub za pomocą CLI .NET:_
   ```bash
   dotnet run --project src/GFlow.Api/GFlow.Api.csproj
   ```

### Uruchomienie Frontendu (React + Vite)

1. Przejdź do folderu frontendu (w nowym terminalu):
   ```bash
   cd frontend
   ```
2. Zainstaluj zależności:
   ```bash
   npm install
   ```
3. Uruchom aplikację w trybie deweloperskim:
   ```bash
   npm run dev
   ```

<br />

## ⚙️ Zmienne środowiskowe (.env)

> **Sugestia:** Zależnie od ostatecznej konfiguracji, możesz chcieć utworzyć plik `.env` w katalogu frontendu (dla adresów API, np. `VITE_API_URL=http://localhost:5000`) lub zmodyfikować `appsettings.json` na backendzie.

Przykładowy `backend/src/GFlow.Api/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=TournamentDb;Username=postgres;Password=TwojeHaslo"
  },
  "JwtSettings": {
    "Secret": "TWOJ_SUPER_TAJNY_KLUCZ_JWT_12345",
    "Issuer": "GFlow",
    "Audience": "GFlowUsers"
  }
}
```

<br />

## 💡 Sposób użycia

1. Załóż konto jako organizator i zaloguj się do systemu.
2. Utwórz nowy turniej podając jego nazwę, typ (np. planszówki, CS:GO) oraz datę.
3. Dodaj uczestników ręcznie lub udostępnij im link do samodzielnej rejestracji.
4. Kliknij **"Generuj Drabinkę"**, a system ułoży harmonogram spotkań.
5. Aktualizuj wyniki w trakcie trwania wydarzenia.

> _[📸 Tutaj warto dodać w przyszłości GIF lub screenshot pokazujący interfejs aplikacji oraz wygląd drabinki turniejowej]_

<br />

## 👥 Zespół

| Rola           | Imię i Nazwisko                                      |
| -------------- | ---------------------------------------------------- |
| PM / Tech Lead | [Piotr Maj](https://github.com/AliasMaster)          |
| Dev            | [Bartosz Jędryka](https://github.com/JedrBart)       |
| QA             | [Aleksandra Kuś](https://github.com/AleksandraKus11) |
| Dev            | [Wojciech Pędziwiatr](https://github.com/Wojtek4321) |
| Dev            | [Adrian Suchenia](https://github.com/Suchy777)       |
