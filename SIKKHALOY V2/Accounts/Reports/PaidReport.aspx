<%@ Page Title="Student Paid Details" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="PaidReport.aspx.cs" Inherits="EDUCATION.COM.Accounts.Reports.PaidReport" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="CSS/Student_Paid.css?v=2" rel="stylesheet" />
    <link href="/CSS/bootstrap-multiselect.css" rel="stylesheet" />
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">

    <h3 class="NoPrint">Student Paid Details</h3>
    <div class="form-inline NoPrint">
        <div class="form-group">
            <asp:DropDownList ID="SessionDownList" runat="server" AppendDataBoundItems="True" AutoPostBack="True" CssClass="form-control" DataSourceID="SessionSQL" DataTextField="EducationYear" DataValueField="EducationYearID">
                <asp:ListItem Value="%">[ ALL Session ]</asp:ListItem>
            </asp:DropDownList>
            <asp:SqlDataSource ID="SessionSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT DISTINCT Education_Year.EducationYearID, Education_Year.EducationYear FROM Education_Year INNER JOIN Income_MoneyReceipt ON Education_Year.EducationYearID = Income_MoneyReceipt.EducationYearID WHERE (Income_MoneyReceipt.SchoolID = @SchoolID) ORDER BY Education_Year.EducationYearID">
                <SelectParameters>
                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                </SelectParameters>
            </asp:SqlDataSource>
        </div>
        <div class="form-group">
            <asp:DropDownList ID="ClassDropDownList" runat="server" AppendDataBoundItems="True" AutoPostBack="True" CssClass="form-control" DataSourceID="ClassNameSQL" DataTextField="Class" DataValueField="ClassID">
                <asp:ListItem Value="0">[ ALL CLASS ]</asp:ListItem>
            </asp:DropDownList>
            <asp:SqlDataSource ID="ClassNameSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT DISTINCT CreateClass.ClassID, CreateClass.Class FROM CreateClass INNER JOIN StudentsClass ON CreateClass.ClassID = StudentsClass.ClassID INNER JOIN Income_MoneyReceipt ON StudentsClass.StudentClassID = Income_MoneyReceipt.StudentClassID WHERE (Income_MoneyReceipt.SchoolID = @SchoolID) ORDER BY CreateClass.ClassID">
                <SelectParameters>
                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                </SelectParameters>
            </asp:SqlDataSource>
        </div>
                        <div class="form-group">
            <asp:DropDownList ID="GroupDropDownList" runat="server" AutoPostBack="True" CssClass="form-control" DataSourceID="GroupSQL" DataTextField="SubjectGroup" DataValueField="SubjectGroupID" OnDataBound="GroupDropDownList_DataBound">
            </asp:DropDownList>
            <asp:SqlDataSource ID="GroupSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT DISTINCT [Join].SubjectGroupID, CreateSubjectGroup.SubjectGroup FROM [Join] INNER JOIN CreateSubjectGroup ON [Join].SubjectGroupID = CreateSubjectGroup.SubjectGroupID WHERE ([Join].ClassID = @ClassID) AND ([Join].SectionID LIKE N'%' + @SectionID + N'%')">
                <SelectParameters>
                    <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                    <asp:ControlParameter ControlID="SectionDropDownList" Name="SectionID" PropertyName="SelectedValue" />
                   </SelectParameters>
            </asp:SqlDataSource>
        </div>
        <div class="form-group S_Show" style="display: none">
            <asp:DropDownList ID="SectionDropDownList" runat="server" AutoPostBack="True" CssClass="form-control" DataSourceID="SectionSQL" DataTextField="Section" DataValueField="SectionID" OnDataBound="SectionDropDownList_DataBound">
            </asp:DropDownList>
            <asp:SqlDataSource ID="SectionSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT DISTINCT [Join].SectionID, CreateSection.Section FROM [Join] INNER JOIN CreateSection ON [Join].SectionID = CreateSection.SectionID WHERE ([Join].ClassID = @ClassID)">
                <SelectParameters>
                    <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                </SelectParameters>
            </asp:SqlDataSource>
        </div>


        
        <div class="form-group">
            <asp:TextBox ID="FormDateTextBox" runat="server" autocomplete="off" CssClass="form-control Datetime" onDrop="blur();return false;" onkeypress="return isNumberKey(event)" onpaste="return false"></asp:TextBox>
        </div>

        <div class="form-group">
            <asp:TextBox ID="ToDateTextBox" runat="server" CssClass="form-control Datetime" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false"></asp:TextBox>
            <i id="PickDate" class="glyphicon glyphicon-calendar fa fa-calendar"></i>
        </div>

        <div class="form-group">
            <asp:Button ID="SubmitButton" runat="server" CssClass="btn btn-primary" Text="Find" ValidationGroup="1" />
        </div>
        <div class="form-group">
            <button type="button" class="btn btn-grey hidden-print" onclick="window.print()"><span class="glyphicon glyphicon-print" aria-hidden="true"></span>Print</button>
        </div>
    </div>

    <asp:FormView ID="IncomeFormView" runat="server" DataSourceID="TotalIncomeSQL" Width="100%">
        <ItemTemplate>
            <div class="alert alert-secondary amount">
                <label class="Date"></label>
                <label class="Class_Section"></label>
                Income: <%#Eval("TOTAL_INCOME") %> Tk
            </div>
        </ItemTemplate>
    </asp:FormView>
    <asp:SqlDataSource ID="TotalIncomeSQL" runat="server" CancelSelectOnNullParameter="False" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT ISNULL(SUM(Income_PaymentRecord.PaidAmount), 0) AS TOTAL_INCOME
