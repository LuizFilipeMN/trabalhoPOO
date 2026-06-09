using System;
using System.Collections.Generic;
using CarteiroChines.Modelos;

namespace CarteiroChines.Servicos
{
    /// <summary>
    /// Resolve o problema do CARTEIRO CHINES em um grafo DIRECIONADO:
    /// encontrar o menor percurso (rota fechada) que passe por TODAS as
    /// ARESTAS do grafo ao menos uma vez, voltando ao ponto de partida.
    ///
    /// Ideia geral (em tres etapas):
    ///
    /// 1) Se o grafo ja for "Euleriano" -- ou seja, fortemente conexo e com
    ///    grau de entrada igual ao grau de saida em todos os vertices -- entao
    ///    existe um circuito que percorre cada aresta exatamente uma vez.
    ///    Nesse caso o custo e simplesmente a soma de todos os pesos.
    ///
    /// 2) Se algum vertice esta "desbalanceado" (entra mais do que sai, ou o
    ///    contrario), precisamos REPETIR algumas arestas para equilibrar. Para
    ///    gastar o minimo possivel, escolhemos quais arestas repetir resolvendo
    ///    um problema de FLUXO DE CUSTO MINIMO, usando como custo a menor
    ///    distancia entre os vertices (calculada com Floyd-Warshall).
    ///
    /// 3) Com o grafo equilibrado, montamos a rota de verdade usando o
    ///    algoritmo de Hierholzer (que constroi um circuito euleriano).
    ///
    /// Se o grafo (considerando os vertices que tem arestas) nao for fortemente
    /// conexo, e impossivel percorrer todas as arestas em uma unica rota fechada.
    /// </summary>
    public class ResolvedorCarteiroChines
    {
        // Valor usado para representar "infinito" (sem caminho).
        // Dividimos por 2 para nunca estourar o int ao somar dois infinitos.
        private const int Infinito = int.MaxValue / 2;

        public Resultado Resolver(Grafo grafo)
        {
            int n = grafo.QuantidadeVertices;
            List<Aresta> arestas = grafo.ObterArestas();

            if (n == 0 || arestas.Count == 0)
                return Resultado.CriarImpossivel(
                    "O grafo nao possui arestas para percorrer.");

            // ---- Graus de entrada e saida de cada vertice ----
            // (contamos CADA aresta; arestas paralelas contam separadamente)
            int[] grauSaida = new int[n];
            int[] grauEntrada = new int[n];
            int somaPesos = 0;

            foreach (Aresta aresta in arestas)
            {
                int origem = grafo.IndiceDoVertice(aresta.Origem);
                int destino = grafo.IndiceDoVertice(aresta.Destino);
                grauSaida[origem]++;
                grauEntrada[destino]++;
                somaPesos += aresta.Peso;
            }

            // ---- Floyd-Warshall: menor distancia entre qualquer par ----
            int[][] distancia;
            int[][] proximo;
            ExecutarFloydWarshall(grafo, out distancia, out proximo);

            // ---- Verifica se da para percorrer tudo (forte conexidade) ----
            if (!EhFortementeConexo(n, grauSaida, grauEntrada, distancia))
            {
                return Resultado.CriarImpossivel(
                    "Nao e possivel gerar a rota: o grafo direcionado nao e " +
                    "fortemente conexo, entao nao da para percorrer todas as " +
                    "arestas em uma unica rota fechada.");
            }

            // ---- Etapa do balanceamento (fluxo de custo minimo) ----
            // saida[i] = lista de vertices para onde existe uma aresta saindo de i.
            // Comecamos com as arestas originais; depois acrescentamos as repetidas.
            List<int>[] saida = new List<int>[n];
            for (int i = 0; i < n; i++)
                saida[i] = new List<int>();

            foreach (Aresta aresta in arestas)
            {
                int origem = grafo.IndiceDoVertice(aresta.Origem);
                int destino = grafo.IndiceDoVertice(aresta.Destino);
                saida[origem].Add(destino);
            }

            int custoRepeticoes = BalancearGrafo(
                n, grauSaida, grauEntrada, distancia, proximo, saida);

            int totalArcos = ContarArcos(saida);

            // ---- Etapa final: monta a rota com Hierholzer ----
            int inicio = PrimeiroVerticeComSaida(saida);
            List<int> circuito = Hierholzer(saida, inicio, totalArcos);

            // Seguranca: se a rota nao usou todas as arestas, algo nao fecha.
            if (circuito.Count != totalArcos + 1)
            {
                return Resultado.CriarImpossivel(
                    "Nao foi possivel montar uma rota que use todas as arestas.");
            }

            // ---- Monta o resultado ----
            Resultado resultado = new Resultado();
            resultado.Possivel = true;
            resultado.CustoTotal = somaPesos + custoRepeticoes;
            resultado.Mensagem = (custoRepeticoes == 0)
                ? "O grafo ja era Euleriano: cada aresta e percorrida uma unica vez."
                : "Algumas arestas precisaram ser repetidas para equilibrar o grafo.";

            foreach (int indice in circuito)
                resultado.Caminho.Add(grafo.NomeDoVertice(indice));

            return resultado;
        }

