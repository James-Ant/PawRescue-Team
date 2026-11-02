-- =====================================================================
-- Paw Rescue — PostgreSQL schema (PostgreSQL 15+)
-- This is the SQL equivalent of the EF Core model in
-- src/PawRescue.Infrastructure/Data/Configurations/EntityConfigurations.cs.
-- The API creates these tables itself on first run; this file is for the
-- capstone paper, manual setup and DBA review.
-- Enums are stored as SMALLINT/INTEGER; meanings are listed in comments.
-- =====================================================================

CREATE EXTENSION IF NOT EXISTS pgcrypto; -- gen_random_uuid()

-- ---------- Roles ----------
CREATE TABLE "Roles" (
    "Id"          INTEGER      PRIMARY KEY,
    "Name"        VARCHAR(30)  NOT NULL,
    "Description" VARCHAR(200)
);
CREATE UNIQUE INDEX "IX_Roles_Name" ON "Roles" ("Name");

INSERT INTO "Roles" ("Id", "Name", "Description") VALUES
  (1, 'User',          'Can report animals and track reports'),
  (2, 'Volunteer',     'Can accept and handle rescue cases'),
  (3, 'Administrator', 'Manages users, reports and the system');

-- ---------- Users ----------
CREATE TABLE "Users" (
    "Id"                     UUID         PRIMARY KEY DEFAULT gen_random_uuid(),
    "FullName"               VARCHAR(120) NOT NULL,
    "Email"                  VARCHAR(200) NOT NULL,
    "PasswordHash"           VARCHAR(100) NOT NULL,               -- BCrypt
    "PhoneNumber"            VARCHAR(20),
    "ProfileImageUrl"        VARCHAR(500),
    "RoleId"                 INTEGER      NOT NULL REFERENCES "Roles"("Id") ON DELETE RESTRICT,
    "IsActive"               BOOLEAN      NOT NULL DEFAULT TRUE,
    "LastLoginAt"            TIMESTAMPTZ,
    "LastLatitude"           DOUBLE PRECISION,
    "LastLongitude"          DOUBLE PRECISION,
    "DeviceToken"            VARCHAR(500),                        -- FCM token
    "PasswordResetTokenHash" VARCHAR(100),
    "PasswordResetExpiresAt" TIMESTAMPTZ,
    "CreatedAt"              TIMESTAMPTZ  NOT NULL DEFAULT now(),
    "UpdatedAt"              TIMESTAMPTZ
);
CREATE UNIQUE INDEX "IX_Users_Email" ON "Users" ("Email");
CREATE INDEX "IX_Users_RoleId_LastLatitude_LastLongitude" ON "Users" ("RoleId", "LastLatitude", "LastLongitude");

-- ---------- Locations ----------
CREATE TABLE "Locations" (
    "Id"        UUID             PRIMARY KEY DEFAULT gen_random_uuid(),
    "Latitude"  DOUBLE PRECISION NOT NULL CONSTRAINT "CK_Locations_Latitude"  CHECK ("Latitude"  BETWEEN -90  AND 90),
    "Longitude" DOUBLE PRECISION NOT NULL CONSTRAINT "CK_Locations_Longitude" CHECK ("Longitude" BETWEEN -180 AND 180),
    "Address"   VARCHAR(300),
    "Barangay"  VARCHAR(100),
    "City"      VARCHAR(100),
    "Province"  VARCHAR(100),
    "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT now(),
    "UpdatedAt" TIMESTAMPTZ
);
CREATE INDEX "IX_Locations_Latitude_Longitude" ON "Locations" ("Latitude", "Longitude");

