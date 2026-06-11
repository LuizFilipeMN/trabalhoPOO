using System;
using System.Globalization;
using System.IO;
using CarteiroChines.Modelos;

namespace CarteiroChines.Servicos
{
    public class LeitorCsv
    {
        public Grafo Ler(string caminhoArquivo)
        {
            if (!File.Exists(caminhoArquivo))
                throw new FileNotFoundException("Arquivo CSV nao encontrado: " + caminhoArquivo);

            Grafo grafo = new Grafo();
            string[] linhas = File.ReadAllLines(caminhoArquivo);

            int numeroLinha = 0;
            foreach (string linhaOriginal in linhas)
            {
                numeroLinha++;
                string linha = linhaOriginal.Trim();

                if (linha.Length == 0)
                    continue;

                string[] colunas = linha.Split(new char[] { ',', ';' });

                if (colunas.Length < 3)
                    throw new FormatException(
                        "Linha " + numeroLinha + " invalida (esperado 'origem,destino,peso'): " + linhaOriginal);

                string origem = colunas[0].Trim();
                string destino = colunas[1].Trim();
                string textoPeso = colunas[2].Trim();

                int peso;
                bool pesoValido = int.TryParse(textoPeso, NumberStyles.Integer,
                    CultureInfo.InvariantCulture, out peso);

                if (!pesoValido)
                {
                    if (numeroLinha == 1)
                        continue;

                    throw new FormatException(
                        "Peso invalido na linha " + numeroLinha + ": '" + textoPeso + "'");
                }

                Aresta aresta = new Aresta(origem, destino, peso);
                grafo.AdicionarAresta(aresta);
            }

            if (grafo.QuantidadeVertices == 0)
                throw new FormatException("O arquivo CSV nao gerou nenhum vertice. Ele esta vazio?");

            return grafo;
        }
    }
}
