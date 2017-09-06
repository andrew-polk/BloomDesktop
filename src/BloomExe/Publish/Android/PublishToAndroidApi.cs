﻿using System.Collections.Generic;
using System.Linq;
using Bloom.Api;
using Bloom.Book;
using Bloom.Properties;
using Bloom.Publish.Android.file;
#if !__MonoCS__
using Bloom.Publish.Android.usb;
#endif
using Bloom.Publish.Android.wifi;
using Bloom.web;
using DesktopAnalytics;

namespace Bloom.Publish.Android
{
	/// <summary>
	/// Handles api request dealing with the publishing of books to an Android device
	/// </summary>
	public class PublishToAndroidApi
	{
		private const string kApiUrlPart = "publish/android/";
		private const string kWebsocketStateId = "publish/android/state";
		private readonly WiFiPublisher _wifiPublisher;
#if !__MonoCS__
		private readonly UsbPublisher _usbPublisher;
#endif
		private readonly BloomWebSocketServer _webSocketServer;

		public PublishToAndroidApi(BloomWebSocketServer bloomWebSocketServer)
		{
			_webSocketServer = bloomWebSocketServer;
			var progress = new WebSocketProgress(_webSocketServer);
			_wifiPublisher = new WiFiPublisher(progress);
#if !__MonoCS__
			_usbPublisher = new UsbPublisher(progress)
			{
				Stopped = () => SetState("stopped")
			};
#endif
		}

		public void RegisterWithServer(EnhancedImageServer server)
		{
			// This is just for storing the user preference of method
			// If we had a couple of these, we could just have a generic preferences api
			// that browser-side code could use.
			server.RegisterEndpointHandler(kApiUrlPart + "method", request =>
			{
				if(request.HttpMethod == HttpMethods.Get)
				{
					var method = Settings.Default.PublishAndroidMethod;
					if(!new string[]{"wifi", "usb", "file"}.Contains(method))
					{
						method = "wifi";
					}
					request.ReplyWithText(method);
				}
				else // post
				{
					Settings.Default.PublishAndroidMethod = request.RequiredPostString();
					request.PostSucceeded();
				}
			}, true);

#if !__MonoCS__
			server.RegisterEndpointHandler(kApiUrlPart + "usb/start", request =>
			{
				SetState("UsbStarted");
				_usbPublisher.Connect(request.CurrentBook);
				request.PostSucceeded();
			}, true);

			server.RegisterEndpointHandler(kApiUrlPart + "usb/stop", request =>
			{
				_usbPublisher.Stop();
				SetState("stopped");
				request.PostSucceeded();
			}, true);
#endif
			server.RegisterEndpointHandler(kApiUrlPart + "wifi/start", request =>
			{
				_wifiPublisher.Start(request.CurrentBook, request.CurrentCollectionSettings);
				SetState("ServingOnWifi");
				request.PostSucceeded();
			}, true);

			server.RegisterEndpointHandler(kApiUrlPart + "wifi/stop", request =>
			{
				_wifiPublisher.Stop();
				SetState("stopped");
				request.PostSucceeded();
			}, true);

			server.RegisterEndpointHandler(kApiUrlPart + "file/save", request =>
			{
				FilePublisher.Save(request.CurrentBook);
				SetState("stopped");
				request.PostSucceeded();
			}, true);

			server.RegisterEndpointHandler(kApiUrlPart + "cleanup", request =>
			{
#if !__MonoCS__
				_usbPublisher.Stop();
#endif
				_wifiPublisher.Stop();
				SetState("stopped");
				request.PostSucceeded();
			}, true);
		}

		private void SetState(string state)
		{
			_webSocketServer.Send(kWebsocketStateId, state);
		}

		public static void ReportAnalytics(string mode, Book.Book book)
		{
			Analytics.Track("Publish Android", new Dictionary<string, string>() {{"mode", mode}, {"BookId", book.ID}, { "Country", book.CollectionSettings.Country}, {"Language", book.CollectionSettings.Language1Iso639Code}});
		}
	}
}