-- ---------- AnimalReports ----------
-- AnimalType: 0 Dog, 1 Cat, 2 Bird, 3 Other
-- Condition : 0 Starving, 1 Injured, 2 Abandoned, 3 Sick, 4 Lost, 5 Other
-- Severity  : 0 Low, 1 Medium, 2 High, 3 Critical
-- Status    : 0 Reported, 1 UnderReview, 2 RescueAssigned, 3 InProgress, 4 Rescued, 5 Closed
CREATE TABLE "AnimalReports" (
    "Id"            UUID         PRIMARY KEY DEFAULT gen_random_uuid(),
    "Title"         VARCHAR(120) NOT NULL,
    "Description"   VARCHAR(2000) NOT NULL DEFAULT '',
    "AnimalType"    INTEGER      NOT NULL,
    "Condition"     INTEGER      NOT NULL,
    "Severity"      INTEGER      NOT NULL CONSTRAINT "CK_AnimalReports_Severity" CHECK ("Severity" BETWEEN 0 AND 3),
    "Status"        INTEGER      NOT NULL DEFAULT 0 CONSTRAINT "CK_AnimalReports_Status" CHECK ("Status" BETWEEN 0 AND 5),
    "PriorityScore" INTEGER      NOT NULL DEFAULT 0 CONSTRAINT "CK_AnimalReports_Priority" CHECK ("PriorityScore" BETWEEN 0 AND 100),
    "ReporterId"    UUID         NOT NULL REFERENCES "Users"("Id") ON DELETE RESTRICT,
    "LocationId"    UUID         NOT NULL REFERENCES "Locations"("Id") ON DELETE RESTRICT,
    "IsDeleted"     BOOLEAN      NOT NULL DEFAULT FALSE,
    "CreatedAt"     TIMESTAMPTZ  NOT NULL DEFAULT now(),
    "UpdatedAt"     TIMESTAMPTZ
);
CREATE INDEX "IX_AnimalReports_Status_Severity" ON "AnimalReports" ("Status", "Severity");
CREATE INDEX "IX_AnimalReports_CreatedAt"       ON "AnimalReports" ("CreatedAt");
CREATE INDEX "IX_AnimalReports_ReporterId"      ON "AnimalReports" ("ReporterId");
CREATE INDEX "IX_AnimalReports_PriorityScore"   ON "AnimalReports" ("PriorityScore");
CREATE INDEX "IX_AnimalReports_LocationId"      ON "AnimalReports" ("LocationId");

-- ---------- Images ----------
CREATE TABLE "Images" (
    "Id"          UUID         PRIMARY KEY DEFAULT gen_random_uuid(),
    "ReportId"    UUID         NOT NULL REFERENCES "AnimalReports"("Id") ON DELETE CASCADE,
    "Url"         VARCHAR(500) NOT NULL,
    "StorageKey"  VARCHAR(300) NOT NULL,   -- Cloudinary public_id or local path
    "ContentType" VARCHAR(50)  NOT NULL,
    "SizeBytes"   BIGINT       NOT NULL CONSTRAINT "CK_Images_Size" CHECK ("SizeBytes" > 0),
    "IsPrimary"   BOOLEAN      NOT NULL DEFAULT FALSE,
    "CreatedAt"   TIMESTAMPTZ  NOT NULL DEFAULT now(),
    "UpdatedAt"   TIMESTAMPTZ
);
CREATE INDEX "IX_Images_ReportId" ON "Images" ("ReportId");

-- ---------- RescueAssignments ----------
-- Status: 0 Active, 1 Completed, 2 Cancelled
CREATE TABLE "RescueAssignments" (
    "Id"          UUID          PRIMARY KEY DEFAULT gen_random_uuid(),
    "ReportId"    UUID          NOT NULL REFERENCES "AnimalReports"("Id") ON DELETE CASCADE,
    "VolunteerId" UUID          NOT NULL REFERENCES "Users"("Id") ON DELETE RESTRICT,
    "Status"      INTEGER       NOT NULL DEFAULT 0,
    "AssignedAt"  TIMESTAMPTZ   NOT NULL DEFAULT now(),
    "CompletedAt" TIMESTAMPTZ,
    "Notes"       VARCHAR(4000),
    "CreatedAt"   TIMESTAMPTZ   NOT NULL DEFAULT now(),
    "UpdatedAt"   TIMESTAMPTZ
);
-- Only one ACTIVE assignment per report (stops two volunteers accepting at once)
CREATE UNIQUE INDEX "UX_RescueAssignments_ActivePerReport" ON "RescueAssignments" ("ReportId") WHERE "Status" = 0;
CREATE INDEX "IX_RescueAssignments_VolunteerId_Status" ON "RescueAssignments" ("VolunteerId", "Status");

-- ---------- Notifications ----------
-- Type: 0 RescueAccepted, 1 StatusChanged, 2 NewNearbyReport, 3 System
CREATE TABLE "Notifications" (
    "Id"        UUID         PRIMARY KEY DEFAULT gen_random_uuid(),
    "UserId"    UUID         NOT NULL REFERENCES "Users"("Id") ON DELETE CASCADE,
    "ReportId"  UUID         REFERENCES "AnimalReports"("Id") ON DELETE SET NULL,
    "Type"      INTEGER      NOT NULL,
    "Title"     VARCHAR(120) NOT NULL,
    "Message"   VARCHAR(500) NOT NULL,
    "IsRead"    BOOLEAN      NOT NULL DEFAULT FALSE,
    "ReadAt"    TIMESTAMPTZ,
    "CreatedAt" TIMESTAMPTZ  NOT NULL DEFAULT now(),
    "UpdatedAt" TIMESTAMPTZ
);
CREATE INDEX "IX_Notifications_UserId_IsRead_CreatedAt" ON "Notifications" ("UserId", "IsRead", "CreatedAt");

