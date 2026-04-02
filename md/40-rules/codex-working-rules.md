# Codex Calisma Kurallari

Bu repo icin sonraki promptlarda uygulanacak calisma kurallari:

## Genel

- Her gorev kucuk ve bitirilebilir parcaya bolunur.
- Mevcut adim bitmeden yeni module gecilmez.
- Kod sade, okunabilir ve bakimi kolay olmali.
- Gereksiz generic yapi, gereksiz abstraction ve gereksiz katman eklenmez.

## Backend

- Yazma islemleri EF Core ile ve gerekliyse transaction ile yapilir.
- Listeleme, pagination, grid ve rapor tarafinda Dapper tercih edilir.
- Tum buyuk listelerde pagination zorunludur.
- Soft delete tum sorgularda dikkate alinir.
- ProblemDetails API hata standardi olarak kullanilir.

## Domain

- Belge numarasi teknik PK olarak kullanilmaz.
- Kimlik modeli PIN odakli sade yapida kalir.
- Kullanici cihaza sabitlenmez.
- Aktif vardiya olmadan satis yapilmaz.
- Barkod urune ya da varyanta baglanir; ayni barkod iki farkli anlama dusmez.

## Faz Disi Alanlar

Su konular cekirdek disa tasinir:
- lisanslama
- ERP entegrasyonlari
- e-fatura
- yazarkasa/POS entegrasyonu
- terazi
- recete/uretim detaylari
- gelismis stok fisleri
- merkez sync motoru
- web panel
- mobil uygulama

## Beklenen Calisma Biçimi

Sonraki promptlarda once:
1. mevcut adimin hedefi netlestirilir
2. sadece o adimin kodu yazilir
3. build/test ile dogrulanir
4. sonra bir sonraki prompt beklenir
