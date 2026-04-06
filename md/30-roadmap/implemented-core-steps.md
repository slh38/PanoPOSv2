# Uygulanan Cekirdek Adimlar

Bu dosya, prompt bazli tamamlanan cekirdek adimlari ve teknik sonucunu kayit altinda tutar.

## Prompt 4 - PIN Login Cekirdegi

Kapsam:
- PIN ile hizli kullanici girisi
- JWT olmadan oturum acma
- hash'li PIN saklama
- ayni kullanici icin tek aktif oturum

Yapilanlar:
- `Kullanici` tablosu `PinHash`, `PinSonDegistirmeTarihi`, `SonGirisTarihi`, `BasarisizGirisSayisi`, `KilitliMi` alanlari ile genislendi
- `IPinHashServisi` ve `PinHashServisi` eklendi
- `IAuthServisi` ve `AuthServisi` ile login/logout akisi kuruldu
- `POST /api/v1/auth/login`
- `POST /api/v1/auth/logout`
- ProblemDetails ile yanlis PIN, pasif cihaz, pasif kullanici, kilitli kullanici gibi durumlar ayrildi
- `AddPinLoginCore` migration'i olusturuldu ve uygulandi

Durum:
- build gecti
- testler gecti
- login/logout endpointleri ayakta

## Prompt 5 - Kasa ve Vardiya Cekirdegi

Kapsam:
- kasa tanimi
- vardiya acma/kapatma
- nakit hareketlerinin `KasaHareket` uzerinden izlenmesi

Eklenen tablolar:
- `Kasa`
- `Vardiya`
- `VardiyaKapanis`
- `KasaHareket`

Temel kurallar:
- ayni cihazda ayni anda tek aktif vardiya
- ayni kasada ayni anda tek aktif vardiya
- vardiya acilisinda acilis nakdi `KasaHareket` olarak yazilir
- vardiya kapanisinda beklenen nakit hesaplanir ve fark kaydi olusur

Eklenen endpointler:
- `POST /api/v1/vardiya/ac`
- `POST /api/v1/vardiya/kapat`
- `GET /api/v1/vardiya/aktif?cihazId=...`
- `GET /api/v1/kasa`
- `POST /api/v1/kasa`

Not:
- vardiya zorunlulugu sabit kural olarak yazilmadi
- isletme ayari `VardiyaliSatisZorunlu = true` ise aktif vardiya zorunlu olacak
- ayar kapaliysa vardiyasiz satis mumkun olacak

Migration:
- `AddCashAndShiftCore`

Durum:
- build gecti
- testler gecti
- migration uygulandi

## Prompt 6 - Urun, Varyant ve Barkod Cekirdegi

Amac:
- hizli satis ve restoran icin temel urun altyapisini kurmak
- varyantli urunleri desteklemek
- urun veya varyanta bagli barkod tanimi yapmak
- listeleme ve barkod aramada performansli sorgu kullanmak

Eklenen tablolar:
- `Urun`
- `Renk`
- `Beden`
- `UrunVaryant`
- `Barkod`

Temel kurallar:
- `Barkod` ya `UrunId` ya da `UrunVaryantId` uzerine baglanir
- ikisi ayni anda dolu veya ayni anda bos olamaz
- ayni tenant icinde ayni barkod tekrar edemez
- ayni urun altinda ayni `RenkId + BedenId` kombinasyonu tekrar edemez
- varyantta renk ve beden ikisi birden bos olamaz
- urun listesinde pagination zorunludur

Teknik kararlar:
- yazma islemleri EF Core ile kaldi
- `GET /api/v1/urun` listeleme Dapper ile yazildi
- `GET /api/v1/barkod/{barkodNo}` lookup sorgusu Dapper ile yazildi
- `SELECT *` kullanilmadi
- SQL Server ve SQLite icin ayri pagination SQL'i yazildi

Eklenen endpointler:
- `POST /api/v1/urun`
- `PUT /api/v1/urun/{id}`
- `GET /api/v1/urun/{id}`
- `GET /api/v1/urun?search=&page=&pageSize=`
- `POST /api/v1/urun/{urunId}/varyant`
- `GET /api/v1/urun/{urunId}/varyant`
- `POST /api/v1/barkod`
- `GET /api/v1/barkod/{barkodNo}`
- `POST /api/v1/renk`
- `GET /api/v1/renk`
- `POST /api/v1/beden`
- `GET /api/v1/beden`

