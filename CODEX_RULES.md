# PanoPOS - CODEX_RULES.md

Bu dosya PanoPOS projesinin backend, veritabanı, API ve genel geliştirme
kurallarını tanımlar.

Codex herhangi bir geliştirme görevine başlamadan önce bu dosyayı okumalıdır.

Görev promptunda açıkça aksi belirtilmediği sürece bu kurallar geçerlidir.

---

# 1. TEMEL PRENSİPLER

PanoPOS:

- Basit
- Stabil
- Performanslı
- Bakımı kolay
- Genişletilebilir

olmalıdır.

Gereksiz mimari karmaşıklık oluşturma.

İhtiyaç oluşmadan:

- cache
- event bus
- message broker
- generic repository katmanları
- gereksiz abstraction
- microservice
- karmaşık configuration framework

ekleme.

YAGNI prensibini uygula.

Ancak ileride gerekli olacak özelliklerin önünü kapatacak tasarım da yapma.

---

# 2. TEKNOLOJİ

Backend:

- .NET 8
- ASP.NET Core Web API
- EF Core
- Dapper
- SQL Server

Write işlemlerinde genel tercih:

EF Core

Listeleme / arama / raporlama sorgularında genel tercih:

Dapper

Relational backend testlerinde:

SQLite in-memory

kullan.

EF Core InMemory provider, ilişkisel davranış ve transaction testleri için
tercih edilmemelidir.

---

# 3. VERİTABANI İSİMLENDİRME

Entity ve tablo isimleri mümkün olduğunca:

- Türkçe
- anlaşılır
- tekil

olmalıdır.

Türkçe karakterleri teknik isimlerde kullanma.

Örnek:

StokKart
StokKartVaryant
StokKartSatisBirimi
StokKartFiyat
Cari
Siparis
SiparisDetay
Fatura
FaturaDetay
AlisFatura
AlisFaturaDetay
Depo
Kdv

Eski migration geçmişindeki isimleri sırf isim standardı için değiştirme.

---

# 4. PRIMARY KEY / FOREIGN KEY

Standart primary key:

Id bigint

Foreign key alanları da:

bigint

olmalıdır.

PK/FK tipleri tutarlı olmalıdır.

TenantId:

uniqueidentifier / Guid

olmalıdır.

---

# 5. MULTI-TENANT

PanoPOS multi-tenant sistemdir.

Tenant izolasyonu zorunludur.

Tenant'a bağlı tüm işlemlerde:

TenantId

scope uygulanmalıdır.

Başka Tenant'a ait kayıtlar:

- okunamamalı
- güncellenememeli
- silinememeli
- başka entity'lere bağlanamamalıdır.

Foreign key'in mevcut olması tek başına yeterli değildir.

İlişkilendirilen kaydın doğru Tenant'a ait olduğu servis katmanında
doğrulanmalıdır.

---

# 6. ŞUBE SCOPE

Şube bazlı entity'lerde:

SubeId bigint

kullanılır.

Başka şubeye ait verilerin yanlışlıkla kullanılmasını engelle.

Ancak Tenant seviyesindeki master tabloları gereksiz yere şubeye bağlama.

Örneğin Kdv gibi Tenant seviyesindeki tanımlar için gereksiz SubeId ekleme.

---

# 7. ORTAK AUDIT ALANLARI

Uygun entity'lerde mevcut proje standardına göre:

OlusturmaTarihi
GuncellemeTarihi

OlusturanKullaniciId
GuncelleyenKullaniciId

SilenKullaniciId
SilinmeTarihi

AktifMi
SilindiMi

kullan.

Yeni entity oluştururken önce mevcut base entity / mapping standardını incele.

Aynı altyapıyı tekrar oluşturma.

---

# 8. SOFT DELETE

Normal master ve operasyonel kayıtlarda fiziksel DELETE yerine soft delete
tercih edilir.

SilindiMi = true

ve:

SilinmeTarihi
SilenKullaniciId

kullanılır.

Normal sorgular silinmiş kayıtları döndürmemelidir.

Ancak immutable/audit kayıtları için farklı kurallar olabilir.

Örneğin:

