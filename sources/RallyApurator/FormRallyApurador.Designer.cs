namespace RallyApurator
{
    partial class FormRallyApurador
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
            txtTempoAlvo = new TextBox();
            lblTempoAlvo = new Label();
            btnCarregar = new Button();
            dgvResultados = new DataGridView();
            label1 = new Label();
            txtVoltasValidas = new TextBox();
            txtVoltasDesclassificacao = new TextBox();
            label2 = new Label();
            label3 = new Label();
            txtTempoDesclassificacao = new TextBox();
            btnExportar = new Button();
            pictureBox1 = new PictureBox();
            btnCuriosidades = new Button();
            ((System.ComponentModel.ISupportInitialize)dgvResultados).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            SuspendLayout();
            // 
            // txtTempoAlvo
            // 
            txtTempoAlvo.Location = new Point(174, 23);
            txtTempoAlvo.Margin = new Padding(2);
            txtTempoAlvo.Name = "txtTempoAlvo";
            txtTempoAlvo.Size = new Size(121, 27);
            txtTempoAlvo.TabIndex = 0;
            txtTempoAlvo.Text = "2:40.000";
            // 
            // lblTempoAlvo
            // 
            lblTempoAlvo.AutoSize = true;
            lblTempoAlvo.Location = new Point(10, 28);
            lblTempoAlvo.Margin = new Padding(2, 0, 2, 0);
            lblTempoAlvo.Name = "lblTempoAlvo";
            lblTempoAlvo.Size = new Size(92, 20);
            lblTempoAlvo.TabIndex = 1;
            lblTempoAlvo.Text = "Tempo Alvo:";
            // 
            // btnCarregar
            // 
            btnCarregar.Location = new Point(10, 168);
            btnCarregar.Margin = new Padding(2);
            btnCarregar.Name = "btnCarregar";
            btnCarregar.Size = new Size(89, 27);
            btnCarregar.TabIndex = 2;
            btnCarregar.Text = "Carregar";
            btnCarregar.UseVisualStyleBackColor = true;
            btnCarregar.Click += btnCarregar_Click;
            // 
            // dgvResultados
            // 
            dgvResultados.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            dataGridViewCellStyle1.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = SystemColors.GradientActiveCaption;
            dataGridViewCellStyle1.Font = new Font("Segoe UI", 10F, FontStyle.Bold, GraphicsUnit.Point, 0);
            dataGridViewCellStyle1.ForeColor = SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = DataGridViewTriState.True;
            dgvResultados.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            dgvResultados.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvResultados.Location = new Point(10, 209);
            dgvResultados.Margin = new Padding(2);
            dgvResultados.Name = "dgvResultados";
            dgvResultados.RowHeadersWidth = 62;
            dgvResultados.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvResultados.Size = new Size(1041, 424);
            dgvResultados.TabIndex = 3;
            dgvResultados.CellFormatting += dgvResultados_CellFormatting;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(10, 60);
            label1.Margin = new Padding(2, 0, 2, 0);
            label1.Name = "label1";
            label1.Size = new Size(103, 20);
            label1.TabIndex = 4;
            label1.Text = "Voltas Válidas:";
            // 
            // txtVoltasValidas
            // 
            txtVoltasValidas.Location = new Point(174, 60);
            txtVoltasValidas.Margin = new Padding(2);
            txtVoltasValidas.Name = "txtVoltasValidas";
            txtVoltasValidas.Size = new Size(121, 27);
            txtVoltasValidas.TabIndex = 5;
            txtVoltasValidas.Text = "6";
            // 
            // txtVoltasDesclassificacao
            // 
            txtVoltasDesclassificacao.Location = new Point(174, 126);
            txtVoltasDesclassificacao.Margin = new Padding(2);
            txtVoltasDesclassificacao.Name = "txtVoltasDesclassificacao";
            txtVoltasDesclassificacao.Size = new Size(121, 27);
            txtVoltasDesclassificacao.TabIndex = 9;
            txtVoltasDesclassificacao.Text = "2";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(10, 126);
            label2.Margin = new Padding(2, 0, 2, 0);
            label2.Name = "label2";
            label2.Size = new Size(162, 20);
            label2.TabIndex = 8;
            label2.Text = "Voltas Desqualificação:";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(10, 94);
            label3.Margin = new Padding(2, 0, 2, 0);
            label3.Name = "label3";
            label3.Size = new Size(113, 20);
            label3.TabIndex = 7;
            label3.Text = "Tempo Minimo:";
            // 
            // txtTempoDesclassificacao
            // 
            txtTempoDesclassificacao.Location = new Point(174, 90);
            txtTempoDesclassificacao.Margin = new Padding(2);
            txtTempoDesclassificacao.Name = "txtTempoDesclassificacao";
            txtTempoDesclassificacao.Size = new Size(121, 27);
            txtTempoDesclassificacao.TabIndex = 6;
            txtTempoDesclassificacao.Text = "2:25.000";
            // 
            // btnExportar
            // 
            btnExportar.Enabled = false;
            btnExportar.Location = new Point(103, 168);
            btnExportar.Margin = new Padding(2);
            btnExportar.Name = "btnExportar";
            btnExportar.Size = new Size(86, 27);
            btnExportar.TabIndex = 10;
            btnExportar.Text = "Exportar";
            btnExportar.UseVisualStyleBackColor = true;
            btnExportar.Click += btnExportar_Click;
            // 
            // pictureBox1
            // 
            pictureBox1.Image = Properties.Resources.rallyApuratorBanner;
            pictureBox1.Location = new Point(322, 18);
            pictureBox1.Margin = new Padding(2);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(728, 178);
            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox1.TabIndex = 11;
            pictureBox1.TabStop = false;
            // 
            // btnCuriosidades
            // 
            btnCuriosidades.Enabled = false;
            btnCuriosidades.Location = new Point(194, 168);
            btnCuriosidades.Margin = new Padding(2);
            btnCuriosidades.Name = "btnCuriosidades";
            btnCuriosidades.Size = new Size(100, 27);
            btnCuriosidades.TabIndex = 12;
            btnCuriosidades.Text = "Curiosidades";
            btnCuriosidades.UseVisualStyleBackColor = true;
            btnCuriosidades.Click += btnCuriosidades_Click;
            // 
            // FormRallyApurador
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1064, 642);
            Controls.Add(btnCuriosidades);
            Controls.Add(pictureBox1);
            Controls.Add(btnExportar);
            Controls.Add(txtVoltasDesclassificacao);
            Controls.Add(label2);
            Controls.Add(label3);
            Controls.Add(txtTempoDesclassificacao);
            Controls.Add(txtVoltasValidas);
            Controls.Add(label1);
            Controls.Add(dgvResultados);
            Controls.Add(btnCarregar);
            Controls.Add(lblTempoAlvo);
            Controls.Add(txtTempoAlvo);
            Margin = new Padding(2);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FormRallyApurador";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Trex Rally - Apurador";
            Load += Form1_Load;
            ((System.ComponentModel.ISupportInitialize)dgvResultados).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox txtTempoAlvo;
        private Label lblTempoAlvo;
        private Button btnCarregar;
        private DataGridView dgvResultados;
        private Label label1;
        private TextBox txtVoltasValidas;
        private TextBox txtVoltasDesclassificacao;
        private Label label2;
        private Label label3;
        private TextBox txtTempoDesclassificacao;
        private Button btnExportar;
        private PictureBox pictureBox1;
        private Button btnCuriosidades;
    }
}