Migration:
- `AddProductVariantBarcodeCore`

Durum:
- build gecti
- toplam 22 test gecti
- migration olusturuldu ve uygulandi

## Prompt 7 - Cari Cekirdegi

Amac:
- temel cari kartini sisteme eklemek
- satis, tahsilat ve cari hareketleri icin baslangic referans yapisini hazirlamak
- tenant ve sube bazli sade CRUD ve performansli listeleme kurmak

Eklenen tablo:
- `Cari`

Alanlar:
- `Id`
- `TenantId`
- `SubeId`
- `CariKodu`
- `Ad`
- `Tip`
- `Telefon`
- `Email`
- `VergiNo`
- ortak audit alanlari
- `AktifMi`
- `SilindiMi`

Tip alani:
- `CariTipi` enum'u eklendi
- `Satici`
- `Alici`
- `Personel`
- `Masraf`

Temel kurallar:
- `(TenantId, CariKodu)` unique tanimlandi
- listeleme ve detay sorgularinda soft delete filtresi calisiyor
- tum islemler tenant ve sube bazli yurutuluyor
- controller icinde is kurali tutulmadi
- listeleme Dapper ile yazildi
- liste sorgusunda sadece gerekli kolonlar secildi:
  - `Id`
  - `CariKodu`
  - `Ad`
  - `Tip`
  - `Telefon`
  - `AktifMi`

Eklenen servis:
- `ICariServisi`
- `CariServisi`

Servis metotlari:
- `CariOlusturAsync`
- `CariGuncelleAsync`
- `CariGetirAsync`
- `CariListeleAsync`

Endpointler:
- `POST /api/v1/cari`
- `PUT /api/v1/cari/{id}`
- `GET /api/v1/cari/{id}?subeId=...`
- `GET /api/v1/cari?subeId=...&search=&page=&pageSize=`

Listeleme davranisi:
- `search` alani `Ad` veya `CariKodu` uzerinden calisir
- pagination zorunludur
- donus modeli:
  - `toplamKayit`
  - `sayfa`
  - `sayfaBoyutu`
  - `kayitlar`

Indexler:
- `(TenantId, SubeId, SilindiMi)`
- `(TenantId, CariKodu)`

Test kapsami:
- cari ekleme
- duplicate cari kodu kontrolu
- soft delete filtre kontrolu
- sayfali listeleme

Migration:
- `AddCustomerCore`
- dosya: `20260402144841_AddCustomerCore`

Uygulama notu:
- `dotnet ef migrations add` ilk denemede tasarim zamani build asamasinda sessiz hata verdi
- ayni kod tabani `dotnet build PanoPos.sln` ile sorunsuz derlendi
- migration, mevcut derlenmis cikti uzerinden `--no-build` ile olusturuldu
- sonraki benzer durumda once `dotnet build`, sonra gerekirse `dotnet ef ... --no-build` yolu izlenebilir

Durum:
- `dotnet build PanoPos.sln` gecti
- `dotnet test PanoPos.sln` gecti
- toplam 26 test gecti
- `AddCustomerCore` migration'i SQL'e uygulandi

## Prompt 8 - Restoran Cekirdegi

Amac:
- masa ve adisyon cekirdegi ile restoran tarafinin temel operasyonunu hazirlamak
- masa durumunu sistem tarafinda tutmak
- ayni masada ayni anda tek acik adisyon kuralini uygulamak

Eklenen tablolar:
- `MasaDurum`
- `Masa`
- `Adisyon`

Seed:
- `Bos`
- `Dolu`
- `Rezerve`

Temel kurallar:
- ayni masada ayni anda sadece 1 acik adisyon olabilir
- adisyon acilinca masa durumu `Dolu` olur
- adisyon kapaninca masa durumu `Bos` olur
- pasif veya silinmis masa icin adisyon acilamaz
- soft delete filtreleri ana tablolarda calisir

