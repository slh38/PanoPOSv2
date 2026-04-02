# Cekirdek Mimari

## Teknoloji Kararlari

- Backend: .NET 8 Web API
- ORM: EF Core
- Listeleme ve performansli sorgular: Dapper
- Veritabani: SQL Server
- Masaustu: DevExpress
- Web panel: ileride React
- Mobil: ileride Flutter

## Katmanlar

Proje sade katmanli olacak:
- Domain
- Application
- Infrastructure
- WebApi
- Tests

Amac gereksiz soyutlama degil, temiz ayrimdir.

## Gelistirme Stratejisi

Sira:
1. Core backend
2. Testler
3. DevExpress desktop
4. Web panel
5. Mobil
6. Lisans
7. ERP entegrasyonlari
8. Gelismis moduller

## Offline Yaklasimi

- Her cihaz kendi SQL Server veritabani ile calisabilir.
- Ileride merkez PC veya merkez servis ile senkron olacak.
- Bunun icin ilk asamada yalnizca altyapi hazirligi yapilir.
- Tam sync ilk fazda yazilmaz.
- Outbox olay yapisi cekirdekte dusunulur.

## Performans Yaklasimi

- Yazma islemleri EF Core ile yapilir.
- Listeleme, grid, pagination, rapor ve arama ekranlari Dapper ile yapilir.
- `SELECT *` kullanilmaz.
- Buyuk listeler sayfali olur.
- Ilk gunden fazla index basilmaya calisilmaz.
- Once dogru sorgu, sonra gerekirse index.
- Cache olmadan dogru calisan sistem esastir.

## API Yaklasimi

- ProblemDetails standardi kullanilir.
- Soft delete dikkate alinir.
- Pagination zorunludur.
- Ticari belge numaralari teknik PK degildir.

## Lisans Hazirligi

Ilk asamada lisans sistemi yazilmaz.

Ancak tasarim su prensiple yapilir:
- modul bazli ac/kapa eklenebilir olmali
- ayri lisans servisi sonradan baglanabilir olmali

Olası paketler:
- Pano Lite
- Pano
- Pano Pro
