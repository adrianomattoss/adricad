using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Adricad
{
    public partial class FrmPesquisar : Form
    {
        private FrmCadCliente formCadCliente;

        public FrmPesquisar(FrmCadCliente formCadCliente)
        {
            InitializeComponent();
            this.formCadCliente = formCadCliente;
        }

        /*public FrmPesquisar()
        {
            InitializeComponent();
        }*/

        private void FrmPesquisar_Load(object sender, EventArgs e)
        {
            HabDesbControles();

            ReiniciarCampos();
        }



        private void FrmPesquisar_Activated(object sender, EventArgs e)
        {
            TxtCodigo.Focus();
        }

        private void BtnPesquisar_Click(object sender, EventArgs e)
        {
            CarregarDados();
        }

        private void BtnDeletar_Click(object sender, EventArgs e)
        {
            DeletarRegistro();
        }

        private void TxtCodigo_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)13)
                CarregarDados();
            else
                MetodosFormularios.VerificaTeclas(false, e);
        }

        private void TxtNome_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)13)
                CarregarDados();
            else
                MetodosFormularios.VerificaTeclas(true, e);
        }

        private void DgvPesquisa_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            CarregarContato();
            CarregarEndereco();
        }

        private void dgvPesquisa_SelectionChanged(object sender, EventArgs e)
        {
            CarregarContato();
            CarregarEndereco();
        }

        private void RbCodigo_CheckedChanged(object sender, EventArgs e)
        {
            if (rbCodigo.Checked == true)
            {
                PanelCodigoPesquisa.Visible = true;
                PanelNomePesquisa.Visible = false;

                TxtNome.Text = string.Empty;

                TxtCodigo.Focus();
            }
            else
            {
                PanelNomePesquisa.Location = new Point(90, 20);

                PanelCodigoPesquisa.Visible = false;
                PanelNomePesquisa.Visible = true;

                TxtCodigo.Text = string.Empty;

                TxtNome.Focus();
            }
        }

        private void FrmPesquisar_FormClosed(object sender, FormClosedEventArgs e)
        {
            formCadCliente.Visible = true;

            MetodosFormularios.CarregarGridView(formCadCliente.DgvPessoas);
        }

        private void BtnAlterar_Click(object sender, EventArgs e)
        {
            FrmAlterarCadastro FormAlterarRegistro = new FrmAlterarCadastro(this);

            FormAlterarRegistro.GetcodigoClientePesquisa = Convert.ToInt32(DgvPesquisa.CurrentRow.Cells[0].Value);

            if (TxtNome.Text != string.Empty)
                FormAlterarRegistro.GetPesquisaPorNome = true;
            else
                FormAlterarRegistro.GetPesquisaPorNome = false;

            FormAlterarRegistro.ShowDialog();
        }

        private void ReiniciarCampos()
        {
            PanelPesquisarInicial.Visible = true;
            panelDados.Visible = false;

            PanelCodigoPesquisa.Visible = true;
            PanelNomePesquisa.Visible = false;

            rbCodigo.Checked = true;
            TxtCodigo.Text = string.Empty;

            TxtCodigo.Focus();
        }

        private void HabDesbControles()
        {
            if (DgvPesquisa.RowCount != 0)
            {
                BtnAlterar.Enabled = true;
                BtnDeletar.Enabled = true;
            }
            else
            {
                BtnAlterar.Enabled = false;
                BtnDeletar.Enabled = false;
            }
        }

        private void CarregarDados()
        {
            int iCodigo = 0;
            string sNome = string.Empty;

            //Verifica qual RadioButton foi solicito que informe um valor.
            if (rbCodigo.Checked)
            {
                if (TxtCodigo.Text != string.Empty)
                    iCodigo = Convert.ToInt32(TxtCodigo.Text);
                else
                {
                    MessageBox.Show("Informe o código do cliente.", "Informação",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                    TxtCodigo.Focus();
                }
            }
            else if (rbNome.Checked)
            {
                if (TxtNome.Text != string.Empty)
                    sNome = TxtNome.Text;
                else
                {
                    MessageBox.Show("Informe o nome do cliente.", "Informação",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);

                    TxtNome.Focus();
                }
            }

            if (iCodigo != 0 || sNome != string.Empty) //Veridica se os campos de pesquisa não estão vazios
            {
                if (ConexaoBanco.ExisteRegistro(iCodigo, sNome)) //Verifica se o codigo ou nome informado existem na tabela
                {

                    MetodosFormularios.CarregarGridView(DgvPesquisa, iCodigo, sNome); 

                    //Se código for 0 quer dizer que a pesquisa foi pelo nome, então usa o código que está no Grid
                    if (iCodigo == 0)
                        iCodigo = Convert.ToInt32(DgvPesquisa.CurrentRow.Cells[0].Value);

                    CarregarContato(iCodigo);
                    CarregarEndereco(iCodigo); 

                    if (DgvPesquisa.RowCount != 0)
                    {
                        PanelPesquisarInicial.Visible = false;
                        panelDados.Visible = true;
                    }                  
                    else 
                    {
                        PanelPesquisarInicial.Visible = true;
                        panelDados.Visible = false;
                    }
                        
                }
                else
                {
                    string valor = "valor";

                    if (rbCodigo.Checked)
                    {
                        valor = "código";
                        TxtCodigo.Focus();
                    }
                    else
                    {
                        valor = "nome";
                        TxtNome.Focus();
                    }

                    MessageBox.Show("O " + valor + " informado não existe na tabela.", "Informação",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }

            HabDesbControles();
        }

        private void CarregarContato(int iCodigoCliente = 0)
        {           
            if (DgvPesquisa.RowCount != 0)
            {
                if (iCodigoCliente == 0) //Usado para trocar endereço na seleção do Grid de clientes
                    iCodigoCliente = Convert.ToInt32(DgvPesquisa.CurrentRow.Cells[0].Value);

                List<string> DadosCliente = new List<string>();

                try
                {
                    DadosCliente = ConexaoBanco.BuscaCamposCliente(iCodigoCliente);

                    if (DadosCliente[13].ToString() == string.Empty &&
                        DadosCliente[14].ToString() == string.Empty &&
                        DadosCliente[15].ToString() == string.Empty)
                    {
                        LblContato.Text = "Não informado";
                    }
                    else
                    {
                        string sTelefone = string.Empty;
                        string sCelular = string.Empty;
                        string sEmail = string.Empty;

                        if (DadosCliente[13].ToString() != string.Empty)
                            sTelefone = "Telefone: " + DadosCliente[13].ToString() + new string(' ', 10);
                        else
                            sTelefone = string.Empty;

                        if (DadosCliente[14].ToString() != string.Empty)
                            sCelular = "Celular: " + DadosCliente[14].ToString() + new string(' ', 10);
                        else
                            sCelular = string.Empty;

                        if (DadosCliente[15].ToString() != string.Empty)
                            sEmail = "E-mail: " + DadosCliente[15].ToString();
                        else
                            sEmail = string.Empty;

                        LblContato.Text = sTelefone + sCelular + sEmail;
                    }
                }
                catch
                {
                    PanelPesquisarInicial.Visible = true;
                    panelDados.Visible = false;

                    MessageBox.Show("Não foi possível localizar o contato. Contate do desenvolvedor.", "Erro",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void CarregarEndereco(int CodigoCliente = 0)
        {
            if (DgvPesquisa.RowCount != 0)
            {
                if (CodigoCliente == 0) //Usado para trocar endereço na seleção do Grid de clientes
                    CodigoCliente = Convert.ToInt32(DgvPesquisa.CurrentRow.Cells[0].Value);

                List<string> ListaEndereco = new List<string>();

                try
                {
                    string sCEP;
                    string sBairro;
                    string sNumero;

                    ListaEndereco = ConexaoBanco.BuscaCamposCliente(CodigoCliente);


                    if (ListaEndereco[5].ToString() != string.Empty)
                        sCEP = ListaEndereco[5].ToString() + new string(' ', 10);
                    else
                        sCEP = "Não informado" + new string(' ', 10);

                    if (ListaEndereco[9].ToString() != string.Empty)
                        sBairro = ListaEndereco[9].ToString() + new string(' ', 10);
                    else
                        sBairro = "Não informado" + new string(' ', 10);

                    if (ListaEndereco[11] != null)
                        sNumero = ListaEndereco[11].ToString();
                    else
                        sNumero = "S/N";

                    LblCEPPaisUFCidade.Text =                        
                        "CEP: " + sCEP +
                        "Pais: " + ListaEndereco[6].ToString() + new string(' ', 10) +
                        "UF: " + ListaEndereco[7].ToString() + new string(' ', 10) +
                        "Cidade: " + ListaEndereco[8].ToString();


                    LblBairroRuaNumero.Text =
                        "Bairro: " + sBairro +
                        "Rua: " + ListaEndereco[10].ToString() + new string(' ', 10) +
                        "Nº: " + sNumero;

                    if (ListaEndereco[12].ToString() == string.Empty)
                        LblComplemento.Text = string.Empty;
                    else
                        LblComplemento.Text =
                            "Complemento: " + ListaEndereco[12].ToString();
                }
                catch
                {
                    PanelPesquisarInicial.Visible = true;
                    panelDados.Visible = false;

                    MessageBox.Show("Não foi possível localizar o endereço. Contate do desenvolvedor.", "Erro",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void DeletarRegistro()
        {
            if (MetodosFormularios.DeletarRegistroGrid(DgvPesquisa) == true)
            {
                if (DgvPesquisa.RowCount != 0)
                    PanelPesquisarInicial.Visible = false;
                else
                    PanelPesquisarInicial.Visible = true;
            }

            if (rbCodigo.Checked == true)
                TxtCodigo.Focus();
            else
                TxtNome.Focus();

            HabDesbControles();
        }
    }
}
