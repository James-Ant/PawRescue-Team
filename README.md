# 🐾 Paw Rescue

A mobile app for reporting stray, injured, sick, lost or abandoned animals. Users snap a photo, the app captures GPS, and the report is pinned on a map so volunteers can accept and track the rescue.

BS Computer Science Capstone — **.NET MAUI (MVVM) + ASP.NET Core Web API + PostgreSQL + EF Core + JWT**.

---

## 👥 Team tasks

Four features are intentionally left for team members to build: **forgot password, notifications screen, map filters + clustering, and admin user management**. See **[CONTRIBUTING.md](CONTRIBUTING.md)** for assignments, step-by-step hints and the Git workflow. Search the code for `TODO(MEMBER 1)` … `TODO(MEMBER 4)`.

---

## 1. Solution layout (Clean Architecture)

```
PawRescue.sln
├── database/schema.sql                 PostgreSQL DDL + sample queries (nearby search, stats)
├── src/
│   ├── PawRescue.Domain/               Entities, enums, business rules (no dependencies)
│   │   ├── Common/BaseEntity.cs         abstract base: Id, CreatedAt, UpdatedAt
│   │   └── Entities/                    User, Role, AnimalReport, Location, ReportImage,
│   │                                    RescueAssignment, Notification, AuditLog, RefreshToken
│   ├── PawRescue.Application/          Use cases (depends only on Domain)
│   │   ├── Interfaces/                  repository + service abstractions
│   │   ├── Services/                    AuthService, ReportService, RescueService, NotificationService,
│   │   │                                UserService, AdminService, AuditLogger, SeverityPriorityStrategy
│   │   ├── Notifications/               NotificationTemplate hierarchy (polymorphism)
│   │   └── DTOs/, Common/               request/response records, exceptions, GeoCalculator
│   ├── PawRescue.Infrastructure/       EF Core, PostgreSQL, JWT, BCrypt, Cloudinary, Firebase
│   │   ├── Data/                        AppDbContext (also the Unit of Work), configurations, seeder
│   │   ├── Repositories/                generic Repository<T> + one repository per aggregate
│   │   ├── Security/ Storage/ Messaging/
│   │   └── DependencyInjection.cs
│   ├── PawRescue.Api/                  Thin REST controllers, JWT setup, error middleware, Swagger
│   └── PawRescue.Mobile/               .NET MAUI app (MVVM with CommunityToolkit.Mvvm)
│       ├── Views/                       11 screens + Forgot Password (XAML)
│       ├── ViewModels/                  one ViewModel per screen, BaseViewModel
│       ├── Services/                    ApiClient, AuthHeaderHandler (auto token refresh),
│       │                                typed API services, GPS, camera, marker clustering
│       └── Resources/Styles/            Colors.xaml (design tokens), Styles.xaml, Templates.xaml
└── tests/PawRescue.Tests/              xUnit tests for domain rules, priority, geo, notifications
```

Dependency direction: `Api → Infrastructure → Application → Domain`. The Domain knows nothing about EF Core, HTTP or PostgreSQL.

---

## 2. Running it

### Requirements
- .NET 10 SDK, with the MAUI workload: `dotnet workload install maui-android`
- PostgreSQL 15+ (or Docker)
- Visual Studio 2022/2026 or VS Code with the C# Dev Kit + .NET MAUI extension
- Android emulator (or a phone on the same Wi-Fi)

### Database
```bash
docker run --name pawrescue-db -e POSTGRES_PASSWORD=postgres -p 5432:5432 -d postgres:16
```
The API creates the tables automatically on first run. For real migrations (recommended before defense/deploy):
```bash
dotnet tool install --global dotnet-ef
dotnet ef migrations add InitialCreate -p src/PawRescue.Infrastructure -s src/PawRescue.Api
```
After that, the API applies migrations on start-up instead of `EnsureCreated`.

