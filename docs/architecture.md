# Arquitetura inicial

O domínio em `ParkWise.Core` contém os nove critérios obrigatórios e as entidades de utilizadores, parques, avaliações, simulações, comparações AHP, pesos e resultados. A entidade de utilizador reserva um campo `PasswordHash`; nenhuma palavra-passe em texto simples é guardada pelo modelo. A autenticação ainda não existe.

`ParkWise.Application` define o contrato de acesso aos dados. `ParkWise.Infrastructure` implementa o contrato com EF Core e SQL Server. O contexto só é registado quando a variável de ambiente da ligação está definida; a abertura da interface não necessita de base de dados.

`ParkWise.App` usa ViewModels para a navegação e templates WPF para associar cada ViewModel à sua View. A dashboard chama `IDashboardService`, que calcula indicadores, listas recentes e distribuição de preços a partir de `IDashboardDataSource`. A fonte de demonstração é escolhida quando não há connection string; a fonte EF Core é escolhida quando está configurada uma ligação SQL Server. A interface identifica os dados fictícios e não troca silenciosamente para eles se a leitura SQL falhar. Os restantes ecrãs são marcadores de posição.

Um parque é considerado disponível para comparação quando tem preço positivo e avaliações para os nove critérios obrigatórios. O gráfico só aparece com pelo menos dois preços válidos. A criação de simulações continua por implementar; a ação rápida da dashboard navega para a página informativa «Nova simulação».

As comparações par a par e os pesos têm entidades próprias para permitir a sua persistência futura. Ainda não existem cálculos AHP, regras de validação das pontuações ou esquema de migração. Estes detalhes devem ser definidos e testados na fase de simulações.