        // ----------------------------------------------------------------
        // Floyd-Warshall: preenche a menor distancia entre todos os pares
        // e a matriz "proximo" usada para reconstruir os caminhos.
        // ----------------------------------------------------------------
        private void ExecutarFloydWarshall(Grafo grafo,
            out int[][] distancia, out int[][] proximo)
        {
            int n = grafo.QuantidadeVertices;
            distancia = grafo.MontarMatrizAdjacencia(Infinito);
            proximo = ConstruirMatrizProximo(distancia, n);

            for (int k = 0; k < n; k++)
            {
                for (int i = 0; i < n; i++)
                {
                    for (int j = 0; j < n; j++)
                    {
                        if (distancia[i][k] + distancia[k][j] < distancia[i][j])
                        {
                            distancia[i][j] = distancia[i][k] + distancia[k][j];
                            proximo[i][j] = proximo[i][k];
                        }
                    }
                }
            }
        }

        // Cria a matriz "proximo" inicial: se existe aresta direta de i para j,
        // o proximo passo ao ir de i para j e o proprio j.
        private int[][] ConstruirMatrizProximo(int[][] distancia, int n)
        {
            int[][] proximo = new int[n][];
            for (int i = 0; i < n; i++)
            {
                proximo[i] = new int[n];
                for (int j = 0; j < n; j++)
                {
                    if (i != j && distancia[i][j] < Infinito)
                        proximo[i][j] = j;
                    else
                        proximo[i][j] = -1; // sem caminho conhecido (ainda)
                }
            }
            return proximo;
        }

        // O grafo precisa ser fortemente conexo CONSIDERANDO so os vertices que
        // tem alguma aresta: todos eles precisam alcancar uns aos outros.
        private bool EhFortementeConexo(int n, int[] grauSaida, int[] grauEntrada,
            int[][] distancia)
        {
            for (int i = 0; i < n; i++)
            {
                bool iTemAresta = grauSaida[i] + grauEntrada[i] > 0;
                if (!iTemAresta)
                    continue; // vertice isolado nao atrapalha as arestas

                for (int j = 0; j < n; j++)
                {
                    bool jTemAresta = grauSaida[j] + grauEntrada[j] > 0;
                    if (!jTemAresta)
                        continue;

                    if (distancia[i][j] >= Infinito)
                        return false; // i nao alcanca j
                }
            }
            return true;
        }