### API
```bash
cd src/PawRescue.Api
dotnet user-secrets set "Jwt:SigningKey" "a-long-random-secret-of-at-least-32-characters"
dotnet run
```
Open **http://localhost:5080/swagger**. A seeded admin is created: `admin@pawrescue.local` / `Admin@12345` (change it in `appsettings.json` → `Seed`).

Password-reset codes are printed in the API console (email sending is a dev stub) once Member 1 finishes that feature.

### Mobile app
1. Put a Google Maps API key in `src/PawRescue.Mobile/Platforms/Android/AndroidManifest.xml` (`YOUR_GOOGLE_MAPS_API_KEY`).
2. Start the API, then run `PawRescue.Mobile` on the Android emulator. It calls `http://10.0.2.2:5080` (the host PC).
3. On a real phone, change `ApiConfig.BaseUrl` in `Services/Infrastructure.cs` to your PC's LAN IP and set `Storage:PublicBaseUrl` in the API to the same IP so photos load.

### Cloud image storage
Local disk (`wwwroot/uploads`) is used by default. To use Cloudinary (free tier), set `Storage:Provider = "Cloudinary"` and fill `Storage:Cloudinary` keys. No code changes — DI swaps the implementation.

### Tests
```bash
dotnet test tests/PawRescue.Tests
```

---

## 3. API endpoints

| Area | Method & route | Who |
|---|---|---|
| Auth | `POST /api/auth/register` · `login` · `refresh` · `logout` · `forgot-password` · `reset-password` | anyone |
| Reports | `POST /api/reports` (multipart: fields + `Images`) | logged in |
| | `GET /api/reports/{id}` · `PUT /api/reports/{id}` · `DELETE /api/reports/{id}` | owner / admin for edit & delete |
| | `GET /api/reports/mine` | logged in |
| | `POST /api/reports/{id}/ai-analysis` → **501** (future feature) | logged in |
| Map | `GET /api/map/nearby?latitude=&longitude=&radiusKm=&minSeverity=&status=&openOnly=` | logged in |
| | `GET /api/map/search?keyword=&animalType=&severity=&status=&page=` | logged in |
| Rescues | `GET /api/rescues/available` · `POST /api/rescues/{reportId}/accept` | Volunteer, Admin |
| | `PATCH /api/rescues/{reportId}/status` `{ status, note }` · `GET /api/rescues/mine` | Volunteer, Admin |
| Notifications | `GET /api/notifications` · `GET unread-count` · `PATCH {id}/read` · `PATCH read-all` | logged in |
| Profile | `GET/PUT /api/users/me` · `PUT /api/users/me/location` | logged in |
| Admin | `GET /api/admin/stats` · `users` · `reports` · `activity` · `PATCH users/{id}/role\|activate\|deactivate` | Administrator |

Errors always come back as `application/problem+json` (`{ status, title, detail }`).

**JWT flow:** login → short-lived access token (30 min) + refresh token (14 days, stored *hashed*, rotated on every use, reuse revokes all sessions). The app's `AuthHeaderHandler` attaches the token and silently refreshes on a 401.

**Report status workflow** (enforced in `AnimalReport.ChangeStatus`):
`Reported → UnderReview → RescueAssigned → InProgress → Rescued → Closed` (Closed reachable from any state; a volunteer can drop a case `RescueAssigned → UnderReview`). A partial unique index guarantees only one active rescuer per report.

---

## 4. Where each OOP / design principle lives