Eklenen servisler:
- `IMasaServisi`
- `MasaServisi`
- `IAdisyonServisi`
- `AdisyonServisi`

Servis metotlari:
- `MasaOlusturAsync`
- `MasaListeleAsync`
- `AdisyonAcAsync`
- `AdisyonKapatAsync`
- `AcikAdisyonGetirAsync`

Eklenen endpointler:
- `POST /api/v1/masa`
- `GET /api/v1/masa?subeId=...`
- `POST /api/v1/adisyon/ac`
- `POST /api/v1/adisyon/kapat`
- `GET /api/v1/adisyon/acik?masaId=...`

Indexler:
- `Masa (TenantId, SubeId, SilindiMi)`
- `Adisyon (MasaId, Durum)`
- `Adisyon (TenantId, SubeId, AcilisTarihi)`

Test kapsami:
- masa olusturma
- adisyon acma
- ayni masada ikinci acik adisyonu engelleme
- adisyon kapaninca masa durumunun degismesi
- acik adisyonun getirilebilmesi

Migration:
- `AddRestaurantCore`
- dosya: `20260402154029_AddRestaurantCore`

Durum:
- `dotnet build PanoPos.sln` gecti
- `dotnet test PanoPos.sln` gecti
- toplam 31 test gecti
- `AddRestaurantCore` migration'i SQL'e uygulandi

## Prompt 9 - Siparis Cekirdegi

Amac:
- bekleyen hizli satis ve restoran siparislerini tek veri modelinde toplamak
- siparis satirlarini ayri tabloda tutmak
- siparis toplam tutarini satirlardan uretmek
- listeleme ve filtrelemeyi Dapper ile sade ve hizli tutmak

Eklenen tablolar:
- `Siparis`
- `SiparisDetay`

Temel kurallar:
- siparis tipi `Masa` veya `HizliSatisBekleyen` olabilir
- masa siparisinde `AdisyonId` zorunludur
- hizli satis bekleyende `AdisyonId` bos olabilir
- satir eklendiginde `Siparis.ToplamTutar` guncellenir
- siparis fiziksel olarak silinmez, `Durum` ile yonetilir
- soft delete filtreleri aktif kalir

Enumlar:
- `SiparisTipi`
  - `Masa`
  - `HizliSatisBekleyen`
- `SiparisDurumu`
  - `Bekliyor`
  - `Tamamlandi`
  - `Iptal`

Eklenen servis:
- `ISiparisServisi`
- `SiparisServisi`

Servis metotlari:
- `SiparisOlusturAsync`
- `SiparisSatirEkleAsync`
- `SiparisGetirAsync`
- `SiparisListeleAsync`
- `SiparisIptalAsync`

Eklenen endpointler:
- `POST /api/v1/siparis`
- `POST /api/v1/siparis/{id}/satir`
- `GET /api/v1/siparis/{id}`
- `GET /api/v1/siparis?subeId=...&durum=...&page=&pageSize=`
- `POST /api/v1/siparis/{id}/iptal`

Listeleme davranisi:
- Dapper kullanildi
- pagination zorunlu
- sadece gerekli kolonlar doner:
  - `Id`
  - `SiparisNo`
  - `SiparisTipi`
  - `AdisyonId`
  - `ToplamTutar`
  - `Durum`

Indexler:
- `Siparis (TenantId, SubeId, Durum)`
- `Siparis (TenantId, SiparisNo)`
- `Siparis (AdisyonId, Durum)`
- `SiparisDetay (SiparisId)`

Test kapsami:
- siparis olusturma
- masa siparisinde adisyon zorunlulugu
- hizli satis siparisinde adisyonun opsiyonel olmasi
- satir eklenince toplam guncellenmesi
- siparis iptali
- sayfali listeleme
- adisyona bagli siparis olusturma

Migration:
- `AddOrderCore`
- dosya: `20260406150419_AddOrderCore`

Uygulama notu:
- SQLite decimal `Sum` cevirisinde EF kisiti oldugu icin siparis toplam guncellemesi istemci tarafinda toplama ile cozuldu
- bu karar test uyumlulugu ve sadelik icin secildi

Durum:
- `dotnet build PanoPos.sln` gecti
- `dotnet test PanoPos.sln` gecti
- toplam 38 test gecti
- `AddOrderCore` migration'i SQL'e uygulandi

