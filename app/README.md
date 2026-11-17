# Деканат та успішність студентів

Цю папку потрібно відкрити у Visual Studio Code.

- `DEMO.cmd` — демонстраційний запуск із 4 студентами.
- `RUN.cmd` — звичайна робота з `data/academic.json`.
- `TEST.cmd` — 36 автоматичних перевірок бібліотеки та JSON-сховища.
- `AcademicRecords.sln` — рішення з бібліотекою, консоллю та перевірками.
- `.vscode/tasks.json` — складання, запуск, демонстрація та перевірки.

Короткий опис проєкту й порядок запуску наведено в `../README.md`; діаграми розміщено в `../docs/diagrams`.

```powershell
dotnet build AcademicRecords.sln -c Release -m:1
dotnet run --project src/AcademicRecords.Console -c Release
dotnet run --project src/AcademicRecords.Console -c Release -- --data samples/demo.json
```

Для перенесеного комплекту потрібен .NET 8 SDK. Зовнішні NuGet-пакети не використовуються.

