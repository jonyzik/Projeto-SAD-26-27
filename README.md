# ParkWise — Decision Support System for Science Park Selection

Projeto académico da unidade curricular de Sistemas de Apoio à Decisão, Licenciatura em Engenharia Informática do ISPGAYA, ano letivo 2026/2027. A distribuição atual do trabalho envolve João, Francisco, Melão, Guilherme, Rodrigo e Pinto.

O ParkWise será uma aplicação desktop Windows para apoiar a escolha de parques de ciência e tecnologia. A avaliação futura usará AHP, custos operacionais e os nove critérios definidos em [`EvaluationCriteria.cs`](src/ParkWise.Core/EvaluationCriteria.cs).

## Estado atual do desenvolvimento

- Solução WPF com MVVM, injeção de dependências, sidebar fixa e navegação dentro da mesma janela. O menu ativo fica destacado e as páginas são registadas por um contrato de rotas, preparado para receber os módulos da equipa.
- Dashboard com quatro indicadores calculados por um serviço da camada Application, parques recentes, últimas simulações, ação rápida para abrir Simulações e gráfico simples por intervalos de preço.
- Sem ligação configurada, a dashboard usa **dados fictícios**, identificados na interface. Com ligação configurada, lê a base de dados através de EF Core; se a leitura falhar, apresenta um erro em vez de números fictícios.
- Entidades fundamentais e `DbContext` preparados para SQL Server Express. Não há migrações nem criação automática da base de dados.
- «Parques de Ciência» inclui pesquisa por nome e localização, filtros por serviços e preço, detalhes com os nove critérios e seleção para comparação descritiva. Os seis parques do catálogo são **fictícios** e identificados na interface. A ligação ao serviço real de parques ainda não existe; a dashboard usa uma fonte de demonstração independente, pelo que os seus indicadores não representam este catálogo.
- Simulações, Histórico e Definições continuam com ecrãs informativos. A comparação preliminar oferece uma ligação para a página provisória «Nova simulação» quando há pelo menos dois parques selecionados. Autenticação, importação XML/XSD, AHP, custos, exportação e módulos adicionais ainda não estão implementados.

### Regras dos indicadores

- **Total de parques:** número de parques na fonte de dados.
- **Total de simulações:** número de simulações guardadas na fonte de dados.
- **Disponíveis para comparação:** parques com preço mensal por m² positivo e avaliações para os nove critérios obrigatórios.
- **Preço médio:** média dos preços mensais por m² positivos, arredondada a duas casas decimais. Quando não existem preços válidos, é apresentado «—».
- As listas mostram, no máximo, os cinco registos mais recentes de cada tipo. O gráfico apresenta quatro intervalos de preço quando existem pelo menos dois parques com preço válido.

## Requisitos de desenvolvimento

