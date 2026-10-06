using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Provider;
using Android.Runtime;
using Android.Webkit;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using AndroidFileProvider = AndroidX.Core.Content.FileProvider;
using AUri = Android.Net.Uri;
using AWebView = Android.Webkit.WebView;
using JFile = Java.IO.File;

namespace SradcApp;

/// <summary>
/// WebChromeClient for the college website and the Sikkhaloy portal:
///  - &lt;input type="file"&gt; support: camera capture (FileProvider URI in the app cache) + gallery/files,
///    honouring the accept types and the capture attribute. The callback is ALWAYS answered
///    (null on cancel/error) so the input never gets stuck.
///  - getUserMedia camera requests are granted only for our own https hosts.
/// </summary>
public class AppWebChromeClient : MauiWebChromeClient
{
	public const int FileChooserRequestCode = 7301;
	const string ProviderSuffix = ".webviewfileprovider";
	const string CaptureFolder = "camera"; // must match Resources/xml/app_file_paths.xml

	static readonly HashSet<string> TrustedHosts = new(StringComparer.OrdinalIgnoreCase)
	{
		"sradc.edu.bd", "www.sradc.edu.bd",
		"sikkhaloy.com", "www.sikkhaloy.com", "hybrid.sikkhaloy.com", "hybridapi.sikkhaloy.com"
	};

	static IValueCallback? _pendingCallback;
	static AUri? _pendingCaptureUri;
	static JFile? _pendingCaptureFile;

	public AppWebChromeClient(WebViewHandler handler) : base(handler)
	{
	}

	// ---------------------------------------------------------------- load progress

	public override void OnProgressChanged(AWebView? view, int newProgress)
	{
		try { base.OnProgressChanged(view, newProgress); } catch { }
		WebLoadEvents.RaiseProgress(newProgress);
	}

	// ---------------------------------------------------------------- file upload

	public override bool OnShowFileChooser(AWebView? webView, IValueCallback? filePathCallback, FileChooserParams? fileChooserParams)
	{
		if (filePathCallback is null)
			return false;

		Deliver(null); // finish an older request first, so that input is not left waiting
		_pendingCallback = filePathCallback;
		_ = ShowChooserAsync(fileChooserParams);
		return true;
	}

	static async Task ShowChooserAsync(FileChooserParams? p)
	{
		try
		{
			var activity = Platform.CurrentActivity;
			if (activity is null)
			{
				Deliver(null);
				return;
			}

			var mimeTypes = GetMimeTypes(p);
			bool multiple = p?.Mode == ChromeFileChooserMode.OpenMultiple;
			bool captureAttr = p?.IsCaptureEnabled ?? false;
			bool wantsImages = mimeTypes.Length == 0 || mimeTypes.Any(m =>
				m == "*/*" || m.StartsWith("image/", StringComparison.OrdinalIgnoreCase));

			Intent? cameraIntent = null;
			if (wantsImages
				&& activity.PackageManager?.HasSystemFeature(PackageManager.FeatureCameraAny) == true
				&& await EnsureCameraPermissionAsync())
			{
				cameraIntent = CreateCameraIntent(activity); // null if no camera app; denied -> gallery only
			}

			Intent toLaunch;
			if (captureAttr && cameraIntent is not null)
			{
				toLaunch = cameraIntent; // <input capture> -> open the camera directly
			}
			else
			{
				var content = new Intent(Intent.ActionGetContent);
				content.AddCategory(Intent.CategoryOpenable);
				if (mimeTypes.Length == 1)
				{
					content.SetType(mimeTypes[0]);
				}
				else
				{
					content.SetType("*/*");
					if (mimeTypes.Length > 1)
						content.PutExtra(Intent.ExtraMimeTypes, mimeTypes);
				}
				if (multiple)
					content.PutExtra(Intent.ExtraAllowMultiple, true);

				toLaunch = Intent.CreateChooser(content, "ছবি বা ফাইল বাছাই করুন / Choose photo or file")!;
				if (cameraIntent is not null)
					toLaunch.PutExtra(Intent.ExtraInitialIntents, new IParcelable[] { cameraIntent });
			}

			activity.StartActivityForResult(toLaunch, FileChooserRequestCode);
		}
		catch (Exception)
		{
			Deliver(null);
		}
	}

	/// <summary>Called from MainActivity.OnActivityResult.</summary>
	public static bool HandleActivityResult(int requestCode, Result resultCode, Intent? data)
	{
		if (requestCode != FileChooserRequestCode)
			return false;

		AUri[]? results = null;
		try
		{
			if (resultCode == Result.Ok)
			{
				var list = new List<AUri>();
				if (data?.ClipData is ClipData clip && clip.ItemCount > 0)
				{
					for (int i = 0; i < clip.ItemCount; i++)
					{
						var u = clip.GetItemAt(i)?.Uri;
						if (u is not null)
							list.Add(u);
					}
				}
				else if (data?.Data is AUri single)
				{
					list.Add(single);
				}

				// Camera apps usually return no data: the photo was written to our capture file.
				if (list.Count == 0 && _pendingCaptureUri is not null && _pendingCaptureFile is { } f && f.Exists() && f.Length() > 0)
					list.Add(_pendingCaptureUri);

				if (list.Count > 0)
					results = list.ToArray();
			}
		}
		catch (Exception)
		{
			results = null;
		}

		Deliver(results);
		return true;
	}

