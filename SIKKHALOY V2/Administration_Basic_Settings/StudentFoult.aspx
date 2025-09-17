<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/BASIC.Master" CodeBehind="StudentFoult.aspx.cs" Inherits="EDUCATION.COM.Administration_Basic_Settings.StudentFoult" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="CSS/Student_Paid.css?v=2" rel="stylesheet" />
    <link href="/CSS/bootstrap-multiselect.css" rel="stylesheet" />
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
 
    <h3>Student Report/Foult Details</h3>
    <div class="form-inline NoPrint">

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
        <asp:TextBox ID="FormDateTextBox" runat="server" autocomplete="off" CssClass="form-control Datetime" onDrop="blur();return false;" onkeypress="return isNumberKey(event)" onpaste="return false"></asp:TextBox>
    </div>

    <div class="form-group">
        <asp:TextBox ID="ToDateTextBox" runat="server" CssClass="form-control Datetime" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false"></asp:TextBox>
        <i id="PickDate" class="glyphicon glyphicon-calendar fa fa-calendar"></i>
    </div>
        <div class="form-group">
            <asp:TextBox ID="IDTextBox" placeholder="Enter ID" autocomplete="off" runat="server" CssClass="form-control"></asp:TextBox>
          </div>
    <div class="form-group">
        <asp:Button ID="SubmitButton" runat="server" CssClass="btn btn-primary" Text="Find" ValidationGroup="1" />
    </div>
    <div class="form-group">
        <button type="button" class="btn btn-grey hidden-print" onclick="window.print()"><span class="glyphicon glyphicon-print" aria-hidden="true"></span>Print</button>
    </div>
        
</div>
                    

    <div class="table-responsive">
        <asp:GridView ID="IncomeGridView" runat="server" AutoGenerateColumns="False" DataSourceID="StudentFoultSQL" CssClass="mGrid" AllowPaging="True" PageSize="35" AllowSorting="True">
            <Columns>
                <asp:TemplateField HeaderText="Students Name" SortExpression="StudentsName">
                    <ItemTemplate>
                        
                    <leble><%# Eval("StudentsName") %> </leble>
                    <leble>( <%# Eval("ID") %>)</leble>
                    </ItemTemplate>
                </asp:TemplateField>

   <asp:BoundField DataField="FathersName" HeaderText="Father's Name" SortExpression="FathersName"></asp:BoundField>
  <asp:BoundField DataField="SMSPhoneNo" HeaderText="SMS Phone" SortExpression="SMSPhoneNo" />
   <asp:BoundField DataField="Fault_Title" HeaderText="Fault Title" SortExpression="Fault_Title"></asp:BoundField>
 <asp:BoundField DataField="Fault" HeaderText="Report/Fault" SortExpression="Fault"> </asp:BoundField>
  <asp:BoundField DataField="Fault_Date" HeaderText="Fault Date" DataFormatString="{0:d MMM yyyy}" SortExpression="Fault_Date"></asp:BoundField>
   <asp:BoundField DataField="InsertDate"  HeaderText="Insert Date" DataFormatString="{0:d MMM yyyy}" SortExpression="InsertDate"></asp:BoundField>
<asp:BoundField DataField="UserName" HeaderText="Submited By" SortExpression="UserName"></asp:BoundField>
               
            </Columns>
           
        </asp:GridView>
        <asp:SqlDataSource ID="StudentFoultSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT Student.StudentsName,Student.ID,Student.FathersName,Student.MothersName,Student.SMSPhoneNo,StudentsClass.ClassID,StudentsClass.SectionID,StudentsClass.SubjectGroupID,StudentsClass.ShiftID, 
Student_Fault.Fault_Title, Student_Fault.EducationYearID,Student_Fault.Fault,Student_Fault.Fault_Date,Registration.UserName,
Student_Fault.InsertDate from Student_Fault inner join Student on Student_Fault.StudentID=Student.StudentID
inner join StudentsClass on Student_Fault.StudentClassID=StudentsClass.StudentClassID
inner join Registration on Student_Fault.RegistrationID=Registration.RegistrationID 
WHERE 
    (Student_Fault.SchoolID = @SchoolID ) 
and (Student_Fault.EducationYearID = @EducationYearID)
and ((StudentsClass.ClassID = @ClassID) OR (@ClassID = 0)) 
AND (CAST(Student_Fault.Fault_Date AS DATE) BETWEEN ISNULL(@From_Date, '1-1-1000') AND ISNULL(@To_Date, '1-1-3000'))
AND (Student.ID LIKE @Id+'%')" CancelSelectOnNullParameter="False">
            <SelectParameters>
                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                <asp:ControlParameter ControlID="FormDateTextBox" Name="From_Date" PropertyName="Text" />
                <asp:ControlParameter ControlID="ToDateTextBox" Name="To_Date" PropertyName="Text" />
                <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                <asp:ControlParameter ControlID="IDTextBox" DefaultValue="%" Name="Id" PropertyName="Text" />
            </SelectParameters>
        </asp:SqlDataSource>
    </div>

    <asp:UpdateProgress ID="UpdateProgress1" runat="server">
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

       
    </script>


</asp:Content>