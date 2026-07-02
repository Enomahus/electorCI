# Server Infrastructure Persistence

Ouvrir un terminal dans ce dossier

## Création des migrations

dotnet ef migrations add InitialMigration --context ApplicationDbContext --output-dir Migrations

## Application des migrations

dotnet ef database update --context ApplicationDbContext

## Supprimer et annuler dernière migration

dotnet ef migrations remove --context ApplicationDbContext --force
