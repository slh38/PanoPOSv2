# PanoPOS - DESKTOP_RULES.md

Bu dosya yeni PanoPOS Desktop uygulamasının kalıcı geliştirme
anayasasıdır. Codex Desktop görevi öncesinde sırasıyla
`PROJECT_CONTEXT.md`, `CODEX_RULES.md` ve `DESKTOP_RULES.md` dosyalarını
tamamen okumalıdır. Gelecek hedefleri görev açıkça istemedikçe otomatik
implement edilmemelidir.

## 1. Temel teknoloji ve sınır

-   İlk Desktop: **React + TypeScript + Tauri**.
-   Yeni Desktop sıfırdan geliştirilebilir; eski WinForms/DevExpress
    kodu geriye uyumluluk zorunluluğu oluşturmaz.
-   Desktop doğrudan SQL Server'a bağlanmaz:
    `Desktop -> Local API -> Database`.
-   SQL connection string, doğrudan SQL/SP çağrısı Desktop'ta bulunmaz.
-   Fiyat, KDV, iskonto, stok, maliyet, fatura, tahsilat, tenant/şube
    güvenliği, concurrency ve idempotency backend otoritesindedir.
-   Backend iş kurallarını React içinde ikinci kez yazma.
-   Backend/Ticari domain Tauri'ye bağımlı hale getirilmemelidir.

## 2. Kod mimarisi

Sade feature/component yaklaşımı kullan. Önerilen başlangıç:

``` text
src/
  api/
  components/
  features/
  layouts/
  pages/
  hooks/
  stores/
  types/
  utils/
  tauri/
```

İhtiyaç yoksa yeni katman/framework ekleme. Componentleri küçük ve tek
sorumluluklu tut. API çağrılarını UI componentlerine dağıtma.

## 3. API client

Tek merkezi HTTP client kullan:

-   configurable Base URL
-   Bearer token
-   timeout
-   JSON
-   ProblemDetails/domain hata dönüşümü
-   merkezi 401 davranışı

Her component kendi `fetch` altyapısını yazmamalıdır. DTO'lar TypeScript
ile açık tiplenmeli; gereksiz `any` kullanılmamalıdır.

Local API adresi hard-code edilmez. Ana makinede localhost, terminalde
LAN IP kullanılabilir. Desktop yalnız API adresini saklar; DB connection
string saklamaz.

## 4. Login ve oturum

Giriş PIN ile `POST /api/v1/auth/login` üzerinden yapılır. Dönen opaque
`OturumToken` korunan isteklerde `Authorization: Bearer <token>` olarak
kullanılır.

Token: - loglanmaz, - kullanıcıya gösterilmez, - hata mesajına
yazılmaz, - gelişigüzel localStorage'a konmaz.

TenantId/KullaniciId/CihazId güvenlik kaynağı Desktop değildir; backend
session context otoritedir.

## 5. Uygulama Shell

Login sonrası ortak bir Desktop Shell kullanılmalıdır.

Temel düzen:

``` text
+--------------------------------------------------------------+
| Top Bar: Şube | Kasa | Kullanıcı | Bağlantı                 |
+------+----------+--------------------+---------+--------------+
| Ana  | İşlem    | Sepet / Toplamlar  | Rakamlar| Ürünler      |
| Menü | Butonları|                    |         | Kategoriler   |
+------+----------+--------------------+---------+--------------+
```

### Ana navigasyon

En soldaki ana navigasyon **opsiyonel/daraltılabilir** alandır. Ana
Sayfa, Hızlı Satış, Restaurant, Stok, Cari, Alış, Kasa, Raporlar,
Ayarlar gibi modülleri taşır. Hızlı Satışta yalnız ikon görünümüne
küçülebilir.

### Hızlı Satış işlem butonları

Ana navigasyonun hemen yanındaki işlem kolonu **zorunlu layout
alanıdır** ve Hızlı Satışta sürekli görünür.

Başlangıç örnekleri: - Borç - Beklet - Bekleyen Satışlar - Müşteri Seç -
Fiyat Gör - İskonto - Satış İptal - Diğer İşlemler