	static void Deliver(AUri[]? uris)
	{
		var callback = _pendingCallback;
		_pendingCallback = null;

		try
		{
			if (uris is null && _pendingCaptureFile is { } f && f.Exists() && f.Length() == 0)
				f.Delete();
		}
		catch { }
		_pendingCaptureFile = null;
		_pendingCaptureUri = null;

		if (callback is null)
			return;
		try
		{
			if (uris is null)
				callback.OnReceiveValue(null);
			else
				callback.OnReceiveValue(new Java.Lang.Object(JNIEnv.NewArray(uris), JniHandleOwnership.TransferLocalRef));
		}
		catch { }
	}

	static string[] GetMimeTypes(FileChooserParams? p)
	{
		var list = new List<string>();
		foreach (var raw in p?.GetAcceptTypes() ?? Array.Empty<string>())
		{
			foreach (var part in (raw ?? string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
			{
				string? mime = part.StartsWith('.')
					? MimeTypeMap.Singleton?.GetMimeTypeFromExtension(part.TrimStart('.').ToLowerInvariant())
					: part.Contains('/') ? part.ToLowerInvariant() : null;
				if (!string.IsNullOrEmpty(mime) && !list.Contains(mime))
					list.Add(mime);
			}
		}
		return list.ToArray();
	}

	static Intent? CreateCameraIntent(Activity activity)
	{
		try
		{
			var intent = new Intent(MediaStore.ActionImageCapture);
			if (activity.PackageManager is null || intent.ResolveActivity(activity.PackageManager) is null)
				return null;

			var dir = new JFile(activity.CacheDir, CaptureFolder);
			if (!dir.Exists())
				dir.Mkdirs();
			CleanupOldCaptures(dir);

			var file = new JFile(dir, $"IMG_{DateTime.Now:yyyyMMdd_HHmmss}.jpg");
			var uri = AndroidFileProvider.GetUriForFile(activity, activity.PackageName + ProviderSuffix, file);
			if (uri is null)
				return null;

			intent.PutExtra(MediaStore.ExtraOutput, uri);
			intent.AddFlags(ActivityFlags.GrantWriteUriPermission | ActivityFlags.GrantReadUriPermission);
			intent.ClipData = ClipData.NewRawUri(string.Empty, uri);

			_pendingCaptureFile = file;
			_pendingCaptureUri = uri;
			return intent;
		}
		catch (Exception)
		{
			return null;
		}
	}

	static void CleanupOldCaptures(JFile dir)
	{
		try
		{
			var cutoff = Java.Lang.JavaSystem.CurrentTimeMillis() - (long)TimeSpan.FromDays(1).TotalMilliseconds;
			foreach (var f in dir.ListFiles() ?? Array.Empty<JFile>())
			{
				if (f.LastModified() < cutoff)
					f.Delete();
			}
		}
		catch { }
	}

	static async Task<bool> EnsureCameraPermissionAsync()
	{
		try
		{
			var status = await Permissions.CheckStatusAsync<Permissions.Camera>();
			if (status != PermissionStatus.Granted)
				status = await MainThread.InvokeOnMainThreadAsync(() => Permissions.RequestAsync<Permissions.Camera>());
			return status == PermissionStatus.Granted;
		}
		catch (Exception)
		{
			return false;
		}
	}

	// ---------------------------------------------------------------- live camera (getUserMedia)

	public override void OnPermissionRequest(PermissionRequest? request)
	{
		if (request is null)
			return;

		try
		{
			var origin = request.Origin;
			bool trusted = origin is not null
				&& string.Equals(origin.Scheme, "https", StringComparison.OrdinalIgnoreCase)
				&& origin.Host is string host && TrustedHosts.Contains(host);
			bool wantsCamera = (request.GetResources() ?? Array.Empty<string>())
				.Contains(PermissionRequest.ResourceVideoCapture);

			if (!trusted || !wantsCamera)
			{
				request.Deny();
				return;
			}

			_ = GrantCameraAsync(request);
		}
		catch (Exception)
		{
			try { request.Deny(); } catch { }
		}
	}

	static async Task GrantCameraAsync(PermissionRequest request)
	{
		bool granted = await EnsureCameraPermissionAsync();
		MainThread.BeginInvokeOnMainThread(() =>
		{
			try
			{
				if (granted)
					request.Grant(new[] { PermissionRequest.ResourceVideoCapture }); // camera only, never microphone
				else
					request.Deny();
			}
			catch { }
		});
	}
}
