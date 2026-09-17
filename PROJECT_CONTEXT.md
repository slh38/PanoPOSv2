# PanoPOS - PROJECT_CONTEXT.md

Bu doküman PanoPOS projesinin ne olduğunu, hangi işletmeler için
geliştirildiğini, gerçek hayatta nasıl kullanılacağını ve sistemin
uzun vadeli yönünü açıklar.

Bu dosya teknik kodlama standartlarından çok İŞ BAĞLAMINI anlatır.

Teknik ve kalıcı geliştirme kuralları için:

CODEX_RULES.md

kullanılır.

Codex yeni bir göreve başlamadan önce:

1. PROJECT_CONTEXT.md
2. CODEX_RULES.md

dosyalarını okumalıdır.

Bir özellik bu dokümanda gelecek hedefi olarak anlatılıyorsa,
görev açıkça istemedikçe otomatik olarak implement edilmemelidir.

---

# 1. PANOPOS NEDİR?

PanoPOS, farklı ticari işletmelerde kullanılabilecek yeni nesil satış,
restoran ve stok yönetim sistemidir.

İlk ana hedefler:

1. Hızlı Satış / POS
2. Restaurant / Adisyon

modülleridir.

Sistem ileride:

- stok yönetimi
- satın alma
- üretim
- ERP entegrasyonu
- e-Fatura
- yazarkasa POS
- mobil uygulamalar
- web yönetim
- çok şubeli merkezi yönetim

gibi alanlara genişleyebilir.

Ancak geliştirme aşamalı yapılacaktır.

Bir gelecek hedefinin bu dokümanda bulunması o özelliğin mevcut görevde
implement edilmesi gerektiği anlamına gelmez.

---

# 2. PROJENİN TEMEL AMACI

PanoPOS'un en önemli mimari hedeflerinden biri başka bir ERP
veritabanına bağımlı olmamaktır.

Eski sistemlerde satış/restoran uygulamaları başka ERP sistemlerinin
veritabanları, tabloları, stored procedure'leri ve trigger'ları ile
doğrudan ilişkili olabilmektedir.

PanoPOS'ta temel prensip:

PanoPOS kendi verisinin sahibidir.

Yani:

PanoPOS
    ->
Kendi API
    ->
Kendi Database

şeklinde çalışır.

Başka ERP sistemleri PanoPOS'un ana veritabanı değildir.

ERP bağlantıları ileride entegrasyon katmanı üzerinden yapılacaktır.

---

# 3. HEDEF İŞLETME TİPLERİ

PanoPOS tek bir sektör için tasarlanmamaktadır.

Başlangıçta özellikle:

- Market
- Büfe
- Hızlı satış noktası
- Toptancı
- Restaurant
- Cafe
- Lokanta

gibi işletmeler hedeflenmektedir.

İlerleyen aşamalarda:

- üretim işletmeleri
- mobilya üretimi
- çelik kapı üretimi
- mezbaha/kesim takip
- farklı ticari işletmeler

için modüller eklenebilir.

Ancak ilk geliştirme odağı:

Hızlı Satış + Restaurant

olmalıdır.

---

# 4. TEMEL MİMARİ YAKLAŞIM

Şube kendi içinde çalışabilmelidir.

Temel hedef topoloji:

Şube
 |
 +-- Local API
 |
 +-- Local SQL Server
 |
 +-- Desktop POS
 |
 +-- Mobil Garson
 |
 +-- diğer yerel cihazlar

İnternet bağlantısı satışın temel çalışma şartı olmamalıdır.

Şubenin günlük operasyonu mümkün olduğunca yerel sistem üzerinden
devam edebilmelidir.

---

# 5. OFFLINE ÇALIŞMA

PanoPOS'un önemli gereksinimlerinden biri offline çalışmadır.

İnternet kesildiğinde:

- satış
- restaurant siparişi
- ödeme
- kasa işlemleri

gibi şube içi temel operasyonların mümkün olduğunca devam etmesi
hedeflenmektedir.

Şube operasyonu sürekli merkezi cloud API'ye bağımlı olmamalıdır.

Offline/merkez senkronizasyon altyapısının tamamı henüz bitmiş değildir.

Mevcut OutboxOlay altyapısı gelecekteki senkronizasyon için temel
oluşturmaktadır.

Görev açıkça istemedikçe yeni sync sistemi oluşturulmamalıdır.

---

# 6. TEK ŞUBE / ÇOK ŞUBE

PanoPOS:

1 şubeli küçük işletmeden

çok şubeli işletmelere

kadar çalışabilecek şekilde tasarlanmalıdır.

Gelecekte bazı müşteriler:

5
10
20
80

veya daha fazla şubeye sahip olabilir.

Her şubenin operasyonel verisi yerel olabilir.

Merkezi yapı ileride şubelerden gerekli verileri toplayabilir.

---

# 7. TENANT

Tenant bir PanoPOS müşterisini/işletmesini temsil eder.

Bir Tenant'ın:

- bir veya birden fazla şubesi
- kullanıcıları
- stok kartları
- carileri
- KDV tanımları
- fiyatları
- ticari belgeleri

olabilir.

Bir Tenant'ın verisi başka Tenant ile karışmamalıdır.

---

# 8. ŞUBE

Sube fiziksel veya operasyonel işletme noktasını temsil eder.

Örnek:

Merkez
Talas Şubesi
Organize Şubesi

Her şube kendi:

- cihazları
- kasaları
- depoları
- vardiyaları
- satışları
- restaurant masaları

ile çalışabilir.

---

# 9. CİHAZ

Cihaz fiziksel POS terminali veya ilgili PanoPOS terminalini temsil eder.

