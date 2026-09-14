# Doğrulama

## GitHub Actions

[Başarılı koşu · 12 Eylül 2026](https://github.com/nursimaotcu/reservehub/actions/runs/34699822971)

Commit: 8cc6df9885c7457905402bd407dc1e6951e9f20f

Docker image build, PostgreSQL ve API başlangıcı, migration ve gerçek HTTP entegrasyon testleri geçti.

## Yerel kontrol

12 Eylül 2026 · .NET SDK 10.0.401 · PostgreSQL 18.6

- Release derlemesi: 0 hata, 0 uyarı.
- Migration’lar gerçek PostgreSQL veritabanına uygulandı.
- Kayıt/giriş, yanlış parola, tekrar eden e-posta, bozuk token, yönetici yetkisi, veri doğrulama, sahiplik ve sayfalama sınırı kontrolleri geçti.
- Eşzamanlı rezervasyon çakışması, ardışık saatler, tekrar iptal, iptal sonrası yeniden rezervasyon ve OpenAPI kontrolleri geçti.

Bellek içi veritabanı kullanılmadı. Yerelde API ve PostgreSQL doğrudan başlatıldı; Docker çalıştırması daha sonra CI ortamında doğrulandı.

Tekrarlama: Docker ile uygulamayı başlattıktan sonra Python 3 ile scripts/test.ps1. Her test yeni örnek kayıtlar oluşturur; izole demo veritabanı kullanılmalıdır. Yük ve bağımsız güvenlik testleri bu sonuçların kapsamında değildir.
