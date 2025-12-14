<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/BASIC.Master" CodeBehind="ExmamPositionBangla.aspx.cs" Inherits="EDUCATION.COM.Exam.ExmamPositionBangla" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="CSS/ExamPosition.css?v=1.0.5" rel="stylesheet" />
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
                        <input onchange="printHiddenTableColumnByHeader('ক্লাশ মেধা', this);" type="checkbox" id="isHiddenPrintClassCol" />
                        <label for="isHiddenPrintClassCol">Hide ক্লাশ মেধা Column</label>
                    </div>
                    <div class="form-group">
                        <input onchange="printHiddenTableColumnByHeader('শাখা মেধা', this);" type="checkbox" id="isHiddenPrintSectionCol" />
                        <label for="isHiddenPrintSectionCol">Hide শাখা মেধা Column</label>
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
        // Print Option - Add class for print media only (don't hide on screen)
        function printHiddenTableColumn(columnNumber, checkbox) {
            const gridViewId = '<%=StudentsGridView.ClientID %>';
            const table = document.getElementById(gridViewId);
            
            if (!table) {
                console.error('GridView not found!');
                return;
            }

            const isChecked = checkbox.checked;
            console.log('Column:', columnNumber, 'Checked:', isChecked);

            // Get header cell
            const headerRow = table.querySelector('thead tr');
            const headerCell = headerRow ? headerRow.querySelector(`th:nth-child(${columnNumber})`) : null;

            // Get all body cells in this column
            const bodyRows = table.querySelectorAll('tbody tr');

            if (isChecked) {
                // Add class to hide only in print (NOT on screen)
                if (headerCell) {
                    headerCell.classList.add('d-print-none');
                    console.log('Added d-print-none to header column', columnNumber);
                }

                bodyRows.forEach(row => {
                    const cell = row.querySelector(`td:nth-child(${columnNumber})`);
                    if (cell) {
                        cell.classList.add('d-print-none');
                    }
                });
                console.log('Added d-print-none to', bodyRows.length, 'body rows');
            } else {
                // Remove class to show in print
                if (headerCell) {
                    headerCell.classList.remove('d-print-none');
                    console.log('Removed d-print-none from header column', columnNumber);
                }

                bodyRows.forEach(row => {
                    const cell = row.querySelector(`td:nth-child(${columnNumber})`);
                    if (cell) {
                        cell.classList.remove('d-print-none');
                    }
                });
                console.log('Removed d-print-none from', bodyRows.length, 'body rows');
            }
        };

        // Print Option - Find column by header text and hide it in print
        function printHiddenTableColumnByHeader(headerText, checkbox) {
            const gridViewId = '<%=StudentsGridView.ClientID %>';
            const table = document.getElementById(gridViewId);
            
            if (!table) {
                console.error('GridView not found!');
                return;
            }

            const isChecked = checkbox.checked;
            console.log('Looking for header:', headerText, 'Checked:', isChecked);

            // Get header row
            const headerRow = table.querySelector('thead tr');
            if (!headerRow) {
                console.error('Header row not found!');
                return;
            }

            // Find the column index by header text
            const headerCells = headerRow.querySelectorAll('th');
            let columnIndex = -1;
            
            headerCells.forEach((cell, index) => {
                if (cell.textContent.trim() === headerText) {
                    columnIndex = index + 1; // nth-child is 1-based
                    console.log('Found column at index:', columnIndex);
                }
            });

            if (columnIndex === -1) {
                console.error('Column with header "' + headerText + '" not found!');
                return;
            }

            // Get header cell and body cells
            const headerCell = headerRow.querySelector(`th:nth-child(${columnIndex})`);
            const bodyRows = table.querySelectorAll('tbody tr');

            if (isChecked) {
                // Add class to hide only in print (NOT on screen)
                if (headerCell) {
                    headerCell.classList.add('d-print-none');
                    console.log('Added d-print-none to header:', headerText);
                }

                bodyRows.forEach(row => {
                    const cell = row.querySelector(`td:nth-child(${columnIndex})`);
                    if (cell) {
                        cell.classList.add('d-print-none');
                    }
                });
                console.log('Added d-print-none to', bodyRows.length, 'body rows');
            } else {
                // Remove class to show in print
                if (headerCell) {
                    headerCell.classList.remove('d-print-none');
                    console.log('Removed d-print-none from header:', headerText);
                }

                bodyRows.forEach(row => {
                    const cell = row.querySelector(`td:nth-child(${columnIndex})`);
                    if (cell) {
                        cell.classList.remove('d-print-none');
                    }
                });
                console.log('Removed d-print-none from', bodyRows.length, 'body rows');
            }
        };
    </script>




</asp:Content>
