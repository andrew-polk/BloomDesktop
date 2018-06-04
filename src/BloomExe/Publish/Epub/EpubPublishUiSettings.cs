﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Bloom.Publish.Epub
{
	/// <summary>
	/// Wraps the persisted properties of the (HTML) EpubPublishUI, being sure JsonConvert will serialize and deserialize it
	/// as that code expects. Defines equality operators for testing whether two instances are equivalent.
	/// </summary>
	public class EpubPublishUiSettings
	{
		[JsonConverter(typeof(StringEnumConverter))]
		public EpubMaker.ImageDescriptionPublishing imageDescriptionPublishing;
		public bool removeFontSizes;

		public override bool Equals(object obj)
		{
			var other = obj as EpubPublishUiSettings;
			if (other == null)
				return false;
			return other.imageDescriptionPublishing == this.imageDescriptionPublishing
			       && other.removeFontSizes == this.removeFontSizes;
		}

		public override int GetHashCode()
		{
			return imageDescriptionPublishing.GetHashCode() ^ removeFontSizes.GetHashCode();
		}

		public static bool operator ==(EpubPublishUiSettings a, EpubPublishUiSettings b)
		{
			if (Object.ReferenceEquals(a, b)) return true; // same object, including both null
			// If one is null, but not both, return false. The casts are needed to prevent
			// calling this method again recursively (and infinitely).
			if (((object)a == null) || ((object)b == null))
			{
				return false;
			}

			return a.Equals(b);
		}

		public static bool operator !=(EpubPublishUiSettings a, EpubPublishUiSettings b)
		{
			return !(a == b);
		}
	}
}
