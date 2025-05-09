using System.Text;

namespace TASVideos.Common.Tests;

[TestClass]
public sealed class HtmlWriterTests : IDisposable
{
	private readonly ThreadLocal<HtmlWriter> _w = new();

	private HtmlWriter W => _w.Value!;

	private void AssertOutputEquals(ReadOnlySpan<char> expected)
	{
		AssertOutputStartsWith(expected, out var continues);
		Assert.IsFalse(continues, "output does not continue (Equals, not StartsWith)");
	}

	private void AssertOutputIsEmpty()
		=> Assert.AreEqual(0, GetOutput().Length);

	private void AssertOutputIsNotEmpty()
		=> Assert.AreNotEqual(0, GetOutput().Length);

	private void AssertOutputStartsWith(ReadOnlySpan<char> expected, out bool continues)
	{
		static void Fail(ReadOnlySpan<char> ex, ReadOnlySpan<char> ac)
			=> Assert.Fail($"expected string prefixed with\n{ex}\nbut got\n{ac}");

		// I've written a `StringBuilder.StartsWith` extension which avoids the `ToString` copying, but it's not much faster, and I'd need to copy for the error message anyway --yoshi
		var str = GetOutput().ToString();
		if (str.Length < expected.Length)
		{
			Fail(expected, str);
		}

		continues = expected.Length < str.Length;
		if (continues)
		{
			if (!expected.SequenceEqual(str.AsSpan(start: 0, length: expected.Length)))
			{
				Fail(expected, $"{str.AsSpan(start: 0, length: Math.Min(expected.Length + 50, str.Length))}...");
			}
		}
		else if (!expected.SequenceEqual(str))
		{
			Fail(expected, str);
		}
	}

	public void Dispose() => _w.Value?.BaseWriter.Dispose();

	private StringBuilder GetOutput() => ((StringWriter)W.BaseWriter).GetStringBuilder();

	[TestInitialize]
	public void InitTextWriter()
	{
		Dispose();
		_w.Value = new(new StringWriter());
	}

	[TestMethod]
	public void TestComplexExample()
	{
		W.OpenTag("div");
		W.Attribute("class", "col-auto mb-4 mb-md-0 mx-auto text-center text-md-start");
		{
			W.OpenTag("a");
			W.Attribute("href", "https://www.youtube.com/watch?v=LDx4KpYdykg");
			W.Attribute("target", "_blank");
			{
				W.VoidTag("img");
				W.Attribute("src", "/media/3216M.png");
				W.Attribute("class", "w-100 pixelart-image");
				W.Attribute("loading", "lazy");
			}

			W.CloseTag("a");

			W.OpenTag("a");
			W.Attribute("href", "https://www.youtube.com/watch?v=LDx4KpYdykg");
			W.Attribute("class", "btn btn-primary btn-sm mt-1");
			W.Attribute("target", "_blank");
			{
				W.OpenTag("i");
				W.Attribute("class", "fa fa-external-link");
				W.CloseTag("i");

				W.Text(" Watch on YouTube");
			}

			W.CloseTag("a");

			W.OpenTag("div");
			{
				W.OpenTag("a");
				W.Attribute("class", "btn btn-secondary btn-sm mt-1");
				W.Attribute("href", "/5206S");
				{
					W.OpenTag("i");
					W.Attribute("class", "fa fa-info-circle");
					W.CloseTag("i");

					W.Text(" Author notes");
				}

				W.CloseTag("a");

				W.OpenTag("a");
				W.Attribute("class", "btn btn-secondary btn-sm mt-1");
				W.Attribute("href", "/Forum/Topics/18324");
				{
					W.OpenTag("i");
					W.Attribute("class", "fa-regular fa-comments");
					W.CloseTag("i");

					W.Text(" Discuss");
				}

				W.CloseTag("a");
			}

			W.CloseTag("div");

			W.OpenTag("a");
			W.Attribute("class", "btn btn-warning btn-sm mt-1");
			W.Attribute("href", "/Publications/Rate/3216");
			{
				W.OpenTag("i");
				W.Attribute("class", "fa-regular fa-star");
				W.CloseTag("i");

				W.Text(" ");

				W.OpenTag("span");
				W.Attribute("id", "overallRating-3216");
				W.Text("9.76");
				W.CloseTag("span");

				W.Text(" / 10");
			}

			W.CloseTag("a");

			W.OpenTag("a");
			W.Attribute("class", "align-bottom");
			W.Attribute("href", "/Awards/2016#tas_gba_2016");
			W.Attribute("title", "Award - GBA TAS of 2016");
			{
				W.VoidTag("img");
				W.Attribute("style", "max-height: 48px;");
				W.Attribute("src", "/awards/tas_gba_2016.png");
				W.Attribute("alt", "GBA TAS of 2016");
				W.Attribute("loading", "lazy");
			}

			W.CloseTag("a");

			W.OpenTag("a");
			W.Attribute("class", "align-bottom");
			W.Attribute("href", "/Awards/2016#tas_lucky_2016");
			W.Attribute("title", "Award - Lucky TAS of 2016");
			{
				W.VoidTag("img");
				W.Attribute("style", "max-height: 48px;");
				W.Attribute("src", "/awards/tas_lucky_2016.png");
				W.Attribute("alt", "Lucky TAS of 2016");
				W.Attribute("loading", "lazy");
			}

			W.CloseTag("a");
		}

		W.CloseTag("div");

		W.AssertFinished();
		AssertOutputEquals("""
		<div class="col-auto mb-4 mb-md-0 mx-auto text-center text-md-start">
			<a href="https://www.youtube.com/watch?v=LDx4KpYdykg" target="_blank"><img src="/media/3216M.png" loading="lazy" class="w-100 pixelart-image"></a>
			<a href="https://www.youtube.com/watch?v=LDx4KpYdykg" target="_blank" class="btn btn-primary btn-sm mt-1"><i class="fa fa-external-link"></i> Watch on YouTube</a>
			<div>
				<a href="/5206S" class="btn btn-secondary btn-sm mt-1"><i class="fa fa-info-circle"></i> Author notes</a>
				<a href="/Forum/Topics/18324" class="btn btn-secondary btn-sm mt-1"><i class="fa-regular fa-comments"></i> Discuss</a>
			</div>
			<a href="/Publications/Rate/3216" class="btn btn-warning btn-sm mt-1"><i class="fa-regular fa-star"></i> <span id="overallRating-3216">9.76</span> / 10</a>
			<a href="/Awards/2016#tas_gba_2016" title="Award - GBA TAS of 2016" class="align-bottom"><img style="max-height: 48px;" src="/awards/tas_gba_2016.png" alt="GBA TAS of 2016" loading="lazy"></a>
			<a href="/Awards/2016#tas_lucky_2016" title="Award - Lucky TAS of 2016" class="align-bottom"><img style="max-height: 48px;" src="/awards/tas_lucky_2016.png" alt="Lucky TAS of 2016" loading="lazy"></a>
		</div>
		""".Replace("\r", "").Replace("\n", "").Replace("\t", ""));
	}