IslemLog
OutboxOlay

kalıcı kayıtlardır.

---

# 9. TİCARİ BELGELER

Kesinleşmiş ticari belgeler normal CRUD kaydı gibi ele alınmamalıdır.

Örneğin:

Fatura
AlisFatura

kesinleştikten sonra sessizce değiştirilmemeli veya geçmişten
silinmemelidir.

Düzeltme gereken durumlarda ileride:

- iptal
- iade
- ters hareket
- düzeltme belgesi

gibi ticari akışlar kullanılacaktır.

---

# 10. SNAPSHOT PRENSİBİ

Ticari belge oluşturulduğu anda işlemi etkileyen bilgiler snapshot olarak
saklanmalıdır.

Master kayıt daha sonra değişse bile eski ticari belge değişmemelidir.

Örnek:

StokKart -> KdvId

güncel ürün KDV tanımını gösterir.

Ancak:

SiparisDetay
FaturaDetay
AlisFaturaDetay

üzerinde:

KdvId
KdvOrani
KdvDahilMi

snapshot tutulur.

Aynı prensip aşağıdakiler için de geçerlidir:

- BirimAdi
- BirimKodu
- Katsayi
- BirimFiyat
- ParaBirimKodu
- Kur
- KDV
- iskonto
- ticari hesaplamayı etkileyen diğer bilgiler

Master kayıtlardaki sonraki değişiklikler geçmiş belgeleri değiştirmemelidir.

---

# 11. PARA HESAPLARI

Para hesaplarında:

decimal

kullan.

double veya float kullanma.

Genel mevcut standart:

BirimFiyat decimal(18,4)

Kur decimal(18,6)

Belge parasal toplamları:

decimal(18,2)

Yuvarlama:

MidpointRounding.AwayFromZero

ile deterministik yapılmalıdır.

Aynı işlem farklı endpointlerde farklı sonuç üretmemelidir.

---

# 12. PARA BİRİMİ

ParaBirimKodu:

nvarchar(10)

mantığında tutulur.

Kaydetmeden önce:

Trim
Uppercase

uygulanır.

Sistemi yalnızca:

TRY
USD
EUR
GBP

ile sınırlandırma.

TRY için kur:

1

olmalıdır.

Kur bilgisi fiyat master kaydına gömülmemelidir.

Kur ticari işlem sırasında snapshot olarak alınır.

---

# 13. KDV MASTER YAPISI

KDV oranları master kayıt olarak:

Kdv

tablosunda tutulur.

StokKart:

KdvId

ile Kdv kaydına bağlanır.

KDV oranlarını enum veya hard-coded liste haline getirme.

Başlangıç seedleri olabilir:

%0
%1
%10
%20

Ancak sistem ileride yeni oran eklenmesini desteklemelidir.

Ticari belgelerde yalnız KdvId'ye güvenilmez.

KdvOrani snapshot olarak saklanır.

---

# 14. KDV DAHİL / HARİÇ

Sistem hem:

KDV dahil

hem:

KDV hariç

fiyatlarla çalışmalıdır.

Tenant seviyesinde iki ayrı ayar bulunur:

SatisFiyatlariKdvDahilMi
AlisFiyatlariKdvDahilMi

Satış ve alış birbirinden bağımsızdır.

Ayar sonradan değiştiğinde eski belge etkilenmemelidir.

Bu nedenle:

KdvDahilMi

ticari belge detayında snapshot tutulur.

---

# 15. KDV HESAPLAMA

KDV hariç fiyat:

Matrah üzerinden KDV eklenir.

KDV dahil fiyat:

KDV toplam fiyatın içinden ayrıştırılır.

KDV dahil fiyatın üzerine tekrar KDV eklenmez.

KDV hesaplaması controller'larda tekrar edilmemelidir.

Merkezi hesaplama servisi kullanılmalıdır.

---

# 16. İSKONTO

MVP'de:

- Satır iskontosu
- Genel belge/fatura iskontosu

desteklenir.

Şimdilik:

Iskonto1
Iskonto2
Iskonto3

gibi zincir iskonto yapıları oluşturma.

