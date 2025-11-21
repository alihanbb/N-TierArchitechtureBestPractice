# Health Check UI Konfigürasyonu

Bu proje Health Check UI ile yapýlandýrýlmýþtýr.

## Eriþim URL'leri

Uygulama çalýþtýktan sonra aþaðýdaki URL'lerden health check'lere eriþebilirsiniz:

### Health Check UI Dashboard
- **Dashboard**: `https://localhost:7000/health-ui`

### Health Check Endpoints
- **Tüm Health Checks**: `https://localhost:7000/health`
- **Hazýr Durumu**: `https://localhost:7000/health/ready`
- **Canlýlýk**: `https://localhost:7000/health/live`

## Health Check Bileþenleri

1. **Self Check**: API'nin temel çalýþma durumu
2. **Redis Cache**: Redis baðlantýsý (Hybrid/Redis modunda)
3. **PostgreSQL Database**: Veritabaný baðlantýsý

## Health Check UI Özellikleri

- ? Gerçek zamanlý health check durumu
- ?? Geçmiþ performans grafikleri
- ?? 30 saniyede bir otomatik güncelleme
- ?? Hata durumlarýnda bildirim
- ?? Mobil uyumlu responsive tasarým

## Konfigürasyon

Health Check UI ayarlarý `appsettings.json` dosyasýnda yapýlandýrýlmýþtýr:

```json
{
  "HealthChecksUI": {
    "HealthChecks": [
      {
        "Name": "API Health",
        "Uri": "/health"
      }
    ],
    "EvaluationTimeInSeconds": 30,
    "MinimumSecondsBetweenFailureNotifications": 60
  }
}
```

## Kullanýlan Paketler

- `AspNetCore.HealthChecks.UI` - Health Check UI
- `AspNetCore.HealthChecks.UI.InMemory.Storage` - In-memory storage
- `AspNetCore.HealthChecks.UI.Client` - UI response writer
- `AspNetCore.HealthChecks.Redis` - Redis health check
- `AspNetCore.HealthChecks.NpgSql` - PostgreSQL health check