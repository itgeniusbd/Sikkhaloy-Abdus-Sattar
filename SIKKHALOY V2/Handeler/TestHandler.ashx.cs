using System;
using System.Web;

namespace EDUCATION.COM.Handeler
{
    public class TestHandler : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/plain";
            context.Response.Write("Test Handler Working");
        }

        public bool IsReusable => false;
    }
}           