FROM Income_PaymentRecord INNER JOIN Income_MoneyReceipt ON Income_PaymentRecord.MoneyReceiptID = Income_MoneyReceipt.MoneyReceiptID INNER JOIN
StudentsClass ON Income_MoneyReceipt.StudentClassID = StudentsClass.StudentClassID
WHERE        
 (StudentsClass.SchoolID = @SchoolID) AND 
 ((StudentsClass.ClassID = @ClassID)OR(@ClassID = 0)) AND 
 (Income_MoneyReceipt.EducationYearID LIKE @EducationYearID) AND 
 (StudentsClass.SectionID LIKE @SectionID) AND 
 (StudentsClass.SubjectGroupID LIKE @SubjectGroupID) AND 
 (CAST(Income_MoneyReceipt.PaidDate AS DATE) BETWEEN ISNULL(@From_Date, '1-1-1000') AND ISNULL(@To_Date, '1-1-3000'))">
        <SelectParameters>
            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
            <asp:ControlParameter ControlID="FormDateTextBox" Name="From_Date" PropertyName="Text" />
            <asp:ControlParameter ControlID="ToDateTextBox" Name="To_Date" PropertyName="Text" />
            <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
            <asp:ControlParameter ControlID="SectionDropDownList" Name="SectionID" PropertyName="SelectedValue" />
            <asp:ControlParameter ControlID="GroupDropDownList" Name="SubjectGroupID" PropertyName="SelectedValue" />
            <asp:ControlParameter ControlID="SessionDownList" Name="EducationYearID" PropertyName="SelectedValue" />
        </SelectParameters>
    </asp:SqlDataSource>

    <div class="table-responsive">
        <asp:GridView ID="IncomeGridView" runat="server" AutoGenerateColumns="False" DataSourceID="IncomeSQL" CssClass="mGrid" AllowPaging="True" PageSize="35" AllowSorting="True" OnRowDataBound="IncomeGridView_RowDataBound">
            <Columns>
                <asp:BoundField DataField="MoneyReceipt_SN" HeaderText="RCPT.NO" SortExpression="MoneyReceipt_SN">
                    <ItemStyle Width="54px" />
                </asp:BoundField>
                <asp:BoundField DataField="ID" HeaderText="ID" SortExpression="ID" />
                <asp:BoundField DataField="StudentsName" HeaderText="Name" SortExpression="StudentsName" />
                <asp:BoundField DataField="RollNo" HeaderText="Roll No." SortExpression="RollNo" />
                <asp:BoundField DataField="Class" HeaderText="Class" SortExpression="Class" />
                <asp:BoundField DataField="Section" HeaderText="Section" SortExpression="Section" />
                <asp:TemplateField HeaderText="Paid Details">
                    <ItemTemplate>
                        <asp:Repeater ID="PRRepeater" runat="server" DataSourceID="PRSQL">
                            <ItemTemplate>
                                <p><%# Eval("Role") +" For "+Eval("PayFor")+" ("+Eval("EducationYear")+") :  "+Eval("PaidAmount") %></p>
                            </ItemTemplate>
                        </asp:Repeater>
                        <asp:HiddenField ID="MRidHiddenField" runat="server" Value='<%# Bind("MoneyReceiptID")%>' />
                        <asp:SqlDataSource ID="PRSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT Income_Roles.Role, Income_PaymentRecord.PayFor, Income_PaymentRecord.PaidAmount, Education_Year.EducationYear FROM Income_PaymentRecord INNER JOIN Income_Roles ON Income_PaymentRecord.RoleID = Income_Roles.RoleID INNER JOIN Education_Year ON Income_PaymentRecord.EducationYearID = Education_Year.EducationYearID WHERE (Income_PaymentRecord.MoneyReceiptID = @MoneyReceiptID)">
                            <SelectParameters>
                                <asp:ControlParameter ControlID="MRidHiddenField" Name="MoneyReceiptID" PropertyName="Value" />
                            </SelectParameters>
                        </asp:SqlDataSource>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:BoundField DataField="TotalAmount" HeaderText="Total" SortExpression="TotalAmount" />
                <asp:BoundField DataField="PaidDate" HeaderText="P.Date" SortExpression="PaidDate" DataFormatString="{0:d-MM-yy}" />
            </Columns>
            <PagerSettings Mode="NumericFirstLast" PageButtonCount="5" LastPageText="Last" FirstPageText="First" NextPageText="Next" PreviousPageText="Previous" />
            <PagerStyle CssClass="pgr" />
        </asp:GridView>
        <asp:SqlDataSource ID="IncomeSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT Student.ID, StudentsClass.RollNo, Student.StudentsName, CreateClass.Class, CreateSection.Section, Income_MoneyReceipt.MoneyReceipt_SN, 
