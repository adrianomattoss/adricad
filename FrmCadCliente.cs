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
    public partial class FrmCadCliente : Form
    {

        private string sUltimoNumeroInformado; //Pega o ultimo registro informado no banco.
        private int iControleTimer = 0; //Contrala timer dos paineis de registro salvo e registro excluído.
        private int indexRowGrid = 0; //Controla o index da linha do grid selecionado
        private bool bModoAlteracao = false; //Verifica se cadastro esta em modo de alteração ou gravação

        public FrmCadCliente()
        {
            InitializeComponent();
        }

        private void CadCliente_Load(object sender, EventArgs e)
        {
            ConexaoBanco.CriarConexao();

            MetodosFormularios.LimparCamposForm(this.Controls);

            MetodosFormularios.CarregarGridView(DgvPessoas);

            ReiniciarCampos();

            HabDesabControles();
        }

        private void FrmCadCliente_Activated(object sender, EventArgs e)
        {
            TxtNomeCliente.Focus();
        }

        private void BtnGravarCliente_Click(object sender, EventArgs e)
        {
            GravarRegistro();
        }

        private void BtnDeletar_Click(object sender, EventArgs e)
        {
            DeletarRegistro();
        }

        private void BtnAlterar_Click(object sender, EventArgs e)
        {
            AlterarRegistro(Convert.ToInt32(DgvPessoas.CurrentRow.Cells[0].Value));
        }

        private void BtnCancelar_Click(object sender, EventArgs e)
        {           
            CancelarAlteracao();
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

        private void BtnPesquisar_Click(object sender, EventArgs e)
        {
            this.Visible = false;

            //FrmPesquisar Pesquisa = new FrmPesquisar();
            FrmPesquisar formPesquisa = new FrmPesquisar(this);
            formPesquisa.ShowDialog();
        }

        private void BtnLimparCampos_Click(object sender, EventArgs e)
        {
            MetodosFormularios.LimparCamposForm(this.Controls);

            ReiniciarCampos();

            TxtNomeCliente.Focus();
        }

        private void TxtNomeCliente_KeyPress(object sender, KeyPressEventArgs e)
        {
            MetodosFormularios.VerificaTeclas(true, e);
        }

        private void TxtRG_KeyPress(object sender, KeyPressEventArgs e)
        {
            MetodosFormularios.VerificaTeclas(false, e);
        }

        private void TxtNumero_KeyPress(object sender, KeyPressEventArgs e)
        {
            MetodosFormularios.VerificaTeclas(false, e);
        }

        private void TxtPais_KeyPress(object sender, KeyPressEventArgs e)
        {

            MetodosFormularios.VerificaTeclas(true, e);
        }

        private void CbEstado_KeyPress(object sender, KeyPressEventArgs e)
        {


            MetodosFormularios.VerificaTeclas(true, e);
        }

        private void TimerSavarRegistro_Tick(object sender, EventArgs e)
        {
            ExecutarTimer(0);  
        }

        private void BtnSobre_Click(object sender, EventArgs e)
        {
            FrmSobre Sobre = new FrmSobre();
            Sobre.ShowDialog();
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
                    MessageBox.Show("Data de nascimento inválida, por favor informe outro valor.", "Atenção",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    MtxtDataNasc.Focus();
                }
            }
        }

        private void ReiniciarCampos()
        {
            CbSexo.Text = "Masculino";
            TxtPais.Text = "BRASIL";
            PanelRegistro.Visible = false;

            DgvPessoas.DefaultCellStyle.BackColor = Color.White;
            DgvPessoas.DefaultCellStyle.ForeColor = Color.Black;
            DgvPessoas.DefaultCellStyle.SelectionForeColor = Color.White;
            DgvPessoas.DefaultCellStyle.SelectionBackColor = SystemColors.Highlight;
        }

        private bool CamposPreenchidos()
        {
            bool result = false;

            if (TxtNomeCliente.Text.Trim() != string.Empty
                && MetodosFormularios.TextoSemMascara(MtxtCPF).Trim() != string.Empty
                && MtxtDataNasc.Text.Trim() != string.Empty
                && TxtPais.Text.Trim() != string.Empty
                && CbUF.Text.Trim() != string.Empty
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

        private void GravarRegistro()
        {
            if (CamposPreenchidos() == true)
            {
                int iCodigoCliente = 0;

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
                DadosCliente.Add(CbUF.Text);
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

                try
                {
                    if (DgvPessoas.RowCount != 0 && bModoAlteracao == true)
                        iCodigoCliente = Convert.ToInt32(DgvPessoas.CurrentRow.Cells[0].Value);

                    if (ConexaoBanco.VerificaCPF(MtxtCPF.Text, iCodigoCliente) == true)
                    {
                        if (bModoAlteracao == true)
                        {
                            ConexaoBanco.GravarRegistro(DadosCliente, iCodigoCliente);

                            LblRegistro.Text = "Registro [" + Convert.ToString(iCodigoCliente) + "] alterado com sucesso!";
                            LblCodigoRegistro.Text = string.Empty;
                        }
                        else
                        {
                            ConexaoBanco.GravarRegistro(DadosCliente);

                            LblRegistro.Text = "Registro salvo com sucesso!"; ;
                            LblCodigoRegistro.Text = "Código cliente: " + Convert.ToString(ConexaoBanco.UltimoID);// arrumar                                         
                        }

                        MetodosFormularios.CarregarGridView(DgvPessoas);

                        MetodosFormularios.LimparCamposForm(this.Controls);

                        ReiniciarCampos();

                        PanelRegistro.Visible = true;

                        timerSalvarRegistro.Start();

                        TxtNomeCliente.Focus();
                    }
                    else
                    {
                        MessageBox.Show("CPF já informado no banco de dados, por favor informe outro valor.", "Atenção",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);

                        MtxtCPF.Focus();
                    }   
                }
                catch
                {
                    MessageBox.Show("Não foi possível salvar o registro! Por favor, informe o desenvolvedor.", "Atenção",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);

                    if (bModoAlteracao == true)
                        CancelarAlteracao();
                }

                finally
                {
                    bModoAlteracao = false;

                    HabDesabControles();
                }   
            }
        }

        private void DeletarRegistro()
        {
            if (MetodosFormularios.DeletarRegistroGrid(DgvPessoas) == true)
            {
                MetodosFormularios.LimparCamposForm(this.Controls);

                MetodosFormularios.CarregarGridView(DgvPessoas);

                ReiniciarCampos();

                HabDesabControles();
            }

            bModoAlteracao = false;

            TxtNomeCliente.Focus();
        }

        private void AlterarRegistro(int iCodigoCliente)
        {
            bModoAlteracao = true;

            HabDesabControles(true);

            try
            {
                if (DgvPessoas.RowCount != 0)
                {
                    PreencherCampos(iCodigoCliente);

                    DgvPessoas.DefaultCellStyle.BackColor = Color.LightGray;
                    DgvPessoas.DefaultCellStyle.ForeColor = Color.DarkGray;
                    DgvPessoas.DefaultCellStyle.SelectionBackColor = SystemColors.Highlight;
                    DgvPessoas.DefaultCellStyle.SelectionForeColor = Color.White;

                    //Grava o index da linha atual do grid.
                    indexRowGrid = DgvPessoas.CurrentRow.Index;
                }
            }
            catch
            {
                MessageBox.Show("Não foi realizar a alteração do registro selecionado. Por favor, informe o desenvolvedor.", "Atenção",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            TxtNomeCliente.Focus();
        }

        private void CancelarAlteracao()
        {
            bModoAlteracao = false;

            MetodosFormularios.LimparCamposForm(this.Controls);

            ReiniciarCampos();

            HabDesabControles();

            DgvPessoas.DefaultCellStyle.BackColor = Color.White;
            DgvPessoas.DefaultCellStyle.ForeColor = Color.Black;
            DgvPessoas.DefaultCellStyle.SelectionForeColor = Color.White;
            DgvPessoas.DefaultCellStyle.SelectionBackColor = SystemColors.Highlight;
            
            DgvPessoas.Rows[indexRowGrid].Selected = true; 
        }

        private void PreencherCampos(int iCodigoCliente) 
        {
            if (DgvPessoas.RowCount != 0)
            {
                if (iCodigoCliente == 0)
                    iCodigoCliente = Convert.ToInt32(DgvPessoas.CurrentRow.Cells[0].Value);

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
                CbUF.Text = DadosPessoa[7].ToString();
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

        private void ExecutarTimer(byte panel)
        {
            //0: Panel salvar registro
            //1: Panel deletar registro

            iControleTimer = ++iControleTimer;

            if (iControleTimer == 9)
            {
                PanelRegistro.Visible = false;

                timerSalvarRegistro.Stop();

                iControleTimer = 0;
            }
        }

        private void HabDesabControles(bool bAlterar = false)
        {
            BtnAlterar.Visible = true;
            BtnCancelar.Visible = false;
            BtnPesquisar.Enabled = true;
            DgvPessoas.Enabled = true;

            if (DgvPessoas.RowCount != 0)
            {             
                BtnAlterar.Enabled = true;
                BtnDeletar.Enabled = true;
                BtnPesquisar.Enabled = true;
            }                
            else
            {                
                BtnAlterar.Enabled = false;
                BtnDeletar.Enabled = false;
                BtnPesquisar.Enabled = false;
            }

            if (bAlterar == true)
            {
                BtnCancelar.Location = new Point(476, 266);

                BtnAlterar.Visible = false;
                BtnCancelar.Visible = true;
                BtnPesquisar.Enabled = false;
                DgvPessoas.Enabled = false;
            }
        }
    }
}
