<%@ Page Title="Add Institution In Reference" Language="C#" MasterPageFile="~/Basic_Authority.Master" AutoEventWireup="true" CodeBehind="School_Reference.aspx.cs" Inherits="EDUCATION.COM.Authority.Reference.School_Reference" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">

</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">

        <h3>Add Institution In Reference</h3>

        <asp:GridView ID="SchoolGridView" runat="server" AutoGenerateColumns="False" CssClass="mGrid" DataKeyNames="SchoolID" DataSourceID="SchoolSQL">
            <Columns>
                <asp:TemplateField>
                    <ItemTemplate>
                        <asp:CheckBox ID="SelectCheckBox" runat="server" Text=" " />
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:BoundField DataField="School_SN" HeaderText="School_SN" SortExpression="School_SN" />
                <asp:BoundField DataField="SchoolName" HeaderText="SchoolName" SortExpression="SchoolName" />
                <asp:BoundField DataField="Phone" HeaderText="Phone" SortExpression="Phone" />
                <asp:BoundField DataField="Email" HeaderText="Email" SortExpression="Email" />
                <asp:BoundField DataField="Per_Student_Rate" HeaderText="Per_Student_Rate" SortExpression="Per_Student_Rate" />
            </Columns>
        </asp:GridView>
        <asp:SqlDataSource ID="SchoolSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT SchoolID, SchoolName, SchoolLogo, Established, Principal, AcadamicStaff, Students, Address, City, State, LocalArea, PostalCode, Phone, Email, Website, UserName, Validation, Date, School_SN, Per_Student_Rate FROM SchoolInfo WHERE (SchoolID NOT IN (SELECT SchoolID FROM AAP_Reference_School))"></asp:SqlDataSource>
        <br />
        <table>
            <tr>
                <td>Reference</td>
                <td>Percentage</td>
                <td>SignUp Date</td>
                <td>End Reference Date</td>
                <td>&nbsp;</td>
            </tr>
            <tr>
                <td>
                    <asp:DropDownList ID="RefaranceDropDownList" runat="server" CssClass="form-control" DataSourceID="RefarenceSQL" DataTextField="Reference_Name" DataValueField="ReferenceID" Width="200px" AppendDataBoundItems="True" AutoPostBack="True">
                        <asp:ListItem Value="0">Select</asp:ListItem>
                    </asp:DropDownList>
                    <asp:SqlDataSource ID="RefarenceSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT * FROM [AAP_Reference]"></asp:SqlDataSource>
                </td>
                <td>
                    <asp:TextBox ID="PercentageTextBox" runat="server" CssClass="form-control"></asp:TextBox>
                </td>
                <td>
                    <asp:TextBox ID="SignUp_DateTextBox" runat="server" CssClass="form-control"></asp:TextBox>
                </td>
                <td>
                    <asp:TextBox ID="End_Reference_DateTextBox" runat="server" CssClass="form-control"></asp:TextBox>
                </td>
                <td>
        <asp:Button ID="SubmitButton" runat="server" CssClass="btn btn-primary" OnClick="SubmitButton_Click" Text="Submit" />
                </td>
            </tr>
        </table>
        <br />
        <asp:SqlDataSource ID="AAP_Reference_SchoolSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" DeleteCommand="DELETE FROM [AAP_Reference_School] WHERE [Reference_School_ID] = @Reference_School_ID" InsertCommand="INSERT INTO [AAP_Reference_School] ([SchoolID], [ReferenceID], [Percentage], [School_SignUp_Date], [End_Reference_Date]) VALUES (@SchoolID, @ReferenceID, @Percentage, @School_SignUp_Date, @End_Reference_Date)" SelectCommand="SELECT AAP_Reference_School.Percentage, AAP_Reference_School.School_SignUp_Date, AAP_Reference_School.End_Reference_Date, SchoolInfo.SchoolName, SchoolInfo.Address, SchoolInfo.Phone, SchoolInfo.Email, SchoolInfo.City, SchoolInfo.Principal, SchoolInfo.Per_Student_Rate, AAP_Reference_School.Reference_School_ID FROM AAP_Reference_School INNER JOIN SchoolInfo ON AAP_Reference_School.SchoolID = SchoolInfo.SchoolID WHERE (AAP_Reference_School.ReferenceID = @ReferenceID)" UpdateCommand="UPDATE AAP_Reference_School SET Percentage = @Percentage, School_SignUp_Date = @School_SignUp_Date, End_Reference_Date = @End_Reference_Date WHERE (Reference_School_ID = @Reference_School_ID)">
            <DeleteParameters>
                <asp:Parameter Name="Reference_School_ID" Type="Int32" />
            </DeleteParameters>
            <InsertParameters>
                <asp:Parameter Name="SchoolID" Type="Int32" />
                <asp:ControlParameter ControlID="RefaranceDropDownList" Name="ReferenceID" PropertyName="SelectedValue" Type="Int32" />
                <asp:ControlParameter ControlID="PercentageTextBox" Name="Percentage" PropertyName="Text" Type="Double" />
                <asp:ControlParameter ControlID="SignUp_DateTextBox" DbType="Date" Name="School_SignUp_Date" PropertyName="Text" />
                <asp:ControlParameter ControlID="End_Reference_DateTextBox" DbType="Date" Name="End_Reference_Date" PropertyName="Text" />
            </InsertParameters>
            <SelectParameters>
                <asp:ControlParameter ControlID="RefaranceDropDownList" Name="ReferenceID" PropertyName="SelectedValue" />
            </SelectParameters>
            <UpdateParameters>
                <asp:Parameter Name="Percentage" />
                <asp:Parameter Name="School_SignUp_Date" />
                <asp:Parameter Name="End_Reference_Date" />
                <asp:Parameter Name="Reference_School_ID" />
            </UpdateParameters>
        </asp:SqlDataSource>
        <br />
        <asp:GridView ID="RefaranceSchoolGridView" runat="server" AutoGenerateColumns="False" CssClass="mGrid" DataSourceID="AAP_Reference_SchoolSQL" DataKeyNames="Reference_School_ID">
            <Columns>
                <asp:BoundField DataField="SchoolName" HeaderText="SchoolName" SortExpression="SchoolName" />
                <asp:BoundField DataField="Per_Student_Rate" HeaderText="Per_Student_Rate" SortExpression="Per_Student_Rate" />
                <asp:BoundField DataField="Percentage" HeaderText="Percentage" SortExpression="Percentage" />
                <asp:BoundField DataField="School_SignUp_Date" DataFormatString="{0:d MMM yyyy}" HeaderText="School_SignUp_Date" SortExpression="School_SignUp_Date" />
                <asp:BoundField DataField="End_Reference_Date" DataFormatString="{0:d MMM yyyy}" HeaderText="End_Reference_Date" SortExpression="End_Reference_Date" />
                <asp:CommandField ShowDeleteButton="True" />
            </Columns>
        </asp:GridView>


</asp:Content>