Satır iskontosu vergi matrahından önce uygulanır.

Genel iskonto da vergi matrahını azaltır.

Genel iskonto farklı KDV oranlı satırlara oransal dağıtılmalıdır.

Kuruş farkları deterministik şekilde dağıtılmalıdır.

Satır toplamları ile header toplamları tutarlı olmalıdır.

---

# 17. MERKEZİ HESAPLAMA

Ticari hesaplama kurallarını mümkün olduğunca merkezi servislerde tut.

Aynı:

- KDV
- iskonto
- yuvarlama
- genel iskonto dağıtımı

formüllerini farklı controller/service içerisinde kopyalama.

Siparis, Fatura ve AlisFatura aynı hesaplama standardını kullanmalıdır.

---

# 18. STOK KARTI

Ana ürün entity'si:

StokKart

tır.

Eski teknik Urun/UrunVaryant isimlerini yeni backend geliştirmelerinde
yeniden kullanma.

İlgili yapılar:

StokKart
StokKartVaryant
StokKartSatisBirimi
StokKartFiyat
StokKategori
StokGrup

şeklindedir.

---

# 19. SATIŞ BİRİMİ

Bir StokKart birden fazla satış birimine sahip olabilir.

Örnek:

Adet
Kutu
Koli
Palet

StokKartSatisBirimi.Katsayi temel miktarı ifade eder.

Örnek:

Adet = 1
Kutu = 10
Koli = 100

Ticari belgeye birim ve Katsayi snapshot olarak yazılır.

Sonradan satış biriminin Katsayi değeri değişirse eski belge değişmemelidir.

---

# 20. BARKOD

Bir barkod:

StokKart

veya:

StokKartVaryant

ile ilişkili olabilir.

XOR kuralını koru.

Aynı Tenant içinde:

BarkodNo

unique olmalıdır.

Barkod satış birimini belirleyebilir.

Ancak barkod doğrudan fiyat tipini belirlemez.

Fiyat:

Satış Birimi + Fiyat Tipi

üzerinden belirlenir.

---

# 21. FİYAT

FiyatTipi master tablodur.

Örnek başlangıç tipleri:

Perakende
KrediKarti
Toptan

Ancak yeni fiyat tipleri eklenebilmelidir.

StokKartFiyat:

StokKartSatisBirimiId
FiyatTipiId
Fiyat
ParaBirimKodu

mantığında çalışır.

StokKartFiyat üzerinde Kur tutma.

---

# 22. DEPO

Stok takibi depo bazında yapılacaktır.

Her şubenin varsayılan deposu bulunur.

Başlangıçta:

DepoKodu = MERKEZ
Ad = Merkez Depo
VarsayilanMi = true

kullanılabilir.

Teknik ilişkiler her zaman:

DepoId

ile yapılır.

Depo adına göre ilişki kurma.

İleride çoklu depo ve depo transferi desteklenmelidir.

---

# 23. STOK MİKTARI

Stok miktarını:

StokKart.StokMiktari

gibi mutable bir kolonla yönetme.

Stok miktarının kaynağı stok hareketleri olacaktır.

StokKart + Depo

bazında takip yapılacaktır.

İlk stok miktarı girilirse doğrudan StokKart üzerinde miktar değiştirilmez.

İleride uygun:

Devir
Sayim
IlkStok

hareketi oluşturulacaktır.

---

# 24. STOKBAKIYE

Şimdilik StokBakiye gibi cache/current-balance tablosunu otomatik oluşturma.

Öncelikli kaynak:

StokHareket

olacaktır.

Performans ihtiyacı ortaya çıktığında StokBakiye ayrıca değerlendirilecektir.

---

# 25. EKSİ STOK

Sistemin genel tasarımı eksi stok çalışmasına izin verebilmelidir.

Restaurant, market ve hızlı satış senaryolarında satışın gereksiz yere
engellenmemesi önemlidir.

İleride:

- global
- depo bazlı
- stok kartı bazlı

eksi stok kontrol ayarları eklenebilir.

Görev açıkça istemedikçe eksi stok engelleme mekanizması oluşturma.

---

# 26. SATIŞ AKIŞI