CAST(Income_MoneyReceipt.PaidDate AS DATE) AS PaidDate,
 Income_MoneyReceipt.MoneyReceiptID, Income_MoneyReceipt.TotalAmount, StudentsClass.ClassID
 FROM Income_MoneyReceipt INNER JOIN 
 Student ON Income_MoneyReceipt.StudentID = Student.StudentID INNER JOIN 
 StudentsClass ON Income_MoneyReceipt.StudentClassID = StudentsClass.StudentClassID LEFT OUTER JOIN 
 CreateSection ON StudentsClass.SectionID = CreateSection.SectionID LEFT OUTER JOIN 
 CreateClass ON StudentsClass.ClassID = CreateClass.ClassID 
 WHERE (StudentsClass.SchoolID = @SchoolID) AND 
 ((StudentsClass.ClassID = @ClassID)OR(@ClassID = 0)) AND 
 (Income_MoneyReceipt.EducationYearID LIKE @EducationYearID) AND 
 (StudentsClass.SectionID LIKE @SectionID) AND 
 (StudentsClass.SubjectGroupID LIKE @SubjectGroupID) AND 
 (CAST(Income_MoneyReceipt.PaidDate AS DATE) BETWEEN ISNULL(@From_Date, '1-1-1000') AND ISNULL(@To_Date, '1-1-3000'))