İleride kullanıcı/rol yetkisine göre butonlar gösterilebilir,
gizlenebilir veya pasif olabilir. UI buna engel olmayacak component
yapısında olmalı. Buton gizlemek backend yetkilendirmesinin yerine
geçmez.

Sağ ürün/kategori alanının ayrıntıları geliştirme sırasında
netleştirilebilir; mevcut referans yerleşimi korunur, gereksiz erken
detaylandırılmaz.

## 6. Görsel dil

İlk tema açık, modern ve kurumsaldır. Web sitesi/landing page görünümü
verme.

Başlangıç paleti:

``` text
Ana lacivert   #172033
Sidebar        #111827
Primary mavi   #2563EB
Hover          #1D4ED8
Nakit/Başarı   #16A34A
Beklet/Uyarı   #F59E0B
İptal/Hata     #DC2626
Arka plan      #F4F6F8
Panel          #FFFFFF
Border         #E2E8F0
Ana yazı       #172033
İkincil yazı   #64748B
```

Renkler anlam taşımalı; uygulamayı gereksiz rengârenk yapma. Renk,
spacing, radius, font ve shadow değerlerini merkezi design token/theme
üzerinden yönet; componentlerde rastgele hard-code etme.

## 7. Desktop ergonomisi

PanoPOS mouse + klavye + dokunmatik ile kullanılmalıdır. POS butonları
dokunmaya uygun, yönetim ekranları ise kompakt olmalıdır.

Kaçın: - dev boşluklar, - aşırı animasyon, - mobil uygulama gibi dev
kontroller, - her yerde büyük yuvarlak kartlar.

Tercih: - net grid, - yüksek bilgi yoğunluğu, - hızlı kısayollar, -
tutarlı toolbar, - okunaklı font.

## 8. Klavye ve barkod

Klavye kısayolları merkezi yönetilmelidir. Referans olarak F10 Nakit,
F11 Kart, F12 Parçalı kullanılabilir; kesin liste ekran geliştirilirken
netleştirilir.

Barkod input Hızlı Satışın sıcak yoludur. Barkod sonrası gereksiz mouse
tıklaması gerektirme. Modal/ödeme sonrası uygun durumda barkod odağını
geri getir. Input yazarken yanlışlıkla kritik kısayol tetikleme.

## 9. Hızlı Satış local sepet

Yeni sepet Desktop local state'te tutulur. Her barkod, miktar değişimi
veya satır silme backend'e Siparis update'i göndermez.

``` text
Local Sepet -> Beklet -> Siparis
```

veya:

``` text
Local Sepet -> Siparis -> Fatura -> Tahsilat
```

Bekleyen satış backend'den local sepete yüklenir, local düzenlenir ve
son hali toplu update edilir. Chatty satır API davranışı oluşturma.

Restaurant bu local-sepet davranışına zorlanmaz; gerçek zamanlı backend
Siparis/Adisyon yaklaşımını korur.

## 10. State yönetimi

Local UI/sepet state ile server state'i ayır. State yönetimini gereksiz
büyütme. Önce React'in sade imkanlarını değerlendir; gerçek ihtiyaç
yoksa Redux benzeri ağır yapı ekleme. Server data için tek tutarlı
fetching/cache yaklaşımı kullan.

## 11. Fiyat, KDV ve döviz

Desktop normal satış fiyatını uydurmaz. Backend
`StokKartSatisBirimi + FiyatTipi -> StokKartFiyat` otoritesidir.

`FiyatTipi != OdemeTipi`.

Dövizde mevcut backend `FiyatKur` sözleşmesini kullan; Desktop farklı
kur formülü üretmez. Backend response ticari kayıt için son sözdür.

## 12. Ödeme

Mevcut backend parçalı ödeme, fazla ödeme koruması, concurrency ve
idempotency kurallarına uy.

