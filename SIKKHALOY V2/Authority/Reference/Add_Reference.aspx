<%@ Page Title="Add Reference" Language="C#" MasterPageFile="~/Basic_Authority.Master" AutoEventWireup="true" CodeBehind="Add_Reference.aspx.cs" Inherits="EDUCATION.COM.Authority.Reference.Add_Reference" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">

    <h3>Reference person</h3>

    <table>
        <tr>
            <td>Reference Name</td>
            <td>Reference Phone	</td>
            <td>Marketing Start Date</td>
            <td>Marketing End Date</td>
            <td>Address</td>
            <td>&nbsp;</td>
        </tr>
        <tr>
            <td>
                <asp:TextBox ID="Reference_NameTextBox" runat="server" CssClass="form-control"></asp:TextBox></td>
            <td>
                <asp:TextBox ID="Reference_PhoneTextBox" runat="server" CssClass="form-control"></asp:TextBox></td>
            <td>
                <asp:TextBox ID="Marketing_StartDateTextBox" runat="server" CssClass="form-control"></asp:TextBox></td>
            <td>
                <asp:TextBox ID="Marketing_EndDateTextBox" runat="server" CssClass="form-control"></asp:TextBox></td>
            <td>
                <asp:TextBox ID="AddressTextBox" runat="server" CssClass="form-control"></asp:TextBox></td>
            <td>
                <asp:Button ID="SubmitButton" runat="server" CssClass="btn btn-primary" Text="Submit" OnClick="SubmitButton_Click" />
            </td>
        </tr>
    </table>

    <asp:SqlDataSource ID="ReferenceSQl" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" InsertCommand="INSERT INTO AAP_Reference(Reference_Name, Reference_Phone, Address, Marketing_StartDate, Marketing_EndDate) VALUES (@Reference_Name, @Reference_Phone, @Address, @Marketing_StartDate, @Marketing_EndDate)" SelectCommand="SELECT * FROM [AAP_Reference]" UpdateCommand="UPDATE AAP_Reference SET Reference_Name = @Reference_Name, Reference_Phone = @Reference_Phone, Address = @Address, Marketing_StartDate = @Marketing_StartDate, Marketing_EndDate = @Marketing_EndDate WHERE (ReferenceID = @ReferenceID)">
        <InsertParameters>
            <asp:ControlParameter ControlID="Reference_NameTextBox" Name="Reference_Name" PropertyName="Text" Type="String" />
            <asp:ControlParameter ControlID="Reference_PhoneTextBox" Name="Reference_Phone" PropertyName="Text" Type="String" />
            <asp:ControlParameter ControlID="AddressTextBox" Name="Address" PropertyName="Text" Type="String" />
            <asp:ControlParameter ControlID="Marketing_StartDateTextBox" DbType="Date" Name="Marketing_StartDate" PropertyName="Text" />
            <asp:ControlParameter ControlID="Marketing_EndDateTextBox" DbType="Date" Name="Marketing_EndDate" PropertyName="Text" />
        </InsertParameters>
        <UpdateParameters>
            <asp:Parameter Name="Reference_Name" Type="String" />
            <asp:Parameter Name="Reference_Phone" Type="String" />
            <asp:Parameter Name="Address" Type="String" />
            <asp:Parameter DbType="Date" Name="Marketing_StartDate" />
            <asp:Parameter DbType="Date" Name="Marketing_EndDate" />
            <asp:Parameter Name="ReferenceID" Type="Int32" />
        </UpdateParameters>
    </asp:SqlDataSource>
    <asp:GridView ID="ReferenceGridView" runat="server" AutoGenerateColumns="False" CssClass="mGrid" DataKeyNames="ReferenceID" DataSourceID="ReferenceSQl">
        <Columns>
            <asp:CommandField HeaderText="Details" ShowSelectButton="True" />
            <asp:BoundField DataField="Reference_SN" HeaderText="SN" SortExpression="Reference_SN" />
            <asp:BoundField DataField="Reference_Name" HeaderText="Name" SortExpression="Reference_Name" />
            <asp:BoundField DataField="Reference_Phone" HeaderText="Phone" SortExpression="Reference_Phone" />
            <asp:BoundField DataField="Address" HeaderText="Address" SortExpression="Address" />
            <asp:BoundField DataField="Marketing_StartDate" HeaderText="Start Date" SortExpression="Marketing_StartDate" DataFormatString="{0:d MMM yyyy}" />
            <asp:BoundField DataField="Marketing_EndDate" HeaderText="End Date" SortExpression="Marketing_EndDate" DataFormatString="{0:d MMM yyyy}" />
            <asp:BoundField DataField="TotalAmount" HeaderText="Total" SortExpression="TotalAmount" />
            <asp:BoundField DataField="PaidAmount" HeaderText="Paid" SortExpression="PaidAmount" />
            <asp:BoundField DataField="Due" HeaderText="Due" ReadOnly="True" SortExpression="Due" />
            <asp:BoundField DataField="PaymentStatus" HeaderText="P.Status" ReadOnly="True" SortExpression="PaymentStatus" />
            <asp:CommandField ShowEditButton="True" />
        </Columns>
        <SelectedRowStyle CssClass="Selected" />
    </asp:GridView>

    <br />
    <br />

    <asp:GridView ID="PayorderGridView" runat="server" AutoGenerateColumns="False" CssClass="mGrid" DataKeyNames="SchoolID,InvoiceID,Reference_PayOrderID,ReferenceID" DataSourceID="DetailsSQL">
        <Columns>
            <asp:CommandField ShowSelectButton="True" />
            <asp:BoundField DataField="Invoice_SN" HeaderText="SN" SortExpression="Invoice_SN" />
            <asp:BoundField DataField="Invoice_For" HeaderText="Invoice For" SortExpression="Invoice_For" />
            <asp:BoundField DataField="SchoolName" HeaderText="School" SortExpression="SchoolName" />
            <asp:BoundField DataField="PaymentStatus" HeaderText="School Paid Status" SortExpression="PaymentStatus" />
            <asp:BoundField DataField="Amount" HeaderText="Amount" SortExpression="Amount" />
            <asp:BoundField DataField="PayOrderDate" DataFormatString="{0:d MMM yyyy}" HeaderText="Payorder Date" SortExpression="PayOrderDate" />
        </Columns>
        <SelectedRowStyle CssClass="Selected" />
    </asp:GridView>

    <asp:SqlDataSource ID="DetailsSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT AAP_Reference_PayOrder.Reference_PayOrderID, AAP_Reference_PayOrder.SchoolID, AAP_Reference_PayOrder.Reference_School_ID, AAP_Reference_PayOrder.ReferenceID, AAP_Reference_PayOrder.InvoiceID, AAP_Reference_PayOrder.Amount, AAP_Reference_PayOrder.PayOrderDate, SchoolInfo.SchoolName, AAP_Invoice.Invoice_For, AAP_Invoice.Invoice_SN, AAP_Invoice.PaymentStatus FROM AAP_Reference_PayOrder INNER JOIN AAP_Invoice ON AAP_Reference_PayOrder.InvoiceID = AAP_Invoice.InvoiceID INNER JOIN SchoolInfo ON AAP_Reference_PayOrder.SchoolID = SchoolInfo.SchoolID WHERE (AAP_Reference_PayOrder.ReferenceID = @ReferenceID)">
        <SelectParameters>
            <asp:ControlParameter ControlID="ReferenceGridView" Name="ReferenceID" PropertyName="SelectedValue" />
        </SelectParameters>
    </asp:SqlDataSource>

    <table>
        <tr>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
        </tr>
        <tr>
            <td>Amount</td>
            <td>Paid Date</td>
            <td>Paid By</td>
            <td>Payment Method</td>
            <td>&nbsp;</td>
        </tr>
        <tr>
            <td>
                <asp:TextBox ID="PaidAmountTextBox" runat="server" CssClass="form-control"></asp:TextBox>
            </td>
            <td>
                <asp:TextBox ID="PaidDateTextBox" runat="server" CssClass="form-control"></asp:TextBox>
            </td>
            <td>
                <asp:TextBox ID="Paid_ByTextBox" runat="server" CssClass="form-control"></asp:TextBox>
            </td>
            <td>
                <asp:TextBox ID="Payment_MethodTextBox" runat="server" CssClass="form-control"></asp:TextBox>
            </td>
            <td>
                <asp:Button ID="PaidButton" runat="server" CssClass="btn btn-primary" OnClick="PaidButton_Click" Text="Pay" />
            </td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
            <td>
                <asp:SqlDataSource ID="Reference_PaymentRecordSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" InsertCommand="INSERT INTO [AAP_Reference_PaymentRecord] ([Reference_PayOrderID], [ReferenceID], [SchoolID], [InvoiceID], [Amount], [PaidDate], [Paid_By], [Payment_Method]) VALUES (@Reference_PayOrderID, @ReferenceID, @SchoolID, @InvoiceID, @Amount, @PaidDate, @Paid_By, @Payment_Method)" SelectCommand="SELECT * FROM [AAP_Reference_PaymentRecord]">
                    <InsertParameters>
                        <asp:ControlParameter ControlID="PayorderGridView" Name="SchoolID" PropertyName="SelectedDataKey[0]" Type="Int32" />
                        <asp:ControlParameter ControlID="PayorderGridView" Name="InvoiceID" PropertyName="SelectedDataKey[1]" Type="Int32" />
                        <asp:ControlParameter ControlID="PayorderGridView" Name="Reference_PayOrderID" PropertyName="SelectedDataKey[2]" Type="Int32" />
                        <asp:ControlParameter ControlID="PayorderGridView" Name="ReferenceID" PropertyName="SelectedDataKey[3]" Type="Int32" />
                        <asp:ControlParameter ControlID="PaidAmountTextBox" Name="Amount" PropertyName="Text" Type="Double" />
                        <asp:ControlParameter ControlID="PaidDateTextBox" DbType="Date" Name="PaidDate" PropertyName="Text" />
                        <asp:ControlParameter ControlID="Paid_ByTextBox" Name="Paid_By" PropertyName="Text" Type="String" />
                        <asp:ControlParameter ControlID="Payment_MethodTextBox" Name="Payment_Method" PropertyName="Text" Type="String" />
                    </InsertParameters>
                </asp:SqlDataSource>
            </td>
        </tr>
    </table>


    <asp:GridView ID="PaidRecordGridView" runat="server" AutoGenerateColumns="False" CssClass="mGrid" DataKeyNames="ReferencePaymentRecordID" DataSourceID="PRecordSQL">
        <Columns>
            <asp:BoundField DataField="Amount" HeaderText="Amount" SortExpression="Amount" />
            <asp:BoundField DataField="PaidDate" HeaderText="PaidDate" SortExpression="PaidDate" />
            <asp:BoundField DataField="Paid_By" HeaderText="Paid_By" SortExpression="Paid_By" />
            <asp:BoundField DataField="Payment_Method" HeaderText="Payment_Method" SortExpression="Payment_Method" />
        </Columns>
    </asp:GridView>
    <asp:SqlDataSource ID="PRecordSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT * FROM [AAP_Reference_PaymentRecord] WHERE ([Reference_PayOrderID] = @Reference_PayOrderID)">
        <SelectParameters>
            <asp:ControlParameter ControlID="PayorderGridView" Name="Reference_PayOrderID" PropertyName="SelectedDataKey[3]" Type="Int32" />
        </SelectParameters>
    </asp:SqlDataSource>

    <script>
        $(function () {
            $(".Datetime").datepick();
        })
    </script>
</asp:Content>
