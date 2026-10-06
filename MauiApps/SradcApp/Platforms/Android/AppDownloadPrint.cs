using Android.App;
using Android.Content;
using Android.OS;
using Android.Print;
using Android.Webkit;
using Android.Widget;
using Java.Interop;
using AUri = Android.Net.Uri;
using AWebView = Android.Webkit.WebView;
using Environment = Android.OS.Environment;

namespace SradcApp;

/// <summary>
/// WebView file downloads via Android DownloadManager (public Downloads folder + status notification).
/// Keeps session cookies so authenticated portal downloads work.
/// </summary>
public sealed class AppDownloadListener : Java.Lang.Object, IDownloadListener
{
	public void OnDownloadStart(string? url, string? userAgent, string? contentDisposition, string? mimeType, long contentLength)
	{
		try
		{
			if (string.IsNullOrWhiteSpace(url))
				return;

			// blob: / data: cannot go through DownloadManager — the page's own JS must handle those
			if (url.StartsWith("blob:", StringComparison.OrdinalIgnoreCase)
				|| url.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
			{
				Toast.MakeText(Android.App.Application.Context, "এই ফাইলটি অ্যাপে সরাসরি ডাউনলোড করা যায় না / Cannot download this file type here", ToastLength.Long)?.Show();
				return;
			}

			var activity = Platform.CurrentActivity;
			if (activity is null)
				return;

			_ = StartDownloadAsync(activity, url, userAgent, contentDisposition, mimeType);
		}
		catch (Exception ex)
		{
			try { Toast.MakeText(Android.App.Application.Context, "Download failed: " + ex.Message, ToastLength.Long)?.Show(); } catch { }
		}
	}

	static async Task StartDownloadAsync(Activity activity, string url, string? userAgent, string? contentDisposition, string? mimeType)
	{
		// Android 13+: notification permission so the user sees download progress
		if (Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu)
		{
			var status = await Permissions.CheckStatusAsync<Permissions.PostNotifications>();
			if (status != PermissionStatus.Granted)
				status = await MainThread.InvokeOnMainThreadAsync(() => Permissions.RequestAsync<Permissions.PostNotifications>());
		}

		MainThread.BeginInvokeOnMainThread(() =>
		{
			try
			{
				Enqueue(activity, url, userAgent, contentDisposition, mimeType);
			}
			catch (Exception ex)
			{
				Toast.MakeText(activity, "Download failed: " + ex.Message, ToastLength.Long)?.Show();
			}
		});
	}

	static void Enqueue(Activity activity, string url, string? userAgent, string? contentDisposition, string? mimeType)
	{
		var fileName = GuessFileName(url, contentDisposition, mimeType);
		var request = new DownloadManager.Request(AUri.Parse(url)!);
		if (!string.IsNullOrEmpty(mimeType))
			request.SetMimeType(mimeType);
		if (!string.IsNullOrEmpty(userAgent))
			request.AddRequestHeader("User-Agent", userAgent);

		var cookies = CookieManager.Instance?.GetCookie(url);
		if (!string.IsNullOrEmpty(cookies))
			request.AddRequestHeader("Cookie", cookies);

		request.SetTitle(fileName);
		request.SetDescription("SRADC");
		request.AllowScanningByMediaScanner();
		request.SetNotificationVisibility(DownloadVisibility.VisibleNotifyCompleted);
		request.SetDestinationInExternalPublicDir(Environment.DirectoryDownloads, fileName);

		var dm = (DownloadManager?)activity.GetSystemService(Context.DownloadService);
		if (dm is null)
			throw new InvalidOperationException("DownloadManager unavailable");

		dm.Enqueue(request);
		Toast.MakeText(activity, "ডাউনলোড শুরু হয়েছে / Download started: " + fileName, ToastLength.Short)?.Show();
	}

	static string GuessFileName(string url, string? contentDisposition, string? mimeType)
	{
		try
		{
			var guessed = URLUtil.GuessFileName(url, contentDisposition, mimeType);
			if (!string.IsNullOrWhiteSpace(guessed))
				return Sanitize(guessed);
		}
		catch { }

		var ext = mimeType switch
		{
			"application/pdf" => ".pdf",
			"image/png" => ".png",
			"image/jpeg" => ".jpg",
			"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" => ".xlsx",
			"application/vnd.ms-excel" => ".xls",
			_ => ""
		};
		return Sanitize($"sradc_{DateTime.Now:yyyyMMdd_HHmmss}{ext}");
	}

	static string Sanitize(string name)
	{
		foreach (var c in Path.GetInvalidFileNameChars())
			name = name.Replace(c, '_');
		return string.IsNullOrWhiteSpace(name) ? $"sradc_{DateTime.Now:yyyyMMdd_HHmmss}" : name;
	}
}

/// <summary>Prints the current WebView via Android PrintManager (system print / Save as PDF).</summary>
public static class AppPrint
{
	public static void PrintCurrentWebView(string jobName = "SRADC")
	{
		try
		{
			var activity = Platform.CurrentActivity;
			if (activity is null)
				return;

			var web = FindWebView(activity.Window?.DecorView);
			if (web is null)
			{
				Toast.MakeText(activity, "Print: page not ready", ToastLength.Short)?.Show();
				return;
			}

			var printManager = (PrintManager?)activity.GetSystemService(Context.PrintService);
			if (printManager is null)
				return;

			var adapter = web.CreatePrintDocumentAdapter(jobName);
			printManager.Print(jobName, adapter, new PrintAttributes.Builder().Build());
		}
		catch (Exception ex)
		{
			try { Toast.MakeText(Android.App.Application.Context, "Print failed: " + ex.Message, ToastLength.Long)?.Show(); } catch { }
		}
	}

	static AWebView? FindWebView(Android.Views.View? view)
	{
		if (view is AWebView web)
			return web;
		if (view is Android.Views.ViewGroup group)
		{
			for (var i = 0; i < group.ChildCount; i++)
			{
				var found = FindWebView(group.GetChildAt(i));
				if (found is not null)
					return found;
			}
		}
		return null;
	}
}