	[DataRow(false)]
	[DataRow(true)]
	[TestMethod]
	public void TestMayNotOpenChildInsideForeignContent(bool isVoidElement)
	{
		W.OpenTag("script");
		W.Text("console.log(location.href);");
		_ = Assert.ThrowsExactly<InvalidOperationException>(isVoidElement
			? () => W.VoidTag("br")
			: () => W.OpenTag("p"));
		W.CloseTag("script");
		W.AssertFinished();
		AssertOutputEquals("<script>console.log(location.href);</script>");
	}

	[DataRow("")]
	[DataRow(".")]
	[DataRow("-")]
	[DataRow("/")]
	[DataRow("<")]
	[DataRow("ABBR")]
	[DataRow("TABLE")]
	[DataRow("Tableau")]
	[DataRow("WebComponent")]
	[DataRow("big box")]
	[TestMethod]
	public void TestTagNameRegExInvalid(string tagName)
	{
		_ = Assert.ThrowsExactly<InvalidOperationException>(() => W.OpenTag(tagName), "as normal element");
		_ = Assert.ThrowsExactly<InvalidOperationException>(() => W.VoidTag(tagName), "as void element");
		W.AssertFinished();
		AssertOutputIsEmpty();
	}

	[DataRow(false, "div")]
	[DataRow(false, "footer")]
	[DataRow(false, "p")]
	[DataRow(false, "progress")]
	[DataRow(true, "br")]
	[DataRow(true, "hr")]
	[DataRow(true, "img")]
	[TestMethod]
	public void TestTagNameRegExValid(bool isVoidElement, string tagName)
	{
		if (isVoidElement)
		{
			W.VoidTag(tagName);
		}
		else
		{
			W.OpenTag(tagName);
			W.CloseTag(tagName);
		}

		W.AssertFinished();
		AssertOutputIsNotEmpty();
	}

	[DataRow(false, "div")]
	[DataRow(false, "footer")]
	[DataRow(false, "p")]
	[DataRow(false, "progress")]
	[DataRow(true, "br")]
	[DataRow(true, "hr")]
	[DataRow(true, "img")]
	[TestMethod]
	public void TestMayNotUseOpenWithVoidElementOrViceVersa(bool isVoidElement, string tagName)
	{
		_ = Assert.ThrowsExactly<InvalidOperationException>(isVoidElement
			? () => W.OpenTag(tagName)
			: () => W.VoidTag(tagName));
		W.AssertFinished();
		AssertOutputIsEmpty();
	}

