<%@ Page Title="Money Receipt" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Money_Receipt_By_Date.aspx.cs" Inherits="EDUCATION.COM.Accounts.Payment.Money_Receipt_By_Date" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="CSS/Money_Receipt.css?v=.1" rel="stylesheet" />
    <!--for printing options-->
    <style type="text/css" media="print" id="print-content"></style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <a class="d-print-none" href="Payment_Collection_By_Date.aspx"><< Back To payment Page</a>

    <asp:FormView ID="StudentInfoFormView" runat="server" DataSourceID="StudentInfoSQL" DataKeyNames="SMSPhoneNo,StudentID,ID" Width="100%">
        <ItemTemplate>
            <div class="SInfo">
                (ID:<%# Eval("ID") %>)
                <asp:Label ID="StudentsNameLabel" runat="server" Text='<%# Eval("StudentsName") %>' />
                <br />
                Class: <%# Eval("Class") %> <%# Eval("Section",", Section: {0}") %> <%# Eval("RollNo",", Roll No: {0}") %>
            </div>
        </ItemTemplate>
    </asp:FormView>
    <asp:SqlDataSource ID="StudentInfoSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT Student.ID, Student.SMSPhoneNo, Student.StudentsName, CreateClass.Class, Student.StudentID, CreateSection.Section, StudentsClass.RollNo FROM StudentsClass INNER JOIN CreateClass ON StudentsClass.ClassID = CreateClass.ClassID INNER JOIN Student ON StudentsClass.StudentID = Student.StudentID LEFT OUTER JOIN CreateSection ON StudentsClass.SectionID = CreateSection.SectionID WHERE (Student.SchoolID = @SchoolID) AND (Student.ID = @ID) AND (StudentsClass.Class_Status IS NULL)">
        <SelectParameters>
            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
            <asp:Parameter Name="ID" Type="String" />
        </SelectParameters>
    </asp:SqlDataSource>

    <asp:FormView ID="ReceiptFormView" runat="server" DataSourceID="MoneyRSQL" Width="100%" DataKeyNames="TotalAmount">
        <ItemTemplate>
            <div class="SInfo">
                Receipt No:
            <asp:Label ID="MoneyReceiptIDLabel" runat="server" Text='<%# Eval("MoneyReceipt_SN") %>' />
                <br />
                Paid Date: 
            <asp:Label ID="PaidDateLabel" runat="server" Text='<%# Eval("PaidDate","{0:d-MMM-yy}") %>' />
                <br />
                <%#Eval("AccountName", "Payment Method: {0}").ToString()%>
            </div>
        </ItemTemplate>
    </asp:FormView>
    <asp:SqlDataSource ID="MoneyRSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
        SelectCommand="SELECT DISTINCT Income_MoneyReceipt.PaidDate, Income_MoneyReceipt.MoneyReceipt_SN, Income_MoneyReceipt.TotalAmount, Account.AccountName FROM Account INNER JOIN                       Income_PaymentRecord ON Account.AccountID = Income_PaymentRecord.AccountID RIGHT OUTER JOIN                       Income_MoneyReceipt ON Income_PaymentRecord.MoneyReceiptID = Income_MoneyReceipt.MoneyReceiptID WHERE (Income_MoneyReceipt.SchoolID = @SchoolID) AND (Income_MoneyReceipt.MoneyReceiptID = @MoneyReceiptID)">
        <SelectParameters>
            <asp:Parameter Name="MoneyReceiptID" />
            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
        </SelectParameters>
    </asp:SqlDataSource>

    <asp:GridView ID="PaidDetailsGridView" runat="server" AutoGenerateColumns="False" DataSourceID="PaidDetailsSQL" CssClass="mGrid" ShowFooter="True" Font-Bold="False" RowStyle-CssClass="Rows" DataKeyNames="Role,PayFor">
        <Columns>
            <asp:BoundField DataField="PayFor" HeaderText="Pay For" />
            <asp:TemplateField HeaderText="Fee">
                <FooterTemplate>
                    Total:
                </FooterTemplate>
                <ItemTemplate>
                    <asp:Label ID="RoleLabel" runat="server" Text='<%# Bind("Role")%>' />
                    : 
               <asp:Label ID="Label2" runat="server" Text='<%# Bind("Amount") %>' />
                    TK
                </ItemTemplate>
                <FooterStyle HorizontalAlign="Right" />
                <ItemStyle HorizontalAlign="Right" />
            </asp:TemplateField>
            <asp:TemplateField HeaderText="Paid">
                <ItemTemplate>
                    <asp:Label ID="PaidAmountLabel" runat="server" Text='<%# Bind("PaidAmount") %>'></asp:Label>
                    TK
                </ItemTemplate>
                <FooterTemplate>
                    <span id="PGTLabel"></span>
                    TK
                    <asp:HiddenField ID="PaidHF" runat="server" />
                </FooterTemplate>
            </asp:TemplateField>
            <asp:TemplateField HeaderText="Due">
                <ItemTemplate>
                    <asp:Label ID="Label1" runat="server" Text='<%# Bind("Due") %>'></asp:Label>
                    TK
                </ItemTemplate>
            </asp:TemplateField>
        </Columns>
        <FooterStyle CssClass="GVfooter" />

        <RowStyle CssClass="Rows"></RowStyle>
    </asp:GridView>
    <asp:SqlDataSource ID="PaidDetailsSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
        SelectCommand="SELECT Income_PaymentRecord.MoneyReceiptID, Income_Roles.Role, Income_PaymentRecord.PayFor + ' (' + Education_Year.EducationYear + ')' AS PayFor, Income_PaymentRecord.PaidAmount, CASE WHEN Income_PayOrder.EndDate &lt; GETDATE() - 1 THEN ISNULL(Income_PayOrder.Amount , 0) + ISNULL(Income_PayOrder.LateFee , 0) - ISNULL(Income_PayOrder.Discount , 0) - ISNULL(Income_PayOrder.PaidAmount , 0) - ISNULL(Income_PayOrder.LateFee_Discount , 0) ELSE ISNULL(Income_PayOrder.Amount , 0) - ISNULL(Income_PayOrder.Discount , 0) - ISNULL(Income_PayOrder.PaidAmount , 0) END AS Due, Income_PaymentRecord.PaidDate, Income_PayOrder.Amount FROM Income_PaymentRecord INNER JOIN Income_Roles ON Income_PaymentRecord.RoleID = Income_Roles.RoleID INNER JOIN Income_PayOrder ON Income_PaymentRecord.PayOrderID = Income_PayOrder.PayOrderID INNER JOIN Education_Year ON Income_PaymentRecord.EducationYearID = Education_Year.EducationYearID WHERE (Income_PaymentRecord.MoneyReceiptID = @MoneyReceiptID) AND (Income_PaymentRecord.SchoolID = @SchoolID) ORDER BY Income_PayOrder.EndDate">
        <SelectParameters>
            <asp:Parameter Name="MoneyReceiptID" />
            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
        </SelectParameters>
    </asp:SqlDataSource>

     <p class="text-right" id="amount-in-word"></p>

    <div id="Due_Show">
        <div class="P_Dues">Current Due</div>
        <asp:GridView ID="DueDetailsGridView" runat="server" AutoGenerateColumns="False" DataSourceID="ID_DueDetailsODS" CssClass="mGrid" ShowFooter="True" RowStyle-CssClass="Rows">
            <Columns>
                <asp:TemplateField HeaderText="Pay For">
                    <ItemTemplate>
                        <%# Eval("PayFor")%>
                        (<%# Eval("EducationYear")%>)
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Fee">
                    <ItemTemplate>
                        <asp:Label ID="RoleLabel" runat="server" Text='<%# Bind("Role")%>' />
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:BoundField DataField="EndDate" HeaderText="End Date" SortExpression="EndDate" DataFormatString="{0:d MMM yyyy}" />
              <asp:TemplateField HeaderText="Paid">
                    <ItemTemplate>
                        <%# Eval("PaidAmount") %>
                        TK
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Due">
                    <ItemTemplate>
                        <asp:Label ID="DueLabel" runat="server" Text='<%# Bind("Due") %>'></asp:Label>
                        TK
                    </ItemTemplate>
                    <FooterTemplate>
                        <span id="DGTLabel"></span>
                        TK
                    </FooterTemplate>
                </asp:TemplateField>
            </Columns>
            <FooterStyle CssClass="GVfooter" />
            <RowStyle CssClass="Rows" />
        </asp:GridView>
        <asp:ObjectDataSource ID="ID_DueDetailsODS" runat="server" OldValuesParameterFormatString="original_{0}" SelectMethod="GetData" TypeName="EDUCATION.COM.PaymentDataSetTableAdapters.DueDetailsTableAdapter">
            <SelectParameters>
                <asp:Parameter DefaultValue="" Name="ID" Type="String" />
                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" Type="Int32" />
                <asp:Parameter DefaultValue="%" Name="RoleID" Type="String" />
            </SelectParameters>
        </asp:ObjectDataSource>
    </div>

    <asp:FormView ID="RByFormView" runat="server" DataSourceID="ReceivedBySQL" Width="100%">
        <ItemTemplate>
            <div class="RecvBy">
                (© Sikkhaloy.com) Received By:
                <asp:Label ID="NameLabel" runat="server" Text='<%# Bind("Name") %>' />
            </div>
        </ItemTemplate>
    </asp:FormView>
    <asp:SqlDataSource ID="ReceivedBySQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT Admin.FirstName + ' ' + Admin.LastName AS Name FROM Admin INNER JOIN Income_MoneyReceipt ON Admin.RegistrationID = Income_MoneyReceipt.RegistrationID WHERE (Income_MoneyReceipt.MoneyReceiptID = @MoneyReceiptID) AND (Income_MoneyReceipt.SchoolID = @SchoolID)">
        <SelectParameters>
            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
            <asp:Parameter Name="MoneyReceiptID" />
        </SelectParameters>
    </asp:SqlDataSource>

    <asp:FormView ID="SMSFormView" CssClass="NoPrint" runat="server" DataKeyNames="SMSID" DataSourceID="SMSSQL" Width="100%">
        <ItemTemplate>
            <div class="alert alert-info">Remaining SMS: <%# Eval("SMS_Balance") %></div>
        </ItemTemplate>
    </asp:FormView>
    <asp:SqlDataSource ID="SMSSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT * FROM [SMS] WHERE ([SchoolID] = @SchoolID)">
        <SelectParameters>
            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
        </SelectParameters>
    </asp:SqlDataSource>
    <asp:SqlDataSource ID="SMS_OtherInfoSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
        InsertCommand="INSERT INTO SMS_OtherInfo(SMS_Send_ID, SchoolID, StudentID, TeacherID, EducationYearID) VALUES (@SMS_Send_ID, @SchoolID, @StudentID, @TeacherID, @EducationYearID)" SelectCommand="SELECT * FROM [SMS_OtherInfo]">
        <InsertParameters>
            <asp:Parameter Name="SMS_Send_ID" DbType="Guid" />
            <asp:Parameter Name="SchoolID" />
            <asp:Parameter Name="StudentID" />
            <asp:Parameter Name="TeacherID" />
            <asp:Parameter Name="EducationYearID" />
        </InsertParameters>
    </asp:SqlDataSource>

    <div class="d-print-none my-4 card">
        <div class="card-header">
            <h4 class="card-title mb-0">
                <i class="fa fa-print"></i>
                Print Options
            </h4>
        </div>
        <div class="card-body">
            <div>
                <input id="checkboxInstitution" type="checkbox" />
                <label for="checkboxInstitution">Hide Institution Name</label>
            </div>
            <div class="my-3">
                <input id="checkboxDueDetails" type="checkbox" />
                <label for="checkboxDueDetails">Hide Current Due</label>
            </div>
            <div class="form-group ml-2">
                <label for="inputTopSpace">Page Space From Top (px)</label>
                <input id="inputTopSpace" min="0" type="number" class="form-control" />
            </div>
            <div class="form-group ml-2">
                <label for="inputFontSize">Font Size (px)</label>
                <input id="inputFontSize" min="10" max="20" type="number" class="form-control" />
            </div>
        </div>
        <div class="card-footer">
            <input id="PrintButton" type="button" value="Print" onclick="window.print();" class="btn btn-info" />
        </div>
    </div>

    <div class="form-group d-print-none">
        <asp:CheckBox ID="RoleCheckBox" runat="server" Text="Send Payment Roles" />
        <asp:CheckBox ID="CurrentDueCheckBox" runat="server" Text="Send Current Due" /><br />
        <asp:Button ID="SMSButton" runat="server" Text="Send SMS" CssClass="btn btn-primary" OnClick="SMSButton_Click" />
        <asp:Label ID="ErrorLabel" runat="server" CssClass="EroorSummer"></asp:Label>
    </div>

    <!--Amount in word js-->
    <script src="../../JS/amount-in-word.js"></script>
    <script>
        $(function () {
            //Paid Grand Total
            var PaidTotal = 0;
            $("[id*=PaidAmountLabel]").each(function () { PaidTotal = PaidTotal + parseFloat($(this).text()) });
            $("#PGTLabel").text(PaidTotal);

            const inWord = number2text(PaidTotal);
            document.getElementById("amount-in-word").textContent = inWord;

            //Due Grand Total
            var DueTotal = 0;
            $("[id*=DueLabel]").each(function () { DueTotal = DueTotal + parseFloat($(this).text()) });
            $("#DGTLabel").text(`Total: ${DueTotal}`);

            //Is Grid view is empty
            if ($('[id*=DueDetailsGridView] tr').length) {
                $(".P_Dues").show();
            }
        });


        //print options
        let printingOptions = {
            isInstitutionName: false,
            topSpace: 0,
            fontSize: 11
        };

        const stores = {
            set: function () {
                localStorage.setItem('receipt-printing', JSON.stringify(printingOptions));
            },
            get: function () {
                const data = localStorage.getItem("receipt-printing");

                if (data) printingOptions = JSON.parse(data);
            }
        }

        const printContent = document.getElementById("print-content");
        const checkboxInstitution = document.getElementById("checkboxInstitution");
        const header = document.getElementById("header");
        const institutionInfo = document.querySelector(".bg-main");

        const inputTopSpace = document.getElementById("inputTopSpace");
        const checkboxDueDetails = document.getElementById("checkboxDueDetails");
        const currentDuesContainer = document.getElementById("Due_Show");

        const inputFontSize = document.getElementById("inputFontSize");

        //institution name show/hide checkbox
        checkboxInstitution.addEventListener("change", function () {
            printingOptions.isInstitutionName = this.checked;

            stores.set();
            bindPrintOption();
        });

        //input top space
        inputTopSpace.addEventListener("input", function () {
            printingOptions.topSpace = +this.value

            stores.set();
            bindPrintOption();
        });

        //input font size
        inputFontSize.addEventListener("input", function () {
            const min = +this.min;
            const max = +this.max;
            const size = +this.value;

            if (min < size) {
                printingOptions.fontSize = size;

                stores.set();
                bindPrintOption();
            }
        });

        //due details show/hide checkbox
        checkboxDueDetails.addEventListener("change", function () {
            currentDuesContainer.style.display = this.checked ? "none" : "block";
        });

        //bind selected options
        function bindPrintOption() {
            stores.get();

            //institution show hide
            checkboxInstitution.checked = printingOptions.isInstitutionName;
            printingOptions.isInstitutionName ? institutionInfo.classList.add("d-print-none") : institutionInfo.classList.remove("d-print-none");

            //space from top
            inputTopSpace.value = printingOptions.topSpace;

            //font size
            inputFontSize.value = printingOptions.fontSize;
            printContent.textContent = `
                 #InstitutionName { font-size: ${printingOptions.fontSize + 4}px}
                 .SInfo {font-size: ${printingOptions.fontSize + 1}px}
                 #header { padding-top: ${printingOptions.topSpace}px}
                .InsInfo p { font-size: ${printingOptions.fontSize + 1}px}
                .mGrid th { font-size: ${printingOptions.fontSize}px}
                .mGrid td { font-size: ${printingOptions.fontSize}px}`;
        }

        bindPrintOption();


        //disable after submit SMS
        function Disable_Submited() { document.getElementById("<%=SMSButton.ClientID %>").disabled = true; }
        window.onbeforeunload = Disable_Submited;
    </script>
</asp:Content>
