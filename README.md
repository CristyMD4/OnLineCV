# Online CV

Interactive Blazor WebAssembly CV for Corniță Cristian. The app includes a responsive online CV, project case studies, professional details, education and certificates, compact print styles for PDF export, and JSON translations for English, Romanian, and Russian.

## Scripts

```bash
dotnet run
dotnet build
```

## Structure

- `Pages/Home.razor` contains the Blazor CV rendering and interactions.
- `Models/CvContent.cs` contains the typed CV content model.
- `wwwroot/data/translations.json` contains the English, Romanian, and Russian translation resources.
- `wwwroot/css/app.css` contains the screen, responsive, and print layouts.
- `wwwroot/favicon.svg` is the browser favicon.

Use the `Print CV` button or browser print preview to export the print-optimized version.
