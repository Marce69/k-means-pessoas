using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace K_means_TrabalhoII
{
    class Pessoa
    {
        int id;
        String Sexo;
        int idade;
        String ocupacao;
        String codigo_postal;

        public Pessoa(int id, String sexo, int idade, string ocupacao, String codigo_postal)
        {
            this.id = id;
            Sexo = sexo;
            this.idade = idade;
            this.ocupacao = ocupacao;
            this.codigo_postal = codigo_postal;
        }

        public int Id
        {
            get
            {
                return id;
            }

            set
            {
                id = value;
            }
        }

        public String Sexo1
        {
            get
            {
                return Sexo;
            }

            set
            {
                Sexo = value;
            }
        }

        public int Idade
        {
            get
            {
                return idade;
            }

            set
            {
                idade = value;
            }
        }

        public string Ocupacao
        {
            get
            {
                return ocupacao;
            }

            set
            {
                ocupacao = value;
            }
        }

        public String Codigo_postal
        {
            get
            {
                return codigo_postal;
            }

            set
            {
                codigo_postal = value;
            }
        }
    }
}