Örneğin bir markette:

Kasa 1
Kasa 2
Kasa 3

olabilir.

Kullanıcı belirli bir bilgisayara sabitlenmek zorunda değildir.

Kullanıcı PIN ile farklı kasalarda oturum açabilir.

Cihaz kimliği kullanıcı tarafından her login sırasında seçilmek yerine
terminal configuration üzerinden bilinebilir.

---

# 10. KULLANICI GİRİŞİ

POS kullanımında hızlı giriş önemlidir.

Temel giriş yöntemi:

PIN

dir.

Kullanıcı:

kullanıcı adı + şifre

yazmak zorunda olmamalıdır.

Örnek:

Kasiyer PIN:
2580

girerek oturum açar.

Sistem PIN üzerinden kullanıcıyı bulur ve cihaz/şube bağlamını
oluşturur.

---

# 11. ROL VE YETKİ

Kullanıcıların rolleri vardır.

Örnek:

Admin
Kasiyer
Garson
Yönetici

Detaylı Yetki/RolYetki sistemi gelecekte genişletilebilir.

Görev açıkça istemedikçe karmaşık permission sistemi oluşturulmamalıdır.

---

# 12. KASA

Bir şubede birden fazla fiziksel kasa olabilir.

Örneğin:

Kasa 1
Kasa 2
Kasa 3

Kullanıcı herhangi uygun kasada çalışabilir.

Kasa hareketleri fiziksel para hareketini temsil eder.

---

# 13. VARDİYA

PanoPOS vardiya destekler.

Örnek:

Kasiyer vardiya açar.
Satış yapar.
Kasa hareketleri oluşur.
Vardiya sonunda kapanış yapar.

Ancak vardiya HER müşteride zorunlu değildir.

Bazı küçük işletmeler vardiya kullanmadan çalışabilir.

Bu nedenle sistemin genel ticari akışları vardiya bulunmasına
zorunlu olarak bağlanmamalıdır.

---

# 14. STOKKART

StokKart satılan veya ticari işlem gören ana ürün/hizmet kartıdır.

Örnek:

Coca Cola 330 ml
Su 500 ml
Hamburger
A4 Kağıt
Vida
MDF Levha

StokKart yalnızca market ürünü anlamına gelmez.

Gelecekte:

- Hammadde
- YariMamul
- Mamul

gibi üretim türleri de bulunabilir.

---

# 15. STOK VARYANTI

Bir StokKart varyantlara sahip olabilir.

Örnek:

Tişört
 |
 +-- Siyah / M
 +-- Siyah / L
 +-- Beyaz / M
 +-- Beyaz / L

Renk ve Beden mevcut varyant yapısının parçalarıdır.

Varyant kullanılmayan ürünlerde doğrudan StokKart üzerinden işlem
yapılabilir.

---

# 16. BARKOD

Bir ürünün bir veya birden fazla barkodu olabilir.

Barkod:

StokKart

veya:

StokKartVaryant

ile ilişkili olabilir.

Barkod ayrıca hangi satış biriminin okutulduğunu belirleyebilir.

Örneğin:

869000000001
    ->
Coca Cola
    ->
Adet

başka barkod:

869000000002
    ->
Coca Cola
    ->
Koli

olabilir.

Barkod fiyat tipini doğrudan belirlemez.

---

# 17. SATIŞ BİRİMLERİ

Aynı ürün farklı birimlerde satılabilir.

Örnek:

Adet
Kutu
Koli
Palet

Her satış biriminin Katsayi değeri vardır.

Örnek:

Adet = 1
Kutu = 10
Koli = 100

Katsayi stok temel miktarına dönüşüm için kullanılır.

---

# 18. SATIŞ BİRİMİ SENARYOSU

Örnek ürün:

Su

Satış birimleri:

Adet
Koli

Koli:

24 Adet

ise:

Katsayi = 24

olur.

Kasiyer koli barkodunu okuttuğunda:

1 Koli

satar.

Ticari belgede:

Miktar = 1
Birim = Koli
Katsayi = 24

snapshot olarak saklanır.

İleride stok hareketi oluşturulduğunda bunun stok etkisi:

24 temel birim

olacaktır.

---

# 19. FİYAT TİPLERİ

Bir ürünün tek bir fiyatı olmak zorunda değildir.

Örnek fiyat tipleri:

Perakende
KrediKarti
Toptan

Yeni fiyat tipleri ileride eklenebilir.

Fiyat:

StokKartSatisBirimi
+
FiyatTipi

kombinasyonuna bağlıdır.

---

# 20. FİYAT SENARYOSU

Örnek:

Ürün:
Su

Adet / Perakende:
10 TL

Adet / Toptan:
8 TL

Koli / Perakende:
220 TL

Koli / Toptan:
190 TL

olabilir.

Bu nedenle fiyat yalnızca StokKart üzerinde tek kolon olarak
düşünülmemelidir.

---

# 21. PARA BİRİMİ

Ürün fiyatları farklı para birimlerinde tanımlanabilir.

Örnek:

TRY
USD
EUR

Ancak sistem yalnızca bu para birimleriyle sınırlandırılmamalıdır.

Kur fiyat kartının sabit özelliği değildir.

Kur ticari işlem sırasında alınır ve belgeye snapshot olarak yazılır.

---

# 22. KDV

KDV oranları Kdv master tablosunda tutulur.

Örnek başlangıç kayıtları:

%0
%1
%10
%20

Ancak oranlar hard-coded değildir.

StokKart:

KdvId

ile güncel KDV tanımına bağlanır.

---

# 23. KDV SNAPSHOT

Bir ürün bugün:

%20 KDV

ile satılıyor olabilir.

Gelecekte oran:

