<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/BASIC.Master" CodeBehind="ExmamPositionBangla.aspx.cs" Inherits="EDUCATION.COM.Exam.ExmamPositionBangla" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="CSS/ExamPosition.css?v=1.0.5" rel="stylesheet" />
    <link href="https://fonts.maateen.me/kalpurush/font.css" rel="stylesheet">
    <style>
        /* Subject columns: equal width, header wraps */
        #<%= StudentsGridView.ClientID %> th.subject-col,
        #<%= StudentsGridView.ClientID %> td.subject-col {
            width: 70px !important;
            min-width: 70px !important;
            max-width: 70px !important;
            word-wrap: break-word;
            white-space: normal;
            text-align: center;
        }
        /* Name column fixed width */
        #<%= StudentsGridView.ClientID %> th:nth-child(3),
        #<%= StudentsGridView.ClientID %> td:nth-child(3) {
            width: 150px !important;
            min-width: 150px !important;
            max-width: 150px !important;
            word-wrap: break-word;
            white-space: normal;
        }
        #<%= StudentsGridView.ClientID %> th {
            white-space: normal;
            word-wrap: break-word;
        }
        .mGrid th {
    padding: 0.3rem 0.1rem ;
    border: 1px solid #717783;
    font-size: 11px;
    font-weight: 400;
    background-color: #4B515D;
    color: #fff;
}
        .mGrid td {
    padding: 0.3rem 0.1rem !important;
    border: 1px solid #dee2e6;
    color: #000;
    font-size: 11px;
    font-weight: 300;
        font-weight: bold;
}
    </style>
    <style media="print">
        .FthSub {
            color: #304ffe;
            font-size: 12px;
        }
        body {
            font-family: 'Kalpurush', serif;
        }
        .mGrid td {
            font-weight: bold !important;
            font-size: 11px !important;
            color: #000 !important;
        }
        .mGrid th {
            font-weight: bold !important;
            font-size: 11px !important;
        }
        .First, .Second, .Third {
            font-weight: bold !important;
        }
        /* GridView header repeated on each printed page */
        #<%= StudentsGridView.ClientID %> thead { 
            display: table-header-group; 
        }
        #<%= StudentsGridView.ClientID %> tfoot { 
            display: table-footer-group; 
        }
        .NoPrint { 
            display: none !important; 
        }
        .signature-section {
            margin-top: 60px;
            page-break-inside: avoid;
            font-size:16px;
            font-weight: bold !important;
            
        }
    </style>
    
    <style>
        /* Merit position colors for both screen and print */
        .First {
            background-color: #28a745 !important; /* Green for 1st position */
            color: white !important;
            font-weight: bold !important;
            font-size: 16px !important;
        }
        
        .Second {
            background-color: #1e90ff !important; /* Blue for 2nd position */
            color: white !important;
            font-weight: bold !important;
            font-size: 16px !important;
        }
        
        .Third {
            background-color: #ff8c00 !important; /* Orange for 3rd position */
            color: white !important;
            font-weight: bold !important;
            font-size: 16px !important;
        }
        
        /* Failed student row background */
        .RowColor {
            background-color: #ffebee !important; /* Light red background */
        }
        
        /* Merit position text styling */
        .merit-text {
            font-size: 16px !important;
            font-weight: bold !important;
        }
        
        /* Enhanced print CSS for column hiding */
        @media print {
            .d-print-none {
                display: none !important;
                visibility: hidden !important;
            }
            
            .First {
                background-color: #28a745 !important;
                color: white !important;
                font-weight: bold !important;
                font-size: 16px !important;
                -webkit-print-color-adjust: exact !important;
                color-adjust: exact !important;
            }
            
            .Second {
                background-color: #1e90ff !important;
                color: white !important;
                font-weight: bold !important;
                font-size: 16px !important;
                -webkit-print-color-adjust: exact !important;
                color-adjust: exact !important;
            }
            
            .Third {
                background-color: #ff8c00 !important;
                color: white !important;
                font-weight: bold !important;
                font-size: 16px !important;
                -webkit-print-color-adjust: exact !important;
                color-adjust: exact !important;
            }
            
            .RowColor {
                background-color: #ffebee !important;
                -webkit-print-color-adjust: exact !important;
                color-adjust: exact !important;
            }
            
            .merit-text {
                font-size: 16px !important;
                font-weight: bold !important;
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
    <div class="d-print-none text-right mb-1">

    </div>

    <div class="d-print-none mb-2" style="background:#f8f9fa; border:1px solid #dee2e6; border-radius:6px; padding:8px 14px;">

        <strong style="margin-right:8px; font-size:13px;">যে কোন কলাম হাইড করে প্রিন্ট দিতে টিক উঠান:</strong>
        <label style="margin-right:10px; font-size:13px; cursor:pointer;"><input type="checkbox" class="col-toggle" data-header="আইডি" checked style="width:15px; height:15px; display:inline-block; opacity:1; position:relative; margin-right:4px; cursor:pointer;" /> আইডি</label>
        <label style="margin-right:10px; font-size:13px; cursor:pointer;"><input type="checkbox" class="col-toggle" data-header="রোল" checked style="width:15px; height:15px; display:inline-block; opacity:1; position:relative; margin-right:4px; cursor:pointer;" /> রোল</label>
        <label style="margin-right:10px; font-size:13px; cursor:pointer;"><input type="checkbox" class="col-toggle" data-header="মোট" checked style="width:15px; height:15px; display:inline-block; opacity:1; position:relative; margin-right:4px; cursor:pointer;" /> মোট</label>
        <label style="margin-right:10px; font-size:13px; cursor:pointer;"><input type="checkbox" class="col-toggle" data-header="গড়" checked style="width:15px; height:15px; display:inline-block; opacity:1; position:relative; margin-right:4px; cursor:pointer;" /> গড়</label>
        <label style="margin-right:10px; font-size:13px; cursor:pointer;"><input type="checkbox" class="col-toggle" data-header="গ্রেড" checked style="width:15px; height:15px; display:inline-block; opacity:1; position:relative; margin-right:4px; cursor:pointer;" /> গ্রেড</label>
        <label style="margin-right:10px; font-size:13px; cursor:pointer;"><input type="checkbox" class="col-toggle" data-header="পয়েন্ট" checked style="width:15px; height:15px; display:inline-block; opacity:1; position:relative; margin-right:4px; cursor:pointer;" /> পয়েন্ট</label>
        <label style="margin-right:10px; font-size:13px; cursor:pointer;"><input type="checkbox" class="col-toggle" data-header="ক্লাশ মেধা" checked style="width:15px; height:15px; display:inline-block; opacity:1; position:relative; margin-right:4px; cursor:pointer;" /> ক্লাশ মেধা</label>
        <label style="margin-right:10px; font-size:13px; cursor:pointer;"><input type="checkbox" class="col-toggle" data-header="শাখা মেধা" checked style="width:15px; height:15px; display:inline-block; opacity:1; position:relative; margin-right:4px; cursor:pointer;" /> শাখা মেধা</label>
            <button type="button" class="btn btn-primary" onclick="window.print()">
            <i class="fa fa-print" aria-hidden="true"></i> Print
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
    OnRowCreated="StudentsGridView_RowCreated"
    OnSorting="StudentsGridView_Sorting"
    OnRowDataBound="StudentsGridView_RowDataBound">
    <Columns>
        <asp:BoundField DataField="RollNo" HeaderText="রোল" SortExpression="RollNo" />
        <asp:BoundField DataField="StudentsName" HeaderText="নাম" />
        <asp:BoundField DataField="Total" HeaderText="মোট" />
        <asp:BoundField DataField="Average" HeaderText="গড়" />
        <asp:BoundField DataField="Student_Grade" HeaderText="গ্রেড" />
        <asp:BoundField DataField="Student_Point" HeaderText="পয়েন্ট" />
        <asp:BoundField DataField="Position_InExam_Class" HeaderText="ক্লাশ মেধা" SortExpression="Position_InExam_Class" />
        <asp:BoundField DataField="Position_InExam_Subsection" HeaderText="শাখা মেধা" SortExpression="Position_InExam_Subsection" />
       
    </Columns>
</asp:GridView>

        </div>

    </div>

    <%if (StudentsGridView.Rows.Count > 0)
        {%>
    <div class="signature-section" style="margin-top: 30px; page-break-inside: avoid;">
        <table style="width: 100%; border-collapse: collapse; font-family: 'Kalpurush', serif;">
            <tr>
              <td style="width: 33%; text-align: center; padding-top: 45px;">
                    <div style="width: 130px; border-top: 1px solid #000; margin: 0 auto 6px auto;"></div>
                    <strong style="font-size: 15px; font-weight: bold;">পরীক্ষা নিয়ন্ত্রক</strong>
                </td>
                <td style="width: 33%; text-align: center; padding-top: 45px;">
                    <div style="width: 130px; border-top: 1px solid #000; margin: 0 auto 6px auto;"></div>
                    <strong style="font-size: 15px; font-weight: bold;">শ্রেণি শিক্ষক</strong>
                </td>

                <td style="width: 33%; text-align: center; padding-top: 45px;">
                    <div style="width: 130px; border-top: 1px solid #fff; margin: 0 auto 6px auto;"></div>
                    <strong style="font-size: 15px; font-weight: bold;"></strong>
                </td>
            </tr>
        </table>
    </div>
    <%}%>






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
        function applyColToggleChange() {
            var table = $("[id*=StudentsGridView]");
            $(".col-toggle").each(function () {
                var headerText = $(this).data("header");
                var visible = $(this).is(":checked");
                var colNum = -1;
                table.find("thead th").each(function (i) {
                    if ($(this).text().trim() === headerText) { colNum = i + 1; return false; }
                });
                if (colNum === -1) return;
                table.find("tr").each(function () {
                    $(this).find("th:nth-child(" + colNum + "), td:nth-child(" + colNum + ")").toggle(visible);
                });
            });
        }

        $(document).ready(function () {
            $(document).on("change", ".col-toggle", function () {
                applyColToggleChange();
            });
        });

        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
            applyColToggleChange();
        });
    </script>




</asp:Content>
