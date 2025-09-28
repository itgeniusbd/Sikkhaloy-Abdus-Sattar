using System;
using System.Web;

namespace EDUCATION.COM.Handeler
{
    /// <summary>
    /// Test Handler for basic functionality
    /// </summary>
    public class TestHandler : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/plain";
            context.Response.Write("Test Handler is working!");
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}