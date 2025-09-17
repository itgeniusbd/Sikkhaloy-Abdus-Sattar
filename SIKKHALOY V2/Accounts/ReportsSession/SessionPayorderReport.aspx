<%@ Page Title="Class based payorder report" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="SessionPayorderReport.aspx.cs" Inherits="EDUCATION.COM.Accounts.ReportsSession.SessionPayorder" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="CSS/Session_Payorder.css?v=2.1" rel="stylesheet" />
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <asp:UpdatePanel ID="UpdatePanel1" runat="server">
        <ContentTemplate>
            <div class="row NoPrint">
                <div class="col-lg-4">
                    <div class="form-group">
                        <label>SESSION YEAR</label>
                        <asp:DropDownList ID="Session_DropDownList" CssClass="form-control" runat="server" DataSourceID="All_SessionSQL" DataTextField="EducationYear" DataValueField="EducationYearID" AutoPostBack="True">
                        </asp:DropDownList>
                        <asp:SqlDataSource ID="All_SessionSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT * FROM [Education_Year] WHERE ([SchoolID] = @SchoolID)">
                            <SelectParameters>
                                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" Type="Int32" />
                            </SelectParameters>
                        </asp:SqlDataSource>
                    </div>
                </div>
            </div>

            <asp:FormView ID="SessionFormView" runat="server" DataKeyNames="EducationYearID" DataSourceID="SessionSQL" Width="100%">
                <ItemTemplate>
                    <div class="Session-Title">
                        <div class="row">
                            <div class="col-sm-6">
                                <div class="p-2">
                                    <div class="session-name">
                                        <i class="fa fa-table"></i>
                                        CLASS BASED PAYORDER REPORT
                                    </div>
                                    <div class="sort-Date">
                                        <i class="fa fa-calendar"></i>
                                        <label class="Date"></label>
                                    </div>
                                </div>
                            </div>
                            <div class="col-sm-6">
                                <div class="text-right p-2">
                                    <div class="session-name">
                                        <i class="fa fa-database"></i>
                                        SESSION: <%# Eval("EducationYear") %>
                                    </div>
                                    <div class="session-date">
                                        <i class="fa fa-calendar"></i>
                                        <%# Eval("StartDate","{0:d MMM yyyy}") %>  - <%# Eval("EndDate","{0:d MMM yyyy}") %>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </ItemTemplate>
            </asp:FormView>
            <asp:SqlDataSource ID="SessionSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT EducationYearID, EducationYear, StartDate, EndDate FROM Education_Year WHERE (SchoolID = @SchoolID) AND (EducationYearID = @EducationYearID)">
                <SelectParameters>
                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                    <asp:ControlParameter ControlID="Session_DropDownList" Name="EducationYearID" PropertyName="SelectedValue" />
                </SelectParameters>
            </asp:SqlDataSource>

            <div class="form-inline NoPrint">
                <div class="form-group">
                    <asp:TextBox ID="From_Date_TextBox" CssClass="form-control datepicker" placeholder="From Date" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false" runat="server"></asp:TextBox>
                </div>
                <div class="form-group">
                    <asp:TextBox ID="To_Date_TextBox" CssClass="form-control datepicker" placeholder="From Date" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false" runat="server"></asp:TextBox>
                </div>
                <div class="form-group">
                    <asp:Button ID="Find_Button" CssClass="btn btn-primary" runat="server" Text="Find" />
                </div>
                <div class="form-group">
                    <a title="Print This Page" onclick="window.print();"><i class="fa fa-print" aria-hidden="true"></i></a>
                </div>
            </div>

            <asp:FormView ID="Sessi_Student_FormView" runat="server" DataSourceID="Stu_P_SQL" Width="100%">
                <ItemTemplate>
                    <div id="Print-Set" class="row">
                        <div class="col-lg-2 col-md-4">
                            <div class="Fees-section Student-bg">
                                <asp:LinkButton ID="Student_LB" title="Click To More Details" runat="server" OnClick="Student_LB_Click">
                                <div class="tile-description">Student <i class="fa fa-caret-right"></i></div>
                                <div class="tile-number"><%#Eval("Total_Stu") %></div>
                                </asp:LinkButton>
                            </div>
                        </div>
                        <div class="col-lg-2 col-md-4">
                            <div class="Fees-section Payorder-bg">
                                <asp:LinkButton ID="TotalFee_LB" title="Click To More Details" runat="server" OnClick="Student_LB_Click">
                        <div class="tile-description">Payorder <i class="fa fa-caret-right"></i></div>
                        <div class="tile-number"><%#Eval("TotalFee","{0:N0}") %> Tk</div>
                                </asp:LinkButton>
                            </div>
                        </div>
                        <div class="col-lg-2 col-md-4">
                            <div class="Fees-section Late-Fee-bg">
                                <div class="tile-description">Late Fee</div>
                                <div class="tile-number"><%#Eval("TotalLateFee","{0:N0}") %> Tk</div>
                            </div>
                        </div>
                        <div class="col-lg-2 col-md-4">
                            <div class="Fees-section Concession-bg">
                                <asp:LinkButton ID="Concession_LB" title="Click To More Details" runat="server" OnClick="Concession_LB_Click">
                        <div class="tile-description">Concession <i class="fa fa-caret-right"></i></div>
                        <div class="tile-number"><%#Eval("TotalDiscount","{0:N0}") %> Tk</div>
                                </asp:LinkButton>
                            </div>
                        </div>
                        <div class="col-lg-2 col-md-4">
                            <div class="Fees-section Paid-bg">
                                <asp:LinkButton ID="Paid_LB" title="Click To More Details" runat="server" OnClick="Paid_LB_Click">
                        <div class="tile-description">Paid <i class="fa fa-caret-right"></i></div>
                        <div class="tile-number"><%#Eval("TotalPaid","{0:N0}") %> Tk</div>
                                </asp:LinkButton>
                            </div>
                        </div>
                        <div class="col-lg-2 col-md-4">
                            <div class="Fees-section Unpaid-bg">
                                <asp:LinkButton ID="Unpaid_LB" title="Click To More Details" runat="server" OnClick="Unpaid_LB_Click">
                        <div class="tile-description">Unpaid <i class="fa fa-caret-right"></i></div>
                        <div class="tile-number"><%#Eval("Unpaid","{0:N0}") %> Tk</div>
                                </asp:LinkButton>
                            </div>
                        </div>
                    </div>
                </ItemTemplate>
            </asp:FormView>
            <asp:SqlDataSource ID="Stu_P_SQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT COUNT(DISTINCT StudentID) AS Total_Stu,   
