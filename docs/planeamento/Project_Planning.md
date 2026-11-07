# ParkWise — Project Planning

**Unidade curricular:** Sistemas de Apoio à Decisão · ISPGAYA · 2026/2027

**Responsável pelo planeamento:** João

**Data de referência:** 05/11/2026

**Estado:** proposta de planeamento para revisão pela equipa.

## 1. Objetivo do projeto

Desenvolver uma aplicação desktop Windows que apoie empresas na escolha de parques de ciência e tecnologia. O ParkWise permitirá consultar e comparar parques, definir preferências, realizar simulações através do método AHP, calcular custos operacionais e apresentar os resultados da decisão.

O planeamento organiza o trabalho necessário até à entrega final, identificando tarefas, responsáveis, datas, dependências e critérios de conclusão.

## 2. Âmbito

O desenvolvimento deve cobrir os requisitos funcionais do enunciado:

| Requisito | Funcionalidade | Responsabilidade técnica principal |
| --- | --- | --- |
| RF1 | Autenticação na aplicação | Rodrigo |
| RF2 | Dashboard | João |
| RF3 | Introdução de novos parques | Francisco |
| RF4 | Importação de parques | Francisco |
| RF5 | Pesquisa de parques | Francisco nos dados e serviços; João na interface existente |
| RF6 | Nova simulação: AHP, custos e pontos fortes/fracos | Melão no AHP; Guilherme nas simulações e custos; Pinto nos resultados; João na integração visual |
| RF7 | Exportação dos resultados das simulações | Pinto |

A solução existente utiliza C#, WPF, MVVM, .NET 10, EF Core e SQL Server. A página web é um entregável de divulgação do projeto e tem um trabalho próprio no calendário.

## 3. Equipa e distribuição do trabalho

### Entregáveis com responsáveis definidos pela equipa

| Entregável | Responsáveis | Contributos necessários |
| --- | --- | --- |
| Project planning: Gantt e WBS | João | Cada elemento revê as suas tarefas, datas e dependências. |
| Performance reports | Francisco e Guilherme | Todos fornecem evidências do trabalho realizado, dificuldades e desvios face ao plano. |
| Página web do projeto | Melão, Pinto e Rodrigo | A equipa fornece a descrição do projeto, identificação dos elementos, estado de desenvolvimento e materiais a publicar. |

O planeamento define o que se pretende fazer e quando. Os relatórios de desempenho registam o que foi efetivamente realizado e comparam esse trabalho com o plano. As responsabilidades destes entregáveis acumulam com os módulos técnicos abaixo.

### Módulos técnicos

Mantém-se a distribuição descrita no README:

| Elemento | Módulo principal |
| --- | --- |
| João | Interface WPF, navegação, dashboard e integração visual dos módulos. |
| Francisco | Base de dados, gestão e pesquisa de parques, importação XML e persistência. |
| Melão | Método AHP, pesos, consistência e validação matemática. |
| Guilherme | Simulações, necessidades da empresa e custos operacionais. |
| Rodrigo | Autenticação, sessões e permissões. |
| Pinto | Ranking, gráficos, pontos fortes/fracos e exportação de resultados. |

Os responsáveis pelos restantes documentos no Excel são propostas de coordenação. A equipa deve rever essas atribuições e a disponibilidade de cada elemento.

## 4. Ponto de partida

Segundo a documentação atual do repositório, existe uma base com estrutura por camadas, navegação WPF, dashboard, pesquisa e comparação sobre dados fictícios e configuração visual das comparações AHP. Existem também entidades, DbContext e código de testes.

Esta base ainda não representa uma solução completa: faltam, entre outros, autenticação, gestão e importação real de parques, cálculos AHP, execução e persistência de simulações, custos e exportação. A presença de código de testes não comprova a sua execução nem aprovação.

As datas históricas do Gantt original são referências de planeamento e não representam horas de esforço comprovadas. O progresso das tarefas futuras deve ser atualizado com evidências da equipa.

## 5. Marcos e entregas

