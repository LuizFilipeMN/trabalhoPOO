using System;
using System.Collections.Generic;

namespace CarteiroChines.Modelos
{
    public class Grafo
    {
        private List<string> vertices;
        private List<Aresta> arestas;

        public Grafo()
        {
            vertices = new List<string>();
            arestas = new List<Aresta>();
        }

        public int QuantidadeVertices
        {
            get { return vertices.Count; }
        }

        public List<string> ObterVertices()
        {
            return new List<string>(vertices);
        }

        public List<Aresta> ObterArestas()
        {
            return new List<Aresta>(arestas);
        }

        public void AdicionarAresta(Aresta aresta)
        {
            if (aresta == null)
                throw new ArgumentNullException(nameof(aresta));

            GarantirVertice(aresta.Origem);
            GarantirVertice(aresta.Destino);
            arestas.Add(aresta);
        }

        private void GarantirVertice(string nome)
        {
            if (!vertices.Contains(nome))
                vertices.Add(nome);
        }

        public int IndiceDoVertice(string nome)
        {
            return vertices.IndexOf(nome);
        }

        public string NomeDoVertice(int indice)
        {
            return vertices[indice];
        }

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
