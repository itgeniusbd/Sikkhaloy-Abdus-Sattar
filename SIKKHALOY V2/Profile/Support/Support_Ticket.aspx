<%@ Page Title="Support Ticket" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Support_Ticket.aspx.cs" Inherits="EDUCATION.COM.Profile.Support.Support_Ticket" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <h3>Support Ticket</h3>
    <div class="form-inline">
        <div class="form-group">
            <asp:DropDownList ID="TitleDropDownList" CssClass="form-control" runat="server" AppendDataBoundItems="True" DataSourceID="TitleSQL" DataTextField="Support_Title" DataValueField="SupportTitleID">
                <asp:ListItem Value="0">[ SELECT SUBJECT ]</asp:ListItem>
            </asp:DropDownList>
            <asp:SqlDataSource ID="TitleSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT * FROM [Public_Support_Title]"></asp:SqlDataSource>
            <asp:RequiredFieldValidator ControlToValidate="TitleDropDownList" InitialValue="0" CssClass="EroorStar" ValidationGroup="S" ID="RequiredFieldValidator1" runat="server" ErrorMessage="*"></asp:RequiredFieldValidator>
        </div>
        <div class="form-group">
            <asp:TextBox ID="MessageTextBox" Rows="1" placeholder="Message" TextMode="MultiLine" runat="server" CssClass="form-control"></asp:TextBox>
             <asp:RequiredFieldValidator ControlToValidate="MessageTextBox" CssClass="EroorStar" ValidationGroup="S" ID="RequiredFieldValidator2" runat="server" ErrorMessage="*"></asp:RequiredFieldValidator>
        </div>
        <div class="form-group">
            <asp:Button ID="SubmitButton" ValidationGroup="S" runat="server" Text="Submit" CssClass="btn btn-primary" OnClick="SubmitButton_Click" />
        </div>
    </div>
    <asp:GridView ID="SupportGridView"  CssClass="mGrid" runat="server" AutoGenerateColumns="False" DataKeyNames="SupportID" DataSourceID="SupportSQL">
        <Columns>
            <asp:BoundField DataField="Support_Title" HeaderText="Subject" SortExpression="Support_Title" />
            <asp:BoundField DataField="Message" HeaderText="Message" SortExpression="Message" />
            <asp:BoundField DataField="Sent_Date" DataFormatString="{0:d MMM yyyy}" HeaderText="Date" SortExpression="Sent_Date" />
        </Columns>
    </asp:GridView>
    <asp:SqlDataSource ID="SupportSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" InsertCommand="INSERT INTO Public_Support(SchoolID, RegistrationID, SupportTitleID, Message) VALUES (@SchoolID, @RegistrationID, @SupportTitleID, @Message)" SelectCommand="SELECT Public_Support.SupportID, Public_Support.Message, Public_Support.Sent_Date, Public_Support_Title.Support_Title FROM Public_Support INNER JOIN Public_Support_Title ON Public_Support.SupportTitleID = Public_Support_Title.SupportTitleID WHERE (Public_Support.SchoolID = @SchoolID) AND (Public_Support.RegistrationID = @RegistrationID)">
        <InsertParameters>
            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" Type="Int32" />
            <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" Type="Int32" />
            <asp:ControlParameter ControlID="TitleDropDownList" Name="SupportTitleID" PropertyName="SelectedValue" Type="Int32" />
            <asp:ControlParameter ControlID="MessageTextBox" Name="Message" PropertyName="Text" Type="String" />
        </InsertParameters>
        <SelectParameters>
            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
            <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" />
        </SelectParameters>
    </asp:SqlDataSource>
</asp:Content>