| Marco | Data | Entregáveis previstos |
| --- | --- | --- |
| Milestone 1 | **12/11/2026 — provisória** | Planeamento com Gantt e WBS; abertura da página web; relatório de desempenho I. |
| Milestone 2 | **17/12/2026** | Especificação de requisitos; relatório de testes de aceitação; revisão de código I; teste de sobrevivência; relatório de desempenho II. |
| Entrega final | **07/01/2027** | Aplicação e instalação; manual; relatório final; artigo científico; apresentação; revisão de código II; página web atualizada; avaliação/autoavaliação; relatório de desempenho III e suporte de entrega pedido. |

**Data a esclarecer:** a página 16 do enunciado indica «12/11/2023», apesar de o projeto ser de 2026/2027. Este plano utiliza 12/11/2026 como hipótese, a confirmar com o docente. Os formatos, a plataforma e as horas de submissão também devem ser confirmados.

## 6. WBS e calendário geral

A WBS decompõe o projeto em blocos de trabalho. As datas seguintes são propostas e devem ser revistas de acordo com a disponibilidade da equipa.

| WBS | Bloco de trabalho | Período previsto | Responsáveis | Resultado esperado |
| --- | --- | --- | --- | --- |
| 1 | Base existente | Anterior a 05/11/2026 | João, conforme o plano existente | Estrutura e protótipo visual disponíveis para continuação. |
| 2 | Preparação da primeira entrega | 05/11–12/11/2026 | João; Francisco e Guilherme; Melão, Pinto e Rodrigo | Gantt e WBS revistos, página web publicada e desempenho I preparado. |
| 3 | Requisitos e contratos de integração | 13/11–19/11/2026 | Equipa, por módulo | Requisitos, critérios de aceitação, modelo de dados e interfaces entre módulos definidos. |
| 4 | Desenvolvimento e integração | 17/11–10/12/2026 | Todos, nos respetivos módulos | Autenticação, parques, AHP, simulações, custos, resultados e exportação integrados. |
| 5 | Testes e segunda entrega | 07/12–17/12/2026 | Equipa; Francisco e Guilherme no desempenho II | Percurso completo validado, requisitos consolidados e documentação intermédia preparada. |
| 6 | Estabilização e entrega final | 04/12/2026–07/01/2027 | Equipa; responsáveis documentais conforme o Excel | Artigo iniciado em paralelo; aplicação estabilizada, instalação validada e documentação final pronta. |

O ficheiro **ParkWise_Project_Planning.xlsx**, na mesma pasta, contém o detalhe das tarefas, datas, duração em dias úteis, responsáveis, progresso, dependências e critérios de conclusão. Inclui as folhas «Resumo», «Gantt semanal», «Milestone 1», «WBS» e «Parâmetros».

As durações em dias úteis representam intervalos de calendário, não horas de trabalho por pessoa. A disponibilidade semanal não foi fornecida; por isso, este plano não apresenta estimativas de esforço nem garante o equilíbrio da carga individual. O calendário exclui os feriados configurados, mas não assume férias escolares.

## 7. Plano da primeira entrega

| Tarefa | Responsáveis | Início | Fim previsto | Critério de conclusão |
| --- | --- | --- | --- | --- |
| Esclarecer critérios de avaliação e data da primeira entrega | Melão, com contributo de João no calendário | 05/11 | 06/11 | Decisões registadas e refletidas nos documentos afetados. |
| Preparar e rever a WBS e o Gantt | João, com revisão dos seis elementos | 05/11 | 06/11 | Tarefas, responsáveis, datas, dependências e entregáveis revistos. |
| Preparar conteúdos e implementar a página web | Melão, Pinto e Rodrigo | 05/11 | 06/11 | Conteúdo do projeto e identificação da equipa preparados. |
| Publicar e verificar a página web | Melão, Pinto e Rodrigo | 09/11 | 10/11 | Endereço acessível e conteúdo verificado. |
| Recolher contributos para o relatório de desempenho I | Todos; consolidação por Francisco e Guilherme | 05/11 | 09/11 | Evidências do trabalho realizado e dificuldades de cada elemento reunidas. |
| Redigir o relatório de desempenho I | Francisco e Guilherme | 10/11 | 11/11 | Trabalho realizado comparado com o planeado e desvios explicados. |
| Rever o conjunto e preparar a submissão | Todos | 12/11 | 12/11 | Planeamento, página web e relatório conferidos antes da entrega. |

