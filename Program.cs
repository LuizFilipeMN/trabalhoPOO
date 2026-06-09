using System;
using System.Collections.Generic;
using System.IO;
using CarteiroChines.Modelos;
using CarteiroChines.Servicos;

namespace CarteiroChines
{
    /// <summary>
    /// Classe principal. Faz a interacao com o usuario via console:
    /// 1) pergunta o caminho do arquivo CSV;
    /// 2) le o grafo;
    /// 3) mostra os dados lidos;
    /// 4) calcula e exibe a menor rota que passa por todas as arestas.
    /// </summary>
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("===========================================");
            Console.WriteLine("   CARTEIRO CHINES - Menor rota fechada");
            Console.WriteLine("   (passa por todas as arestas do grafo)");
            Console.WriteLine("===========================================");
            Console.WriteLine();

            try
            {
                // O usuario pode passar o caminho do arquivo como argumento
                // ou digitar quando for solicitado.
                string caminhoArquivo = ObterCaminhoArquivo(args);

                // ---- 1) Leitura do CSV ----
                LeitorCsv leitor = new LeitorCsv();
                Grafo grafo = leitor.Ler(caminhoArquivo);

                ExibirGrafo(grafo);

                // ---- 2) Resolucao do problema (Carteiro Chines) ----
                ResolvedorCarteiroChines resolvedor = new ResolvedorCarteiroChines();
                Resultado resultado = resolvedor.Resolver(grafo);

                // ---- 3) Exibicao do resultado ----
                ExibirResultado(resultado);
            }
            catch (FileNotFoundException erro)
            {
                Console.WriteLine();
                Console.WriteLine("[ERRO] " + erro.Message);
            }
            catch (FormatException erro)
            {
                Console.WriteLine();
                Console.WriteLine("[ERRO de formato no CSV] " + erro.Message);
            }
            catch (Exception erro)
            {
                Console.WriteLine();
                Console.WriteLine("[ERRO inesperado] " + erro.Message);
            }

            Console.WriteLine();
            Console.WriteLine("Pressione ENTER para sair...");
            Console.ReadLine();
        }

        // Decide de onde vem o caminho do arquivo CSV.
        private static string ObterCaminhoArquivo(string[] args)
        {
            if (args != null && args.Length > 0 && !string.IsNullOrWhiteSpace(args[0]))
                return args[0];

            Console.Write("Digite o caminho do arquivo CSV (ENTER para usar 'grafo.csv'): ");
            string entrada = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(entrada))
                return "grafo.csv"; // arquivo de exemplo que acompanha o projeto

            return entrada.Trim();
        }

        // Mostra os vertices e as arestas que foram lidos do arquivo.
        private static void ExibirGrafo(Grafo grafo)
        {
            Console.WriteLine();
            Console.WriteLine("Grafo lido do arquivo:");

            List<string> vertices = grafo.ObterVertices();
            Console.WriteLine("  Vertices (" + vertices.Count + "): " + string.Join(", ", vertices));

            Console.WriteLine("  Arestas:");
            foreach (Aresta aresta in grafo.ObterArestas())
            {
                Console.WriteLine("    " + aresta.ToString());
            }
            Console.WriteLine();
        }

        // Mostra o resultado final de forma amigavel.
        private static void ExibirResultado(Resultado resultado)
        {
            Console.WriteLine("-------------------------------------------");
            Console.WriteLine("RESULTADO:");

            if (!resultado.Possivel)
            {
                Console.WriteLine("  NAO foi possivel gerar a rota.");
                Console.WriteLine("  Motivo: " + resultado.Mensagem);
                return;
            }

            Console.WriteLine("  Menor rota que passa por todas as arestas (rota fechada):");
            Console.WriteLine("    " + string.Join(" -> ", resultado.Caminho));
            Console.WriteLine("  Custo total: " + resultado.CustoTotal);
            Console.WriteLine("  Observacao: " + resultado.Mensagem);
        }
    }
}
