using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.IO;

namespace Adricad
{
    class ConexaoBanco
    {
        
        //1. STRINGS PARA CONEXAÇÃO COM O BANCO
        private const string dsConexao = "Data Source=Banco.db";
        private const string sNomeBanco = "Banco.db";

        public static long UltimoID = 0;

        //2. CRIAÇÃO DAS TABELAS

        //2.1. Pessoas
        private const string SQLCriarTabelaPessoa = 
            "CREATE TABLE IF NOT EXISTS PESSOA ([ID] INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL, " +
                                               "[NOME] VARCHAR (50), " +
                                               "[CPF] CHAR(11), " +
                                               "[RG] CHAR(10), " +
                                               "[DATANASCIMENTO] DATE, " +
                                               "[SEXO] CHAR(1), " +
                                               "[ENDERECO_ID] INT, " +
                                               "[CONTATO_ID] INT, " +
                                               "CONSTRAINT FK_ENDERECOPESSOA FOREIGN KEY(ENDERECO_ID) REFERENCES ENDERECO(ID), " +
                                               "CONSTRAINT FK_CONTATOPESSOA FOREIGN KEY(CONTATO_ID) REFERENCES CONTATO(ID) " +
                                               ");";

        //2.2. Endereço
        private const string SQLCriarTabelaEndereco = 
            "CREATE TABLE IF NOT EXISTS ENDERECO ([ID] INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL, " +
                                                 "[CEP] CHAR(9), " +
                                                 "[PAIS] VARCHAR(20), " +                                     
                                                 "[UF] VARCHAR(20), " +
                                                 "[CIDADE] VARCHAR(30), " +
                                                 "[BAIRRO] VARCHAR(30), " +
                                                 "[RUA] VARCHAR(50), " +
                                                 "[NUMERO] INT, " +
                                                 "[COMPLEMENTO] VARCHAR(70) " +
                                                 ");";

        //2.3 Contato
        private const string SQLCriarTabelaContato =
            "CREATE TABLE IF NOT EXISTS CONTATO([ID] INTEGER PRIMARY KEY AUTOINCREMENT, " +
                                               "[TEL] CHAR(10), " +
                                               "[CEL] CHAR(11), " +
                                               "[EMAIL] VARCHAR(40) " +
                                               ");";

        //3. INSERÇÃO DE DADOS

        //3.1. Pessoas COM contato
        private const string SQLInserirPessoaComContato =
            "INSERT INTO PESSOA (NOME, CPF, RG, DATANASCIMENTO, SEXO, ENDERECO_ID, CONTATO_ID) " +
            "VALUES (@NOME, @CPF, @RG, @DATANASC, @SEXO, (SELECT MAX(ID) FROM ENDERECO), (SELECT MAX(ID) FROM CONTATO)" +
            ")";

        //3.2. Endereço
        private const string SQLInserirEndereco =
            "INSERT INTO ENDERECO (CEP, PAIS, UF, CIDADE, BAIRRO, RUA, NUMERO, COMPLEMENTO) " +
            "VALUES (@CEP, @PAIS, @UF, @CIDADE, @BAIRRO, @RUA, @NUMERO, @COMPLEMENTO)";

        //3.3 Contatos
        private const string SQLInserirContato =
            "INSERT INTO CONTATO (TEL, CEL, EMAIL) VALUES (@TEL, @CEL, @EMAIL)";


        //4.0. SELEÇÃO DE DADOS

        //4.1 Dados parciais
        private const string SQLSelecionarDadosParciais = "SELECT ID, NOME FROM PESSOA ORDER BY ID DESC LIMIT 10";

        //4.2 Dados form pesquisa (Cliente)
        private const string SQLPesquisaCLienteContato =
            "SELECT P.ID, P.NOME, P.CPF, P.RG, P.DATANASCIMENTO, P.SEXO " +
            "FROM PESSOA P OUTER LEFT JOIN CONTATO C ON C.ID = P.CONTATO_ID";

        //4.3 Verifica se existe registros nas tabelas
        private const string SQLPesquisaDados =
            "SELECT COUNT(P.ID) AS QUANTDADOS " +
            "FROM PESSOA P " +
            "LEFT OUTER JOIN ENDERECO E ON E.ID = P.ENDERECO_ID " +
            "LEFT OUTER JOIN CONTATO C ON E.ID = P.CONTATO_ID";

        //4.4 Buscar último registro
        private const string SQLSelecionarUltimoRegistro = "SELECT MAX(ID) FROM PESSOA";

