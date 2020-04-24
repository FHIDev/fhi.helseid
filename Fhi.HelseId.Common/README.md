Dette prosjektet er i bruk i andre prosjekter vha at prosjektet pakkes ned som en NuGet-pakke og distribueres på Fhi sine NuGet feeds.


## Bygge og laste opp ny versjon

1. Gjør ønskelige endringer i prosjektet
2. Oppdater versjon i `Fhi.HelseId.Common.csproj` fila ved å øke Version, AssemblyVersion og FileVersion.

Eksempelvis:
1.1.0 => 1.2.0
1.1.0.0 => 1.2.0.0


```
 <PropertyGroup>
        <TargetFramework>netcoreapp3.1</TargetFramework>
        <Configurations>Debug;Release</Configurations>
        <Platforms>AnyCPU</Platforms>
=>      <Version>1.2.0</Version>
        <Authors>IT-Systemer Oslo</Authors>
        <Company>Fhi</Company>
        <GeneratePackageOnBuild>false</GeneratePackageOnBuild>
=>      <AssemblyVersion>1.2.0.0</AssemblyVersion>
=>      <FileVersion>1.2.0.0</FileVersion>
        <PackageReleaseNotes>En beskrivelse av mine endringer</PackageReleaseNotes>
    </PropertyGroup>
```

3. Installer nuget.exe fra https://nuget.org/downloads, og legg denne i PATH (eller bruk Chocolatey og skriv 'choco install NuGet.CommandLine' fra en Admin CMD prompt)

4. I Visual Studio: Åpne 'Developers command prompt' ved å klikke 
```
Tools => Command Line => Developer Command Prompt
```

5.  Kjør følgende kommandoer:
```
cd Fhi.HelseId.Common
push-nuget-pakke.bat
```
7. Lag feature branch. Commit, meldingen skal inneholde noe på denne formen, hvor NY_VERSJON erstattes av den nye versjonen:

```Oppdater Fhi.HelseId.Common til versjon NY_VERSJON```

8. Opprett pull request - få en peer til å gjøre en peer review, sjekk at Fhi.HelseId.Common bygger, før feature branchen merges inn til Master.
