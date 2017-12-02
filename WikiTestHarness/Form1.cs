using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using TASVideos.WikiEngine;

namespace WikiTestHarness
{
	public partial class Form1 : Form
	{
		private Config Config { get; set; }

		public Form1()
		{
			InitializeComponent();
			Closing += (o, e) =>
			{
				Config.WikiMarkup = MarkupBox.Text;
				ConfigService.Save("./config.ini", Config);
			};
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			Config = ConfigService.Load<Config>("./config.ini");
			MarkupBox.Text = Config.WikiMarkup;
			RunBtn_Click(null, null);
		}

		private void RunBtn_Click(object sender, EventArgs e)
		{
			var wikiAst = new WikiAst(MarkupBox.Text);
			AstBox.Text = wikiAst.ToString();
			HtmlBox.Text = wikiAst.ToHtml();
		}

		private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			linkLabel1.LinkVisited = true;
			Process.Start("http://tasvideos.org/TextFormattingRules.html");
		}
	}
}
