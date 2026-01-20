<%@ Page Title="Money Receipt" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Money_Receipt.aspx.cs" Inherits="EDUCATION.COM.Accounts.Payment.Money_Receipt" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="CSS/Money_Receipt.css?v=0.3" rel="stylesheet" />

    <!--add dynamic css for printing-->
    <style type="text/css" media="print" id="print-content"></style>
    
    <style>
        /* Discount column styling */
        .discount-column-hidden {
            display: none !important;
        }
        
        /* Payment Receipt Header */
    .payment-receipt-header {
    text-align: center;
    font-size: 16px;
    font-weight: bold;
    color: #333;
    margin: 0px 0;
    padding: 0px 0;
    border-top: 1px solid #333;
    border-bottom: 1px solid #333;
    letter-spacing: 1px;
}
        
        /* Two Column Layout for Receipt Info */
        .receipt-info-container {
            display: flex;
            justify-content: space-between;
            margin: 15px 0;
            padding: 1px;
            border: 1px solid #ddd;
        }
        
        .receipt-info-left {
            flex: 1;
            padding-right: 15px;
            border-right: 1px solid #ddd;
        }
        
        .receipt-info-right {
            flex: 1;
            padding-left: 15px;
        }
        
        .receipt-info-left p, .receipt-info-right p {
            margin: 5px 0;
            font-size: 10px;
           border-bottom: 1px solid #bbb3b3;
        }
        
        /* Make all text in receipt info dark black */
        .receipt-info-container,
        .receipt-info-container p,
        .receipt-info-container strong,
        .receipt-info-container span,
        .receipt-info-container label,
        .student-id,
        .student-name,
        .student-class,
        .student-section,
        .student-roll {
            color: #000 !important;
        }
        
        .receipt-info-left strong, .receipt-info-right strong {
            font-weight: bold;
        }
        
        @media print {
            .payment-receipt-header {
                font-size: 15px;
            }
            .receipt-info-container {
                border: 1px solid #000;
            }
            .receipt-info-left {
                border-right: 1px solid #000;
            }
            .receipt-info-container,
            .receipt-info-container p,
            .receipt-info-container strong,
            .receipt-info-container span,
            .receipt-info-container label {
                color: #000 !important;
            }
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <a class="d-print-none" href="Payment_Collection.aspx"><< Back To payment Page</a>

    <!-- Payment Receipt Header -->
    <div class="payment-receipt-header">PAYMENT RECEIPT</div>

    <asp:SqlDataSource ID="StudentInfoSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT Student.ID, Student.SMSPhoneNo, Student.StudentsName, CreateClass.Class, Student.StudentID, CreateSection.Section, StudentsClass.RollNo, StudentsClass.Class_Status FROM StudentsClass INNER JOIN CreateClass ON StudentsClass.ClassID = CreateClass.ClassID INNER JOIN Student ON StudentsClass.StudentID = Student.StudentID LEFT OUTER JOIN CreateSection ON StudentsClass.SectionID = CreateSection.SectionID WHERE (Student.SchoolID = @SchoolID) AND (Student.ID = @ID) AND (StudentsClass.Class_Status IS NULL)">
        <SelectParameters>
            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
            <asp:Parameter Name="ID" Type="String" />
        </SelectParameters>
    </asp:SqlDataSource>

    <asp:SqlDataSource ID="MoneyRSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
        SelectCommand="SELECT DISTINCT Income_MoneyReceipt.PaidDate, Income_MoneyReceipt.MoneyReceipt_SN, Income_MoneyReceipt.TotalAmount, Income_MoneyReceipt.MoneyReceiptID, Income_MoneyReceipt.PrintedReceiptNo, Account.AccountName FROM Account INNER JOIN
          Income_PaymentRecord ON Account.AccountID = Income_PaymentRecord.AccountID RIGHT OUTER JOIN
  Income_MoneyReceipt ON Income_PaymentRecord.MoneyReceiptID = Income_MoneyReceipt.MoneyReceiptID WHERE (Income_MoneyReceipt.SchoolID = @SchoolID) AND (Income_MoneyReceipt.MoneyReceiptID = @MoneyReceiptID)"
UpdateCommand="UPDATE Income_MoneyReceipt SET PrintedReceiptNo = @PrintedReceiptNo WHERE MoneyReceiptID = @MoneyReceiptID AND SchoolID = @SchoolID">
<SelectParameters>
   <asp:Parameter Name="MoneyReceiptID" />
            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
        </SelectParameters>
        <UpdateParameters>
    <asp:Parameter Name="PrintedReceiptNo" Type="String" />
    <asp:Parameter Name="MoneyReceiptID" Type="Int32" />
            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" Type="Int32" />
  </UpdateParameters>
    </asp:SqlDataSource>

    <!-- Student and Receipt Information in Two Columns -->
    <asp:FormView ID="StudentInfoFormView" runat="server" DataSourceID="StudentInfoSQL" DataKeyNames="SMSPhoneNo,StudentID,ID" Width="100%">
        <ItemTemplate>
            <div id="studentData" 
                 data-id='<%# Eval("ID") %>' 
                 data-name='<%# Eval("StudentsName") %>' 
                 data-class='<%# Eval("Class") %>'
                 data-roll='<%# Eval("RollNo") %>'
                 style="display:none;">
            </div>
            <%-- Hidden label for C# code behind to access student name --%>
            <asp:Label ID="StudentsNameLabel" runat="server" Text='<%# Eval("StudentsName") %>' style="display:none;"></asp:Label>
        </ItemTemplate>
    </asp:FormView>

    <asp:FormView ID="ReceiptFormView" runat="server" DataSourceID="MoneyRSQL" Width="100%" DataKeyNames="TotalAmount,MoneyReceiptID">
        <ItemTemplate>
            <div class="receipt-info-container">
                <div class="receipt-info-left">
                    <p style="color: #000 !important;"><strong style="color: #000 !important;">(ID:<span class="student-id" style="color: #000 !important;"></span>)</strong> <span class="student-name" style="color: #000 !important;"></span></p>
                    <p style="color: #000 !important;">
                        <strong style="color: #000 !important;">Class:</strong> <span class="student-class" style="color: #000 !important;"></span><strong style="color: #000 !important;"></span><strong style="color: #000 !important;">, Roll:</strong> <span class="student-roll" style="color: #000 !important;"></span>
                    </p>
                    <p style="color: #000 !important;"><strong style="color: #000 !important;">Receipt No:</strong> <span style="color: #000 !important;"><%# Eval("MoneyReceipt_SN") %></span></p>
                </div>
                <div class="receipt-info-right">
                    <p style="color: #000 !important;"><strong style="color: #000 !important;">Paid Date:</strong> <span style="color: #000 !important;"><%# Eval("PaidDate","{0:d-MMM-yy (hh:mm tt)}") %></span></p>
                    <p style="color: #000 !important;"><strong style="color: #000 !important;">Payment Method:</strong> <span style="color: #000 !important;"><%# Eval("AccountName") %></span></p>
                    <p style="color: #000 !important;">
                        <strong style="color: #000 !important;">Printed Receipt No:</strong> 
                        <asp:Label ID="PrintedReceiptNoLabel" runat="server" 
                            Text='<%# string.IsNullOrEmpty(Convert.ToString(Eval("PrintedReceiptNo"))) ? "Not Set" : Eval("PrintedReceiptNo").ToString() %>' 
                            style="color: #000 !important;" />
                    </p>
                </div>
            </div>
            
            <%-- Printed Receipt Number Inline Edit --%>
            <div class="d-flex align-items-center justify-content-center mt-2 d-print-none">
                <strong class="mr-2">Update Printed Receipt No:</strong>
                <asp:TextBox ID="PrintedReceiptNoTextBox" runat="server" 
                    Text='<%# Eval("PrintedReceiptNo") %>'
                    CssClass="form-control form-control-sm d-print-none mr-2" 
                    placeholder="Enter No" 
                    MaxLength="50"
                    style="width: 150px; display: inline-block;"
                    autocomplete="off"></asp:TextBox>
                
                <asp:Button ID="UpdatePrintedReceiptButton" runat="server" 
                    Text="Update" 
                    CssClass="btn btn-sm btn-success d-print-none" 
                    OnClick="UpdatePrintedReceiptButton_Click" 
                    CommandArgument='<%# Eval("MoneyReceiptID") %>'
                    OnClientClick="return confirm('Update printed receipt number?');" />
                
                <asp:Label ID="UpdateMessageLabel" runat="server" 
                    CssClass="ml-2 text-success d-print-none" 
                    style="font-size: 0.9rem;"></asp:Label>
            </div>

            <script type="text/javascript">
                (function() {
                    var studentData = document.getElementById('studentData');
                    if (studentData) {
                        var idEl = document.querySelector('.student-id');
                        var nameEl = document.querySelector('.student-name');
                        var classEl = document.querySelector('.student-class');
                        var sectionEl = document.querySelector('.student-section');
                        var rollEl = document.querySelector('.student-roll');
                        
                        if (idEl) idEl.textContent = studentData.getAttribute('data-id') || '';
                        if (nameEl) nameEl.textContent = studentData.getAttribute('data-name') || '';
                        if (classEl) classEl.textContent = studentData.getAttribute('data-class') || '';
                        if (sectionEl) sectionEl.textContent = studentData.getAttribute('data-section') || '';
                        if (rollEl) rollEl.textContent = studentData.getAttribute('data-roll') || '';
                    }
                })();
            </script>
        </ItemTemplate>
    </asp:FormView>

    <asp:GridView ID="PaidDetailsGridView" DataKeyNames="Role,PayFor" runat="server" AutoGenerateColumns="False" DataSourceID="PaidDetailsSQL" CssClass="mGrid" ShowFooter="True" Font-Bold="False" RowStyle-CssClass="Rows">
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
            <asp:TemplateField HeaderText="Discount">
                <ItemTemplate>
                    <asp:Label ID="DiscountLabel" runat="server" Text='<%# Bind("Total_Discount") %>'></asp:Label>
                    TK
                </ItemTemplate>
                <FooterTemplate>
                    <span id="DiscountTotalLabel"></span>
                    TK
                </FooterTemplate>
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
        <RowStyle CssClass="Rows" />
    </asp:GridView>
    <asp:SqlDataSource ID="PaidDetailsSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
        SelectCommand="SELECT 
    Income_PaymentRecord.MoneyReceiptID, 
    Income_Roles.Role, 
    Income_PaymentRecord.PayFor + ' (' + Education_Year.EducationYear + ')' AS PayFor, 
    Income_PaymentRecord.PaidAmount,
    ISNULL(Income_PayOrder.Discount, 0) + ISNULL(Income_PayOrder.LateFee_Discount, 0) AS Total_Discount,
    CASE 
        WHEN Income_PayOrder.EndDate < GETDATE() - 1 THEN 
            ISNULL(Income_PayOrder.Receivable_Amount, 0) + ISNULL(Income_PayOrder.LateFee, 0) - ISNULL(Income_PayOrder.LateFee_Discount, 0)
        ELSE 
            ISNULL(Income_PayOrder.Receivable_Amount, 0)
    END AS Due,
    Income_PaymentRecord.PaidDate, 
    Income_PayOrder.Amount
FROM Income_PaymentRecord 
INNER JOIN Income_Roles ON Income_PaymentRecord.RoleID = Income_Roles.RoleID 
INNER JOIN Income_PayOrder ON Income_PaymentRecord.PayOrderID = Income_PayOrder.PayOrderID 
INNER JOIN Education_Year ON Income_PaymentRecord.EducationYearID = Education_Year.EducationYearID 
WHERE (Income_PaymentRecord.MoneyReceiptID = @MoneyReceiptID) 
  AND (Income_PaymentRecord.SchoolID = @SchoolID) 
ORDER BY Income_PayOrder.EndDate">
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

            <RowStyle CssClass="Rows"></RowStyle>
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
    <asp:SqlDataSource ID="ReceivedBySQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" 
        SelectCommand="SELECT Admin.FirstName + ' ' + Admin.LastName AS Name FROM Admin INNER JOIN Income_MoneyReceipt ON Admin.RegistrationID = Income_MoneyReceipt.RegistrationID 
        WHERE (Income_MoneyReceipt.MoneyReceiptID = @MoneyReceiptID) AND (Income_MoneyReceipt.SchoolID = @SchoolID)">
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
   <div class="d-flex align-items-center">
      <div>
  <input id="checkboxInstitution" type="checkbox" />
         <label for="checkboxInstitution">Hide Institution Name</label>
                </div>
                <div class="ml-3">
             <input id="checkboxDueDetails" type="checkbox" />
           <label for="checkboxDueDetails">Hide Current Due</label>
                </div>
     </div>

   <div class="d-flex align-items-center mt-3">
   <div>
        <label for="inputTopSpace">Page Space From Top (px)</label>
        <input id="inputTopSpace" min="0" type="number" class="form-control" />
           </div>
      <div class="ml-3">
           <label for="inputFontSize">Font Size (px)</label>
      <input id="inputFontSize" min="10" max="20" type="number" class="form-control" />
    </div>
            </div>
        </div>
  <div class="card-footer">
    <input id="PrintButton" type="button" value="Print" onclick="window.print();" class="btn btn-info" />
        </div>
    </div>

    <!-- SMS SECTION -->
    <div class="d-print-none my-3">
    <!-- SMS Template Info & Edit Link -->
    <div style="background-color: #e7f3ff; border: 1px solid #2196F3; border-radius: 4px; padding: 12px 15px; margin-bottom: 15px;">
   <div style="display: flex; align-items: center; justify-content: space-between;">
       <div style="flex: 1;">
         <i class="fa fa-info-circle" style="color: #2196F3; font-size: 18px; margin-right: 8px;"></i>
    <strong style="color: #1976D2;">SMS Template Active:</strong>
            <span style="color: #555; margin-left: 10px;">
   Using system template for payment receipt. 
          <small style="color: #777;">(Placeholders: {StudentName}, {Amount}, {ReceiptNo}, {PaymentDetails}, {CurrentDue}, etc.)</small>
    </span>
     </div>
        <div>
    <a href="/SMS/SMS_Template.aspx" class="btn btn-info btn-sm" style="text-decoration: none;">
       <i class="fa fa-edit"></i> Edit SMS Templates
    </a>
        </div>
       </div>
    </div>

    <asp:Button ID="SMSButton" runat="server" Text="Send Receipt SMS" CssClass="btn btn-primary" OnClick="SMSButton_Click" />
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

 //Discount Grand Total and Hide/Show Discount Column
 var DiscountTotal = 0;
 var hasDiscount = false;
 
 $("[id*=DiscountLabel]").each(function () { 
     var val = parseFloat($(this).text());
     if (!isNaN(val) && val > 0) {
         hasDiscount = true;
         DiscountTotal = DiscountTotal + val;
     }
 });
 
 // Show/Hide Discount Column based on whether there's any discount
 var gridView = $('#<%= PaidDetailsGridView.ClientID %>');
 
 if (!hasDiscount || DiscountTotal === 0) {
     // Hide Discount column (header and all cells) - it's the 3rd column
     gridView.find('tr').each(function() {
         $(this).find('th:eq(2), td:eq(2)').addClass('discount-column-hidden');
     });
     
     console.log('Discount column hidden - no discounts found');
 } else {
     // Show discount total in footer
     $("#DiscountTotalLabel").text(DiscountTotal.toFixed(2));
     console.log('Discount column visible - Total discount: ' + DiscountTotal.toFixed(2));
 }

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
