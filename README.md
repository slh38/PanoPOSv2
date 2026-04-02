# Pano POS

Pano POS, sade ve moduler bir cekirdek satis altyapisi olarak baslatilan .NET 8 tabanli projedir.

## Proje Yapisi

- `src/PanoPos.Domain`
- `src/PanoPos.Application`
- `src/PanoPos.Infrastructure`
- `src/PanoPos.WebApi`
- `tests/PanoPos.Tests`

## Gereksinimler

- .NET SDK 8 veya ustu

## Calistirma

1. Paketleri geri yukleyin:

```powershell
dotnet restore PanoPos.sln --configfile NuGet.Config
```

2. Uygulamayi baslatin:

```powershell
dotnet run --project src/PanoPos.WebApi/PanoPos.WebApi.csproj --no-launch-profile
```

3. Swagger arayuzune gidin:

- `http://localhost:5000/swagger`

4. Health endpoint'ini test edin:

- `GET http://localhost:5000/api/v1/system/health`

## Build

```powershell
dotnet build PanoPos.sln --configfile NuGet.Config
```

## Test

```powershell
dotnet test PanoPos.sln --configfile NuGet.Config
```
