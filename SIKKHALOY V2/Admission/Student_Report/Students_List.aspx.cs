using System;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.Admission.Student_Rerport
{
    public partial class Students_List : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Session["Group"] = GroupDropDownList.SelectedValue;
            Session["Shift"] = ShiftDropDownList.SelectedValue;
            Session["Section"] = SectionDropDownList.SelectedValue;

            if (!IsPostBack)
            {
                GroupDropDownList.Visible = false;
                SectionDropDownList.Visible = false;
                ShiftDropDownList.Visible = false;
            }
        }


        protected void view()
        {
            DataView GroupDV = new DataView();
            GroupDV = (DataView)GroupSQL.Select(DataSourceSelectArguments.Empty);
            if (GroupDV.Count < 1)
            {
                GroupDropDownList.Visible = false;
            }
            else
            {
                GroupDropDownList.Visible = true;
            }

            DataView SectionDV = new DataView();
            SectionDV = (DataView)SectionSQL.Select(DataSourceSelectArguments.Empty);
            if (SectionDV.Count < 1)
            {
                SectionDropDownList.Visible = false;
            }
            else
            {
                SectionDropDownList.Visible = true;
            }

            DataView ShiftDV = new DataView();
            ShiftDV = (DataView)ShiftSQL.Select(DataSourceSelectArguments.Empty);
            if (ShiftDV.Count < 1)
            {
                ShiftDropDownList.Visible = false;
            }
            else
            {
                ShiftDropDownList.Visible = true;
            }

            IDTextBox.Text = string.Empty;

            string name = string.Empty;

            if (ClassDropDownList.SelectedIndex != 0)
            {
                name += " Class: " + ClassDropDownList.SelectedItem.Text;
            }
            if (SectionDropDownList.SelectedIndex != 0)
            {
                name += ", Section: " + SectionDropDownList.SelectedItem.Text;
            }
            if (GroupDropDownList.SelectedIndex != 0)
            {
                name += ", Group: " + GroupDropDownList.SelectedItem.Text;
            }
            if (ShiftDropDownList.SelectedIndex != 0)
            {
                name += ", Shift: " + ShiftDropDownList.SelectedItem.Text;
            }

            CGSSLabel.Text = name;
        }

        //---------------------------------------Class DDL-------------------------------------------
        protected void ClassDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            Session["Group"] = "%";
            Session["Shift"] = "%";
            Session["Section"] = "%";

            GroupDropDownList.DataBind();
            ShiftDropDownList.DataBind();
            SectionDropDownList.DataBind();
            view();
        }


        //---------------------------------------Group DDL-------------------------------------------
        protected void GroupDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            view();
        }

        protected void GroupDropDownList_DataBound(object sender, EventArgs e)
        {
            GroupDropDownList.Items.Insert(0, new ListItem("[ ALL GROUP ]", "%"));
            if (IsPostBack)
                GroupDropDownList.Items.FindByValue(Session["Group"].ToString()).Selected = true;
        }


        //---------------------------------------Section DDL-------------------------------------------
        protected void SectionDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            view();
        }

        protected void SectionDropDownList_DataBound(object sender, EventArgs e)
        {
            SectionDropDownList.Items.Insert(0, new ListItem("[ ALL SECTION ]", "%"));
            if (IsPostBack)
                SectionDropDownList.Items.FindByValue(Session["Section"].ToString()).Selected = true;
        }

        //---------------------------------------Shift DDL-------------------------------------------
        protected void ShiftDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            view();
        }

        protected void ShiftDropDownList_DataBound(object sender, EventArgs e)
        {
            ShiftDropDownList.Items.Insert(0, new ListItem("[ ALL SHIFT ]", "%"));
            if (IsPostBack)
                ShiftDropDownList.Items.FindByValue(Session["Shift"].ToString()).Selected = true;
        }

        //---------------------------------------End DD-------------------------------------------
        protected void IDFindButton_Click(object sender, EventArgs e)
        {
            DataView ShowIDDV = new DataView();
            ShowIDDV = (DataView)ShowIDSQL.Select(DataSourceSelectArguments.Empty);
            if (ShowIDDV.Count < 1)
            {
                ClassDropDownList.SelectedIndex = 0;
                GroupDropDownList.Visible = false;
                SectionDropDownList.Visible = false;
                ShiftDropDownList.Visible = false;
            }
            else
            {
                Response.Redirect("Report.aspx?Student=" + ShowIDDV[0]["StudentID"].ToString() + "&Student_Class=" + ShowIDDV[0]["StudentClassID"].ToString());
            }
        }

        public override void VerifyRenderingInServerForm(Control control)
        {
            //'Export to word' required to avoid the run time error Control 'GridView1' of type 'Grid View' must be placed inside a form tag with runat=server."  
        }

    }

}