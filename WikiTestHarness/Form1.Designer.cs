namespace WikiTestHarness
{
	partial class Form1
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
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
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.splitContainer1 = new System.Windows.Forms.SplitContainer();
			this.splitContainer2 = new System.Windows.Forms.SplitContainer();
			this.MarkupBox = new System.Windows.Forms.RichTextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.AstBox = new System.Windows.Forms.RichTextBox();
			this.label3 = new System.Windows.Forms.Label();
			this.HtmlBox = new System.Windows.Forms.RichTextBox();
			this.RunBtn = new System.Windows.Forms.Button();
			this.linkLabel1 = new System.Windows.Forms.LinkLabel();
			this.ProcessingTimeLabel = new System.Windows.Forms.Label();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
			this.splitContainer2.Panel1.SuspendLayout();
			this.splitContainer2.Panel2.SuspendLayout();
			this.splitContainer2.SuspendLayout();
			this.SuspendLayout();
			// 
			// splitContainer1
			// 
			this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer1.Location = new System.Drawing.Point(0, 0);
			this.splitContainer1.Name = "splitContainer1";
			// 
			// splitContainer1.Panel1
			// 
			this.splitContainer1.Panel1.Controls.Add(this.ProcessingTimeLabel);
			this.splitContainer1.Panel1.Controls.Add(this.linkLabel1);
			this.splitContainer1.Panel1.Controls.Add(this.RunBtn);
			this.splitContainer1.Panel1.Controls.Add(this.MarkupBox);
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
			this.splitContainer1.Size = new System.Drawing.Size(850, 790);
			this.splitContainer1.SplitterDistance = 283;
			this.splitContainer1.TabIndex = 0;
			// 
			// splitContainer2
			// 
			this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer2.Location = new System.Drawing.Point(0, 0);
			this.splitContainer2.Name = "splitContainer2";
			// 
			// splitContainer2.Panel1
			// 
			this.splitContainer2.Panel1.Controls.Add(this.AstBox);
			this.splitContainer2.Panel1.Controls.Add(this.label2);
			// 
			// splitContainer2.Panel2
			// 
			this.splitContainer2.Panel2.Controls.Add(this.HtmlBox);
			this.splitContainer2.Panel2.Controls.Add(this.label3);
			this.splitContainer2.Size = new System.Drawing.Size(563, 790);
			this.splitContainer2.SplitterDistance = 187;
			this.splitContainer2.TabIndex = 0;
			// 
			// MarkupBox
			// 
			this.MarkupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.MarkupBox.Location = new System.Drawing.Point(19, 25);
			this.MarkupBox.Name = "MarkupBox";
			this.MarkupBox.Size = new System.Drawing.Size(265, 733);
			this.MarkupBox.TabIndex = 1;
			this.MarkupBox.Text = "";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(3, 9);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(28, 13);
			this.label2.TabIndex = 0;
			this.label2.Text = "AST";
			// 
			// AstBox
			// 
			this.AstBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.AstBox.Location = new System.Drawing.Point(3, 25);
			this.AstBox.Name = "AstBox";
			this.AstBox.ReadOnly = true;
			this.AstBox.Size = new System.Drawing.Size(181, 753);
			this.AstBox.TabIndex = 1;
			this.AstBox.Text = "";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(3, 9);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(37, 13);
			this.label3.TabIndex = 0;
			this.label3.Text = "HTML";
			// 
			// HtmlBox
			// 
			this.HtmlBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.HtmlBox.Location = new System.Drawing.Point(6, 25);
			this.HtmlBox.Name = "HtmlBox";
			this.HtmlBox.ReadOnly = true;
			this.HtmlBox.Size = new System.Drawing.Size(354, 726);
			this.HtmlBox.TabIndex = 1;
			this.HtmlBox.Text = "";
			// 
			// RunBtn
			// 
			this.RunBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.RunBtn.Location = new System.Drawing.Point(19, 764);
			this.RunBtn.Name = "RunBtn";
			this.RunBtn.Size = new System.Drawing.Size(75, 23);
			this.RunBtn.TabIndex = 2;
			this.RunBtn.Text = "button1";
			this.RunBtn.UseVisualStyleBackColor = true;
			this.RunBtn.Click += new System.EventHandler(this.RunBtn_Click);
			// 
			// linkLabel1
			// 
			this.linkLabel1.AutoSize = true;
			this.linkLabel1.Location = new System.Drawing.Point(16, 9);
			this.linkLabel1.Name = "linkLabel1";
			this.linkLabel1.Size = new System.Drawing.Size(43, 13);
			this.linkLabel1.TabIndex = 3;
			this.linkLabel1.TabStop = true;
			this.linkLabel1.Text = "Markup";
			this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
			// 
			// ProcessingTimeLabel
			// 
			this.ProcessingTimeLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.ProcessingTimeLabel.AutoSize = true;
			this.ProcessingTimeLabel.Location = new System.Drawing.Point(100, 769);
			this.ProcessingTimeLabel.Name = "ProcessingTimeLabel";
			this.ProcessingTimeLabel.Size = new System.Drawing.Size(88, 13);
			this.ProcessingTimeLabel.TabIndex = 4;
			this.ProcessingTimeLabel.Text = "Processing Time:";
			// 
			// Form1
			// 
			this.AcceptButton = this.RunBtn;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(850, 790);
			this.Controls.Add(this.splitContainer1);
			this.Name = "Form1";
			this.Text = "Wiki Test Harness";
			this.Load += new System.EventHandler(this.Form1_Load);
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel1.PerformLayout();
			this.splitContainer1.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
			this.splitContainer1.ResumeLayout(false);
			this.splitContainer2.Panel1.ResumeLayout(false);
			this.splitContainer2.Panel1.PerformLayout();
			this.splitContainer2.Panel2.ResumeLayout(false);
			this.splitContainer2.Panel2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
			this.splitContainer2.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.SplitContainer splitContainer1;
		private System.Windows.Forms.SplitContainer splitContainer2;
		private System.Windows.Forms.RichTextBox MarkupBox;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.RichTextBox AstBox;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.RichTextBox HtmlBox;
		private System.Windows.Forms.Button RunBtn;
		private System.Windows.Forms.LinkLabel linkLabel1;
		private System.Windows.Forms.Label ProcessingTimeLabel;
	}
}