        //4.5 Busca todos os campos do cadastro do cliente (Dados pessoais, endereço e contato) <<<---- IMPLEMENTAR PARAMETRO NO SQL "@"
        private const string SQLSelecionaDadosPessoaCompleto =
            "SELECT P.ID, P.NOME, P.CPF, P.RG, P.DATANASCIMENTO, P.SEXO, " +
                   "E.CEP, E.PAIS, E.UF, E.CIDADE, E.BAIRRO, E.RUA, E.NUMERO, E.COMPLEMENTO, " +
                   "C.TEL, C.CEL, C.EMAIL " +
            "FROM PESSOA P " +
            "INNER JOIN ENDERECO E ON P.ENDERECO_ID = E.ID " +
            "LEFT OUTER JOIN CONTATO C ON P.CONTATO_ID = C.ID " +
            "WHERE P.ID = ";

        //4.6 Verifica se existe CPF já cadastrado
        private const string SQLExisteCPF = "SELECT ID, CPF FROM PESSOA WHERE CPF = @CPF";

        //5.0 DELETAR REGISTRO
        private const string SQLDeletaPessoa = "DELETE FROM PESSOA WHERE ID = ";

        //6.0 ATUALIZAÇÃO DE REGISTROS

        //6.1 Atualiza tabela PESSOA
        private const string SQLUpdateCliente =
            "UPDATE PESSOA SET NOME = @NOME, CPF = @CPF, RG = @RG, DATANASCIMENTO = @DATANASC, SEXO = @SEXO WHERE ID = @ID";
        //6.2 Atualiza tabela ENDERECO
        private const string SQLUpdateEndereco =
            "UPDATE ENDERECO SET CEP = @CEP, PAIS = @PAIS, UF = @UF, CIDADE = @CIDADE, BAIRRO = @BAIRRO, RUA = @RUA, " +
                                "NUMERO = @NUMERO, COMPLEMENTO = @COMPLEMENTO WHERE ID = @ID";
        
        //6.3 Atualiza tabela CONTATO
        private const string SQLUpdateContato = "UPDATE CONTATO SET TEL = @TEL, CEL = @CEL, EMAIL = @EMAIL WHERE ID = @ID";

        //Posições de cada campo referente a lista string que carrega os valores da tela(CadCliente) para este método,
        //esses valores são usados para alimentar as novas lists deste método, assim é mais fácil a manutenção
        //caso um novo campo seja adicionado, porém é necessário ficar atento para as posições corretas de cada campo.                
        private const int iPosInicialPessoa = 0;
        private const int iPosFinalPessoa = 4;

        private const int iPosInicialEndereco = 5;
        private const int iPosFinalEndereco = 12;

        private const int iPosInicialContato = 13;
        private const int iPosFinalContato = 15;

        public static void CriarConexao()
        {      
            if (!File.Exists(sNomeBanco))
                SQLiteConnection.CreateFile(sNomeBanco);

            SQLiteConnection con = new SQLiteConnection(dsConexao);
            con.Open();

            CriarTabelas(con);
        }

        public static void CriarTabelas(SQLiteConnection con)
        {
            StringBuilder SQL = new StringBuilder();

            SQL.AppendLine(SQLCriarTabelaEndereco);
            SQL.AppendLine(SQLCriarTabelaContato);
            SQL.AppendLine(SQLCriarTabelaPessoa);

            SQLiteCommand cmd = new SQLiteCommand(SQL.ToString(), con);

            cmd.ExecuteNonQuery();
        }