Todas as datas desta tabela pertencem a 2026 e dependem da confirmação do primeiro marco. O objetivo é ter os entregáveis preparados até 11/11 e reservar o dia 12/11 para a conferência e submissão.

## 8. Dependências principais

- O modelo de dados e os contratos devem ser definidos antes de ligar os módulos aos serviços reais.
- A simulação completa depende dos dados dos parques, do motor AHP, dos custos e do estado de autenticação.
- A apresentação e a exportação dependem dos resultados calculados pela simulação.
- Os testes de aceitação dependem de uma versão integrada do percurso principal.
- Os relatórios de desempenho dependem dos contributos e evidências fornecidos por todos.
- A documentação e a apresentação finais dependem de uma versão estável e de resultados validados.

No Excel, as dependências usam os identificadores da WBS. As fórmulas de datas estão ligadas aos marcos; não existe um motor de reagendamento automático por dependências. Alterar um prazo exige rever o encadeamento das tarefas.

## 9. Riscos e medidas previstas

| Risco | Efeito possível | Medida prevista |
| --- | --- | --- |
| Ano incoerente na data do Milestone 1 | Planeamento baseado num prazo incorreto. | Confirmar com o docente e atualizar o calendário. |
| Lista do docente com dez itens, incluindo distrito, enquanto o código usa nove critérios | Alterações ao modelo de dados, à interface e ao cálculo AHP. | Esclarecer o tratamento do distrito antes de fechar esses módulos. |
| Atrasos nos serviços de parques ou no motor AHP | Bloqueio da execução de simulações e da integração. | Acordar contratos cedo, testar cada módulo e rever dependências semanalmente. |
| Acumulação dos módulos técnicos com documentos e página web | Sobrecarga de alguns elementos. | Rever a carga semanal e repartir subtarefas dentro dos responsáveis indicados. |
| Disponibilidade reduzida no Natal e Ano Novo | Menos tempo para correções e preparação da entrega final. | Antecipar documentação e testes e confirmar a disponibilidade para esse período. |
| Diferenças no ambiente Windows, SDK ou SQL Server | Dificuldades de compilação, execução ou instalação. | Validar os pré-requisitos e testar o pacote de instalação noutro ambiente antes da entrega. |

## 10. Acompanhamento do plano

Propõe-se uma revisão semanal curta com os seis elementos. Cada pessoa comunica o trabalho concluído, as evidências disponíveis, os bloqueios e as tarefas seguintes. João atualiza o planeamento e Francisco e Guilherme usam essa informação para os relatórios de desempenho.

As alterações de prazo ou âmbito devem ficar registadas com motivo, impacto e nova previsão. Uma tarefa só deve ser assinalada como concluída quando cumprir o critério definido na WBS. Para código, isso inclui as verificações relevantes e a revisão da equipa; para documentos, a revisão do conteúdo; para a página web, a publicação e verificação do endereço.

O fluxo de colaboração do repositório mantém-se: trabalho nas branches atribuídas e propostas de integração através de Pull Requests revistos pela equipa.

## 11. Verificação antes da primeira submissão

- [ ] Confirmar a data do Milestone 1 com o docente.
- [ ] Esclarecer o tratamento do distrito na avaliação dos parques.
- [ ] Rever com a equipa as tarefas, durações, dependências e disponibilidade.
- [ ] Atualizar o progresso com base no trabalho efetivamente realizado.
- [ ] Confirmar que o Gantt e a WBS apresentam a mesma informação.
- [ ] Confirmar os responsáveis dos documentos das entregas seguintes.
- [ ] Verificar a publicação da página web com Melão, Pinto e Rodrigo.
- [ ] Conferir o relatório de desempenho I com Francisco e Guilherme.
- [ ] Confirmar os formatos, a plataforma e a hora limite de submissão.

## 12. Fontes

- `ISPGAYA - Projeto_SAD_20262027_Laboral.pdf`: requisitos funcionais, datas dos marcos e avaliação.
- `Sistemas de Apoio à Decisão - critérios T. Prático.pdf`: lista de critérios fornecida pelo docente.
- `README.md` e `docs/architecture.md`: âmbito técnico, estado documentado e módulos da equipa.
- `docs/planeamento/ParkWise_Gantt.xlsx`: base do calendário e da WBS detalhada.
- Distribuição dos entregáveis indicada por João em 05/11/2026.