Her yeni ödeme girişiminde benzersiz `IslemAnahtari` üret. Ağ hatasında
**aynı ödeme tekrar gönderiliyorsa aynı IslemAnahtari** kullanılmalıdır;
yeni ödeme için yeni anahtar oluşturulur.

Ödeme başarılı backend response'u gelmeden satış tamamlandı varsayma ve
local sepeti geri dönülemez şekilde temizleme.

Başlangıç ana ödeme butonları: - Nakit - Kredi Kartı - Parçalı

## 13. Hata yönetimi

Raw exception/stack trace kullanıcıya gösterilmez. API
ProblemDetails/domain hata kodları merkezi olarak kısa, anlaşılır ve
eyleme dönük mesaja çevrilir.

Örnek:

Kötü: `HttpRequestException`

İyi:
`Sunucuya ulaşılamıyor. Ana makinenin açık ve ağ bağlantısının çalıştığını kontrol edin.`

Gerekirse stabil PanoPOS hata kodu destek amacıyla gösterilebilir.
Token, parola, kart bilgisi gibi hassas veri loglanmaz.

## 14. Loading ve kritik işlem koruması

Her API çağrısında tüm ekranı kilitleme; yalnız ilgili alanı loading
yap. Ödeme/kayıt gibi kritik işlem gönderilirken duplicate click'i
engelle. Uzun işlemde kullanıcıya net durum göster.

## 15. Realtime

Genel kural:

-   **HTTP:** CRUD, sorgu, ödeme başlatma, fiş/etiket komutu.
-   **SignalR:** canlı event, durum değişimi, sürekli/anlık veri.

Her şeyi SignalR'a taşıma. Restaurant olayları ve ileride Device Service
realtime verileri SignalR kullanabilir.

## 16. Device Service sınırı

İlk Desktop önizlemesinde Device Service zorunlu değildir.

İleride: - COM/USB terazi - ESC/POS - etiket yazıcı - yazarkasa POS -
üretici SDK/driver cihazları

ayrı `PanoPOS Device Service` üzerinden çalışabilir.

Desktop -\> Local API = ticari işlem. Desktop -\> Device Service =
fiziksel cihaz işlemi.

Device Service her terminalde zorunlu değildir; fiziksel/driver bağımlı
cihaz hangi PC'deyse orada çalışabilir.

## 17. Müşteri ekranı

Müşteri ekranı Device Service değildir; React/Tauri içinde ikinci
pencere/monitör UI'ıdır.

Hedef davranış: - satış yok -\> slayt/reklam, - satış var -\> müşteri
sepeti/toplam, - ödeme -\> teşekkür/sonuç, - süre sonunda -\> slayt.

Bu özellik ilk görevde otomatik implement edilmez.

## 18. Responsive / ekran boyutu

Öncelik Windows masaüstüdür. Tasarım ilk olarak 1440x900 ve yaygın
1920x1080 kullanımını hedeflemelidir. Daha küçük çözünürlüklerde kritik
ödeme/sepet alanları kaybolmamalıdır.

Mobil responsive web sitesi mantığıyla bütün layoutu baştan aşağı
yeniden dizme. Desktop için minimum desteklenen çözünürlük gerçek
kullanım testleriyle kesinleştirilecektir.

## 19. Modal ve popup

Modal yalnız kullanıcı kararını gerçekten gerektiren işlemlerde
kullanılmalıdır. Her CRUD için modal açma. Kritik onaylarda açık eylem
isimleri kullan.

`Evet/Hayır` yerine mümkün olduğunda: - Satışı İptal Et - Vazgeç

gibi anlaşılır butonlar tercih edilir.

## 20. Performans

Barkod, ürün arama, sepet ve ödeme sıcak yoldur.

-   gereksiz re-render oluşturma,
-   binlerce ürün kartını aynı anda render etme,
-   her tuşta gereksiz API çağrısı yapma,
-   büyük görseller kullanma.

Gerekirse liste virtualizasyonu/pagination kullan; fakat ölçüm olmadan
ağır optimizasyon altyapısı ekleme.

## 21. Görseller

