<%@ Page Title="Money Receipt Check" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Payment_Check_by_Money_Receipt.aspx.cs" Inherits="EDUCATION.COM.ACCOUNTS.AccountsAnalysis.Payment_Check_by_Money_Receipt" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .SInfo { text-align: center; color: #333; font-size: 13px; font-weight: bold; margin-bottom: 6px; margin-top:10px;}
        .RecvBy { text-align: center; color: #333; font-size: 11px; margin-top: 5px; }
        @page { margin: 0 193px; }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <div class="Contain">
        <h3 class="NoPrint">Find Paid Details By Money Receipt No.</h3>

        <div class="form-inline d-print-none">
            <div class="form-group">
                <asp:TextBox ID="ReceiptNoTextBox" placeholder="Money Receipt No." runat="server" CssClass="form-control" onkeypress="return isNumberKey(event)"></asp:TextBox>
                <asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" ControlToValidate="ReceiptNoTextBox" CssClass="EroorSummer" ErrorMessage="*" ValidationGroup="F"></asp:RequiredFieldValidator>
            </div>
            <div class="form-group">
                <asp:Button ID="FindButton" runat="server" CssClass="btn btn-primary" Text="Find" ValidationGroup="F" />
            </div>
        </div>

        <asp:FormView ID="StudentInfoFormView" runat="server" DataSourceID="StudentInfoSQL" Width="100%">
            <ItemTemplate>
                <div class="SInfo">
                    (ID:<%# Eval("ID") %>) <%# Eval("StudentsName") %>
                    <br />
                    Class:<%# Eval("Class") %></div>
            </ItemTemplate>
        </asp:FormView>
        <asp:SqlDataSource ID="StudentInfoSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT CreateClass.Class, Student.StudentsName, Student.ID, Student.SMSPhoneNo FROM StudentsClass INNER JOIN Student ON StudentsClass.StudentID = Student.StudentID INNER JOIN Income_MoneyReceipt ON StudentsClass.StudentClassID = Income_MoneyReceipt.StudentClassID LEFT OUTER JOIN CreateClass ON StudentsClass.ClassID = CreateClass.ClassID WHERE (Income_MoneyReceipt.MoneyReceipt_SN = @MoneyReceipt_SN) AND (Income_MoneyReceipt.SchoolID = @SchoolID) AND (StudentsClass.EducationYearID = @EducationYearID)">
            <SelectParameters>
                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                <asp:ControlParameter ControlID="ReceiptNoTextBox" Name="MoneyReceipt_SN" PropertyName="Text" />
            </SelectParameters>
        </asp:SqlDataSource>

        <asp:FormView ID="ReceiptFormView" runat="server" DataSourceID="MoneyRSQL" Width="100%">
            <ItemTemplate>
                <div class="SInfo">
                    Receipt No:
            <asp:Label ID="MoneyReceiptIDLabel" runat="server" Text='<%# Eval("MoneyReceipt_SN") %>' />
                    <br />
                    Paid Date: 
            <asp:Label ID="PaidDateLabel" runat="server" Text='<%# Eval("PaidDate","{0:d-MMM-yy (hh:mm tt)}") %>' />
                </div>
            </ItemTemplate>
        </asp:FormView>
        <asp:SqlDataSource ID="MoneyRSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
            SelectCommand="SELECT PaidDate, MoneyReceipt_SN FROM Income_MoneyReceipt WHERE (SchoolID = @SchoolID) AND (MoneyReceipt_SN = @MoneyReceipt_SN)">
            <SelectParameters>
                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                <asp:ControlParameter ControlID="ReceiptNoTextBox" Name="MoneyReceipt_SN" PropertyName="Text" />
            </SelectParameters>
        </asp:SqlDataSource>

        <hr class="Hide" />

        <asp:GridView ID="PaidDetailsGridView" runat="server" AutoGenerateColumns="False" DataSourceID="PaidRecordsSQL" CssClass="mGrid" ShowFooter="True" RowStyle-CssClass="Rows">
            <Columns>
                <asp:BoundField DataField="PayFor" HeaderText="Pay For" />
                <asp:TemplateField HeaderText="Role">
                    <ItemTemplate>
                        <asp:Label ID="RoleLabel" runat="server" Text='<%# Eval("Role") %>'></asp:Label>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Paid" SortExpression="PaidAmount">
                    <ItemTemplate>
                        <asp:Label ID="PaidAmountLabel" runat="server" Text='<%# Eval("PaidAmount") %>'></asp:Label>
                    </ItemTemplate>
                    <FooterTemplate>
                        <label id="PGTLabel"></label>
                    </FooterTemplate>
                </asp:TemplateField>
            </Columns>
            <FooterStyle CssClass="GVfooter" />
            <RowStyle CssClass="Rows"></RowStyle>
        </asp:GridView>
        <asp:SqlDataSource ID="PaidRecordsSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT Income_PaymentRecord.PaidAmount, Income_PaymentRecord.PayFor + ' (' + Education_Year.EducationYear + ')' AS PayFor, Income_PaymentRecord.PaidDate, Income_Roles.Role, Income_MoneyReceipt.MoneyReceipt_SN FROM Income_PaymentRecord INNER JOIN Income_Roles ON Income_PaymentRecord.RoleID = Income_Roles.RoleID INNER JOIN Income_MoneyReceipt ON Income_PaymentRecord.MoneyReceiptID = Income_MoneyReceipt.MoneyReceiptID INNER JOIN Education_Year ON Income_PaymentRecord.EducationYearID = Education_Year.EducationYearID WHERE (Income_PaymentRecord.SchoolID = @SchoolID) AND (Income_MoneyReceipt.MoneyReceipt_SN = @MoneyReceipt_SN)">
            <SelectParameters>
                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                <asp:ControlParameter ControlID="ReceiptNoTextBox" Name="MoneyReceipt_SN" PropertyName="Text" />
            </SelectParameters>
        </asp:SqlDataSource>

        <br />
        <asp:FormView ID="RByFormView" runat="server" CssClass="Hide" DataSourceID="ReceivedBySQL" Width="100%">
            <ItemTemplate>
                <div class="RecvBy">
                    Received By:
               <asp:Label ID="NameLabel" runat="server" Text='<%# Eval("Name") %>' />
                    (© Sikkhaloy.com)
                </div>
            </ItemTemplate>
        </asp:FormView>
        <asp:SqlDataSource ID="ReceivedBySQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT Admin.FirstName + ' ' + Admin.LastName AS Name FROM Admin INNER JOIN Income_MoneyReceipt ON Admin.RegistrationID = Income_MoneyReceipt.RegistrationID WHERE (Income_MoneyReceipt.MoneyReceipt_SN = @MoneyReceipt_SN) AND (Income_MoneyReceipt.SchoolID = @SchoolID)">
            <SelectParameters>
                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                <asp:ControlParameter ControlID="ReceiptNoTextBox" Name="MoneyReceipt_SN" PropertyName="Text" />
            </SelectParameters>
        </asp:SqlDataSource>


        <input type="button" onclick="window.print();" value="Print" class="btn btn-primary Hide d-print-none" />
    </div>

    <script>
        function isNumberKey(a) { a = a.which ? a.which : event.keyCode; return 46 != a && 31 < a && (48 > a || 57 < a) ? !1 : !0 };
        $(function () {
            //Paid Grand Total
            var PaidTotal = 0;
            $("[id*=PaidAmountLabel]").each(function () { PaidTotal = PaidTotal + parseFloat($(this).text()) });
            $("#PGTLabel").text(PaidTotal);

            //Due Grand Total
            var DueTotal = 0;
            $("[id*=DueLabel]").each(function () { DueTotal = DueTotal + parseFloat($(this).text()) });
            $("#DGTLabel").text(DueTotal);

            //Due GridView is empty
            if (!$('[id*=PaidDetailsGridView] tr').length) {
                $(".Hide").hide();
            }
        });
    </script>

    <asp:UpdateProgress ID="UpdateProgress" runat="server">
        <ProgressTemplate>
            <div id="progress_BG"></div>
            <div id="progress">
                <img src="../../CSS/loading.gif" alt="Loading..." />
                <br />
                <b>Loading...</b>
            </div>
        </ProgressTemplate>
    </asp:UpdateProgress>
</asp:Content>