	[TestMethod]
	public void TestCloseNormalisesCase()
	{
		W.OpenTag("header");
		W.CloseTag("hEAdEr");
		W.AssertFinished();
		AssertOutputEquals("<header></header>");
	}

	[TestMethod]
	public void TestMismatchedClose()
	{
		var sNothingToClose = Assert.ThrowsExactly<InvalidOperationException>(() => W.CloseTag("div"), "nothing to close")
			.Message;
		W.AssertFinished();
		AssertOutputIsEmpty();

		W.VoidTag("br");
		var sCantCloseVoid = Assert.ThrowsExactly<InvalidOperationException>(() => W.CloseTag("br"), "can't close void")
			.Message;
		Assert.AreNotEqual(sCantCloseVoid, sNothingToClose, "\"can't close void\" distinct from \"nothing to close\"");
		W.AssertFinished();
		AssertOutputEquals("<br>");

		W.OpenTag("p");
		var sMismatchedClose = Assert.ThrowsExactly<InvalidOperationException>(() => W.CloseTag("span"), "mismatched close")
			.Message;
		Assert.AreNotEqual(sMismatchedClose, sCantCloseVoid, "\"mismatched close\" distinct from \"can't close void\"");
		Assert.AreNotEqual(sMismatchedClose, sNothingToClose, "\"mismatched close\" distinct from \"nothing to close\"");
		W.CloseTag("p");
		W.AssertFinished();
		AssertOutputEquals("<br><p></p>");
	}

	[TestMethod]
	public void TestNothingToAddAttrTo()
	{
		_ = Assert.ThrowsExactly<InvalidOperationException>(() => W.Attribute("id", "elem1"), "at start");
		W.AssertFinished();
		AssertOutputIsEmpty();

		W.OpenTag("span");
		W.Text("Hello, world!");
		_ = Assert.ThrowsExactly<InvalidOperationException>(() => W.Attribute("id", "elem2"), "after opening tag");
		W.CloseTag("span");
		W.AssertFinished();
		AssertOutputEquals("<span>Hello, world!</span>");

		W.OpenTag("div");
		W.CloseTag("div");
		_ = Assert.ThrowsExactly<InvalidOperationException>(() => W.Attribute("id", "elem3"), "after all closed");
		W.AssertFinished();
		AssertOutputEquals("<span>Hello, world!</span><div></div>");
	}

	[DataRow("href", "/Forum")]
	[DataRow("data-bs-target", "#dropdown")]
	[DataRow("made-up", "example")]
	[DataRow("class", "big small")]
	[DataRow("style", "background-color: red;")]
	[TestMethod]
	public void TestAttrNameRegEx(string name, string value)
	{
		W.OpenTag("a");
		W.Attribute(name, value);
		W.CloseTag("a");
		W.AssertFinished();
		AssertOutputIsNotEmpty(); // could assert it contains the name and value I guess?
	}

	[TestMethod]
	public void TestAttributeValueEscaping()
	{
		W.OpenTag("a");
		W.Attribute("class", """break="\"\\&yes;><""");
		W.Attribute("password", """hunter2" authenticated="true" role="admin" break="\"\\&yes;><""");
		W.CloseTag("a");
		W.AssertFinished();
		AssertOutputEquals("""<a password="hunter2&quot; authenticated=&quot;true&quot; role=&quot;admin&quot; break=&quot;\&quot;\\&amp;yes;>&lt;" class="break=&quot;\&quot;\\&amp;yes;>&lt;"></a>"""); // I'm not proud to say I copied this from the actual value... --yoshi
	}

	[TestMethod]
	public void TestClassListHandling()
	{
		W.OpenTag("span");
		W.Attribute("class", "small");
		W.Attribute("class", "big");
		W.Attribute("id", "elem1");
		W.Attribute("class", "small");
		W.Text("Hello, world!");
		W.CloseTag("span");
		W.AssertFinished();
		AssertOutputEquals("""<span id="elem1" class="small big">Hello, world!</span>""");
	}

	[TestMethod]
	public void TestRelListHandling()
	{
		W.VoidTag("link");
		W.Attribute("rel", "nofollow external");
		W.Attribute("rel", "noopener external");
		W.AssertFinished();
		AssertOutputEquals("""<link rel="nofollow external noopener">""");
	}

	[TestMethod]
	public void TestInnerText()
	{
		W.OpenTag("a");
		W.Attribute("href", "./");
		{
			W.Text("click to go nowhere");
		}

		W.CloseTag("a");

		W.OpenTag("script");
		{
			W.Text("""document.getElementsByTagName("a")[0].onclick = () => location.assign("//evil.example");""");
		}

		W.CloseTag("script");

		W.AssertFinished();
		AssertOutputEquals("""<a href="./">click to go nowhere</a><script>document.getElementsByTagName("a")[0].onclick = () => location.assign("//evil.example");</script>""");
	}
}
