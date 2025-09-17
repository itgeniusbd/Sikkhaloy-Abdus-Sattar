<%@ Page Title="Paid And Due Report" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Paid_And_Due_Report.aspx.cs" Inherits="EDUCATION.COM.Accounts.AccountsAnalysis.Paid_And_Due_Report" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .wp-box { margin-bottom: 15px; border: 1px solid #ddd; padding: 10px 0; text-align: center; }
        .amount { color: #333; font-size: 17px; }
            .amount:before { content: '৳'; font-size: 13px; }
        .a-title { font-size: 13px; color: #666; }

        .parentGrid { width: 100%; border: none; color: #333; }
            .parentGrid > tbody > tr > td { border: none; }
            .parentGrid .pgr table { margin: 3px 0; }
            .parentGrid .pgr td { font-weight: bold; line-height: 12px; padding: 2px 6px; color: #00a12a; }
            .parentGrid .pgr a { color: #000; text-decoration: none; }
                .parentGrid.pgr a:hover { color: #000; text-decoration: none; }

        .mGrid th { background-color: #fff; border: 1px solid #ddd; color: #333; font-size: 14px; padding: 5px; text-align: center; }
        .info { font-size:1rem; margin-bottom:8px;}
        @media print {
            .mGrid th { color: #000 !important; }
                .mGrid th a { color: #000 !important; }
        }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <h3>Paid And Due Report
        <label id="Class_Se"></label>
    </h3>

    <asp:UpdatePanel ID="UpdatePanel1" runat="server">
        <ContentTemplate>
            <div class="form-inline NoPrint">
                <div class="form-group">
                    <asp:RadioButtonList ID="PDRadioButtonList" CssClass="form-control" runat="server" RepeatDirection="Horizontal" AutoPostBack="True">
                        <asp:ListItem Value="%">All</asp:ListItem>
                        <asp:ListItem>Paid</asp:ListItem>
                        <asp:ListItem Selected="True">Due</asp:ListItem>
                    </asp:RadioButtonList>
                </div>
                <div class="form-group mx-1">
                    <asp:DropDownList ID="ClassDropDownList" runat="server" AppendDataBoundItems="True" CssClass="form-control" DataSourceID="ClassNameSQL" DataTextField="Class" DataValueField="ClassID" AutoPostBack="True">
                        <asp:ListItem Value="">ALL CLASS </asp:ListItem>
                    </asp:DropDownList>
                    <asp:SqlDataSource ID="ClassNameSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT ClassID, SchoolID, RegistrationID, Class FROM CreateClass WHERE (SchoolID = @SchoolID)">
                        <SelectParameters>
                            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                        </SelectParameters>
                    </asp:SqlDataSource>
                </div>
                <div class="S_Show form-group mx-1" style="display: none">
                    <asp:DropDownList ID="SectionDropDownList" runat="server" AutoPostBack="True" CssClass="form-control" DataSourceID="SectionSQL" DataTextField="Section" DataValueField="SectionID" OnDataBound="SectionDropDownList_DataBound">
                    </asp:DropDownList>
                    <asp:SqlDataSource ID="SectionSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT DISTINCT [Join].SectionID, CreateSection.Section FROM [Join] INNER JOIN CreateSection ON [Join].SectionID = CreateSection.SectionID WHERE ([Join].ClassID = @ClassID)" CancelSelectOnNullParameter="False">
                        <SelectParameters>
                            <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                        </SelectParameters>
                    </asp:SqlDataSource>
                </div>
                <div class="form-group mx-1">
                    <asp:DropDownList ID="RoleDropDownList" runat="server" DataSourceID="RoleSQL" DataTextField="Role" DataValueField="RoleID" CssClass="form-control" AutoPostBack="True" OnDataBound="RoleDropDownList_DataBound">
                    </asp:DropDownList>
                    <asp:SqlDataSource ID="RoleSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT DISTINCT Income_Roles.Role, Income_Roles.RoleID FROM Income_PayOrder INNER JOIN Income_Roles ON Income_PayOrder.RoleID = Income_Roles.RoleID WHERE (Income_PayOrder.SchoolID = @SchoolID) AND (Income_PayOrder.EducationYearID = @EducationYearID) AND (Income_PayOrder.ClassID LIKE ISNULL(@ClassID,'%'))" CancelSelectOnNullParameter="False">
                        <SelectParameters>
                            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                            <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                            <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                        </SelectParameters>
                    </asp:SqlDataSource>
                </div>
                <div class="form-group mx-1">
                    <asp:DropDownList ID="PayForDropDownList" runat="server" DataSourceID="PayForSQL" DataTextField="PayFor" DataValueField="PayFor" CssClass="form-control" AutoPostBack="True" OnDataBound="PayForDropDownList_DataBound">
                    </asp:DropDownList>
                    <asp:SqlDataSource ID="PayForSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT DISTINCT PayFor FROM Income_PayOrder WHERE (SchoolID = @SchoolID) AND (EducationYearID = @EducationYearID) AND (RoleID LIKE @RoleID) AND (ClassID LIKE  ISNULL(@ClassID,'%'))" CancelSelectOnNullParameter="False">
                        <SelectParameters>
                            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                            <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                            <asp:ControlParameter ControlID="RoleDropDownList" Name="RoleID" PropertyName="SelectedValue" />
                            <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                        </SelectParameters>
                    </asp:SqlDataSource>
                </div>
                <div class="form-group">
                    <asp:TextBox ID="FromDateTextBox" placeholder="From Date" runat="server" CssClass="form-control Datetime" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false"></asp:TextBox>
                </div>
                <div class="form-group ml-2">
                    <asp:TextBox ID="ToDateTextBox" placeholder="To Date" runat="server" CssClass="form-control Datetime" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false"></asp:TextBox>
                </div>
                <div class="form-group">
                    <asp:Button ID="SearchButton" runat="server" Text="Search" CssClass="btn btn-primary" />
                </div>
            </div>

            <asp:FormView ID="TotalFormView" runat="server" DataSourceID="TotalSQL" Width="100%">
                <ItemTemplate>
                    <div class="row">
                        <div class="col-lg-2 col-sm-4">
                            <div class="wp-box">
                                <div class="amount"><%# Eval("TotalFee","{0:N0}") %></div>
                                <div class="a-title">Fee</div>
                            </div>
                        </div>
                        <div class="col-lg-2 col-sm-4">
                            <div class="wp-box">
                                <div class="amount"><%# Eval("TotalLateFee","{0:N0}") %></div>
                                <div class="a-title">Late Fee</div>
                            </div>
                        </div>
                        <div class="col-lg-2 col-sm-4">
                            <div class="wp-box">
                                <div class="amount"><%# Eval("TotalDiscount","{0:N0}") %></div>
                                <div class="a-title">Fee Concession</div>
                            </div>
                        </div>
                        <div class="col-lg-2 col-sm-4">
                            <div class="wp-box">
                                <div class="amount"><%# Eval("Receivable_Amount","{0:N0}") %></div>
                                <div class="a-title">Receivable Amount</div>
                            </div>
                        </div>
                        <div class="col-lg-2 col-sm-4">
                            <div class="wp-box">
                                <div class="amount"><%# Eval("TotalPaid","{0:N0}") %></div>
                                <div class="a-title">Paid</div>
                            </div>
                        </div>
                        <div class="col-lg-2 col-sm-4">
                            <div class="wp-box">
                                <div class="amount"><%# Eval("Unpaid","{0:N0}") %></div>
                                <div class="a-title">Due</div>
                            </div>
                        </div>
                    </div>

                    <div class="alert alert-dark">
                        Total Student: <%# Eval("Total_Stu") %>
                    </div>
                </ItemTemplate>
            </asp:FormView>
            <asp:SqlDataSource ID="TotalSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT 
COUNT(DISTINCT Income_PayOrder.StudentID) AS Total_Stu, 
SUM(Income_PayOrder.Amount) AS TotalFee,
SUM(Income_PayOrder.LateFeeCountable) AS TotalLateFee, 
SUM(Income_PayOrder.Total_Discount) AS TotalDiscount, 
SUM( ISNULL(Income_PayOrder.Amount, 0) - ISNULL(Income_PayOrder.Total_Discount, 0)) AS Receivable_Amount,
SUM(ISNULL(Income_PayOrder.PaidAmount, 0)) AS TotalPaid,
SUM(Income_PayOrder.Receivable_Amount) AS Unpaid
FROM Income_PayOrder 
			INNER JOIN StudentsClass ON Income_PayOrder.StudentClassID = StudentsClass.StudentClassID 
			INNER JOIN Income_Roles ON Income_PayOrder.RoleID = Income_Roles.RoleID 
WHERE (Income_PayOrder.PayFor LIKE  @PayFor ) AND 
      (Income_Roles.RoleID LIKE  @RoleID)  AND 
	  (Income_PayOrder.ClassID LIKE  ISNULL(@ClassID,'%')) AND 
	  (StudentsClass.SectionID LIKE @SectionID) AND 
	  (Income_PayOrder.SchoolID = @SchoolID) AND  
	  (Income_PayOrder.EducationYearID = @EducationYearID) AND  
	  (Income_PayOrder.Is_Active = 1) AND
	   Income_PayOrder.EndDate BETWEEN ISNULL(@From_Date, '1-1-1000') AND ISNULL(@To_Date, '1-1-3000')"
                CancelSelectOnNullParameter="False">
                <SelectParameters>
                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                    <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                    <asp:ControlParameter ControlID="PayForDropDownList" Name="PayFor" PropertyName="SelectedValue" />
                    <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" DefaultValue="" />
                    <asp:ControlParameter ControlID="RoleDropDownList" Name="RoleID" PropertyName="SelectedValue" />
                    <asp:ControlParameter ControlID="SectionDropDownList" Name="SectionID" PropertyName="SelectedValue" />
                    <asp:ControlParameter ControlID="FromDateTextBox" Name="From_Date" PropertyName="Text" />
                    <asp:ControlParameter ControlID="ToDateTextBox" Name="To_Date" PropertyName="Text" />
                </SelectParameters>
            </asp:SqlDataSource>

            <asp:GridView ID="DueGridView" runat="server" AutoGenerateColumns="False" DataSourceID="DueSQL" AllowSorting="True" CssClass="parentGrid" AllowPaging="True" ShowHeader="False">
                <Columns>
                    <asp:TemplateField>
                        <ItemTemplate>
                            <div class="table-responsive mb-4">
                                <div class="info">
                                    <div class="pull-left">
                                        [<%# Eval("ID") %>] <%# Eval("StudentsName") %>. <b>Roll No.</b> <%# Eval("RollNo") %>.
                                    <b>Class: </b><%# Eval("Class") %>
                                    </div>
                                    <div class="pull-right">
                                        <b>Paid:</b> <%# Eval("Total_Paid") %> Tk.
                                    <b>Due:</b> <%# Eval("Unpaid") %> Tk.
                                    </div>
                                    <div class="clearfix"></div>
                                </div>

                                <asp:HiddenField ID="StudentClassID_HF" runat="server" Value='<%# Eval("StudentClassID") %>' />
                                <asp:GridView ID="PDDetailsGridView" runat="server" AutoGenerateColumns="False" DataSourceID="PDDetailsSQL" CssClass="mGrid">
                                    <Columns>
                                        <asp:BoundField DataField="Role" HeaderText="Role" SortExpression="Role" />
                                        <asp:BoundField DataField="PayFor" HeaderText="Pay For" SortExpression="PayFor" />
                                        <asp:BoundField DataField="Amount" HeaderText="Amount" SortExpression="Amount" />
                                        <asp:BoundField DataField="Total_Discount" HeaderText="Conc." SortExpression="Total_Discount" />
                                        <asp:BoundField DataField="LateFee" HeaderText="L.F" SortExpression="LateFee" />
                                        <asp:BoundField DataField="PaidAmount" HeaderText="Paid" SortExpression="PaidAmount" />
                                        <asp:BoundField DataField="Receivable_Amount" HeaderText="Due" SortExpression="Receivable_Amount" />
                                        <asp:BoundField DataField="Status" HeaderText="Status" SortExpression="Status" />
                                    </Columns>
                                </asp:GridView>
                                <asp:SqlDataSource ID="PDDetailsSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT Income_PayOrder.StudentID, 	
	   Income_Roles.Role,
	   Income_PayOrder.PayFor, 
	   Income_PayOrder.Amount, 
	   Income_PayOrder.LateFee, 
	   Income_PayOrder.Total_Discount, 
	   Income_PayOrder.PaidAmount, 
	   Income_PayOrder.Receivable_Amount,  
	   Income_PayOrder.Status
FROM Income_PayOrder INNER JOIN 
     Income_Roles ON Income_PayOrder.RoleID = Income_Roles.RoleID 
WHERE (Income_PayOrder.StudentClassID = @StudentClassID) AND
      (Income_PayOrder.RoleID LIKE @RoleID) AND 
      (Income_PayOrder.PayFor LIKE @PayFor) AND 
      (Income_PayOrder.Status LIKE @Status) AND 
	  Income_PayOrder.EndDate BETWEEN ISNULL(@From_Date, '1-1-1000') AND ISNULL(@To_Date, '1-1-3000') ORDER BY EndDate"
                                    CancelSelectOnNullParameter="False">
                                    <SelectParameters>
                                        <asp:ControlParameter ControlID="PayForDropDownList" Name="PayFor" PropertyName="SelectedValue" />
                                        <asp:ControlParameter ControlID="PDRadioButtonList" Name="Status" PropertyName="SelectedValue" />
                                        <asp:ControlParameter ControlID="RoleDropDownList" Name="RoleID" PropertyName="SelectedValue" />
                                        <asp:ControlParameter ControlID="StudentClassID_HF" Name="StudentClassID" PropertyName="Value" />
                                        <asp:ControlParameter ControlID="FromDateTextBox" Name="From_Date" PropertyName="Text" />
                                        <asp:ControlParameter ControlID="ToDateTextBox" Name="To_Date" PropertyName="Text" />
                                    </SelectParameters>
                                </asp:SqlDataSource>
                            </div>
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
                <PagerSettings FirstPageText="First" LastPageText="Last" Mode="NumericFirstLast" NextPageText="Next" PreviousPageText="Previous" />
                <PagerStyle CssClass="pgr" HorizontalAlign="Right" />
                <EmptyDataTemplate>
                    No result found!
                </EmptyDataTemplate>
            </asp:GridView>
            <asp:SqlDataSource ID="DueSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                SelectCommand="SELECT Income_PayOrder.StudentID, 
                Income_PayOrder.StudentClassID,
				CreateClass.ClassID,
				Student.ID,
				Student.StudentsName,
				StudentsClass.RollNo,
				CreateClass.Class,
				SUM(Income_PayOrder.PaidAmount) AS Total_Paid,
				SUM(Income_PayOrder.Receivable_Amount) AS Unpaid,
			    max(Income_PayOrder.LastPaidDate) as LastPaidDate 
FROM Income_PayOrder INNER JOIN
     Student ON Income_PayOrder.StudentID = Student.StudentID INNER JOIN
     StudentsClass ON Income_PayOrder.StudentClassID = StudentsClass.StudentClassID INNER JOIN
     CreateClass ON Income_PayOrder.ClassID = CreateClass.ClassID 
WHERE (Income_PayOrder.PayFor LIKE @PayFor) AND 
      (Income_PayOrder.Status LIKE @Status) AND 
	  (Income_PayOrder.SchoolID = @SchoolID) AND 
	  (Income_PayOrder.EducationYearID = @EducationYearID) AND 
	   Income_PayOrder.EndDate BETWEEN ISNULL(@From_Date, '1-1-1000') AND ISNULL(@To_Date, '1-1-3000') AND 
	  (Income_PayOrder.Is_Active = 1) AND 
	  (StudentsClass.SectionID LIKE @SectionID) AND 
	  (Income_PayOrder.RoleID LIKE @RoleID) AND 
	  (Income_PayOrder.ClassID LIKE ISNULL(@ClassID, N'%')) 
GROUP BY Income_PayOrder.StudentID, Income_PayOrder.StudentClassID,Student.ID,CreateClass.ClassID, CreateClass.Class, Student.StudentsName, StudentsClass.RollNo
ORDER BY CreateClass.ClassID,CASE WHEN ISNUMERIC(StudentsClass.RollNo) = 1 THEN CAST(REPLACE(REPLACE(StudentsClass.RollNo, '$', ''), ',', '') AS INT) ELSE 0 END"
                CancelSelectOnNullParameter="False">
                <SelectParameters>
                    <asp:ControlParameter ControlID="PayForDropDownList" Name="PayFor" PropertyName="SelectedValue" />
                    <asp:ControlParameter ControlID="PDRadioButtonList" Name="Status" PropertyName="SelectedValue" />
                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                    <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                    <asp:ControlParameter ControlID="SectionDropDownList" Name="SectionID" PropertyName="SelectedValue" />
                    <asp:ControlParameter ControlID="RoleDropDownList" Name="RoleID" PropertyName="SelectedValue" />
                    <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" DefaultValue="" />
                    <asp:ControlParameter ControlID="FromDateTextBox" Name="From_Date" PropertyName="Text" />
                    <asp:ControlParameter ControlID="ToDateTextBox" Name="To_Date" PropertyName="Text" />
                </SelectParameters>
            </asp:SqlDataSource>
        </ContentTemplate>
    </asp:UpdatePanel>


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


    <script type="text/javascript">
        $(function () {
            $('.Datetime').datepicker({
                format: 'dd M yyyy',
                todayBtn: "linked",
                todayHighlight: true,
                autoclose: true
            });

            if ($('[id*=SectionDropDownList]').find('option').length > 1) {
                $(".S_Show").show();
            }
        });

        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function (a, b) {
            $('.Datetime').datepicker({
                format: 'dd M yyyy',
                todayBtn: "linked",
                todayHighlight: true,
                autoclose: true
            });

            if ($('[id*=SectionDropDownList]').find('option').length > 1) {
                $(".S_Show").show();
            }
            var Class = "";
            if ($('[id*=ClassDropDownList] :selected').index() > 0) {
                Class = " Class: " + $('[id*=ClassDropDownList] :selected').text();
            }
            var Section = "";
            if ($('[id*=SectionDropDownList] :selected').index() > 0) {
                Section = " Section: " + $('[id*=SectionDropDownList] :selected').text();
            }

            $("#Class_Se").text(Class + Section);
        })

        function isNumberKey(a) { a = a.which ? a.which : event.keyCode; return 46 != a && 31 < a && (48 > a || 57 < a) ? !1 : !0 };
    </script>
</asp:Content>