%25

olabilir.

Eski faturanın değişmesi kabul edilemez.

Bu nedenle ticari belge satırında:

KdvId
KdvOrani
KdvDahilMi

snapshot tutulur.

Master Kdv kaydı değişirse geçmiş ticari belge değişmez.

---

# 24. KDV DAHİL / HARİÇ İŞLETME SENARYOSU

PanoPOS farklı işletme tiplerinde farklı KDV fiyatlama biçimleriyle
çalışmalıdır.

Örnek:

Market/Büfe:
KDV dahil fiyat

Toptancı:
KDV hariç fiyat

Tenant ayarlarında:

SatisFiyatlariKdvDahilMi
AlisFiyatlariKdvDahilMi

ayrı ayrı bulunur.

---

# 25. MARKET KDV DAHİL ÖRNEĞİ

Ürün satış fiyatı:

120 TL

KDV:
%20

İşletme:
KDV dahil çalışıyor.

Sonuç:

Matrah = 100 TL
KDV = 20 TL
Müşterinin ödediği = 120 TL

KDV tekrar 120 TL üzerine eklenmez.

---

# 26. TOPTANCI KDV HARİÇ ÖRNEĞİ

Ürün fiyatı:

100 TL

KDV:
%20

İşletme:
KDV hariç çalışıyor.

Sonuç:

Matrah = 100 TL
KDV = 20 TL
Toplam = 120 TL

---

# 27. İSKONTO

MVP'de iki temel iskonto vardır:

1. Satır iskontosu
2. Genel fatura/belge iskontosu

Zincir iskonto:

Iskonto1
Iskonto2
Iskonto3

şimdilik hedef değildir.

Genel iskonto farklı KDV oranlı satırlarda KDV matrahını doğru
etkilemelidir.

---

# 28. HIZLI SATIŞ TEMEL SENARYOSU

Kasiyer PIN ile giriş yapar.

Ürün barkodunu okutur.

Sistem:

Barkod
    ->
StokKart / Varyant
    ->
Satış Birimi
    ->
Fiyat

bilgilerini çözer.

Ürün sepete eklenir.

Kasiyer:

- miktar değiştirebilir
- izin verilen iskonto işlemlerini yapabilir
- ürünü iptal edebilir
- satışı bekletebilir
- ödeme alabilir

---

# 29. HIZLI SATIŞ - BEKLET

Müşteri ödeme yapmadan satış bekletilebilir.

Bu durumda temel ticari kayıt:

Siparis

olarak kalır.

Örnek:

SiparisTipi:
HizliSatisBekleyen

Müşteri geri geldiğinde sipariş yeniden açılıp ödeme alınabilir.

---

# 30. HIZLI SATIŞ - ÖDEME

Müşteri doğrudan ödeme yaptığında sistemin temel ticari akışı:

Siparis
    ->
Fatura
    ->
Tahsilat

şeklindedir.

UI kullanıcıya bu teknik adımların tamamını göstermek zorunda değildir.

Kasiyer açısından işlem basitçe:

Ürünleri okut
    ->
Ödeme Al
    ->
Satış Tamamlandı

şeklinde olabilir.

---

# 31. ÇOKLU ÖDEME

Bir fatura birden fazla ödeme ile kapatılabilir.

Örnek:

Toplam:
1.000 TL

Müşteri:

600 TL Nakit
400 TL Kredi Kartı

öder.

Sonuç:

1 Fatura
2 Tahsilat

oluşur.

---

# 32. KISMİ ÖDEME

Fatura tamamen ödenmek zorunda değildir.

Örneğin:

Fatura:
1.000 TL

Ödenen:
600 TL

Kalan:
400 TL

olabilir.

Fatura:

OdenenTutar
KalanTutar

bilgilerini takip eder.

Tam ödeme gerçekleştiğinde fatura kapanabilir.

---

# 33. NAKİT ÖDEME

Nakit Tahsilat:

KasaHareket

oluşturur.

Amaç fiziksel kasadaki para hareketini izlemektir.

---

# 34. KREDİ KARTI

Kredi kartı Tahsilat:

Banka / BankaHareket

üzerinden takip edilir.

POS/banka entegrasyonları ileride genişletilebilir.

---

# 35. VERESİYE / AÇIK HESAP

Müşteri ödemeyi hemen yapmayabilir.

Veresiye/Açık Hesap işlemleri:

CariHareket

üzerinden takip edilebilir.

Bu yapı Cari borç/alacak takibinin temelidir.

---

# 36. CARİ

Cari:

- müşteri
- tedarikçi
- personel
- masraf hesabı
- diğer ticari hesap

olabilir.

Mevcut CariTipi örnekleri:

Alici
Satici
Personel
Masraf

Bir Cari'nin gelecekte hem müşteri hem tedarikçi gibi davranabilmesi
engellenmemelidir.

---

# 37. SATIŞ FATURASI

Fatura satışın nihai ticari belgesidir.

FaturaDetay satış anındaki bilgileri snapshot olarak saklar.

Örneğin:

- ürün
- varyant
- satış birimi
- Katsayi
- fiyat
- para birimi
- kur
- iskonto
- KDV

daha sonra master kayıtlar değişse bile geçmiş Fatura değişmemelidir.

---

# 38. ALIŞ FATURASI

AlisFatura tedarikçiden yapılan ticari alışın belgesidir.

AlisFatura:

Cari

ile ilişkilidir.

AlisFaturaDetay:

- StokKart
- Varyant
- Satış Birimi
- Katsayi
- fiyat
- kur
- iskonto
- KDV

snapshotlarını saklar.

---

# 39. ALIŞ FATURASI DURUMLARI

