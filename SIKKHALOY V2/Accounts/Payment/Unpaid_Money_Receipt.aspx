<%@ Page Title="Unpaid Money Receipt" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Unpaid_Money_Receipt.aspx.cs" Inherits="EDUCATION.COM.Accounts.Payment.Unpaid_Money_Receipt" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="/Admission/CSS/Student_List.css" rel="stylesheet" />
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <h3>Unpaid Money Receipt</h3>

    <div class="form-inline">
        <div class="form-group">
            <asp:TextBox ID="ReceiptTextBox" placeholder="Enter Receipt No" CssClass="form-control" runat="server"></asp:TextBox>
        </div>
        <div class="form-group">
            <asp:Button ID="FindButton" runat="server" Text="Find" CssClass="btn btn-primary" />
        </div>
    </div>

    <asp:FormView ID="StudentInfoFormView" runat="server" DataSourceID="StudentInfoSQL" DataKeyNames="SMSPhoneNo,StudentID" Width="100%">
        <ItemTemplate>
                    <div class="z-depth-1 mb-4 p-3">
                        <div class="d-flex flex-sm-row flex-column text-center text-sm-left">
                            <div class="p-image">
                                <img alt="No Image" src="/Handeler/Student_Photo.ashx?SID=<%#Eval("StudentImageID") %>" class="img-thumbnail rounded-circle z-depth-1" />
                            </div>
                            <div class="info">
                                <ul>
                                    <li><b>ID:</b>
                                        <%# Eval("ID") %>
                                    </li>
                                    <li>
                                        <b>Name:</b>
                                        <%# Eval("StudentsName") %>
                                    </li>
                                    <li class="alert-info">
                                        <b>Class:</b>
                                        <%# Eval("Class") %>
                                    </li>
                                    <li><b>Roll No:</b>
                                        <%# Eval("RollNo") %>
                                    </li>
                                    <li><b>Phone:</b>
                                        <%# Eval("SMSPhoneNo") %>
                                    </li>
                                </ul>
                            </div>
                        </div>
                    </div>
                </ItemTemplate>
    </asp:FormView>
    <asp:SqlDataSource ID="StudentInfoSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT Student.ID, Student.SMSPhoneNo, Student.StudentsName, CreateClass.Class, Student.StudentID, Income_MoneyReceipt.MoneyReceipt_SN, Income_MoneyReceipt.MoneyReceiptID, Student.StudentImageID, Student.FathersName, StudentsClass.RollNo FROM StudentsClass INNER JOIN CreateClass ON StudentsClass.ClassID = CreateClass.ClassID INNER JOIN Student ON StudentsClass.StudentID = Student.StudentID INNER JOIN Income_MoneyReceipt ON StudentsClass.StudentClassID = Income_MoneyReceipt.StudentClassID WHERE (Student.SchoolID = @SchoolID) AND (Income_MoneyReceipt.MoneyReceipt_SN = @MoneyReceipt_SN)">
        <SelectParameters>
            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
            <asp:ControlParameter ControlID="ReceiptTextBox" Name="MoneyReceipt_SN" PropertyName="Text" />
        </SelectParameters>
    </asp:SqlDataSource>

    <asp:GridView ID="PaidDetailsGridView" runat="server" AutoGenerateColumns="False" DataSourceID="PaidDetailsSQL" CssClass="mGrid" Font-Bold="False" RowStyle-CssClass="Rows">
        <Columns>
            <asp:BoundField DataField="PayFor" HeaderText="Pay For" />
            <asp:TemplateField HeaderText="Fee">
                <ItemTemplate>
                    <%# Eval("Role")%>
                    : 
                  <%# Eval("Amount") %>
                </ItemTemplate>
                <ItemStyle HorizontalAlign="Right" />
            </asp:TemplateField>
            <asp:TemplateField HeaderText="Paid">
                <ItemTemplate>
                    <%# Eval("PaidAmount") %>
                </ItemTemplate>
            </asp:TemplateField>
            <asp:TemplateField HeaderText="Due">
                <ItemTemplate>
                    <%# Eval("Due") %>
                </ItemTemplate>
            </asp:TemplateField>
        </Columns>
    </asp:GridView>
    <asp:SqlDataSource ID="PaidDetailsSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
        SelectCommand="SELECT Income_PaymentRecord.MoneyReceiptID, Income_Roles.Role, Income_PaymentRecord.PayFor, Income_PaymentRecord.PaidAmount, Income_PayOrder.Receivable_Amount AS Due, Income_PaymentRecord.PaidDate, Income_PayOrder.Amount, Income_MoneyReceipt.MoneyReceipt_SN FROM Income_PaymentRecord INNER JOIN Income_Roles ON Income_PaymentRecord.RoleID = Income_Roles.RoleID INNER JOIN Income_PayOrder ON Income_PaymentRecord.PayOrderID = Income_PayOrder.PayOrderID INNER JOIN Income_MoneyReceipt ON Income_PaymentRecord.MoneyReceiptID = Income_MoneyReceipt.MoneyReceiptID WHERE (Income_PaymentRecord.SchoolID = @SchoolID) AND (Income_MoneyReceipt.MoneyReceipt_SN = @MoneyReceipt_SN)">
        <SelectParameters>
            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
            <asp:ControlParameter ControlID="ReceiptTextBox" Name="MoneyReceipt_SN" PropertyName="Text" />
        </SelectParameters>
    </asp:SqlDataSource>

    <asp:FormView ID="ReceiptFormView" runat="server" DataSourceID="MoneyRSQL" Width="100%" DataKeyNames="TotalAmount">
        <ItemTemplate>
            <div class="mt-3">
                <h4>
                    <span class="badge badge-light m-2">Receipt No: <%# Eval("MoneyReceipt_SN") %> </span>
                    <span class="badge badge-light m-2">Total Amount: <%# Eval("TotalAmount") %> Tk</span>
                    <span class="badge badge-light m-2">Paid Date: <%# Eval("PaidDate","{0:d MMM yyyy (hh:mm tt)}") %></span>
                </h4>
            </div>
        </ItemTemplate>
    </asp:FormView>
    <asp:SqlDataSource ID="MoneyRSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
        SelectCommand="SELECT PaidDate, MoneyReceipt_SN, TotalAmount FROM Income_MoneyReceipt WHERE (SchoolID = @SchoolID) AND (MoneyReceipt_SN = @MoneyReceipt_SN)"
        DeleteCommand="BEGIN TRY
    BEGIN TRANSACTION
       UPDATE Income_PayOrder SET PaidAmount = Income_PayOrder.PaidAmount - Income_PaymentRecord.PaidAmount ,NumberOfPayment = 0, LastPaidDate = NULL 
        FROM Income_MoneyReceipt INNER JOIN Income_PaymentRecord ON Income_MoneyReceipt.MoneyReceiptID = Income_PaymentRecord.MoneyReceiptID INNER JOIN Income_PayOrder
        ON Income_PaymentRecord.PayOrderID = Income_PayOrder.PayOrderID
        WHERE (Income_MoneyReceipt.SchoolID = @SchoolID) AND (Income_MoneyReceipt.MoneyReceipt_SN = @MoneyReceipt_SN)

        DELETE FROM Income_PaymentRecord FROM Income_MoneyReceipt INNER JOIN Income_PaymentRecord ON Income_MoneyReceipt.MoneyReceiptID = Income_PaymentRecord.MoneyReceiptID 
        WHERE (Income_MoneyReceipt.SchoolID = @SchoolID) AND (Income_MoneyReceipt.MoneyReceipt_SN = @MoneyReceipt_SN)

        DELETE FROM Income_MoneyReceipt WHERE (SchoolID = @SchoolID) AND (MoneyReceipt_SN = @MoneyReceipt_SN)
        COMMIT
