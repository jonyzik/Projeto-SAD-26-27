# Arquitetura inicial

O domínio em `ParkWise.Core` contém os nove critérios obrigatórios e as entidades de utilizadores, parques, avaliações, simulações, comparações AHP, pesos e resultados. A entidade de utilizador reserva um campo `PasswordHash`; nenhuma palavra-passe em texto simples é guardada pelo modelo. A autenticação ainda não existe.

`ParkWise.Application` define o contrato de acesso aos dados. `ParkWise.Infrastructure` implementa o contrato com EF Core e SQL Server. O contexto só é registado quando a variável de ambiente da ligação está definida; a abertura da interface não necessita de base de dados.

`ParkWise.App` usa ViewModels para a navegação e templates WPF para associar cada ViewModel à sua View. A dashboard chama `IDashboardService`, que calcula indicadores, listas recentes e distribuição de preços a partir de `IDashboardDataSource`. A fonte de demonstração é escolhida quando não há connection string; a fonte EF Core é escolhida quando está configurada uma ligação SQL Server. A interface identifica os dados fictícios e não troca silenciosamente para eles se a leitura SQL falhar. Simulações, Histórico e Definições continuam com marcadores de posição.

## Integração de páginas

`IPageRoute`, em `ParkWise.App/Navigation`, define a chave da página, os textos de navegação, o ícone, a fábrica do ViewModel e uma ação opcional executada ao navegar. `PageRegistration` regista as rotas no contentor de dependências. `MainWindowViewModel` constrói a sidebar a partir dessas rotas e apresenta o ViewModel da rota selecionada no `ContentControl`, sem abrir outra janela. Uma rota secundária, como «Nova simulação», pode ficar fora da sidebar e indicar a rota principal que deve permanecer destacada.

Para integrar um módulo, registar o seu ViewModel e uma implementação de `IPageRoute` no projeto App, substituindo a fábrica da página provisória correspondente. Adicionar ao `App.xaml` um `DataTemplate` que associe esse ViewModel à respetiva View. O ViewModel deve depender de serviços da camada Application; a navegação e a MainWindow não precisam de conhecer a lógica de pesquisa, AHP ou simulações. O menu «Parques de Ciência» é a designação em português do módulo Science Parks.

## Pesquisa e comparação preliminar de parques

`IScienceParkCatalog` em `ParkWise.Application/ScienceParks` devolve os parques, os serviços conhecidos e a indicação de demonstração. `DemoScienceParkCatalog`, registado em `App.xaml.cs`, fornece seis exemplos fictícios e é o único registo a substituir pelo serviço real do módulo de parques. Os ViewModels de pesquisa, detalhes e comparação dependem apenas do contrato. Nesta fase, o catálogo de demonstração é independente da fonte de dados da dashboard; os respetivos valores não devem ser interpretados como os mesmos registos.

`ScienceParkSearch` combina nome, cidade, serviços disponíveis e limites inclusivos de preço. Um parque sem preço não corresponde a um filtro de preço. `ScienceParkSelection` mantém os identificadores selecionados ao mudar de página. As rotas de detalhes e comparação são secundárias e mantêm «Parques de Ciência» destacado na sidebar. A comparação apresenta dados descritivos e os nove critérios, sem cálculo de pontuação. Com dois ou mais parques, o botão de simulação navega para o ecrã provisório existente; não cria nem calcula uma simulação.

Um parque é considerado disponível para comparação quando tem preço positivo e avaliações para os nove critérios obrigatórios. O gráfico só aparece com pelo menos dois preços válidos. A criação de simulações continua por implementar; a ação rápida da dashboard navega para a página informativa «Nova simulação».

As comparações par a par e os pesos têm entidades próprias para permitir a sua persistência futura. Ainda não existem cálculos AHP, regras de validação das pontuações ou esquema de migração. Estes detalhes devem ser definidos e testados na fase de simulações.