        public static void GravarRegistro(List<string> DadosCliente, int iCodigoCliente = 0)
        {
            {               
                SQLiteConnection con = new SQLiteConnection(dsConexao);

                if (con.State == ConnectionState.Closed)
                {
                    con.Open();

                    //Cria o SQLite Command
                    SQLiteCommand cmdGravarRegistro = new SQLiteCommand()
                    {
                        Connection = con
                    };
                   
                    //Gravar tabela ENDERECO 
                    List<string> Endereco = new List<string>();
                    
                    for (int i = iPosInicialEndereco; i <= iPosFinalEndereco; i++)
                        Endereco.Add(DadosCliente[i]);                  

                    if (iCodigoCliente == 0) //INSERT
                        cmdGravarRegistro.CommandText = SQLInserirEndereco;
                    else
                    {   //UPDATE 
                        cmdGravarRegistro.CommandText = SQLUpdateEndereco;
                        cmdGravarRegistro.Parameters.AddWithValue("ID", iCodigoCliente);
                    }

                    if (Endereco[0] == string.Empty)
                        cmdGravarRegistro.Parameters.AddWithValue("CEP", null);
                    else
                        cmdGravarRegistro.Parameters.AddWithValue("CEP", Endereco[0].ToString());

                    cmdGravarRegistro.Parameters.AddWithValue("PAIS", Endereco[1].ToString());
                    cmdGravarRegistro.Parameters.AddWithValue("UF", Endereco[2].ToString());
                    cmdGravarRegistro.Parameters.AddWithValue("CIDADE", Endereco[3].ToString());
                    cmdGravarRegistro.Parameters.AddWithValue("BAIRRO", Endereco[4].ToString());
                    cmdGravarRegistro.Parameters.AddWithValue("RUA", Endereco[5].ToString());

                    if (Endereco[6] != null)
                        cmdGravarRegistro.Parameters.AddWithValue("NUMERO", Convert.ToInt32(Endereco[6]));                    
                    else
                        cmdGravarRegistro.Parameters.AddWithValue("NUMERO", null);

                    cmdGravarRegistro.Parameters.AddWithValue("COMPLEMENTO", Endereco[7].ToString());

                    cmdGravarRegistro.ExecuteNonQuery();

                    //Grava tabela CONTATO
                    List<string> Contato = new List<string>();

                    for (int i = iPosInicialContato; i <= iPosFinalContato; i++)
                        Contato.Add(DadosCliente[i]);
                    
                    if (iCodigoCliente == 0) //INSERT
                        cmdGravarRegistro.CommandText = SQLInserirContato;
                    else
                    {   //UPDATE
                        cmdGravarRegistro.CommandText = SQLUpdateContato;
                        cmdGravarRegistro.Parameters.AddWithValue("ID", iCodigoCliente);
                    }                  

                    if (Contato[0] == string.Empty)
                        cmdGravarRegistro.Parameters.AddWithValue("TEL", null);
                    else
                        cmdGravarRegistro.Parameters.AddWithValue("TEL", Contato[0].ToString());

                    if (Contato[1] == string.Empty)
                        cmdGravarRegistro.Parameters.AddWithValue("CEL", null);
                    else
                        cmdGravarRegistro.Parameters.AddWithValue("CEL", Contato[1].ToString());

                    if (Contato[2] == string.Empty)
                        cmdGravarRegistro.Parameters.AddWithValue("EMAIL", null);
                    else
                        cmdGravarRegistro.Parameters.AddWithValue("EMAIL", Contato[2].ToString());

                    cmdGravarRegistro.ExecuteNonQuery();

                    //Gravar tabela PESSOA
                    List<string> Pessoa = new List<string>();

                    for (int i = iPosInicialPessoa; i <= iPosFinalPessoa; i++)
                        Pessoa.Add(DadosCliente[i]);
                    
                    if (iCodigoCliente == 0) //INSERT
                        cmdGravarRegistro.CommandText = SQLInserirPessoaComContato;
                    else
                    {   //UPDATE
                        cmdGravarRegistro.CommandText = SQLUpdateCliente;
                        cmdGravarRegistro.Parameters.AddWithValue("ID", iCodigoCliente);
                    }

                    cmdGravarRegistro.Parameters.AddWithValue("NOME", Pessoa[0].ToString());
                    cmdGravarRegistro.Parameters.AddWithValue("CPF", Pessoa[1].ToString());
                    cmdGravarRegistro.Parameters.AddWithValue("RG", Pessoa[2].ToString());
                    cmdGravarRegistro.Parameters.AddWithValue("DATANASC", DateTime.Parse(Pessoa[3]));
                    cmdGravarRegistro.Parameters.AddWithValue("SEXO", Pessoa[4].ToString());

                    cmdGravarRegistro.ExecuteNonQuery();

                    UltimoID = con.LastInsertRowId; //Busca o último código salvo na tabela PESSOA.

                    con.Close();
                }
            }
        }

        public static void DeletarRegistro(int Codigo)
        {
            SQLiteConnection con = new SQLiteConnection(dsConexao);
            con.Open();

            string SQL;

            SQL = SQLDeletaPessoa + Convert.ToString(Codigo);

            SQLiteCommand cmdDeletar = new SQLiteCommand(SQL, con);
            cmdDeletar.ExecuteNonQuery();
        }