Temel durumlar:

Taslak
Kesinlesti
Iptal

Taslak belge düzenlenebilir.

Kesinleşmiş belge normal CRUD ile sessizce değiştirilmemelidir.

İptal/iade stok ve finans entegrasyonları ilerleyen görevlerde ayrıca
ele alınacaktır.

Stok girişi oluşturulmuş kesinleşmiş alış faturası mevcut iptal
endpointiyle iptal edilemez; ters stok hareketi gerektiğini belirten
domain hatası döner. Taslak iptal edilebilir ve stok oluşturmaz.

---

# 40. ALIŞ FATURASI STOK AKIŞI

Mevcut akış:

AlisFatura (Taslak)
    ->
Kesinlestir
    ->
StokFis (Alis)
    ->
StokFisDetay + StokHareket (+)

şeklindedir.

AlisFatura:
ticari belge

StokFis:
stok işleminin belge/fiş katmanı

StokHareket:
gerçek stok ledger hareketi

olarak düşünülür.

Bu katmanlar aynı kavram değildir.

AlisFatura oluşturma/güncelleme isteğinde DepoId açıkça zorunludur.
Depo aynı Tenant/Sube içinde aktif ve silinmemiş olmalıdır.
Taslak kayıt ve güncelleme stok üretmez; depo yalnız taslakta değişebilir.
Kesinleştirme, fiş/detay/hareket ve IslemLog aynı transaction içindedir.
StokFis.AlisFaturaId nullable FK ve unique filtered index ile
aynı faturanın ikinci stok fişi oluşturması engellenir.
SQL Server'da tüm fatura yazma işlemleri aynı fatura satırını
UPDLOCK/HOLDLOCK ile kilitler. Tekrarlanan kesinleştirme stok/audit
üretmeden mevcut sonucu döndürür.
Alış fişi numarası ALIS-{AlisFaturaId}, tarihi FaturaTarihi olur.
Hareket miktarı fatura detayındaki Miktar * snapshot Katsayi ile
hesaplanır; master birimin güncel adı/katsayısıyla değiştirilmez.
KDV/fiyat hesapları yeniden yapılmaz; finansal snapshotlar korunur.

---

# 41. DEPO

Stoklar bağımsız stok fişleri ve hareket ledger'ı üzerinden depo bazında takip edilir.

Bir şubede bir veya birden fazla depo olabilir.

Her şubenin varsayılan deposu vardır.

Başlangıç örneği:

DepoKodu:
MERKEZ

Ad:
Merkez Depo

VarsayilanMi:
true

---

# 42. DEPO ADI İLİŞKİ DEĞİLDİR

Teknik stok ilişkileri:

DepoId

üzerinden yapılır.

Depo adı kullanıcıya gösterilen bilgidir.

Örneğin gelecekte:

Kayseri Merkez
Talas
Organize

gibi depo adları kullanılabilir.

İlişki depo adına göre kurulmaz.

---

# 43. STOK MİKTARI

StokKart üzerinde doğrudan:

StokMiktari

gibi sürekli değiştirilen tek bir alan temel stok kaynağı olmamalıdır.

Stok miktarının kaynağı:

StokHareket

ledger'ıdır. Mevcut miktar Tenant + Sube + Depo + StokKart + nullable
StokKartVaryant kapsamında işaretli hareketlerin toplamıdır.

SQL Server sorgusu SUM(StokHareket.Miktar) kullanır. SQLite testlerinde
binary floating-point toplam kaybını önlemek için aynı kapsamdaki
hareket miktarları decimal olarak toplanır.

Stok:

StokKart + Depo

bazında takip edilir.

---

# 44. İLK STOK

Backend başlangıç miktarını Devir stok fişi ile alır.
StokKart ekranı bağlantısı bu aşamada yapılmamıştır.

Ancak bu:

StokKart.StokMiktari = 100

şeklinde doğrudan veri değiştirmek anlamına gelmemelidir.

Devir işleminde StokFis, StokFisDetay ve StokHareket tek transaction
içinde oluşturulur. Pozitif miktar satış birimi katsayısıyla çarpılır
ve temel birim cinsinden giriş hareketi yazılır.

---

# 45. EKSİ STOK

PanoPOS'un hedef kullanımında eksi stok bazı işletmeler için
normal çalışma biçimi olabilir.

Örneğin:

- restaurant
- market
- hızlı satış

işletmelerinde yanlış stok bakiyesi satışın tamamen durmasına
neden olmamalıdır.

Bu nedenle genel yaklaşım:

eksi stok mümkün

şeklindedir.

İleride isteyen müşteriler için eksi stok engelleme ayarı eklenebilir.

---

# 46. STOKBAKIYE

StokHareket stok için temel gerçek kaynaktır.

Başlangıç aşamasında ayrı StokBakiye cache tablosu zorunlu değildir.

Performans ihtiyacı oluştuğunda:

StokKart + Depo

bazında güncel bakiye tablosu ayrıca değerlendirilebilir.

---

# 47. RESTAURANT

Restaurant PanoPOS'un ikinci ana kullanım alanıdır.

Restaurant temel kavramları:

Masa
MasaGrup
Adisyon
Siparis
SiparisDetay

şeklindedir.

---

# 48. RESTAURANT MASA

Masalar gruplandırılabilir.

Örnek:

Salon
Bahçe
Teras
VIP

Masa'nın tanımlı kapasitesi olabilir.

Örnek:

Masa 5
Kapasite = 4

Ancak gerçek oturan kişi sayısı Masa'nın kalıcı özelliği değildir.

Gerçek kişi sayısı Adisyon üzerinde tutulabilir.

---

# 49. RESTAURANT ADİSYON

