using System.Collections.Generic;

namespace CarteiroChines.Modelos
{
    public class Resultado
    {
        public bool Possivel { get; set; }
        public List<string> Caminho { get; set; }
        public int CustoTotal { get; set; }
        public string Mensagem { get; set; }

        public Resultado()
        {
            Caminho = new List<string>();
        }

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
