using System.Collections.Generic;
using CarteiroChines.Modelos;

namespace CarteiroChines.Servicos
{
    public class ResolvedorCarteiroChines
    {
        private const int Infinito = int.MaxValue / 2;

        public Resultado Resolver(Grafo grafo)
        {
            int n = grafo.QuantidadeVertices;
            List<Aresta> arestas = grafo.ObterArestas();

            if (n == 0 || arestas.Count == 0)
                return Resultado.CriarImpossivel("O grafo nao possui arestas para percorrer.");

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

            int[][] distancia;
            int[][] proximo;
            ExecutarFloydWarshall(grafo, out distancia, out proximo);

            if (!EhFortementeConexo(n, grauSaida, grauEntrada, distancia))
            {
                return Resultado.CriarImpossivel(
                    "Nao e possivel gerar a rota: o grafo direcionado nao e " +
                    "fortemente conexo, entao nao da para percorrer todas as " +
                    "arestas em uma unica rota fechada.");
            }

            List<int>[] saida = new List<int>[n];
            for (int i = 0; i < n; i++)
                saida[i] = new List<int>();

            foreach (Aresta aresta in arestas)
            {
                int origem = grafo.IndiceDoVertice(aresta.Origem);
                int destino = grafo.IndiceDoVertice(aresta.Destino);
                saida[origem].Add(destino);
            }

            int custoRepeticoes = BalancearGrafo(n, grauSaida, grauEntrada, distancia, proximo, saida);

            int totalArcos = ContarArcos(saida);
            int inicio = PrimeiroVerticeComSaida(saida);
            List<int> circuito = Hierholzer(saida, inicio, totalArcos);

            if (circuito.Count != totalArcos + 1)
            {
                return Resultado.CriarImpossivel(
                    "Nao foi possivel montar uma rota que use todas as arestas.");
            }

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

        private void ExecutarFloydWarshall(Grafo grafo, out int[][] distancia, out int[][] proximo)
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
                        proximo[i][j] = -1;
                }
            }
            return proximo;
        }

        private bool EhFortementeConexo(int n, int[] grauSaida, int[] grauEntrada, int[][] distancia)
        {
            for (int i = 0; i < n; i++)
            {
                bool iTemAresta = grauSaida[i] + grauEntrada[i] > 0;
                if (!iTemAresta)
                    continue;

                for (int j = 0; j < n; j++)
                {
                    bool jTemAresta = grauSaida[j] + grauEntrada[j] > 0;
                    if (!jTemAresta)
                        continue;

                    if (distancia[i][j] >= Infinito)
                        return false;
                }
            }
            return true;
        }

        private int BalancearGrafo(int n, int[] grauSaida, int[] grauEntrada,
            int[][] distancia, int[][] proximo, List<int>[] saida)
        {
            int[] enviar = new int[n];
            int[] receber = new int[n];

            for (int i = 0; i < n; i++)
            {
                int excesso = grauSaida[i] - grauEntrada[i];
                if (excesso > 0)
                    receber[i] = excesso;
                else if (excesso < 0)
                    enviar[i] = -excesso;
            }

            int custo = 0;

            while (true)
            {
                int melhorOrigem = -1;
                int melhorDestino = -1;
                int melhorCusto = Infinito;

                for (int u = 0; u < n; u++)
                {
                    if (enviar[u] == 0)
                        continue;

                    for (int w = 0; w < n; w++)
                    {
                        if (receber[w] == 0)
                            continue;

                        if (distancia[u][w] < melhorCusto)
                        {
                            melhorCusto = distancia[u][w];
                            melhorOrigem = u;
                            melhorDestino = w;
                        }
                    }
                }

                if (melhorOrigem == -1)
                    break;

                enviar[melhorOrigem]--;
                receber[melhorDestino]--;
                custo += melhorCusto;

                List<int> caminho = ReconstruirCaminho(melhorOrigem, melhorDestino, proximo);
                for (int k = 0; k < caminho.Count - 1; k++)
                    saida[caminho[k]].Add(caminho[k + 1]);
            }

            return custo;
        }

        private List<int> ReconstruirCaminho(int origem, int destino, int[][] proximo)
        {
            List<int> caminho = new List<int>();

            if (origem != destino && proximo[origem][destino] == -1)
                return caminho;

            int atual = origem;
            caminho.Add(atual);

            while (atual != destino)
            {
                atual = proximo[atual][destino];
                caminho.Add(atual);
            }

            return caminho;
        }

        private int ContarArcos(List<int>[] saida)
        {
            int total = 0;
            for (int i = 0; i < saida.Length; i++)
                total += saida[i].Count;
            return total;
        }

        private int PrimeiroVerticeComSaida(List<int>[] saida)
        {
            for (int i = 0; i < saida.Length; i++)
                if (saida[i].Count > 0)
                    return i;
            return 0;
        }

        private List<int> Hierholzer(List<int>[] saida, int inicio, int totalArcos)
        {
            int n = saida.Length;
            int[] ponteiro = new int[n];
            Stack<int> pilha = new Stack<int>();
            List<int> circuito = new List<int>();

            pilha.Push(inicio);
            while (pilha.Count > 0)
            {
                int v = pilha.Peek();

                if (ponteiro[v] < saida[v].Count)
                {
                    int proximoVertice = saida[v][ponteiro[v]];
                    ponteiro[v]++;
                    pilha.Push(proximoVertice);
                }
                else
                {
                    circuito.Add(v);
                    pilha.Pop();
                }
            }

            circuito.Reverse();
            return circuito;
        }
    }
}
