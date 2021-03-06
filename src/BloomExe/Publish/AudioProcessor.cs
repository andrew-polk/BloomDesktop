﻿using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml;
using Bloom.Edit;
using SIL.IO;
using SIL.Progress;
using SIL.Xml;

namespace Bloom.Publish
{
	public class AudioProcessor
	{

		private static LameEncoder _mp3Encoder;

		//extracted so unit test can override
		public static Func<string, string> _compressorMethod = MakeCompressedAudio;

		/// <summary>
		/// Compares timestamps on .wav files and .mp3 files to see if we need to update any .mp3 files.
		/// Be aware that if you are staging audio files in preparation for publishing, the bookFolderPath
		/// should be the original book folder files, not the staged files. Otherwise you may get unexpected
		/// results. (See BL-5437)
		/// </summary>
		public static bool IsAnyCompressedAudioMissing(string bookFolderPath, XmlDocument dom)
		{
			if (!LameEncoder.IsAvailable())
				return true;

			return !GetTrueForAllAudioSpans(bookFolderPath, dom,
				(wavpath, mp3path) => !Mp3IsNeeded(wavpath, mp3path));
		}

		/// <summary>
		/// Compress all the existing wav files into mp3s, if they aren't already compressed
		/// </summary>
		/// <returns>true if everything is compressed</returns>
		public static bool TryCompressingAudioAsNeeded(string bookFolderPath, XmlDocument dom)
		{
			var watch = Stopwatch.StartNew();
			bool result = GetTrueForAllAudioSpans(bookFolderPath, dom,
				(wavpath, mp3path) =>
				{
					if (Mp3IsNeeded(wavpath, mp3path))
					{
						return MakeCompressedAudio(wavpath) != null;
					}
					return true; // already have an up-to-date mp3 (or can't make one because there's no wav)
				});
			watch.Stop();
			Debug.WriteLine("compressing audio took " + watch.ElapsedMilliseconds);
			return result;
		}

		// We only need to make an MP3 if we actually have a corresponding wav file. If not, it's just a hypothetical recording that
		// the user could have made but didn't.
		// Assuming we have a wav file and thus want a corresponding mp3, we need to make it if either it does not exist
		// or it is out of date (older than the wav file).
		// It's of course possible that although it is newer the two don't correspond. I don't know any way to reliably prevent that
		// except to regenerate them all on every publish event, but that is quite time-consuming.
		private static bool Mp3IsNeeded(string wavpath, string mp3path)
		{
			return RobustFile.Exists(wavpath) &&
			       (!RobustFile.Exists(mp3path) || (new FileInfo(wavpath).LastWriteTimeUtc) > new FileInfo(mp3path).LastWriteTimeUtc);
		}

		private static bool GetTrueForAllAudioSpans(string bookFolderPath, XmlDocument dom, Func<string, string, bool> predicate)
		{
			var audioFolderPath = GetAudioFolderPath(bookFolderPath);
			return dom.SafeSelectNodes("//span[@id]")
				.Cast<XmlElement>()
				.All(span =>
				{
					var wavpath = Path.Combine(audioFolderPath, Path.ChangeExtension(span.Attributes["id"].Value, "wav"));
					var mp3path = Path.ChangeExtension(wavpath, "mp3");
					return predicate(wavpath, mp3path);
				});
		}

		internal static string GetAudioFolderPath(string bookFolderPath)
		{
			return Path.Combine(bookFolderPath, "audio");
		}

		/// <summary>
		/// Make a compressed audio file for the specified .wav file.
		/// (Or return null if it can't be done because we don't have a LAME package installed.)
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		// internal and virtual for testing.
		private static string MakeCompressedAudio(string wavPath)
		{
			// We have a recording, but not compressed. Possibly the LAME package was installed after
			// the recordings were made. Compress it now.
			if(_mp3Encoder == null)
			{
				if(!LameEncoder.IsAvailable())
				{
					return null;
				}
				_mp3Encoder = new LameEncoder();
			}
			_mp3Encoder.Encode(wavPath, wavPath.Substring(0, wavPath.Length - 4), new NullProgress());
			return Path.ChangeExtension(wavPath, "mp3");
		}

		public static string GetOrCreateCompressedAudioIfWavExists(string bookFolderPath, string recordingSegmentId)
		{
			var root = GetAudioFolderPath(bookFolderPath);
			var extensions = new[] {"mp3", "mp4"}; // .ogg,, .wav, ...?

			foreach(var ext in extensions)
			{
				var path = Path.Combine(root, Path.ChangeExtension(recordingSegmentId, ext));
				if(RobustFile.Exists(path))
					return path;
			}
			var wavPath = Path.Combine(root, Path.ChangeExtension(recordingSegmentId, "wav"));
			if(!RobustFile.Exists(wavPath))
				return null;
			return _compressorMethod(wavPath);
		}

		public static bool GetWavOrMp3Exists(string bookFolderPath, string recordingSegmentId)
		{
			var root = GetAudioFolderPath(bookFolderPath);
			var extensions = new[] {"wav", "mp3"}; // .ogg,, .wav, ...?

			foreach(var ext in extensions)
			{
				var path = Path.Combine(root, Path.ChangeExtension(recordingSegmentId, ext));
				if(RobustFile.Exists(path))
					return true;
			}
			return false;
		}
	}
}
