# ParkWise — Decision Support System for Science Park Selection

Projeto académico da unidade curricular de Sistemas de Apoio à Decisão, Licenciatura em Engenharia Informática do ISPGAYA, ano letivo 2026/2027. Equipa: João Amen, Gonçalo Melão, Rodrigo Sousa e Gonçalo Pinto.

O ParkWise será uma aplicação desktop Windows para apoiar a escolha de parques de ciência e tecnologia. A avaliação futura usará AHP, custos operacionais e os nove critérios definidos em [`EvaluationCriteria.cs`](src/ParkWise.Core/EvaluationCriteria.cs).

## Estado da primeira fase

- Solução WPF com MVVM, injeção de dependências, sidebar, navegação e dashboard inicial.
- A dashboard mostra **apenas dados fictícios**, identificados na interface. Não existe ligação a dados reais.
- Entidades fundamentais e `DbContext` preparados para SQL Server Express. Não há migrações nem criação automática da base de dados.
- Os menus Parques Tecnológicos, Simulações, Histórico e Definições apresentam ecrãs informativos. Autenticação, importação XML/XSD, AHP, custos, exportação e módulos adicionais ainda não estão implementados.

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

Também é possível abrir `ParkWise.sln` no Visual Studio e definir `ParkWise.App` como projeto de arranque. O SDK e os pacotes NuGet são necessários para restaurar, compilar e testar. A conexão SQL Server é opcional nesta fase. A configuração de `DbContext` apenas regista o fornecedor quando `PARKWISE_SQLSERVER_CONNECTION` está definida; não executa migrações.

## Organização

| Pasta | Responsabilidade |
| --- | --- |
| `src/ParkWise.App` | WPF, Views, ViewModels, navegação, composição de dependências |
| `src/ParkWise.Core` | Entidades, enumerações e catálogo dos critérios obrigatórios |
| `src/ParkWise.Application` | Contratos para casos de utilização e acesso a dados |
| `src/ParkWise.Infrastructure` | EF Core, mapeamento e SQL Server |
| `tests/ParkWise.Tests` | Testes unitários |
| `docs` | Notas de arquitetura e planeamento |
| `database` | Futuras migrações e documentação da base de dados |

Fluxo de dependências: App → Application e Infrastructure; Infrastructure → Application e Core; Application → Core. A interface não consulta diretamente o `DbContext`.

## Git e GitHub — Guia de colaboração

O repositório da equipa é [jonyzik/Projeto-SAD-26-27](https://github.com/jonyzik/Projeto-SAD-26-27). Os comandos seguintes usam `dev/joao` como exemplo de branch pessoal. Cada elemento deve substituir `joao` pelo seu nome. Executar os comandos dentro da pasta do repositório, exceto o primeiro `git clone`.

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

Criar uma branch pessoal a partir da `main` atualizada e publicá-la no GitHub. A criação é feita apenas uma vez por branch:

```powershell
git switch -c dev/joao
git push -u origin dev/joao
```

O parâmetro `-u` associa a branch local à branch remota; depois disso, basta usar `git push`. Para mudar entre branches já existentes:

```powershell
git switch dev/joao
git switch main
```

Antes de mudar de branch, confirmar com `git status` que as alterações em curso estão guardadas ou que a mudança não as afetará.

### Guardar e enviar alterações

Na branch pessoal, rever os ficheiros alterados, adicionar apenas os pretendidos e criar um commit com uma mensagem descritiva:

```powershell
git status
git add README.md
git diff --cached
git commit -m "docs: acrescenta guia de colaboração Git"
git push
```

Para incluir vários ficheiros, indicá-los no `git add` ou usar `git add -A` depois de rever `git status`. Não incluir ficheiros com credenciais ou outros dados sensíveis.

### Integrar atualizações da main na branch pessoal

Guardar primeiro o trabalho em curso. Na branch pessoal, obter a versão mais recente da `main` remota e integrá-la:

```powershell
git switch dev/joao
git fetch origin
git merge origin/main
git push
```

Se houver conflitos, `git status` identifica os ficheiros afetados. Abrir cada um, escolher e combinar as alterações corretas, remover os marcadores `<<<<<<<`, `=======` e `>>>>>>>`, e voltar a verificar o resultado. Quando todos estiverem resolvidos:

```powershell
git status
git add caminho/do/ficheiro
git commit
git push
```

O `git commit` conclui o merge interrompido pelos conflitos. Se for necessário desistir da tentativa antes de a concluir, usar `git merge --abort`. Não apagar alterações de colegas nem usar `git reset --hard` para contornar conflitos. Depois da resolução, compilar e executar os testes antes de publicar ou criar o Pull Request.

### Criar e concluir um Pull Request

1. Confirmar que o trabalho está na branch pessoal e atualizado com a `main`. Executar `dotnet build ParkWise.sln --configuration Debug` e `dotnet test ParkWise.sln --configuration Debug` num ambiente Windows com o SDK instalado.
2. Enviar os commits com `git push`.
3. No [GitHub do projeto](https://github.com/jonyzik/Projeto-SAD-26-27/pulls), escolher **New pull request**, definir `main` como **base** e `dev/joao` como **compare**.
4. Descrever o que mudou, indicar como foi verificado e pedir revisão à equipa. Integrar apenas depois da revisão e das verificações necessárias.

Após a integração do Pull Request, atualizar a `main` local:

```powershell
git switch main
git pull --ff-only origin main
```

Para começar trabalho novo, criar uma nova branch `dev/nome` a partir da `main` atualizada, ou atualizar a branch pessoal existente com `git fetch origin` e `git merge origin/main`.

### Regras da equipa

- Cada elemento trabalha na sua própria branch `dev/nome`.
- A `main` deve manter-se estável; evitar commits diretos nela.
- Integrar todas as alterações na `main` através de Pull Requests revistos pela equipa.
- Antes de criar um Pull Request, confirmar que o projeto compila e que os testes passam.
- Nunca fazer `force push` para a `main`.
- Nunca incluir passwords, connection strings com credenciais ou outros dados sensíveis no repositório.
- Usar mensagens de commit claras e descritivas.

## Próximos passos

Implementar autenticação segura, gestão e importação de parques com XML/XSD, pesquisa, casos de utilização da simulação AHP, cálculo de custos e exportação. Rever o modelo de dados e criar migrações versionadas antes da primeira base de dados partilhada. Nenhuma migração deve ser aplicada sem confirmar o ambiente de destino.