END TRY
BEGIN CATCH
    ROLLBACK
END CATCH">
        <DeleteParameters>
            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
            <asp:ControlParameter ControlID="ReceiptTextBox" Name="MoneyReceipt_SN" PropertyName="Text" />
        </DeleteParameters>
        <SelectParameters>
            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
            <asp:ControlParameter ControlID="ReceiptTextBox" Name="MoneyReceipt_SN" PropertyName="Text" />
        </SelectParameters>
    </asp:SqlDataSource>

    <asp:FormView ID="RByFormView" runat="server" DataSourceID="ReceivedBySQL" Width="100%">
        <ItemTemplate>
            <div class="RecvBy">
                Received By:
                <%# Eval("Name") %>
            </div>
        </ItemTemplate>
    </asp:FormView>
    <asp:SqlDataSource ID="ReceivedBySQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT Admin.FirstName + ' ' + Admin.LastName AS Name FROM Admin INNER JOIN Income_MoneyReceipt ON Admin.RegistrationID = Income_MoneyReceipt.RegistrationID WHERE (Income_MoneyReceipt.SchoolID = @SchoolID) AND (Income_MoneyReceipt.MoneyReceipt_SN = @MoneyReceipt_SN)">
        <SelectParameters>
            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
            <asp:ControlParameter ControlID="ReceiptTextBox" Name="MoneyReceipt_SN" PropertyName="Text" />
        </SelectParameters>
    </asp:SqlDataSource>

    <%if (PaidDetailsGridView.Rows.Count > 0)
        { %>
    <div class="mt-2">
        <asp:Button ID="DeleteReceiptButton" runat="server" OnClick="DeleteReceiptButton_Click" Text="Unpaid" CssClass="btn btn-info" />
    </div>
    <%} %>
</asp:Content>
