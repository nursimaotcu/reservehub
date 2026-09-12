# Tasarım kararları

## Tek uygulama

Kimlik, oda ve rezervasyon uçları ayrı dosyalarda; aynı ASP.NET Core uygulamasında çalışır. Bu ölçek için servisler arası ağ iletişimi gerekmiyor. EF Core DbContext bir istek boyunca yaşar.

## Veritabanında çakışma kontrolü

İki istek aynı anda boşluk sorgulayıp ikisi de boş sonuç alabilir. Bu nedenle PostgreSQL GiST exclusion constraint, aynı RoomId için kesişen `tstzrange` aralıklarını reddeder. İptal edilmiş kayıtlar kısıta dahil değildir. `[)` sınırları ardışık rezervasyonlara izin verir. Npgsql'in `23P01` hatası API'de 409'a çevrilir.

Kısıt `PreventOverlap` migration'ında SQL ile tanımlıdır; EF modelinde bir index olarak temsil edilmez. Gelecekte tabloyu değiştiren migration'larda bu kısıt korunmalıdır. Kaynak: [PostgreSQL range constraints](https://www.postgresql.org/docs/current/rangetypes.html#RANGETYPES-CONSTRAINT).

## Yetkilendirme

Genel kayıt yalnızca Member oluşturur. Yönetici bootstrap komutuyla oluşturulur. Rezervasyon okuma ve iptalde kullanıcı ID'si JWT'den alınır; istek gövdesinden kabul edilmez. Başka kullanıcıya ait kayıt için 404 döner. Parola hash'i API yanıtlarında bulunmaz.

## Sınırlar

JWT 15 dakika geçerlidir; anlık oturum iptali yoktur. Rate limiter tek API sürecinin belleğinde çalışır. Hazır bir e-posta hizmeti, Redis veya kuyruk bağımlılığı bulunmaz. Bunları eklemek ayrı tasarım ve test gerektirir.
