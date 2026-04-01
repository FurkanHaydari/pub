# Pubinno Task Portal API 🍺

[![.NET](https://img.shields.io/badge/.NET-10.0-blue.svg)](https://dotnet.microsoft.com/)
[![Architecture](https://img.shields.io/badge/Architecture-Clean%20Architecture%20%2B%20CQRS-success.svg)](https://learn.microsoft.com/en-us/dotnet/architecture/modern-web-apps-azure/common-web-application-architectures#clean-architecture)
[![PostgreSQL](https://img.shields.io/badge/Database-Neon%20Serverless%20PostgreSQL-336791.svg)](https://neon.tech/)
[![Docker](https://img.shields.io/badge/Deployment-Docker-2496ED.svg)](https://www.docker.com/)

Yüksek hacimli IoT sensör verilerini (Akıllı Bira Muslukları) saniyeler içinde kabul eden (ingestion) ve bu verileri gerçek zamanlı analiz (summary) pencerelerinde sunan, **Event-Driven & CQRS** standartlarında kurgulanmış bir .NET Microservice (REST API) uygulamasıdır. Test ve bakım kolaylığı gözetilerek kurumsal ölçekte tasarlanmıştır.

---

## 🏗️ Mimari Tasarım (Architecture)

Proje, geleneksel "Minimalist/Monolith" yapılardan sıyrılarak sektör standartlarında bir **Çok Katmanlı Clean Architecture (Soğan Mimarisi)** konseptine dayanır.

1. **`Pubinno.Api` (Presentation):** Sistemin giriş noktası. Sadece Router, HTTP Context ve Middleware yapılarını yönetir. Hiçbir iş mantığı (business logic) içermez.
2. **`Pubinno.Application` (Business & Domain):** Uygulamanın beyni. MediatR Handler'larını, AutoMapper kurgularını ve FluentValidation kurallarını barındırır. HTTP katmanından ve veritabanı spesifikasyonlarından tamamen izole (decoupled) şekilde yaşar.
3. **`Pubinno.Data` (Infrastructure):** Entity Framework Core Context sınıfını, Foreign-Key bağlantılarını ve DB Seeding (Otomatik başlatma verileri) mantığını barındırır.

### Kullanılan Desenler (Patterns):
- **CQRS (Command Query Responsibility Segregation):** `MediatR` paketi kullanılarak Yazma (CreatePour) ve Okuma (GetSummary) işlemleri sistem içerisinde iki ayrı kola ayrılmış, bağımsız ölçeklenebilirlik sağlanmıştır.
- **Fail-Fast Doğrulama (Validation):** `FluentValidation` ara katmanı sayesinde, geçersiz bir cüzdan, tarih veya ürün bilgisi daha Domain katmanına ulaşmadan anında `400 Bad Request` ile sistemden püskürtülür.

---

## 🧠 Tasarımsal (Mühendislik) Kararlar ve Trade-Off'lar

Performansın ve dayanıklılığın %100 istendiği IoT veri akışlarında bazı bilinçli mühendislik kararları alınmıştır:

### 1. Guid Yerine "Doğal Anahtar" (Natural Keys) Kullanımı
**Sorun:** Geleneksel ilişkisel RDBMS mimarilerinde her satıra sahte bir `Guid` kimliği atanır ve asıl okunaklı veri `Name` sütununda tutulur. Ancak sensörlerden gelen IoT JSON dataları bize sadece `"guinness"` veya `"ipa"` gibi metinsel (Natural Identifier) veriler yollar.
**Karar:** `Products` ve `Locations` gibi ana tablolarda Primary Key (Birincil Anahtar) olarak sahte Guid üretmek yerine, doğrudan bu **Doğal String Veriler (`"guinness"`) PK olarak atanmıştır.**
**Neden:** Yüksek hacimli akış (Test esnasındaki asenkron saniyede yüzlerce istek) durumunda, her `INSERT` işleminden önce "guinness kelimesinin karşılığı olan Guid'i bulmak" için veritabanına atılacak o ağır `SELECT` (Lookup) maliyetinden **komple kurtulunmuş**, O(1) hızında "0-Maliyetli" Insert yapabilen en verimli model (No-Join) inşa edilmiştir.

### 2. Idempotency (Gereksiz Paslanma)
**Sorun:** IoT ağlarında paket kayıpları veya gecikmeler nedeniyle aynı EventID'ye sahip bir sensör kaydı (Retry stratejilerinden dolayı) API'ye birçok kez gelebilir.
**Karar:** Bir döküm verisi (Pour) aynı EventId ile ikinci kez yazılmaya çalışıldığında, sistem (Postgres `23505` constraint violation) hatasını yakalar, bunu kusurlu bir `400` veya `500` olarak patlatmak yerine zarifçe ele alır ve **`200 OK` (İşlem zaten var - No-Op)** döner.
**Neden:** Aynı ID'yi 500 dönerek reddetmek, sensörün bu veriyi sonsuza kadar kuyrukta bekletmesine ve tekrar yollamasına sebep olur. Idempotency kurgusu ile bu zincir kırılmıştır.

### 3. Read (Okuma) Operasyonlarında RAM Tasarrufu
Rapor (Summary) çekilen Query Handler işlemleri sırasında büyük veri tabanlarında Entity Framework'ün arkada iz (Track) sürmemesi için `.AsNoTracking()` özelliği yapılandırılmıştır. Bu, cihaz bazlı istatistik sorgularında (Group By) CPU ve Bellek sarfiyatını dramatik ölçüde düşürür.

---

## 🚀 Başlangıç ve Nasıl Çalıştırılır?

Projeyi lokalinize indirebilir veya Render/Docker konfigürasyonları üzerinden anında servise alabilirsiniz:

### 🐳 Docker İle Çalıştırma
```bash
# Proje kökünden build alma (Kök klasörde Dockerfile mevcut)
docker build -t pubinno-api .

# Ayağa kaldırma (-e ile API_KEY ortam değişkenini yollayarak)
docker run -p 8080:8080 -e API_KEY="gizli-test-sifresi" pubinno-api
```

### 💻 .Net CLI İle Çalıştırma
```bash
cd PubinnoApi && dotnet build
cd Pubinno.Api && dotnet run
```
Uygulama başarıyla derlendiğinde doğrudan yerel makinenizde `http://localhost:8080` portundan hizmet vermeye başlar.

## 📌 Test Edilebilir Uç Noktalar (Endpoints)

Tüm komut işlemleri (Health check hariç) `X-API-Key` Yetki kimliği (Header) gerektirmektedir.

- **`GET /health`**
  - Kestrel sunucusu ve Database limitajlarını kontrol eden açık uç nokta.
- **`POST /v1/pours`**
  - Sensör verilerinin döküldüğü ve Event doğrulamasının yapıldığı (Ingestion) uç nokta.
- **`GET /v1/taps/{deviceId}/summary?from=ISO_DATE&to=ISO_DATE`**
  - Lokasyon, Ürün Hacimleri (Top Products) ve döküm sayılarının detaylı analitik kronogramı.