Ürün görselleri UI'ı bloklamamalıdır. Görsel yoksa temiz placeholder
kullan. Bozuk URL uygulamayı bozmamalıdır. Görsel optimizasyon/cache
stratejisi gerçek ihtiyaçta ayrıca ele alınır.

## 22. Tauri/native sınırı

Tauri yalnız native Desktop ihtiyacı olduğunda kullanılmalıdır:

-   pencere yönetimi,
-   ikinci ekran,
-   local configuration,
-   güvenli native özellikler,
-   ileride updater/setup entegrasyonu.

Normal ticari iş mantığını Rust/Tauri command içine taşıma.

## 23. Güvenlik

UI gizleme güvenlik değildir. Backend 401/403/404 davranışına saygı
göster. Kullanıcı rolü yüzünden gizlenen butonun API'sinin backend'de de
yetkili olduğu varsayılmamalıdır.

Hassas verileri console.log ile yazma. Production'da debug loglarını
kontrolsüz bırakma.

## 24. Test yaklaşımı

Desktop geliştikçe en az: - saf utility/business olmayan UI helper
testleri, - kritik component davranışları, - API client hata/401
davranışı, - local sepet state davranışı

test edilmelidir.

Backend'in 530 testini Desktop'ta yeniden kopyalama. Desktop testi
UI/client davranışını doğrular.

E2E Desktop testleri gerçek ihtiyaç oluştuğunda ayrıca eklenebilir.

## 25. Kod kalitesi

-   TypeScript strict yaklaşımı tercih et.
-   Gereksiz `any` kullanma.
-   Tek dosyada dev component oluşturma.
-   Magic string/number yayma.
-   Tekrarlanan UI davranışını ortaklaştır.
-   Ancak erken/genel amaçlı aşırı abstraction yapma.
-   Kullanılmayan kod ve sahte placeholder servis bırakma.
-   Yeni bağımlılık eklemeden önce gerçekten gerekli olup olmadığını
    değerlendir.

## 26. Paket bağımlılıkları

Her problemi yeni npm paketiyle çözme. Yeni dependency: - aktif
bakımlı, - Tauri/React ile uyumlu, - gerçekten gerekli

olmalıdır.

UI framework/component kütüphanesi seçimi ilk proje iskeleti görevinde
bilinçli yapılmalı; görev dışında kendiliğinden değiştirilmemelidir.

## 27. Setup / updater

Nihai hedef tek `PanoPOS Setup.exe`dir; Ana Makine ve Terminal rolü
seçilebilir.

Ancak ilk Desktop UI önizlemesinde setup/updater geliştirme. Önce gerçek
UI + API akışını doğrula.

## 28. Gelecek kasa-kasa offline sync

Uzun vadede Ana Makine kapanınca diğer PanoPOS kasalarının satışa devam
etmesi hedeflenmektedir.

Bu özellik henüz uygulanmamıştır. Desktop geliştirmesi sırasında
varsayımsal sync motoru yazma; fakat gereksiz merkezi UI
bağımlılıklarıyla gelecekteki bağımsız kasa çalışmasını da
imkansızlaştırma.

## 29. Dış entegrasyonlar

Yemeksepeti/Getir/Trendyol Yemek/ERP/e-Fatura Device Service'in görevi
değildir. Gelecekte Integration/Adapter katmanında ele alınır. Desktop
görevi sırasında bu entegrasyonları otomatik implement etme.

## 30. İlk Desktop geliştirme sırası

Başlangıç sırası:

1.  React + TypeScript + Tauri iskeleti
2.  API configuration/client
3.  PIN Login
4.  ortak Shell / navigasyon
5.  Hızlı Satış görsel iskeleti
6.  gerçek barkod/fiyat lookup
7.  local sepet
8.  beklet / geri aç
9.  fatura / tahsilat
10. satış sonucu / fiş read model
11. müşteri ekranı önizlemesi

Device Service, yazıcı, terazi, yazarkasa POS, setup/updater ve
kasa-kasa sync daha sonra ele alınır.