SUM(Amount) AS TotalFee, 
     SUM(LateFeeCountable) AS TotalLateFee, 
     SUM(Total_Discount) AS TotalDiscount, 
     SUM(ISNULL(PaidAmount, 0)) AS TotalPaid,
     SUM(Receivable_Amount) AS Unpaid
FROM Income_PayOrder WHERE (SchoolID = @SchoolID) AND (EducationYearID = @EducationYearID) AND (Is_Active = 1) AND EndDate BETWEEN ISNULL(@From_Date, '1-1-1000') AND ISNULL(@To_Date, '1-1-3000')"
                CancelSelectOnNullParameter="False">
                <SelectParameters>
                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                    <asp:ControlParameter ControlID="Session_DropDownList" Name="EducationYearID" PropertyName="SelectedValue" />
                    <asp:ControlParameter Name="From_Date" ControlID="From_Date_TextBox" PropertyName="Text" />
                    <asp:ControlParameter ControlID="To_Date_TextBox" Name="To_Date" PropertyName="Text" />
                </SelectParameters>
            </asp:SqlDataSource>

            <div id="IS_Class">
                <div class="grid-title Class-tle-bg">
                    CLASS BASED PAY ORDER SUMMARY
                </div>
                <div class="table-responsive">
                    <asp:GridView ID="ClassGridView" runat="server" AllowSorting="True" AutoGenerateColumns="False" CssClass="mGrid" DataSourceID="ClassSQL" DataKeyNames="ClassID">
                        <Columns>
                            <asp:TemplateField HeaderText="Class" SortExpression="Class">
                                <ItemTemplate>
                                    <asp:LinkButton ID="Class_LB" title="Click To More Details" CssClass="scrollToBottom" runat="server" CausesValidation="False" Text='<%#Eval("Class") %>' CommandArgument='<%#Eval("Class") %>' CommandName="Select" OnCommand="Class_LB_Command" />
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Student" SortExpression="Total_Stu">
                                <ItemTemplate>
                                    <asp:LinkButton ID="GV_Student_LB" title="Click To More Details" runat="server" Text='<%#Eval("Total_Stu") %>' CommandName='Session_Stu_Report.aspx' CommandArgument='<%#Eval("ClassID") %>' OnCommand="Class_Command" />
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Fee" SortExpression="TotalFee">
                                <ItemTemplate>
                                    <asp:LinkButton ID="GV_Fee_LB" title="Click To More Details" runat="server" Text='<%#Eval("TotalFee") %>' CommandName='Session_Stu_Report.aspx' CommandArgument='<%#Eval("ClassID") %>' OnCommand="Class_Command" />
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:BoundField DataField="TotalLateFee" HeaderText="Late Fee" ReadOnly="True" SortExpression="TotalLateFee" />
                            <asp:TemplateField HeaderText="Concession" SortExpression="TotalDiscount">
                                <ItemTemplate>
                                    <asp:LinkButton ID="GV_Concession_LB" title="Click To More Details" runat="server" Text='<%#Eval("TotalDiscount") %>' CommandName='Concession_Report.aspx' CommandArgument='<%#Eval("ClassID") %>' OnCommand="Class_Command" />
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Paid" SortExpression="TotalPaid">
                                <ItemTemplate>
                                    <asp:LinkButton ID="GV_Paid_LB" title="Click To More Details" runat="server" Text='<%#Eval("TotalPaid") %>' CommandName='Session_Paid_Report.aspx' CommandArgument='<%#Eval("ClassID") %>' OnCommand="Class_Command" />
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Unpaid" SortExpression="Unpaid">
                                <ItemTemplate>
                                    <asp:LinkButton ID="GV_Unpaid_LB" title="Click To More Details" runat="server" Text='<%#Eval("Unpaid") %>' CommandName='Session_Due_Report.aspx' CommandArgument='<%#Eval("ClassID") %>' OnCommand="Class_Command" />
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                        <SelectedRowStyle CssClass="Selected" />
                    </asp:GridView>
                    <asp:SqlDataSource ID="ClassSQL" runat="server" CancelSelectOnNullParameter="False" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT        Income_PayOrder.ClassID, CreateClass.Class, COUNT(DISTINCT Income_PayOrder.StudentID) AS Total_Stu, SUM(Income_PayOrder.Amount) AS TotalFee, SUM(Income_PayOrder.LateFeeCountable) 
                         AS TotalLateFee, SUM(Income_PayOrder.Total_Discount) AS TotalDiscount, SUM(ISNULL(Income_PayOrder.PaidAmount, 0)) AS TotalPaid, 
                         SUM(Income_PayOrder.Receivable_Amount) AS Unpaid
