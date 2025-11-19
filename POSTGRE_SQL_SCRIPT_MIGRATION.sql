CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251119014250_InitialCreate') THEN
    CREATE TABLE users (
        "Id" text NOT NULL,
        "Name" character varying(200) NOT NULL,
        "Email" character varying(255) NOT NULL,
        "Phone" character varying(20),
        "Bio" text,
        "Password" text NOT NULL,
        "Gender" text NOT NULL,
        "Birthdate" timestamp with time zone NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_users" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251119014250_InitialCreate') THEN
    CREATE TABLE organizations (
        "Id" text NOT NULL,
        "Name" character varying(256) NOT NULL,
        "Description" text,
        "Email" character varying(255),
        "Phone" character varying(20),
        "Address" text,
        "Color" text,
        "ImageUrl" text,
        "Icon" text,
        "CreatedById" text NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_organizations" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_organizations_users_CreatedById" FOREIGN KEY ("CreatedById") REFERENCES users ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251119014250_InitialCreate') THEN
    CREATE TABLE groups (
        "Id" text NOT NULL,
        "Name" character varying(200) NOT NULL,
        "Description" text,
        "OrganizationId" text NOT NULL,
        "CreatedById" text NOT NULL,
        "Color" character varying(20),
        "Icon" character varying(50),
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_groups" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_groups_organizations_OrganizationId" FOREIGN KEY ("OrganizationId") REFERENCES organizations ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_groups_users_CreatedById" FOREIGN KEY ("CreatedById") REFERENCES users ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251119014250_InitialCreate') THEN
    CREATE TABLE organization_members (
        "Id" text NOT NULL,
        "UserId" text NOT NULL,
        "OrganizationId" text NOT NULL,
        "Role" integer NOT NULL,
        "JoinedAt" timestamp with time zone NOT NULL,
        "InvitedById" text,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_organization_members" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_organization_members_organizations_OrganizationId" FOREIGN KEY ("OrganizationId") REFERENCES organizations ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_organization_members_users_InvitedById" FOREIGN KEY ("InvitedById") REFERENCES users ("Id") ON DELETE SET NULL,
        CONSTRAINT "FK_organization_members_users_UserId" FOREIGN KEY ("UserId") REFERENCES users ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251119014250_InitialCreate') THEN
    CREATE TABLE positions (
        "Id" text NOT NULL,
        "Name" character varying(200) NOT NULL,
        "Description" text,
        "GroupId" text NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_positions" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_positions_groups_GroupId" FOREIGN KEY ("GroupId") REFERENCES groups ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251119014250_InitialCreate') THEN
    CREATE TABLE shifts (
        "Id" text NOT NULL,
        "StartDate" timestamp with time zone NOT NULL,
        "EndDate" timestamp with time zone NOT NULL,
        "Title" character varying(200),
        "Notes" text,
        "Status" integer NOT NULL,
        "GroupId" text NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_shifts" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_shifts_groups_GroupId" FOREIGN KEY ("GroupId") REFERENCES groups ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251119014250_InitialCreate') THEN
    CREATE TABLE shift_positions (
        "Id" text NOT NULL,
        "ShiftId" text NOT NULL,
        "PositionId" text NOT NULL,
        "RequiredCount" integer NOT NULL,
        "VolunteersCount" integer NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_shift_positions" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_shift_positions_positions_PositionId" FOREIGN KEY ("PositionId") REFERENCES positions ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_shift_positions_shifts_ShiftId" FOREIGN KEY ("ShiftId") REFERENCES shifts ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251119014250_InitialCreate') THEN
    CREATE TABLE shift_position_assignment (
        "Id" text NOT NULL,
        "UserId" text NOT NULL,
        "ShiftPositionId" text NOT NULL,
        "Status" integer NOT NULL,
        "Notes" text,
        "AppliedAt" timestamp with time zone NOT NULL,
        "ConfirmedAt" timestamp with time zone,
        "RejectedAt" timestamp with time zone,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_shift_position_assignment" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_shift_position_assignment_shift_positions_ShiftPositionId" FOREIGN KEY ("ShiftPositionId") REFERENCES shift_positions ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_shift_position_assignment_users_UserId" FOREIGN KEY ("UserId") REFERENCES users ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251119014250_InitialCreate') THEN
    CREATE INDEX "IX_groups_CreatedById" ON groups ("CreatedById");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251119014250_InitialCreate') THEN
    CREATE INDEX "IX_groups_OrganizationId" ON groups ("OrganizationId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251119014250_InitialCreate') THEN
    CREATE INDEX "IX_organization_members_InvitedById" ON organization_members ("InvitedById");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251119014250_InitialCreate') THEN
    CREATE INDEX "IX_organization_members_OrganizationId" ON organization_members ("OrganizationId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251119014250_InitialCreate') THEN
    CREATE INDEX "IX_organization_members_OrganizationId_Role" ON organization_members ("OrganizationId", "Role");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251119014250_InitialCreate') THEN
    CREATE INDEX "IX_organization_members_UserId" ON organization_members ("UserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251119014250_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_organization_members_UserId_OrganizationId" ON organization_members ("UserId", "OrganizationId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251119014250_InitialCreate') THEN
    CREATE INDEX "IX_organizations_CreatedById" ON organizations ("CreatedById");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251119014250_InitialCreate') THEN
    CREATE INDEX "IX_positions_GroupId" ON positions ("GroupId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251119014250_InitialCreate') THEN
    CREATE INDEX "IX_shift_position_assignment_ShiftPositionId" ON shift_position_assignment ("ShiftPositionId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251119014250_InitialCreate') THEN
    CREATE INDEX "IX_shift_position_assignment_ShiftPositionId_Status" ON shift_position_assignment ("ShiftPositionId", "Status");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251119014250_InitialCreate') THEN
    CREATE INDEX "IX_shift_position_assignment_UserId" ON shift_position_assignment ("UserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251119014250_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_shift_position_assignment_UserId_ShiftPositionId" ON shift_position_assignment ("UserId", "ShiftPositionId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251119014250_InitialCreate') THEN
    CREATE INDEX "IX_shift_positions_PositionId" ON shift_positions ("PositionId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251119014250_InitialCreate') THEN
    CREATE INDEX "IX_shift_positions_ShiftId" ON shift_positions ("ShiftId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251119014250_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_shift_positions_ShiftId_PositionId" ON shift_positions ("ShiftId", "PositionId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251119014250_InitialCreate') THEN
    CREATE INDEX "IX_shifts_GroupId" ON shifts ("GroupId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251119014250_InitialCreate') THEN
    CREATE INDEX "IX_shifts_GroupId_StartDate" ON shifts ("GroupId", "StartDate");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251119014250_InitialCreate') THEN
    CREATE INDEX "IX_shifts_GroupId_Status" ON shifts ("GroupId", "Status");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251119014250_InitialCreate') THEN
    CREATE INDEX "IX_shifts_StartDate" ON shifts ("StartDate");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251119014250_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_users_Email" ON users ("Email");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251119014250_InitialCreate') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20251119014250_InitialCreate', '9.0.10');
    END IF;
END $EF$;
COMMIT;

