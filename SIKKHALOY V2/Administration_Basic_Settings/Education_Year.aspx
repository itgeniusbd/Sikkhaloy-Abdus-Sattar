<%@ Page Title="Session Year" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Education_Year.aspx.cs" Inherits="EDUCATION.COM.ADMINISTRATION_BASIC_SETTING.Education_Year" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <h3>Session Year</h3>

    <div class="form-inline">
        <div class="form-group">
            <asp:TextBox ID="YearTextBox" runat="server" CssClass="form-control" placeholder="Enter Session Year"></asp:TextBox>
            <asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" ControlToValidate="YearTextBox" CssClass="EroorSummer" ErrorMessage="*" ValidationGroup="1"></asp:RequiredFieldValidator>
        </div>

        <div class="form-group">
            <asp:TextBox ID="StartDateTextBox" runat="server" placeholder="Start Date" CssClass="form-control Datetime" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false"></asp:TextBox>
            <asp:RequiredFieldValidator ID="RequiredFieldValidator2" runat="server" ControlToValidate="StartDateTextBox" CssClass="EroorSummer" ErrorMessage="*" ValidationGroup="1"></asp:RequiredFieldValidator>
        </div>

        <div class="form-group">
            <asp:TextBox ID="EndDateTextBox" runat="server" placeholder="End Date" CssClass="form-control Datetime" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false"></asp:TextBox>
            <asp:RequiredFieldValidator ID="RequiredFieldValidator3" runat="server" ControlToValidate="EndDateTextBox" CssClass="EroorSummer" ErrorMessage="*" ValidationGroup="1"></asp:RequiredFieldValidator>
        </div>

        <div class="form-group">
            <asp:Button ID="SubmitButton" runat="server" OnClick="SubmitButton_Click" Text="Submit" CssClass="btn btn-primary" ValidationGroup="1" />
        </div>
    </div>

    <asp:GridView ID="EduYearGridView" runat="server" AutoGenerateColumns="False" DataKeyNames="EducationYearID,Check_ID" DataSourceID="EduYearSQL" OnRowDataBound="EduYearGridView_RowDataBound" CssClass="mGrid">
        <Columns>
            <asp:BoundField DataField="SN" HeaderText="SN" ReadOnly="True" SortExpression="SN" />
            <asp:TemplateField HeaderText="Session Year" SortExpression="EducationYear">
                <EditItemTemplate>
                    <asp:TextBox ID="TextBox1" runat="server" CssClass="form-control" Text='<%# Bind("EducationYear") %>'></asp:TextBox>
                    <asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" ControlToValidate="TextBox1" CssClass="EroorSummer" ErrorMessage="*" ValidationGroup="UP"></asp:RequiredFieldValidator>
                </EditItemTemplate>
                <ItemTemplate>
                    <asp:Label ID="Label1" runat="server" Text='<%# Bind("EducationYear") %>'></asp:Label>
                </ItemTemplate>
            </asp:TemplateField>
            <asp:TemplateField HeaderText="Start Date" SortExpression="StartDate">
                <EditItemTemplate>
                    <asp:TextBox ID="TextBox2" runat="server" CssClass="form-control Datetime" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false" Text='<%# Bind("StartDate","{0:d MMM yyyy}") %>' ValidationGroup="UP"></asp:TextBox>
                    <asp:RequiredFieldValidator ID="RequiredFieldValidator4" runat="server" ControlToValidate="TextBox2" CssClass="EroorSummer" ErrorMessage="*" ValidationGroup="UP"></asp:RequiredFieldValidator>
                </EditItemTemplate>
                <ItemTemplate>
                    <asp:Label ID="Label2" runat="server" Text='<%# Bind("StartDate", "{0:d MMM yyyy}") %>'></asp:Label>
                </ItemTemplate>
            </asp:TemplateField>
            <asp:TemplateField HeaderText="End Date" SortExpression="EndDate">
                <EditItemTemplate>
                    <asp:TextBox ID="TextBox3" runat="server" CssClass="form-control Datetime" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false" Text='<%# Bind("EndDate","{0:d MMM yyyy}") %>'></asp:TextBox>
                    <asp:RequiredFieldValidator ID="RequiredFieldValidator5" runat="server" ControlToValidate="TextBox3" CssClass="EroorSummer" ErrorMessage="*" ValidationGroup="UP"></asp:RequiredFieldValidator>
                </EditItemTemplate>
                <ItemTemplate>
                    <asp:Label ID="Label3" runat="server" Text='<%# Bind("EndDate", "{0:d MMM yyyy}") %>'></asp:Label>
                </ItemTemplate>
            </asp:TemplateField>
            <asp:TemplateField HeaderText="Status" SortExpression="Status">
                <ItemTemplate>
                    <asp:CheckBox ID="ActiveCheckBox" runat="server" AutoPostBack="True" OnCheckedChanged="CheckBox1_CheckedChanged" Text=" " />
                </ItemTemplate>
                <ItemStyle Width="50px" />
            </asp:TemplateField>
            <asp:TemplateField ShowHeader="False">
                <EditItemTemplate>
                    <asp:LinkButton ID="LinkButton1" runat="server" CausesValidation="True" CommandName="Update" Text="Update" ValidationGroup="UP"></asp:LinkButton>
                    &nbsp;<asp:LinkButton ID="LinkButton2" runat="server" CausesValidation="False" CommandName="Cancel" Text="Cancel"></asp:LinkButton>
                </EditItemTemplate>
                <ItemTemplate>
                    <asp:LinkButton ID="LinkButton1" runat="server" CausesValidation="False" CommandName="Edit" Text="Edit"></asp:LinkButton>
                </ItemTemplate>
                <ItemStyle Width="100px" />
            </asp:TemplateField>
            <asp:TemplateField ShowHeader="False">
                <ItemTemplate>
                    <asp:LinkButton ID="DeleteLinkButton" runat="server" CausesValidation="False" CommandName="Delete" Text="Delete" OnClientClick="return confirm('Are You sure want to delete?')"></asp:LinkButton>
                </ItemTemplate>
                <ItemStyle Width="50px" />
            </asp:TemplateField>
        </Columns>
    </asp:GridView>
    <asp:SqlDataSource ID="EduYearSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
        DeleteCommand="IF NOT EXISTS(SELECT * FROM  StudentsClass WHERE (EducationYearID = @EducationYearID) and  (SchoolID = @SchoolID))
