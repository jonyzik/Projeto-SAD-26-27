# Arquitetura inicial

O domínio em `ParkWise.Core` contém os nove critérios obrigatórios e as entidades de utilizadores, parques, avaliações, simulações, comparações AHP, pesos e resultados. A entidade de utilizador reserva um campo `PasswordHash`; nenhuma palavra-passe em texto simples é guardada pelo modelo. A autenticação ainda não existe.

`ParkWise.Application` define o contrato de acesso aos dados. `ParkWise.Infrastructure` implementa o contrato com EF Core e SQL Server. O contexto só é registado quando a variável de ambiente da ligação está definida; a abertura da interface não necessita de base de dados.

`ParkWise.App` usa ViewModels para a navegação e templates WPF para associar cada ViewModel à sua View. A dashboard é estática e usa dados fictícios. Os restantes ecrãs são marcadores de posição. Os casos de utilização reais serão adicionados à camada Application antes de ligar a interface à persistência.

As comparações par a par e os pesos têm entidades próprias para permitir a sua persistência futura. Ainda não existem cálculos AHP, regras de validação das pontuações ou esquema de migração. Estes detalhes devem ser definidos e testados na fase de simulações.