Temel satış akışı:

Siparis
    ->
Fatura + FaturaDetay
    ->
Tahsilat

şeklindedir.

Ödeme yapıldığında ticari satış Fatura üzerinden tamamlanır.

Bekletilen hızlı satış:

Siparis

olarak kalabilir.

Restaurant siparişleri Adisyon üzerinden Siparis ile ilişkilidir.

Mevcut çalışan satış akışını yeni görevlerde gereksiz yere değiştirme.

---

# 27. ALIŞ AKIŞI

Alış işlemi yalnızca StokFis değildir.

Ticari alış belgesi:

AlisFatura
AlisFaturaDetay

olarak tutulur.

Planlanan akış:

AlisFatura
    ->
StokFis
    ->
StokHareket

şeklindedir.

AlisFatura ticari/finansal belgedir.

StokFis stok belgesidir.

StokHareket gerçek stok hareket ledger'ıdır.

Bu katmanları birbirine karıştırma.

---

# 28. STOKFIS / STOKHAREKET

StokFis ve StokHareket modülleri kullanıcıyla iş kuralları
netleştirilmeden otomatik olarak tasarlanıp genişletilmemelidir.

Görev açıkça istemedikçe:

StokFis
StokFisDetay
StokHareket

oluşturma veya davranışlarını değiştirme.

Bu alan PanoPOS için kritik ticari çekirdektir.

---

# 29. CARİ

Cari ortak ticari hesap yapısıdır.

CariTipi mevcut sistemde:

Satici
Alici
Personel
Masraf

gibi tipler içerebilir.

Bir Cari'nin gelecekte hem alıcı hem satıcı olabilmesini engelleyecek
gereksiz katı kurallar oluşturma.

Tenant izolasyonunu her zaman koru.

---

# 30. FATURA / ALIŞ FATURASI

Fatura satış belgesidir.

AlisFatura alış belgesidir.

İki kavramı tek tabloya birleştirme.

Her iki yapıda da ticari snapshot prensibini uygula.

Kesinleşmiş belgelerin geçmişini koru.

---

# 31. DAPPER

Listeleme ve arama endpointlerinde Dapper tercih edilir.

SELECT * KULLANMA.

DTO için gereken kolonları açıkça seç.

Liste sorgularında:

- Tenant filtreleme
- soft delete filtreleme
- gerekli Sube filtreleme
- pagination

uygulanmalıdır.

---

# 32. PAGINATION

Liste endpointleri kontrolsüz şekilde tüm tabloyu dönmemelidir.

Genel olarak:

page
pageSize

desteklenmelidir.

Uygun maksimum pageSize sınırı uygulanabilir.

---

# 33. EF CORE

Create/Update/Delete ve transaction gerektiren operasyonlarda mevcut
EF Core mimarisini kullan.

Yeni bir repository/unit-of-work framework oluşturma.

Mevcut DbContext ve transaction yaklaşımına uy.

---

# 34. TRANSACTION

Bir ticari işlem birden fazla tabloyu birlikte değiştiriyorsa transaction
kullan.

Örnek:

AlisFatura + AlisFaturaDetay

veya:

Siparis -> Fatura

gibi işlemler yarım kalmamalıdır.

Transaction başarısız olursa kısmi kayıt bırakma.

---

# 35. OUTBOX

Mevcut:

OutboxOlay

altyapısını koru.

Yeni entegrasyon/senkronizasyon görevlerinde mevcut outbox yapısını
incelemeden ikinci bir outbox sistemi oluşturma.

Gerçek senkronizasyon görev açıkça istemedikçe eklenmemelidir.

---

# 36. AUDIT / İŞLEM LOGU

Mevcut:

IslemLog

kalıcı işlem audit yapısıdır.

Teknik application log ile IslemLog'u birbirine karıştırma.

Yeni audit framework oluşturma.

Mevcut yapıyı kullan.

---

# 37. API

Endpoint standardı:

/api/v1/...

şeklindedir.

Mevcut endpoint naming standardına uy.

API değişikliği yaparken mevcut client kullanımını gereksiz yere kırma.