| Principle | Example in the code |
|---|---|
| **Encapsulation** | `AnimalReport` has private setters; status only changes through `ChangeStatus()` which validates transitions. `TokenStore` (mobile) is the only class touching SecureStorage. |
| **Inheritance** | `BaseEntity` → all entities. `Repository<T>` → `UserRepository`, `ReportRepository`… `BaseViewModel` → every ViewModel. `BasePage` → every page. `AppException` → `NotFoundException`, `ForbiddenException`… |
| **Polymorphism** | `NotificationTemplate` subclasses override `Title`/`BuildMessage()`; `NotificationService.NotifyAsync` accepts any of them. `IImageStorageService` → Local vs Cloudinary. Middleware reads the overridden `AppException.StatusCode`. `OneWayConverter` subclasses. |
| **Abstraction** | Application depends on `IReportRepository`, `ITokenService`, `IPushNotificationSender`, never on EF Core / Firebase directly. Mobile ViewModels depend on `IReportApi`, `ILocationService`. |
| **S**ingle responsibility | One service per feature (Auth, Report, Rescue…); controllers only translate HTTP. |
| **O**pen/closed | New notification type = new subclass; new priority logic = new `IPriorityStrategy`. |
| **L**iskov | Any `IImageStorageService` / `IPriorityStrategy` can replace another without breaking callers. |
| **I**nterface segregation | Small interfaces: `IAuthService`, `IRescueService`, `IAuthApi`, `IReportApi`… |
| **D**ependency inversion / DI | `AddApplication()`, `AddInfrastructure()`, `MauiProgram` register everything; constructors receive interfaces. |
| Repository + Unit of Work | `Infrastructure/Repositories`, `AppDbContext : IUnitOfWork` |
| Service layer | `Application/Services` |
| Strategy | `IPriorityStrategy` / `SeverityPriorityStrategy` |

---

## 5. Why PostgreSQL

| | PostgreSQL | SQL Server | MySQL | Firebase Firestore |
|---|---|---|---|---|
| Cost | Free, open source | Express is limited (10 GB) | Free | Free tier, then per-read pricing |
| Geo queries | Excellent (**PostGIS** upgrade path) | Good (geography type) | Basic | Very limited (no radius query) |
| EF Core support | Mature (Npgsql) | Native | Good (Pomelo) | None |
| JSON | `jsonb` (used for AuditLogs) | JSON functions | JSON | Native |
| Hosting | Neon, Supabase, Render, Azure, AWS | Azure | Most hosts | Google Cloud only |

PostgreSQL stays the recommendation. The capstone uses plain lat/lng columns + a bounding-box index + Haversine (no extension needed). If the dataset grows, switch to PostGIS (`geography(Point)` + GiST index + `ST_DWithin`) without changing the API.

---

## 6. Design (Figma)

The Figma Make file requires a signed-in Figma account, so it could not be opened while writing this code. The UI follows the requested screen list with a warm orange / teal "rescue" theme, rounded cards, a bottom tab bar (Home · Map · Report · Alerts · Profile) and sticky primary buttons.

**To match the Figma exactly:** all colors are tokens in `Resources/Styles/Colors.xaml`, typography/buttons/cards/inputs are in `Styles.xaml`, and the shared report card is in `Templates.xaml`. Copy the hex values, font sizes and corner radii from Figma's Inspect panel into those files and every screen updates. Custom fonts go in `Resources/Fonts` + `builder.ConfigureFonts(...)` in `MauiProgram.cs`.

Things MAUI can't replicate 1:1 (and what was done instead):
- **Custom-drawn map markers / clustering bubbles** — MAUI Maps only supports standard pins; clustering is done in code (`MarkerClusterer`) and shown as "N reports here" pins.
- **Blur/glass effects** — not supported natively; replaced with solid white cards + soft shadow.
- **Complex Figma animations** — only a simple splash scale animation is implemented.

---

## 7. Unfinished features (intentional, future iterations)