FROM            Income_PayOrder INNER JOIN
                         CreateClass ON Income_PayOrder.ClassID = CreateClass.ClassID
WHERE        (Income_PayOrder.SchoolID = @SchoolID) AND (Income_PayOrder.EducationYearID = @EducationYearID) AND (Income_PayOrder.Is_Active = 1) AND (Income_PayOrder.EndDate BETWEEN ISNULL(@From_Date, 
                         '1-1-1000') AND ISNULL(@To_Date, '1-1-3000'))
GROUP BY Income_PayOrder.ClassID, CreateClass.Class
ORDER BY Income_PayOrder.ClassID">
                        <SelectParameters>
                            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                            <asp:ControlParameter ControlID="Session_DropDownList" Name="EducationYearID" PropertyName="SelectedValue" />
                            <asp:ControlParameter ControlID="From_Date_TextBox" Name="From_Date" PropertyName="Text" />
                            <asp:ControlParameter ControlID="To_Date_TextBox" Name="To_Date" PropertyName="Text" />
                        </SelectParameters>
                    </asp:SqlDataSource>
                </div>
            </div>

            <div id="IS_Role" class="mt-3">
                <div class="grid-title Role-tle-bg">
                    PAY ORDER SUMMARY OF CLASS:
               <asp:Label ID="Class_Name_Label" runat="server"></asp:Label>
                </div>
                <div class="table-responsive">
                    <asp:GridView ID="RoleGridView" runat="server" AllowSorting="True" AutoGenerateColumns="False" CssClass="mGrid" DataSourceID="RoleSQL" DataKeyNames="RoleID">
                        <Columns>
                            <asp:TemplateField HeaderText="Role" SortExpression="Role">
                                <ItemTemplate>
                                    <asp:LinkButton ID="Role_LB" title="Click To More Details" CssClass="scrollToBottom" runat="server" CausesValidation="False" Text='<%#Eval("Role") %>' CommandArgument='<%#Eval("Role") %>' CommandName="Select" OnCommand="Role_LB_Command" />
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Student" SortExpression="Total_Stu">
                                <ItemTemplate>
                                    <asp:LinkButton ID="LB1" title="Click To More Details" runat="server" Text='<%#Eval("Total_Stu") %>' CommandName='Session_Stu_Report.aspx' CommandArgument='<%#Eval("RoleID") %>' OnCommand="Role_Command" />
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Fee" SortExpression="TotalFee">
                                <ItemTemplate>
                                    <asp:LinkButton ID="LB2" title="Click To More Details" runat="server" Text='<%#Eval("TotalFee") %>' CommandName='Session_Stu_Report.aspx' CommandArgument='<%#Eval("RoleID") %>' OnCommand="Role_Command" />
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:BoundField DataField="TotalLateFee" HeaderText="Late Fee" ReadOnly="True" SortExpression="TotalLateFee" />
                            <asp:TemplateField HeaderText="Concession" SortExpression="TotalDiscount">
                                <ItemTemplate>
                                    <asp:LinkButton ID="LB3" title="Click To More Details" runat="server" Text='<%#Eval("TotalDiscount") %>' CommandName='Concession_Report.aspx' CommandArgument='<%#Eval("RoleID") %>' OnCommand="Role_Command" />
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Paid" SortExpression="TotalPaid">
                                <ItemTemplate>
                                    <asp:LinkButton ID="LB4" title="Click To More Details" runat="server" Text='<%#Eval("TotalPaid") %>' CommandName='Session_Paid_Report.aspx' CommandArgument='<%#Eval("RoleID") %>' OnCommand="Role_Command" />
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Unpaid" SortExpression="Unpaid">
                                <ItemTemplate>
                                    <asp:LinkButton ID="LB5" title="Click To More Details" runat="server" Text='<%#Eval("Unpaid") %>' CommandName='Session_Due_Report.aspx' CommandArgument='<%#Eval("RoleID") %>' OnCommand="Role_Command" />
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                        <SelectedRowStyle CssClass="Selected" />
                    </asp:GridView>
                    <asp:SqlDataSource ID="RoleSQL" runat="server" CancelSelectOnNullParameter="False" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT        Income_PayOrder.RoleID, Income_Roles.Role, COUNT(DISTINCT Income_PayOrder.StudentID) AS Total_Stu, SUM(Income_PayOrder.Amount) AS TotalFee, SUM(Income_PayOrder.LateFeeCountable) 
                         AS TotalLateFee, SUM(Income_PayOrder.Total_Discount) AS TotalDiscount, SUM(ISNULL(Income_PayOrder.PaidAmount, 0)) AS TotalPaid, 
                         SUM(Income_PayOrder.Receivable_Amount) AS Unpaid