-- ---------- AuditLogs ----------
CREATE TABLE "AuditLogs" (
    "Id"         UUID        PRIMARY KEY DEFAULT gen_random_uuid(),
    "UserId"     UUID        REFERENCES "Users"("Id") ON DELETE SET NULL,
    "Action"     VARCHAR(60) NOT NULL,
    "EntityName" VARCHAR(60) NOT NULL,
    "EntityId"   VARCHAR(60),
    "Details"    JSONB,
    "IpAddress"  VARCHAR(45),
    "CreatedAt"  TIMESTAMPTZ NOT NULL DEFAULT now(),
    "UpdatedAt"  TIMESTAMPTZ
);
CREATE INDEX "IX_AuditLogs_CreatedAt" ON "AuditLogs" ("CreatedAt");
CREATE INDEX "IX_AuditLogs_EntityName_EntityId" ON "AuditLogs" ("EntityName", "EntityId");

-- ---------- RefreshTokens (supports JWT refresh) ----------
CREATE TABLE "RefreshTokens" (
    "Id"                  UUID         PRIMARY KEY DEFAULT gen_random_uuid(),
    "UserId"              UUID         NOT NULL REFERENCES "Users"("Id") ON DELETE CASCADE,
    "TokenHash"           VARCHAR(100) NOT NULL,
    "ExpiresAt"           TIMESTAMPTZ  NOT NULL,
    "RevokedAt"           TIMESTAMPTZ,
    "ReplacedByTokenHash" VARCHAR(100),
    "CreatedAt"           TIMESTAMPTZ  NOT NULL DEFAULT now(),
    "UpdatedAt"           TIMESTAMPTZ
);
CREATE UNIQUE INDEX "IX_RefreshTokens_TokenHash" ON "RefreshTokens" ("TokenHash");
CREATE INDEX "IX_RefreshTokens_UserId" ON "RefreshTokens" ("UserId");


-- =====================================================================
-- Useful queries (for the paper / testing)
-- =====================================================================

-- 1) Nearby open reports within 5 km of a point (Haversine in SQL).
--    The API does the same thing: bounding box first (uses the index), then exact distance.
--    :lat = 14.8136, :lng = 121.0453 (San Jose del Monte, Bulacan)
WITH params AS (SELECT 14.8136::float8 AS lat, 121.0453::float8 AS lng, 5.0::float8 AS radius_km)
SELECT r."Id", r."Title", r."Severity", r."Status", l."Latitude", l."Longitude",
       6371 * 2 * ASIN(SQRT(
           POWER(SIN(RADIANS(l."Latitude" - p.lat) / 2), 2) +
           COS(RADIANS(p.lat)) * COS(RADIANS(l."Latitude")) *
           POWER(SIN(RADIANS(l."Longitude" - p.lng) / 2), 2))) AS distance_km
FROM "AnimalReports" r
JOIN "Locations" l ON l."Id" = r."LocationId"
CROSS JOIN params p
WHERE r."IsDeleted" = FALSE
  AND r."Status" NOT IN (4, 5)
  AND l."Latitude"  BETWEEN p.lat - p.radius_km / 111.32 AND p.lat + p.radius_km / 111.32
  AND l."Longitude" BETWEEN p.lng - p.radius_km / (111.32 * COS(RADIANS(p.lat)))
                        AND p.lng + p.radius_km / (111.32 * COS(RADIANS(p.lat)))
ORDER BY r."PriorityScore" DESC, distance_km
LIMIT 200;

-- 2) Admin dashboard: reports per status
SELECT "Status", COUNT(*) FROM "AnimalReports" WHERE "IsDeleted" = FALSE GROUP BY "Status" ORDER BY "Status";

-- 3) Top volunteers by completed rescues
SELECT u."FullName", COUNT(*) AS rescues
FROM "RescueAssignments" a JOIN "Users" u ON u."Id" = a."VolunteerId"
WHERE a."Status" = 1
GROUP BY u."FullName" ORDER BY rescues DESC LIMIT 10;
