<%@ Page Title="Concession details" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="All_Discount_Details.aspx.cs" Inherits="EDUCATION.COM.ACCOUNTS.Payment.All_Discount_Details" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
   <link href="CSS/All-Discount-List.css" rel="stylesheet" />
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">

   <h3>Concession details:</h3>

   <table class="NoPrint">
      <tr>
         <td>Class</td>
         <td>
            <asp:Label ID="GroupLabel" runat="server" Text="Group"></asp:Label>
         </td>
         <td>
            <asp:Label ID="SectionLabel" runat="server" Text="Section"></asp:Label>
         </td>
         <td>

            <asp:Label ID="ShiftLabel" runat="server" Text="Shift"></asp:Label>
         </td>
      </tr>
      <tr>
         <td>
            <asp:DropDownList ID="ClassDropDownList" runat="server" CssClass="form-control" AutoPostBack="True" DataSourceID="ClassNameSQL" DataTextField="Class" DataValueField="ClassID" OnSelectedIndexChanged="ClassDropDownList_SelectedIndexChanged" OnDataBound="ClassDropDownList_DataBound">
               <asp:ListItem Value="0">[ SELECT ]</asp:ListItem>
            </asp:DropDownList>
            <asp:SqlDataSource ID="ClassNameSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT DISTINCT Class, ClassID FROM CreateClass WHERE (SchoolID = @SchoolID)">
               <SelectParameters>
                  <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
               </SelectParameters>
            </asp:SqlDataSource>
         </td>
         <td>
            <asp:DropDownList ID="GroupDropDownList" runat="server" CssClass="form-control" AutoPostBack="True" DataSourceID="GroupSQL" DataTextField="SubjectGroup" DataValueField="SubjectGroupID" OnDataBound="GroupDropDownList_DataBound" OnSelectedIndexChanged="GroupDropDownList_SelectedIndexChanged">
            </asp:DropDownList>
            <asp:SqlDataSource ID="GroupSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT DISTINCT [Join].SubjectGroupID, CreateSubjectGroup.SubjectGroup FROM [Join] INNER JOIN CreateSubjectGroup ON [Join].SubjectGroupID = CreateSubjectGroup.SubjectGroupID WHERE ([Join].ClassID = @ClassID) AND ([Join].SectionID LIKE @SectionID) AND ([Join].ShiftID LIKE  @ShiftID) ">
               <SelectParameters>
                  <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                  <asp:ControlParameter ControlID="SectionDropDownList" Name="SectionID" PropertyName="SelectedValue" />
                  <asp:ControlParameter ControlID="ShiftDropDownList" Name="ShiftID" PropertyName="SelectedValue" />
               </SelectParameters>
            </asp:SqlDataSource>
         </td>
         <td>
            <asp:DropDownList ID="SectionDropDownList" runat="server" CssClass="form-control" DataSourceID="SectionSQL" DataTextField="Section" DataValueField="SectionID" AutoPostBack="True" OnDataBound="SectionDropDownList_DataBound" OnSelectedIndexChanged="SectionDropDownList_SelectedIndexChanged">
            </asp:DropDownList>
            <asp:SqlDataSource ID="SectionSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT DISTINCT [Join].SectionID, CreateSection.Section FROM [Join] INNER JOIN CreateSection ON [Join].SectionID = CreateSection.SectionID WHERE ([Join].ClassID = @ClassID) AND ([Join].SubjectGroupID LIKE @SubjectGroupID) AND ([Join].ShiftID LIKE @ShiftID) ">
               <SelectParameters>
                  <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                  <asp:ControlParameter ControlID="GroupDropDownList" Name="SubjectGroupID" PropertyName="SelectedValue" />
                  <asp:ControlParameter ControlID="ShiftDropDownList" Name="ShiftID" PropertyName="SelectedValue" />
               </SelectParameters>
            </asp:SqlDataSource>
         </td>
         <td>


            <asp:DropDownList ID="ShiftDropDownList" runat="server" AutoPostBack="True" CssClass="form-control" DataSourceID="ShiftSQL" DataTextField="Shift" DataValueField="ShiftID" OnDataBound="ShiftDropDownList_DataBound" OnSelectedIndexChanged="ShiftDropDownList_SelectedIndexChanged">
            </asp:DropDownList>
            <asp:SqlDataSource ID="ShiftSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT DISTINCT [Join].ShiftID, CreateShift.Shift FROM [Join] INNER JOIN CreateShift ON [Join].ShiftID = CreateShift.ShiftID WHERE ([Join].SubjectGroupID LIKE @SubjectGroupID) AND ([Join].SectionID LIKE  @SectionID) AND ([Join].ClassID = @ClassID)">
               <SelectParameters>
                  <asp:ControlParameter ControlID="GroupDropDownList" Name="SubjectGroupID" PropertyName="SelectedValue" />
                  <asp:ControlParameter ControlID="SectionDropDownList" Name="SectionID" PropertyName="SelectedValue" />
                  <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
               </SelectParameters>
            </asp:SqlDataSource>
         </td>
      </tr>
   </table>


   <%if (LPGridViewTooltip.Rows.Count > 0)
     { %>
   <div class="Title">Concession Fee For
      <label class="Class_Section"></label>
   </div>
   <label id="PGTLabel" class="Amt"></label>
   <asp:GridView ID="LPGridViewTooltip" runat="server" AutoGenerateColumns="False" DataSourceID="TooltipFeeSQL" CssClass="mGrid" PageSize="40" AllowSorting="True">
      <Columns>
         <asp:BoundField DataField="ID" HeaderText="ID" SortExpression="ID" />
         <asp:BoundField DataField="RollNo" HeaderText="Roll No" SortExpression="RollNo" />
         <asp:BoundField DataField="StudentsName" HeaderText="Name" SortExpression="StudentsName" />
         <asp:BoundField DataField="Role" HeaderText="Role" SortExpression="Role" />
         <asp:BoundField DataField="PayFor" HeaderText="Pay For" SortExpression="PayFor" />
         <asp:BoundField DataField="PreviousAmount" HeaderText="Prev.Amount" SortExpression="PreviousAmount" />
         <asp:TemplateField HeaderText="Post Amount" SortExpression="PostAmount">
            <ItemTemplate>
               <asp:Label ID="FeeConLabel" runat="server" Text='<%# Bind("PostAmount") %>'></asp:Label>
            </ItemTemplate>
         </asp:TemplateField>
         <asp:BoundField DataField="Reason" HeaderText="Reason" SortExpression="Reason" />
         <asp:BoundField DataField="Date" HeaderText="Date" SortExpression="Date" DataFormatString="{0:d MMM yyyy}" />
      </Columns>
      <PagerStyle CssClass="pgr" />
   </asp:GridView>
   <asp:SqlDataSource ID="TooltipFeeSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
      SelectCommand="SELECT Income_Discount_Record.Reason, Income_Discount_Record.PreviousAmount, Income_Discount_Record.PostAmount, Income_Discount_Record.Date, StudentsClass.RollNo, Student.ID, Student.StudentsName, CreateClass.Class, Income_Roles.Role, Income_PayOrder.PayFor FROM Income_Discount_Record INNER JOIN StudentsClass ON Income_Discount_Record.StudentClassID = StudentsClass.StudentClassID INNER JOIN Student ON StudentsClass.StudentID = Student.StudentID INNER JOIN CreateClass ON StudentsClass.ClassID = CreateClass.ClassID INNER JOIN Income_PayOrder ON Income_Discount_Record.PayOrderID = Income_PayOrder.PayOrderID INNER JOIN Income_Roles ON Income_PayOrder.RoleID = Income_Roles.RoleID WHERE (Student.Status = @Status) AND (Income_Discount_Record.EducationYearID = @EducationYearID) AND (StudentsClass.SchoolID = @SchoolID) AND (StudentsClass.ClassID = @ClassID) AND (StudentsClass.SectionID LIKE @SectionID) AND (StudentsClass.ShiftID LIKE @ShiftID) AND (StudentsClass.SubjectGroupID LIKE @SubjectGroupID) ORDER BY Income_PayOrder.ClassID">
      <SelectParameters>
         <asp:Parameter DefaultValue="Active" Name="Status" />
         <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
         <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
         <asp:ControlParameter ControlID="ClassDropDownList" DefaultValue="" Name="ClassID" PropertyName="SelectedValue" />
         <asp:ControlParameter ControlID="SectionDropDownList" Name="SectionID" PropertyName="SelectedValue" />
         <asp:ControlParameter ControlID="ShiftDropDownList" Name="ShiftID" PropertyName="SelectedValue" />
         <asp:ControlParameter ControlID="GroupDropDownList" Name="SubjectGroupID" PropertyName="SelectedValue" />
      </SelectParameters>
   </asp:SqlDataSource>
   <%} %>
   <%if (DLFGridView.Rows.Count > 0)
     { %>
   <div class="Title">Concession Late fee For
      <label class="Class_Section"></label>
   </div>
   <label id="LFeeConLabel" class="Amt"></label>
   <asp:GridView ID="DLFGridView" runat="server" AutoGenerateColumns="False" DataSourceID="ToltipLateFSQL" CssClass="mGrid" AllowSorting="True">
      <Columns>
         <asp:BoundField DataField="ID" HeaderText="ID" SortExpression="ID" />
         <asp:BoundField DataField="RollNo" HeaderText="Roll No" SortExpression="RollNo" />
         <asp:BoundField DataField="StudentsName" HeaderText="Name" SortExpression="StudentsName" />
         <asp:BoundField DataField="Role" HeaderText="Role" SortExpression="Role" />
         <asp:BoundField DataField="PayFor" HeaderText="PayFor" SortExpression="PayFor" />
         <asp:BoundField DataField="PreviousAmount" HeaderText="Prev.Amount" SortExpression="PreviousAmount" />
         <asp:TemplateField HeaderText="Post Amount" SortExpression="PostAmount">
            <ItemTemplate>
               <asp:Label ID="LateFeeConLabel" runat="server" Text='<%# Bind("PostAmount") %>'></asp:Label>
            </ItemTemplate>
         </asp:TemplateField>
         <asp:BoundField DataField="Reason" HeaderText="Reason" SortExpression="Reason" />
         <asp:BoundField DataField="Date" HeaderText="Date" SortExpression="Date" DataFormatString="{0:d MMM yyyy}" />
      </Columns>
      <PagerStyle CssClass="pgr" />
   </asp:GridView>
   <asp:SqlDataSource ID="ToltipLateFSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
      SelectCommand="SELECT Income_LateFee_Discount_Record.Reason, Income_LateFee_Discount_Record.PreviousAmount, Income_LateFee_Discount_Record.PostAmount, Income_LateFee_Discount_Record.Date, StudentsClass.RollNo, Student.ID, Student.StudentsName, CreateClass.Class, Income_LateFee_Discount_Record.EducationYearID, Income_Roles.Role, Income_PayOrder.PayFor FROM Income_LateFee_Discount_Record INNER JOIN StudentsClass ON Income_LateFee_Discount_Record.StudentClassID = StudentsClass.StudentClassID INNER JOIN CreateClass ON StudentsClass.ClassID = CreateClass.ClassID INNER JOIN Student ON StudentsClass.StudentID = Student.StudentID INNER JOIN Income_PayOrder ON Income_LateFee_Discount_Record.PayOrderID = Income_PayOrder.PayOrderID INNER JOIN Income_Roles ON Income_PayOrder.RoleID = Income_Roles.RoleID WHERE (Student.Status = @Status) AND (Income_LateFee_Discount_Record.EducationYearID = @EducationYearID) AND (StudentsClass.SchoolID = @SchoolID) AND (StudentsClass.ClassID = @ClassID) AND (StudentsClass.SectionID LIKE @SectionID) AND (StudentsClass.ShiftID LIKE @ShiftID) AND (StudentsClass.SubjectGroupID LIKE @SubjectGroupID) ORDER BY Income_PayOrder.ClassID">
      <SelectParameters>
         <asp:Parameter DefaultValue="Active" Name="Status" />
         <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
         <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
         <asp:ControlParameter ControlID="ClassDropDownList" DefaultValue="" Name="ClassID" PropertyName="SelectedValue" />
         <asp:ControlParameter ControlID="SectionDropDownList" Name="SectionID" PropertyName="SelectedValue" />
         <asp:ControlParameter ControlID="ShiftDropDownList" Name="ShiftID" PropertyName="SelectedValue" />
         <asp:ControlParameter ControlID="GroupDropDownList" Name="SubjectGroupID" PropertyName="SelectedValue" />
      </SelectParameters>
   </asp:SqlDataSource>
   <%} %>

   <%if (LateFeeChargeGridView.Rows.Count > 0)
     { %>
   <div class="Title">Charge Late fee For
      <label class="Class_Section"></label>
   </div>

    <label id="ChargeLFeeLabel" class="Amt"></label>
   <asp:GridView ID="LateFeeChargeGridView" runat="server" AutoGenerateColumns="False" DataSourceID="TooltipFeeChargeSQL" CssClass="mGrid" AllowSorting="True">
      <Columns>
         <asp:BoundField DataField="ID" HeaderText="ID" SortExpression="ID" />
         <asp:BoundField DataField="RollNo" HeaderText="Roll No" SortExpression="RollNo" />
         <asp:BoundField DataField="StudentsName" HeaderText="Name" SortExpression="StudentsName" />
         <asp:BoundField DataField="Role" HeaderText="Role" SortExpression="Role" />
         <asp:BoundField DataField="PayFor" HeaderText="PayFor" SortExpression="PayFor" />
         <asp:BoundField DataField="PreviousAmount" HeaderText="Prev.Amount" SortExpression="PreviousAmount" />
         <asp:TemplateField HeaderText="Post Amount" SortExpression="PostAmount">
            <ItemTemplate>
               <asp:Label ID="ChargeLFLabel" runat="server" Text='<%# Bind("PostAmount") %>'></asp:Label>
            </ItemTemplate>
         </asp:TemplateField>
         <asp:BoundField DataField="Date" HeaderText="Date" SortExpression="Date" DataFormatString="{0:d MMM yyyy}" />
      </Columns>
      <PagerStyle CssClass="pgr" />
   </asp:GridView>
   <asp:SqlDataSource ID="TooltipFeeChargeSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
      SelectCommand="SELECT Income_LateFee_Change_Record.PreviousAmount, Income_LateFee_Change_Record.PostAmount, Income_LateFee_Change_Record.Date, CreateClass.Class, Student.ID, Student.StudentsName, StudentsClass.RollNo, Income_LateFee_Change_Record.EducationYearID, Income_Roles.Role, Income_PayOrder.PayFor FROM Income_PayOrder INNER JOIN Income_LateFee_Change_Record INNER JOIN Student ON Income_LateFee_Change_Record.StudentID = Student.StudentID ON Income_PayOrder.PayOrderID = Income_LateFee_Change_Record.PayOrderID INNER JOIN CreateClass INNER JOIN StudentsClass ON CreateClass.ClassID = StudentsClass.ClassID ON Student.StudentID = StudentsClass.StudentID INNER JOIN Income_Roles ON Income_PayOrder.RoleID = Income_Roles.RoleID WHERE (Student.Status = @Status) AND (Income_LateFee_Change_Record.EducationYearID = @EducationYearID) AND (StudentsClass.SchoolID = @SchoolID) AND (StudentsClass.ClassID = @ClassID) AND (StudentsClass.SectionID LIKE @SectionID) AND (StudentsClass.ShiftID LIKE @ShiftID) AND (StudentsClass.SubjectGroupID LIKE @SubjectGroupID) ORDER BY Income_PayOrder.ClassID">
      <SelectParameters>
         <asp:Parameter DefaultValue="Active" Name="Status" />
         <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
         <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
         <asp:ControlParameter ControlID="ClassDropDownList" DefaultValue="" Name="ClassID" PropertyName="SelectedValue" />
         <asp:ControlParameter ControlID="SectionDropDownList" Name="SectionID" PropertyName="SelectedValue" />
         <asp:ControlParameter ControlID="ShiftDropDownList" Name="ShiftID" PropertyName="SelectedValue" />
         <asp:ControlParameter ControlID="GroupDropDownList" Name="SubjectGroupID" PropertyName="SelectedValue" />
      </SelectParameters>
   </asp:SqlDataSource>
   <%} %>

   <br /><input id="Print_Con" class="btn btn-primary" type="submit" value="Print" onclick="window.print();" />


   <script>
      //GridView Is Empty
      if (!$('[id*=LPGridViewTooltip] tr').length) {
         $("#Print_Con").hide();
      }


      var Class = "";
      if ($('[id*=ClassDropDownList] :selected').index() > 0) {
         Class = " Class: " + $('[id*=ClassDropDownList] :selected').text() + ".";
      }

      var Group = "";
      if ($('[id*=GroupDropDownList] :selected').index() > 0) {
         Group = " Group: " + $('[id*=GroupDropDownList] :selected').text() + ".";
      }

      var Section = "";
      if ($('[id*=SectionDropDownList] :selected').index() > 0) {
         Section = " Section: " + $('[id*=SectionDropDownList] :selected').text() + ".";
      }

      var Shift = "";
      if ($('[id*=ShiftDropDownList] :selected').index() > 0) {
         Shift = " Shift: " + $('[id*=ShiftDropDownList] :selected').text() + ".";
      }


      $(".Class_Section").text(Class + Group + Section + Shift);


      //Fee ConGrand Total
      var FeeTotal = 0;
      $("[id*=FeeConLabel]").each(function () { FeeTotal = FeeTotal + parseFloat($(this).text()) });
      $("#PGTLabel").text("Total: "+FeeTotal + " Tk");

      var LateConTotal = 0;
      $("[id*=LateFeeConLabel]").each(function () { LateConTotal = LateConTotal + parseFloat($(this).text()) });
      $("#LFeeConLabel").text("Total: " + LateConTotal + " Tk");

     
      var ChargeTotal = 0;
      $("[id*=ChargeLFLabel]").each(function () { ChargeTotal = ChargeTotal + parseFloat($(this).text()) });
      $("# ChargeLFeeLabel").text("Total: " + ChargeTotal + " Tk");
   </script>
</asp:Content>