FROM            Income_PayOrder INNER JOIN
                         Income_Roles ON Income_PayOrder.RoleID = Income_Roles.RoleID
WHERE        (Income_PayOrder.SchoolID = @SchoolID) AND (Income_PayOrder.EducationYearID = @EducationYearID) AND (Income_PayOrder.Is_Active = 1) AND (Income_PayOrder.EndDate BETWEEN ISNULL(@From_Date, 
                         '1-1-1000') AND ISNULL(@To_Date, '1-1-3000')) AND (Income_PayOrder.ClassID = @ClassID)
GROUP BY Income_PayOrder.RoleID, Income_Roles.Role
ORDER BY Income_PayOrder.RoleID">
                        <SelectParameters>
                            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                            <asp:ControlParameter ControlID="Session_DropDownList" Name="EducationYearID" PropertyName="SelectedValue" />
                            <asp:ControlParameter ControlID="From_Date_TextBox" Name="From_Date" PropertyName="Text" />
                            <asp:ControlParameter ControlID="To_Date_TextBox" Name="To_Date" PropertyName="Text" />
                            <asp:ControlParameter ControlID="ClassGridView" Name="ClassID" PropertyName="SelectedValue" />
                        </SelectParameters>
                    </asp:SqlDataSource>
                </div>
            </div>

            <div id="IS_Pfor" class="mt-3">
                <div class="grid-title For-tle-bg">
                    PAY ORDER SUMMARY OF:
              <asp:Label ID="Role_Label" runat="server"></asp:Label>
                </div>
                <div class="table-responsive">
                    <asp:GridView ID="ForGridView" runat="server" AutoGenerateColumns="False" CssClass="mGrid" DataSourceID="ForSQL">
                        <Columns>
                            <asp:BoundField DataField="PayFor" HeaderText="Pay for" SortExpression="PayFor" />
                            <asp:BoundField DataField="Total_Stu" HeaderText="Student" ReadOnly="True" SortExpression="Total_Stu" />
                            <asp:BoundField DataField="TotalFee" HeaderText="Fee" ReadOnly="True" SortExpression="TotalFee" />
                            <asp:BoundField DataField="TotalLateFee" HeaderText="Late Fee" ReadOnly="True" SortExpression="TotalLateFee" />
                            <asp:BoundField DataField="TotalDiscount" HeaderText="Concession" ReadOnly="True" SortExpression="TotalDiscount" />
                            <asp:BoundField DataField="TotalPaid" HeaderText="Paid" ReadOnly="True" SortExpression="TotalPaid" />
                            <asp:BoundField DataField="Unpaid" HeaderText="Unpaid" ReadOnly="True" SortExpression="Unpaid" />
                        </Columns>
                    </asp:GridView>
                    <asp:SqlDataSource ID="ForSQL" runat="server" CancelSelectOnNullParameter="False" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT PayFor, COUNT(DISTINCT StudentID) AS Total_Stu, SUM(Amount) AS TotalFee, SUM(LateFeeCountable) AS TotalLateFee, SUM(Total_Discount) AS TotalDiscount, SUM(ISNULL(PaidAmount, 0)) AS TotalPaid, SUM(Receivable_Amount) AS Unpaid FROM Income_PayOrder WHERE (SchoolID = @SchoolID) AND (EducationYearID = @EducationYearID) AND (Is_Active = 1) AND (EndDate BETWEEN ISNULL(@From_Date, '1-1-1000') AND ISNULL(@To_Date, '1-1-3000')) AND (ClassID = @ClassID) AND (RoleID = @RoleID) GROUP BY PayFor ORDER BY MAX(EndDate)">
                        <SelectParameters>
                            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                            <asp:ControlParameter ControlID="Session_DropDownList" Name="EducationYearID" PropertyName="SelectedValue" />
                            <asp:ControlParameter ControlID="From_Date_TextBox" Name="From_Date" PropertyName="Text" />
                            <asp:ControlParameter ControlID="To_Date_TextBox" Name="To_Date" PropertyName="Text" />
                            <asp:ControlParameter ControlID="ClassGridView" Name="ClassID" PropertyName="SelectedValue" />
                            <asp:ControlParameter ControlID="RoleGridView" Name="RoleID" PropertyName="SelectedValue" />
                        </SelectParameters>
                    </asp:SqlDataSource>
                </div>
            </div>
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


    <script>
        $(function () {
            // grid is empty
            if (!$('[id*=ClassGridView] tr').length) {
                $('#IS_Class').hide();
            }

            if (!$('[id*=RoleGridView] tr').length) {
                $('#IS_Role').hide();
            }

            if (!$('[id*=ForGridView] tr').length) {
                $('#IS_Pfor').hide();
            }

            //scroll To Bottom
            $('.scrollToBottom').bind("click", function () {
                $('html, body').animate({ scrollTop: $(document).height() }, 1200);
            });


            $('.datepicker').datepicker({
                format: 'dd M yyyy',
                todayBtn: "linked",
                todayHighlight: true,
                autoclose: true
            });

            //get date in label
            var from = $("[id*=From_Date_TextBox]").val();
            var To = $("[id*=To_Date_TextBox]").val();

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

            if ($(".Date").text() != "") {
                $(".sort-Date").show();
            }
        });

        //For Update pannel
        var prm = Sys.WebForms.PageRequestManager.getInstance();
        prm.add_endRequest(function () {
            // grid is empty
            if (!$('[id*=ClassGridView] tr').length) {
                $('#IS_Class').hide();
            }

            if (!$('[id*=RoleGridView] tr').length) {
                $('#IS_Role').hide();
            }

            if (!$('[id*=ForGridView] tr').length) {
                $('#IS_Pfor').hide();
            }

            //scroll To Bottom
            $('.scrollToBottom').bind("click", function () {
                $('html, body').animate({ scrollTop: $(document).height() }, 1200);
            });


            $('.datepicker').datepicker({
                format: 'dd M yyyy',
                todayBtn: "linked",
                todayHighlight: true,
                autoclose: true
            });

            //get date in label
            var from = $("[id*=From_Date_TextBox]").val();
            var To = $("[id*=To_Date_TextBox]").val();

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

            if ($(".Date").text() != "") {
                $(".sort-Date").show();
            }
        });

        function isNumberKey(a) { a = a.which ? a.which : event.keyCode; return 46 != a && 31 < a && (48 > a || 57 < a) ? !1 : !0 };
    </script>
</asp:Content>