Breaking değişiklik gerekiyorsa görev kapsamında açıkça belirtilmelidir.

---

# 38. VALIDATION

Client tarafından gönderilen ticari hesap sonuçlarına güvenme.

Örneğin client'ın gönderdiği:

AraToplam
Matrah
KdvTutari
NetToplam

backend için kaynak kabul edilmemelidir.

Backend hesaplamalıdır.

Foreign key ilişkilerinde:

- kayıt mevcut mu
- doğru Tenant mı
- gerekiyorsa doğru Sube mi
- aktif mi
- silinmiş mi

kontrol edilmelidir.

---

# 39. SQL SERVER INDEX

Indexleri gerçek sorgu ihtiyaçlarına göre oluştur.

Genel sorgu alanları:

Id
TenantId
SubeId
Tarih
Durum

olabilir.

Her kolona gereksiz index oluşturma.

Composite indexleri gerçek sorgulara göre tasarla.

---

# 40. SQL SERVER UYUMLULUĞU

Veritabanı tasarımı mümkün olduğunca projenin hedef SQL Server
uyumluluğunu korumalıdır.

SQL Server'a özel yeni özellik kullanmadan önce mevcut proje/migration
standardını kontrol et.

SQLite in-memory testleriyle SQL Server davranış farklarını göz önünde
bulundur.

---

# 41. GELİŞTİRME VERİTABANI KURALI

PanoPosDb şu anda GELİŞTİRME veritabanıdır.

PanoPosDb içinde korunması gereken production veri YOKTUR.

Mevcut geliştirme aşamasında:

- StokKart
- Siparis
- Fatura
- AlisFatura
- stok hareketi
- finansal geçmiş

gibi verilerin migration sırasında korunması zorunlu değildir.

Migration yazarken production veri taşıma senaryosu varsayma.

Eski ticari belgeleri:

- yeniden hesaplayan
- yeniden dağıtan
- dönüştüren
- finansal geçmişi korumaya çalışan

karmaşık migration kodları oluşturma.

Yeni kolon/tablo için:

- gerekli default
- seed
- FK
- index
- constraint

oluşturmak yeterlidir.

Gerekirse PanoPosDb silinip migration zincirinden temiz şekilde yeniden
oluşturulabilir.

Production migration/veri taşıma konusu ileride ayrıca ele alınacaktır.

---

# 42. MIGRATION

Mevcut migration geçmişini gereksiz yere değiştirme.

Yeni schema değişikliği için yeni migration oluştur.

Ancak geliştirme DB'sinde eski veriyi korumak adına gereksiz data migration
kodları yazma.

Migration sonrasında mümkünse:

database update

çalıştır ve sonucu doğrula.

Migration başarısızsa görevi tamamlanmış sayma.

---

# 43. TEST

Yeni iş kuralı testsiz bırakılmamalıdır.

Relational davranış gereken backend testlerinde:

SQLite in-memory

kullan.

Özellikle test edilmesi gerekenler:

- Tenant izolasyonu
- transaction
- unique constraint
- soft delete
- durum geçişleri
- snapshot
- hesaplama
- yuvarlama
- KDV
- iskonto
- pagination

Mevcut testleri bozma.

---

# 44. BUILD

Görev sonunda:

dotnet test

ve:

dotnet build

çalıştır.

Yeni migration varsa database update de çalıştır.

Başarısız test veya build varken görevi tamamlandı olarak raporlama.

---

# 45. MEVCUT KULLANICI DEĞİŞİKLİKLERİ

Working tree'de kullanıcıya ait commit dışı değişiklikler olabilir.

Görevle ilgisiz kullanıcı dosyalarına dokunma.

Özellikle açıkça izin verilmedikçe:

- reset
- stash
- checkout ile geri alma
- overwrite
- clean

yapma.

appsettings.json gibi yerel configuration değişikliklerini görev
gerektirmedikçe değiştirme veya commit'e alma.

---

# 46. GIT

Görev sonunda otomatik commit/push yapma.

Standart sıra:

1. Kod değişiklikleri
2. Migration
3. Database update
4. Test
5. Build
6. Git status
7. Son rapor
8. Kullanıcı onayı
9. Commit
10. Push

