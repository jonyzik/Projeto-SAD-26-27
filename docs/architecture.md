# Arquitetura inicial

O domínio em `ParkWise.Core` contém os nove critérios obrigatórios e as entidades de utilizadores, parques, avaliações, simulações, comparações AHP, pesos e resultados. A entidade de utilizador reserva um campo `PasswordHash`; nenhuma palavra-passe em texto simples é guardada pelo modelo. A autenticação ainda não existe.

`ParkWise.Application` define o contrato de acesso aos dados. `ParkWise.Infrastructure` implementa o contrato com EF Core e SQL Server. O contexto só é registado quando a variável de ambiente da ligação está definida; a abertura da interface não necessita de base de dados.

`ParkWise.App` usa ViewModels para a navegação e templates WPF para associar cada ViewModel à sua View. A dashboard chama `IDashboardService`, que calcula indicadores, listas recentes e distribuição de preços a partir de `IDashboardDataSource`. A fonte de demonstração é escolhida quando não há connection string; a fonte EF Core é escolhida quando está configurada uma ligação SQL Server. A interface identifica os dados fictícios e não troca silenciosamente para eles se a leitura SQL falhar. Simulações, Histórico e Definições continuam com marcadores de posição.

## Integração de páginas

`IPageRoute`, em `ParkWise.App/Navigation`, define a chave da página, os textos de navegação, o ícone, a fábrica do ViewModel e uma ação opcional executada ao navegar. `PageRegistration` regista as rotas no contentor de dependências. `MainWindowViewModel` constrói a sidebar a partir dessas rotas e apresenta o ViewModel da rota selecionada no `ContentControl`, sem abrir outra janela. Uma rota secundária, como «Configurar critérios AHP», pode ficar fora da sidebar e indicar a rota principal que deve permanecer destacada.

Para integrar um módulo, registar o seu ViewModel e uma implementação de `IPageRoute` no projeto App, substituindo a fábrica da página provisória correspondente. Adicionar ao `App.xaml` um `DataTemplate` que associe esse ViewModel à respetiva View. O ViewModel deve depender de serviços da camada Application; a navegação e a MainWindow não precisam de conhecer a lógica de pesquisa, AHP ou simulações. O menu «Parques de Ciência» é a designação em português do módulo Science Parks.

## Pesquisa e comparação preliminar de parques

`IScienceParkCatalog` em `ParkWise.Application/ScienceParks` devolve os parques, os serviços conhecidos e a indicação de demonstração. `DemoScienceParkCatalog`, registado em `App.xaml.cs`, fornece seis exemplos fictícios. Os ViewModels de pesquisa, detalhes e comparação dependem apenas do contrato. Sem ligação SQL, `DemoCatalogDashboardDataSource` adapta esse mesmo catálogo aos indicadores da dashboard; não acrescenta simulações fictícias. Quando Francisco fornecer o serviço real, será necessário substituir o registo de `IScienceParkCatalog` e acordar a fonte da dashboard. Com ligação SQL, a dashboard usa a fonte EF Core, enquanto as páginas de parques continuam a identificar claramente o seu catálogo fictício até à integração.

`ScienceParkSearch` combina nome, cidade, serviços disponíveis e limites inclusivos de preço. Um parque sem preço não corresponde a um filtro de preço. `ScienceParkSelection` mantém os identificadores selecionados ao mudar de página. As rotas de detalhes e comparação são secundárias e mantêm «Parques de Ciência» destacado na sidebar. A comparação apresenta dados descritivos e os nove critérios, sem cálculo de pontuação. Com dois ou mais parques, o botão de simulação navega para a configuração visual dos critérios AHP; não cria nem calcula uma simulação.

## Configuração visual dos critérios AHP

`AhpComparisonSession`, em `ParkWise.Application/AhpConfiguration`, recebe `EvaluationCriteria.Required` do projeto Core e gera os 36 pares sem manter outro catálogo de critérios. Conserva escolhas parciais ou concluídas ao navegar e só exporta `AhpPairwiseResponse` quando as 36 estão completas. Cada resposta contém os dois `CriterionKind`, a direção (`Equal`, `Left` ou `Right`) e a intensidade de Saaty. `Right` representa a comparação recíproca; a conversão para matrizes, pesos e consistência pertence ao futuro motor AHP.

`AhpCriteriaConfigurationViewModel` mantém a sessão em memória, apresenta um par de cada vez e disponibiliza as respostas concluídas por `GetCompletedResponses()`. A revisão mostra as escolhas e `AhpResultPresentation` reserva estados para pesos, consistência e erros fornecidos futuramente pelo motor. Fechar a aplicação descarta as escolhas. Não existe persistência nem execução de simulações nesta interface.

Um parque é considerado disponível para comparação quando tem preço positivo e avaliações para os nove critérios obrigatórios. O gráfico só aparece com pelo menos dois preços válidos. A criação de simulações continua por implementar.

As comparações par a par e os pesos têm entidades próprias para permitir a sua persistência futura. Ainda não existem cálculos AHP, regras de validação das pontuações ou esquema de migração. Estes detalhes devem ser definidos e testados na fase de simulações.

## Fronteiras de integração da interface

- **Francisco — parques:** `IScienceParkCatalog` e `ScienceParkCatalogData` são o ponto de substituição dos dados fictícios. `ScienceParkSearch.Filter` é uma filtragem local provisória da interface; a pesquisa real, paginação e acesso à base de dados devem ser acordados com Francisco antes de alterar esse contrato. Os identificadores `Guid` usados na seleção e nos detalhes têm de ser estáveis entre consultas.
- **Melão — AHP:** `AhpPairwiseResponse` contém apenas escolhas do utilizador; não representa uma matriz ou pesos. O motor AHP deverá receber as respostas completas e devolver pesos, estado de consistência e erros para `AhpResultPresentation`. A definição dos nove critérios continua única em `EvaluationCriteria.Required`.
- **Guilherme — simulações:** a chave de rota `PageKeys.NewSimulation` é mantida para compatibilidade com a navegação atual, mas apresenta apenas a configuração visual dos critérios. O arranque, a execução, os custos e a persistência de uma simulação precisam de um contrato próprio do módulo de simulações.
- **Rodrigo e Pinto:** as rotas de definições e histórico são apenas marcadores de posição. Estado de sessão/permissões, rankings, gráficos analíticos e exportação só serão ligados quando os respetivos contratos estiverem disponíveis.

`PageRegistration` e `App.xaml.cs` são os pontos de composição da interface. A fonte de demonstração da dashboard substitui o registo padrão de `AddParkWiseInfrastructure` apenas na ausência de ligação SQL; a fonte EF Core mantém-se quando a ligação está configurada. Ao integrar dados reais, rever estes registos em conjunto para evitar uma dashboard real ao lado de páginas de parques fictícios.
