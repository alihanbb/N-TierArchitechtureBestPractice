# Rate Limiting Implementation

## Overview
Comprehensive rate limiting has been added to the application using ASP.NET Core's built-in rate limiting middleware (.NET 7+).

## Rate Limiting Policies

### 1. **Fixed Window** (`"fixed"`)
- **Limit**: 100 requests per minute
- **Queue**: 10 requests
- **Applied to**: ProductsController (class level)
- **Best for**: General API protection

### 2. **Sliding Window** (`"sliding"`)
- **Limit**: 50 requests per minute
- **Segments**: 6 per window
- **Queue**: 5 requests
- **Applied to**: GET /api/products (list all products)
- **Best for**: More flexible rate limiting

### 3. **Token Bucket** (`"token"`)
- **Token Limit**: 100 tokens
- **Replenishment**: 50 tokens per minute
- **Queue**: 10 requests
- **Applied to**: POST /api/products/create
- **Best for**: Handling burst traffic while maintaining average rate

### 4. **Concurrency Limiter** (`"concurrency"`)
- **Concurrent Requests**: Maximum 50 simultaneous
- **Queue**: 20 requests
- **Applied to**: PATCH /api/products/stock
- **Best for**: Resource-intensive operations

### 5. **Per-IP Policy** (`"perIp"`)
- **Limit**: 30 requests per minute per IP
- **Queue**: 5 requests
- **Applied to**: CategoriesController
- **Best for**: Preventing abuse from specific IPs

## Response When Rate Limit Exceeded

**HTTP Status Code:** `429 Too Many Requests`

**Response Body:**
```json
{
  "error": "Too many requests. Please try again later.",
  "retryAfter": 60
}
```

## Logging
All rate limit violations are logged with:
- Client IP address
- Requested endpoint path
- Log level: Warning

Example log:
```
[Warning] Rate limit exceeded! IP: 192.168.1.100, Path: /api/v1/products
```

## Configuration in Code

### Program.cs
```csharp
builder.Services.AddRateLimiter(options =>
{
    // Various policies configured here
    options.OnRejected = async (context, cancellationToken) =>
    {
        // Custom rejection handling
    };
});

// Middleware placement (must be before MapControllers)
app.UseRateLimiter();
```

### Controller Usage
```csharp
[EnableRateLimiting("fixed")]
public class ProductsController : CustomBaseController
{
    [HttpGet]
    [EnableRateLimiting("sliding")] // Override class-level policy
    public async Task<IActionResult> GetAll()
    {
        //...
    }
}
```

## Testing Rate Limiting

### Using curl:
```bash
# Test fixed window (100 req/min)
for i in {1..110}; do curl http://localhost:5000/api/v1/products; done

# Test per-IP (30 req/min)
for i in {1..35}; do curl http://localhost:5000/api/v1/categories; done
```

### Using PowerShell:
```powershell
# Test rate limit
1..110 | ForEach-Object { 
    Invoke-RestMethod -Uri "http://localhost:5000/api/v1/products" 
}
```

## Best Practices Implemented

✅ **Multiple Policy Types**: Different policies for different scenarios  
✅ **Queuing**: Requests can wait in queue instead of immediate rejection  
✅ **Custom Rejection Handler**: Informative error messages  
✅ **Logging**: All violations are logged for monitoring  
✅ **IP-based Limiting**: Prevents abuse from specific sources  
✅ **Granular Control**: Policy can be applied at controller or action level

## Monitoring & Alerts

Rate limit violations are logged using Serilog and sent to:
- Console
- File (`Logs/log-*.txt`)
- Seq (`http://localhost:5341`)

Set up alerts in Seq to monitor excessive rate limit violations.

## Performance Impact

Rate limiting middleware has minimal performance overhead:
- In-memory tracking (no database calls)
- Efficient algorithm implementations
- Async processing

## Future Enhancements

Consider adding:
- [ ] Distributed rate limiting (Redis) for multi-instance deployments
- [ ] Per-user rate limiting (authenticated users)
- [ ] Dynamic rate limits based on user tiers (free vs premium)
- [ ] Rate limit headers in responses (X-RateLimit-Remaining, etc.)
- [ ] Dashboard for rate limit monitoring