Kullanıcı açıkça commit/push izni verirse uygula.

---

# 47. KAPSAM KONTROLÜ

Görevde istenmeyen modülleri "ileride lazım olur" düşüncesiyle oluşturma.

Örneğin görev AlisFatura ise otomatik olarak:

StokHareket
CariHareket
Odeme
e-Fatura
TCMB
StokBakiye

ekleme.

Görev kapsamını koru.

---

# 48. MEVCUT YAPIYI ÖNCE İNCELE

Yeni:

- entity
- servis
- helper
- base class
- configuration
- enum
- abstraction

oluşturmadan önce projede eşdeğer yapı olup olmadığını kontrol et.

Var olan standardı kullan.

Aynı işi yapan ikinci altyapıyı oluşturma.

---

# 49. GERİYE DÖNÜK DAVRANIŞ

Yeni geliştirme mevcut çalışan modülleri gereksiz yere bozmamalıdır.

Özellikle:

Siparis -> Fatura -> Tahsilat

gibi çalışan çekirdek akışlarda regression testlerini koru.

Yeni özellik eklenirken eski testlerin tamamı geçmelidir.

---

# 50. PERFORMANS

Öncelik:

basit ve doğru tasarım.

Ancak açık performans hataları oluşturma.

Kaçınılması gerekenler:

- kontrolsüz tüm tablo sorguları
- SELECT *
- gereksiz Include zincirleri
- N+1 sorgular
- client tarafında yapılabilecek diye backend'de tüm tabloyu belleğe çekme

Performans problemi kanıtlanmadan karmaşık cache sistemi oluşturma.

---

# 51. HATA YÖNETİMİ

Mevcut GlobalExceptionHandler / ProblemDetails yaklaşımını kullan.

Her controller için ayrı hata sistemi oluşturma.

İş kuralı ihlalleri anlaşılır hata üretmelidir.

Teknik exception detaylarını gereksiz yere API client'a sızdırma.

---

# 52. DOKÜMANTASYON

Yeni modül veya önemli iş kuralı eklendiğinde kısa ve güncel
dokümantasyon bırak.

Ancak koddan kopuk, büyük ve bakım yükü oluşturacak dokümanlar üretme.

Dokümantasyon gerçek implementasyonla uyumlu olmalıdır.

---

# 53. TODO KULLANIMI

Kapsam dışındaki gelecekteki işleri otomatik implement etme.

Gerekirse kısa TODO/not bırak.

Örneğin:

- kesinleşmiş alış faturası iptalinde ters stok hareketi
- alış ödeme
- ERP sync
- e-Fatura
- StokBakiye

gelecek görevler olabilir.

TODO bırakmak mevcut görevin kapsamını genişletmekten daha iyidir.

---

# 54. GÖREV SONU RAPORU

Görev tamamlanmadan başarı raporu verme.

Görev sonunda kısa şekilde:

- Ne yapıldı
- Migration adı
- Database update sonucu
- Test sonucu (x/x)
- Build sonucu
- Önemli tasarım kararları
- Dokunulmayan kullanıcı değişiklikleri
- Git status

raporlanmalıdır.

Commit/push yapılmadıysa açıkça belirtilmelidir.

---

# 55. DESKTOP KAPSAMI

Desktop / WinForms / DevExpress kuralları bu dosyanın kapsamı DIŞINDADIR.

Desktop tarafı için ayrı bir kural dosyası kullanılacaktır.

Backend görevi açıkça Desktop değişikliği istemiyorsa Desktop projesine
dokunma.

---

# ANA PRENSİP

PanoPOS için her geliştirmede şu sırayı koru:

DOĞRU İŞ KURALI
    ↓
BASİT VERİ MODELİ
    ↓
BACKEND DOĞRULAMA
    ↓
TRANSACTION / SNAPSHOT
    ↓
TEST
    ↓
PERFORMANS
    ↓
UI / ENTEGRASYON

Gereksiz özellik ekleme.

Ticari geçmişi değiştirme.

Tenant izolasyonunu bozma.

Çalışan akışları bozma.

Görev kapsamının dışına çıkma.