# Pano POS

Pano POS, sade ve moduler bir cekirdek satis altyapisi olarak baslatilan .NET 8 tabanli projedir.

## Proje Yapisi

- `src/PanoPos.Desktop`
- `src/PanoPos.Domain`
- `src/PanoPos.Application`
- `src/PanoPos.Infrastructure`
- `src/PanoPos.WebApi`
- `tests/PanoPos.Tests`

## Gereksinimler

- .NET SDK 8 veya ustu

## Calistirma

### Backend

1. Paketleri geri yukleyin:

```powershell
dotnet restore PanoPos.sln --configfile NuGet.Config
```

2. API uygulamasini baslatin:

```powershell
dotnet run --project src/PanoPos.WebApi/PanoPos.WebApi.csproj --no-launch-profile
```

3. Swagger arayuzune gidin:

- `http://localhost:5000/swagger`

4. Health endpoint'ini test edin:

- `GET http://localhost:5000/api/v1/system/health`

### Desktop

Desktop istemci WinForms + DevExpress ile `src/PanoPos.Desktop` altindadir.

Desktop tarafinda su akislari bulunur:
- PIN login
- ana ekran tile menu
- hizli satis ekrani
- parcali tahsilat ekrani

Desktop calistirma adimlari:

1. `src/PanoPos.Desktop/App.config` icinde API adresini ve cihaz id bilgisini ayarlayin.

Ornek:

```xml
<appSettings>
  <add key="BaseApiUrl" value="http://localhost:5296" />
  <add key="CihazId" value="1" />
</appSettings>
```

2. Desktop projeyi build edin:

```powershell
dotnet build src/PanoPos.Desktop/PanoPos.Desktop.csproj
```

3. Desktop uygulamayi calistirin:

- `src/PanoPos.Desktop/bin/Debug/net8.0-windows/PanoPos.Desktop.exe`

### Desktop Adimlari

1. Uygulama acilinca PIN login ekrani gelir.
2. Basarili giriste ana ekran tile menu acilir.
3. `Hizli Satis` secilince satis ekrani acilir.
4. Urun karti veya barkod ile sepet olusturulur.
5. `Beklet` ile siparis beklemeye alinabilir.
6. `Nakit`, `Kredi Karti` veya `Parcali` ile tahsilat ekranina gecilir.
7. Tahsilat ekraninda parcali odeme yapilabilir.

## Build

```powershell
dotnet build PanoPos.sln --configfile NuGet.Config
```

## Test

```powershell
dotnet test PanoPos.sln --configfile NuGet.Config
```