        //<<<<<<<<<<<<<<<<<<<<<<MUDAR SQL
        public static DataTable CarregarDadosGrid(byte Dgv, int iCodigo = 0, string sNome = "")
        {        
            SQLiteConnection con = new SQLiteConnection(dsConexao);

            DataTable dtTabela = new DataTable();

            if (con.State == ConnectionState.Closed)
            {
                con.Open();              

                string sSQL = string.Empty;

                if (Dgv == 0) //DataGridView do FrmCadCliente
                    sSQL = SQLSelecionarDadosParciais;

                else if (Dgv == 1) //DataGridView do FrmPesquisa
                {
                    if (iCodigo != 0 && sNome == string.Empty)
                        sSQL = SQLPesquisaCLienteContato + " WHERE P.ID = " + Convert.ToUInt32(iCodigo);

                    else if (sNome != string.Empty && iCodigo == 0)
                        sSQL = SQLPesquisaCLienteContato + " WHERE NOME LIKE '%" + sNome + "%'";
                }

                SQLiteDataAdapter da = new SQLiteDataAdapter(sSQL, con);
                da.Fill(dtTabela);

                con.Close();
            }
            
            return dtTabela;
        }

        public static List<string> BuscaCamposCliente(int CodigoCliente)
        {
            SQLiteConnection con = new SQLiteConnection(dsConexao);
            con.Open();

            string SQL = SQLSelecionaDadosPessoaCompleto + CodigoCliente.ToString();

            SQLiteCommand cmd = new SQLiteCommand(SQL, con);

            SQLiteDataReader Leitor = cmd.ExecuteReader();

            List<string> DadosPessoa = new List<string>();

            while (Leitor.Read())
            {
                //Tabela pessoa
                DadosPessoa.Add(Leitor["NOME"].ToString());
                DadosPessoa.Add(Leitor["CPF"].ToString());
                DadosPessoa.Add(Leitor["RG"].ToString());
                DadosPessoa.Add(Leitor["DATANASCIMENTO"].ToString());
                DadosPessoa.Add(Leitor["SEXO"].ToString());

                //Tabela endereço
                DadosPessoa.Add(Leitor["CEP"].ToString());
                DadosPessoa.Add(Leitor["PAIS"].ToString());
                DadosPessoa.Add(Leitor["UF"].ToString());
                DadosPessoa.Add(Leitor["CIDADE"].ToString());
                DadosPessoa.Add(Leitor["BAIRRO"].ToString());
                DadosPessoa.Add(Leitor["RUA"].ToString());

                if (Leitor["NUMERO"].ToString() == string.Empty)
                    DadosPessoa.Add(null);
                else
                    DadosPessoa.Add(Leitor["NUMERO"].ToString());

                DadosPessoa.Add(Leitor["COMPLEMENTO"].ToString());

                //Tabela contato
                DadosPessoa.Add(Leitor["TEL"].ToString());
                DadosPessoa.Add(Leitor["CEL"].ToString());
                DadosPessoa.Add(Leitor["EMAIL"].ToString());
            }

            return DadosPessoa;
        }
  
        //USADO NA PESQUISA - APRIMORAR PARA USAR EM OUTROS METODOS!!! CORRIGOR: USAR PARAMETRO "@"!!!!
        public static bool ExisteRegistro(int codigo, string nome) 
        {
            string sSQL = string.Empty;

            if (codigo != 0)
                sSQL = SQLPesquisaDados + " WHERE P.ID = " + Convert.ToInt32(codigo);

            else if (nome != string.Empty)
                sSQL = SQLPesquisaDados + " WHERE NOME LIKE '%" + nome + "%'";

            SQLiteConnection con = new SQLiteConnection(dsConexao);
            con.Open();

            SQLiteCommand cmd = new SQLiteCommand(sSQL, con);

            int result = Convert.ToInt32(cmd.ExecuteScalar());

            con.Close();

            if (result != 0)
                return true;
            else
                return false;
        }

        public static bool VerificaCPF(string sCPF, int iCodigoCliente = 0)
        {
            SQLiteConnection con = new SQLiteConnection(dsConexao);
            con.Open();

            SQLiteCommand cmd = new SQLiteCommand(SQLExisteCPF, con);

            cmd.Parameters.AddWithValue("CPF", sCPF);

            int iCodigoClienteBanco = Convert.ToInt32(cmd.ExecuteScalar());

            con.Close();

            if (iCodigoClienteBanco > 0)
            {
                if (iCodigoClienteBanco == iCodigoCliente)
                    return true;
                else
                    return false;
            }
            else
                return true;
        }
    }  
}