| Feature | Current implementation | What is missing | Technologies needed | Next steps |
|---|---|---|---|---|
| **AI animal health detection** | `IAnimalHealthAnalyzer` interface + `NotImplementedHealthAnalyzer`; endpoint `POST /api/reports/{id}/ai-analysis` returns 501; "AI health check (beta)" button in the app shows "Coming soon". | A trained model and labeled dataset (injured / malnourished / skin disease / healthy). | Azure Custom Vision or ML.NET / ONNX Runtime; Python + PyTorch for training. | Collect & label report photos → train classifier → implement `CustomVisionHealthAnalyzer : IAnimalHealthAnalyzer` → register in DI → store result on the report. |
| **Automatic emergency prioritization** | Rule-based `SeverityPriorityStrategy` (severity + condition + time open) sorts volunteer cases. | Learning from real outcomes; using AI result, weather, time of day. | ML.NET regression, historical rescue data. | Log response times → train model → `AiPriorityStrategy : IPriorityStrategy` → swap in `AddApplication()`. |
| **Push notifications (device side)** | Server is complete: `FcmPushNotificationSender` sends via Firebase when `Firebase:CredentialsPath` is set; in-app notification list works. | The app doesn't yet obtain an FCM device token. | Firebase project, `Plugin.Firebase` / `Plugin.FirebasePushNotifications`, `google-services.json`. | Add plugin → get token on login → send in `LoginRequest.DeviceToken` / `PUT /api/users/me/location`. |
| **Real-time tracking** | Status updates are pulled (pull-to-refresh, on page open). | Live volunteer location + instant status updates. | ASP.NET Core **SignalR**, background location on mobile. | Add `RescueHub` → broadcast status changes → volunteer app streams location while a rescue is InProgress. |
| **Offline synchronization** | None — reports need internet to submit. | Saving drafts offline and syncing later. | SQLite (`sqlite-net-pcl`), `Connectivity` API, background sync queue. | Save draft report + photos locally → retry queue on `ConnectivityChanged` → idempotency key on `POST /api/reports`. |
| **Rescue organization integration** | Volunteers are individual users. | Shelters/NGOs as entities, forwarding cases, capacity. | New `Organizations` table, partner API/webhooks. | Add Organization entity + role → assign reports to orgs → webhook/email to partner. |
| **Analytics heatmap** | Admin stats (counts by status/type, last 7 days); `GET /api/admin/heatmap` returns 501 with a placeholder card in the app. | Geographic density visualization. | PostGIS `ST_SnapToGrid` or server-side grid aggregation; a heatmap layer (Mapsui/OpenStreetMap or web admin with Leaflet). | Aggregate reports per grid cell → endpoint returns cells + counts → render layer. |
| **Recommendation system** | Cases are sorted by priority then distance. | Matching volunteers to cases by skills, history, availability. | Volunteer profile fields, scoring algorithm/ML. | Add volunteer skills/availability → scoring service → "Recommended for you" list. |
| **Email delivery** | `ConsoleEmailSender` prints reset codes to the API log. | Real email. | SMTP via MailKit, or SendGrid/Brevo. | Implement `SmtpEmailSender : IEmailSender`, register in DI. |
| **Image compression** | Photos uploaded as-is (Cloudinary resizes server-side). | Client-side compression to save mobile data. | SkiaSharp. | Resize to ~1600px JPEG 80% in `MediaService` before upload. |

---

## 8. Security notes
- Passwords hashed with BCrypt (work factor 11); refresh & reset tokens stored as SHA-256 hashes.
- Login/register/refresh are rate-limited (10/min per IP).
- Role-based authorization on controllers; ownership checks in services.
- Image type/size validation (JPEG/PNG/WEBP/HEIC, 8 MB, max 5).
- Soft delete for reports; every important action written to `AuditLogs`.
- Dev-only settings to remove for production: `usesCleartextTraffic` (Android), `NSAllowsLocalNetworking` (iOS), the seed admin password, and the placeholder JWT key.

## 9. Deployment (suggested, low-cost)
- **Database:** Neon or Supabase (managed PostgreSQL, free tier).
- **API:** Render / Railway / Azure App Service (Docker or `dotnet publish`), secrets via environment variables (`Jwt__SigningKey`, `ConnectionStrings__Default`, `Storage__Cloudinary__ApiSecret`).
- **Images:** Cloudinary.
- **Mobile:** `dotnet publish -f net10.0-android -c Release` → signed APK/AAB for testers.
