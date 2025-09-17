<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/BASIC.Master" CodeBehind="ExmamPositionBangla.aspx.cs" Inherits="EDUCATION.COM.Exam.ExmamPositionBangla" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="CSS/ExamPosition.css?v=1.0.1" rel="stylesheet" />
    <link href="https://fonts.maateen.me/kalpurush/font.css" rel="stylesheet">
    <style media="print">
        .FthSub {
            color: #304ffe;
            font-size: 12px;
        }
        body {
    font-family: 'Kalpurush', serif;
    font-weight:bolder;
    font-size:large;
}

         /* GridView header repeated on each printed page */
    #<%= StudentsGridView.ClientID %> thead { 
        display: table-header-group; 
    }

    /* GridView footer repeated on each printed page, যদি থাকে */
    #<%= StudentsGridView.ClientID %> tfoot { 
        display: table-footer-group; 
    }

    /* Optional: Hide any element with class 'NoPrint' */
    .NoPrint { 
        display: none !important; 
    }

    @media print {
    #<%= StudentsGridView.ClientID %> thead {
        display: table-header-group;
    }
    #<%= StudentsGridView.ClientID %> tfoot {
        display: table-footer-group;
    }
}
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <a href="ExamPosition_WithSub.aspx" class="NoPrint">Full Tabulation Sheet >>></a>

    <h3 style="text-align: center; font-size: 20px; font-weight: bold;">
        <asp:Label ID="CGSSLabel" runat="server"></asp:Label>

    </h3>

    <div class="form-inline NoPrint">
        <div class="form-group">
            <asp:DropDownList ID="ClassDropDownList" runat="server" CssClass="form-control" AppendDataBoundItems="True" AutoPostBack="True" DataSourceID="ClassNameSQL" DataTextField="Class" DataValueField="ClassID" OnSelectedIndexChanged="ClassDropDownList_SelectedIndexChanged">
                <asp:ListItem Value="0">[ শ্রেনি নির্বাচন করুন ]</asp:ListItem>
            </asp:DropDownList>
            <asp:SqlDataSource ID="ClassNameSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                SelectCommand="SELECT DISTINCT CreateClass.Class, CreateClass.ClassID FROM Exam_Result_of_Student INNER JOIN CreateClass ON Exam_Result_of_Student.ClassID = CreateClass.ClassID WHERE (Exam_Result_of_Student.SchoolID = @SchoolID) AND (Exam_Result_of_Student.EducationYearID = @EducationYearID) ORDER BY CreateClass.ClassID">
                <SelectParameters>
                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                    <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                </SelectParameters>
            </asp:SqlDataSource>
        </div>
        <div class="form-group">
            <asp:DropDownList ID="GroupDropDownList" runat="server" AutoPostBack="True" CssClass="form-control" DataSourceID="GroupSQL" DataTextField="SubjectGroup"
                DataValueField="SubjectGroupID" OnDataBound="GroupDropDownList_DataBound" OnSelectedIndexChanged="GroupDropDownList_SelectedIndexChanged">
            </asp:DropDownList>
            <asp:SqlDataSource ID="GroupSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                SelectCommand="SELECT DISTINCT [Join].SubjectGroupID, CreateSubjectGroup.SubjectGroup FROM [Join] INNER JOIN CreateSubjectGroup ON [Join].SubjectGroupID = CreateSubjectGroup.SubjectGroupID WHERE ([Join].ClassID = @ClassID) AND ([Join].SectionID LIKE @SectionID) AND ([Join].ShiftID LIKE  @ShiftID) ">
                <SelectParameters>
                    <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                    <asp:ControlParameter ControlID="SectionDropDownList" Name="SectionID" PropertyName="SelectedValue" />
                    <asp:ControlParameter ControlID="ShiftDropDownList" Name="ShiftID" PropertyName="SelectedValue" />
                </SelectParameters>
            </asp:SqlDataSource>
        </div>
        <div class="form-group">
            <asp:DropDownList ID="SectionDropDownList" runat="server" AutoPostBack="True" CssClass="form-control"
                DataSourceID="SectionSQL" DataTextField="Section" DataValueField="SectionID"
                OnDataBound="SectionDropDownList_DataBound" OnSelectedIndexChanged="SectionDropDownList_SelectedIndexChanged">
            </asp:DropDownList>
            <asp:SqlDataSource ID="SectionSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                SelectCommand="SELECT DISTINCT [Join].SectionID, CreateSection.Section FROM [Join] INNER JOIN CreateSection ON [Join].SectionID = CreateSection.SectionID WHERE ([Join].ClassID = @ClassID) AND ([Join].SubjectGroupID LIKE @SubjectGroupID) AND ([Join].ShiftID LIKE @ShiftID) ">
                <SelectParameters>
                    <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                    <asp:ControlParameter ControlID="GroupDropDownList" Name="SubjectGroupID" PropertyName="SelectedValue" />
                    <asp:ControlParameter ControlID="ShiftDropDownList" Name="ShiftID" PropertyName="SelectedValue" />
                </SelectParameters>
            </asp:SqlDataSource>
        </div>
        <div class="form-group">
            <asp:DropDownList ID="ShiftDropDownList" runat="server" AutoPostBack="True" CssClass="form-control"
                DataSourceID="ShiftSQL" DataTextField="Shift" DataValueField="ShiftID"
                OnDataBound="ShiftDropDownList_DataBound" OnSelectedIndexChanged="ShiftDropDownList_SelectedIndexChanged">
            </asp:DropDownList>
            <asp:SqlDataSource ID="ShiftSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                SelectCommand="SELECT DISTINCT [Join].ShiftID, CreateShift.Shift FROM [Join] INNER JOIN CreateShift ON [Join].ShiftID = CreateShift.ShiftID WHERE ([Join].SubjectGroupID LIKE @SubjectGroupID) AND ([Join].SectionID LIKE  @SectionID) AND ([Join].ClassID = @ClassID)">
                <SelectParameters>
                    <asp:ControlParameter ControlID="GroupDropDownList" Name="SubjectGroupID" PropertyName="SelectedValue" />
                    <asp:ControlParameter ControlID="SectionDropDownList" Name="SectionID" PropertyName="SelectedValue" />
                    <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                </SelectParameters>
            </asp:SqlDataSource>
        </div>
        <div class="form-group">
            <asp:DropDownList ID="ExamDropDownList" runat="server" AutoPostBack="True" CssClass="form-control"
                DataSourceID="ExamSQL" DataTextField="ExamName" DataValueField="ExamID"
                OnDataBound="ExamDropDownList_DataBound" OnSelectedIndexChanged="ExamDropDownList_SelectedIndexChanged">
            </asp:DropDownList>
            <asp:SqlDataSource ID="ExamSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT DISTINCT Exam_Name.ExamID, Exam_Name.ExamName FROM Exam_Name INNER JOIN Exam_Result_of_Student ON Exam_Name.ExamID = Exam_Result_of_Student.ExamID WHERE (Exam_Name.EducationYearID = @EducationYearID) AND (Exam_Name.SchoolID = @SchoolID) AND (Exam_Result_of_Student.ClassID = @ClassID) ORDER BY Exam_Name.ExamID">
                <SelectParameters>
                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                    <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                    <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                </SelectParameters>
            </asp:SqlDataSource>
        </div>

    </div>


    <%if (StudentsGridView.Rows.Count > 0)
        {%>
    <div class="d-print-none text-right">
        <button type="button" class="btn btn-link" data-toggle="modal" data-target="#printOptionModal">
            <i class="fa fa-cog" aria-hidden="true"></i>
            Print Option
        </button>

        <button type="button" class="btn btn-primary" onclick="window.print()">
            <i class="fa fa-print" aria-hidden="true"></i>
            Print
        </button>
    </div>
    <%}%>


    <div id="ExportPanel" runat="server" class="Exam_Position">
        <asp:Label ID="Export_ClassLabel" runat="server" Font-Bold="True" Font-Names="Tahoma" Font-Size="20px"></asp:Label>
        <div class="table-responsive">

            <asp:GridView ID="StudentsGridView" runat="server" 
    AutoGenerateColumns="False" 
    PagerStyle-CssClass="pgr" 
    AllowSorting="True" 
    CssClass="mGrid"
    OnRowCreated="StudentsGridView_RowCreated">
    <Columns>
        <asp:BoundField DataField="RollNo" HeaderText="Roll No" />
        <asp:BoundField DataField="StudentsName" HeaderText="Student Name" />
        <asp:BoundField DataField="Total" HeaderText="Total" />
        <asp:BoundField DataField="Average" HeaderText="Average" />
           <asp:BoundField DataField="Student_Grade" HeaderText="Grade" />
        <asp:BoundField DataField="Student_Point" HeaderText="Point" />
        <asp:BoundField DataField="Position_InExam_Class" HeaderText="Class Position" />
        <asp:BoundField DataField="Position_InExam_Subsection" HeaderText="Section Position" />
       
    </Columns>
