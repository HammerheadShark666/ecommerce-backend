BEGIN TRANSACTION;
DECLARE @var nvarchar(max);
SELECT @var = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ECOMMERCE_Products]') AND [c].[name] = N'Slug');
IF @var IS NOT NULL EXEC(N'ALTER TABLE [ECOMMERCE_Products] DROP CONSTRAINT ' + @var + ';');
UPDATE [ECOMMERCE_Products] SET [Slug] = N'' WHERE [Slug] IS NULL;
ALTER TABLE [ECOMMERCE_Products] ALTER COLUMN [Slug] nvarchar(255) NOT NULL;
ALTER TABLE [ECOMMERCE_Products] ADD DEFAULT N'' FOR [Slug];

CREATE UNIQUE INDEX [IX_ECOMMERCE_Products_Slug] ON [ECOMMERCE_Products] ([Slug]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260801160438_AlterProductMakeSlugRequired', N'10.0.8');

COMMIT;
GO

