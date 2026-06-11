# Explicação do Projeto — Carteiro Chinês

Documento de apoio que explica **o que é o problema**, **como o código está organizado** e **qual a lógica** usada em cada etapa. Serve para entender e apresentar o trabalho.

---

## 1. O que é o problema do Carteiro Chinês

Imagine um **carteiro** que precisa entregar cartas em **todas as ruas** de um bairro. Ele quer:

1. passar por **todas as ruas** ao menos uma vez;
2. **voltar ao ponto de partida** no final (rota fechada);
3. andar o **mínimo possível**.

Esse é o **Problema do Carteiro Chinês** (em inglês, *Chinese Postman Problem*). No nosso projeto:

- as **ruas** são as **arestas** do grafo (têm um sentido e um custo/peso);
- os **cruzamentos** são os **vértices**;
- o **peso** de uma aresta é a "distância" ou "custo" de percorrê-la.

> Diferença importante para o "Caixeiro Viajante": o caixeiro precisa passar por todos os **vértices**; o carteiro precisa passar por todas as **arestas**.

### Quando existe solução?

Para existir uma rota fechada que passe por todas as arestas, o grafo precisa ser **fortemente conexo**: a partir de qualquer vértice deve ser possível chegar a qualquer outro. Se o grafo estiver "partido" em pedaços que não se conectam, é **impossível** (ver `grafo_impossivel.csv`).

### Quando a rota é "perfeita" (Euleriana)?

Se **todo vértice** tem **grau de entrada = grau de saída** (entram tantas arestas quanto saem), o grafo é **Euleriano**: existe um circuito que passa por **cada aresta exatamente uma vez**. Aí o custo é só a soma dos pesos, sem repetir nada (ver `grafo_euleriano.csv`).

Quando algum vértice está **desbalanceado**, é preciso **repetir algumas arestas** para conseguir fechar a rota — e queremos repetir o **mínimo** possível (ver `grafo.csv`).

---

## 2. Conceitos de grafo usados

- **Grafo direcionado**: cada aresta tem sentido. `A,B,1` cria `A → B`, mas **não** cria `B → A`.
- **Grau de saída** de um vértice: quantas arestas **saem** dele.
- **Grau de entrada**: quantas arestas **chegam** nele.
- **Matriz de adjacência**: tabela `n × n` onde a posição `[i][j]` guarda o peso da aresta de `i` para `j` (ou "infinito" se não houver ligação direta).
- **Circuito Euleriano**: rota fechada que usa cada aresta exatamente uma vez.

---

## 3. Estrutura do projeto (POO)

O projeto separa **dados** (Modelos) de **ações** (Serviços). Cada classe tem **uma responsabilidade**.

```
trabalhoPOO/
├── Program.cs                          -> interação com o usuário (entrada/saída)
├── Modelos/                            -> as "coisas" do problema (dados)
│   ├── Aresta.cs                       -> uma ligação origem -> destino com peso
│   ├── Grafo.cs                        -> vértices + arestas + matriz de adjacência
│   └── Resultado.cs                    -> a resposta (possível?, rota, custo)
├── Servicos/                           -> as "ações" do problema (lógica)
│   ├── LeitorCsv.cs                    -> lê o CSV e monta o Grafo
│   └── ResolvedorCarteiroChines.cs     -> calcula a menor rota
├── grafo.csv                           -> exemplo que precisa repetir arestas
├── grafo_euleriano.csv                 -> exemplo já euleriano (sem repetição)
└── grafo_impossivel.csv               -> exemplo sem solução
```

### As classes em detalhe

| Classe | Papel |
|---|---|
| **`Aresta`** | Guarda `Origem`, `Destino` e `Peso`. Valida no construtor (origem/destino não vazios, peso ≥ 0). |
| **`Grafo`** | Guarda a lista de vértices e de arestas. Sabe adicionar arestas (criando vértices novos), achar o índice de um vértice e montar a matriz de adjacência. |
| **`Resultado`** | Guarda a resposta: `Possivel`, `Caminho` (lista de vértices), `CustoTotal` e `Mensagem`. |
| **`LeitorCsv`** | Lê o arquivo, valida o formato e devolve um `Grafo` pronto. |
| **`ResolvedorCarteiroChines`** | O cérebro: recebe um `Grafo` e devolve um `Resultado`. |
| **`Program`** | Conversa com o usuário: pede o arquivo, chama os serviços e mostra o resultado. |

---

## 4. Fluxo de execução (passo a passo)

```
[Usuário] -> Program.Main
     |
     | 1) pede o caminho do CSV  (ObterCaminhoArquivo)
     v
[LeitorCsv.Ler]  -> lê o arquivo e devolve um Grafo
     |
     | 2) mostra o grafo na tela  (ExibirGrafo)
     v
[ResolvedorCarteiroChines.Resolver]  -> calcula a rota e devolve um Resultado
     |
     | 3) mostra o resultado  (ExibirResultado)
     v
[Usuário] <- rota + custo, ou "não foi possível"
```

