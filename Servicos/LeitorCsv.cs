using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using CarteiroChines.Modelos;

namespace CarteiroChines.Servicos
{
    /// <summary>
    /// Responsavel por LER um arquivo CSV e transformar em um objeto Grafo.
    ///
    /// Formato esperado do CSV (uma aresta por linha):
    ///     origem,destino,peso
    ///     A,B,3
    ///     B,C,1
    ///
    /// A primeira linha pode ser um cabecalho ("origem,destino,peso"),
    /// que sera ignorado automaticamente.
    /// </summary>
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

                // Pula linhas em branco
                if (linha.Length == 0)
                    continue;

                // Quebra a linha pelos separadores aceitos (virgula ou ponto e virgula)
                string[] colunas = linha.Split(new char[] { ',', ';' });

                if (colunas.Length < 3)
                    throw new FormatException(
                        "Linha " + numeroLinha + " invalida (esperado 'origem,destino,peso'): " + linhaOriginal);

                string origem = colunas[0].Trim();
                string destino = colunas[1].Trim();
                string textoPeso = colunas[2].Trim();

                // Se a primeira linha for o cabecalho, ela nao tem um numero no peso -> ignoramos
                int peso;
                bool pesoValido = int.TryParse(textoPeso, NumberStyles.Integer,
                    CultureInfo.InvariantCulture, out peso);

                if (!pesoValido)
                {
                    if (numeroLinha == 1)
                        continue; // era o cabecalho, tudo bem

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
