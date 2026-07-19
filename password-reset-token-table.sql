BEGIN TRANSACTION;
CREATE TABLE [ECOMMERCE_PasswordResetToken] (
    [Id] uniqueidentifier NOT NULL,
    [UserId] uniqueidentifier NOT NULL,
    [TokenHash] nvarchar(44) NOT NULL,
    [ExpiresAt] datetime2 NOT NULL,
    [Used] bit NOT NULL DEFAULT CAST(0 AS bit),
    [UsedAt] datetime2 NULL,
    [CreatedByIp] nvarchar(45) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_ECOMMERCE_PasswordResetToken] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ECOMMERCE_PasswordResetToken_ECOMMERCE_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [ECOMMERCE_Users] ([Id]) ON DELETE CASCADE
);

CREATE UNIQUE INDEX [IX_ECOMMERCE_PasswordResetToken_TokenHash] ON [ECOMMERCE_PasswordResetToken] ([TokenHash]);

CREATE INDEX [IX_ECOMMERCE_PasswordResetToken_UserId] ON [ECOMMERCE_PasswordResetToken] ([UserId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260718102826_PasswordResetToken', N'10.0.8');

COMMIT;
GO

