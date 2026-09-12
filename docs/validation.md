# Doğrulama

12 Eylül 2026 tarihinde .NET SDK 10.0.401 ve PostgreSQL 18.6 ile yerel kontrol yapıldı.

- Release derlemesi: 0 hata, 0 uyarı.
- Migration'lar gerçek PostgreSQL veritabanına uygulandı.
- HTTP testleri: kayıt/giriş, yanlış parola, tekrar eden e-posta, bozuk token, yönetici yetkisi, veri doğrulama, sahiplik, sayfalama sınırı, eşzamanlı rezervasyon çakışması, ardışık saatler, tekrar iptal, iptal sonrası yeniden rezervasyon ve OpenAPI geçti.

Testlerde bellek içi veritabanı kullanılmadı. Docker servisi bu ortamda çalışmadığı için API ve PostgreSQL doğrudan başlatıldı. Docker image build ve GitHub Actions henüz çalıştırılmadı; CI dosyasının bulunması başarılı CI sonucu anlamına gelmez.

Tekrarlama: Docker ile uygulamayı başlattıktan sonra Python 3 ile `scripts/test.ps1` çalıştırın. Her test yeni örnek kayıtlar oluşturur.
