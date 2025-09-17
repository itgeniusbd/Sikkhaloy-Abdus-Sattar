<%@ Page Title="Testimonial" Language="C#" MasterPageFile="~/Basic_Authority.Master" AutoEventWireup="true" CodeBehind="Testimonial.aspx.cs" Inherits="EDUCATION.COM.Authority.Institutions.Testimonial" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <h3>Testimonial</h3>
    <asp:GridView ID="TestimonialGridView" CssClass="mGrid" runat="server" AutoGenerateColumns="False" DataKeyNames="TestimonialID" DataSourceID="TestimonialSQL">
        <Columns>
            <asp:BoundField DataField="Show_SN" HeaderText="SN" SortExpression="Show_SN" />
            <asp:BoundField DataField="SchoolName" ReadOnly="true" HeaderText="Institution" SortExpression="SchoolName" />
            <asp:TemplateField HeaderText="Testimonial" SortExpression="Testimonial_Text">
                <EditItemTemplate>
                    <asp:TextBox ID="TextBox1" TextMode="MultiLine" Rows="3" CssClass="form-control" runat="server" Text='<%# Bind("Testimonial_Text") %>'></asp:TextBox>
                </EditItemTemplate>
                <ItemTemplate>
                    <asp:Label ID="Label1" runat="server" Text='<%# Bind("Testimonial_Text") %>'></asp:Label>
                </ItemTemplate>
            </asp:TemplateField>
            <asp:BoundField ReadOnly="true" DataField="Insert_Date" DataFormatString="{0:d MMM yyyy}" HeaderText="Date" SortExpression="Insert_Date" />
            <asp:TemplateField HeaderText="Show" SortExpression="Is_Show">
                <ItemTemplate>
                    <asp:CheckBox ID="ShowCheckBox" Text=" " runat="server" Checked='<%# Bind("Is_Show") %>' AutoPostBack="true" OnCheckedChanged="ShowCheckBox_CheckedChanged"/>
                </ItemTemplate>
            </asp:TemplateField>
            <asp:CommandField ShowEditButton="True" />
        </Columns>
    </asp:GridView>
    <asp:SqlDataSource ID="TestimonialSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" DeleteCommand="DELETE FROM [Public_Testimonial] WHERE [TestimonialID] = @TestimonialID" InsertCommand="INSERT INTO [Public_Testimonial] ([RegistrationID], [SchoolID], [Testimonial_Text], [Is_Show], [Show_SN], [Insert_Date]) VALUES (@RegistrationID, @SchoolID, @Testimonial_Text, @Is_Show, @Show_SN, @Insert_Date)" SelectCommand="SELECT Public_Testimonial.TestimonialID, Public_Testimonial.Testimonial_Text, Public_Testimonial.Is_Show, Public_Testimonial.Show_SN, SchoolInfo.SchoolName, Public_Testimonial.Insert_Date FROM Public_Testimonial INNER JOIN SchoolInfo ON Public_Testimonial.SchoolID = SchoolInfo.SchoolID ORDER BY Public_Testimonial.Show_SN" UpdateCommand="UPDATE Public_Testimonial SET Testimonial_Text = @Testimonial_Text, Show_SN = @Show_SN WHERE (TestimonialID = @TestimonialID)">
        <DeleteParameters>
            <asp:Parameter Name="TestimonialID" Type="Int32" />
        </DeleteParameters>
        <InsertParameters>
            <asp:Parameter Name="RegistrationID" Type="Int32" />
            <asp:Parameter Name="SchoolID" Type="Int32" />
            <asp:Parameter Name="Testimonial_Text" Type="String" />
            <asp:Parameter Name="Is_Show" Type="Boolean" />
            <asp:Parameter Name="Show_SN" Type="Int32" />
            <asp:Parameter DbType="Date" Name="Insert_Date" />
        </InsertParameters>
        <UpdateParameters>
            <asp:Parameter Name="Testimonial_Text" Type="String" />
            <asp:Parameter Name="Show_SN" Type="Int32" />
            <asp:Parameter Name="TestimonialID" Type="Int32" />
        </UpdateParameters>
    </asp:SqlDataSource>
    <asp:SqlDataSource ID="ShowUpdateSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT * FROM [Public_Testimonial]" UpdateCommand="UPDATE Public_Testimonial SET Is_Show = @Is_Show WHERE (TestimonialID = @TestimonialID)">
        <UpdateParameters>
            <asp:Parameter Name="Is_Show" Type="Boolean" />
            <asp:Parameter Name="TestimonialID" Type="Int32" />
        </UpdateParameters>
    </asp:SqlDataSource>
</asp:Content>
