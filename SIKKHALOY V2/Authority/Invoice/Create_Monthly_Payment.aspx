<%@ Page Title="Create Invoice" Language="C#" MasterPageFile="~/Basic_Authority.Master" AutoEventWireup="true" CodeBehind="Create_Monthly_Payment.aspx.cs" Inherits="EDUCATION.COM.Authority.Invoice.Create_Monthly_Payment" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .Invaid_Ins td { color: #ff2b2b; }
        .Invaid_Ins td a { color: #ff2b2b; }
        
        /* Select2 custom styling for institution dropdown */
        .select2-container--default .select2-results__option {
            padding: 8px 12px;
        }
        .select2-container {
            width: 100% !important;
        }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <h3>Create Invoice</h3>

    <ul class="nav nav-tabs nav-justified z-depth-1">
        <li class="nav-item"><a class="nav-link active" data-toggle="tab" role="tab" href="#tab1">Service Charge</a></li>
        <li class="nav-item"><a class="nav-link" data-toggle="tab" role="tab" href="#tab2">SMS Invoice</a></li>
        <li class="nav-item"><a class="nav-link" data-toggle="tab" role="tab" href="#tab3">Others Invoice</a></li>
    </ul>

    <div class="tab-content card">
        <div class="tab-pane fade in show active" role="tabpanel" id="tab1">
            <asp:UpdatePanel ID="UpdatePanel3" runat="server">
                <ContentTemplate>
                    <div class="form-inline">
                        <div class="form-group">
                            <asp:DropDownList ID="Month_DropDownList" CssClass="form-control" runat="server" DataSourceID="MonthSQL" DataTextField="Month" DataValueField="Date_N" AppendDataBoundItems="True" AutoPostBack="True">
                                <asp:ListItem Value="0">[ SELECT MONTH ]</asp:ListItem>
                            </asp:DropDownList>
                            <asp:SqlDataSource ID="MonthSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT DISTINCT EOMONTH(Month) AS Date_N, FORMAT(Month, 'MMM yyyy') AS Month FROM AAP_Student_Count_Monthly GROUP BY Month ORDER BY EOMONTH(Month) DESC"></asp:SqlDataSource>
                            <asp:RequiredFieldValidator ControlToValidate="Month_DropDownList" InitialValue="0" ValidationGroup="SC" ID="RequiredFieldValidator2" runat="server" ErrorMessage="*" CssClass="EroorStar"></asp:RequiredFieldValidator>
                        </div>
                    </div>

                    <asp:GridView ID="Payment_GridView" CssClass="mGrid" runat="server" AutoGenerateColumns="False" DataSourceID="SchoolListSQL" DataKeyNames="SchoolID">
                        <Columns>
                            <asp:TemplateField>
                                <HeaderTemplate>
                                    <asp:CheckBox ID="AllCheckBox" Text="All" runat="server" />
                                </HeaderTemplate>
                                <ItemTemplate>
                                    <asp:CheckBox ID="Invoice_CheckBox" Text=' ' runat="server" />
                                    <input type="hidden" class="IS_ServiceActive" value="<%#Eval("IS_ServiceChargeActive") %>" />
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:BoundField DataField="SchoolID" HeaderText="School ID" SortExpression="SchoolID" />
                            <asp:TemplateField HeaderText="Institution" SortExpression="SchoolName">
                                <ItemTemplate>
                                    <asp:LinkButton OnCommand="Ins_LinkButton_Command" CommandArgument='<%#Eval("SchoolName") %>' CommandName='<%# Bind("SchoolID") %>' ID="Ins_LinkButton" runat="server"><%# Eval("SchoolName") %></asp:LinkButton>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Total" SortExpression="StudentCount">
                                <ItemTemplate>
                                    <asp:Label ID="Total_Student_Label" runat="server" Text='<%# Bind("StudentCount") %>' />
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Per Student" SortExpression="Per_Student_Rate">
                                <ItemTemplate>
                                    <asp:Label ID="PerStudent_Label" runat="server" Text='<%# Bind("Per_Student_Rate") %>' />
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:BoundField DataField="Reject_Countable" HeaderText="Rejected Countable" SortExpression="Reject_Countable" />
                            <asp:BoundField DataField="Reject_Uncountable" HeaderText="Rejt. Uncountable" SortExpression="Reject_Uncountable" />
                            <asp:TemplateField HeaderText="Payment Active" SortExpression="IS_ServiceChargeActive">
                                <ItemTemplate>
                                    <asp:CheckBox ID="SCCheckBox" Text=" " runat="server" Checked='<%# Bind("IS_ServiceChargeActive") %>' Enabled="false" />
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:BoundField DataField="Active_Student" HeaderText="Active" SortExpression="Active_Student" />
                            <asp:TemplateField HeaderText="Discount" SortExpression="Discount">
                                <ItemTemplate>
                                    <asp:TextBox ID="Discount_TextBox" onkeypress="return isNumberKey(event)" autocomplete="off" CssClass="form-control" runat="server" Text='<%# Bind("Discount") %>'></asp:TextBox>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Fixed" SortExpression="Fixed">
                                <ItemTemplate>
                                    <asp:Label ID="Fixed_Label" runat="server" Text='<%# Bind("Fixed") %>' />
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                    </asp:GridView>

                    <asp:SqlDataSource ID="SchoolListSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT SchoolInfo.SchoolName, SchoolInfo.Per_Student_Rate, SchoolInfo.IS_ServiceChargeActive, SchoolInfo.Discount, SchoolInfo.Fixed, AAP_Student_Count_Monthly.SchoolID, AAP_Student_Count_Monthly.StudentCount, AAP_Student_Count_Monthly.Active_Student, AAP_Student_Count_Monthly.Reject_Countable, AAP_Student_Count_Monthly.Reject_Uncountable, AAP_Student_Count_Monthly.Month FROM SchoolInfo INNER JOIN AAP_Student_Count_Monthly ON SchoolInfo.SchoolID = AAP_Student_Count_Monthly.SchoolID WHERE (FORMAT(AAP_Student_Count_Monthly.Month, 'MMM yyyy') = @Month)">
                        <SelectParameters>
                            <asp:ControlParameter ControlID="Month_DropDownList" Name="Month" PropertyName="SelectedItem.Text" />
                        </SelectParameters>
                    </asp:SqlDataSource>
                    <asp:SqlDataSource ID="PayOrderSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" InsertCommand="IF NOT EXISTS (SELECT InvoiceID FROM AAP_Invoice WHERE (SchoolID = @SchoolID) AND (InvoiceCategoryID = (SELECT InvoiceCategoryID FROM AAP_Invoice_Category WHERE (InvoiceCategory = N'Service Charge'))) AND (FORMAT(MonthName, 'MMM yyyy') = @Month))
BEGIN
INSERT INTO AAP_Invoice(RegistrationID, InvoiceCategoryID, SchoolID, IssuDate, EndDate, Invoice_For, TotalAmount, Discount, MonthName, Invoice_SN, Unit, UnitPrice) VALUES (@RegistrationID, (SELECT InvoiceCategoryID FROM AAP_Invoice_Category WHERE (InvoiceCategory = N'Service Charge')), @SchoolID, @IssuDate, @EndDate, @Invoice_For, @TotalAmount, @Discount, @MonthName, dbo.Invoice_SerialNumber(@SchoolID), @Unit, @UnitPrice)
END"
                        SelectCommand="SELECT * FROM [AAP_Invoice]">
                        <InsertParameters>
                            <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" Type="Int32" />
                            <asp:ControlParameter ControlID="sIssueDate_TextBox" DbType="Date" Name="IssuDate" PropertyName="Text" />
                            <asp:ControlParameter ControlID="Month_DropDownList" DbType="Date" Name="MonthName" PropertyName="SelectedValue" />
                            <asp:ControlParameter ControlID="Month_DropDownList" Name="Month" PropertyName="SelectedItem.Text" />
                            <asp:ControlParameter ControlID="Month_DropDownList" Name="Invoice_For" PropertyName="SelectedItem.Text" Type="String" />
                            <asp:Parameter DbType="Date" Name="EndDate" />
                            <asp:Parameter Name="SchoolID" Type="Int32" />
                            <asp:Parameter Name="TotalAmount" Type="Double" />
                            <asp:Parameter Name="Discount" Type="Double" />
                            <asp:Parameter Name="Unit" />
                            <asp:Parameter Name="UnitPrice" />
                        </InsertParameters>
                    </asp:SqlDataSource>

                    <%if (Payment_GridView.Rows.Count > 0)
                        {%>
                    <div class="form-inline">
                        <div class="form-group">
                            <asp:TextBox ID="sIssueDate_TextBox" placeholder="Issue Date" CssClass="form-control datepicker" runat="server"></asp:TextBox>
                            <asp:RequiredFieldValidator ControlToValidate="sIssueDate_TextBox" ValidationGroup="SC" ID="RequiredFieldValidator7" runat="server" ErrorMessage="*" CssClass="EroorStar"></asp:RequiredFieldValidator>
                        </div>

                        <div class="form-group">
                            <asp:Button ID="Monthly_Button" ValidationGroup="SC" OnClick="Monthly_Button_Click" runat="server" Text="Submit" CssClass="btn btn-success" />
                        </div>
                    </div>
                    <%} %>
                </ContentTemplate>
            </asp:UpdatePanel>
        </div>

        <div class="tab-pane fade" role="tabpanel" id="tab2">
            <asp:UpdatePanel ID="UpdatePanel4" runat="server">
                <ContentTemplate>
                    <div class="form-inline">
                        <div class="form-group">
                            <asp:TextBox ID="Find_Institution_TextBox" CssClass="form-control" placeholder="Institution" runat="server"></asp:TextBox>
                        </div>
                        <div class="form-group mx-1">
                            <asp:TextBox ID="Find_FromDate_TextBox" CssClass="form-control datepicker" placeholder="From Date" runat="server"></asp:TextBox>
                        </div>
                        <div class="form-group">
                            <asp:TextBox ID="Find_ToDate_TextBox" CssClass="form-control datepicker" placeholder="To Date" runat="server"></asp:TextBox>
                        </div>
                        <div class="form-group">
                            <asp:Button ID="Find_Button" runat="server" Text="Find" CssClass="btn btn-brown" />
                        </div>
                    </div>

                    <asp:GridView ID="SMSGridView" CssClass="mGrid" runat="server" AutoGenerateColumns="False" DataKeyNames="SMS_Recharge_RecordID,SchoolID" DataSourceID="SMS_SQL">
                        <Columns>
                            <asp:TemplateField>
                                <HeaderTemplate>
                                    <asp:CheckBox ID="AllSMS_CheckBox" Text="All" runat="server" />
                                </HeaderTemplate>
                                <ItemTemplate>
                                    <asp:CheckBox ID="SMS_CheckBox" Text=' ' runat="server" />
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:BoundField DataField="SchoolID" HeaderText="School ID" SortExpression="SchoolID" />
                            <asp:BoundField DataField="SchoolName" HeaderText="Institution" SortExpression="SchoolName" />
                            <asp:TemplateField HeaderText="SMS Unit" SortExpression="RechargeSMS">
                                <ItemTemplate>
                                    <asp:Label ID="SMS_Unit_Label" runat="server" Text='<%# Bind("RechargeSMS") %>'></asp:Label>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Per SMS" SortExpression="PerSMS_Price">
                                <ItemTemplate>
                                    <asp:Label ID="PerSMS_Label" runat="server" Text='<%# Bind("PerSMS_Price") %>'></asp:Label>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Total" SortExpression="Total_Price">
                                <ItemTemplate>
                                    <asp:Label ID="TotalAmount_Label" runat="server" Text='<%# Bind("Total_Price") %>'></asp:Label>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Date" SortExpression="Date">
                                <ItemTemplate>
                                    <asp:Label ID="RechargeDate_Label" runat="server" Text='<%# Bind("Date", "{0:d MMM yyyy}") %>'></asp:Label>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Paid">
                                <ItemTemplate>
                                    <asp:CheckBox OnCheckedChanged="SMS_Paid_CheckBox_CheckedChanged" AutoPostBack="true" ID="SMS_Paid_CheckBox" Text=' ' runat="server" />
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                    </asp:GridView>
                    <asp:SqlDataSource ID="SMS_SQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT SMS_Recharge_Record.SMS_Recharge_RecordID, SMS_Recharge_Record.SchoolID, SchoolInfo.SchoolName, SMS_Recharge_Record.RechargeSMS, SMS_Recharge_Record.PerSMS_Price, SMS_Recharge_Record.Total_Price, SMS_Recharge_Record.Date FROM SMS_Recharge_Record INNER JOIN SchoolInfo ON SMS_Recharge_Record.SchoolID = SchoolInfo.SchoolID WHERE (SMS_Recharge_Record.Total_Price &gt; 0) AND (SMS_Recharge_Record.Is_Paid = 0) AND (SMS_Recharge_Record.Date BETWEEN ISNULL(@From_Date, N'1-1-1000') AND ISNULL(@To_Date, N'1-1-3000'))" UpdateCommand="UPDATE SMS_Recharge_Record SET Is_Paid = 1 WHERE (SMS_Recharge_RecordID = @SMS_Recharge_RecordID)" InsertCommand="INSERT INTO AAP_Invoice(RegistrationID, InvoiceCategoryID, SchoolID, IssuDate, EndDate, Invoice_For, TotalAmount, MonthName, Invoice_SN, Unit, UnitPrice) VALUES (@RegistrationID, (SELECT InvoiceCategoryID FROM AAP_Invoice_Category WHERE (InvoiceCategory = N'SMS')), @SchoolID, @IssuDate, @EndDate, @Invoice_For, @TotalAmount, @MonthName, dbo.Invoice_SerialNumber(@SchoolID), @Unit, @UnitPrice)"
                        FilterExpression="SchoolName LIKE '{0}%'" CancelSelectOnNullParameter="False">
                        <FilterParameters>
                            <asp:ControlParameter ControlID="Find_Institution_TextBox" Name="Ins" PropertyName="Text" />
                        </FilterParameters>
                        <InsertParameters>
                            <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" />
                            <asp:ControlParameter ControlID="SMS_Issue_TextBox" Name="IssuDate" PropertyName="Text" />
                            <asp:ControlParameter ControlID="SMS_MonthName_TextBox" Name="MonthName" PropertyName="Text" />
                            <asp:Parameter Name="SchoolID" />
                            <asp:Parameter Name="EndDate" />
                            <asp:Parameter Name="Invoice_For" />
                            <asp:Parameter Name="TotalAmount" />
                            <asp:Parameter Name="Unit" />
                            <asp:Parameter Name="UnitPrice" />
                        </InsertParameters>
                        <SelectParameters>
                            <asp:ControlParameter ControlID="Find_FromDate_TextBox" Name="From_Date" PropertyName="Text" />
                            <asp:ControlParameter ControlID="Find_ToDate_TextBox" Name="To_Date" PropertyName="Text" />
                        </SelectParameters>
                        <UpdateParameters>
                            <asp:Parameter Name="SMS_Recharge_RecordID" />
                        </UpdateParameters>
                    </asp:SqlDataSource>

                    <%if (SMSGridView.Rows.Count > 0)
                        {%>
                    <div class="form-inline">
                        <div class="form-group">
                            <asp:TextBox ID="SMS_MonthName_TextBox" placeholder="Invoice Month Name" CssClass="form-control datepicker" runat="server"></asp:TextBox>
                            <asp:RequiredFieldValidator ControlToValidate="SMS_MonthName_TextBox" ValidationGroup="sms" ID="RequiredFieldValidator11" runat="server" ErrorMessage="*" CssClass="EroorStar"></asp:RequiredFieldValidator>
                        </div>
                        <div class="form-group">
                            <asp:TextBox ID="SMS_Issue_TextBox" placeholder="Issue Date" CssClass="form-control datepicker" runat="server"></asp:TextBox>
                            <asp:RequiredFieldValidator ControlToValidate="SMS_Issue_TextBox" ValidationGroup="sms" ID="RequiredFieldValidator8" runat="server" ErrorMessage="*" CssClass="EroorStar"></asp:RequiredFieldValidator>
                        </div>
                        <div class="form-group">
                            <asp:Button ID="SMS_Invoice_Button" OnClick="SMS_Invoice_Button_Click" ValidationGroup="sms" runat="server" Text="Submit" CssClass="btn btn-primary" />
                        </div>
                    </div>
                    <%} %>
                </ContentTemplate>
            </asp:UpdatePanel>
        </div>

        <div class="tab-pane fade" role="tabpanel" id="tab3">
            <asp:UpdatePanel ID="UpdatePanel5" runat="server">
                <ContentTemplate>
                    <div class="row">
                        <div class="col-md-6">
                            <div class="card">
                                <div class="card-body">
                                    <div class="form-group">
                                        <label>
                                            Category
                            <asp:RequiredFieldValidator ControlToValidate="Category_DropDownList" ValidationGroup="I" InitialValue="0" ID="RequiredFieldValidator9" runat="server" ErrorMessage="*" CssClass="EroorStar"></asp:RequiredFieldValidator>
                                            <a data-toggle="modal" class="text-primary" data-target="#CategoryModal">Add New</a></label>
                                        <asp:DropDownList ID="Category_DropDownList" CssClass="form-control" runat="server" DataSourceID="CategorySQL" DataTextField="InvoiceCategory" DataValueField="InvoiceCategoryID" AppendDataBoundItems="True">
                                            <asp:ListItem Value="0">[ SELECT CATEGORY ]</asp:ListItem>
                                        </asp:DropDownList>

                                        <asp:SqlDataSource ID="CategorySQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT * FROM [AAP_Invoice_Category]"></asp:SqlDataSource>
                                    </div>
                                    <div class="form-group">
                                        <label>Institution<asp:RequiredFieldValidator ControlToValidate="School_DropDownList" ValidationGroup="I" InitialValue="0" ID="RequiredFieldValidator1" runat="server" ErrorMessage="*" CssClass="EroorStar"></asp:RequiredFieldValidator></label>
                                        <asp:DropDownList ID="School_DropDownList" AutoPostBack="true" CssClass="form-control select2-dropdown" runat="server" AppendDataBoundItems="True" DataSourceID="InstitutionSQL" DataTextField="DisplayText" DataValueField="SchoolID">
                                            <asp:ListItem Value="0">[ INSTITUTION ]</asp:ListItem>
                                        </asp:DropDownList>
                                        <asp:SqlDataSource ID="InstitutionSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT SchoolID, CAST(SchoolID AS NVARCHAR) + ' - ' + SchoolName AS DisplayText FROM SchoolInfo ORDER BY SchoolID"></asp:SqlDataSource>
                                    </div>
                                    <div class="form-group">
                                        <label>
                                            Invoice Month Name
                            <asp:RequiredFieldValidator ControlToValidate="MonthName_TextBox" ValidationGroup="I" ID="RequiredFieldValidator10" runat="server" ErrorMessage="*" CssClass="EroorStar"></asp:RequiredFieldValidator></label>
                                        <asp:TextBox ID="MonthName_TextBox" placeholder="Invoice Month Name" CssClass="form-control datepicker" runat="server"></asp:TextBox>
                                    </div>
                                    <div class="form-group">
                                        <label>
                                            Issue Date 
                            <asp:RequiredFieldValidator ControlToValidate="IssuDateTextBox" ValidationGroup="I" ID="RequiredFieldValidator3" runat="server" ErrorMessage="*" CssClass="EroorStar"></asp:RequiredFieldValidator></label>
                                        <asp:TextBox ID="IssuDateTextBox" placeholder="Issue Date" CssClass="form-control datepicker" runat="server"></asp:TextBox>
                                    </div>
                                    <div class="form-group">
                                        <label>
                                            End Date
                            <asp:RequiredFieldValidator ControlToValidate="EndDateTextBox" ValidationGroup="I" ID="RequiredFieldValidator4" runat="server" ErrorMessage="*" CssClass="EroorStar"></asp:RequiredFieldValidator></label>
                                        <asp:TextBox ID="EndDateTextBox" placeholder="End Date" CssClass="form-control datepicker" runat="server"></asp:TextBox>
                                    </div>
                                    <div class="form-group">
                                        <label>
                                            Invoice For
                            <asp:RequiredFieldValidator ControlToValidate="Invoice_ForTextBox" ValidationGroup="I" ID="RequiredFieldValidator5" runat="server" ErrorMessage="*" CssClass="EroorStar"></asp:RequiredFieldValidator></label>
                                        <asp:TextBox ID="Invoice_ForTextBox" TextMode="MultiLine" placeholder="Invoice For" CssClass="form-control" runat="server"></asp:TextBox>
                                    </div>
                                    <div class="form-group">
                                        <label>Unit</label>
                                        <asp:TextBox ID="UnitTextBox" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false" placeholder="Unit" CssClass="form-control" runat="server"></asp:TextBox>
                                    </div>
                                    <div class="form-group">
                                        <label>Unit Price</label>
                                        <asp:TextBox ID="UnitPrice_TextBox" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false" placeholder="Unit Price" CssClass="form-control" runat="server"></asp:TextBox>
                                    </div>
                                    <div class="form-group">
                                        <label>
                                            Total Amount
                            <asp:RequiredFieldValidator ControlToValidate="TotalAmountTextBox" ValidationGroup="I" ID="RequiredFieldValidator6" runat="server" ErrorMessage="*" CssClass="EroorStar"></asp:RequiredFieldValidator></label>
                                        <asp:TextBox ID="TotalAmountTextBox" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false" placeholder="Total Amount" CssClass="form-control" runat="server"></asp:TextBox>
                                    </div>
                                    <div class="form-group">
                                        <label>Discount</label>
                                        <asp:TextBox ID="DiscountTextBox" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false" placeholder="Discount" CssClass="form-control" runat="server"></asp:TextBox>
                                    </div>
                                    <div class="form-group">
                                        <asp:Button ID="OtherInvoice_Button" ValidationGroup="I" runat="server" Text="Create" CssClass="btn btn-orange" OnClick="OtherInvoice_Button_Click" />
                                        <asp:SqlDataSource ID="OthersInvoiceSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" DeleteCommand="DELETE FROM [AAP_Invoice] WHERE [InvoiceID] = @InvoiceID" InsertCommand="INSERT INTO AAP_Invoice(RegistrationID, InvoiceCategoryID, SchoolID, IssuDate, EndDate, Invoice_For, TotalAmount, Discount, Invoice_SN, Unit, UnitPrice, MonthName) VALUES (@RegistrationID, @InvoiceCategoryID, @SchoolID, @IssuDate, @EndDate, @Invoice_For, @TotalAmount, @Discount, dbo.Invoice_SerialNumber(@SchoolID), @Unit, @UnitPrice, @MonthName)" SelectCommand="SELECT InvoiceID, RegistrationID, InvoiceCategoryID, SchoolID, Invoice_SN, IssuDate, EndDate, Invoice_For, TotalAmount, Discount, CreateDate, PaidAmount, Due, NumberOfPayment, IsPaid, LastPaidDate FROM AAP_Invoice WHERE (SchoolID = @SchoolID)" UpdateCommand="UPDATE [AAP_Invoice] SET [RegistrationID] = @RegistrationID, [InvoiceCategoryID] = @InvoiceCategoryID, [SchoolID] = @SchoolID, [Invoice_SN] = @Invoice_SN, [IssuDate] = @IssuDate, [EndDate] = @EndDate, [Invoice_For] = @Invoice_For, [TotalAmount] = @TotalAmount, [Discount] = @Discount, [CreateDate] = @CreateDate, [PaidAmount] = @PaidAmount, [Due] = @Due, [NumberOfPayment] = @NumberOfPayment, [IsPaid] = @IsPaid, [LastPaidDate] = @LastPaidDate WHERE [InvoiceID] = @InvoiceID">
                                            <DeleteParameters>
                                                <asp:Parameter Name="InvoiceID" Type="Int32" />
                                            </DeleteParameters>
                                            <InsertParameters>
                                                <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" Type="Int32" />
                                                <asp:ControlParameter ControlID="Category_DropDownList" Name="InvoiceCategoryID" PropertyName="SelectedValue" Type="Int32" />
                                                <asp:ControlParameter ControlID="School_DropDownList" Name="SchoolID" PropertyName="SelectedValue" Type="Int32" />
                                                <asp:ControlParameter ControlID="IssuDateTextBox" DbType="Date" Name="IssuDate" PropertyName="Text" />
                                                <asp:ControlParameter ControlID="EndDateTextBox" DbType="Date" Name="EndDate" PropertyName="Text" />
                                                <asp:ControlParameter ControlID="Invoice_ForTextBox" Name="Invoice_For" PropertyName="Text" Type="String" />
                                                <asp:ControlParameter ControlID="TotalAmountTextBox" Name="TotalAmount" PropertyName="Text" Type="Double" />
                                                <asp:ControlParameter ControlID="DiscountTextBox" Name="Discount" PropertyName="Text" Type="Double" DefaultValue="0" />
                                                <asp:ControlParameter ControlID="UnitTextBox" Name="Unit" PropertyName="Text" />
                                                <asp:ControlParameter ControlID="UnitPrice_TextBox" Name="UnitPrice" PropertyName="Text" />
                                                <asp:ControlParameter ControlID="MonthName_TextBox" DefaultValue="" Name="MonthName" PropertyName="Text" />
                                            </InsertParameters>
                                            <SelectParameters>
                                                <asp:ControlParameter ControlID="School_DropDownList" Name="SchoolID" PropertyName="SelectedValue" />
                                            </SelectParameters>
                                            <UpdateParameters>
                                                <asp:Parameter Name="RegistrationID" Type="Int32" />
                                                <asp:Parameter Name="InvoiceCategoryID" Type="Int32" />
                                                <asp:Parameter Name="SchoolID" Type="Int32" />
                                                <asp:Parameter Name="Invoice_SN" Type="Int32" />
                                                <asp:Parameter DbType="Date" Name="IssuDate" />
                                                <asp:Parameter DbType="Date" Name="EndDate" />
                                                <asp:Parameter Name="Invoice_For" Type="String" />
                                                <asp:Parameter Name="TotalAmount" Type="Double" />
                                                <asp:Parameter Name="Discount" Type="Double" />
                                                <asp:Parameter DbType="Date" Name="CreateDate" />
                                                <asp:Parameter Name="PaidAmount" Type="Double" />
                                                <asp:Parameter Name="Due" Type="Double" />
                                                <asp:Parameter Name="NumberOfPayment" Type="Int32" />
                                                <asp:Parameter Name="IsPaid" Type="Int32" />
                                                <asp:Parameter DbType="Date" Name="LastPaidDate" />
                                                <asp:Parameter Name="InvoiceID" Type="Int32" />
                                            </UpdateParameters>
                                        </asp:SqlDataSource>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                    <asp:GridView ID="OthersInvoiceGridView" CssClass="mGrid" runat="server" AutoGenerateColumns="False" DataKeyNames="InvoiceID" DataSourceID="OthersInvoiceSQL">
                        <Columns>
                            <asp:BoundField DataField="Invoice_SN" HeaderText="SN" SortExpression="Invoice_SN" />
                            <asp:BoundField DataField="IssuDate" DataFormatString="{0:d MMM yyyy}" HeaderText="Issue Date" SortExpression="IssuDate" />
                            <asp:BoundField DataField="EndDate" DataFormatString="{0:d MMM yyyy}" HeaderText="End Date" SortExpression="EndDate" />
                            <asp:BoundField DataField="Invoice_For" HeaderText="Invoice For" SortExpression="Invoice_For" />
                            <asp:BoundField DataField="TotalAmount" HeaderText="Amount" SortExpression="TotalAmount" />
                            <asp:BoundField DataField="Discount" HeaderText="Discount" SortExpression="Discount" />
                            <asp:BoundField DataField="CreateDate" DataFormatString="{0:d MMM yyyy}" HeaderText="Create Date" SortExpression="CreateDate" />
                            <asp:BoundField DataField="Due" HeaderText="Due" ReadOnly="True" SortExpression="Due" />
                            <asp:BoundField DataField="PaidAmount" HeaderText="Paid" SortExpression="PaidAmount" />
                            <asp:BoundField DataField="LastPaidDate" DataFormatString="{0:d MMM yyyy}" HeaderText="Paid Date" SortExpression="LastPaidDate" />
                            <asp:CommandField ShowDeleteButton="True" />
                        </Columns>
                    </asp:GridView>
                </ContentTemplate>
            </asp:UpdatePanel>
        </div>
    </div>


    <div class="modal fade" id="CategoryModal" tabindex="-1" role="dialog" aria-labelledby="myModalLabel" aria-hidden="true">
        <div class="modal-dialog" role="document">
            <div class="modal-content">
                <div class="modal-header">
                    <div class="title">Category</div>
                    <button type="button" class="close" data-dismiss="modal" aria-label="Close"><span aria-hidden="true">&times;</span></button>
                </div>
                <div class="modal-body">
                    <asp:UpdatePanel ID="UpdatePanel1" runat="server">
                        <ContentTemplate>
                            <div class="form-inline">
                                <div class="form-group">
                                    <asp:TextBox placeholder="Category" ID="Category_TextBox" CssClass="form-control" runat="server"></asp:TextBox>
                                </div>
                                <div class="form-group">
                                    <asp:Button ID="CategoryButton" runat="server" Text="Add" CssClass="btn btn-blue" OnClick="CategoryButton_Click" />
                                </div>
                            </div>

                            <asp:GridView ID="CategoryGridView" CssClass="mGrid" runat="server" AutoGenerateColumns="False" DataKeyNames="InvoiceCategoryID" DataSourceID="InvoiceCategorySQL">
                                <Columns>
                                    <asp:CommandField ShowEditButton="True" />
                                    <asp:BoundField DataField="InvoiceCategory" HeaderText="Invoice Category" SortExpression="InvoiceCategory" />
                                    <asp:BoundField DataField="Insert_Date" ReadOnly="true" DataFormatString="{0:d MMM yyyy}" HeaderText="Date" SortExpression="Insert_Date" />
                                    <asp:CommandField ShowDeleteButton="True" />
                                </Columns>
                            </asp:GridView>

                            <asp:SqlDataSource ID="InvoiceCategorySQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" DeleteCommand="DELETE FROM [AAP_Invoice_Category] WHERE [InvoiceCategoryID] = @InvoiceCategoryID" InsertCommand="IF NOT EXISTS (SELECT * FROM AAP_Invoice_Category WHERE InvoiceCategory = @InvoiceCategory)
INSERT INTO AAP_Invoice_Category(RegistrationID, InvoiceCategory) VALUES (@RegistrationID, @InvoiceCategory)"
                                SelectCommand="SELECT * FROM [AAP_Invoice_Category]" UpdateCommand="IF NOT EXISTS (SELECT * FROM AAP_Invoice_Category WHERE InvoiceCategory = @InvoiceCategory AND InvoiceCategoryID &lt;&gt; @InvoiceCategoryID)
UPDATE AAP_Invoice_Category SET InvoiceCategory = @InvoiceCategory WHERE (InvoiceCategoryID = @InvoiceCategoryID)">
                                <DeleteParameters>
                                    <asp:Parameter Name="InvoiceCategoryID" Type="Int32" />
                                </DeleteParameters>
                                <InsertParameters>
                                    <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" Type="Int32" />
                                    <asp:ControlParameter ControlID="Category_TextBox" Name="InvoiceCategory" PropertyName="Text" Type="String" />
                                </InsertParameters>
                                <UpdateParameters>
                                    <asp:Parameter Name="InvoiceCategory" Type="String" />
                                    <asp:Parameter Name="InvoiceCategoryID" Type="Int32" />
                                </UpdateParameters>
                            </asp:SqlDataSource>
                        </ContentTemplate>
                    </asp:UpdatePanel>
                </div>
            </div>
        </div>
    </div>

    <div class="modal fade" id="myModal" tabindex="-1" role="dialog" aria-labelledby="myModalLabel" aria-hidden="true">
        <div class="modal-dialog modal-lg" role="document">
            <div class="modal-content">
                <div class="modal-header">
                    <div class="title">
                        <asp:UpdatePanel ID="UpdatePanel6" runat="server">
                            <ContentTemplate>
                                <asp:Label ID="Institution_Label" runat="server"></asp:Label>
                            </ContentTemplate>
                        </asp:UpdatePanel>
                    </div>
                    <button type="button" class="close" data-dismiss="modal" aria-label="Close"><span aria-hidden="true">&times;</span></button>
                </div>
                <div class="modal-body">
                    <asp:UpdatePanel ID="UpdatePanel2" runat="server">
                        <ContentTemplate>
                            <asp:Repeater ID="Details_Repeater" runat="server" DataSourceID="DetailsSQL">
                                <ItemTemplate>
                                    <asp:HiddenField ID="EducationYearIDHF" runat="server" Value='<%#Eval("EducationYearID") %>' />
                                    <asp:HiddenField ID="SchoolIDHF" runat="server" Value='<%#Eval("SchoolID") %>' />

                                    <h4>
                                        <span class="badge badge-primary">Year: <%#Eval("EducationYear") %>
                                        </span>
                                        <span class="badge badge-primary">Total: <%#Eval("Total_Count") %>
                                        </span>
                                        <span class="badge badge-primary">Active: <%#Eval("Total_Active") %>
                                        </span>
                                        <span class="badge badge-primary">Rejt. Countable: <%#Eval("Total_Re_Countable") %>
                                        </span>
                                        <span class="badge badge-primary">Rejt. Uncountable: <%#Eval("Total_Re_Uncountable") %>
                                        </span>
                                    </h4>

                                    <asp:GridView ID="DetailsGridView" CssClass="mGrid" runat="server" AutoGenerateColumns="False" DataSourceID="ClassDetailsSQL">
                                        <Columns>
                                            <asp:BoundField DataField="Class" HeaderText="Class" SortExpression="Class" />
                                            <asp:BoundField DataField="StudentCount" HeaderText="Student Count" ReadOnly="True" SortExpression="StudentCount" />
                                            <asp:BoundField DataField="Active_Student" HeaderText="Active Student" SortExpression="Active_Student" />
                                            <asp:BoundField DataField="Reject_Countable" HeaderText="Reject Countable" SortExpression="Reject_Countable" />
                                            <asp:BoundField DataField="Reject_Uncountable" HeaderText="Reject Uncountable" SortExpression="Reject_Uncountable" />
                                        </Columns>
                                    </asp:GridView>
                                    <asp:SqlDataSource ID="ClassDetailsSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT CreateClass.Class, AAP_StudentClass_Count_Monthly.StudentCount, AAP_StudentClass_Count_Monthly.Active_Student, AAP_StudentClass_Count_Monthly.Reject_Countable, 
                         AAP_StudentClass_Count_Monthly.Reject_Uncountable
FROM            AAP_StudentClass_Count_Monthly INNER JOIN
                         CreateClass ON AAP_StudentClass_Count_Monthly.ClassID = CreateClass.ClassID
WHERE        (AAP_StudentClass_Count_Monthly.SchoolID = @SchoolID) AND (FORMAT(AAP_StudentClass_Count_Monthly.Month, 'MMM yyyy') = @Month) AND (AAP_StudentClass_Count_Monthly.EducationYearID = @EducationYearID)">
                                        <SelectParameters>
                                            <asp:ControlParameter ControlID="SchoolIDHF" PropertyName="Value" Name="SchoolID" />
                                            <asp:ControlParameter ControlID="Month_DropDownList" Name="Month" PropertyName="SelectedValue" />
                                            <asp:ControlParameter ControlID="EducationYearIDHF" PropertyName="Value" Name="EducationYearID" />
                                        </SelectParameters>
                                    </asp:SqlDataSource>
                                </ItemTemplate>
                            </asp:Repeater>
                            <asp:SqlDataSource ID="DetailsSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT        AAP_StudentClass_Count_Monthly.SchoolID, Education_Year.EducationYearID, Education_Year.EducationYear, SUM(AAP_StudentClass_Count_Monthly.StudentCount) AS Total_Count, 
                         SUM(AAP_StudentClass_Count_Monthly.Active_Student) AS Total_Active, SUM(AAP_StudentClass_Count_Monthly.Reject_Countable) AS Total_Re_Countable, SUM(AAP_StudentClass_Count_Monthly.Reject_Uncountable) 
                         AS Total_Re_Uncountable
FROM            AAP_StudentClass_Count_Monthly INNER JOIN
                         Education_Year ON AAP_StudentClass_Count_Monthly.EducationYearID = Education_Year.EducationYearID
WHERE        (AAP_StudentClass_Count_Monthly.SchoolID = @SchoolID) AND (FORMAT(AAP_StudentClass_Count_Monthly.Month, 'MMM yyyy') = @Month)
GROUP BY AAP_StudentClass_Count_Monthly.SchoolID, AAP_StudentClass_Count_Monthly.Month, Education_Year.EducationYear, Education_Year.EducationYearID">
                                <SelectParameters>
                                    <asp:Parameter Name="SchoolID" />
                                    <asp:ControlParameter ControlID="Month_DropDownList" Name="Month" PropertyName="SelectedValue" />
                                </SelectParameters>
                            </asp:SqlDataSource>
                        </ContentTemplate>
                    </asp:UpdatePanel>
                </div>
            </div>
        </div>
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

    <script>
        function openModal() {
            $('#myModal').modal('show');
        }

        $(function () {
            // Initialize Select2 for Institution dropdown
            initializeSelect2();
            
            $('.datepicker').datepicker({
                format: 'dd M yyyy',
                todayBtn: "linked",
                todayHighlight: true,
                autoclose: true
            });

            //SMS Checkbox
            $("[id*=AllSMS_CheckBox]").on("click", function () {
                var a = $(this), b = $(this).closest("table");
                $("[id*=SMS_CheckBox]", b).each(function () {
                    a.is(":checked") ? ($(this).attr("checked", "checked"), $("td", $(this).closest("tr")).addClass("selected")) : ($(this).removeAttr("checked"), $("td", $(this).closest("tr")).removeClass("selected"));
                });
            });

            $("[id*=SMS_CheckBox]").on("click", function () {
                var a = $(this).closest("table"), b = $("[id*=AllSMS_CheckBox]", a);
                $(this).is(":checked") ? ($("td", $(this).closest("tr")).addClass("selected"), $("[id*=SMS_CheckBox]", a).length == $("[id*=SMS_CheckBox]:checked", a).length && b.attr("checked", "checked")) : ($("td", $(this).closest("tr")).removeClass("selected"), b.removeAttr("checked"));
            });
        });

        // Function to initialize Select2
        function initializeSelect2() {
            // Destroy existing Select2 if any
            if ($('.select2-dropdown').hasClass('select2-hidden-accessible')) {
                $('.select2-dropdown').select2('destroy');
            }
            
            // Initialize Select2 with search
            $('.select2-dropdown').select2({
                placeholder: "Search by ID or Name...",
                allowClear: true,
                width: '100%',
                matcher: function(params, data) {
                    // If there are no search terms, return all data
                    if ($.trim(params.term) === '') {
                        return data;
                    }

                    // Skip the default option
                    if (data.id === '0') {
                        return null;
                    }

                    // `params.term` is the user's search term
                    var term = params.term.toLowerCase();
                    var text = data.text.toLowerCase();

                    // Check if the search term matches
                    if (text.indexOf(term) > -1) {
                        return data;
                    }

                    // Return null if no match
                    return null;
                }
            });
        }

        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function (a, b) {
            // Re-initialize Select2 after UpdatePanel postback
            initializeSelect2();
            
            //Service charge Checkbox
            $("[id*=AllCheckBox]").on("click", function () {
                var a = $(this), b = $(this).closest("table");
                $("[id*=Invoice_CheckBox]", b).each(function () {
                    a.is(":checked") ? ($(this).attr("checked", "checked"), $("td", $(this).closest("tr")).addClass("selected")) : ($(this).removeAttr("checked"), $("td", $(this).closest("tr")).removeClass("selected"));
                });
            });

            $("[id*=Invoice_CheckBox]").on("click", function () {
                var a = $(this).closest("table"), b = $("[id*=AllCheckBox]", a);
                $(this).is(":checked") ? ($("td", $(this).closest("tr")).addClass("selected"), $("[id*=Invoice_CheckBox]", a).length == $("[id*=Invoice_CheckBox]:checked", a).length && b.attr("checked", "checked")) : ($("td", $(this).closest("tr")).removeClass("selected"), b.removeAttr("checked"));
            });

            //SMS Checkbox
            $("[id*=AllSMS_CheckBox]").on("click", function () {
                var a = $(this), b = $(this).closest("table");
                $("[id*=SMS_CheckBox]", b).each(function () {
                    a.is(":checked") ? ($(this).attr("checked", "checked"), $("td", $(this).closest("tr")).addClass("selected")) : ($(this).removeAttr("checked"), $("td", $(this).closest("tr")).removeClass("selected"));
                });
            });

            $("[id*=SMS_CheckBox]").on("click", function () {
                var a = $(this).closest("table"), b = $("[id*=AllCheckBox]", a);
                $(this).is(":checked") ? ($("td", $(this).closest("tr")).addClass("selected"), $("[id*=SMS_CheckBox]", a).length == $("[id*=SMS_CheckBox]:checked", a).length && b.attr("checked", "checked")) : ($("td", $(this).closest("tr")).removeClass("selected"), b.removeAttr("checked"));
            });

            $('.mGrid tr').each(function () {
                if ($(this).find('.IS_ServiceActive').val() === "False") {
                    $(this).addClass("Invaid_Ins");
                }
            });


            $('.datepicker').datepicker({
                format: 'dd M yyyy',
                todayBtn: "linked",
                todayHighlight: true,
                autoclose: true
            });
        });

        function isNumberKey(a) { a = a.which ? a.which : event.keyCode; return 46 != a && 31 < a && (48 > a || 57 < a) ? !1 : !0 };
    </script>
</asp:Content>
