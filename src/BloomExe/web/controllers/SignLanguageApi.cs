﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Bloom.Api;
using Bloom.Book;
using Bloom.Edit;
using DesktopAnalytics;
using Gecko;
using Gecko.DOM;
using L10NSharp;
using SIL.IO;
using SIL.Progress;
using SIL.Reporting;
using SIL.Windows.Forms.FileSystem;

namespace Bloom.web.controllers
{
	/// <summary>
	/// Handles API calls related to the Sign Language tool (mainly to video).
	/// </summary>
	public class SignLanguageApi
	{
		private EditingModel _model;
		private EditingView _view;
		private readonly BookSelection _bookSelection;
		private readonly PageSelection _pageSelection;
		public EditingView View
		{
			get { return _view;}
			set { _view = value; }
		}
		public EditingModel Model
		{
			get { return _model;}
			set { _model = value; }
		}

		public SignLanguageApi(BookSelection bookSelection, PageSelection pageSelection)
		{
			_bookSelection = bookSelection;
			_pageSelection = pageSelection;
		}


		public void RegisterWithServer(EnhancedImageServer server)
		{
			server.RegisterEndpointHandler("toolbox/recordedVideo", HandleRecordedVideoRequest, true);
			server.RegisterEndpointHandler("toolbox/editVideo", HandleEditVideoRequest, true);
			server.RegisterEndpointHandler("toolbox/deleteVideo", HandleDeleteVideoRequest, true);
			server.RegisterEndpointHandler("toolbox/restoreOriginal", HandleRestoreOriginalRequest, true);
			server.RegisterEndpointHandler("toolbox/saveChangesAndRethinkPageEvent", RethinkPageAndReloadIt, true);
		}

		private void RethinkPageAndReloadIt(ApiRequest request)
		{
			_model.RethinkPageAndReloadIt(request);
		}


		public Book.Book CurrentBook
		{
			get { return _bookSelection.CurrentSelection; }
		}

		// Request from sign language tool, issued when a complete recording has been captured.
		// It is passed as a binary blob that is the actual content that needs to be made into
		// an mp4 file. (At this point we don't try to handle recordings too big for this approach.)
		// We make a file (with an arbitrary guid name) and attempt to make it the recording for the
		// first page element with class bloom-videoContainer.
		private void HandleRecordedVideoRequest(ApiRequest request)
		{
			lock (request)
			{
				var bytes = request.RawPostData;
				var fileName = GetNewVideoFileName();
				var videoFolder = BookStorage.GetVideoDirectoryAndEnsureExistence(CurrentBook.FolderPath);
				var path = Path.Combine(videoFolder, fileName);
				RobustFile.WriteAllBytes(path, bytes);
				var videoContainer = GetSelectedVideoContainer();
				if (videoContainer == null)
				{
					// Enhance: if we end up needing this it should be localizable. But the current plan is to disable
					// video recording if there is no container on the page.
					MessageBox.Show("There's nowhere to put a video on this page. You can find it later at " + fileName);
					request.Failed("nowhere to put video");
					return;
				}

				if (WarnIfVideoCantChange(videoContainer))
				{
					request.Failed("editing not allowed");
					return;
				}

				// Technically this could fail and we might want to report that the post failed.
				// But currently nothing is using the success/fail status, and we don't expect this to fail.
				SaveChangedVideo(videoContainer, path, "Bloom had a problem including that video");
				request.PostSucceeded();
			}
		}


		// Request from sign language tool to restore the original video.
		private void HandleRestoreOriginalRequest(ApiRequest request)
		{
			lock (request)
			{
				var videoContainer = GetSelectedVideoContainer();
				string fileName;
				if (!GetFileNameFromVideoContainer(request, videoContainer, out fileName))
					return; // method reports failure
				var videoPath = Path.Combine(CurrentBook.FolderPath, fileName);
				var originalPath = Path.ChangeExtension(videoPath, "orig");
				if (!RobustFile.Exists(originalPath))
				{
					request.Failed("no original");
					return;
				}
				var newVideoPath = Path.Combine(BookStorage.GetVideoFolderPath(CurrentBook.FolderPath), GetNewVideoFileName()); // Use a new name to defeat caching.
				var newOriginalPath = Path.ChangeExtension(newVideoPath, "orig");
				RobustFile.Move(originalPath, newOriginalPath); // Keep old original associated with new name
				RobustFile.Copy(newOriginalPath, newVideoPath);
				// I'm not absolutely sure we need to get the Video container again on the UI thread, but have had some problems
				// with COM interfaces in a similar situation so it seems safest.
				_view.Invoke((Action)(() => SaveChangedVideo(GetSelectedVideoContainer(), newVideoPath, "Bloom had a problem updating that video")));
				request.PostSucceeded();
			}
		}

