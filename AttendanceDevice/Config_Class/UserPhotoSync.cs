using AttendanceDevice.Model;
using AttendanceDevice.ViewModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace AttendanceDevice.Config_Class
{
    internal static class UserPhotoSync
    {
        internal class UserPhotoApiDto
        {
            [JsonProperty("id")]
            public string ID { get; set; }

            [JsonProperty("image")]
            public byte[] Image { get; set; }
        }

        public struct DownloadResult
        {
            public int Saved;
            public int Skipped;
            public int Failed;
            public string Error;
            public string Summary =>
                Failed > 0
                    ? $"{Saved} downloaded, {Skipped} already in folder, {Failed} failed (file in use?)."
                    : Saved > 0
                        ? $"{Saved} photo(s) downloaded, {Skipped} already in folder."
                        : Skipped > 0
                            ? $"All {Skipped} photo(s) already exist in folder."
                            : "No photos saved.";
        }

        public static async Task<DownloadResult> DownloadToFolderAsync(RestClient client, Institution ins)
        {
            if (ins == null)
                return Fail("Institution info missing.");

            if (string.IsNullOrWhiteSpace(ins.Image_Link) || !Directory.Exists(ins.Image_Link))
                return Fail("Set a valid photo folder in Institution Info first.");

            var photos = await FetchPhotosAsync(client, ins);
            if (photos.error != null)
                return Fail(photos.error);

            if (!photos.items.Any())
                return Fail("No photos found on server for this school.");

            var folder = ins.Image_Link.Trim();
            var result = new DownloadResult();

            foreach (var photo in photos.items.Where(p => !string.IsNullOrWhiteSpace(p.ID) && p.Image != null && p.Image.Length > 0))
            {
                if (UserPhotoHelper.PhotoExists(folder, photo.ID))
                {
                    result.Skipped++;
                    continue;
                }

                var path = UserPhotoHelper.GetPhotoPath(folder, photo.ID);
                if (TryWritePhoto(path, photo.Image))
                    result.Saved++;
                else
                    result.Failed++;
            }

            return result;
        }

        private static DownloadResult Fail(string message)
        {
            return new DownloadResult { Error = message };
        }

        private static bool TryWritePhoto(string path, byte[] bytes)
        {
            var tempPath = path + ".download";
            try
            {
                File.WriteAllBytes(tempPath, bytes);
                if (File.Exists(path))
                {
                    try
                    {
                        File.Delete(path);
                    }
                    catch (IOException)
                    {
                        File.Delete(tempPath);
                        return false;
                    }
                }

                File.Move(tempPath, path);
                return true;
            }
            catch (IOException)
            {
                return false;
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
            finally
            {
                try
                {
                    if (File.Exists(tempPath))
                        File.Delete(tempPath);
                }
                catch
                {
                    // ignored
                }
            }
        }

        private static async Task<(List<UserPhotoApiDto> items, string error)> FetchPhotosAsync(RestClient client, Institution ins)
        {
            var apiRequest = new RestRequest("api/Users/{id}/photos", Method.GET);
            ApiRequestHelper.AddAuthorizedJsonHeaders(apiRequest, ins.Token);
            apiRequest.AddUrlSegment("id", ins.SchoolID);

            var apiResponse = await client.ExecuteTaskAsync(apiRequest);
            if (apiResponse.StatusCode == HttpStatusCode.OK)
                return (DeserializePhotos(apiResponse.Content), null);

            if (apiResponse.StatusCode != HttpStatusCode.NotFound)
            {
                return (null, FormatHttpError("Photo download failed", apiResponse.StatusCode, apiResponse.Content));
            }

            if (string.IsNullOrWhiteSpace(ins.SettingKey))
            {
                return (null, "Photo API not found on server. Deploy Attendance_API update, or set Institution Setting Key.");
            }

            var webClient = new RestClient(ApiUrl.WebUrl.TrimEnd('/'));
            var webRequest = new RestRequest("Handeler/Device_UserPhotos.ashx", Method.GET);
            webRequest.AddQueryParameter("schoolId", ins.SchoolID.ToString());
            webRequest.AddQueryParameter("key", ins.SettingKey.Trim());

            var webResponse = await webClient.ExecuteTaskAsync(webRequest);
            if (webResponse.StatusCode != HttpStatusCode.OK)
            {
                return (null, FormatHttpError(
                    "Photo download failed (API 404; web fallback also failed)",
                    webResponse.StatusCode,
                    webResponse.Content));
            }

            return (DeserializePhotos(webResponse.Content), null);
        }

        private static List<UserPhotoApiDto> DeserializePhotos(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return new List<UserPhotoApiDto>();

            var array = JArray.Parse(content);
            var list = new List<UserPhotoApiDto>();
            foreach (var item in array)
            {
                var id = (item["id"] ?? item["ID"])?.ToString();
                var imageToken = item["image"] ?? item["Image"];
                byte[] image = null;
                if (imageToken != null)
                {
                    if (imageToken.Type == JTokenType.String)
                        image = Convert.FromBase64String(imageToken.ToString());
                    else
                        image = imageToken.ToObject<byte[]>();
                }

                if (!string.IsNullOrWhiteSpace(id) && image != null && image.Length > 0)
                    list.Add(new UserPhotoApiDto { ID = id, Image = image });
            }

            return list;
        }

        private static string FormatHttpError(string prefix, HttpStatusCode status, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return $"{prefix} ({(int)status}).";

            if (content.TrimStart().StartsWith("<", StringComparison.Ordinal))
            {
                if (status == HttpStatusCode.NotFound)
                    return $"{prefix} ({(int)status}): endpoint not found on server. Deploy Attendance_API or SIKKHALOY web update.";
                return $"{prefix} ({(int)status}): server returned HTML error page.";
            }

            if (content.Length > 200)
                content = content.Substring(0, 200) + "...";
            return $"{prefix} ({(int)status}): {content}";
        }
    }
}
