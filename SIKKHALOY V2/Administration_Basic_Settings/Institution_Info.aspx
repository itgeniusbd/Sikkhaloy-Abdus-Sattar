<%@ Page Title="Institution Info" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Institution_Info.aspx.cs" Inherits="EDUCATION.COM.Administration_Basic_Settings.Institution_Info" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .mGrid input[type=text] {
            border: 1px solid #c4c4c4;
            border-radius: 4px;
            box-shadow: 0 0 8px #d9d9d9;
            font-size: 13px;
            padding: 5px;
            width: 100%;
        }

        .mGrid td {
            text-align: left;
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <h3>Institution Info</h3>


    <div class="form-group d-print-none">
        <h4>Payment Button SMS Active & Deactive</h4>
        <asp:RadioButtonList ID="rbSendSMS" RepeatColumns="2" RepeatDirection="Horizontal " runat="server" Visible="true" Width="180" AutoPostBack="true" OnSelectedIndexChanged="rbSendSMS_SelectedIndexChanged">
            <asp:ListItem Enabled="True" Text="Active" Value="1" />
            <asp:ListItem Enabled="True" Text="Deactive" Value="0" />
        </asp:RadioButtonList>
    </div>

    <asp:SqlDataSource ID="SmsSettingSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EduConnectionString %>" SelectCommand="SELECT TOP 1 PAY_Buttton_SMS_Enable_Disable FROM Account Where SchoolID=@SchoolID " UpdateCommand="UPDATE Account SET PAY_Buttton_SMS_Enable_Disable = @PAY_Buttton_SMS_Enable_Disable Where SchoolID=@SchoolID">
    <SelectParameters>
        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" Type="Int32" />
    </SelectParameters>
    <UpdateParameters>       
        <asp:ControlParameter ControlID="rbSendSMS" Name="PAY_Buttton_SMS_Enable_Disable" PropertyName="SelectedValue" />
        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" Type="Int32" />
    </UpdateParameters>
</asp:SqlDataSource>
               

    <asp:FormView ID="PImgFormView" runat="server" DataKeyNames="SchoolID" DataSourceID="ImgSQL">
        <ItemTemplate>
            <img alt="No Logo" src="/Handeler/SchoolLogo.ashx?SLogo=<%#Eval("SchoolID") %>" style="height: 60px" />
        </ItemTemplate>
    </asp:FormView>
    <asp:SqlDataSource ID="ImgSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
        SelectCommand="SELECT * FROM SchoolInfo WHERE (SchoolID = @SchoolID)">
        <SelectParameters>
            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
        </SelectParameters>
    </asp:SqlDataSource>

    <asp:DetailsView ID="InstitutionInfoDetailsView" DefaultMode="Edit" runat="server" AutoGenerateRows="False" DataKeyNames="SchoolID" DataSourceID="InstitutionInfoSQL" CssClass="mGrid" OnItemUpdated="InstitutionInfoDetailsView_ItemUpdated">
        <AlternatingRowStyle CssClass="alt" />
        <Fields>
            <asp:BoundField DataField="SchoolName" HeaderText="Institution Name" SortExpression="SchoolName" />
            <asp:BoundField DataField="Institution_Dialog" HeaderText="Institution Dialog" SortExpression="Institution_Dialog" />

            <asp:BoundField DataField="Established" HeaderText="Established" SortExpression="Established" />
            <asp:BoundField DataField="Principal" HeaderText="Principal" SortExpression="Principal" />
            <asp:BoundField DataField="AcadamicStaff" HeaderText="Acadamic Staff" SortExpression="AcadamicStaff" />
            <asp:BoundField DataField="Students" HeaderText="Students" SortExpression="Students" />
            <asp:BoundField DataField="Address" HeaderText="Address" SortExpression="Address" />
            <asp:BoundField DataField="City" HeaderText="City" SortExpression="City" />
            <asp:BoundField DataField="State" HeaderText="State" SortExpression="State" />
            <asp:BoundField DataField="LocalArea" HeaderText="Local Area" SortExpression="LocalArea" />
            <asp:BoundField DataField="PostalCode" HeaderText="Postal Code" SortExpression="PostalCode" />
            <asp:BoundField DataField="Phone" HeaderText="Phone" SortExpression="Phone" />
            <asp:BoundField DataField="Email" HeaderText="Email" SortExpression="Email" />
            <asp:BoundField DataField="Website" HeaderText="Website" SortExpression="Website" />
            <asp:TemplateField HeaderText="Logo">
                <EditItemTemplate>
                    <asp:FileUpload ID="LogoFileUpload" runat="server" />
                </EditItemTemplate>
            </asp:TemplateField>
            <asp:TemplateField ShowHeader="False">
                <EditItemTemplate>
                    <asp:Button ID="LinkButton1" CssClass="btn btn-success" runat="server" CausesValidation="True" CommandName="Update" Text="Update"></asp:Button>
                    <asp:Button ID="LinkButton2" runat="server" CssClass="btn btn-danger" CausesValidation="False" CommandName="Cancel" Text="Cancel"></asp:Button>
                </EditItemTemplate>
            </asp:TemplateField>
        </Fields>
        <RowStyle CssClass="RowStyle" />
    </asp:DetailsView>
    <asp:SqlDataSource ID="InstitutionInfoSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
        SelectCommand="SELECT *  FROM SchoolInfo WHERE (SchoolID = @SchoolID)"
        UpdateCommand="UPDATE SchoolInfo SET SchoolName = @SchoolName, Established = @Established, Principal = @Principal, AcadamicStaff = @AcadamicStaff, Students = @Students, Address = @Address, City = @City, State = @State, LocalArea = @LocalArea, PostalCode = @PostalCode, Phone = @Phone, Email = @Email, Website = @Website, Institution_Dialog = @Institution_Dialog WHERE (SchoolID = @SchoolID)">
        <SelectParameters>
            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
        </SelectParameters>
        <UpdateParameters>
            <asp:Parameter Name="SchoolName" Type="String" />
            <asp:Parameter Name="Established" Type="String" />
            <asp:Parameter Name="Principal" Type="String" />
            <asp:Parameter Name="AcadamicStaff" Type="String" />
            <asp:Parameter Name="Students" Type="String" />
            <asp:Parameter Name="Address" Type="String" />
            <asp:Parameter Name="City" Type="String" />
            <asp:Parameter Name="State" Type="String" />
            <asp:Parameter Name="LocalArea" Type="String" />
            <asp:Parameter Name="PostalCode" Type="String" />
            <asp:Parameter Name="Phone" Type="String" />
            <asp:Parameter Name="Email" Type="String" />
            <asp:Parameter Name="Website" Type="String" />
            <asp:Parameter Name="SchoolID" Type="Int32" />
            <asp:Parameter Name="Institution_Dialog" />
        </UpdateParameters>

    </asp:SqlDataSource>
</asp:Content>