</asp:GridView>

        </div>

    </div>



    <!-- modal print option--->
    <div class="modal fade d-print-none" id="printOptionModal" tabindex="-1" role="dialog" aria-hidden="true">
        <div class="modal-dialog" role="document">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title">Print Option</h5>
                    <button type="button" class="close" data-dismiss="modal" aria-label="Close">
                        <span aria-hidden="true">&times;</span>
                    </button>
                </div>
                <div class="modal-body">
   <div class="form-group">
       <input onchange="toggleColumnByHeader('শাখা মেধা', this);" type="checkbox" id="hideSectionMeritCol" />
       <label for="hideSectionMeritCol">Hide শাখা মেধা</label>
   </div>
                </div>

                <div class="modal-footer">
                    <button type="button" class="btn btn-primary" data-dismiss="modal">OK</button>
                </div>
            </div>
        </div>
    </div>




    <asp:UpdateProgress ID="UpdateProgress" runat="server">
        <ProgressTemplate>
            <div id="progress_BG"></div>
            <div id="progress">
                <img src="../CSS/loading.gif" alt="Loading..." />
                <br />
                <b>Loading...</b>
            </div>
        </ProgressTemplate>
    </asp:UpdateProgress>

    <script type="text/javascript">
  

        
            function toggleColumnByHeader(headerText, checkboxEl) {
        var grid = $('#<%=StudentsGridView.ClientID %>');
            if (!grid || grid.length === 0) return;

            headerText = headerText ? headerText.toString().trim() : '';

            var headerRow = grid.find('tr:first');
            var headerCells = headerRow.children('th,td');

            var matchedIndex = -1;
            headerCells.each(function (i) {
                var txt = $(this).text().trim();
                if (txt === headerText) { matchedIndex = i; return false; }
            });

            // partial match চেষ্টা
            if (matchedIndex === -1) {
                headerCells.each(function (i) {
                    var txt = $(this).text().trim();
                    if (txt.indexOf(headerText) !== -1) { matchedIndex = i; return false; }
                });
            }

            if (matchedIndex === -1) return; // না পেলে কিছু করবে না

            var idx = matchedIndex;
            var hide = $(checkboxEl).is(':checked');

            // header
            if (hide) {
                headerCells.eq(idx).addClass('d-print-none');
            } else {
                headerCells.eq(idx).removeClass('d-print-none');
            }

            // সব data row
            grid.find('tr').not(':first').each(function () {
                var $cells = $(this).children('td,th');
                if ($cells.length > idx) {
                    if (hide) {
                        $cells.eq(idx).addClass('d-print-none');
                    } else {
                        $cells.eq(idx).removeClass('d-print-none');
                    }
                }
            });
        }
    </script>




</asp:Content>
