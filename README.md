# Origen Cacao

Sistema integral para la compra y venta de cacao en Ecuador: portal público multipágina y editable desde un CMS, más un ERP/CRM para productores, compras, lotes, secado, inventario FIFO, ventas, rentabilidad, caja y configuración de precios.

## Arquitectura

- `backend/src/OrigenCacao.Domain`: entidades y reglas de negocio puras.
- `backend/src/OrigenCacao.Application`: contratos, DTOs y puertos de aplicación.
- `backend/src/OrigenCacao.Infrastructure`: EF Core, PostgreSQL, servicios, autenticación, PDF y motor configurable de precios.
- `backend/src/OrigenCacao.Api`: API REST, JWT, Swagger, CORS y health check.
- `frontend`: Angular 21 LTS + Tailwind CSS 4, con portal público y panel administrativo.
- `backend/database/initial-migration.sql`: migración PostgreSQL idempotente para ejecución manual.

## Arranque local

Requisitos: .NET 8 Runtime/SDK compatible, Node 22 y acceso a PostgreSQL/Supabase.

1. Copia `backend/src/OrigenCacao.Api/appsettings.Local.example.json` como `appsettings.Local.json` y completa la conexión. El archivo local está ignorado por Git.
2. Desde la raíz, aplica la migración:

   ```powershell
   dotnet ef database update --project backend/src/OrigenCacao.Infrastructure/OrigenCacao.Infrastructure.csproj --startup-project backend/src/OrigenCacao.Api/OrigenCacao.Api.csproj
   ```

3. Inicia la API:

   ```powershell
   dotnet run --project backend/src/OrigenCacao.Api/OrigenCacao.Api.csproj --launch-profile http
   ```

4. En otra terminal inicia Angular:

   ```powershell
   Set-Location frontend
   npm start
   ```

Portal: `http://localhost:4200` · Swagger: `http://localhost:5080/swagger` · Health: `http://localhost:5080/health`.

Credenciales iniciales locales: `admin@cacao.local` / `CacaoLocal2026!`. Deben cambiarse antes de publicar.

## Precio automático

Define `ApiNinjas:ApiKey` y `ApiNinjas:RefreshIntervalMinutes` en `appsettings.Local.json`, o usa las variables `ApiNinjas__ApiKey` y `ApiNinjas__RefreshIntervalMinutes`. El `BackgroundService` recibe esa configuración mediante Options, consulta `cocoa`, convierte USD/TM a USD/quintal con `precio / 22.046`, resta el margen configurable y calcula el cacao en baba con el factor configurado.

Para activarlo:

1. El motor intenta API Ninjas primero. Si `cocoa` requiere plan premium, está caído o responde inválidamente, consulta automáticamente Yahoo Finance con `CC=F` y toma `chart.result[0].meta.regularMarketPrice`.
2. Entra a **Admin → Configuración**, desactiva **Precio manual**, define el margen por quintal y guarda.
3. Pulsa **Consultar mercado ahora** para probarlo inmediatamente. Después el servicio se ejecuta con el intervalo configurado; el valor predeterminado es 60 minutos.

Si las dos fuentes externas fallan, se aplica el **Precio manual de respaldo** configurado; si tampoco existe, el sistema conserva el último precio válido. API Ninjas exige además revisar su licencia para uso comercial.

## Marca, contacto y correo

En **Admin → Configuración** se gestiona el nombre que aparece en todo el portal, ciudad, dirección, teléfono, WhatsApp, correo de contacto y URL de inserción de Google Maps. El campo de mapa debe contener la URL `src` que entrega Google Maps en “Insertar un mapa”.

La misma pantalla configura SMTP: host, puerto, correo remitente, contraseña o clave de aplicación, TLS/SSL y el interruptor de envío. La contraseña se guarda en la base, pero la API nunca la devuelve al navegador. Con SMTP habilitado, cada compra y venta terminada muestra un modal para descargar su PDF o enviarlo al correo registrado; si no hay correo, se puede escribir uno opcionalmente. Endpoints: `POST /api/purchases/{id}/email-receipt` y `POST /api/sales/{id}/email-receipt`.

La calculadora pública trabaja en quintales para cacao seco y exclusivamente en libras para cacao en baba: `(precio de baba por quintal / 100) × libras`.

## Módulos operativos

- **Gestión de Sitio Web:** CRUD de `PublicContent` para Hero, Nosotros, Servicios y Contacto. El portal consume `GET /api/public-content`.
- **Compras y lotes:** una compra crea un lote con stock, costo, humedad y trazabilidad; sus comprobantes PDF pueden descargarse o enviarse por correo.
- **Caja:** apertura diaria, gastos/aportes/retiros, movimientos automáticos para compras y ventas en efectivo y conciliación de cierre.
- **Secado:** consume cacao en baba por FIFO, registra rendimiento y pérdida, y crea un lote seco conservando el costo total.
- **Ventas:** descuenta lotes por FIFO, conserva la asignación exacta y calcula costo de venta y utilidad bruta.
- **Inventario:** resumen por variedad/estado y detalle de cada lote disponible, agotado, en proceso o anulado.

El flujo de caja exige una caja abierta para cualquier compra o reversión pagada en efectivo. Las operaciones por transferencia o cheque se registran en compras/ventas, pero no modifican el efectivo físico esperado.

## Seguridad antes de producción

- Cambia `Jwt__Key`, `Admin__Email` y `Admin__Password`.
- Mantén `ConnectionStrings__DefaultConnection` y `ApiNinjas__ApiKey` solo en secretos del hosting.
- Actualiza los orígenes CORS y la URL de API del frontend.
- Rota la contraseña de base de datos que se haya compartido fuera de un gestor de secretos.

## Verificación

```powershell
dotnet build OrigenCacao.sln --no-restore
dotnet test OrigenCacao.sln --no-build --no-restore
Set-Location frontend
npm run build
npm run test -- --watch=false
```

La prueba `LiveWorkflowTests` es opcional: al definir `DATABASE_CONNECTION` valida compra, lote, caja, secado, venta y PDF contra PostgreSQL dentro de una transacción que termina en rollback, por lo que no deja datos de prueba.
