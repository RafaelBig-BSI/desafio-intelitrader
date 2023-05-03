using System;
using System.Collections.Generic;
using System.IO;

namespace Desafio_Intelitrader_RafaelBigeschi
{
    class Program
    {
        static StreamWriter RetornaArquivo(string path)
        {
            try
            {
                StreamWriter streamWriter = new StreamWriter(path);
                return streamWriter;
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return null;
        }
        static Boolean InserirCodigosVerificados(List<string> codigos_produtos, string codigoP)
        {
            int i = 0;
            while (i < codigos_produtos.Count && codigoP != codigos_produtos[i])
                i++;

            if (i == codigos_produtos.Count)
            {
                codigos_produtos.Add(codigoP);
                return true;
            }

            return false; //o código já foi verificado (já está na lista).
        }

        static void GerarArquivoTransfere(string arqProdutos, string cod_produto, int qtVendas, StreamWriter arqTransfere)
        {
            string[] arrayProdutos, auxP;
            int qtCO = 0, qtMin = 0, estoqueAposVendas = 0, necessidade = 0, transfArmCO = 0;
            
            arrayProdutos = arqProdutos.Split('\n');            

            for(int i=0; i < arrayProdutos.Length-1; i++)
            {
                auxP = arrayProdutos[i].Split(";");

                if(auxP[0] == cod_produto)
                {
                    qtCO = Convert.ToInt32(auxP[1]);
                    qtMin = Convert.ToInt32(auxP[2]);
                    estoqueAposVendas = qtCO - qtVendas;

                    if (estoqueAposVendas < qtMin)
                        necessidade = qtMin - estoqueAposVendas;

                    if (necessidade >= 1 && necessidade <= 10)
                        transfArmCO = 10;
                    else
                        transfArmCO = necessidade;

                    try
                    {
                        arqTransfere.WriteLine(cod_produto + "\t\t " + qtCO + "\t " + qtMin + " \t " + qtVendas + "\t\t\t" + estoqueAposVendas + "\t\t\t\t" + necessidade + "\t\t\t\t\t" + transfArmCO);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }                
            }
        }

        static void VerificarDivergencias(StreamWriter arqDivergencias, string arqVendas, string arqProdutos)
        {
            string[] arrayVendas = arqVendas.Split("\n");
            string[] auxVendas;
            int linha = 1;

            for(int i=0; i < arrayVendas.Length-1; i++)
            {
                auxVendas = arrayVendas[i].Split(";");

                if (arqProdutos.Contains(auxVendas[0]))
                {
                    //Verifica a situação da venda
                    switch(auxVendas[2])
                    {
                        case "135": arqDivergencias.WriteLine("Linha " + linha + " - Venda cancelada"); break;

                        case "190": arqDivergencias.WriteLine("Linha " + linha + " - Venda não finalizada"); break;

                        case "999": arqDivergencias.WriteLine("Linha " + linha + " - Erro desconhecido. Acionar equipe de TI"); break;
                    }
                    linha++;
                }
                else
                {
                    // Código inexistente no arquivo de produtos
                    arqDivergencias.WriteLine("Linha " + linha + " - Código de Produto não encontrado " + auxVendas[0]);
                    linha++;
                }
            }            
        }

        static void TotalizarQtdesVendidas(StreamWriter arqTotCanais, string arqVendas)
        {
            string[] arrayVendas = arqVendas.Split("\n");
            string[] auxVendas;
            int totRepresentantes = 0, totWebsite = 0, totAndroid = 0, totApple = 0;
           

            for(int i=0; i < arrayVendas.Length-1; i++)
            {
                auxVendas = arrayVendas[i].Split(";");

                if(auxVendas[2].Equals("100") || auxVendas[2].Equals("102"))
                {
                    switch (auxVendas[3])
                    {
                        case "1": totRepresentantes += Convert.ToInt32(auxVendas[1]); break;

                        case "2": totWebsite += Convert.ToInt32(auxVendas[1]); break;

                        case "3": totAndroid += Convert.ToInt32(auxVendas[1]); break;

                        case "4": totApple += Convert.ToInt32(auxVendas[1]); break;

                    }
                }                
            }

            arqTotCanais.WriteLine("1 - Representantes \t\t " + totRepresentantes);
            arqTotCanais.WriteLine("2 - Website \t\t     " + totWebsite);
            arqTotCanais.WriteLine("3 - App móvel Android \t " + totAndroid);
            arqTotCanais.WriteLine("4 - App móvel iPhone \t " + totApple);
        }

        static void ConferirVendasConfirmadas(string arqProdutos, string arqVendas) // situação: 100 ou 102
        {
            int qtVendas = 0;
            string[] arrayVendas;
            string[] auxV, divideAV;
            List<string> codigos_verificados = new List<string>();
            StreamWriter arqTransfere = null, arqDivergencias = null, arqTotCanais = null;

            String path = System.AppDomain.CurrentDomain.BaseDirectory.ToString();
            string[] newPath = path.Split(@"\");
            string originalPath = "";
            for (int i = 0; i < newPath.Length; i++)
            {
                if (!newPath[i].Equals(""))
                {
                    if (newPath[i] != "bin" && newPath[i] != "Debug" && newPath[i] != "net5.0")
                        originalPath += newPath[i] + @"\";
                }
            }
            originalPath += @"arquivos";

            // Arquivo Transfere
            try
            {
                string pathArqTransfere = originalPath;
                pathArqTransfere += @"\transfere.txt";

                arqTransfere = RetornaArquivo(pathArqTransfere);
                arqTransfere.WriteLine("Necessidade de Transferência Armazém para CO");
                arqTransfere.WriteLine("\n");
                arqTransfere.WriteLine("Produto \t QtCO \t QtMin \t QtVendas \t Est.apósVendas \t Necess. \t Trasnf. de Arm p/ CO");
            }
            catch (Exception e)
            {
                Console.WriteLine("Erro (arq Transfere): " + e.Message);
            }

            //Arquivo Divergencias
            try
            {
                string pathArqDivergencias = originalPath;
                pathArqDivergencias += @"\divergencias.txt";
                arqDivergencias = RetornaArquivo(pathArqDivergencias);
            }
            catch (Exception e)
            {
                Console.WriteLine("Erro (arq Divergencias): " + e.Message);
            }

            //Arquivo TotCanais
            try
            {
                string pathArqTotCanais = originalPath;
                pathArqTotCanais += @"\totcanais.txt";
                arqTotCanais = RetornaArquivo(pathArqTotCanais);
            }
            catch (Exception e)
            {
                Console.WriteLine("Erro (arq TotCanais): " + e.Message);
            }

            // Separar os dados pelo \n
            arrayVendas = arqVendas.Split("\n");        

            // Somar qtVendas dos dados
            for(int i=0; i < arrayVendas.Length-1; i++)
            {
                auxV = arrayVendas[i].Split(";");                

                int situacao_i = Convert.ToInt32(auxV[2]);

                if (situacao_i == 100 || situacao_i == 102)
                {
                    Boolean result = InserirCodigosVerificados(codigos_verificados, auxV[0]);
                    if (result)
                    {
                        qtVendas += Convert.ToInt32(auxV[1]);

                        for (int j = i + 1; j < arrayVendas.Length; j++)
                        {
                            divideAV = arrayVendas[j].Split(";");

                            // Verificar se o codigo do produto é igual
                            if (auxV[0] == divideAV[0])
                            {
                                int situacao_j = Convert.ToInt32(divideAV[2]);

                                if (situacao_j == 100 || situacao_j == 102)
                                    qtVendas += Convert.ToInt32(divideAV[1]);
                            }
                        }
                        GerarArquivoTransfere(arqProdutos, auxV[0], qtVendas, arqTransfere);
                        qtVendas = 0;
                    }
                }
            }            
            arqTransfere.Close();           

            VerificarDivergencias(arqDivergencias, arqVendas, arqProdutos);
            arqDivergencias.Close();

            TotalizarQtdesVendidas(arqTotCanais, arqVendas);
            arqTotCanais.Close();
        }

        static void CasoTesteUm()
        {
            String path = System.AppDomain.CurrentDomain.BaseDirectory.ToString();
            string[] newPath = path.Split(@"\");
            string originalPath = "";
            for (int i = 0; i < newPath.Length; i++)
            {
                if (!newPath[i].Equals(""))
                {
                    if (newPath[i] != "bin" && newPath[i] != "Debug" && newPath[i] != "net5.0")
                        originalPath += newPath[i] + @"\";
                }
            }
            originalPath += @"arquivos";

            string pathProdutos = originalPath;
            pathProdutos += @"\c1_produtos.txt";

            string pathVendas = originalPath;
            pathVendas += @"\c1_vendas.txt";

            try
            {
                string linha_produtos, linha_vendas;
                string arq_produtos = "", arq_vendas = "";

                using (StreamReader readerProd = new StreamReader(pathProdutos))
                {
                    linha_produtos = readerProd.ReadLine();
                    while (linha_produtos != null)
                    {
                        arq_produtos += linha_produtos;
                        arq_produtos += '\n';
                        linha_produtos = readerProd.ReadLine();
                    }
                    readerProd.Close();
                }

                using (StreamReader readerVendas = new StreamReader(pathVendas))
                {
                    linha_vendas = readerVendas.ReadLine();
                    while (linha_vendas != null)
                    {
                        arq_vendas += linha_vendas;
                        arq_vendas += '\n';
                        linha_vendas = readerVendas.ReadLine();
                    }
                    readerVendas.Close();
                }

                ConferirVendasConfirmadas(arq_produtos, arq_vendas);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        static void CasoTesteDois()
        {
            String path = System.AppDomain.CurrentDomain.BaseDirectory.ToString();
            string[] newPath = path.Split(@"\");
            string originalPath = "";
            for (int i = 0; i < newPath.Length; i++)
            {
                if (!newPath[i].Equals(""))
                {
                    if (newPath[i] != "bin" && newPath[i] != "Debug" && newPath[i] != "net5.0")
                        originalPath += newPath[i] + @"\";
                }
            }
            originalPath += @"arquivos";

            string pathProdutos = originalPath;
            pathProdutos += @"\c2_produtos.txt";

            string pathVendas = originalPath;
            pathVendas += @"\c2_vendas.txt";

            try
            {
                string linha_produtos, linha_vendas;
                string arq_produtos = "", arq_vendas = "";

                using (StreamReader readerProd = new StreamReader(pathProdutos))
                {
                    linha_produtos = readerProd.ReadLine();
                    while (linha_produtos != null)
                    {
                        arq_produtos += linha_produtos;
                        arq_produtos += '\n';
                        linha_produtos = readerProd.ReadLine();
                    }
                    readerProd.Close();
                }

                using (StreamReader readerVendas = new StreamReader(pathVendas))
                {
                    linha_vendas = readerVendas.ReadLine();
                    while (linha_vendas != null)
                    {
                        arq_vendas += linha_vendas;
                        arq_vendas += '\n';
                        linha_vendas = readerVendas.ReadLine();
                    }
                    readerVendas.Close();
                }

                ConferirVendasConfirmadas(arq_produtos, arq_vendas);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        static void Main(string[] args)
        {
            Console.WriteLine("");
            Console.WriteLine("");

            Console.WriteLine("\t\t **** Tecle [A + ENTER] - Caso Teste 1 ou Tecle [B + ENTER] - Caso Teste 2 ****");

            if(Console.ReadKey().Key == ConsoleKey.A)
            {
               CasoTesteUm();
                Console.WriteLine("\t -> Arquivo [arquivos/transfere.txt] (re)criado com dados do caso 1");
                Console.WriteLine("\t -> Arquivo [arquivos/divergencias.txt] (re)criado com dados do caso 1");
                Console.WriteLine("\t -> Arquivo [arquivos/totcanais.txt] (re)criado com dados do caso 1");
            }           
            else
            {
                CasoTesteDois();
                Console.WriteLine("\t -> Arquivo [arquivos/transfere.txt] (re)criado com dados do caso 2");
                Console.WriteLine("\t -> Arquivo [arquivos/divergencias.txt] (re)criado com dados do caso 2");
                Console.WriteLine("\t -> Arquivo [arquivos/totcanais.txt] (re)criado com dados do caso 2");
            }
        }
    }
}
