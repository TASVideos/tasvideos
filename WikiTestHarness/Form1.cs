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
			ProcessingTimeLabel.Text = "";
			Config = ConfigService.Load<Config>("./config.ini");
			MarkupBox.Text = Config.WikiMarkup;
			RunBtn_Click(null, null);
		}

		private void RunBtn_Click(object sender, EventArgs e)
		{
			Stopwatch sw = new Stopwatch();
			sw.Start();

			var wikiAst = new WikiAst(MarkupBox.Text);
			HtmlBox.Text = wikiAst.ToHtml();

			sw.Stop();
			ProcessingTimeLabel.Text = $"Total Time: {sw.ElapsedTicks} Ticks";

			AstBox.Text = wikiAst.ToString();
		}

		private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			linkLabel1.LinkVisited = true;
			Process.Start("http://tasvideos.org/TextFormattingRules.html");
		}
	}
}
