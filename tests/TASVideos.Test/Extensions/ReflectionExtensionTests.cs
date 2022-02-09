using TASVideos.Extensions;

namespace TASVideos.RazorPages.Tests.Extensions;

[TestClass]
public class ReflectionExtensionTests
{
	[TestMethod]
	public void Description_NullReturnsEmpty()
	{
		TestEnum? e = null;
		var actual = e.Description();
		Assert.AreEqual("", actual);
	}

	[TestMethod]
	public void Description_ReturnsDescription()
	{
		TestEnum e = TestEnum.Zero;
		var actual = e.Description();
		Assert.AreEqual("00", actual);
	}

	[TestMethod]
	public void Description_DisplayReturnsDisplayDescription()
	{
		TestEnum e = TestEnum.One;
		var actual = e.Description();
		Assert.AreEqual("01", actual);
	}

	[TestMethod]
	public void Description_Display_NoAttributes_ReturnsName()
	{
		TestEnum e = TestEnum.Two;
		var actual = e.Description();
		Assert.AreEqual("", actual);
	}

	[TestMethod]
	public void EnumDisplayName_NullReturnsEmpty()
	{
		TestEnum? e = null;
		var actual = e.EnumDisplayName();
		Assert.AreEqual("", actual);
	}

	[TestMethod]
	public void EnumDisplayName_DisplayReturnDisplay()
	{
		TestEnum e = TestEnum.One;
		var actual = e.EnumDisplayName();
		Assert.AreEqual("The One", actual);
	}

	[TestMethod]
	public void EnumDisplayName_NoAttribute_ReturnsToString()
	{
		TestEnum e = TestEnum.Two;
		var actual = e.EnumDisplayName();
		Assert.AreEqual("Two", actual);
	}
}
