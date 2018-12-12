using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Bloom.Edit;
using SIL.CommandLineProcessing;
using SIL.IO;
using SIL.Progress;

namespace Bloom.Publish
{
	public class AudioProcessor
	{
		// We record as .wav but convert to .mp3 for publication
		public static readonly string[] NarrationAudioExtensions = {".wav", ".mp3"};

		public static readonly string[] PublishableNarrationAudioExtensions = {".mp3"};

		public static readonly string[] MusicFileExtensions = { ".mp3", ".ogg", ".wav" };

		public static readonly string[] AudioFileExtensions = NarrationAudioExtensions.Union(MusicFileExtensions).ToArray();

		private static LameEncoder _mp3Encoder;

		/// <summary>
		/// Compares timestamps on .wav files and .mp3 files to see if we need to update any .mp3 files.
		/// Be aware that if you are staging audio files in preparation for publishing, the bookFolderPath
		/// should be the original book folder files, not the staged files. Otherwise you may get unexpected
		/// results. (See BL-5437)
		/// </summary>
		public static bool IsAnyCompressedAudioMissing(string bookFolderPath, XmlDocument dom)
		{
			return !IsTrueForAllAudioSentenceElements(bookFolderPath, dom,
				(wavpath, mp3path) => !Mp3IsNeeded(wavpath, mp3path));
		}

		/// <summary>
		/// Compress all the existing wav files into mp3s, if they aren't already compressed
		/// </summary>
		/// <returns>true if everything is compressed</returns>
		public static bool TryCompressingAudioAsNeeded(string bookFolderPath, XmlDocument dom)
		{
			var watch = Stopwatch.StartNew();
			bool result = IsTrueForAllAudioSentenceElements(bookFolderPath, dom,
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

		private static bool IsTrueForAllAudioSentenceElements(string bookFolderPath, XmlDocument dom, Func<string, string, bool> predicate)
		{
			var audioFolderPath = GetAudioFolderPath(bookFolderPath);
			return Book.HtmlDom.SelectAudioSentenceElements(dom.DocumentElement)
				.Cast<XmlElement>()
				.All(span =>
				{
					var wavpath = Path.Combine(audioFolderPath, Path.ChangeExtension(span.Attributes["id"]?.Value, "wav"));
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
		/// </summary>
		public static string MakeCompressedAudio(string wavPath)
		{
			if(_mp3Encoder == null)
				_mp3Encoder = new LameEncoder();

			return _mp3Encoder.Encode(wavPath);
		}

		public static bool DoesWavOrMp3Exist(string bookFolderPath, string recordingSegmentId)
		{
			if (recordingSegmentId == null)
			{
				return false;
			}

			var root = GetAudioFolderPath(bookFolderPath);

			foreach(var ext in NarrationAudioExtensions)
			{
				var path = Path.Combine(root, Path.ChangeExtension(recordingSegmentId, ext));
				if(RobustFile.Exists(path))
					return true;
			}
			return false;
		}

		/// <summary>
		/// Returns a path to the publishable audio file corresponding to the given segment (or null if none exists).
		/// If there is only a wav file and no mp3 file, the mp3 file is generated on the fly.
		/// </summary>
		public static string GetPublishableAudioPathIfExists(string bookFolderPath, string recordingSegmentId)
		{
			if (recordingSegmentId == null)
				return null;

			var root = GetAudioFolderPath(bookFolderPath);

			foreach (var ext in PublishableNarrationAudioExtensions)
			{
				var path = Path.Combine(root, Path.ChangeExtension(recordingSegmentId, ext));
				if (RobustFile.Exists(path))
					return path;
			}

			// Some old books could have a wav file without the corresponding mp3 file.
			// If that is the case, generate the mp3 now and return it.
			var wavPath = Path.Combine(root, Path.ChangeExtension(recordingSegmentId, "wav"));
			if (RobustFile.Exists(wavPath))
				return MakeCompressedAudio(wavPath);

			return null;
		}

		/// <summary>
		/// Merge the specified input files into the specified output file. Returns null if successful,
		/// a string that may be useful in debugging if not.
		/// </summary>
		public static string MergeAudioFiles(IEnumerable<string> mergeFiles, string combinedAudioPath)
		{
			var ffmpeg = "/usr/bin/ffmpeg"; // standard Linux location
			if (SIL.PlatformUtilities.Platform.IsWindows)
				ffmpeg = Path.Combine(BloomFileLocator.GetCodeBaseFolder(), "ffmpeg.exe");
			if (RobustFile.Exists(ffmpeg))
			{
				var argsBuilder = new StringBuilder("-i \"concat:");
				foreach (var path in mergeFiles)
					argsBuilder.Append(path + "|");
				argsBuilder.Length--;

				argsBuilder.Append($"\" -c copy \"{combinedAudioPath}\"");
				var result = CommandLineRunner.Run(ffmpeg, argsBuilder.ToString(), "", 60 * 10, new NullProgress());
				var output = result.ExitCode != 0 ? result.StandardError : null;
				if (output != null)
				{
					RobustFile.Delete(combinedAudioPath);
				}

				return output;
			}

			return "Could not find ffmpeg";
		}

		public static bool HasAudioFileExtension(string fileName)
		{
			var extension = Path.GetExtension(fileName);
			return !String.IsNullOrEmpty(extension) && AudioFileExtensions.Contains(extension.ToLowerInvariant());
		}

		public static bool HasBackgroundMusicFileExtension(string fileName)
		{
			var extension = Path.GetExtension(fileName);
			return !String.IsNullOrEmpty(extension) && MusicFileExtensions.Contains(extension.ToLowerInvariant());
		}
	}
}