ORDER BY StudentsClass.ClassID,CASE WHEN ISNUMERIC(StudentsClass.RollNo) = 1 THEN CAST(StudentsClass.RollNo AS int) ELSE 0 END"
            CancelSelectOnNullParameter="False">
            <SelectParameters>
                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                <asp:ControlParameter ControlID="FormDateTextBox" Name="From_Date" PropertyName="Text" />
                <asp:ControlParameter ControlID="ToDateTextBox" Name="To_Date" PropertyName="Text" />
                <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                <asp:ControlParameter ControlID="GroupDropDownList" Name="SubjectGroupID" PropertyName="SelectedValue" />
                <asp:ControlParameter ControlID="SectionDropDownList" Name="SectionID" PropertyName="SelectedValue" />
                <asp:ControlParameter ControlID="SessionDownList" Name="EducationYearID" PropertyName="SelectedValue" />
            </SelectParameters>
        </asp:SqlDataSource>
    </div>

    <asp:UpdateProgress ID="UpdateProgress" runat="server">
        <ProgressTemplate>
            <div id="progress_BG"></div>
            <div id="progress">
                <img src="/CSS/loading.gif" alt="Loading..." />
                <br />
                <b>Loading...</b>
            </div>
        </ProgressTemplate>
    </asp:UpdateProgress>


    <script src="/JS/bootstrap-multiselect.js"></script>
    <script type="text/javascript">
        $('[id*=RoleListBox]').multiselect({
            includeSelectAllOption: true,
            enableFiltering: true,
            includeResetOption: true,
            includeResetDivider: true,
            nonSelectedText: 'All Payment Role'
        });

        $(function () {
            $('.Datetime').datepicker({
                format: 'dd M yyyy',
                todayBtn: "linked",
                todayHighlight: true,
                autoclose: true
            });

            if ($('[id*=SectionDropDownList] option').length > 1) {
                $('.S_Show').show();
            }

            //get date in label
            var from = $("[id*=FormDateTextBox]").val();
            var To = $("[id*=ToDateTextBox]").val();

            var tt;
            var Brases1 = "";
            var Brases2 = "";
            var A = "";
            var B = "";
            var TODate = "";

            if (To == "" || from == "" || To == "" && from == "") {
                tt = "";
                A = "";
                B = "";
            }
            else {
                tt = " To ";
                Brases1 = "(";
                Brases2 = ")";
            }

            if (To == "" && from == "") { Brases1 = ""; }

            if (To == from) {
                TODate = "";
                tt = "";
                var Brases1 = "";
                var Brases2 = "";
            }
            else { TODate = To; }

            if (from == "" && To != "") {
                B = " Before ";
            }

            if (To == "" && from != "") {
                A = " After ";
            }

            if (from != "" && To != "") {
                A = "";
                B = "";
            }

            $(".Date").text(Brases1 + B + A + from + tt + TODate + Brases2);

            //Date range picker
            function cb(start, end) {
                $('[id*=FormDateTextBox]').val(start.format('D MMMM YYYY'));
                $('[id*=ToDateTextBox]').val(end.format('D MMMM YYYY'));

                $("[id*=SubmitButton").trigger("click");
            }

            $('#PickDate').daterangepicker({
                autoApply: true,
                showCustomRangeLabel: false,
                ranges: {
                    'Today': [moment(), moment()],
                    'Yesterday': [moment().subtract(1, 'days'), moment().subtract(1, 'days')],
                    'Last 7 Days': [moment().subtract(6, 'days'), moment()],
                    'Last 30 Days': [moment().subtract(29, 'days'), moment()],
                    'This Month': [moment().startOf('month'), moment().endOf('month')],
                    'Last Month': [moment().subtract(1, 'month').startOf('month'), moment().subtract(1, 'month').endOf('month')],
                    'This Year': [moment().startOf('year'), moment().endOf('year')]
                }
            }, cb);

            cb(start, end);
        });

        function isNumberKey(a) { a = a.which ? a.which : event.keyCode; return 46 != a && 31 < a && (48 > a || 57 < a) ? !1 : !0 };
    </script>
</asp:Content>

