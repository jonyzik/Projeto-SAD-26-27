# Base de dados

O modelo EF Core está em `src/ParkWise.Infrastructure/Persistence/ParkWiseDbContext.cs` e aponta para SQL Server. Os nove critérios obrigatórios estão configurados como dados de referência no modelo; só serão inseridos quando uma migração futura for criada e aplicada.

Esta fase não inclui migrações, scripts SQL, criação automática de bases de dados ou dados reais de parques. Configurar a ligação local através de `PARKWISE_SQLSERVER_CONNECTION` quando houver um servidor SQL Server Express disponível.