Müşteri masaya oturduğunda açık bir Adisyon oluşturulabilir.

Örnek:

Masa 5
    ->
Adisyon #123

Adisyon açıkken Masa:

Dolu

durumuna geçebilir.

Adisyon kapatıldığında Masa:

Bos

durumuna dönebilir.

Bir masa için aynı anda birden fazla açık adisyon oluşturulmaması
temel kuraldır.

---

# 50. RESTAURANT SİPARİŞ

Garson masa için sipariş oluşturur.

Örnek:

Masa 5
Adisyon #123

Sipariş:

2 Adana Kebap
1 Ayran
1 Kola

Siparis ve SiparisDetay üzerinden kaydedilir.

Aynı adisyona zaman içinde birden fazla sipariş eklenebilir.

---

# 51. RESTAURANT MUTFAK

Gelecekte SiparisDetay satırları mutfak yazıcılarına veya mutfak
ekranlarına yönlendirilebilir.

Örnek:

Yemek:
Mutfak Yazıcısı

İçecek:
Bar Yazıcısı

Bu routing sistemi gelecek kapsamıdır.

Görev açıkça istemedikçe implement edilmemelidir.

---

# 52. RESTAURANT ÖDEME

Restaurant ödeme sonunda temel hedef:

Adisyon / Siparisler
    ->
Fatura
    ->
Tahsilat

akışıdır.

Müşteri:

- nakit
- kredi kartı
- karma ödeme

kullanabilir.

İleride hesabı kişi/ürün bazında bölme gibi gelişmiş ödeme
senaryoları ayrıca ele alınabilir.

---

# 53. RESTAURANT REÇETE

Restaurant ürünleri gelecekte reçeteye sahip olabilir.

Örnek:

1 Adana Kebap

stok etkisi:

150 gr kıyma
1 lavaş
50 gr garnitür

olabilir.

Ancak reçete/üretim stok düşümü mevcut temel stok hareket
altyapısından ayrı bir sonraki aşamadır.

Görev açıkça istemedikçe reçete motoru oluşturma.

---

# 54. STOK HAREKETİ GELECEK SENARYOLARI

StokHareket altyapısı aşağıdaki türde işlemleri ileride
destekleyebilmelidir:

Alış
Satış
Satış İade
Alış İade
Sayım Artı
Sayım Eksi
Devir / İlk Stok
Depo Transfer
Üretim Giriş
Üretim Çıkış
Manuel Düzeltme

Mevcut çekirdekte Devir, Sayım ve DepoTransfer uygulanmıştır.
Alış faturası kesinleştirmesi Alış stok girişine bağlanmıştır.
Satış ve iade tipleri enum seviyesinde bulunur; satış/iade
entegrasyonları ve üretim henüz uygulanmamıştır.

StokFis belgeyi, StokFisDetay birim/katsayı ve miktar snapshotlarını,
StokHareket değiştirilemeyen işaretli temel miktarı saklar.
Kayıtlı fiş/detay/hareket için update/delete endpointi yoktur;
DbContext kayıtlı stok belgelerinin ve hareketlerinin değişmesini engeller.
Aynı detay ve hareket tipi için unique kısıtı çift hareketi engeller.

Bu liste otomatik implementasyon talimatı değildir.

---

# 55. DEPOLAR ARASI TRANSFER

Depolar arası transfer tek StokFis ile uygulanmıştır.

Örnek:

Merkez Depo
    ->
10 Adet Ürün
    ->
Talas Depo

Kaynak depoda stok azalır.

Hedef depoda stok artar.

Her detay için aynı transaction içinde kaynak depoda negatif,
hedef depoda pozitif hareket oluşturulur. Depolar farklı, aktif,
silinmemiş ve aynı Tenant/Sube kapsamında olmalıdır.
Kaynak stok yetersiz olsa da transfer engellenmez.

---

# 56. SAYIM

Kullanıcı fiziksel stok sayımı yapabilir.

Örnek:

Sistem:
100 Adet

Fiziksel:
95 Adet

Aradaki:

-5

fark backend tarafından hesaplanır ve sayım stok hareketi oluşturur.

İstek satırındaki Miktar, seçilen satış birimindeki fiziksel sayımdır.
SistemMiktari, SayilanMiktar ve FarkMiktari temel birimde snapshot
saklanır. Fark sıfırsa detay saklanır ancak hareket yazılmaz.
Aynı stok/varyant bir sayım fişinde bir kez bulunabilir.
Sayım seri hale getirilebilir transaction içinde çalışır; SQL Server
stok okumasında UPDLOCK/HOLDLOCK kullanılarak eşzamanlı değişim korunur.

Stok miktarı doğrudan overwrite edilmemelidir.

---

# 57. ERP ENTEGRASYONU

PanoPOS gelecekte farklı ERP sistemleriyle çalışabilir.

Temel prensip:

ERP PanoPOS'un veritabanı değildir.

PanoPOS kendi veritabanında çalışır.

Entegrasyon:

PanoPOS
    ->
Integration / Adapter
    ->
ERP

şeklinde olmalıdır.

Bir ERP'nin tablo yapısını PanoPOS domain modeline taşımak doğru
yaklaşım değildir.

---

# 58. MERKEZ / ŞUBE SENKRONİZASYONU

Gelecekte çok şubeli yapılarda şubeler merkez ile senkronize olabilir.

Örneğin merkez:

- ürün
- fiyat
- kampanya
- kullanıcı
- ayar

gönderebilir.

Şubeler:

- satış
- tahsilat
- stok hareketi
- kasa bilgisi

gönderebilir.

Ancak gerçek sync protokolü ayrıca tasarlanacaktır.

---

