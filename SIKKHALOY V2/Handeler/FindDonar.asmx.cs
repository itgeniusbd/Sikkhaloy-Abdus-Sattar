using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.Script.Services;
using System.Web.Services;

namespace EDUCATION.COM.Handeler
{

    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [ToolboxItem(false)]
    [ScriptService]
    public class FindDonar : WebService
    {

        [WebMethod(EnableSession = true)]
        public string FindDonarAutocomplete(string prefix)
        {
            List<CommitteeMemberViewModel> data = new List<CommitteeMemberViewModel>();
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandText = @"SELECT top(3) CommitteeMemberId, MemberName, SmsNumber FROM CommitteeMember WHERE(SchoolID = @SchoolID) AND (MemberName LIKE @SearchKey + N'%' OR SmsNumber LIKE @SearchKey + N'%')";
                    cmd.Parameters.AddWithValue("@SearchKey", prefix);
                    cmd.Parameters.AddWithValue("@SchoolID", HttpContext.Current.Session["SchoolID"].ToString());
                    cmd.Connection = con;

                    con.Open();
                    SqlDataReader dr = cmd.ExecuteReader();
                    while (dr.Read())
                    {
                        data.Add(new CommitteeMemberViewModel { 
                            CommitteeMemberId = dr["CommitteeMemberId"].ToString(),
                            MemberName = dr["MemberName"].ToString(),
                            SmsNumber= dr["SmsNumber"].ToString(),
                        });
                    }
                    con.Close();

                    var json = new JavaScriptSerializer().Serialize(data);
                    return json;
                }
            }
        }
    }

    public class CommitteeMemberViewModel
    {
        public string CommitteeMemberId { get; set; }
        public string MemberName { get; set; }
        public string SmsNumber { get; set; }

    }
}
