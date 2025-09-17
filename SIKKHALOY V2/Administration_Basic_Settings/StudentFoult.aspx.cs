using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.Administration_Basic_Settings
{
    public partial class StudentFoult : System.Web.UI.Page
    {
       protected void Page_Load(object sender, EventArgs e)
        {
            
        }

        ////---Section DDL
        //protected void SectionDropDownList_DataBound(object sender, EventArgs e)
        //{
        //    SectionDropDownList.Items.Insert(0, new ListItem("[ ALL SECTION ]"));
        //}
        //protected void GroupDropDownList_DataBound(object sender, EventArgs e)
        //{
        //    GroupDropDownList.Items.Insert(0, new ListItem("[ ALL GROUP ]"));
        //}

        protected void IncomeGridView_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            GridView_Header_Printer(IncomeGridView);
        }

        private void GridView_Header_Printer(GridView gridView)
        {
            if (gridView.Rows.Count > 0)
            {
                gridView.UseAccessibleHeader = true;
                gridView.HeaderRow.TableSection = TableRowSection.TableHeader;
            }
        }

    }

}