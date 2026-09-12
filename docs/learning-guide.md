# Çalışma rehberi

Her adımda önce isteği gönder, sonucu gözlemle, ardından ilgili kodu oku. Bir konuyu tamamlamanın ölçütü onu kendi cümlelerinle açıklayabilmek ve küçük bir değişiklik yapabilmek.

| Sıra | Konu | Kod ve uygulama |
|---|---|---|
| 1 | HTTP ve REST | Oda listesini çağır. 200, 201, 400, 401, 403, 404 ve 409 yanıtlarının hangi durumlarda oluştuğunu incele. |
| 2 | C# ve dependency injection | `Program.cs` ve `AppDb` üzerinden servislerin endpoint'e nasıl ulaştığını takip et. `async`, `await` ve `CancellationToken` görevlerini açıkla. |
| 3 | SQL ve veri ilişkileri | `Data.cs` içindeki üç tabloyu çiz. Bir rezervasyonun neden UserId ve RoomId tuttuğunu, foreign key ve index farkını açıkla. |
| 4 | Kimlik ve yetki | Kayıt ol, giriş yap, token ile istek gönder. `AuthEndpoints.cs` içinde parolanın hash'e dönüşmesini izle. JWT'nin şifrelenmiş parola olmadığını açıkla. |
| 5 | İş kuralları | `BookingEndpoints.cs` içindeki tarih kontrollerini incele. Yanlış başlangıç ve bitiş tarihleriyle istek gönder. |
| 6 | Eşzamanlılık | `PreventOverlap` migration'ını oku. Neden önce SELECT ile boşluğu kontrol etmenin tek başına yeterli olmadığını anlat. |
| 7 | Test | `tests/integration.py` dosyasını çalıştır. İki kullanıcının farklı token'larıyla yapılan erişim kontrolünü takip et. |
| 8 | Docker ve CI | Dockerfile'daki build ve runtime aşamalarını açıkla. CI dosyasında başarısız testin nasıl görünür olacağını incele. |

## İlk katkı alıştırması

Oda listesine minimum kapasite filtresi ekle. Örneğin `GET /api/rooms/?minCapacity=6` yalnızca en az altı kişilik odaları getirsin. Önce 2, 6 ve 10 kişilik üç oda oluştur; filtre sonucunu test et. Sonra API belgesini güncelle.

## Sonraki geliştirmeler

1. Müsaitlik arama: tarih aralığı ve kapasiteye göre boş odalar.
2. Redis: yalnızca oda kataloğunu kısa süre önbelleğe alma; oda eklenince önbelleği temizleme.
3. E-posta bildirimi: rezervasyonla birlikte outbox kaydı yazma, arka plan çalışanıyla gönderme.
4. Hesap yönetimi: doğrulama ve parola sıfırlama akışları.

Bunlar henüz uygulanmış özellikler değildir. Her geliştirmeyi ayrı bir problem ve testle ele almak, teknolojinin neden kullanıldığını öğrenmeyi kolaylaştırır.

## Mülakat provası

- Aynı odayı iki kişi aynı anda alırsa ne oluyor?
- 401 ile 403 arasındaki fark nedir?
- Bir kullanıcı başka bir kullanıcının rezervasyon ID'sini tahmin ederse ne görür?
- Entity Framework hangi SQL'i üretiyor?
- İptal neden fiziksel silme yapmıyor?
- Docker'da API kapanıp yeniden açıldığında veriler neden duruyor?
