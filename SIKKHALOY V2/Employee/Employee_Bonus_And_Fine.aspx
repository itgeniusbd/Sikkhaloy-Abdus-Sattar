<%@ Page Title="Employee Bonus And Fine" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Employee_Bonus_And_Fine.aspx.cs" Inherits="EDUCATION.COM.Employee.Employee_Bonus_And_Fine" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <h3>Add Fine</h3>
    <div class="form-inline">
        <div class="form-group">
            <asp:TextBox ID="FineNameTextBox" runat="server" CssClass="form-control" placeholder="Fine Name"></asp:TextBox>
            <asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" ControlToValidate="FineNameTextBox" CssClass="EroorSummer" ErrorMessage="*" ValidationGroup="F"></asp:RequiredFieldValidator>
        </div>
        <div class="form-group">
            <asp:Button ID="AddFineButton" runat="server" CssClass="btn btn-primary" Text="Add Fine" OnClick="AddFineButton_Click" ValidationGroup="F" />
        </div>
    </div>

    <asp:GridView ID="FineGridView" runat="server" CssClass="mGrid" AutoGenerateColumns="False" DataKeyNames="FineID" DataSourceID="FineSQL">
        <Columns>
            <asp:CommandField ShowEditButton="True" />
            <asp:BoundField DataField="FineName" HeaderText="Fine Name" SortExpression="FineName" />
            <asp:BoundField DataField="CreateDate" HeaderText="Create Date" SortExpression="CreateDate" DataFormatString="{0:d MMM yyyy}" />
            <asp:CommandField ShowDeleteButton="True" />
        </Columns>
    </asp:GridView>
    <asp:SqlDataSource ID="FineSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" DeleteCommand="DELETE FROM [Employee_Fine] WHERE [FineID] = @FineID" InsertCommand="IF NOT EXISTS(SELECT * FROM Employee_Fine WHERE SchoolID = @SchoolID AND FineName = @FineName)
INSERT INTO [Employee_Fine] ([SchoolID], [RegistrationID], [FineName]) VALUES (@SchoolID, @RegistrationID, @FineName)"
        SelectCommand="SELECT FineID, SchoolID, RegistrationID, FineName, CreateDate FROM Employee_Fine WHERE (SchoolID = @SchoolID)" UpdateCommand="IF NOT EXISTS(SELECT * FROM Employee_Fine WHERE SchoolID = @SchoolID AND FineName = @FineName)
UPDATE Employee_Fine SET FineName = @FineName WHERE (FineID = @FineID)">
        <DeleteParameters>
            <asp:Parameter Name="FineID" Type="Int32" />
        </DeleteParameters>
        <InsertParameters>
            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" Type="Int32" />
            <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" Type="Int32" />
            <asp:ControlParameter ControlID="FineNameTextBox" Name="FineName" PropertyName="Text" Type="String" />
        </InsertParameters>
        <SelectParameters>
            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
        </SelectParameters>
        <UpdateParameters>
            <asp:Parameter Name="FineName" Type="String" />
            <asp:Parameter Name="FineID" Type="Int32" />
            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
        </UpdateParameters>
    </asp:SqlDataSource>


    <h3 class="mt-5">Add Bonus</h3>
    <div class="form-inline">
        <div class="form-group">
            <asp:TextBox ID="BonusTextBox" runat="server" CssClass="form-control" placeholder="Bonus Name"></asp:TextBox>
            <asp:RequiredFieldValidator ID="RequiredFieldValidator2" runat="server" ControlToValidate="BonusTextBox" CssClass="EroorSummer" ErrorMessage="*" ValidationGroup="B"></asp:RequiredFieldValidator>
        </div>
        <div class="form-group">
            <asp:Button ID="BonusButton" runat="server" CssClass="btn btn-primary" Text="Add Bonus" OnClick="BonusButton_Click" ValidationGroup="B" />
        </div>
    </div>

    <asp:GridView ID="BonusGridView" runat="server" CssClass="mGrid" AutoGenerateColumns="False" DataSourceID="BonusSQL" DataKeyNames="BonusID">
        <Columns>
            <asp:CommandField ShowEditButton="True" />
            <asp:BoundField DataField="BonusName" HeaderText="Bonus Name" SortExpression="BonusName" />
            <asp:BoundField DataField="CreateDate" HeaderText="Create Date" SortExpression="CreateDate" DataFormatString="{0:d MMM yyyy}" />
            <asp:CommandField ShowDeleteButton="True" />
        </Columns>
    </asp:GridView>
    <asp:SqlDataSource ID="BonusSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" DeleteCommand="DELETE FROM [Employee_Bonus] WHERE [BonusID] = @BonusID" InsertCommand="IF NOT EXISTS(SELECT * FROM Employee_Bonus WHERE SchoolID = @SchoolID AND BonusName = @BonusName)
INSERT INTO [Employee_Bonus] ([SchoolID], [RegistrationID], [BonusName]) VALUES (@SchoolID, @RegistrationID, @BonusName)"
        SelectCommand="SELECT * FROM [Employee_Bonus] WHERE (SchoolID = @SchoolID)" UpdateCommand="IF NOT EXISTS(SELECT * FROM Employee_Bonus WHERE SchoolID = @SchoolID AND BonusName = @BonusName)
UPDATE Employee_Bonus SET BonusName = @BonusName WHERE (BonusID = @BonusID)">
        <DeleteParameters>
            <asp:Parameter Name="BonusID" Type="Int32" />
        </DeleteParameters>
        <InsertParameters>
            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" Type="Int32" />
            <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" Type="Int32" />
            <asp:ControlParameter ControlID="BonusTextBox" Name="BonusName" PropertyName="Text" Type="String" />
        </InsertParameters>
        <SelectParameters>
            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
        </SelectParameters>
        <UpdateParameters>
            <asp:Parameter Name="BonusName" />
            <asp:Parameter Name="BonusID" Type="Int32" />
            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
        </UpdateParameters>
    </asp:SqlDataSource>
</asp:Content>
