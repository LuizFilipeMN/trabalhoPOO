# Carteiro Chinês (menor rota por todas as arestas)

Projeto de **Programação Orientada a Objetos (POO)** em **C#**, feito de forma simples e didática.

## O que o programa faz

1. Lê um arquivo **CSV** que representa um **grafo direcionado**.
2. Calcula a **menor rota fechada que percorra todas as arestas ao menos uma vez**
   (problema clássico do **Carteiro Chinês**).
3. Mostra a rota e o custo total **ou** informa que **não é possível** gerá-la.

## Formato do arquivo CSV

Uma aresta por linha, no formato `origem,destino,peso`. A primeira linha pode ser
um cabeçalho (ele é ignorado automaticamente):

```csv
origem,destino,peso
A,B,1
B,C,2
C,D,1
D,A,3
A,C,4
B,D,5
```

- O grafo é **direcionado**: `A,B,1` cria a ligação `A -> B`, mas **não** cria `B -> A`.
- O `peso` deve ser um número inteiro **maior ou igual a zero**.
- Aceita vírgula (`,`) ou ponto e vírgula (`;`) como separador.

## Estrutura do projeto

```
CarteiroChines/
├── CarteiroChines.csproj          (configuração do projeto .NET)
├── Program.cs                     (interação via console - método Main)
├── Modelos/                       (as "coisas" do problema)
│   ├── Aresta.cs                  (uma ligação origem -> destino com peso)
│   ├── Grafo.cs                   (vértices + arestas + matriz de adjacência)
│   └── Resultado.cs              (a resposta: possível?, rota, custo)
├── Servicos/                      (as "ações" do problema)
│   ├── LeitorCsv.cs               (lê o CSV e monta o Grafo)
│   └── ResolvedorCarteiroChines.cs (calcula a menor rota pelas arestas)
├── grafo.csv                      (exemplo que TEM solução)
└── grafo_impossivel.csv           (exemplo que NÃO tem solução)
```

Essa divisão em **Modelos** (dados) e **Serviços** (lógica) é uma forma clássica e
simples de aplicar POO: cada classe tem **uma responsabilidade**.

## Como executar

Precisa ter o **.NET SDK** instalado (https://dotnet.microsoft.com/download).

Dentro da pasta `CarteiroChines`:

```bash
dotnet run
```

- Ao iniciar, ele pergunta o caminho do CSV. Aperte **ENTER** para usar o `grafo.csv` de exemplo.
- Para testar o caso sem solução, digite `grafo_impossivel.csv`.

Também é possível passar o arquivo direto como argumento:

```bash
dotnet run grafo_impossivel.csv
```

> Se você usa o **Visual Studio**, basta abrir o arquivo `CarteiroChines.csproj`
> (ou criar uma solução com ele) e clicar em **Executar (F5)**.

## Como o algoritmo funciona (resumo)

O **Carteiro Chinês** procura a menor rota **fechada** que percorra **todas as
arestas** ao menos uma vez. O programa resolve em três etapas:

1. **Verifica se é possível**: o grafo (considerando os vértices que têm
   arestas) precisa ser **fortemente conexo** — ou seja, dá para ir de qualquer
   vértice a qualquer outro. Se não for, é **impossível** e o programa avisa.

2. **Equilibra os graus (balanceamento)**: se algum vértice tem grau de entrada
   diferente do de saída, é preciso **repetir algumas arestas** para que a rota
   possa fechar. Para repetir o **mínimo** possível, isso é modelado como um
   **fluxo de custo mínimo**, usando como custo a menor distância entre os
   vértices (calculada com **Floyd-Warshall**).

3. **Monta a rota (Hierholzer)**: com o grafo já equilibrado, o algoritmo de
   **Hierholzer** constrói o circuito que passa por todas as arestas.

> O **custo total** é a soma dos pesos de todas as arestas **mais** o custo das
> arestas que precisaram ser repetidas. Se o grafo já for *Euleriano* (todo
> vértice com entrada = saída), nenhuma aresta é repetida e o custo é apenas a
> soma dos pesos.

## Exemplo de saída (usando `grafo.csv`)

O `grafo.csv` tem soma de pesos `16` e precisa repetir arestas no valor de `8`
para equilibrar os graus, então o custo total é `24`:

```
Menor rota que passa por todas as arestas (rota fechada):
    A -> B -> C -> D -> A -> ... -> A
Custo total: 24
```

(A sequência exata de vértices é uma rota válida entre as possíveis de mesmo custo.)