BEGIN
IF NOT EXISTS(SELECT * FROM  Education_Year_User WHERE (EducationYearID = @EducationYearID) and  (SchoolID = @SchoolID))
BEGIN
DELETE FROM [Education_Year] WHERE (EducationYearID = @EducationYearID)
END
END"
        InsertCommand="IF NOT EXISTS(SELECT * FROM [Education_Year] WHERE (SchoolID = @SchoolID) and (EducationYear = @EducationYear))
BEGIN
INSERT INTO Education_Year(SchoolID, RegistrationID, EducationYear, Status, StartDate, EndDate,SN) VALUES (@SchoolID, @RegistrationID, @EducationYear, @Status, @StartDate, @EndDate,[dbo].[F_EducationYear_SN](@SchoolID))
END"
        SelectCommand="SELECT Education_Year.EducationYear, Education_Year.EducationYearID, Education_Year_User.RegistrationID, Education_Year_User.EducationYearID AS U_Edu_YearID, CASE WHEN Education_Year.EducationYearID = Education_Year_User.EducationYearID THEN 'TRUE' ELSE 'FALSE' END AS Check_ID, Education_Year.StartDate, Education_Year.EndDate, Education_Year.SN FROM Education_Year_User INNER JOIN Education_Year ON Education_Year_User.SchoolID = Education_Year.SchoolID WHERE (Education_Year_User.SchoolID = @SchoolID) AND (Education_Year_User.RegistrationID = @RegistrationID)"
        UpdateCommand="IF NOT EXISTS(SELECT * FROM [Education_Year] WHERE  SchoolID = @SchoolID AND EducationYear = @EducationYear) 
BEGIN
UPDATE Education_Year SET EducationYear = @EducationYear, StartDate = @StartDate, EndDate = @EndDate WHERE (EducationYearID = @EducationYearID)
END
ELSE
BEGIN
UPDATE Education_Year SET  StartDate = @StartDate, EndDate = @EndDate WHERE (EducationYearID = @EducationYearID)
END
">
        <DeleteParameters>
            <asp:Parameter Name="EducationYearID" Type="Int32" />
            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
        </DeleteParameters>
        <InsertParameters>
            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
            <asp:ControlParameter ControlID="YearTextBox" Name="EducationYear" PropertyName="Text" Type="String" />
            <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" />
            <asp:Parameter DefaultValue="False" Name="Status" Type="String" />
            <asp:ControlParameter ControlID="StartDateTextBox" Name="StartDate" PropertyName="Text" />
            <asp:ControlParameter ControlID="EndDateTextBox" Name="EndDate" PropertyName="Text" />
        </InsertParameters>
        <SelectParameters>
            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
            <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" />
        </SelectParameters>
        <UpdateParameters>
            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
            <asp:Parameter Name="EducationYearID" Type="Int32" />
            <asp:Parameter Name="EducationYear" />
            <asp:Parameter Name="StartDate" Type="DateTime" />
            <asp:Parameter Name="EndDate" Type="DateTime" />
        </UpdateParameters>
    </asp:SqlDataSource>

   <script>
      $(function () {
          $(".Datetime").datepicker({
              format: 'dd M yyyy',
              todayBtn: "linked",
              todayHighlight: true,
              autoclose: true
          });
      });
   </script>
</asp:Content>
