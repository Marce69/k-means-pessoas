using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace K_means_TrabalhoII
{
    class Program
    {
        // Todos os metodos são estatico pq quando eu começo a separar só da erro
        // Tentei add uma biblioteca para facilitar o calculo da distancia mas não deu certo e 
        // eu tambem não consegui remover ela sem bugar o programa então....
        static void Main(string[] args)
        {
            Console.WriteLine("\nInicializacao do k-means:\n");

            // inicia o vetor onde as idades serão colocadas
            int[][] listaPessoa = new int[6040][];
            int count = 0;
            //abre lista de pessoas que deve estar na pasta bin/debug/grupo de pessoas.dat
            StreamReader arquivo = null;
            String caminho = "Grupo de Pessoas.dat";
            List<Pessoa> Pessoas = new List<Pessoa>();

            try
            {
                arquivo = new System.IO.StreamReader(@caminho);
            }
            catch (IOException e)
            {
                Console.WriteLine("Erro de abertura do arquivo " + caminho + "\n" + e);
            }
            catch (Exception e)
            {
                Console.WriteLine("Erro do programa ou falha da tabela de simbolos\n" + e);
            }
            //carrega linha e monta o objeto lista pessoa
            String linha;
            while ((linha = arquivo.ReadLine()) != null)
            {
                char[] separadores = new char[] { ':', ':' };
                String[] dadoslinha = linha.Split(separadores, StringSplitOptions.RemoveEmptyEntries);
                listaPessoa[count] = new int[] { Convert.ToInt32(dadoslinha[2]) };
                Pessoa aux = new Pessoa(Convert.ToInt32(dadoslinha[0]), dadoslinha[1], Convert.ToInt32(dadoslinha[2]), dadoslinha[3], dadoslinha[4]);
                Pessoas.Add(aux);
                count++;
            }

            try
            {
                arquivo.Close();
                arquivo.Dispose();
            }
            catch (IOException e)
            {
                Console.WriteLine("Erro ao fechar arquivo\n" + e);
            }

            Console.WriteLine("Mostra os dados da lista de pessoas:\n");
            Console.WriteLine("    userID::Sexo::Idade::Ocupacao::codigoPostal");
            Console.WriteLine("-------------------");
            ShowData(listaPessoa, 1, true, true);
            //Passa o nº de grupos que será trabalhado
            int numGrupo = 7;
            Console.WriteLine("\nNumero de Grupos no k-means: " + numGrupo);

            int[] agrupamento = Agrupar(listaPessoa, numGrupo); // comeca a magica do k-means 

            //Verica o tamanho do vetor que foi retornado
            //Console.WriteLine(clustering.Length);

            Console.WriteLine("\nK-means Agrupamento Completo\n");
            //Mostra o vetor de valores que foi usado como comparação no inicio do k-means
            //Console.WriteLine("Vetor inicial (random):\n");
            //ShowVector(clustering, true);

            Console.WriteLine("Agrupamento de pessoas por idade:\n");
            MostraAgrupamento(listaPessoa, agrupamento, numGrupo, Pessoas, 1);

            Console.WriteLine("\nFim da execução do k-means\n");
            Console.ReadLine();

        }


        public static int[] Agrupar(int[][] listaPessoa, int numGrupo)
        {
            int[][] data = listaPessoa; // incicia o agrupamento

            bool mudanca = true; // olha se houve mudanças na ultima movimentação
            bool sucesso = true; // todos os caminhos forão processados?? (nenhum cluster com 0 caminhos)

            // init clustering[] para iniciar os grupos
            int[] agrupamento = InitAgrupamento(data.Length, numGrupo, 0); // semi-random inicialização
            double[][] means = Alocacao(numGrupo, data[0].Length);

            int maxCount = data.Length * 10; // faz rechecagem do agrupamento encontrado varias vezes por segurança
            int ct = 0;
            while (mudanca == true && sucesso == true && ct < maxCount)
            {
                ++ct; // numero de vezes que fez rechecagem
                sucesso = AtualizaMeans(data, agrupamento, means); // computa novo grupo de caminhos se possivel senão passa direto (pode dar erro as vezes)
                mudanca = AtualizaGrupos(data, agrupamento, means); // (re)atribui valores a grupos.
            }

            return agrupamento;
        }

        private static int[] InitAgrupamento(int numTuples, int numGrupo, int randomSeed)
        {
            // inicia Grupo mais ou menos randomicamente (cada grupo tem ao menos um valor nele)
            // means serve para designar novo valor ao grupo
            Random random = new Random(randomSeed);
            int[] clustering = new int[numTuples];
            for (int i = 0; i < numGrupo; ++i) // confirma que cada grupo tem ao menos um valor nele
                clustering[i] = i;
            for (int i = numGrupo; i < clustering.Length; ++i)
                clustering[i] = random.Next(0, numGrupo);
            //teste
            //foreach (float aux in clustering) { Console.WriteLine("pontos: " + aux); }
            //return clustering = new int[] { 60,61,62,63,64,65,66};
            return clustering;
        }

        private static double[][] Alocacao(int numGrupos, int numColunas)
        {
            // convenience matrix allocator for Cluster()
            double[][] resultado = new double[numGrupos][];
            for (int k = 0; k < numGrupos; ++k)
                resultado[k] = new double[numColunas];
            return resultado;
        }

        private static bool AtualizaMeans(int[][] data, int[] grupos, double[][] means)
        {
            // retorna falso se grupo não tiver nenhum valor dentro dele
            // parametro means[][] é só para referencia

            int numGrupos = means.Length;
            int[] clusterCounts = new int[numGrupos];
            for (int i = 0; i < data.Length; ++i)
            {
                int grupo = grupos[i];
                ++clusterCounts[grupo];
            }

            for (int k = 0; k < numGrupos; ++k)
                if (clusterCounts[k] == 0)
                    return false;

            for (int k = 0; k < means.Length; ++k)
                for (int j = 0; j < means[k].Length; ++j)
                    means[k][j] = 0.0;

            for (int i = 0; i < data.Length; ++i)
            {
                int cluster = grupos[i];
                for (int j = 0; j < data[i].Length; ++j)
                    means[cluster][j] += data[i][j]; //soma acumulada
            }

            for (int k = 0; k < means.Length; ++k)
                for (int j = 0; j < means[k].Length; ++j)
                    means[k][j] /= clusterCounts[k]; // não verifico divisão por zero, se tiver vai dar erro
            return true;
        }

        private static bool AtualizaGrupos(int[][] data, int[] Grupos, double[][] means)
        {
            // (RE)Atribui valores ao grupo de acordo com a distancia do means
            // retorna falso se não houver mudanças OU
            // se á mudança resultara em um vetor vazio 

            int numGrupo = means.Length;
            bool mudou = false;

            int[] NovoGrupo = new int[Grupos.Length];
            Array.Copy(Grupos, NovoGrupo, Grupos.Length);

            double[] distancia = new double[numGrupo]; // distancia de idade para outras idades

            for (int i = 0; i < data.Length; ++i) // anda por cada idade
            {
                for (int k = 0; k < numGrupo; ++k)
                    distancia[k] = Distancia(data[i], means[k]); // Calcula distancia da idade para todas as outras idades

                int novoGrupoID = MinIndex(distancia); // Proucura ID_mean mais perto
                if (novoGrupoID != NovoGrupo[i])
                {
                    mudou = true;
                    NovoGrupo[i] = novoGrupoID; // atualiza valor
                }
            }

            if (mudou == false)
                return false; // nenhuma mudanca

            // Conta Grupos[]
            int[] GrupoCounts = new int[numGrupo];
            for (int i = 0; i < data.Length; ++i)
            {
                int cluster = NovoGrupo[i];
                ++GrupoCounts[cluster];
            }

            for (int k = 0; k < numGrupo; ++k)
                if (GrupoCounts[k] == 0)
                    return false; // grupo mau. nenhuma mudanca

            Array.Copy(NovoGrupo, Grupos, NovoGrupo.Length); // Grupo atualizado
            return true; // tem pelo menos uma mudança e não é vazio
        }

        private static double Distancia(int[] tuple, double[] mean)
        {
            // distancia Euclidiana entre dois vetores para Grupos Atualizados()
            // peguei na NET 
            double sumSquaredDiffs = 0.0;
            for (int j = 0; j < tuple.Length; ++j)
                sumSquaredDiffs += Math.Pow((tuple[j] - mean[j]), 2);
            return Math.Sqrt(sumSquaredDiffs);
        }

        private static int MinIndex(double[] distancia)
        {
            // index de menor valor no vetor
            // ajuda para atualizarGrupo
            int indexMin = 0;
            double menorDist = distancia[0];
            for (int k = 0; k < distancia.Length; ++k)
            {
                if (distancia[k] < menorDist)
                {
                    menorDist = distancia[k];
                    indexMin = k;
                }
            }
            return indexMin;
        }

        //Mostra os valores da lista
        static void ShowData(int[][] data, int decimals, bool indices, bool newLine)
        {
            for (int i = 0; i < data.Length; ++i)
            {
                if (indices) Console.Write(i.ToString().PadLeft(3) + " ");
                for (int j = 0; j < data[i].Length; ++j)
                {
                    if (data[i][j] >= 0.0) Console.Write(" ");
                    Console.Write(data[i][j] + " ");
                }
                Console.WriteLine("");
            }
            if (newLine) Console.WriteLine("");
        }

        static void MostraVetor(int[] vector, bool newLine)
        {
            for (int i = 0; i < vector.Length; ++i)
                Console.Write(vector[i] + " ");
            if (newLine) Console.WriteLine("\n");
        }

        static void MostraAgrupamento(int[][] data, int[] Agrupamento, int numGrupos, List<Pessoa> Pessoas, int decimals)
        {
            for (int k = 0; k < numGrupos; ++k)
            {
                int count = 0;
                Console.WriteLine("===================");

                for (int i = 0; i < data.Length; ++i)
                {
                    int GrupoID = Agrupamento[i];
                    if (GrupoID != k) continue;
                    Console.Write(i.ToString().PadLeft(3) + " ");
                    count++;
                    for (int j = 0; j < data[i].Length; ++j)
                    {
                        if (data[i][j] >= 0) { Console.Write(" "); }
                        Console.Write(data[i][j] + " ");
                    }
                    Console.WriteLine("");
                }
                Console.WriteLine("===================");

                string path = @"" + k + "_" + count + "_" + k + ".txt";
                if (!File.Exists(path))
                {
                    // Cria o arquivo para escrever
                    using (StreamWriter sw = File.CreateText(path))
                    {
                        sw.WriteLine("===================");
                        for (int i = 0; i < data.Length; ++i)
                        {
                            int GrupoID = Agrupamento[i];
                            if (GrupoID != k) continue;
                            sw.Write(Pessoas[i].Id + "::" + Pessoas[i].Sexo1 + "::");
                            for (int j = 0; j < data[i].Length; ++j)
                            {
                                if (data[i][j] >= 0) { Console.Write(" "); }
                                sw.Write(data[i][j] + "::" + Pessoas[i].Ocupacao + "::" + Pessoas[i].Codigo_postal);
                            }
                            sw.WriteLine("");
                        }
                        sw.WriteLine("===================");
                    }
                }
            }
        }
    }
}