        /// <summary>
        /// Equilibra os graus do grafo decidindo QUAIS arestas repetir, gastando
        /// o minimo possivel. Modela como fluxo de custo minimo:
        ///  - quem tem grau de saida MAIOR que o de entrada precisa de novas
        ///    arestas CHEGANDO nele (e um "destino" do fluxo);
        ///  - quem tem grau de entrada MAIOR precisa de novas arestas SAINDO
        ///    dele (e uma "fonte" do fluxo).
        /// O custo de cada unidade e a menor distancia entre os dois vertices.
        /// As arestas repetidas sao adicionadas em "saida".
        /// Retorna o custo total das repeticoes.
        /// </summary>
        private int BalancearGrafo(int n, int[] grauSaida, int[] grauEntrada,
            int[][] distancia, int[][] proximo, List<int>[] saida)
        {
            List<int> fontes = new List<int>(); // entra mais do que sai
            List<int> destinos = new List<int>(); // sai mais do que entra

            int superFonte = n;
            int superDestino = n + 1;
            RedeFluxo rede = new RedeFluxo(n + 2);

            int demandaTotal = 0;
            for (int i = 0; i < n; i++)
            {
                int excesso = grauSaida[i] - grauEntrada[i];
                if (excesso > 0)
                {
                    // precisa receber 'excesso' arestas novas
                    rede.AdicionarArco(i, superDestino, excesso, 0);
                    destinos.Add(i);
                    demandaTotal += excesso;
                }
                else if (excesso < 0)
                {
                    // precisa enviar '-excesso' arestas novas
                    rede.AdicionarArco(superFonte, i, -excesso, 0);
                    fontes.Add(i);
                }
            }

            // Ja estava balanceado: nenhuma repeticao necessaria.
            if (demandaTotal == 0)
                return 0;

            // Liga cada fonte a cada destino, com custo = menor distancia real.
            // Guardamos o id do arco para depois saber quanto fluxo passou nele.
            List<int[]> ligacoes = new List<int[]>(); // {fonte, destino, idArco}
            foreach (int u in fontes)
            {
                foreach (int w in destinos)
                {
                    int id = rede.AdicionarArco(u, w, demandaTotal, distancia[u][w]);
                    ligacoes.Add(new int[] { u, w, id });
                }
            }

            int custo = rede.FluxoCustoMinimo(superFonte, superDestino);

            // Para cada unidade de fluxo de u -> w, repetimos o menor caminho
            // real de u ate w (que pode passar por vertices intermediarios).
            foreach (int[] ligacao in ligacoes)
            {
                int u = ligacao[0];
                int w = ligacao[1];
                int fluxo = rede.FluxoNoArco(ligacao[2]);

                for (int vez = 0; vez < fluxo; vez++)
                {
                    List<int> caminho = ReconstruirCaminho(u, w, proximo);
                    for (int k = 0; k < caminho.Count - 1; k++)
                        saida[caminho[k]].Add(caminho[k + 1]); // aresta repetida
                }
            }

            return custo;
        }

        // Usa a matriz "proximo" para listar todos os vertices entre origem e destino.
        private List<int> ReconstruirCaminho(int origem, int destino, int[][] proximo)
        {
            List<int> caminho = new List<int>();

            if (origem != destino && proximo[origem][destino] == -1)
                return caminho; // sem caminho (nao deveria acontecer aqui)

            int atual = origem;
            caminho.Add(atual);

            while (atual != destino)
            {
                atual = proximo[atual][destino];
                caminho.Add(atual);
            }

            return caminho;
        }

        // Conta quantas arestas (originais + repetidas) existem no grafo.
        private int ContarArcos(List<int>[] saida)
        {
            int total = 0;
            for (int i = 0; i < saida.Length; i++)
                total += saida[i].Count;
            return total;
        }

        // Acha o primeiro vertice que possui pelo menos uma aresta saindo dele.
        private int PrimeiroVerticeComSaida(List<int>[] saida)
        {
            for (int i = 0; i < saida.Length; i++)
                if (saida[i].Count > 0)
                    return i;
            return 0;
        }

        /// <summary>
        /// Algoritmo de Hierholzer: monta um circuito euleriano (passa por todas
        /// as arestas exatamente uma vez) em um grafo direcionado balanceado.
        /// Devolve a sequencia de vertices da rota (o primeiro e igual ao ultimo).
        /// </summary>
        private List<int> Hierholzer(List<int>[] saida, int inicio, int totalArcos)
        {
            int n = saida.Length;
            int[] ponteiro = new int[n]; // proxima aresta ainda nao usada de cada vertice
            Stack<int> pilha = new Stack<int>();
            List<int> circuito = new List<int>();

            pilha.Push(inicio);
            while (pilha.Count > 0)
            {
                int v = pilha.Peek();

                if (ponteiro[v] < saida[v].Count)
                {
                    // ainda ha aresta saindo de v: avanca por ela
                    int proximoVertice = saida[v][ponteiro[v]];
                    ponteiro[v]++;
                    pilha.Push(proximoVertice);
                }
                else
                {
                    // travou: fecha esse vertice na rota e volta
                    circuito.Add(v);
                    pilha.Pop();
                }
            }

            circuito.Reverse();
            return circuito;
        }

