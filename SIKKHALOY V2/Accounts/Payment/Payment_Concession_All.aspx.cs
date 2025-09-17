using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.Accounts.Payment
{
    public partial class Payment_Concession_All : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }
        protected void MSNLinkButton_Command(object sender, CommandEventArgs e)
        {
            PaidRecordsSQL.SelectParameters["MoneyReceiptID"].DefaultValue = e.CommandArgument.ToString();
            ReceivedBySQL.SelectParameters["MoneyReceiptID"].DefaultValue = e.CommandArgument.ToString();
            ScriptManager.RegisterStartupScript(this, this.GetType(), "Pop", "openModal();", true);
        }

        protected void SubmitButton_Click(object sender, EventArgs e)
        {
            bool All_Checked = true;
            string Error_Text = "";

            #region Check Inserted Amount
            foreach (GridViewRow row in DueGridView.Rows)
            {
                if (row.RowType == DataControlRowType.DataRow)
                {
                    // For Late Fee
                    Label PrevLateFeeLabel = (Label)DueGridView.Rows[row.RowIndex].FindControl("PrevLateFeeLabel");
                    TextBox LFeeAmountTextBox = (TextBox)DueGridView.Rows[row.RowIndex].FindControl("LateFeeTextBox");

                    // For Late Fee Discount
                    Label LateFeeDiscountLable = (Label)DueGridView.Rows[row.RowIndex].FindControl("LateFeeDiscountLable");
                    TextBox LateFeeDiscountTextBox = (TextBox)DueGridView.Rows[row.RowIndex].FindControl("LateFeeDiscountTextBox");

                    //Fee Amount discount
                    TextBox DiscountTextBox = (TextBox)DueGridView.Rows[row.RowIndex].FindControl("DiscountTextBox");
                    Label DiscountLabel = (Label)DueGridView.Rows[row.RowIndex].FindControl("DiscountLabel");
                    Label DueLabel = (Label)DueGridView.Rows[row.RowIndex].FindControl("DueLabel");

                    double Due = 0;
                    double New_Amount = 0;
                    double Prv_Amount = 0;
                    double Fee = 0;

                    Double.TryParse(DueLabel.Text, out Due);
                    Double.TryParse(DiscountTextBox.Text, out New_Amount);
                    Double.TryParse(DiscountLabel.Text, out Prv_Amount);
                    Double.TryParse(DueGridView.DataKeys[row.RowIndex]["Amount"].ToString(), out Fee);

                    if (Due + Prv_Amount >= New_Amount)
                    {
                        if (Fee >= New_Amount)
                        {
                            //Late fee discount
                            double Latefee = 0;
                            double LatefeeDiscount = 0;

                            Double.TryParse(LFeeAmountTextBox.Text, out Latefee);
                            Double.TryParse(LateFeeDiscountTextBox.Text, out LatefeeDiscount);

                            if (Latefee >= LatefeeDiscount)
                            {
                                if (Due + Latefee >= LatefeeDiscount)
                                {
                                    if (0 <= (Due + Latefee + Prv_Amount) - (LatefeeDiscount + New_Amount))
                                    {

                                    }
                                    else
                                    {
                                        All_Checked = false;
                                        Error_Text = "Fee Amount more than Due Amount";
                                    }
                                }
                                else
                                {
                                    All_Checked = false;
                                    Error_Text = "Late Fee Concession amount more than Due Amount";
                                }
                            }
                            else
                            {
                                All_Checked = false;
                                Error_Text = "Late Fee Concession amount more than Late Fee Amount";
                            }
                        }
                        else
                        {
                            All_Checked = false;
                            Error_Text = "Concession amount more than Fee Amount";
                        }
                    }
                    else
                    {
                        All_Checked = false;
                        Error_Text = "Concession amount more than Due Amount";
                    }
                }
            }
            #endregion Check Inserted Amount

            #region Insert/Update Amount
            if (All_Checked)
            {
                foreach (GridViewRow row in DueGridView.Rows)
                {
                    if (row.RowType == DataControlRowType.DataRow)
                    {
                        // For Late Fee
                        Label PrevLateFeeLabel = (Label)DueGridView.Rows[row.RowIndex].FindControl("PrevLateFeeLabel");
                        TextBox LFeeAmountTextBox = (TextBox)DueGridView.Rows[row.RowIndex].FindControl("LateFeeTextBox");

                        // For Late Fee Discount
                        Label LateFeeDiscountLable = (Label)DueGridView.Rows[row.RowIndex].FindControl("LateFeeDiscountLable");
                        TextBox LateFeeDiscountTextBox = (TextBox)DueGridView.Rows[row.RowIndex].FindControl("LateFeeDiscountTextBox");

                        //Fee Amount discount
                        TextBox DiscountTextBox = (TextBox)DueGridView.Rows[row.RowIndex].FindControl("DiscountTextBox");
                        Label DiscountLabel = (Label)DueGridView.Rows[row.RowIndex].FindControl("DiscountLabel");

                        //Fee Amount discount
                        if (DiscountTextBox.Text != DiscountLabel.Text)
                        {
                            Fee_DiscountSQL.InsertParameters["StudentID"].DefaultValue = DueGridView.DataKeys[row.RowIndex]["StudentID"].ToString();
                            Fee_DiscountSQL.InsertParameters["PayOrderID"].DefaultValue = DueGridView.DataKeys[row.RowIndex]["PayOrderID"].ToString();
                            Fee_DiscountSQL.InsertParameters["StudentClassID"].DefaultValue = DueGridView.DataKeys[row.RowIndex]["StudentClassID"].ToString();
                            Fee_DiscountSQL.InsertParameters["PostAmount"].DefaultValue = DiscountTextBox.Text;
                            Fee_DiscountSQL.InsertParameters["PreviousAmount"].DefaultValue = DiscountLabel.Text;
                            Fee_DiscountSQL.InsertParameters["EducationYearID"].DefaultValue = Session["Edu_Year"].ToString();

                            Fee_DiscountSQL.Insert();

                            Fee_DiscountSQL.UpdateParameters["Discount"].DefaultValue = DiscountTextBox.Text;
                            Fee_DiscountSQL.UpdateParameters["PayOrderID"].DefaultValue = DueGridView.DataKeys[row.RowIndex]["PayOrderID"].ToString();
                            Fee_DiscountSQL.Update();
                        }

                        // For Late Fee
                        if (PrevLateFeeLabel.Text != LFeeAmountTextBox.Text)
                        {
                            LateFeeChangeSQL.InsertParameters["StudentID"].DefaultValue = DueGridView.DataKeys[row.RowIndex]["StudentID"].ToString();
                            LateFeeChangeSQL.InsertParameters["PayOrderID"].DefaultValue = DueGridView.DataKeys[row.RowIndex]["PayOrderID"].ToString();
                            LateFeeChangeSQL.InsertParameters["StudentClassID"].DefaultValue = DueGridView.DataKeys[row.RowIndex]["StudentClassID"].ToString();
                            LateFeeChangeSQL.InsertParameters["PostAmount"].DefaultValue = LFeeAmountTextBox.Text;
                            LateFeeChangeSQL.InsertParameters["PreviousAmount"].DefaultValue = PrevLateFeeLabel.Text;
                            LateFeeChangeSQL.InsertParameters["EducationYearID"].DefaultValue = Session["Edu_Year"].ToString();
                            LateFeeChangeSQL.Insert();

                            LateFeeChangeSQL.UpdateParameters["LateFee"].DefaultValue = LFeeAmountTextBox.Text;
                            LateFeeChangeSQL.UpdateParameters["PayOrderID"].DefaultValue = DueGridView.DataKeys[row.RowIndex]["PayOrderID"].ToString();
                            LateFeeChangeSQL.Update();
                        }

                        // For Late Fee discount 
                        if (LateFeeDiscountLable.Text != LateFeeDiscountTextBox.Text)
                        {
                            LateFee_DiscountSQL.InsertParameters["StudentID"].DefaultValue = DueGridView.DataKeys[row.RowIndex]["StudentID"].ToString();
                            LateFee_DiscountSQL.InsertParameters["PayOrderID"].DefaultValue = DueGridView.DataKeys[row.RowIndex]["PayOrderID"].ToString();
                            LateFee_DiscountSQL.InsertParameters["StudentClassID"].DefaultValue = DueGridView.DataKeys[row.RowIndex]["StudentClassID"].ToString();
                            LateFee_DiscountSQL.InsertParameters["PostAmount"].DefaultValue = LateFeeDiscountTextBox.Text;
                            LateFee_DiscountSQL.InsertParameters["PreviousAmount"].DefaultValue = LateFeeDiscountLable.Text;
                            LateFee_DiscountSQL.InsertParameters["EducationYearID"].DefaultValue = Session["Edu_Year"].ToString();

                            LateFee_DiscountSQL.Insert();

                            LateFee_DiscountSQL.UpdateParameters["LateFee_Discount"].DefaultValue = LateFeeDiscountTextBox.Text;
                            LateFee_DiscountSQL.UpdateParameters["PayOrderID"].DefaultValue = DueGridView.DataKeys[row.RowIndex]["PayOrderID"].ToString();
                            LateFee_DiscountSQL.Update();
                        }
                    }
                }
            }
            else
            {
                ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('" + Error_Text + "')", true);
            }
            #endregion Insert/Update Amount

            #region Check Inserted Amount OtherSessionGridView
            foreach (GridViewRow row in OtherSessionGridView.Rows)
            {
                if (row.RowType == DataControlRowType.DataRow)
                {
                    // For Late Fee
                    Label PrevLateFeeLabel = (Label)OtherSessionGridView.Rows[row.RowIndex].FindControl("PrevLateFeeLabel");
                    TextBox LFeeAmountTextBox = (TextBox)OtherSessionGridView.Rows[row.RowIndex].FindControl("LateFeeTextBox");

                    // For Late Fee Discount
                    Label LateFeeDiscountLable = (Label)OtherSessionGridView.Rows[row.RowIndex].FindControl("LateFeeDiscountLable");
                    TextBox LateFeeDiscountTextBox = (TextBox)OtherSessionGridView.Rows[row.RowIndex].FindControl("LateFeeDiscountTextBox");

                    //Fee Amount discount
                    TextBox DiscountTextBox = (TextBox)OtherSessionGridView.Rows[row.RowIndex].FindControl("DiscountTextBox");
                    Label DiscountLabel = (Label)OtherSessionGridView.Rows[row.RowIndex].FindControl("DiscountLabel");
                    Label DueLabel = (Label)OtherSessionGridView.Rows[row.RowIndex].FindControl("DueLabel");

                    double Due = 0;
                    double New_Amount = 0;
                    double Prv_Amount = 0;
                    double Fee = 0;

                    Double.TryParse(DueLabel.Text, out Due);
                    Double.TryParse(DiscountTextBox.Text, out New_Amount);
                    Double.TryParse(DiscountLabel.Text, out Prv_Amount);
                    Double.TryParse(OtherSessionGridView.DataKeys[row.RowIndex]["Amount"].ToString(), out Fee);

                    if (Due + Prv_Amount >= New_Amount)
                    {
                        if (Fee >= New_Amount)
                        {
                            //Late fee discount
                            double Latefee = 0;
                            double LatefeeDiscount = 0;

                            Double.TryParse(LFeeAmountTextBox.Text, out Latefee);
                            Double.TryParse(LateFeeDiscountTextBox.Text, out LatefeeDiscount);

                            if (Latefee >= LatefeeDiscount)
                            {
                                if (Due + Latefee >= LatefeeDiscount)
                                {
                                    if (0 <= (Due + Latefee + Prv_Amount) - (LatefeeDiscount + New_Amount))
                                    {

                                    }
                                    else
                                    {
                                        All_Checked = false;
                                        Error_Text = "Fee Amount more than Due Amount";
                                    }
                                }
                                else
                                {
                                    All_Checked = false;
                                    Error_Text = "Late Fee Concession amount more than Due Amount";
                                }
                            }
                            else
                            {
                                All_Checked = false;
                                Error_Text = "Late Fee Concession amount more than Late Fee Amount";
                            }
                        }
                        else
                        {
                            All_Checked = false;
                            Error_Text = "Concession amount more than Fee Amount";
                        }
                    }
                    else
                    {
                        All_Checked = false;
                        Error_Text = "Concession amount more than Due Amount";
                    }
                }
            }
            #endregion Check Inserted Amount OtherSessionGridView

            #region Insert/Update Amount OtherSessionGridView
            if (All_Checked)
            {
                foreach (GridViewRow row in OtherSessionGridView.Rows)
                {
                    if (row.RowType == DataControlRowType.DataRow)
                    {
                        // For Late Fee
                        Label PrevLateFeeLabel = (Label)OtherSessionGridView.Rows[row.RowIndex].FindControl("PrevLateFeeLabel");
                        TextBox LFeeAmountTextBox = (TextBox)OtherSessionGridView.Rows[row.RowIndex].FindControl("LateFeeTextBox");

                        // For Late Fee Discount
                        Label LateFeeDiscountLable = (Label)OtherSessionGridView.Rows[row.RowIndex].FindControl("LateFeeDiscountLable");
                        TextBox LateFeeDiscountTextBox = (TextBox)OtherSessionGridView.Rows[row.RowIndex].FindControl("LateFeeDiscountTextBox");

                        //Fee Amount discount
                        TextBox DiscountTextBox = (TextBox)OtherSessionGridView.Rows[row.RowIndex].FindControl("DiscountTextBox");
                        Label DiscountLabel = (Label)OtherSessionGridView.Rows[row.RowIndex].FindControl("DiscountLabel");

                        //Fee Amount discount
                        if (DiscountTextBox.Text != DiscountLabel.Text)
                        {
                            Fee_DiscountSQL.InsertParameters["StudentID"].DefaultValue = OtherSessionGridView.DataKeys[row.RowIndex]["StudentID"].ToString();
                            Fee_DiscountSQL.InsertParameters["PayOrderID"].DefaultValue = OtherSessionGridView.DataKeys[row.RowIndex]["PayOrderID"].ToString();
                            Fee_DiscountSQL.InsertParameters["StudentClassID"].DefaultValue = OtherSessionGridView.DataKeys[row.RowIndex]["StudentClassID"].ToString();
                            Fee_DiscountSQL.InsertParameters["PostAmount"].DefaultValue = DiscountTextBox.Text;
                            Fee_DiscountSQL.InsertParameters["PreviousAmount"].DefaultValue = DiscountLabel.Text;
                            Fee_DiscountSQL.InsertParameters["EducationYearID"].DefaultValue = OtherSessionGridView.DataKeys[row.RowIndex]["EducationYearID"].ToString();
                            Fee_DiscountSQL.Insert();

                            Fee_DiscountSQL.UpdateParameters["Discount"].DefaultValue = DiscountTextBox.Text;
                            Fee_DiscountSQL.UpdateParameters["PayOrderID"].DefaultValue = OtherSessionGridView.DataKeys[row.RowIndex]["PayOrderID"].ToString();
                            Fee_DiscountSQL.Update();
                        }

                        // For Late Fee
                        if (PrevLateFeeLabel.Text != LFeeAmountTextBox.Text)
                        {
                            LateFeeChangeSQL.InsertParameters["StudentID"].DefaultValue = OtherSessionGridView.DataKeys[row.RowIndex]["StudentID"].ToString();
                            LateFeeChangeSQL.InsertParameters["PayOrderID"].DefaultValue = OtherSessionGridView.DataKeys[row.RowIndex]["PayOrderID"].ToString();
                            LateFeeChangeSQL.InsertParameters["StudentClassID"].DefaultValue = OtherSessionGridView.DataKeys[row.RowIndex]["StudentClassID"].ToString();
                            LateFeeChangeSQL.InsertParameters["PostAmount"].DefaultValue = LFeeAmountTextBox.Text;
                            LateFeeChangeSQL.InsertParameters["PreviousAmount"].DefaultValue = PrevLateFeeLabel.Text;
                            LateFeeChangeSQL.InsertParameters["EducationYearID"].DefaultValue = OtherSessionGridView.DataKeys[row.RowIndex]["EducationYearID"].ToString();
                            LateFeeChangeSQL.Insert();

                            LateFeeChangeSQL.UpdateParameters["LateFee"].DefaultValue = LFeeAmountTextBox.Text;
                            LateFeeChangeSQL.UpdateParameters["PayOrderID"].DefaultValue = OtherSessionGridView.DataKeys[row.RowIndex]["PayOrderID"].ToString();
                            LateFeeChangeSQL.Update();
                        }

                        // For Late Fee discount 
                        if (LateFeeDiscountLable.Text != LateFeeDiscountTextBox.Text)
                        {
                            LateFee_DiscountSQL.InsertParameters["StudentID"].DefaultValue = OtherSessionGridView.DataKeys[row.RowIndex]["StudentID"].ToString();
                            LateFee_DiscountSQL.InsertParameters["PayOrderID"].DefaultValue = OtherSessionGridView.DataKeys[row.RowIndex]["PayOrderID"].ToString();
                            LateFee_DiscountSQL.InsertParameters["StudentClassID"].DefaultValue = OtherSessionGridView.DataKeys[row.RowIndex]["StudentClassID"].ToString();
                            LateFee_DiscountSQL.InsertParameters["PostAmount"].DefaultValue = LateFeeDiscountTextBox.Text;
                            LateFee_DiscountSQL.InsertParameters["PreviousAmount"].DefaultValue = LateFeeDiscountLable.Text;
                            LateFee_DiscountSQL.InsertParameters["EducationYearID"].DefaultValue = OtherSessionGridView.DataKeys[row.RowIndex]["EducationYearID"].ToString();
                            LateFee_DiscountSQL.Insert();

                            LateFee_DiscountSQL.UpdateParameters["LateFee_Discount"].DefaultValue = LateFeeDiscountTextBox.Text;
                            LateFee_DiscountSQL.UpdateParameters["PayOrderID"].DefaultValue = OtherSessionGridView.DataKeys[row.RowIndex]["PayOrderID"].ToString();
                            LateFee_DiscountSQL.Update();
                        }
                    }
                } 
            }
            else
            {
                ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('" + Error_Text + "')", true);
            }



            #endregion Insert/Update Amount OtherSessionGridView

            if (All_Checked)
            { 
                Response.Redirect(Request.Url.AbsoluteUri); 
            }
        }

        protected void DueGridView_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                if (Convert.ToDateTime(e.Row.Cells[5].Text) < DateTime.Today)
                {
                    e.Row.CssClass = "PresentDue";
                }
            }
        }

        protected void OtherSessionGridView_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                if (Convert.ToDateTime(e.Row.Cells[7].Text) < DateTime.Today)
                {
                    e.Row.CssClass = "PresentDue";
                }
            }
        }
    }
}