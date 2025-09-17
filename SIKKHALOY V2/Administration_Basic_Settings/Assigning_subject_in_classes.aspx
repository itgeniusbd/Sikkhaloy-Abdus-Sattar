<%@ Page Title="Assign subjects in class" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Assigning_subject_in_classes.aspx.cs" Inherits="EDUCATION.COM.ADMINISTRATION_BASIC_SETTING.Assigning_subject_in_classes" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
   <h3>Assign subjects in class</h3>
   <div class="form-inline">
      <div class="form-group">
         <asp:DropDownList ID="ClassnameDropDownList" runat="server" CssClass="form-control" DataSourceID="ClassSQL" DataTextField="Class" DataValueField="ClassID" OnSelectedIndexChanged="ClassnameDropDownList_SelectedIndexChanged" AutoPostBack="True" AppendDataBoundItems="True">
            <asp:ListItem Value="0">[ SELECT CLASS]</asp:ListItem>
         </asp:DropDownList>
         <asp:SqlDataSource ID="ClassSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT * FROM [CreateClass] WHERE ([SchoolID] = @SchoolID) ORDER BY SN">
            <SelectParameters>
               <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
            </SelectParameters>
         </asp:SqlDataSource>
         <asp:RequiredFieldValidator ID="ClassValidator" runat="server" ControlToValidate="ClassnameDropDownList" CssClass="EroorSummer" ErrorMessage="*" InitialValue="0" ValidationGroup="2"></asp:RequiredFieldValidator>
      </div>

      <div class="form-group">
         <asp:DropDownList ID="GroupNameDropDownList" runat="server" CssClass="form-control" DataSourceID="GroupSQL" DataTextField="SubjectGroup" DataValueField="SubjectGroupID" OnSelectedIndexChanged="GroupNameDropDownList_SelectedIndexChanged" OnDataBound="GroupNameDropDownList_DataBound" AutoPostBack="True">
         </asp:DropDownList>
         <asp:SqlDataSource ID="GroupSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT * FROM [CreateSubjectGroup] WHERE ([ClassID] = @ClassID)">
            <SelectParameters>
               <asp:ControlParameter ControlID="ClassnameDropDownList" Name="ClassID" PropertyName="SelectedValue" Type="Int32" />
            </SelectParameters>
         </asp:SqlDataSource>
         <asp:RequiredFieldValidator ID="GroupValidator" runat="server" ControlToValidate="GroupNameDropDownList" CssClass="EroorSummer" ErrorMessage="*" InitialValue="0" ValidationGroup="2"></asp:RequiredFieldValidator>
      </div>
   </div>

   <div class="table-responsive">
      <asp:GridView ID="GroupGridView" runat="server" AutoGenerateColumns="False" DataKeyNames="SubjectID" DataSourceID="SubjectGroupSQL" CssClass="mGrid" PagerStyle-CssClass="pgr" PageSize="20">
         <Columns>
            <asp:BoundField DataField="SubjectName" HeaderText="Subjects" SortExpression="SubjectName">
               <HeaderStyle HorizontalAlign="Left" />
            </asp:BoundField>
            <asp:TemplateField ShowHeader="False" HeaderText="Select">
               <ItemTemplate>
                  <asp:CheckBox ID="SubjectCheckBox" runat="server" Text=" " />
               </ItemTemplate>
               <ItemStyle HorizontalAlign="Center" Width="30px" />
            </asp:TemplateField>

            <asp:TemplateField HeaderText="This Subject Is">
               <ItemTemplate>
                  <asp:RadioButtonList ID="SubjectTypeRadioButtonList" runat="server" RepeatDirection="Horizontal">
                     <asp:ListItem>Compulsory</asp:ListItem>
                     <asp:ListItem>Optional</asp:ListItem>
                  </asp:RadioButtonList>
               </ItemTemplate>
               <ItemStyle Width="175px" />
            </asp:TemplateField>
         </Columns>
         <EmptyDataTemplate>
            Empty
         </EmptyDataTemplate>
         <PagerStyle CssClass="pgr" />
         <SelectedRowStyle CssClass="Selected" />
      </asp:GridView>
   </div>
   <asp:SqlDataSource ID="SubjectGroupSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT * FROM Subject WHERE (SchoolID = @SchoolID) ORDER BY ISNULL(SN, 9999)">
      <SelectParameters>
         <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
      </SelectParameters>
   </asp:SqlDataSource>

   <br />

   <div class="form-inline">
      <div class="form-group">
         <asp:Button ID="AddSubJectGroupButton" runat="server" CssClass="btn btn-primary" OnClick="AddSubJectGroupButton_Click" Text="Assign subjects" ValidationGroup="2" />
      </div>
      <div class="form-group">
         <asp:Button ID="DeletButton" runat="server" CssClass="btn btn-primary" OnClick="DeletButton_Click" Text="Remove All Subjects From Class" />
      </div>
      <asp:ValidationSummary ID="ValidationSummary6" runat="server" CssClass="EroorSummer" DisplayMode="List" ValidationGroup="2" />
   </div>
   <asp:SqlDataSource ID="CreateGroupSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" InsertCommand="INSERT INTO [SubjectForGroup] ([SchoolID], [RegistrationID], [ClassID], [SubjectID], [SubjectGroupID], [SubjectType], [Date]) VALUES (@SchoolID, @RegistrationID, @ClassID, @SubjectID, @SubjectGroupID, @SubjectType, GETDATE())" SelectCommand="SELECT * FROM [SubjectForGroup]" DeleteCommand="DELETE FROM SubjectForGroup WHERE (ClassID = @ClassID) AND (SubjectGroupID = @SubjectGroupID)">
      <DeleteParameters>
         <asp:ControlParameter ControlID="ClassnameDropDownList" Name="ClassID" PropertyName="SelectedValue" />
         <asp:ControlParameter ControlID="GroupNameDropDownList" Name="SubjectGroupID" PropertyName="SelectedValue" />
      </DeleteParameters>
      <InsertParameters>
         <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
         <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" />
         <asp:ControlParameter ControlID="ClassnameDropDownList" Name="ClassID" PropertyName="SelectedValue" Type="Int32" />
         <asp:Parameter Name="SubjectID" Type="Int32" />
         <asp:ControlParameter ControlID="GroupNameDropDownList" Name="SubjectGroupID" PropertyName="SelectedValue" Type="String" />
         <asp:Parameter Name="SubjectType" Type="String" />
      </InsertParameters>
   </asp:SqlDataSource>

   <script>
       //Add or Remove CheckBox Selected Color
       $(function () {
           $("[id*=SubjectCheckBox]").on("click", function () {
               $(this).is(":checked") ? ($("td", $(this).closest("tr")).addClass("selected")) : ($("td", $(this).closest("tr")).removeClass("selected"), $($(this).closest("tr")).removeClass("selected"));
           });
       });
   </script>
</asp:Content>