        /// <summary>
        /// Rede auxiliar para resolver o FLUXO DE CUSTO MINIMO.
        /// Cada arco tem capacidade e custo, e mantemos um arco reverso (residual)
        /// para permitir "desfazer" decisoes. Usamos o metodo dos caminhos
        /// minimos sucessivos (com Bellman-Ford/SPFA, que aceita custos negativos
        /// nos arcos reversos). E suficiente para os grafos pequenos do estudo.
        /// </summary>
        private class RedeFluxo
        {
            private const int Inf = int.MaxValue / 4;

            private int totalNos;
            private List<int>[] adjacencia;     // adjacencia[v] = ids dos arcos que saem de v
            private List<int> arcoDestino;
            private List<int> arcoCapacidade;
            private List<int> arcoCusto;

            public RedeFluxo(int totalNos)
            {
                this.totalNos = totalNos;
                adjacencia = new List<int>[totalNos];
                for (int i = 0; i < totalNos; i++)
                    adjacencia[i] = new List<int>();

                arcoDestino = new List<int>();
                arcoCapacidade = new List<int>();
                arcoCusto = new List<int>();
            }

            // Adiciona um arco (de -> para) com sua capacidade e custo, e tambem
            // o arco reverso. Devolve o id do arco direto (par; o reverso e id+1).
            public int AdicionarArco(int de, int para, int capacidade, int custo)
            {
                int id = arcoDestino.Count;

                adjacencia[de].Add(id);
                arcoDestino.Add(para);
                arcoCapacidade.Add(capacidade);
                arcoCusto.Add(custo);

                adjacencia[para].Add(id + 1);
                arcoDestino.Add(de);
                arcoCapacidade.Add(0);
                arcoCusto.Add(-custo);

                return id;
            }

            // Quanto de fluxo passou pelo arco direto = capacidade do arco reverso.
            public int FluxoNoArco(int idArcoDireto)
            {
                return arcoCapacidade[idArcoDireto ^ 1];
            }

            // Empurra fluxo da origem ate o destino sempre pelo caminho mais barato.
            // Devolve o custo total do fluxo.
            public int FluxoCustoMinimo(int origem, int destino)
            {
                int custoTotal = 0;

                while (true)
                {
                    int[] dist = new int[totalNos];
                    int[] arcoUsado = new int[totalNos]; // arco pelo qual chegamos ao no
                    bool[] naFila = new bool[totalNos];

                    for (int i = 0; i < totalNos; i++)
                    {
                        dist[i] = Inf;
                        arcoUsado[i] = -1;
                    }
                    dist[origem] = 0;

                    Queue<int> fila = new Queue<int>();
                    fila.Enqueue(origem);
                    naFila[origem] = true;

                    while (fila.Count > 0)
                    {
                        int u = fila.Dequeue();
                        naFila[u] = false;

                        foreach (int id in adjacencia[u])
                        {
                            if (arcoCapacidade[id] <= 0)
                                continue;

                            int v = arcoDestino[id];
                            int novoCusto = dist[u] + arcoCusto[id];
                            if (novoCusto < dist[v])
                            {
                                dist[v] = novoCusto;
                                arcoUsado[v] = id;
                                if (!naFila[v])
                                {
                                    fila.Enqueue(v);
                                    naFila[v] = true;
                                }
                            }
                        }
                    }

                    // Nao chegamos mais ao destino: acabou o fluxo.
                    if (arcoUsado[destino] == -1)
                        break;

                    // Descobre quanto da para empurrar (menor capacidade do caminho).
                    int fluxo = Inf;
                    int no = destino;
                    while (no != origem)
                    {
                        int id = arcoUsado[no];
                        if (arcoCapacidade[id] < fluxo)
                            fluxo = arcoCapacidade[id];
                        no = arcoDestino[id ^ 1];
                    }

                    // Atualiza as capacidades ao longo do caminho.
                    no = destino;
                    while (no != origem)
                    {
                        int id = arcoUsado[no];
                        arcoCapacidade[id] -= fluxo;
                        arcoCapacidade[id ^ 1] += fluxo;
                        no = arcoDestino[id ^ 1];
                    }

                    custoTotal += fluxo * dist[destino];
                }

                return custoTotal;
            }
        }
    }
}
