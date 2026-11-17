# Курсова робота з ООП — варіант 4

**Тема:** Деканат та успішність студентів.

Це консольний застосунок на C# для керування студентами, викладачами, дисциплінами, зарахуваннями та оцінками. Проєкт використовує .NET 8 SDK.

## Склад репозиторію

- `app/src/` — консольний інтерфейс і бібліотека з логікою обліку.
- `app/tests/` — автоматичні перевірки правил програми та JSON-сховища.
- `app/samples/demo.json` — приклад демонстраційних даних.
- `docs/diagrams/` — діаграми класів і сервісів у PNG, SVG та PlantUML.

## Запуск

Установіть .NET 8 SDK і відкрийте папку `app` у Visual Studio Code. `app/global.json` вибирає SDK 8.

- `DEMO.cmd` — запуск із демонстраційними даними.
- `RUN.cmd` — звичайний запуск.
- `TEST.cmd` — автоматичні перевірки.

Команди термінала з папки `app`:

```powershell
dotnet build AcademicRecords.sln -c Release -m:1
dotnet run --project src/AcademicRecords.Console -c Release
dotnet run --project src/AcademicRecords.Console -c Release -- --data samples/demo.json
dotnet run --project tests/AcademicRecords.Tests -c Release
```

Оцінки приймаються в діапазоні 0–100, прохідний бал — 60. Незарахована або відсутня оцінка за дисципліну, на яку зараховано студента, вважається боргом.