## 31. Codex çalışma kuralı

Her Desktop görevinde Codex:

1.  `PROJECT_CONTEXT.md`
2.  `CODEX_RULES.md`
3.  `DESKTOP_RULES.md`

dosyalarını okumalıdır.

Görev kapsamı dışında: - backend'i refactor etme, - migration
oluşturma, - eski Desktop'a dokunma, - Device Service yazma, - yeni
framework ekleme, - tasarımı kendiliğinden değiştirme.

Gerçek backend contract eksikliği bulunursa tahmin ederek workaround
yazma; problemi raporla ve onay bekle.

## 32. Git

Kullanıcı değişikliklerini reset/stash/clean/overwrite etme. Görev
açıkça istemedikçe commit/push yapma.

Commit istenirse yalnız görev dosyalarını stage et ve Türkçe, açıklayıcı
commit mesajı kullan.

## 33. Son prensip

Desktop için öncelik sırası:

**Hız -\> Kullanılabilirlik -\> Stabilite -\> Doğru API kullanımı -\>
Sadelik -\> Görsel kalite -\> Genişletilebilirlik**

PanoPOS gösterişli bir demo değil, gerçek kasada saatlerce kullanılacak
ticari uygulamadır.

UI güzel olmalı; fakat güzellik hiçbir zaman hız, okunabilirlik ve işlem
güvenliğinin önüne geçmemelidir.

## 34. AG Grid Community ve ortak grid kuralları

### 34.1 Ana grid teknolojisi ve doğrulanmış temel

PanoPOS Desktop'ın ana grid altyapısı **AG Grid Community**'dir.
Doğrulanmış mevcut sürüm `ag-grid-community` ve `ag-grid-react` için
**36.2.0**'dır. Güncel Theming API ve ince `PanoDataGrid` wrapper'ı
kullanılır. Desktop testleri başarılıdır; React/Tauri üzerinde gerçek
çalışma doğrulanmıştır. Enterprise bağımlılığı yoktur.

Yalnız ücretsiz Community özellikleri kullanılır. Açık kullanıcı kararı
olmadan Enterprise paketi, modülü veya Enterprise lisansı gerektiren
özellik ekleme. Gerçek ihtiyaç oluşursa Enterprise ayrıca değerlendirilir.

### 34.2 PanoDataGrid ince-wrapper sınırı

Ortak `PanoDataGrid` kullanılmalıdır; kalın bir abstraction değildir.
Sorumluluğu ortak PanoPOS görsel ve teknik temelidir: tema, font, header,
border, temel row/header ölçüleri, selection görünümü, loading, empty
state, Türkçe temel mesajlar, ortak formatterlar ve accessibility tabanı.

AG Grid API'sini yeniden yazma veya gizleme. Feature gridleri gerektiğinde
Community kapsamındaki `columnDefs`, `cellRenderer`, `cellEditor`,
`gridOptions`, selection, keyboard eventleri, event callbackleri,
Grid API ve state API'yi doğrudan kullanabilmelidir.

### 34.3 Ekran bazlı bağımsız özelleştirme

Bütün gridler aynı davranışa zorlanmaz. Ortak olan görsel temel ve teknik
altyapıdır; ekranın business/UI davranışı feature gridinde kalır.
Bir feature gridindeki özelleştirme diğer gridleri etkilememelidir.
Ortak defaults/theme nesnelerini mutate etme; ekran ayarlarını ilgili
grid örneğine uygula ve stilleri ilgili ekran/grid kapsamında tut.

Örnek sorumluluk ayrımı (gelecekteki feature dosyaları zorunlu mevcut
implementasyon anlamına gelmez):

``` text
components/grid/
  PanoDataGrid
  panoGridTheme
  formatters
features/quick-sale/
  QuickSaleGrid
  QuantityEditor
  UnitSelector
features/customers/
  CariListGrid
features/invoices/
  InvoiceListGrid
features/stock/
  StockListGrid
```

### 34.4 Hızlı Satış ve diğer gridlerin sınırı

