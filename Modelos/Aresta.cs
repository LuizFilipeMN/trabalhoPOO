using System;

namespace CarteiroChines.Modelos
{
    /// <summary>
    /// Representa uma aresta (ligacao) de um grafo direcionado.
    /// Vai de "Origem" para "Destino" e possui um "Peso" (custo/distancia).
    /// Por ser direcionada, a aresta A -> B nao significa que existe B -> A.
    /// </summary>
    public class Aresta
    {
        public string Origem { get; private set; }
        public string Destino { get; private set; }
        public int Peso { get; private set; }

        public Aresta(string origem, string destino, int peso)
        {
            // Validacoes basicas para evitar dados invalidos
            if (string.IsNullOrWhiteSpace(origem))
                throw new ArgumentException("A origem da aresta nao pode ser vazia.");

            if (string.IsNullOrWhiteSpace(destino))
                throw new ArgumentException("O destino da aresta nao pode ser vazio.");

            if (peso < 0)
                throw new ArgumentException("O peso da aresta nao pode ser negativo.");

            Origem = origem.Trim();
            Destino = destino.Trim();
            Peso = peso;
        }

        public override string ToString()
        {
            return Origem + " -> " + Destino + " (peso " + Peso + ")";
        }
    }
}
