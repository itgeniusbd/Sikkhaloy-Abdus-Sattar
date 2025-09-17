using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.EXAM.WeeklyExam
{
    public partial class Edit_Weekly_Merit_Status_Marks : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Session["Group"] = GroupDropDownList.SelectedValue;
            Session["Shift"] = ShiftDropDownList.SelectedValue;
            Session["Section"] = SectionDropDownList.SelectedValue;

            try
            {
                if (!IsPostBack)
                {

                    GroupDropDownList.Visible = false;
                    GroupLabel.Visible = false;

                    GroupFormView.Visible = false;

                    SectionDropDownList.Visible = false;
                    SectionLabel.Visible = false;

                    SectionFormView.Visible = false;

                    ShiftDropDownList.Visible = false;
                    ShiftLabel.Visible = false;

                    ShiftFormView.Visible = false;
                    MultiView1.ActiveViewIndex = 0;
                }
            }

            catch { }
        }

        protected void view()
        {
            DataView GroupDV = new DataView();
            GroupDV = (DataView)GroupSQL.Select(DataSourceSelectArguments.Empty);
            if (GroupDV.Count < 1)
            {
                GroupDropDownList.Visible = false;
                GroupLabel.Visible = false;
                GRequiredFieldValidator.Visible = false;

                GroupFormView.Visible = false;

            }
            else
            {
                GroupDropDownList.Visible = true;
                GroupLabel.Visible = true;
                GRequiredFieldValidator.Visible = true;

                GroupFormView.Visible = true;
            }

            DataView SectionDV = new DataView();
            SectionDV = (DataView)SectionSQL.Select(DataSourceSelectArguments.Empty);
            if (SectionDV.Count < 1)
            {
                SectionDropDownList.Visible = false;
                SectionLabel.Visible = false;
                SeRequiredFieldValidator.Visible = false;

                SectionFormView.Visible = false;
            }
            else
            {
                SectionDropDownList.Visible = true;
                SectionLabel.Visible = true;
                SeRequiredFieldValidator.Visible = true;

                SectionFormView.Visible = true;
            }

            DataView ShiftDV = new DataView();
            ShiftDV = (DataView)ShiftSQL.Select(DataSourceSelectArguments.Empty);
            if (ShiftDV.Count < 1)
            {
                ShiftDropDownList.Visible = false;
                ShiftLabel.Visible = false;
                ShRequiredFieldValidator.Visible = false;

                ShiftFormView.Visible = false;

            }
            else
            {
                ShiftDropDownList.Visible = true;
                ShiftLabel.Visible = true;
                ShRequiredFieldValidator.Visible = true;

                ShiftFormView.Visible = true;

            }

            StudentsGridView.DataBind();

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
            GroupDropDownList.Items.Insert(0, new ListItem("- Select -", "%"));
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
            SectionDropDownList.Items.Insert(0, new ListItem("- Select -", "%"));
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
            ShiftDropDownList.Items.Insert(0, new ListItem("- Select -", "%"));
            if (IsPostBack)
                ShiftDropDownList.Items.FindByValue(Session["Shift"].ToString()).Selected = true;
        }


        protected void Calendar_SelectionChanged(object sender, EventArgs e)
        {
            MultiView1.ActiveViewIndex = 1;
        }

        protected void BackLinkButton_Click(object sender, EventArgs e)
        {
            MultiView1.ActiveViewIndex = 0;
        }

        //---------------------------------------End DD-------------------------------------------
    }
}