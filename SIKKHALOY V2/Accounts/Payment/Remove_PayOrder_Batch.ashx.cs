using System;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.Security;

namespace EDUCATION.COM.ACCOUNTS.Payment
{
    public class Remove_PayOrder_Batch : IHttpHandler, System.Web.SessionState.IRequiresSessionState
    {
        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "application/json";
            context.Response.Cache.SetCacheability(HttpCacheability.NoCache);

            if (context.Session == null || context.Session["SchoolID"] == null)
            {
                WriteJson(context, new { ok = false, message = "Session expired. Please login again." });
                return;
            }

            if (!IsAuthorized())
            {
                context.Response.StatusCode = 403;
                WriteJson(context, new { ok = false, message = "You are not authorized to remove pay orders." });
                return;
            }

            string ids = context.Request["ids"] ?? string.Empty;
            if (string.IsNullOrWhiteSpace(ids))
            {
                WriteJson(context, new { ok = false, message = "No pay order selected." });
                return;
            }

            int schoolId;
            if (!int.TryParse(context.Session["SchoolID"].ToString(), out schoolId) || schoolId <= 0)
            {
                WriteJson(context, new { ok = false, message = "Invalid school session." });
                return;
            }

            try
            {
                int deleted = PayOrder_DeleteHelper.DeleteBatch(schoolId, ids);
                string[] requested = PayOrder_DeleteHelper.ParseIds(ids);
                WriteJson(context, new
                {
                    ok = true,
                    deleted = deleted,
                    requested = requested.Length
                });
            }
            catch (Exception ex)
            {
                WriteJson(context, new { ok = false, message = ex.Message });
            }
        }

        private static bool IsAuthorized()
        {
            return Roles.IsUserInRole("Admin")
                || Roles.IsUserInRole("Authority")
                || Roles.IsUserInRole("Sub-Authority")
                || Roles.IsUserInRole("AC_P_Remove_Payorder");
        }

        private static void WriteJson(HttpContext context, object data)
        {
            context.Response.Write(new JavaScriptSerializer().Serialize(data));
        }

        public bool IsReusable
        {
            get { return false; }
        }
    }
}
