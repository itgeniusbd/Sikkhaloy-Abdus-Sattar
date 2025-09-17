using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.Exam.ExamSetting
{
    public partial class PassMark_Change : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }
        protected void SubExamDownList_DataBound(object sender, EventArgs e)  //...Exam and subject
        {
            SubExamDownList.Items.Insert(0, new ListItem("[ ALL SUB-EXAM ]", "0"));
        }

        protected void ExamDropDownList_DataBound(object sender, EventArgs e)
        {
            ExamDropDownList.Items.Insert(0, new ListItem("[ SELECT EXAM ]", "0"));
        }

        protected void ChangeButton_Click(object sender, EventArgs e)
        {
            double PassMark = 0;
            double FullMark = 0;
            bool All_Check = true;

            foreach (GridViewRow row in SubExamGridView.Rows)
            {
                TextBox PassMarkTextBox = (TextBox)row.FindControl("PassMarkTextBox");

                if (double.TryParse(PassMarkTextBox.Text, out PassMark) && double.TryParse(SubExamGridView.DataKeys[row.RowIndex]["FullMarks"].ToString(), out FullMark))
                {
                    if (FullMark < PassMark)
                    {
                        All_Check = false;
                    }
                }
                else
                {
                    All_Check = false;
                }


                SubExamGridView.DataKeys[row.RowIndex]["ExamFullMarksID"].ToString();

                SubExamGridView.DataKeys[row.RowIndex]["FullMarks"].ToString();
            }

            if (All_Check)
            {
                foreach (GridViewRow row in SubExamGridView.Rows)
                {
                    TextBox PassMarkTextBox = (TextBox)row.FindControl("PassMarkTextBox");
                    PassMarkSQL.UpdateParameters["ExamFullMarksID"].DefaultValue = SubExamGridView.DataKeys[row.RowIndex]["ExamFullMarksID"].ToString();
                    PassMarkSQL.UpdateParameters["Sub_PassMarks"].DefaultValue = PassMarkTextBox.Text;
                    PassMarkSQL.Update();
                }
                //---------SP Update Exam_Mark_Re_Submit
                PassMarkSQL.Insert();
            }


        }

    }
}