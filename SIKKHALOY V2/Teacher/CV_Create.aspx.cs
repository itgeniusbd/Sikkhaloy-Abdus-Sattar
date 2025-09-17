using System;

namespace EDUCATION.COM.Teacher
{
    public partial class CV_Create : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void EducationButton_Click(object sender, EventArgs e)
        {
            EducationSQL.Insert();
            InstitutionTextBox.Text = "";
            ExamNameTextBox.Text = "";
            ExamYearTextBox.Text = "";
            ResultTextBox.Text = "";
        }

        protected void Job_Button_Click(object sender, EventArgs e)
        {
            JobSQL.Insert();
            JobInstitutionTextBox.Text = "";
            DesignationTextBox.Text = "";
            JobTypeTextBox.Text = "";
            JobYearTextBox.Text = "";
            AddressTextBox.Text = "";
            PhoneTextBox.Text = "";
            EmailTextBox.Text = "";
        }

        protected void SkillButton_Click(object sender, EventArgs e)
        {
            SkillSQL.Insert();
            SkillTextBox.Text = "";
            DescriptionTextBox.Text = "";
        }

        protected void ObjectiveButton_Click(object sender, EventArgs e)
        {
            ObjectiveSQL.Insert();
            CareerObjectiveTextBox.Text = "";
        }

        protected void LanguageButton_Click(object sender, EventArgs e)
        {
            LanguageSQL.Insert();
            LanguageNameTextBox.Text = "";
            LevelTextBox.Text = "";
        }
    }
}