		// Request from sign language tool to edit the selected video.
		private void HandleEditVideoRequest(ApiRequest request)
		{
			lock (request)
			{
				var videoContainer = GetSelectedVideoContainer();
				string fileName;
				if (!GetFileNameFromVideoContainer(request, videoContainer, out fileName)) return;
				var videoPath = Path.Combine(CurrentBook.FolderPath, fileName);
				var originalPath = Path.ChangeExtension(videoPath, "orig");
				if (!RobustFile.Exists(videoPath))
				{
					if (RobustFile.Exists(originalPath))
					{
						RobustFile.Copy(originalPath, videoPath);
					}
					else
					{
						request.Failed("missing video");
						return;
					}
				}

				var proc = new Process()
				{
					StartInfo = new ProcessStartInfo()
					{
						FileName = videoPath,
						UseShellExecute = true
					},
					EnableRaisingEvents = true
				};
				var begin = DateTime.Now;
				proc.Exited += (sender, args) =>
				{
					var videoFolderPath = BookStorage.GetVideoFolderPath(CurrentBook.FolderPath);
					var lastModifiedFile = new DirectoryInfo(videoFolderPath)
						.GetFiles("*.mp4")
						.OrderByDescending(f => GetRealLastModifiedTime(f))
						.FirstOrDefault();
					if (lastModifiedFile != null && GetRealLastModifiedTime(lastModifiedFile) > begin)
					{
						var newVideoPath = Path.Combine(videoFolderPath, GetNewVideoFileName()); // Use a new name to defeat caching; prefer our standard type of name.
						RobustFile.Move(Path.Combine(videoFolderPath, lastModifiedFile.Name), newVideoPath);
						var newOriginalPath = Path.ChangeExtension(newVideoPath, "orig");
						if (RobustFile.Exists(originalPath))
						{
							RobustFile.Move(originalPath, newOriginalPath); // Keep old original associated with new name
							RobustFile.Delete(videoPath);
						}
						else
						{
							RobustFile.Move(videoPath, newOriginalPath); // Set up original for the first time.
						}
						// I'm not sure why it fails if we use the videoContainer variable we set above,
						// but somehow QueryInterface on the underlying COM object fails. It's probably something to
						// do with the COM threading model that forbids using it on a thread other than the
						// one that created it.
						_view.Invoke((Action)(() => SaveChangedVideo(GetSelectedVideoContainer(), newVideoPath, "Bloom had a problem updating that video")));
						//_view.Invoke((Action)(()=> RethinkPageAndReloadIt()));
					}
				};
				proc.Start();
				request.PostSucceeded();
			}
		}

		// Request from sign language tool to edit the selected video.
		private void HandleDeleteVideoRequest(ApiRequest request)
		{
			lock (request)
			{
				var videoContainer = GetSelectedVideoContainer();
				string fileName;
				if (!GetFileNameFromVideoContainer(request, videoContainer, out fileName))
				{
					request.Failed("no file found");
					return;
				}

				var videoPath = Path.Combine(CurrentBook.FolderPath, fileName);
				var originalPath = Path.ChangeExtension(videoPath, "orig");
				var label = LocalizationManager.GetString("EditTab.Toolbox.SignLanguage.SelectedVideo", "The selected video", "Appears in the contest \"X will be moved to the recycle bin\"");
				var didDelete = ConfirmRecycleDialog.ConfirmThenRecycle(label, videoPath);
				if (!didDelete)
				{
					request.Failed("user canceled");
					return;

				}
				ConfirmRecycleDialog.Recycle(originalPath);
				_view.Invoke((Action)(() => {
					var video = videoContainer.GetElementsByTagName("video").First(); // should be one, since got a path from it above.
					video.ParentNode.RemoveChild(video);
					_model.SaveNow();
					_view.UpdateSingleDisplayedPage(_pageSelection.CurrentSelection);
					_view.UpdateThumbnailAsync(_pageSelection.CurrentSelection);
				}));
				request.PostSucceeded();
			}
		}

		internal void OnChangeVideo(DomEventArgs ge)
		{
			var target = (GeckoHtmlElement)ge.Target.CastToGeckoElement();
			var videoContainer = target.Parent;
			if (videoContainer == null)
				return; // should never happen
			if (WarnIfVideoCantChange(videoContainer))
				return;

			var videoFiles = LocalizationManager.GetString("EditTab.FileDialogVideoFiles", "Video files");
			using (var dlg = new DialogAdapters.OpenFileDialogAdapter
			{
				Multiselect = false,
				CheckFileExists = true,
				// rather restrictive, but the only type that works in all browsers.
				Filter = String.Format("{0} (*.mp4)|*.mp4", videoFiles)
			})
			{
				var result = dlg.ShowDialog();
				if (result == DialogResult.OK)
				{
					// Check memory for the benefit of developers.  The user won't see anything.
					SIL.Windows.Forms.Reporting.MemoryManagement.CheckMemory(true, "picture chosen or canceled", false);
					if (DialogResult.OK == result)
					{
						// var path = MakePngOrJpgTempFileForImage(dlg.ImageInfo.Image);
						SaveChangedVideo(videoContainer, dlg.FileName, "Bloom had a problem including that video");
						// Warn the user if we're starting to use too much memory.
						SIL.Windows.Forms.Reporting.MemoryManagement.CheckMemory(true, "picture chosen and saved", true);
					}
				}
			}

			Logger.WriteMinorEvent("Changed Video");
		}

