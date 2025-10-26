# contacts-manager-dotnet

WinForms contacts manager for .NET 8 using SQLite — CRUD, import/export, validation, paging, and tests.

## Overview
A simple Windows Forms contacts manager built for .NET 8 that stores contacts in a local SQLite database. Features include:

- Create, read, update, delete (CRUD) contacts
- CSV import and export
- Duplicate detection (by Name + Email)
- Input validation: Name (required), Email (basic format), Mobile (digits only, 7–15 chars)
- Search / filter by name or email
- Paging support for large lists
- Database schema migration adding an Id primary key when required
- Unit tests using xUnit and in-memory SQLite
- Azure Pipelines CI configuration included

## Description
A Windows Forms contacts manager targeting .NET 8 that stores contacts in a local SQLite database. The app demonstrates a small but complete desktop data application with schema migration, validation, import/export, paging, search/filter, duplicate checking, and a test suite.

## Features
- Persistent storage using SQLite (data.db)
- Repository pattern (ContactsRepository) encapsulating data access
- CSV import/export (simple quoting support)
- Input validation (extracted to ValidationHelper for unit testing)
- Search/filter helper (SearchFilter) with unit tests
- Paging support in the repository and UI
- Unit tests for repository, validation, CSV import/export, and search/filter
- Azure Pipelines config (azure-pipelines.yml) for CI

## Running the app
- Requires .NET 8 SDK
- From repo root:

  dotnet run --project SQLConnection/SQLConnection.csproj

## Running tests
- From repo root:

  dotnet test

The test project uses xUnit and Microsoft.Data.Sqlite for in-memory database tests.

## CI
An `azure-pipelines.yml` file is included to build the solution and run tests on a Windows hosted agent using the .NET 8 SDK. You can also convert the steps to GitHub Actions if you prefer.

## Topics / tags
winforms, dotnet8, sqlite, contacts, crud, csv, testing, xunit, ci, azure-pipelines

## Contribution
Contributions welcome — open an issue or a pull request. Suggested next improvements:
- Add stronger email validation or integrate a validation library
- Add integration/UI tests (e.g. FlaUI) to exercise the WinForms UI
- Add localization and better error handling

## License

## Repository setup notes
- Recommended repo name: `contacts-manager-dotnet` (clear and descriptive).
- Recommended branch: `main`.
- A `.gitignore` for .NET/Visual Studio is already included.

