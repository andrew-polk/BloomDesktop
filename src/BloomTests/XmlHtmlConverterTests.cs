﻿using System;
using System.IO;
using System.Linq;
using System.Xml;
using Bloom;
using NUnit.Framework;
using Palaso.IO;

namespace BloomTests
{
	[TestFixture]
	public class XmlHtmlConverterTests
	{
		[Test]
		public void GetXmlDomFromHtml_MinimalWellFormedHtml5()
		{

			var dom = XmlHtmlConverter.GetXmlDomFromHtml("<!DOCTYPE html><html></html>", false);
			AssertThatXmlIn.Dom(dom).HasSpecifiedNumberOfMatchesForXpath("//html", 1);//makes sure no namespace was inserted (or does it? what if that assert is too smart))
			Assert.AreEqual("<html><head><title></title></head><body></body></html>",dom.OuterXml);
		}
		[Test]
		public void GetXmlDomFromHtml_HasOpenLinkElement_Closes()
		{
			var dom = XmlHtmlConverter.GetXmlDomFromHtml("<!DOCTYPE html><html><head>    <link rel='stylesheet' href='basePage.css' type='text/css'> </head></html>", false);
			AssertThatXmlIn.Dom(dom).HasSpecifiedNumberOfMatchesForXpath("//html", 1);//makes sure no namespace was inserted (or does it? what if that assert is too smart))
			Assert.AreEqual("<html><head><link rel=\"stylesheet\" href=\"basePage.css\" type=\"text/css\" /><title></title></head><body></body></html>", dom.OuterXml);
		}
		[Test]
		public void GetXmlDomFromHtml_HasErrors_ReportsError()
		{
			Assert.Throws<ApplicationException>(() => XmlHtmlConverter.GetXmlDomFromHtml("<!DOCTYPE html><html><head>    <blahblah> </head></html>", false));
		}

		[Test]
		public void SaveAsHTML_HasXHTMLSelfClosingDiv_ChangesToHTMLStandard()
		{
			var dom = new XmlDocument();
			dom.LoadXml("<html><body><div data-book='test'/></body></html>");
			using(var temp = new TempFile())
			{
				XmlHtmlConverter.SaveDOMAsHtml5(dom, temp.Path);
				var text = File.ReadAllText(temp.Path);
				Assert.IsTrue(text.Contains("</div>"), text);
			}
		}

		[Test]
		public void GetXmlDomFromHtml_HasBrTags_TagsNotDoubled()
		{
			const string html = "<!DOCTYPE html><html><head></head><body><div><br></br></div></body></html>";
			var dom = XmlHtmlConverter.GetXmlDomFromHtml(html, false);
			var found = 0;
			if (dom.DocumentElement != null)
			{
				var xml = dom.DocumentElement.InnerXml;
				found = xml.Select((c, i) => xml.Substring(i)).Count(sub => sub.StartsWith("<br />"));
			}
			Assert.AreEqual(1, found);
		}

		/// <summary>
		/// We don't particularly want it to convert newlines to spaces, as this test demonstrates that it does.
		/// However, that is harmless and unlikely to occur (we're not counting on newlines for formatting)
		/// so I haven't tried to prevent it.
		/// I do want to test that an existing newline is not simply removed, leaving no white space.
		/// </summary>
		[Test]
		public void GetXmlDomFromHtml_HasBandItags_NoExtraNewlinesBefore()
		{
			const string html = @"<!DOCTYPE html><html><head></head><body><div>one<b>two</b>three<i>four</i>five
<b>six</b>seven <i>eight</i>nine</div></body></html>";
			var dom = XmlHtmlConverter.GetXmlDomFromHtml(html, false);
			Assert.That(dom.InnerXml, Is.StringContaining(@"one<b>two</b>three<i>four</i>five <b>six</b>seven <i>eight</i>nine"));
		}
	}
}
