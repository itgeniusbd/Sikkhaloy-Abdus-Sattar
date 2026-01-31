using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.Committee
{
    public partial class Donations : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void ExportExcelButton_Click(object sender, EventArgs e)
        {
            try
            {
                SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ToString());
                SqlCommand cmd = new SqlCommand(@"
                    SELECT 
                        cm.MemberName AS 'Member Name',
                        cmt.CommitteeMemberType AS 'Member Type',
                        cm.SmsNumber AS 'Phone',
                        cdc.DonationCategory AS 'Category',
                        cd.Description,
                        cd.Amount,
                        cd.PaidAmount AS 'Paid Amount',
                        cd.Due,
                        cd.PromiseDate AS 'Promise Date',
                        cd.InsertDate AS 'Created Date',
                        CASE WHEN cd.IsPaid = 1 THEN 'Paid' ELSE 'Due' END AS 'Status'
                    FROM CommitteeDonation cd
                    INNER JOIN CommitteeMember cm ON cd.CommitteeMemberId = cm.CommitteeMemberId
                    INNER JOIN CommitteeMemberType cmt ON cm.CommitteeMemberTypeId = cmt.CommitteeMemberTypeId
                    INNER JOIN CommitteeDonationCategory cdc ON cd.CommitteeDonationCategoryId = cdc.CommitteeDonationCategoryId
                    WHERE cd.SchoolID = @SchoolID
                    AND cd.CommitteeMemberId LIKE @CommitteeMemberId
                    AND cd.CommitteeDonationCategoryId LIKE @CommitteeDonationCategoryId
                    AND cd.IsPaid LIKE @IsPaid
                    ORDER BY cd.InsertDate DESC", con);

                cmd.Parameters.AddWithValue("@SchoolID", Session["SchoolID"].ToString());
                cmd.Parameters.AddWithValue("@CommitteeMemberId", string.IsNullOrEmpty(CommitteeMemberDropDownList.SelectedValue) ? "%" : CommitteeMemberDropDownList.SelectedValue);
                cmd.Parameters.AddWithValue("@CommitteeDonationCategoryId", string.IsNullOrEmpty(DonationCategoryDownList.SelectedValue) ? "%" : DonationCategoryDownList.SelectedValue);
                cmd.Parameters.AddWithValue("@IsPaid", PDRadioButtonList.SelectedValue);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                if (dt.Rows.Count > 0)
                {
                    Response.Clear();
                    Response.Buffer = true;
                    Response.AddHeader("content-disposition", $"attachment;filename=Donations_{DateTime.Now:yyyyMMdd_HHmmss}.xls");
                    Response.Charset = "";
                    Response.ContentType = "application/vnd.ms-excel";

                    StringWriter sw = new StringWriter();
                    System.Web.UI.HtmlTextWriter hw = new System.Web.UI.HtmlTextWriter(sw);

                    // Create a simple HTML table
                    hw.Write("<table border='1'>");
                    hw.Write("<tr>");
                    foreach (DataColumn column in dt.Columns)
                    {
                        hw.Write($"<th>{column.ColumnName}</th>");
                    }
                    hw.Write("</tr>");

                    foreach (DataRow row in dt.Rows)
                    {
                        hw.Write("<tr>");
                        foreach (DataColumn column in dt.Columns)
                        {
                            hw.Write($"<td>{row[column.ColumnName]}</td>");
                        }
                        hw.Write("</tr>");
                    }
                    hw.Write("</table>");

                    Response.Output.Write(sw.ToString());
                    Response.Flush();
                    Response.End();
                }
                else
                {
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "nodata",
                        "alert('No data to export!');", true);
                }
            }
            catch (Exception ex)
            {
                ScriptManager.RegisterStartupScript(this, this.GetType(), "error",
                    $"alert('Error exporting data: {ex.Message}');", true);
            }
        }
    }
}