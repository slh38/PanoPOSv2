# Uygulama Sirasi

## Faz 1

Ilk hedef:
- backend cekirdek iskeleti
- ortak base entity yapisi
- DbContext
- temel moduller icin sade domain modelleri
- migration altyapisi
- ProblemDetails standardi
- test altyapisi

## Faz 2

Ilk uygulanacak is alanlari:
1. kullanici girisi ve PIN mantigi
2. cihaz, kasa, vardiya cekirdegi
3. urun ve barkod yapisi
4. siparis akisi
5. fatura olusturma
6. tahsilat ve finans hareketleri

## Faz 3

Sonraki genisleme:
- restoran masa/adisyon
- cari hareketleri
- rapor ve listeleme endpointleri
- outbox altyapisi
- numara sayac mantigi

## Adim Disiplini

Kurallar:
- her sey kucuk parcali ilerler
- bir adim bitmeden digerine gecilmez
- build ve test gecmeden adim tamamlandi sayilmaz
- gereksiz generic yapi kurulmaz
- gereksiz soyutlama eklenmez
- okunabilirlik ve gelistirilebilirlik korunur
