using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.Services;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.ADMISSION_REGISTER
{
    public partial class Edit_Student_information : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (string.IsNullOrEmpty(Request.QueryString["Student"]) | string.IsNullOrEmpty(Request.QueryString["Student_Class"]))
                    Response.Redirect("Find_Student.aspx");
            }
        }

        protected void IDFindButton_Click(object sender, EventArgs e)
        {
            DataView ShowIDDV = new DataView();
            ShowIDDV = (DataView)ShowIDSQL.Select(DataSourceSelectArguments.Empty);
            if (ShowIDDV.Count > 0)
            {
                Response.Redirect("Edit_Student_information.aspx?Student=" + ShowIDDV[0]["StudentID"].ToString() + "&Student_Class=" + ShowIDDV[0]["StudentClassID"].ToString());
                StudentInfoFormView.DataBind();
            }
            else
            {
                Response.Redirect("Edit_Student_information.aspx?Student=0&Student_Class=0");
                StudentInfoFormView.DataBind();
            }
        }

        // Gender, blood group and religion are selected here instead of SelectedValue='<%#Bind(..)%>', so a value that is
        // not in the list (NULL/'' saved by Hybrid admission, 'B(+)', '0+', '0' ...) can never crash the page.
        // The selected values go back to the UPDATE in StudentInfoFormView_ItemUpdating ('' becomes NULL as before).
        private static readonly string[] V2BloodGroups = { "Unknown", "A+", "A-", "B+", "B-", "AB+", "AB-", "O+", "O-" };

        protected void StudentInfoFormView_DataBound(object sender, EventArgs e)
        {
            DataRowView drv = StudentInfoFormView.DataItem as DataRowView;
            if (drv == null) return;

            // Blood group: exact or recognisable value (B(+), 0+, A -) -> that group, otherwise -> Unknown (V2's "none" value)
            DropDownList blood = StudentInfoFormView.FindControl("BloodGroupDropDownList") as DropDownList;
            if (blood != null)
            {
                blood.ClearSelection();
                ListItem item = FindListItem(blood.Items, NormalizeBloodGroup(Convert.ToString(drv["BloodGroup"])));
                if (item == null) item = blood.Items.FindByValue("Unknown");
                if (item != null) item.Selected = true;
            }

            // Religion: known value selected; empty -> a blank item (saves NULL); any other value is kept as an extra item
            DropDownList religion = StudentInfoFormView.FindControl("ReligionDropDownList") as DropDownList;
            if (religion != null)
            {
                string value = (Convert.ToString(drv["Religion"]) ?? "").Trim();
                religion.ClearSelection();
                ListItem item = FindListItem(religion.Items, value);
                if (item == null)
                {
                    item = new ListItem(value, value);
                    if (value.Length == 0) religion.Items.Insert(0, item); else religion.Items.Add(item);
                }
                item.Selected = true;
            }

            // Gender: Male/Female selected; anything else -> nothing selected
            RadioButtonList gender = StudentInfoFormView.FindControl("GenderRadioButtonList") as RadioButtonList;
            if (gender != null)
            {
                gender.ClearSelection();
                ListItem item = FindListItem(gender.Items, (Convert.ToString(drv["Gender"]) ?? "").Trim());
                if (item != null) item.Selected = true;
            }
        }

        protected void StudentInfoFormView_ItemUpdating(object sender, FormViewUpdateEventArgs e)
        {
            DropDownList blood = StudentInfoFormView.FindControl("BloodGroupDropDownList") as DropDownList;
            if (blood != null) e.NewValues["BloodGroup"] = blood.SelectedValue;

            DropDownList religion = StudentInfoFormView.FindControl("ReligionDropDownList") as DropDownList;
            if (religion != null) e.NewValues["Religion"] = religion.SelectedValue;

            RadioButtonList gender = StudentInfoFormView.FindControl("GenderRadioButtonList") as RadioButtonList;
            if (gender != null) e.NewValues["Gender"] = gender.SelectedValue;
        }

        private static ListItem FindListItem(ListItemCollection items, string value)
        {
            foreach (ListItem item in items)
                if (string.Equals(item.Value, value ?? "", StringComparison.OrdinalIgnoreCase))
                    return item;
            return null;
        }

        private static string NormalizeBloodGroup(string value)
        {
            string text = (value ?? "").Trim().ToUpperInvariant()
                .Replace("POSITIVE", "+").Replace("NEGATIVE", "-").Replace("+VE", "+").Replace("-VE", "-");
            var sb = new System.Text.StringBuilder();
            foreach (char ch in text)
            {
                if (ch == '0') sb.Append('O');
                else if (ch == 'A' || ch == 'B' || ch == 'O' || ch == '+' || ch == '-') sb.Append(ch);
            }
            string compact = sb.ToString();
            foreach (string group in V2BloodGroups)
                if (group == compact) return group;
            return "Unknown";
        }

        //Update Student Image
        [WebMethod(EnableSession = true)]
        public static int Update_Student_Image(string StudentImageID, string StudentID, string Image)
        {
            return Save_Student_Image(StudentID, Image, false);
        }

        //Update Guardian Image
        [WebMethod(EnableSession = true)]
        public static int Update_Guardian_Image(string StudentImageID, string StudentID, string Image)
        {
            return Save_Student_Image(StudentID, Image, true);
        }

        // Saves the student or guardian photo for a student of the logged-in school and returns the Student_Image ID.
        // If the student has no Student_Image row (StudentImageID NULL or the row is missing, e.g. students admitted
        // from Hybrid), a new row is created and linked to Student.StudentImageID in the same transaction.
        // Mirrors Hybrid StudentManagementService photo upload. StudentImageID from the page is no longer trusted.
        private static int Save_Student_Image(string StudentID, string Image, bool isGuardian)
        {
            int studentId;
            if (!int.TryParse(StudentID, out studentId) || studentId <= 0)
                throw new ArgumentException("Invalid student.");
            if (string.IsNullOrEmpty(Image))
                throw new ArgumentException("No image data.");

            var session = HttpContext.Current == null ? null : HttpContext.Current.Session;
            int schoolId;
            if (session == null || session["SchoolID"] == null || !int.TryParse(session["SchoolID"].ToString(), out schoolId))
                throw new InvalidOperationException("Session expired. Please log in again.");

            const string decodeImage = "CAST(N'' AS xml).value('xs:base64Binary(sql:variable(\"@Image\"))', 'varbinary(max)')";
            string constr = ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString;
            using (SqlConnection con = new SqlConnection(constr))
            {
                con.Open();
                using (SqlTransaction tx = con.BeginTransaction())
                {
                    bool found = false;
                    bool exists = false;
                    int imageId = 0;
                    using (SqlCommand chk = new SqlCommand(@"SELECT s.StudentImageID, CASE WHEN si.StudentImageID IS NULL THEN 0 ELSE 1 END AS HasRow
FROM Student s WITH (UPDLOCK, HOLDLOCK)
LEFT JOIN Student_Image si ON si.StudentImageID = s.StudentImageID
WHERE s.StudentID = @StudentID AND s.SchoolID = @SchoolID", con, tx))
                    {
                        chk.Parameters.AddWithValue("@StudentID", studentId);
                        chk.Parameters.AddWithValue("@SchoolID", schoolId);
                        using (SqlDataReader r = chk.ExecuteReader())
                        {
                            if (r.Read())
                            {
                                found = true;
                                exists = Convert.ToInt32(r["HasRow"]) == 1;
                                if (exists) imageId = Convert.ToInt32(r["StudentImageID"]);
                            }
                        }
                    }

                    if (!found)
                        throw new InvalidOperationException("Student not found.");

                    if (exists)
                    {
                        using (SqlCommand upd = new SqlCommand("UPDATE Student_Image SET " + (isGuardian ? "Guardian_Photo" : "Image") + " = " + decodeImage + " WHERE StudentImageID = @StudentImageID", con, tx))
                        {
                            upd.Parameters.AddWithValue("@StudentImageID", imageId);
                            upd.Parameters.AddWithValue("@Image", Image);
                            upd.ExecuteNonQuery();
                        }
                    }
                    else
                    {
                        string insSql = isGuardian
                            ? "INSERT INTO Student_Image (Image, Guardian_Photo) VALUES (NULL, " + decodeImage + "); SELECT CAST(SCOPE_IDENTITY() AS INT);"
                            : "INSERT INTO Student_Image (Image, Guardian_Photo) VALUES (" + decodeImage + ", NULL); SELECT CAST(SCOPE_IDENTITY() AS INT);";
                        using (SqlCommand ins = new SqlCommand(insSql, con, tx))
                        {
                            ins.Parameters.AddWithValue("@Image", Image);
                            imageId = Convert.ToInt32(ins.ExecuteScalar());
                        }

                        using (SqlCommand link = new SqlCommand("UPDATE Student SET StudentImageID = @ImageID WHERE StudentID = @StudentID AND SchoolID = @SchoolID", con, tx))
                        {
                            link.Parameters.AddWithValue("@ImageID", imageId);
                            link.Parameters.AddWithValue("@StudentID", studentId);
                            link.Parameters.AddWithValue("@SchoolID", schoolId);
                            link.ExecuteNonQuery();
                        }
                    }

                    tx.Commit();
                    return imageId;
                }
            }
        }
    }
}