## Prompt 10 - Fatura Cekirdegi

Amac:
- siparisten faturaya gecis akisini kurmak
- siparis detaylarini faturaya snapshot olarak tasimak
- fatura durum yonetimini transaction icinde guvenli sekilde uygulamak

Eklenen tablolar:
- `Fatura`
- `FaturaDetay`

Temel kurallar:
- fatura siparisten uretilebilir
- siparisten faturaya geciste detaylar snapshot olarak kopyalanir
- islem transaction icinde calisir
- islem sonunda siparis durumu `Tamamlandi` olur
- fatura fiziksel olarak silinmez, durum ile yonetilir
- soft delete filtreleri aktif kalir

Enum:
- `FaturaDurumu`
  - `Acik`
  - `Kapali`
  - `Iptal`
  - `Iade`

Eklenen servis:
- `IFaturaServisi`
- `FaturaServisi`

Servis metotlari:
- `SiparistenFaturaOlusturAsync`
- `FaturaGetirAsync`
- `FaturaListeleAsync`
- `FaturaKapatAsync`
- `FaturaIptalAsync`

Eklenen endpointler:
- `POST /api/v1/fatura/olustur-siparisten`
- `GET /api/v1/fatura/{id}`
- `GET /api/v1/fatura?subeId=...&durum=...&page=&pageSize=`
- `POST /api/v1/fatura/{id}/kapat`
- `POST /api/v1/fatura/{id}/iptal`

Listeleme davranisi:
- Dapper kullanildi
- pagination zorunlu
- sadece gerekli kolonlar doner:
  - `Id`
  - `FaturaNo`
  - `SiparisId`
  - `ToplamTutar`
  - `Durum`
  - `KapanisTarihi`

Indexler:
- `Fatura (TenantId, SubeId, Durum)`
- `Fatura (TenantId, FaturaNo)`
- `Fatura (SiparisId)`
- `FaturaDetay (FaturaId)`

Test kapsami:
- siparisten fatura olusturma
- detay snapshot kopyasi
- siparis durumunun guncellenmesi
- fatura kapatma
- fatura iptali
- sayfali listeleme

Migration:
- `AddInvoiceCore`
- dosya: `20260406151457_AddInvoiceCore`

Durum:
- `dotnet build PanoPos.sln --no-restore` gecti
- `dotnet test PanoPos.sln` gecti
- toplam 44 test gecti
- `AddInvoiceCore` migration'i SQL'e uygulandi

## Prompt 11 - Siparis Indirim ve Para Birimi Revizyonu

Amac:
- mevcut siparis cekirdigine satir bazli indirim eklemek
- siparis geneli indirim yapisini eklemek
- para birimi ve kur destegiyle siparisi coklu para birimine hazir hale getirmek
- mevcut endpointleri bozmadan siparis hesap mantigini genisletmek

Revize edilen alanlar:
- `Siparis`
  - `ParaBirimKodu`
  - `Kur`
  - `AraToplam`
  - `GenelIndirimOrani`
  - `GenelIndirimTutari`
  - `NetToplam`
- `SiparisDetay`
  - `SatirAraToplam`
  - `IndirimOrani`
  - `IndirimTutari`
  - `SatirNetToplam`

Temel kurallar:
- siparis `TRY`, `USD`, `EUR` gibi para birimleriyle acilabilir
- `Kur` zorunludur ve `0`'dan buyuk olmalidir
- ayni satirda hem `IndirimOrani` hem `IndirimTutari` dolu olamaz
- ayni sipariste hem `GenelIndirimOrani` hem `GenelIndirimTutari` dolu olamaz
- `SatirAraToplam = Miktar x BirimFiyat`
- `SatirNetToplam = SatirAraToplam - IndirimTutari`
- `AraToplam` tum satirlarin brut toplami uzerinden hesaplanir
- `NetToplam`, satir net toplamlari uzerinden genel indirim dusulerek hesaplanir
- `NetToplam` negatif olamaz

Geriye uyumluluk notu:
- mevcut kullanimlari bozmamak icin `Siparis.ToplamTutar = NetToplam` tutuldu
- mevcut kullanimlari bozmamak icin `SiparisDetay.SatirToplam = SatirNetToplam` tutuldu