`QuickSaleGrid` ileride miktar hücresinden numpad açma, miktar artırma/
azaltma, Adet/Koli/Kutu satış birimi seçimi, satır iskontosu, dokunmatik
satır işlemleri, satır silme, özel keyboard davranışı, farklı row height
ve özel renderer/editor kullanabilir. Yalnız Hızlı Satış ihtiyaç duyuyor
diye bunları `PanoDataGrid` genel davranışına ekleme.

Cari, Fatura, Stok, Tahsilat ve diğer liste gridleri bağımsızdır.
`CariListGrid` filtre, sıralama, kolon düzenleme ve çift tıkla kart açma;
`InvoiceListGrid` tarih, durum, ödeme durumu, toplam ve farklı renderer
kullanabilir. Bu davranışlar `QuickSaleGrid` veya diğer gridlere yayılmaz.

### 34.5 Theming API ve Desktop görünümü

Yeni AG Grid Theming API kullanılmalıdır; legacy/deprecated tema
yaklaşımına dönme. Ortak tema merkezi PanoPOS design tokenlarını kullanır.
Modern Desktop/ERP görünümü; kompakt ve okunaklı satırlar, net kolon/satır
ayrımı, düzgün sayı hizalama ve belirgin selection hedeflenir.
Web sitesi table görünümünden kaçın. Ekrana özel ölçü/tema override'ları
diğer gridlerin görünümünü değiştirmemelidir.

### 34.6 Formatter yalnız görüntüleme içindir

Ortak formatterlar para, miktar, decimal ve gerektiğinde tarih gösterimi
içindir. KDV, fiyat, iskonto veya stok hesabı içermezler; backend ticari
sonuçlarını yeniden hesaplamaz veya değiştirmezler.

## 35. Ekran profili ve grid state gelecek hedefi

### 35.1 Paylaşılabilir ekran profilleri

İleride kullanıcıların ekran/grid tercihlerini kaydetmesi ve başka
kullanıcılara aktarabilmesi hedeflenir. Örneğin "Market Kasiyer Standart"
profili kolon sırası/genişliği, görünür veya gizli kolonlar, grid görünümü,
ekran layout tercihleri ve bazı işlem butonlarının görünümü/sırasını
içerebilir. Aynı profil Ahmet, Mehmet ve Ayşe gibi birden fazla kullanıcıya
atanabilir. Kullanıcı, rol, şube ve cihaz/kasa bazlı atama ayrıca
değerlendirilebilir; UI tercihleri backend yetkilendirmesinin yerine geçmez.

### 35.2 Olası profil hiyerarşisi

Gerekirse aşağıdaki override sırası değerlendirilebilir:

``` text
Sistem Varsayılanı -> Firma/Tenant -> Şube -> Rol/Ekran Profili -> Kullanıcı tercihi
```

Bu hiyerarşi yalnız gelecek hedefidir; bugün uygulanmış veya kesinleşmiş
bir profil sistemi değildir.

### 35.3 Henüz implement edilmemiş kapsam

**Ekran profili sistemi henüz implement edilmemiştir.** Bu kararların
dokümana eklenmesi implementasyon talimatı değildir. Açık görev olmadan
EkranProfil entity'si, database tablosu, migration, API, kullanıcı ayarı,
profil editörü, grid state persistence, localStorage persistence veya
profil paylaşma ekranı oluşturma.

Gerçek Hızlı Satış ve diğer ekranlar şekillendikten sonra hangi ayarların
profile dahil olacağı ayrıca belirlenecektir.

### 35.4 Serialize edilebilirlik ve state ayrımı

Yeni grid/ekran geliştirmeleri AG Grid column/grid state gibi görünüm
tercihlerinin ileride serialize edilmesini engellememelidir. Ancak
bugünden persistence framework oluşturma.

UI preference state ile business/local cart state birbirinden ayrıdır:
kolon genişliği bir UI tercihidir; satış miktarı business/local cart
state'tir. Ticari veriyi ekran profiline veya görünüm tercihlerine taşıma.
