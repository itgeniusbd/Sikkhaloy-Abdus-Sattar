<%@ Page Title="Paid Invoice" Language="C#" MasterPageFile="~/Basic_Authority.Master" AutoEventWireup="true" CodeBehind="Paid_Invoice.aspx.cs" Inherits="EDUCATION.COM.Authority.Invoice.Paid_Invoice" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .IsError { border: 1px solid #ff0000; color: #ff0000; }
        .mGrid .form-control { display: inline; width: 92%; }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <h3>Paid Invoice</h3>

    <div class="form-inline">
        <div class="form-group">
            <asp:DropDownList ID="School_DropDownList" AutoPostBack="true" CssClass="form-control SearchDDL" runat="server" AppendDataBoundItems="True" DataSourceID="InstitutionSQL" DataTextField="SchoolName_ID" DataValueField="SchoolID">
                <asp:ListItem Value="0">[ INSTITUTION ]</asp:ListItem>
            </asp:DropDownList>
            <asp:SqlDataSource ID="InstitutionSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT DISTINCT SchoolInfo.SchoolID,SchoolInfo.SchoolName,  CAST(SchoolInfo.SchoolID AS varchar(10)) + ' - ' + SchoolInfo.SchoolName AS SchoolName_ID FROM SchoolInfo INNER JOIN AAP_Invoice ON SchoolInfo.SchoolID = AAP_Invoice.SchoolID WHERE (SchoolInfo.Validation = N'Valid') AND (AAP_Invoice.IsPaid = 0)  ORDER BY SchoolInfo.SchoolName"></asp:SqlDataSource>
        </div>
    </div>

    <asp:GridView ID="InvoiceGridView" CssClass="mGrid" runat="server" AutoGenerateColumns="False" DataKeyNames="InvoiceID" DataSourceID="InvoiceSQL" ShowFooter="True">
        <Columns>
            <asp:TemplateField>
                <ItemTemplate>
                    <asp:CheckBox ID="Pay_CheckBox" Text=" " runat="server" />
                </ItemTemplate>
            </asp:TemplateField>
            <asp:BoundField DataField="Invoice_SN" HeaderText="SN" SortExpression="Invoice_SN" />
            <asp:BoundField DataField="InvoiceCategory" HeaderText="Category" SortExpression="InvoiceCategory" />
            <asp:BoundField DataField="MonthName" DataFormatString="{0:MMMM yyyy}" HeaderText="Month" SortExpression="MonthName" />
            <asp:BoundField DataField="IssuDate" DataFormatString="{0:d MMM yyyy}" HeaderText="Issue Date" SortExpression="IssuDate" />
            <asp:BoundField DataField="Invoice_For" HeaderText="Invoice For" SortExpression="Invoice_For" />
            <asp:BoundField DataField="Unit" HeaderText="Unit" SortExpression="Unit" />
            <asp:BoundField DataField="UnitPrice" HeaderText="Unit Price" SortExpression="UnitPrice" />
            <asp:BoundField DataField="TotalAmount" HeaderText="Total Amount" SortExpression="TotalAmount" />
            <asp:TemplateField HeaderText="Discount" SortExpression="Discount">
                <ItemTemplate> 
                    <asp:TextBox ID="Discount_TextBox" CssClass="form-control" runat="server" Text='<%# Bind("Discount") %>' onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false"></asp:TextBox>
                </ItemTemplate>
            </asp:TemplateField>
            <asp:BoundField DataField="PaidAmount" HeaderText="Paid" SortExpression="PaidAmount" />
            <asp:TemplateField HeaderText="Due" SortExpression="Due">
                <ItemTemplate>
                    <span class="Due"><%# Eval("Due") %></span>
                </ItemTemplate>
            </asp:TemplateField>
            <asp:TemplateField HeaderText="Pay" SortExpression="Due">
                <ItemTemplate>
                    <asp:TextBox ID="PaidAmount_TextBox" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false" Text='<%# Bind("Due") %>' Enabled="false" runat="server" CssClass="form-control ValueCount"></asp:TextBox>
                    <asp:RequiredFieldValidator ControlToValidate="PaidAmount_TextBox" ValidationGroup="P" CssClass="EroorStar" ID="RequiredFieldValidator1" runat="server" ErrorMessage="*"></asp:RequiredFieldValidator>
                </ItemTemplate>
                <FooterTemplate>
                    <label id="GTAmount"></label>
                </FooterTemplate>
                <ItemStyle Width="130px" />
            </asp:TemplateField>
        </Columns>
    </asp:GridView>
    <asp:SqlDataSource ID="InvoiceSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT AAP_Invoice.Invoice_SN, AAP_Invoice.IssuDate, AAP_Invoice.EndDate, AAP_Invoice.Invoice_For, AAP_Invoice.Unit, AAP_Invoice.UnitPrice, AAP_Invoice.TotalAmount, AAP_Invoice.Discount, AAP_Invoice.CreateDate, AAP_Invoice.PaidAmount, AAP_Invoice.Due, AAP_Invoice.MonthName, AAP_Invoice_Category.InvoiceCategory, AAP_Invoice.InvoiceID FROM AAP_Invoice INNER JOIN AAP_Invoice_Category ON AAP_Invoice.InvoiceCategoryID = AAP_Invoice_Category.InvoiceCategoryID WHERE (AAP_Invoice.SchoolID = @SchoolID) AND (AAP_Invoice.IsPaid = 0)" UpdateCommand="UPDATE AAP_Invoice SET NumberOfPayment = ISNULL(NumberOfPayment, 0) + 1, LastPaidDate = GETDATE(), PaidAmount = PaidAmount + @PaidAmount, Discount = @Discount WHERE (InvoiceID = @InvoiceID)">
        <SelectParameters>
            <asp:ControlParameter ControlID="School_DropDownList" Name="SchoolID" PropertyName="SelectedValue" Type="Int32" />
        </SelectParameters>
        <UpdateParameters>
            <asp:Parameter Name="PaidAmount" />
            <asp:Parameter Name="InvoiceID" />
            <asp:Parameter Name="Discount" />
        </UpdateParameters>
    </asp:SqlDataSource>

    <asp:SqlDataSource ID="Invoice_ReceiptSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" InsertCommand="INSERT INTO [AAP_Invoice_Receipt] ([SchoolID], [RegistrationID], [TotalAmount], [PaidDate], [PaymentBy], [Collected_By], [Payment_Method], [InvoiceReceipt_SN]) VALUES (@SchoolID, @RegistrationID, @TotalAmount, @PaidDate, @PaymentBy, @Collected_By, @Payment_Method, [dbo].[F_InvoiceReceipt_SN]())" SelectCommand="SELECT * FROM [AAP_Invoice_Receipt]">
        <InsertParameters>
            <asp:ControlParameter ControlID="School_DropDownList" Name="SchoolID" PropertyName="SelectedValue" Type="Int32" />
            <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" Type="Int32" />
            <asp:ControlParameter ControlID="GTHiddenField" Name="TotalAmount" PropertyName="Value" Type="Double" />
            <asp:ControlParameter ControlID="PaidDateTextBox" Name="PaidDate" PropertyName="Text" Type="DateTime" />
            <asp:ControlParameter ControlID="PaymentByTextBox" Name="PaymentBy" PropertyName="Text" Type="String" />
            <asp:ControlParameter ControlID="Collected_ByTextBox" Name="Collected_By" PropertyName="Text" Type="String" />
            <asp:ControlParameter ControlID="Payment_MethodTextBox" Name="Payment_Method" PropertyName="Text" Type="String" />
        </InsertParameters>
    </asp:SqlDataSource>
    <asp:SqlDataSource ID="Invoice_Payment_RecordSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" InsertCommand="INSERT INTO [AAP_Invoice_Payment_Record] ([InvoiceReceiptID], [InvoiceID], [RegistrationID], [SchoolID], [Amount], [PaidDate]) VALUES ((SELECT IDENT_CURRENT('AAP_Invoice_Receipt')), @InvoiceID, @RegistrationID, @SchoolID, @Amount, @PaidDate)" SelectCommand="SELECT * FROM [AAP_Invoice_Payment_Record]">
        <InsertParameters>
            <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" Type="Int32" />
            <asp:ControlParameter ControlID="School_DropDownList" Name="SchoolID" PropertyName="SelectedValue" Type="Int32" />
            <asp:ControlParameter ControlID="PaidDateTextBox" DbType="Date" Name="PaidDate" PropertyName="Text" />
            <asp:Parameter Name="InvoiceID" Type="Int32" />
            <asp:Parameter Name="Amount" Type="Double" />
        </InsertParameters>
    </asp:SqlDataSource>

    <%if (InvoiceGridView.Rows.Count > 0)
        {%>
    <div class="form-inline mt-2">
        <div class="form-group">
            <asp:TextBox ID="PaidDateTextBox" placeholder="Paid Date" CssClass="form-control datepicker" runat="server"></asp:TextBox>
        </div>
        <div class="form-group mx-1">
            <asp:TextBox ID="PaymentByTextBox" placeholder="Paid By" CssClass="form-control" runat="server"></asp:TextBox>
        </div>
        <div class="form-group">
            <asp:TextBox ID="Collected_ByTextBox" placeholder="Received By" CssClass="form-control AC2" runat="server"></asp:TextBox>
        </div>
        <div class="form-group ml-1">
            <asp:TextBox ID="Payment_MethodTextBox" placeholder="Payment Method" CssClass="form-control AC" runat="server"></asp:TextBox>
        </div>
        <div class="form-group">
            <asp:Button ID="Pay_Button" ValidationGroup="P" runat="server" Text="Pay" CssClass="btn btn-dark-green" OnClick="Pay_Button_Click" />
            <asp:HiddenField ID="GTHiddenField" runat="server" />
        </div>
    </div>
    <%} %>
   
    <script>
        $(function () {
            $("[id*=Pay_CheckBox]").on("click", function () {
                var tr = $($(this).closest("tr")),
                    IsChecked = $(this).is(":checked"),
                    Due = parseFloat($(".Due", tr).text()),
                    Pay = $("[id*=PaidAmount_TextBox]", tr);
                Pay.prop("disabled", !IsChecked).val(Due);

                if (IsChecked) {
                    tr.addClass("selected");
                }
                else {
                    tr.removeClass("selected").removeAttr('class');
                }

                sum();
            });

            $("[id*=PaidAmount_TextBox]").on("keyup keydown past drop blur", function () {
                var tr = $($(this).closest("tr")),
                    Due = parseFloat($(".Due", tr).text()),
                    Pay = parseFloat($(this).val());

                if (Due < Pay) {
                    $(this).val(Due);
                }
                sum();
            });

            $('.datepicker').datepicker({
                format: 'dd M yyyy',
                todayBtn: "linked",
                todayHighlight: true,
                autoclose: true
            });
        });

        function sum() {
            var grandTotal = 0;
            $('.ValueCount').each(function () {
                if (!$(this).is(':disabled')) {
                    var value = $(this).val() || 0;
                    grandTotal += parseInt(value);
                }
            });

            $('#GTAmount').html("Total: " + grandTotal + " Tk");
            $('[id*=GTHiddenField]').val(grandTotal);
        }

        function isNumberKey(a) { a = a.which ? a.which : event.keyCode; return 46 != a && 31 < a && (48 > a || 57 < a) ? !1 : !0 };
    </script>
</asp:Content>
