# Calisma Ozeti

Bu dosya, tek bakista mevcut durumu gormek icin tutulur.

Amac:
- hangi `.md` klasorleri ve dosyalari var gormek
- su ana kadar neler tamamlanmis gormek
- siradaki isi netlestirmek
- yeni is bittiginde tek noktadan guncellemek

## Dokumantasyon Yapisi

Klasorler:
- `md/00-overview`
- `md/10-architecture`
- `md/20-domain`
- `md/30-roadmap`
- `md/40-rules`
- `md/50-tracking`

Dosyalar:
- `README.md`
- `md/README.md`
- `md/00-overview/project-summary.md`
- `md/10-architecture/core-architecture.md`
- `md/20-domain/domain-model.md`
- `md/30-roadmap/implementation-order.md`
- `md/30-roadmap/implemented-core-steps.md`
- `md/40-rules/codex-working-rules.md`
- `md/50-tracking/work-summary.md`

## Belge Bazli Ozet

`README.md`
- repo giris dokumani
- backend ve desktop calistirma adimlarini tutuyor
- login, hizli satis ve tahsilat akislarini ust seviyede ozetliyor

`md/README.md`
- dokumantasyon indeks dosyasi
- okuma sirasini tanimliyor

`md/00-overview/project-summary.md`
- urun kapsamini ve ilk faz sinirlarini anlatiyor
- cekirdek hedefin satilabilir backend urunu oldugunu netlestiriyor

`md/10-architecture/core-architecture.md`
- teknoloji secimlerini ve katman yapisini anlatiyor
- EF Core + Dapper ayrimini ve offline yaklasimini sabitliyor

`md/20-domain/domain-model.md`
- ana tablolari, temel is kurallarini ve domain davranislarini topluyor
- kullanici, kasa, vardiya, siparis, fatura, restoran, urun ve audit alanlarini tanimliyor

`md/30-roadmap/implementation-order.md`
- faz sirasini ve uygulama disiplinini tanimliyor
- once cekirdek backend sonra diger alanlar prensibini sabitliyor

`md/30-roadmap/implemented-core-steps.md`
- tamamlanan prompt bazli gelismeleri kayit altinda tutuyor
- migration, test ve sonuc bilgisini burada topluyor

`md/40-rules/codex-working-rules.md`
- sonraki gelistirmelerde uyulacak teknik kurallari sabitliyor
- sade mimari, pagination, soft delete ve ProblemDetails gibi kararlar burada

## Tamamlanan Isler

Kayitli tamamlanmis promptlar:
- Prompt 4: PIN login cekirdegi
- Prompt 5: kasa ve vardiya cekirdegi
- Prompt 6: urun, varyant ve barkod cekirdegi
- Prompt 7: cari cekirdegi
- Prompt 8: restoran cekirdegi
- Prompt 9: siparis cekirdegi
- Prompt 10: fatura cekirdegi
- Prompt 11: siparis indirim ve para birimi revizyonu
- Prompt 12: fatura indirim ve para birimi revizyonu
- Prompt 13: tahsilat cekirdegi
- Prompt 14: islemsel audit log cekirdegi
- Prompt 15: outbox olay cekirdegi
- Prompt 16: parcali tahsilat revizyonu

Genel durum:
- backend cekirdek modullerinin buyuk kismi uygulanmis gorunuyor
- migration kayitlari tutulmus
- test sayisi son kayitta `79`
- siparis, fatura, tahsilat, islemsel audit log ve outbox tarafinda cekirdek omurga tamamlanmis
- tahsilat cekirdegi artik parcali tahsilat ve coklu odeme kombinasyonlarini destekliyor
- desktop istemci icin ilk calisan iskelet olustu
- desktop tarafinda login, ana ekran, hizli satis ve tahsilat ekranlari mevcut

## Desktop Durumu

Eklenen desktop ekranlari:
- `LoginForm`
- `MainForm`
- `HizliSatisForm`
- `TahsilatForm`

Desktop teknik omurga:
- `AppSession` ile merkezi oturum yonetimi
- `ApiClient` ile HTTP API erisimi
- `HizliSatisService` ile urun, barkod, siparis ve fatura akisi
- `TahsilatService` ile parcali tahsilat akisi

Desktop'ta tamamlanan akislar:
- PIN ile login
- ana ekran tile menu
- urun kartindan sepete ekleme
- barkod ile sepete ekleme
- sepet miktar guncelleme ve toplam hesaplama
- siparisi beklemeye alma
- siparisten fatura olusturma
- tahsilat formunu acma
- nakit, kredi karti ve acik hesap tahsilati
- parcali tahsilat sonrasi odenen ve kalan tutari guncelleme

Desktop'ta gecici / eksik kalanlar:
- fiyat alanlari backend DTO'larinda olmadigi icin desktop fallback ile calisiyor
- `1/n` butonu placeholder
- `Odeme iptal` butonu placeholder
- `Tahsilat indirim` butonu placeholder
- tahsilat ekranindaki `Fatura` butonu placeholder
- urunsuz durumda tahsilat formu sadece onizleme icin gecici olarak aciliyor

## Siradaki Mantikli Alanlar

Roadmap ve domain modeline gore sonraki guclu adaylar:
- desktop tarafinda urun fiyat sozlesmesini backend ile netlestirmek
- desktop tahsilat ekraninda `Odeme iptal` ve tahsilat indirim akislarini gerceklestirmek
- desktop cari secimi ve acik hesap baglantisini gercek cari listeleme popup'i ile tamamlamak
- islem log servisinin siparis, fatura, tahsilat ve vardiya akislarina yaygin baglanmasi
- outbox kayitlarinin ileride gercek gonderim isleyicisine baglanmasi
- vardiya zorunlulugunun isletme ayariyla aktif uygulanmasi
- rapor ve listeleme endpointlerinin genisletilmesi
- numara sayac mantigi
- outbox altyapisi

## Guncelleme Kurali

Yeni bir is bittiginde en az su iki dosya guncellenmeli:
- `md/30-roadmap/implemented-core-steps.md`
- `md/50-tracking/work-summary.md`

Bu dosyada kisa ozet tutulur.
Detayli teknik sonuc, test ve migration bilgisi `implemented-core-steps.md` icinde kalir.