		internal bool WarnIfVideoCantChange(GeckoHtmlElement videoContainer)
		{
			if (!_model.CanChangeImages())
			{
				MessageBox.Show(
					LocalizationManager.GetString("EditTab.CantPasteImageLocked",
						"Sorry, this book is locked down so that images cannot be changed.")); // Is it worth another LM string so we can say "videos can't be changed?
				return true;
			}
			string currentPath = HtmlDom.GetVideoElementUrl(videoContainer).NotEncoded;

			if (!_view.CheckIfLockedAndWarn(currentPath))
				return true;
			return false;
		}

		private bool GetFileNameFromVideoContainer(ApiRequest request, GeckoHtmlElement videoContainer, out string fileName)
		{
			fileName = null;
			if (videoContainer == null)
			{
				// Enhance: if we end up needing this it should be localizable. But the current plan is that the button should be
				// disabled if we don't have a recording to edit.
				request.Failed("no video container");
				return false;
			}

			if (WarnIfVideoCantChange(videoContainer))
			{
				request.Failed("editing not allowed");
				return false;
			}

			var videos = videoContainer.GetElementsByTagName("video");
			if (videos.Length == 0)
			{
				request.Failed("no existing video to edit");
				return false;
			}

			var sources = videos[0].GetElementsByTagName("source");
			if (sources.Length == 0 || string.IsNullOrWhiteSpace(sources[0].GetAttribute("src")))
			{
				request.Failed("current video has no source");
				return false;
			}

			fileName = sources[0].GetAttribute("src");
			return true;
		}

		private GeckoHtmlElement GetSelectedVideoContainer()
		{
			var root = _view.Browser.WebBrowser.Document;
			var page = root.GetElementById("page") as GeckoIFrameElement;
			var pageDoc = page.ContentWindow.Document;
			var videoContainer =
				pageDoc.GetElementsByClassName("bloom-videoContainer bloom-selected").FirstOrDefault() as GeckoHtmlElement;
			return videoContainer;
		}

		DateTime GetRealLastModifiedTime(FileInfo info)
		{
			if (info.LastWriteTime > info.CreationTime)
				return info.LastWriteTime;
			else
				return info.CreationTime;
		}

		public static string GetNewVideoFileName()
		{
			return Guid.NewGuid().ToString() + ".mp4";
		}

		internal void SaveChangedVideo(GeckoHtmlElement videoElement, string videoPath, string exceptionMsg)
		{
			try
			{
				ChangeVideo(videoElement, videoPath, new NullProgress());
			}
			catch (System.IO.IOException error)
			{
				ErrorReport.NotifyUserOfProblem(error, error.Message);
			}
			catch (ApplicationException error)
			{
				ErrorReport.NotifyUserOfProblem(error, error.Message);
			}
			catch (Exception error)
			{
				ErrorReport.NotifyUserOfProblem(error, exceptionMsg);
			}
		}

		public void ChangeVideo(GeckoHtmlElement videoContainer, string videoPath, IProgress progress)
		{
			try
			{
				Logger.WriteMinorEvent("Starting ChangeVideo {0}...", videoPath);
				var editor = new PageEditingModel();
				editor.ChangeVideo(CurrentBook.FolderPath, new ElementProxy(videoContainer), videoPath, progress);

				// We need to save so that when asked by the thumbnailer, the book will give the proper image
				_model.SaveNow();

				// At some point we might clean up unused videos here. If so, be careful about
				// losing license info if we ever put video information without it in the clipboard.
				// Compare the parallel situation for images, BL-3717

				// But after saving, we need the non-cleaned version back there
				_view.UpdateSingleDisplayedPage(_pageSelection.CurrentSelection);

				_view.UpdateThumbnailAsync(_pageSelection.CurrentSelection);
				Analytics.Track("Change Video");
				Logger.WriteEvent("ChangeVideo {0}...", videoPath);

			}
			catch (Exception e)
			{
				var msg = LocalizationManager.GetString("Errors.ProblemImportingVideo", "Bloom had a problem importing this video.");
				ErrorReport.NotifyUserOfProblem(e, msg + Environment.NewLine + e.Message);
			}
		}
	}
}
