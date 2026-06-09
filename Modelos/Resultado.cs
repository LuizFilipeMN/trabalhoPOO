using System;
using System.Collections.Generic;

namespace CarteiroChines.Modelos
{
    /// <summary>
    /// Guarda a resposta do problema do Carteiro Chines.
    /// Diz se foi possivel ou nao, qual o caminho encontrado e o custo total.
    /// </summary>
    public class Resultado
    {
        // Indica se foi possivel encontrar um caminho passando por todos os vertices
        public bool Possivel { get; set; }

        // Sequencia de vertices do caminho (ja incluindo os vertices intermediarios)
        public List<string> Caminho { get; set; }

        // Soma dos pesos percorridos
        public int CustoTotal { get; set; }

        // Mensagem explicativa (usada principalmente quando NAO e possivel)
        public string Mensagem { get; set; }

        public Resultado()
        {
            Caminho = new List<string>();
        }

        // Cria rapidamente um resultado de "nao foi possivel"
        public static Resultado CriarImpossivel(string mensagem)
        {
            return new Resultado
            {
                Possivel = false,
                CustoTotal = 0,
                Mensagem = mensagem
            };
        }
    }
}