- Windows 10/11 com [SDK .NET 10](https://dotnet.microsoft.com/download/dotnet/10.0) e suporte para desenvolvimento desktop WPF. Pode usar Visual Studio com a carga de trabalho «Desenvolvimento para ambiente de trabalho com .NET».
- Se a equipa não puder instalar o SDK .NET 10, a alternativa suportada é adaptar todos os projetos para .NET 8, com EF Core 8 e pacotes `Microsoft.Extensions.*` 8. Esta solução está configurada para .NET 10; essa adaptação ainda não foi feita.
- A aplicação abre sem SQL Server. Para desenvolvimento futuro da persistência, instalar SQL Server Express e configurar a variável de ambiente `PARKWISE_SQLSERVER_CONNECTION` com uma connection string local. Não colocar passwords ou connection strings com segredos no repositório.

## Compilar, testar e executar

Na raiz do repositório, em Windows PowerShell:

```powershell
dotnet restore ParkWise.sln
dotnet build ParkWise.sln --configuration Debug
dotnet test ParkWise.sln --configuration Debug
dotnet run --project src/ParkWise.App/ParkWise.App.csproj
```

Também é possível abrir `ParkWise.sln` no Visual Studio e definir `ParkWise.App` como projeto de arranque. O SDK e os pacotes NuGet são necessários para restaurar, compilar e testar. A conexão SQL Server é opcional nesta fase. A configuração de `DbContext` apenas regista o fornecedor quando `PARKWISE_SQLSERVER_CONNECTION` está definida; não executa migrações. Uma connection string definida seleciona a fonte EF Core; a dashboard requer então uma base de dados acessível e o esquema correspondente.

Em Linux, os testes das camadas Core e Application podem ser executados com `dotnet test tests/ParkWise.Tests/ParkWise.Tests.csproj` **se o SDK .NET 10 estiver instalado**. A aplicação WPF só pode ser executada em Windows; a compilação cruzada poderá depender da obtenção dos pacotes de targeting do Windows.

## Organização

| Pasta | Responsabilidade |
| --- | --- |
| `src/ParkWise.App` | WPF, Views, ViewModels, contrato de rotas e composição de dependências |
| `src/ParkWise.Core` | Entidades, enumerações e catálogo dos critérios obrigatórios |
| `src/ParkWise.Application` | Contratos e serviço de cálculo da dashboard; pesquisa e seleção de parques |
| `src/ParkWise.Infrastructure` | EF Core, SQL Server e fontes de dados reais ou de demonstração para a dashboard |
| `tests/ParkWise.Tests` | Testes unitários dos critérios, dashboard, pesquisa, filtros e seleção |
| `docs` | Notas de arquitetura e planeamento |
| `database` | Futuras migrações e documentação da base de dados |

Fluxo de dependências: App → Application e Infrastructure; Infrastructure → Application e Core; Application → Core. A interface consulta `IDashboardService`, nunca o `DbContext` diretamente. `IDashboardDataSource` permite trocar a fonte fictícia pela fonte EF Core através da injeção de dependências.

## Distribuição do trabalho

Cada responsável desenvolve na sua branch e propõe a integração na `main` por Pull Request (PR). As áreas abaixo definem a responsabilidade principal; quando um módulo precisar de código de outra área, a equipa deve combinar primeiro os contratos de integração e rever o PR em conjunto.

| Responsável | Branch | Módulo e entregáveis |
| --- | --- | --- |
| **João** | `dev/admin` | Interface principal WPF, sidebar e navegação; dashboard e estatísticas; componentes visuais reutilizáveis; integração das interfaces dos outros módulos. |
| **Francisco** | `dev/parques` | SQL Server e EF Core; registo, edição e pesquisa de Science Parks; armazenamento dos nove critérios; importação XML; serviços de acesso e consulta. |
| **Melão** | `dev/ahp` | Método AHP; comparações dos nove critérios e cálculo dos pesos; consistência; avaliação e classificação dos parques; testes unitários. |
| **Guilherme** | `dev/simulacoes` | Criação e execução de simulações; necessidades da empresa; custos operacionais; integração com AHP; parâmetros das simulações. |
| **Rodrigo** | `dev/auth` | Login e logout; utilizadores; armazenamento seguro de passwords; sessões, permissões e controlo de acesso. |
| **Pinto** | `dev/resultados` | Ranking dos parques; gráficos e comparação; pontos fortes e fracos; exportação PDF/CSV; apresentação detalhada dos resultados. |

### Como integrar os módulos

- **Contratos primeiro:** definir em `ParkWise.Application` as interfaces e os modelos necessários entre módulos antes de ligar a interface aos serviços. Manter as regras de domínio em `ParkWise.Core`, a persistência em `ParkWise.Infrastructure` e as Views/ViewModels em `ParkWise.App`.
- **Interface:** cada módulo fornece ao João o contrato público, o ViewModel ou os dados a apresentar e os estados de carregamento/erro. A navegação existente usa `IPageRoute`; a página provisória pode ser substituída pela View do módulo sem criar outra janela.
- **Dependências:** Francisco fornece os serviços de parques; Melão expõe os cálculos AHP; Guilherme coordena esses serviços nas simulações; Pinto apresenta os resultados produzidos; Rodrigo fornece o estado de autenticação e as permissões. Os contratos concretos devem ser acordados nos respetivos PRs.
- **Ficheiros partilhados:** alterações a `App.xaml.cs`, `MainWindow`, entidades ou configuração de DI devem ser combinadas com os responsáveis afetados. PRs pequenos reduzem conflitos e tornam a revisão mais simples.
- **Critérios:** os nove critérios obrigatórios já constam de [`EvaluationCriteria.cs`](src/ParkWise.Core/EvaluationCriteria.cs); usar esse catálogo como referência comum, sem criar listas divergentes por módulo.

Esta tabela é um plano de trabalho. O estado implementado encontra-se na secção «Estado atual do desenvolvimento»; a atribuição de uma tarefa não significa que já esteja concluída.

## Git e GitHub — Guia de colaboração

O repositório da equipa é [jonyzik/Projeto-SAD-26-27](https://github.com/jonyzik/Projeto-SAD-26-27). A `main` contém o trabalho integrado; cada pessoa trabalha na branch indicada na tabela acima. Executar os comandos na raiz do repositório, exceto o primeiro `git clone`. Os exemplos usam a branch do João, `dev/admin`; os restantes devem substituí-la pela sua branch.

**Resumo do fluxo:** atualizar `main` → trabalhar na branch pessoal → rever alterações → `git add` → `git commit` → **avisar no Discord** → `git push` → abrir PR → rever e integrar → atualizar `main` local.

### Obter o repositório e preparar a branch pessoal

Para quem ainda não tem uma cópia local:

```powershell
git clone https://github.com/jonyzik/Projeto-SAD-26-27.git
cd Projeto-SAD-26-27
```

Verificar a branch atual e as alterações locais antes de mudar de branch:

```powershell
git branch --show-current
git status
```

Atualizar a `main` local com as alterações aprovadas no GitHub:

```powershell
git switch main
git pull --ff-only origin main
```

Para criar uma branch pessoal **nova** a partir da `main` atualizada e publicá-la no GitHub, substituir `nome` pelo identificador da tabela. A criação é feita apenas uma vez por branch:

```powershell
git switch -c dev/nome
git push -u origin dev/nome
```

Se a branch já existir no GitHub, não a recriar: obter as referências e associar a branch local à remota. Por exemplo:

```powershell
git fetch origin
git switch --track origin/dev/admin
```

O parâmetro `-u` de `git push -u origin dev/nome` associa a branch local à remota; depois basta usar `git push`. Para mudar entre branches já existentes localmente:

```powershell
git switch dev/admin
git switch main
```

Antes de mudar de branch, confirmar com `git status` que as alterações em curso estão guardadas ou que a mudança não as afetará.

### Guardar e enviar alterações

Um **commit** guarda um conjunto de alterações no histórico **local** da branch; por si só, não publica nada no GitHub. Na branch pessoal, rever os ficheiros, adicionar apenas os pretendidos e criar um commit com uma mensagem clara:

```powershell
git status
git add README.md
git diff --cached
git commit -m "docs: acrescenta guia de colaboração Git"
```

`git add` prepara os ficheiros para o próximo commit; `git diff --cached` mostra exatamente o que será incluído. Para incluir vários ficheiros, indicá-los no `git add` ou usar `git add -A` **depois** de rever `git status`.

**Aviso no Discord após cada commit:** quem cria o commit avisa a equipa, mesmo antes de fazer push. Indicar módulo, branch, resumo, hash curto e se já foi enviado ao GitHub. Exemplo:

```text
ParkWise · Interface | dev/admin | 58d91ff
Navegação entre páginas e ecrãs provisórios concluídos.
Push: pendente. PR: por abrir.
```

Obter o hash com `git log -1 --oneline`. Depois do aviso, publicar o commit:

```powershell
git push
```

Atualizar a equipa quando o push estiver concluído. `git push` envia os commits locais para o GitHub; não integra a branch na `main`. Se o push falhar porque a branch remota avançou, obter as alterações e resolver a divergência antes de repetir o push. Não usar `force push` como solução rápida. O aviso no Discord facilita a coordenação; a revisão e integração continuam a ser feitas no GitHub. Não publicar segredos nem dados sensíveis na mensagem ou no commit.

### Integrar atualizações da main na branch pessoal

Guardar primeiro o trabalho em curso num commit ou assegurar que a árvore de trabalho está limpa. Na branch pessoal, obter a versão mais recente da `main` remota e integrá-la:

```powershell
git switch dev/admin
git fetch origin
git merge origin/main
git push
```

Se houver conflitos, `git status` identifica os ficheiros afetados. Abrir cada um, compreender as duas versões, combinar as alterações corretas e remover os marcadores `<<<<<<<`, `=======` e `>>>>>>>`. Quando todos estiverem resolvidos:

```powershell
git status
git add caminho/do/ficheiro
git commit
```

O `git commit` conclui o merge interrompido pelos conflitos. Avisar no Discord também após esse commit e, depois de rever o diff, compilar e executar os testes, publicar com `git push`. Se for necessário desistir da tentativa antes de a concluir, usar `git merge --abort`. Em caso de dúvida sobre a solução, contactar o responsável pelo módulo antes de guardar o conflito resolvido. Não apagar alterações de colegas nem usar `git reset --hard` para contornar conflitos.

### Criar e concluir um Pull Request

1. Confirmar que o trabalho está na branch pessoal e atualizado com a `main`. Executar `dotnet build ParkWise.sln --configuration Debug` e `dotnet test ParkWise.sln --configuration Debug` num ambiente Windows com o SDK instalado. Se não for possível validar WPF em Windows, indicar claramente essa limitação no PR e pedir validação a quem tenha esse ambiente.
2. Enviar os commits com `git push`.
3. No [GitHub do projeto](https://github.com/jonyzik/Projeto-SAD-26-27/pulls), escolher **New pull request**, definir `main` como **base** e a própria branch (por exemplo, `dev/admin`) como **compare**.
4. Descrever o módulo, o que mudou, como foi testado, as limitações e qualquer contrato que afete outro módulo. Pedir revisão à equipa e avisar no Discord que o PR está pronto.
5. Integrar apenas depois da revisão e das verificações necessárias. Se o PR exigir correções, criar novos commits na mesma branch, avisar no Discord e fazer `git push`; o PR é atualizado automaticamente.

Após a integração do Pull Request, atualizar a `main` local:

```powershell
git switch main
git pull --ff-only origin main
```

Para continuar na branch pessoal existente, voltar a ela e integrar a `main` atualizada:

```powershell
git switch dev/admin
git merge main
```

Para outra pessoa, usar a branch atribuída na tabela. Repetir este ciclo regularmente para evitar conflitos grandes.

### Regras da equipa

- Cada elemento trabalha na sua branch indicada na tabela de responsabilidades.
- A `main` deve manter-se estável; evitar commits diretos nela.
- Integrar todas as alterações na `main` através de Pull Requests revistos pela equipa.
- Antes de criar um Pull Request, confirmar que o projeto compila e que os testes passam; documentar limitações de ambiente quando a validação em Windows depender de outro elemento.
- Nunca fazer `force push` para a `main`.
- Nunca incluir passwords, connection strings com credenciais ou outros dados sensíveis no repositório.
- Usar mensagens de commit claras e descritivas.
- Avisar no Discord **sempre que fizer um commit**, com branch, hash e resumo; atualizar o aviso depois do push e partilhar o PR para revisão.

## Próximos passos

Implementar autenticação segura, gestão e importação de parques com XML/XSD, pesquisa, casos de utilização da simulação AHP, cálculo de custos e exportação. Rever o modelo de dados e criar migrações versionadas antes da primeira base de dados partilhada. Nenhuma migração deve ser aplicada sem confirmar o ambiente de destino.