# 59. OUTBOX

Mevcut OutboxOlay altyapısı gelecekteki güvenilir entegrasyon/sync
işlemlerinin temel parçalarından biridir.

Önemli ticari işlemler uygun olduğunda outbox event üretebilir.

Ancak görev istemedikçe gerçek sync worker veya yeni event sistemi
oluşturulmamalıdır.

---

# 60. İŞLEM LOGU

PanoPOS'ta kalıcı işlem audit'i için IslemLog bulunmaktadır.

Örneğin:

- Login
- Logout
- kritik kullanıcı işlemleri

audit edilebilir.

IslemLog teknik application log ile aynı şey değildir.

---

# 61. LİSANSLAMA

PanoPOS gelecekte lisans/modül bazlı çalışacaktır.

Planlanan ürün seviyeleri örneğin:

Pano Lite
Pano
Pano Pro

olabilir.

Lisanslar:

- modül
- süre
- demo
- cihaz
- müşteri

gibi kavramları içerebilir.

Ayrı License API planlanmaktadır.

Ancak lisanslama mevcut görev açıkça istemedikçe implement edilmemelidir.

---

# 62. WEB YÖNETİM

Gelecekte merkezi/web yönetim arayüzü bulunabilir.

Hedef teknoloji:

React

olabilir.

Web yönetim:

- ürün
- fiyat
- rapor
- şube
- kullanıcı
- merkezi yönetim

gibi fonksiyonlar sağlayabilir.

Bu gelecek kapsamıdır.

---

# 63. MOBİL GARSON

Restaurant için mobil garson uygulaması hedeflenmektedir.

Hedef teknoloji:

Flutter

dır.

Garson:

- masa seçebilir
- adisyon görebilir
- sipariş ekleyebilir
- sipariş durumunu takip edebilir

gibi işlemler yapabilir.

Mobil uygulama doğrudan SQL Server'a bağlanmamalıdır.

API üzerinden çalışmalıdır.

---

# 64. REALTIME

Restaurant ve çok cihazlı POS kullanımında realtime iletişim için
SignalR kullanılabilir.

Örneğin:

Garson sipariş ekledi.
    ->
POS ekranı güncellendi.

veya:

Masa durumu değişti.
    ->
Diğer cihazlar haberdar oldu.

Ancak SignalR görev açıkça istemedikçe her modüle otomatik
eklenmemelidir.

---

# 65. TERMAL / MUTFAK YAZICILARI

PanoPOS gelecekte:

- 80 mm fiş yazıcısı
- mutfak yazıcısı
- barkod yazıcısı

ile çalışabilir.

Restaurant'ta ürün kategorisine göre yazıcı routing yapılabilir.

Bu donanım entegrasyonları domain modelini gereksiz yere
cihaz bağımlı hale getirmemelidir.

---

# 66. YAZARKASA POS

PanoPOS gelecekte mali/yazarkasa POS cihazlarıyla entegre olabilir.

Örnek olarak Pavo benzeri cihaz entegrasyonları olabilir.

Ancak PanoPOS'un temel satış domain'i belirli bir yazarkasa
markasına bağımlı tasarlanmamalıdır.

Cihaz entegrasyonu adapter/integration katmanında ele alınmalıdır.

---

# 67. E-FATURA

e-Fatura / e-Arşiv gelecekte desteklenecektir.

Ancak Fatura domain'i belirli bir e-Fatura sağlayıcısına bağımlı
tasarlanmamalıdır.

PanoPOS Fatura kendi ticari belgesidir.

Elektronik belge gönderimi entegrasyon katmanıdır.

---

# 68. ÜRETİM

Üretim PanoPOS'un ilk fazının ana hedefi değildir.

İleride:

Hammadde
YariMamul
Mamul
Recete
UretimEmri
UretimGiris
UretimCikis

gibi kavramlar eklenebilir.

Mevcut stok altyapısı gelecekte üretime genişleyebilecek kadar temiz
olmalıdır.

Ancak bugünden tam üretim ERP'si tasarlanmamalıdır.

---

# 69. PERFORMANS BEKLENTİSİ

PanoPOS gerçek zamanlı satış ekranında kullanılacaktır.

Kasiyer barkod okuttuğunda kullanıcı gereksiz beklememelidir.

Bu nedenle özellikle:

- ürün arama
- barkod çözümleme
- fiyat bulma
- sepet işlemleri
- ödeme

hızlı olmalıdır.

Ancak performans adına gereksiz cache veya karmaşık altyapı
başlangıçta eklenmemelidir.

---

# 70. BASİTLİK

PanoPOS'un önemli hedeflerinden biri bakım maliyetini azaltmaktır.

Yeni sistem eski yazılımdaki teknik borcun başka teknolojiyle tekrar
oluşturulması olmamalıdır.

Bir problemi çözmek için 3 tablo yeterliyse 10 tablo oluşturma.

Bir servis yeterliyse gereksiz framework oluşturma.

Ancak ticari verinin doğruluğundan taviz verme.

---

# 71. GERÇEK HAYAT SENARYOSU - MARKET

İşletme:

Mahalle marketi

1 şube
2 kasa
1 varsayılan depo

Ürün:

Coca Cola 330 ml

Barkod okutulur.

Sistem:

StokKart
Satış Birimi = Adet
FiyatTipi = Perakende
KDV

bilgilerini bulur.

Market KDV dahil çalışmaktadır.

Ekran fiyatı:

60 TL

ise müşteri:

60 TL

öder.

KDV 60 TL'nin içinden ayrıştırılır.

Ödeme:

Nakit

ise:

Siparis
    ->
Fatura
    ->
Tahsilat
    ->
KasaHareket