Listeleme davranisi:
- mevcut Dapper listeleme sorgusu revize edildi
- sadece gerekli kolonlar doner:
  - `Id`
  - `SiparisNo`
  - `SiparisTipi`
  - `Durum`
  - `ParaBirimKodu`
  - `Kur`
  - `AraToplam`
  - `GenelIndirimTutari`
  - `NetToplam`
  - `OlusturmaTarihi`

Test kapsami:
- satir indiriminin oranla hesaplanmasi
- satir indiriminin tutarla hesaplanmasi
- ayni satirda oran ve tutarin birlikte verilmesinin engellenmesi
- siparis geneli indirim oraninin calismasi
- siparis geneli indirim tutarinin calismasi
- ayni sipariste oran ve tutarin birlikte verilmesinin engellenmesi
- para birimi ve kur bilgisinin kaydedilmesi
- net toplam hesaplamasinin dogrulanmasi

Migration:
- `ReviseOrderDiscountAndCurrency`
- dosya: `20260406153211_ReviseOrderDiscountAndCurrency`

Durum:
- `dotnet build PanoPos.sln --no-restore` gecti
- `dotnet test PanoPos.sln` gecti
- toplam 52 test gecti
- `ReviseOrderDiscountAndCurrency` migration'i SQL'e uygulandi

## Prompt 12 - Fatura Indirim ve Para Birimi Revizyonu

Amac:
- mevcut fatura cekirdigine satir bazli indirim eklemek
- fatura geneli indirim yapisini eklemek
- para birimi ve kur destegiyle faturayi siparis snapshot'ina tam uyumlu hale getirmek
- mevcut endpointleri bozmadan fatura gorunumunu genisletmek

Revize edilen alanlar:
- `Fatura`
  - `ParaBirimKodu`
  - `Kur`
  - `AraToplam`
  - `GenelIndirimOrani`
  - `GenelIndirimTutari`
  - `NetToplam`
- `FaturaDetay`
  - `SatirAraToplam`
  - `IndirimOrani`
  - `IndirimTutari`
  - `SatirNetToplam`

Temel kurallar:
- fatura siparisten olusurken para birimi ve kur bilgisi siparisten aynen kopyalanir
- siparisteki ara toplam, genel indirim ve net toplam alanlari faturaya snapshot olarak tasinir
- siparis detayindaki satir ara toplam, satir indirimi ve satir net toplam faturaya aynen kopyalanir
- `ParaBirimKodu` bos olamaz
- `Kur` 0'dan buyuk olmalidir
- `NetToplam` negatif olamaz
- fatura olustuktan sonra kapatma ve iptal kurallari aynen korunur

Geriye uyumluluk notu:
- mevcut kullanimlari bozmamak icin `Fatura.ToplamTutar = NetToplam` akisi korunur
- mevcut kullanimlari bozmamak icin `FaturaDetay.SatirToplam = SatirNetToplam` akisi korunur

Listeleme davranisi:
- mevcut Dapper listeleme sorgusu revize edildi
- sadece gerekli kolonlar doner:
  - `Id`
  - `FaturaNo`
  - `Durum`
  - `ParaBirimKodu`
  - `Kur`
  - `AraToplam`
  - `GenelIndirimTutari`
  - `NetToplam`
  - `KapanisTarihi`

Test kapsami:
- siparisteki para biriminin faturaya kopyalanmasi
- siparisteki kur bilgisinin faturaya kopyalanmasi
- satir indirimlerinin faturaya snapshot gelmesi
- genel indirimin faturaya snapshot gelmesi
- net toplamin dogru kopyalanmasi
- fatura listelemede yeni alanlarin donmesi

Migration:
- `ReviseInvoiceDiscountAndCurrency`
- dosya: `20260406155131_ReviseInvoiceDiscountAndCurrency`

Durum:
- `dotnet build PanoPos.sln --no-restore` gecti
- `dotnet test PanoPos.sln --no-build` gecti
- toplam 58 test gecti
- `ReviseInvoiceDiscountAndCurrency` migration'i SQL'e uygulandi
