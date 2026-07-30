using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.Script.Serialization;

namespace EDUCATION.COM.Handeler
{
    /// <summary>
    /// Device photo download JSON (fallback when Attendance_API /photos is unavailable).
    /// GET: schoolId, key (Attendance_Device_Setting.SettingKey)
    /// </summary>
    public class Device_UserPhotos : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "application/json";
            context.Response.TrySkipIisCustomErrors = true;

            if (!int.TryParse(context.Request.QueryString["schoolId"], out var schoolId) || schoolId <= 0)
            {
                context.Response.StatusCode = 400;
                context.Response.Write("{\"error\":\"schoolId required\"}");
                return;
            }

            var settingKey = (context.Request.QueryString["key"] ?? "").Trim();
            if (string.IsNullOrEmpty(settingKey))
            {
                context.Response.StatusCode = 400;
                context.Response.Write("{\"error\":\"key required\"}");
                return;
            }

            var constr = ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString;
            using (var con = new SqlConnection(constr))
            {
                con.Open();

                using (var authCmd = new SqlCommand(
                    "SELECT TOP 1 SchoolID FROM Attendance_Device_Setting WHERE SchoolID = @SchoolID AND SettingKey = @SettingKey AND IsActive = 1",
                    con))
                {
                    authCmd.Parameters.AddWithValue("@SchoolID", schoolId);
                    authCmd.Parameters.AddWithValue("@SettingKey", settingKey);
                    if (authCmd.ExecuteScalar() == null)
                    {
                        context.Response.StatusCode = 403;
                        context.Response.Write("{\"error\":\"invalid key\"}");
                        return;
                    }
                }

                var photos = new List<DeviceUserPhotoDto>();
                using (var cmd = new SqlCommand(
                    "SELECT ID, Image FROM VW_Attendance_Users_Image WHERE SchoolID = @SchoolID AND Image IS NOT NULL AND DATALENGTH(Image) > 0",
                    con))
                {
                    cmd.Parameters.AddWithValue("@SchoolID", schoolId);
                    using (var dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            if (dr["Image"] == DBNull.Value)
                                continue;

                            var bytes = (byte[])dr["Image"];
                            if (bytes == null || bytes.Length == 0)
                                continue;

                            photos.Add(new DeviceUserPhotoDto
                            {
                                id = dr["ID"].ToString(),
                                image = Convert.ToBase64String(bytes)
                            });
                        }
                    }
                }

                var js = new JavaScriptSerializer { MaxJsonLength = int.MaxValue };
                context.Response.Write(js.Serialize(photos));
            }
        }

        public bool IsReusable => false;

        private class DeviceUserPhotoDto
        {
            public string id { get; set; }
            public string image { get; set; }
        }
    }
}
