using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.ADMINISTRATION_BASICSETTING
{
    public partial class Specify_group_section_Shift_for_classes : System.Web.UI.Page
    {
        SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ToString());
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                GroupGridView.DataBind();
                SectionGridView.DataBind();
                ShiftGridView.DataBind();
            }
        }

        protected void JoinButton_Click(object sender, EventArgs e)
        {
            MassageLabel.Text = string.Empty;
            ExistLabel.Text = string.Empty;
            MassageLabel.Text = string.Empty;

            DataView GroupDV = new DataView();
            GroupDV = (DataView)GroupSQL.Select(DataSourceSelectArguments.Empty);

            DataView SectionDV = new DataView();
            SectionDV = (DataView)SectionSQL.Select(DataSourceSelectArguments.Empty);

            DataView ShiftDV = new DataView();
            ShiftDV = (DataView)ShiftSQL.Select(DataSourceSelectArguments.Empty);


            if (GroupDV.Count < 1 && SectionDV.Count < 1 || GroupDV.Count < 1 && ShiftDV.Count < 1 || SectionDV.Count < 1 && ShiftDV.Count < 1 || GroupDV.Count < 1 && SectionDV.Count < 1 && ShiftDV.Count < 1)
            {
                MassageLabel.Text = "No Need to Join";
            }

            else
            {

                if (GroupDV.Count < 1)
                {
                    bool IS_Section = true;
                    bool IS_Shift = true;

                    foreach (GridViewRow SecRow in SectionGridView.Rows)
                    {
                        CheckBox SectionCheckBox = (CheckBox)SecRow.FindControl("SectionCheckBox");

                        if (SectionCheckBox.Checked)
                        {
                            IS_Section = false;
                            string SectionID = SectionGridView.DataKeys[SecRow.RowIndex].Value.ToString();

                            foreach (GridViewRow ShiftRow in ShiftGridView.Rows)
                            {
                                CheckBox ShiftCheckBox = (CheckBox)ShiftRow.FindControl("ShiftCheckBox");

                                if (ShiftCheckBox.Checked)
                                {
                                    IS_Shift = false;
                                    string ShiftID = ShiftGridView.DataKeys[ShiftRow.RowIndex].Value.ToString();

                                    SqlCommand JoinShiftSection = new SqlCommand("Select JoinID from [Join] where ClassID = @ClassID and ShiftID = @ShiftID and SectionID = @SectionID", con);
                                    JoinShiftSection.Parameters.AddWithValue("@ClassID",ClassDropDownList.SelectedValue);
                                    JoinShiftSection.Parameters.AddWithValue("@ShiftID",ShiftID);
                                    JoinShiftSection.Parameters.AddWithValue("@SectionID", SectionID);

                                    con.Open();
                                     Object check = JoinShiftSection.ExecuteScalar();
                                    con.Close();

                                    if (check == null)
                                    {
                                        JoinSQL.InsertParameters["SubjectGroupID"].DefaultValue = "0";
                                        JoinSQL.InsertParameters["SectionID"].DefaultValue = SectionID;
                                        JoinSQL.InsertParameters["ShiftID"].DefaultValue = ShiftID;

                                        JoinSQL.Insert();
                                        MassageLabel.Text += "Shift: " + ShiftRow.Cells[0].Text + " and Section: " + SecRow.Cells[0].Text + " Joined successfully! <br />";
                                        MassageLabel.ForeColor = System.Drawing.Color.Green;
                                    }
                                    else { ExistLabel.Text += "Sorry!! Shift: " + ShiftRow.Cells[0].Text + " and Section: " + SecRow.Cells[0].Text + " Already Joined <br />"; }
                                }
                            }
                        }
                    }

                    if (IS_Shift)
                    {
                        MassageLabel.Text = "Select shift ";
                    }
                    if (IS_Section)
                    {
                        MassageLabel.Text = "Select section ";
                    }


                }
                else if (SectionDV.Count < 1)
                {
                    bool IS_Group = true;
                    bool IS_Shift = true;

                    foreach (GridViewRow GroupRow in GroupGridView.Rows)
                    {
                        CheckBox GroupCheckBox = (CheckBox)GroupRow.FindControl("GroupCheckBox");

                        if (GroupCheckBox.Checked)
                        {
                            IS_Group = false;
                            string SubjectGroupID = GroupGridView.DataKeys[GroupRow.RowIndex].Value.ToString();

                            foreach (GridViewRow ShiftRow in ShiftGridView.Rows)
                            {

                                CheckBox ShiftCheckBox = (CheckBox)ShiftRow.FindControl("ShiftCheckBox");

                                if (ShiftCheckBox.Checked)
                                {
                                    IS_Shift = false;
                                    string ShiftID = ShiftGridView.DataKeys[ShiftRow.RowIndex].Value.ToString();

                                    SqlCommand JoinShiftGroup = new SqlCommand("Select JoinID from [Join] where ClassID = @ClassID and SubjectGroupID = @SubjectGroupID and ShiftID = @ShiftID", con);
                                    JoinShiftGroup.Parameters.AddWithValue("@ClassID", ClassDropDownList.SelectedValue);
                                    JoinShiftGroup.Parameters.AddWithValue("@SubjectGroupID", SubjectGroupID);
                                    JoinShiftGroup.Parameters.AddWithValue("@ShiftID", ShiftID);

                                    con.Open();
                                    Object check = JoinShiftGroup.ExecuteScalar();
                                    con.Close();

                                    if (check == null)
                                    {
                                        JoinSQL.InsertParameters["SubjectGroupID"].DefaultValue = SubjectGroupID;
                                        JoinSQL.InsertParameters["SectionID"].DefaultValue = "0";
                                        JoinSQL.InsertParameters["ShiftID"].DefaultValue = ShiftID;

                                        JoinSQL.Insert();

                                        MassageLabel.Text += "Shift: " + ShiftRow.Cells[0].Text + " and Group: " + GroupRow.Cells[0].Text + " Joined successfully!<br />";
                                        MassageLabel.ForeColor = System.Drawing.Color.Green;
                                    }
                                    else { ExistLabel.Text += "Sorry!! Shift: " + ShiftRow.Cells[0].Text + " and Group: " + GroupRow.Cells[0].Text + " Already Joined <br />"; }
                                }
                            }
                        }
                    }
                    if (IS_Shift)
                    {
                        MassageLabel.Text = "Select shift ";
                    }
                    if (IS_Group)
                    {
                        MassageLabel.Text = "Select Group ";
                    }

                }
                else if (ShiftDV.Count < 1)
                {
                    bool IS_Section = true;
                    bool IS_Group = true;

                    foreach (GridViewRow GroupRow in GroupGridView.Rows)
                    {
                        CheckBox GroupCheckBox = (CheckBox)GroupRow.FindControl("GroupCheckBox");

                        if (GroupCheckBox.Checked)
                        {
                            IS_Group = false;
                            string SubjectGroupID = GroupGridView.DataKeys[GroupRow.RowIndex].Value.ToString();

                            foreach (GridViewRow SecRow in SectionGridView.Rows)
                            {
                                CheckBox SectionCheckBox = (CheckBox)SecRow.FindControl("SectionCheckBox");

                                if (SectionCheckBox.Checked)
                                {
                                    IS_Section = false;
                                    string SectionID = SectionGridView.DataKeys[SecRow.RowIndex].Value.ToString();

                                    SqlCommand JoinGroupSection = new SqlCommand("Select JoinID from [Join] where ClassID = @ClassID and SubjectGroupID = @SubjectGroupID and SectionID = @SectionID", con);
                                    JoinGroupSection.Parameters.AddWithValue("@ClassID", ClassDropDownList.SelectedValue);
                                    JoinGroupSection.Parameters.AddWithValue("@SubjectGroupID", SubjectGroupID);
                                    JoinGroupSection.Parameters.AddWithValue("@SectionID", SectionID);

                                    con.Open();
                                    Object check = JoinGroupSection.ExecuteScalar();
                                    con.Close();

                                    if (check == null)
                                    {
                                        JoinSQL.InsertParameters["SubjectGroupID"].DefaultValue = SubjectGroupID;
                                        JoinSQL.InsertParameters["SectionID"].DefaultValue = SectionID;
                                        JoinSQL.InsertParameters["ShiftID"].DefaultValue = "0";

                                        JoinSQL.Insert();

                                        MassageLabel.Text += "Section: " + SecRow.Cells[0].Text + " and Group: " + GroupRow.Cells[0].Text + " Joined successfully!<br />";
                                        MassageLabel.ForeColor = System.Drawing.Color.Green;
                                    }
                                    else { ExistLabel.Text += "Sorry!! Section: " + SecRow.Cells[0].Text + " and Group: " + GroupRow.Cells[0].Text + " Already Joined <br />"; }
                                }
                            }
                        }
                    }
                    if (IS_Section)
                    {
                        MassageLabel.Text = "Select Section ";
                    }
                    if (IS_Group)
                    {
                        MassageLabel.Text = "Select Group ";
                    }

                }
                else
                {
                    bool IS_Group = true;
                    bool IS_Section = true;
                    bool IS_Shift = true;

                    foreach (GridViewRow GroupRow in GroupGridView.Rows)
                    {
                        CheckBox GroupCheckBox = (CheckBox)GroupRow.FindControl("GroupCheckBox");

                        if (GroupCheckBox.Checked)
                        {
                            IS_Group = false;

                           string SubjectGroupID = GroupGridView.DataKeys[GroupRow.RowIndex].Value.ToString();

                            foreach (GridViewRow SecRow in SectionGridView.Rows)
                            {

                                CheckBox SectionCheckBox = (CheckBox)SecRow.FindControl("SectionCheckBox");

                                if (SectionCheckBox.Checked)
                                {
                                    IS_Section = false;

                                   string SectionID = SectionGridView.DataKeys[SecRow.RowIndex].Value.ToString();

                                    foreach (GridViewRow ShiftRow in ShiftGridView.Rows)
                                    {
                                        CheckBox ShiftCheckBox = (CheckBox)ShiftRow.FindControl("ShiftCheckBox");

                                        if (ShiftCheckBox.Checked)
                                        {
                                            IS_Shift = false;

                                            string ShiftID = ShiftGridView.DataKeys[ShiftRow.RowIndex].Value.ToString();

                                            SqlCommand JoinGroupSectionShift = new SqlCommand("Select JoinID from [Join] where ClassID = @ClassID and SubjectGroupID = @SubjectGroupID and SectionID = @SectionID and ShiftID = @ShiftID", con);
                                            JoinGroupSectionShift.Parameters.AddWithValue("@ClassID",ClassDropDownList.SelectedValue);
                                            JoinGroupSectionShift.Parameters.AddWithValue("@SubjectGroupID", SubjectGroupID);
                                            JoinGroupSectionShift.Parameters.AddWithValue("@SectionID",SectionID);
                                            JoinGroupSectionShift.Parameters.AddWithValue("@ShiftID",ShiftID);

                                            con.Open();
                                            Object check = JoinGroupSectionShift.ExecuteScalar();
                                            con.Close();

                                            if (check == null)
                                            {
                                                JoinSQL.InsertParameters["SubjectGroupID"].DefaultValue = SubjectGroupID;
                                                JoinSQL.InsertParameters["SectionID"].DefaultValue = SectionID;
                                                JoinSQL.InsertParameters["ShiftID"].DefaultValue = ShiftID;

                                                JoinSQL.Insert();
                                                MassageLabel.Text += "Section: " + SecRow.Cells[0].Text + " and Group: " + GroupRow.Cells[0].Text + " and Shift: " + ShiftRow.Cells[0].Text + " Joined successfully! <br />";
                                                MassageLabel.ForeColor = System.Drawing.Color.Green;
                                            }

                                            else
                                            {
                                                ExistLabel.Text += "Sorry!! Section: " + SecRow.Cells[0].Text + " and Group: " + GroupRow.Cells[0].Text + " and Shift: " + ShiftRow.Cells[0].Text + " Already Joined <br />";
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    if (IS_Shift)
                    {
                        MassageLabel.Text = "Select Shift ";
                    }
                    if (IS_Section)
                    {
                        MassageLabel.Text = "Select Section ";
                    }
                    if (IS_Group)
                    {
                        MassageLabel.Text = "Select Group ";
                    }
                }

                ScShGridView.DataBind();
                ShGrGridView.DataBind();
                SeGrGridView.DataBind();
                ShowJoinGridView.DataBind();
            }

        }

        protected void GroupCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            MassageLabel.Text = string.Empty;
            ExistLabel.Text = string.Empty;
        }

        protected void ClassDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            MassageLabel.Text = string.Empty;
            ExistLabel.Text = string.Empty;

            ScShGridView.DataBind();
            ShGrGridView.DataBind();
            SeGrGridView.DataBind();
            ShowJoinGridView.DataBind();

            DataView GroupDV = new DataView();
            GroupDV = (DataView)GroupSQL.Select(DataSourceSelectArguments.Empty);

            DataView SectionDV = new DataView();
            SectionDV = (DataView)SectionSQL.Select(DataSourceSelectArguments.Empty);

            DataView ShiftDV = new DataView();
            ShiftDV = (DataView)ShiftSQL.Select(DataSourceSelectArguments.Empty);

            if (GroupDV.Count > 0 && SectionDV.Count > 0 && ShiftDV.Count > 0)
            {
                ScShGridView.Visible = false;
                ShGrGridView.Visible = false;
                SeGrGridView.Visible = false;
            }
            else
            {
                ScShGridView.Visible = true;
                ShGrGridView.Visible = true;
                SeGrGridView.Visible = true;
            }
        }
    }
}