akışı oluşur.

Stok entegrasyonu tamamlandığında ayrıca ilgili depodan stok
azaltılacaktır.

---

# 72. GERÇEK HAYAT SENARYOSU - TOPTANCI

İşletme:

Toptancı

Ürün:

Su

Satış:

1 Koli

Koli:
24 Adet

Birim fiyat:
1.000 TL

KDV:
%20

İşletme KDV hariç çalışmaktadır.

Sonuç:

Matrah:
1.000 TL

KDV:
200 TL

Toplam:
1.200 TL

Belgede:

Miktar = 1
Birim = Koli
Katsayi = 24
KdvOrani = 20
KdvDahilMi = false

snapshot tutulur.

Stok entegrasyonu tamamlandığında stok etkisi:

24 temel birim çıkış

olacaktır.

---

# 73. GERÇEK HAYAT SENARYOSU - KARMA ÖDEME

Market satışı:

Toplam:
1.000 TL

Müşteri:

700 TL nakit
300 TL kredi kartı

öder.

Sonuç:

1 Fatura

ve:

2 Tahsilat

oluşur.

Nakit tahsilat:

KasaHareket

kredi kartı tahsilatı:

BankaHareket

oluşturur.

---

# 74. GERÇEK HAYAT SENARYOSU - ALIŞ

Tedarikçi:

ABC Gıda

alış faturası gönderir.

Ürün:

Su
10 Koli

Koli:
24 Adet

AlisFatura oluşturulur.

AlisFaturaDetay üzerinde:

Miktar = 10
Birim = Koli
Katsayi = 24

snapshot tutulur.

AlisFatura Taslak olarak hazırlanabilir.

Kontrol edildikten sonra:

Kesinlesti

durumuna alınır.

Kesinleşme ticari belge durumunu değiştirir ve aynı transaction içinde:

AlisFatura
    ->
StokFis
    ->
StokHareket

oluşur ve faturada açıkça seçilen hedef depoya:

240 temel birim

stok girişi yapılır. Aynı fatura tekrar kesinleştirilirse ikinci
stok girişi oluşmaz. Bu stoklanmış faturanın iptal/iade ters hareketi
henüz uygulanmamıştır; sessiz durum değişikliği engellenir.

---

# 75. GERÇEK HAYAT SENARYOSU - RESTAURANT

Garson:

Masa 8

seçer.

Açık adisyon oluşturulur.

Sipariş:

2 Lahmacun
2 Ayran

eklenir.

Daha sonra müşteri:

1 Çay

ister.

Aynı adisyona yeni sipariş eklenebilir.

Hesap istendiğinde adisyonun ticari satışları ödeme akışına alınır.

Müşteri:

Nakit
Kredi Kartı
veya karma ödeme

yapabilir.

Sonuçta satış:

Fatura
+
Tahsilat

ile tamamlanır.

---

# 76. GERÇEK HAYAT SENARYOSU - İNTERNET KESİNTİSİ

Restaurant veya markette internet bağlantısı kesilir.

Şubenin:

Local API
Local Database

çalışmaya devam ettiği sürece temel satış operasyonunun devam etmesi
hedeflenmektedir.

Kasiyer satış yapabilmelidir.

Garson yerel ağ üzerinden sipariş verebilmelidir.

İnternet geri geldiğinde merkez/ERP senkronizasyonu daha sonra
gerçekleşebilir.

Bu senkronizasyon davranışının ayrıntıları gelecekte ayrıca
tasarlanacaktır.

---

# 77. GERÇEK HAYAT SENARYOSU - ÇOK ŞUBE

Müşterinin:

20 şubesi

olabilir.

Her şube kendi yerel operasyonunu yürütür.

Merkez gelecekte:

ürün
fiyat
ayar

gibi verileri şubelere gönderebilir.

Şubeler:

satış
tahsilat
stok hareketi

gibi verileri merkeze aktarabilir.

Bir şubenin internet problemi diğer şubelerin satışını
durdurmamalıdır.

---

# 78. MEVCUT ÇEKİRDEK MODÜLLER

Projede şu ana kadar temel olarak oluşturulmuş alanlar arasında:

- Tenant
- Sube
- Cihaz
- Kullanici
- Rol
- PIN Auth
- Kasa
- Vardiya
- StokKart
- StokKartVaryant
- Barkod
- StokKategori
- StokGrup
- StokKartSatisBirimi
- FiyatTipi
- StokKartFiyat
- Kdv
- TenantAyar
- Cari
- Masa
- MasaGrup
- Adisyon
- Siparis
- SiparisDetay
- Fatura
- FaturaDetay
- Tahsilat
- Banka
- BankaHareket
- KasaHareket
- CariHareket
- AlisFatura
- AlisFaturaDetay
- Depo
- StokFis
- StokFisDetay
- StokHareket
- Devir / İlk Stok
- Sayım
- DepoTransfer
- IslemLog
- OutboxOlay

bulunmaktadır.

Bu liste projenin ilerlemesiyle güncellenmelidir.

---

# 79. HENÜZ TAMAMLANMAMIŞ ÖNEMLİ ALANLAR

Aşağıdaki alanların bazıları planlanmıştır ancak tam uygulaması henüz
yapılmamıştır:

- stok satış çıkışı
- satış/alış iade stok entegrasyonu
- gelişmiş stok bakiye optimizasyonu
- alış ödeme/borç entegrasyonunun tamamı
- gerçek merkez/şube sync
- ERP adapter
- e-Fatura
- yazarkasa POS
- reçete
- üretim
- lisans API
- web yönetim
- mobil garsonun yeni backend ile tam entegrasyonu
- gelişmiş yetkilendirme

