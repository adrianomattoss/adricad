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
    public partial class FrmAlterarCadastro : Form
    {
        private bool bCancelarAlteracao = false;

        private bool bPesquisaPorNome;
        private int iCodigoClientePesquisa;
        string sUltimoNumeroInformado;

        public bool GetPesquisaPorNome
        {
            get { return bPesquisaPorNome; }
            set { bPesquisaPorNome = value; }
        }
              
        public int GetcodigoClientePesquisa
        {
            get { return iCodigoClientePesquisa; }
            set { iCodigoClientePesquisa = value; }
        }

        private FrmPesquisar FormPesquisa;

        public FrmAlterarCadastro(FrmPesquisar FormPesquisa)
        {
            InitializeComponent();

            this.FormPesquisa = FormPesquisa;
        }           

        /*public FrmAlterarCadastro()
         {
             InitializeComponent();
         }*/

         private void BtnGravar_Click(object sender, EventArgs e)
         {
            GravarRegistro();
         }

         private void FrmAlterarPesquisa_Load(object sender, EventArgs e)
         {
            PreencherCampos(iCodigoClientePesquisa);

            TxtNomeCliente.Focus();
         }

        private void FrmAlterarCadastro_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (bCancelarAlteracao == false)
            {
                if (bPesquisaPorNome == true)
                    MetodosFormularios.CarregarGridView(FormPesquisa.DgvPesquisa, 0, FormPesquisa.TxtNome.Text);
                else
                    MetodosFormularios.CarregarGridView(FormPesquisa.DgvPesquisa,
                        iCodigoClientePesquisa);
            }
        }

        private void BtnCancelar_Click(object sender, EventArgs e)
        {
            bCancelarAlteracao = true;

            this.Close();
        }

        private void ChkSN_CheckedChanged(object sender, EventArgs e)
        {
            if (ChkSN.Checked == true)
            {
                sUltimoNumeroInformado = TxtNumero.Text;
                TxtNumero.Text = string.Empty;
                TxtNumero.Enabled = false;
                lblNumero.Enabled = false;
            }
            else
            {
                TxtNumero.Text = sUltimoNumeroInformado;
                TxtNumero.Enabled = true;
                lblNumero.Enabled = true;
            }
        }

        private void MtxtCPF_Leave(object sender, EventArgs e)
        {
            if (MetodosFormularios.TextoSemMascara(MtxtCPF).Trim() != string.Empty)
            {
                if (ValidaDigitoCPF.ValidaCPF(MtxtCPF.Text) == false)
                {
                    MessageBox.Show("CPF inválido, por favor informe outro valor.", "Atenção",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    MtxtCPF.Focus();
                }
            }
        }

        private void MtxtDataNasc_Leave(object sender, EventArgs e)
        {
            if (MetodosFormularios.TextoSemMascara(MtxtDataNasc).Trim() != string.Empty)
            {
                MtxtDataNasc.TextMaskFormat = MaskFormat.IncludePromptAndLiterals;

                if (MetodosFormularios.ValidaData(MtxtDataNasc.Text) == false)
                {
                    MessageBox.Show("A data de nascimento informada é inválida, por favor informe outro valor.", "Atenção",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    MtxtDataNasc.Focus();
                }
            }
        }

        private void GravarRegistro()
        {
            if (CamposPreenchidos() == true)
            {
                List<string> DadosCliente = new List<string>();

                //Dados pessoa: Posições 0 à 4
                DadosCliente.Add(TxtNomeCliente.Text);
                DadosCliente.Add(MtxtCPF.Text);
                DadosCliente.Add(TxtRG.Text);
                DadosCliente.Add(MtxtDataNasc.Text);
                if (CbSexo.Text == "Masculino")
                    DadosCliente.Add("M");
                else
                    DadosCliente.Add("F");

                //Endereço: Posições 5 à 12
                DadosCliente.Add(MetodosFormularios.TextoSemMascara(MtxtCEP).Trim());
                DadosCliente.Add(TxtPais.Text);
                DadosCliente.Add(CbEstado.Text);
                DadosCliente.Add(TxtCidade.Text);
                DadosCliente.Add(TxtBairro.Text);
                DadosCliente.Add(TxtRua.Text);
                if (ChkSN.Checked)
                    DadosCliente.Add(null);
                else
                    DadosCliente.Add(TxtNumero.Text.Trim());
                DadosCliente.Add(TxtComplemento.Text);

                //Contato: Posições 13 à 15
                DadosCliente.Add(MetodosFormularios.TextoSemMascara(MtxtTelefone).Trim());
                DadosCliente.Add(MetodosFormularios.TextoSemMascara(MtxtCelular).Trim());
                DadosCliente.Add(TxtEmail.Text.Trim());
                
                if (ConexaoBanco.VerificaCPF(MtxtCPF.Text, iCodigoClientePesquisa) == true)
                {
                    try
                    {
                        ConexaoBanco.GravarRegistro(DadosCliente, iCodigoClientePesquisa);

                        MessageBox.Show("Registo alterado com sucesso!", "Alteração realizada",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);

                        this.Close();
                    }
                    catch
                    {
                        MessageBox.Show("Não foi possível salvar o registro! Por favor, informe o desenvolvedor.", "Atenção",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                    TxtNomeCliente.Focus();
                }
                else
                {
                    MessageBox.Show("CPF já informado no banco de dados, por favor informe outro valor.", "Atenção",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);

                    MtxtCPF.Focus();
                }                       
            }
        }

        private bool CamposPreenchidos()
        {
            bool result = false;

            if (TxtNomeCliente.Text.Trim() != string.Empty
                && MetodosFormularios.TextoSemMascara(MtxtCPF).Trim() != string.Empty
                //&& TxtRG.Text.Trim() != string.Empty
                && MtxtDataNasc.Text.Trim() != string.Empty
                && TxtPais.Text.Trim() != string.Empty
                && CbEstado.Text.Trim() != string.Empty
                && TxtCidade.Text.Trim() != string.Empty
                && TxtRua.Text.Trim() != string.Empty
                && (TxtNumero.Text.Trim() != string.Empty || ChkSN.Checked == true))
            {
                result = true;
            }
            else
            {
                MessageBox.Show("Um ou mais campos obrigatórios não foram preenchidos. Campos marcados com '*' são obrigatórios.",
                    "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Information);

                TxtNomeCliente.Focus();

                result = false;
            }

            return result;
        }

        private void PreencherCampos(int iCodigoCliente)
        {
            List<string> DadosPessoa = new List<string>();

            DadosPessoa = ConexaoBanco.BuscaCamposCliente(iCodigoCliente);

            TxtNomeCliente.Text = DadosPessoa[0].ToString();
            MtxtCPF.Text = DadosPessoa[1].ToString();
            TxtRG.Text = DadosPessoa[2].ToString();
            MtxtDataNasc.Text = DadosPessoa[3].ToString();

            if (DadosPessoa[4].ToString() == "M")
                CbSexo.Text = "Masculino";
            else
                CbSexo.Text = "Feminino";

            MtxtCEP.Text = DadosPessoa[5].ToString();
            TxtPais.Text = DadosPessoa[6].ToString();
            CbEstado.Text = DadosPessoa[7].ToString();
            TxtCidade.Text = DadosPessoa[8].ToString();
            TxtBairro.Text = DadosPessoa[9].ToString();
            TxtRua.Text = DadosPessoa[10].ToString();

            if (DadosPessoa[11] == null)
                ChkSN.Checked = true;
            else
                TxtNumero.Text = DadosPessoa[11].ToString();
            TxtComplemento.Text = DadosPessoa[12].ToString();

            MtxtTelefone.Text = DadosPessoa[13].ToString();
            MtxtCelular.Text = DadosPessoa[14].ToString();
            TxtEmail.Text = DadosPessoa[15].ToString();
        }
    }
}