### Etapa 1 — Leitura (`LeitorCsv.Ler`)
- Confere se o arquivo existe.
- Lê linha por linha. Pula linhas em branco e ignora o cabeçalho (`origem,destino,peso`).
- Quebra cada linha por `,` ou `;`, valida e cria uma `Aresta`.
- Adiciona cada aresta ao `Grafo` (que cria os vértices automaticamente).

### Etapa 2 — Exibição do grafo (`Program.ExibirGrafo`)
- Mostra os vértices encontrados e a lista de arestas (`origem -> destino (peso X)`).

### Etapa 3 — Resolução (`ResolvedorCarteiroChines.Resolver`)
É o coração do projeto. Funciona em **quatro fases**:

#### Fase A — Contar os graus
Percorre todas as arestas contando o **grau de saída** e o **grau de entrada** de cada vértice, e soma todos os pesos (`somaPesos`).

#### Fase B — Menores distâncias (Floyd-Warshall)
`ExecutarFloydWarshall` calcula a **menor distância entre todos os pares** de vértices. Guarda também a matriz `proximo`, que permite **reconstruir o caminho** mais curto entre dois vértices depois.

- *Por que precisamos disso?* Para saber, quando for repetir arestas, qual o trajeto **mais barato** entre dois vértices desbalanceados.

#### Fase C — Verificar se é possível (`EhFortementeConexo`)
Usando as distâncias, confere se **todo vértice que tem aresta alcança todos os outros**. Se algum par não se alcança, devolve um `Resultado` "impossível" — é o caso do `grafo_impossivel.csv`.

#### Fase D — Balancear e montar a rota
1. **Balanceamento guloso** (`BalancearGrafo`):
   - Vértices com **saída > entrada** precisam **receber** arestas extras.
   - Vértices com **entrada > saída** precisam **enviar** arestas extras.
   - Enquanto houver desbalanceamento, escolhe o par (origem que precisa enviar → destino que precisa receber) com a **menor distância** e "repete" esse caminho mais curto (acrescentando as arestas em `saida`). Soma o custo dessas repetições em `custoRepeticoes`.
2. **Montar o circuito** (`Hierholzer`):
   - Com o grafo já equilibrado, o **algoritmo de Hierholzer** percorre as arestas e constrói a rota fechada que passa por todas elas.
3. **Resultado final**:
   - `CustoTotal = somaPesos + custoRepeticoes`.
   - Se `custoRepeticoes == 0`, a mensagem informa que o grafo **já era Euleriano**.

### Etapa 4 — Exibição do resultado (`Program.ExibirResultado`)
- Se possível: mostra a rota (`A -> B -> ...`), o custo total e uma observação.
- Se impossível: mostra o motivo.

---

## 5. A lógica dos algoritmos (resumo)

| Algoritmo | Onde | Para quê |
|---|---|---|
| **Floyd-Warshall** | `ExecutarFloydWarshall` | Descobrir a menor distância entre **todos** os pares de vértices. |
| **Verificação de forte conexidade** | `EhFortementeConexo` | Decidir se o problema tem solução. |
| **Balanceamento guloso** | `BalancearGrafo` | Escolher quais arestas repetir, gastando o mínimo, para equilibrar os graus. |
| **Hierholzer** | `Hierholzer` | Construir a rota final que passa por todas as arestas. |

> **Observação sobre o "guloso":** a cada passo ele liga o vértice que precisa enviar ao que precisa receber pela **menor distância disponível**. É simples e dá o resultado **ótimo** nos grafos pequenos deste trabalho. (A versão totalmente geral usaria *fluxo de custo mínimo*, mais complexa; aqui optamos pela versão didática.)

---

## 6. Contexto dos grafos de exemplo

### `grafo.csv` — precisa repetir arestas
```
A,B,1   B,C,2   C,D,1   D,A,3   A,C,4   B,D,5
```
Os graus ficam **desbalanceados**, então o programa repete o trajeto mais curto para equilibrar.
- Soma dos pesos = **16**
- Repetições necessárias = **8**
- **Custo total = 24**

### `grafo_euleriano.csv` — já está perfeito
```
A,B,1   B,C,2   C,A,3
```
Ciclo simples: todo vértice tem entrada = saída. Não repete nada.
- **Custo total = 6** (= soma dos pesos)
- Mensagem: "O grafo já era Euleriano".

### `grafo_impossivel.csv` — sem solução
```
A,B,1   B,A,1   C,D,1   D,C,1
```
Dois pares isolados (`A⇄B` e `C⇄D`) que **não se conectam**. Não é fortemente conexo, então é **impossível** percorrer tudo numa única rota fechada. O programa detecta e avisa.

---

## 7. Como executar

```bash
cd trabalhoPOO
dotnet run                      # pergunta o arquivo; ENTER usa grafo.csv
dotnet run grafo_euleriano.csv  # caso já euleriano
dotnet run grafo_impossivel.csv # caso sem solução
```

Para testar um grafo próprio, crie um `.csv` com uma aresta por linha (`origem,destino,peso`) dentro da pasta `trabalhoPOO` e informe o nome dele ao programa.
