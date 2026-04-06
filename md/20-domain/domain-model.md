# Domain Modeli

## Ortak Kolonlar

Tum ana tablolarda ortak alanlar hedeflenir:
- Id
- TenantId
- SubeId
- OlusturmaTarihi
- GuncellemeTarihi
- AktifMi
- SilindiMi
- OlusturanKullaniciId
- GuncelleyenKullaniciId
- SilenKullaniciId
- SilinmeTarihi

## Anahtar Stratejisi

- Ilk asamada basit `Id` ile baslanabilir.
- Offline ve sync ihtiyaci netlestiginde `KayitGuid` eklenebilir.
- Fis numarasi PK olmaz.

## Belge Numaralari

Ayrik is kurali ile uretilir:
- FaturaNo
- SiparisNo
- TahsilatFisNo
- StokFisNo

Ornek format:
- `FTR-20260402-S01-C03-000123`
- `SIP-20260402-S01-C03-000045`
- `TAH-20260402-S01-C03-000022`

Ileride `NumaraSayac` tablosu ile yonetilebilir.

## Kimlik ve Kullanici

Login mantigi:
- JWT yok
- kullanici secme ekrani yok
- sade PIN mantigi var

Akis:
1. kullanici PIN girer
2. sistem PIN ile kullaniciyi bulur
3. rol, sube, cihaz ve yetkiler yuklenir
4. aktif oturum acilir

Tablolar:
- Kullanici
- Rol
- KullaniciRol
- KullaniciSube
- KullaniciOturum

Ileride:
- KullaniciAyar

## Yetki

- Ilk asamada rol bazli yetki
- Ileride detay yetki genisleyebilir

## Kasa ve Vardiya

Kavramsal ayrim:
- Cihaz ayri
- Kasa ayri
- Kullanici ayri
- Vardiya ayri

Kurallar:
- Her cihazin varsayilan kasasi olabilir.
- Kullanici giris yaptiktan sonra vardiya acar.
- Aktif vardiya yoksa satis yapilamaz.

Hedeflenen tablolar:
- Kasa
- KasaHareket
- Vardiya
- VardiyaKapanis

Ileride:
- VardiyaDevir

## Satis

Temel akis:
1. masa siparisi veya bekleyen hizli satis `Siparis` olarak tutulur
2. satirlar `SiparisDetay` tablosunda tutulur
3. siparis faturaya donustugunde detaylar `FaturaDetay` tablosuna snapshot olarak kopyalanir
4. islem tamamlaninca `Fatura` olusur
5. tahsilat alinir
6. odeme turune gore finans hareketleri yazilir

Siparis tipleri:
- `Masa`
- `HizliSatisBekleyen`

Siparis kurallari:
- masa siparisinde `AdisyonId` zorunludur
- hizli satis bekleyende `AdisyonId` bos olabilir
- siparis satirlari para birimi ve kur bilgisiyle acilabilir
- satir bazli indirim oran veya tutar olarak uygulanabilir
- siparis geneli indirim oran veya tutar olarak uygulanabilir
- `AraToplam` satir brut toplamlari uzerinden hesaplanir
- `NetToplam` satir net toplami uzerinden genel indirim dusulerek hesaplanir
- mevcut uyumluluk icin `ToplamTutar`, `NetToplam` degeriyle ayni tutulur
- siparis fiziksel olarak silinmez, durum ile yonetilir

Bagli hareketler:
- nakitse `KasaHareket`
- kartsa `BankaHareket`
- veresiyeyse `CariHareket`

Tablolar:
- Siparis
- SiparisDetay
- Fatura
- FaturaDetay
- Tahsilat

Siparis alanlari icin notlar:
- `ParaBirimKodu` ornek olarak `TRY`, `USD`, `EUR` degerlerini tasir
- `Kur` alani zorunludur
- `SatirAraToplam = Miktar x BirimFiyat`
- `SatirNetToplam = SatirAraToplam - IndirimTutari`
- mevcut uyumluluk icin `SatirToplam`, `SatirNetToplam` ile ayni tutulur

Fatura kurallari:
- fatura siparisten transaction icinde uretilir
- siparis detaylari faturaya snapshot olarak kopyalanir
- siparisin para birimi, kur, ara toplam, genel indirim ve net toplam alanlari faturaya aynen tasinir
- faturada satir indirimi ve genel indirim gorunur halde korunur
- mevcut uyumluluk icin `ToplamTutar`, `NetToplam` ile ayni tutulur
- fatura fiziksel olarak silinmez, durum ile yonetilir
- Kasa
- KasaHareket
- Banka
- BankaHareket
- Cari
- CariTip
- CariHareket

## Restoran

Tablolar:
- Masa
- MasaDurum
- Adisyon

Masa durumlari:
- Bos
- Dolu
- Rezerve

Temel kurallar:
- ayni masada ayni anda tek acik adisyon olabilir
- adisyon acilinca masa durumu `Dolu` olur
- adisyon kapaninca masa durumu `Bos` olur
- pasif masa icin adisyon acilamaz

Ileride masa durumu SignalR ile izlenebilir.

## Urun

Gereksinimler:
- barkod destekli
- EAN / QR
- varyantli urun
- renk / beden
- varyantlara ayri barkod
- ileride terazi barkodu
- ileride recete ve uretim destegi

Tablolar:
- Urun
- Renk
- Beden
- UrunVaryant
- Barkod

Kural:
- Barkod ya urune bagli olur ya varyanta bagli olur.

## Audit ve Log

Amac:
- kim hangi islemi yapti
- hangi butona basti
- hangi cihazdan yapti
- islem basarili mi basarisiz mi
- hata varsa neydi

Veri ayirimi:
- islemi ilgilendiren audit log SQL'de tutulur
- teknik uygulama loglari dosyada tutulur

Tablo:
- IslemLog

Ileride:
- IslemLogDetay
