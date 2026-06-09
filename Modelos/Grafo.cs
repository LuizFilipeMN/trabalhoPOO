using System;
using System.Collections.Generic;
using CarteiroChines.Modelos;

namespace CarteiroChines.Modelos
{
    /// <summary>
    /// Representa um grafo DIRECIONADO.
    /// Guarda a lista de vertices (nomes) e a lista de arestas.
    /// Tambem sabe construir uma matriz de adjacencia para os algoritmos.
    /// </summary>
    public class Grafo
    {
        // Lista com o nome de cada vertice, sem repeticao. Ex: "A", "B", "C"
        private List<string> vertices;

        // Todas as arestas direcionadas do grafo
        private List<Aresta> arestas;

        public Grafo()
        {
            vertices = new List<string>();
            arestas = new List<Aresta>();
        }

        // Quantidade de vertices do grafo
        public int QuantidadeVertices
        {
            get { return vertices.Count; }
        }

        // Devolve uma copia da lista de vertices (para o resto do programa apenas ler)
        public List<string> ObterVertices()
        {
            return new List<string>(vertices);
        }

        // Devolve uma copia da lista de arestas
        public List<Aresta> ObterArestas()
        {
            return new List<Aresta>(arestas);
        }

        /// <summary>
        /// Adiciona uma aresta ao grafo. Se os vertices ainda nao existirem,
        /// eles sao criados automaticamente.
        /// </summary>
        public void AdicionarAresta(Aresta aresta)
        {
            if (aresta == null)
                throw new ArgumentNullException(nameof(aresta));

            GarantirVertice(aresta.Origem);
            GarantirVertice(aresta.Destino);
            arestas.Add(aresta);
        }

        // Cria o vertice na lista somente se ele ainda nao existir
        private void GarantirVertice(string nome)
        {
            if (!vertices.Contains(nome))
                vertices.Add(nome);
        }

        // Descobre a posicao (indice) de um vertice na lista. Retorna -1 se nao existir.
        public int IndiceDoVertice(string nome)
        {
            return vertices.IndexOf(nome);
        }

        public string NomeDoVertice(int indice)
        {
            return vertices[indice];
        }

        /// <summary>
        /// Monta a matriz de adjacencia do grafo.
        /// matriz[i][j] = peso da aresta do vertice i para o vertice j.
        /// Quando NAO existe ligacao direta, usamos o valor "infinito" informado.
        /// A diagonal (i == i) recebe 0 (custo de ficar no mesmo lugar).
        /// </summary>
        public int[][] MontarMatrizAdjacencia(int infinito)
        {
            int n = vertices.Count;
            int[][] matriz = new int[n][];

            for (int i = 0; i < n; i++)
            {
                matriz[i] = new int[n];
                for (int j = 0; j < n; j++)
                {
                    matriz[i][j] = (i == j) ? 0 : infinito;
                }
            }

            // Preenche com as arestas existentes.
            // Se houver duas arestas iguais, mantemos a de menor peso.
            foreach (Aresta aresta in arestas)
            {
                int origem = IndiceDoVertice(aresta.Origem);
                int destino = IndiceDoVertice(aresta.Destino);

                if (aresta.Peso < matriz[origem][destino])
                    matriz[origem][destino] = aresta.Peso;
            }

            return matriz;
        }
    }
}