Bu maddeleri görev istemedikçe otomatik implement etme.

Stok çekirdeği endpointleri:

- POST /api/v1/stok-fis/devir
- POST /api/v1/stok-fis/sayim
- POST /api/v1/stok-fis/depo-transfer
- GET /api/v1/stok-fis/{id}?subeId=...
- GET /api/v1/stok-fis?subeId=...&page=1&pageSize=50
- GET /api/v1/stok/miktar?subeId=...&depoId=...&stokKartId=...&stokKartVaryantId=...

Fiş listesi ayrıca StokFisTipi, DepoId ve Arama filtrelerini destekler.
Tenant mevcut proje yaklaşımıyla SubeId üzerinden çözülür; bağlı
stok/varyant/birim/depo ilişkileri bu kapsama göre doğrulanır.
FisNo verilmezse mevcut tarih/sıra yaklaşımıyla STK-yyyyMMdd-000001
biçiminde üretilir; verilirse Tenant içinde tekrar kullanılamaz.
Fişin aynı numarayla tekrar gönderilmesi ikinci stok etkisi üretmez.
Miktar ve katsayı decimal(18,4) sınırındadır; bu hassasiyete sığmayan
temel miktar sessizce yuvarlanmaz, istek reddedilir.
Kritik stok operasyonları IslemLog'a aynı transaction içinde yazılır.
Cihaz bağlamı zorunlu olmayan bu işlemlerde Outbox olayı eklenmemiştir;
mevcut Outbox altyapısı değiştirilmemiştir.

Stok migration: 20260917080742_AddStockLedgerCore.
PanoPosDb database update başarılı; 42 yeni SQLite stok testi ile
toplam 193/193 test başarılıdır. Solution build başarılıdır.
Fatura satış stok bağlantısı, StokBakiye ve Desktop değişikliği yoktur.

Alış stok entegrasyonu migration:
20260917084513_LinkPurchaseInvoiceToStockLedger.
PanoPosDb database update başarılıdır. 24 yeni SQLite testi ile
toplam 217/217 test başarılı; solution build başarılıdır.
POST/PUT /api/v1/alis-fatura sözleşmesinde DepoId zorunludur;
liste/detay cevapları DepoId, stok fişi cevapları AlisFaturaId içerir.
POST /api/v1/alis-fatura/{id}/kesinlestir stok üretir.
Stoklanmış faturanın iptal isteği purchase_stock_reversal_required
hatasıyla reddedilir. Cari/ödeme entegrasyonu ve Outbox event'i
eklenmemiştir; mevcut stok çekirdeği gibi IslemLog kullanılır.

---

# 80. GELİŞTİRME ÖNCELİĞİ

Şu an temel öncelik:

1. Hızlı Satış
2. Restaurant
3. Ortak ticari çekirdek
4. Stok
5. Alış
6. Kasa / Tahsilat

alanlarının stabil hale gelmesidir.

Daha sonra:

- sync
- ERP
- e-Fatura
- lisans
- web
- mobil
- üretim

gibi alanlar genişletilecektir.

---

# 81. DOMAIN SINIRLARI

Bir kavramın görevini başka bir kavrama yükleme.

Örnek:

AlisFatura = ticari alış belgesi

StokFis = stok belgesi

StokHareket = stok ledger kaydı

Tahsilat = alınan ödeme

KasaHareket = fiziksel kasa hareketi

CariHareket = cari borç/alacak hareketi

OutboxOlay = entegrasyon/sync olayı

IslemLog = audit

Bu kavramların ayrı olmasının nedeni aynı işlemin farklı iş
sonuçlarını temsil etmeleridir.

---

# 82. CODEX İÇİN EN ÖNEMLİ BAĞLAM

PanoPOS'u yalnızca CRUD ekranlarından oluşan bir uygulama olarak
düşünme.

Bu sistem gerçek işletmelerde:

- kasa sırasında
- yoğun restaurant servisinde
- internet kesildiğinde
- birden fazla cihaz aynı anda çalışırken
- farklı KDV/fiyat modellerinde
- farklı satış birimleriyle
- çok şubeli yapılarda

kullanılacaktır.

Bu nedenle iş kuralları doğru olmalı, fakat çözüm gereksiz derecede
karmaşık olmamalıdır.

---

# 83. YENİ ÖZELLİK GELİŞTİRİRKEN

Yeni bir özellik geliştirirken şu sorular düşünülmelidir:

1. Bu özellik hangi gerçek işletme senaryosunu çözüyor?
2. Hızlı satış ve restaurant kullanımını etkiliyor mu?
3. Offline şube çalışmasını gereksiz yere bozuyor mu?
4. Tenant/Şube sınırları doğru mu?
5. Ticari geçmiş snapshot ile korunuyor mu?
6. Stok/finans/ticari belge kavramları birbirine karıştırılıyor mu?
7. Başka ERP'ye gereksiz bağımlılık oluşturuyor mu?
8. Gelecekte çok şube kullanımını engelliyor mu?
9. Gereğinden fazla mimari karmaşıklık oluşturuyor mu?
10. Bu özellik gerçekten mevcut görevin kapsamında mı?

---

# 84. SON PRENSİP

PanoPOS'un amacı mümkün olan en fazla özelliği eklemek değildir.

Amaç:

gerçek işletmede çalışan,
hızlı,
stabil,
anlaşılır,
bakımı kolay,
ticari verisi güvenilir

bir sistem oluşturmaktır.

Öncelik her zaman:

Gerçek Kullanım
    ->
Doğru İş Kuralı
    ->
Basit Tasarım
    ->
Güvenilir Veri
    ->
Performans
    ->
Genişletilebilirlik

sırasıdır.
