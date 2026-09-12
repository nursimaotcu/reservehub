# ReserveHub

Çalışma alanları ve toplantı odaları için geliştirdiğim rezervasyon API'si. Backend temellerini tek bir uygulamada bir araya getiriyorum: HTTP, ilişkisel veri modeli, kullanıcı girişi, yetkilendirme, veri doğrulama ve eşzamanlı istekler.

Örneğin iki kişi aynı odanın 14.00–15.00 aralığını aynı anda ayırtmaya çalıştığında yalnızca bir rezervasyon kaydedilir. Diğer istek `409 Conflict` yanıtı alır. Bu kural PostgreSQL'de de uygulanır.

## Teknolojiler

C# · .NET 10 · ASP.NET Core Minimal API · Entity Framework Core · PostgreSQL · JWT · OpenAPI · Docker Compose · GitHub Actions

API testleri Python standart kütüphanesiyle HTTP üzerinden çalışır; ek Python paketi gerekmez.

## Özellikler

- E-posta ve parola ile kayıt/giriş; parolaların ASP.NET Core PasswordHasher ile saklanması
- 15 dakika geçerli JWT erişim belirteci
- Yöneticiye özel oda ekleme, herkese açık oda listesi
- Kullanıcının kendi rezervasyonlarını listelemesi, görüntülemesi ve iptal etmesi
- Çakışan rezervasyonları engelleyen PostgreSQL exclusion constraint
- Sayfalama, istek doğrulama, giriş uçlarında IP başına istek sınırı
- EF Core migration'ları, JSON logları ve sağlık uçları

## Yerelde çalıştırma

Docker Desktop'ın Linux container desteği açık olmalı. Proje klasöründe PowerShell ile:

```powershell
./scripts/setup.ps1
docker compose up -d --build
```

İlk komut rastgele yerel parolaları `.env` dosyasına oluşturur. Bu dosya Git'e eklenmez. Yönetici e-postası ve parolası bu dosyadaki `ADMIN_EMAIL` ve `ADMIN_PASSWORD` alanlarındadır.

Migration servisi şemayı oluşturur ve ilk yönetici hesabını ekler; ardından API başlar.

- API: `http://localhost:5080`
- OpenAPI belgesi: `http://localhost:5080/openapi/v1.json` (Postman'a aktarılabilir)
- Veritabanı sağlık kontrolü: `http://localhost:5080/health/ready`

```powershell
./scripts/test.ps1
docker compose down
```

Test komutu Python 3 gerektirir ve örnek kullanıcı, oda ve rezervasyon kayıtları oluşturur. Testleri kişisel/veri içeren bir veritabanında çalıştırmayın. `docker compose down` yerel veritabanı volume'unu korur.

## API akışı

| İşlem | Uç | Erişim |
|---|---|---|
| Kayıt | `POST /api/auth/register` | Herkes |
| Giriş | `POST /api/auth/login` | Herkes |
| Hesabım | `GET /api/auth/me` | Giriş yapmış kullanıcı |
| Odalar | `GET /api/rooms/?page=1&pageSize=20` | Herkes |
| Oda ekleme | `POST /api/rooms/` | Yönetici |
| Rezervasyon | `POST /api/bookings/` | Giriş yapmış kullanıcı |
| Rezervasyonlarım | `GET /api/bookings/` | Giriş yapmış kullanıcı |
| Rezervasyon ayrıntısı | `GET /api/bookings/{id}` | Kaydın sahibi |
| İptal | `DELETE /api/bookings/{id}` | Kaydın sahibi |

Giriş yanıtındaki `accessToken`, korumalı isteklerde `Authorization: Bearer <token>` başlığına eklenir. Örnek gövdeler [requests.http](requests.http) dosyasındadır.

## İş kuralları

Rezervasyon başlangıcı gelecekte ve en fazla 90 gün ileride olmalı; süre en fazla 8 saattir. Tarihler saat dilimi bilgisi içermeli, örneğin `2026-10-01T14:00:00+03:00`. Veritabanında UTC kullanılır. Bitiş anı aralığa dahil değildir: 14.00–15.00 ve 15.00–16.00 çakışmaz. İptal kaydı silmez, saati yeniden kullanılabilir yapar.

## Proje yapısı

`src/ReserveHub.Api` API, veri modeli ve migration'ları; `tests` gerçek HTTP entegrasyon testlerini; `docs` mimari kararları ve çalışma rehberini içerir. Tek uygulama içinde endpoint gruplarıyla ilerliyorum.

## Mevcut kapsam

Bu sürüm yerel demo ve öğrenme amaçlıdır. E-posta doğrulama, parola sıfırlama, refresh token, ödeme, çoklu işletme ve arayüz içermez. İnternete açılacak kurulumda HTTPS ve dağıtık istek sınırlama ayrıca ele alınmalıdır. Redis ve mesaj kuyruğu henüz kullanılmıyor.

Release derlemesi ve gerçek PostgreSQL üzerindeki HTTP testleri geçti. Docker çalıştırması ve GitHub Actions sonucu bu ortamda doğrulanmadı; ayrıntılar [doğrulama notlarında](docs/validation.md).

Öğrenme sırası için [çalışma rehberini](docs/learning-guide.md), tasarım gerekçeleri için [mimari notlarını](docs/architecture.md) kullanabilirsiniz.
