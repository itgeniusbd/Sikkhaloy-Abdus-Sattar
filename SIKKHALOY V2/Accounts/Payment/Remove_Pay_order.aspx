<%@ Page Title="Remove Pay Order" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Remove_Pay_order.aspx.cs" Inherits="EDUCATION.COM.ACCOUNTS.Payment.Remove_Pay_order" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="CSS/RemovePayorder.css" rel="stylesheet" />
    <style>
        .modal-body { max-height: 500px; overflow: auto; }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <h3>Remove Pay order from student</h3>

    <div class="form-inline">
        <div class="form-group">
            <asp:DropDownList ID="Session_DropDownList" CssClass="form-control" runat="server" DataSourceID="All_SessionSQL" DataTextField="EducationYear" DataValueField="EducationYearID" AutoPostBack="True">
            </asp:DropDownList>
            <asp:SqlDataSource ID="All_SessionSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT * FROM [Education_Year] WHERE ([SchoolID] = @SchoolID)">
                <SelectParameters>
                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" Type="Int32" />
                </SelectParameters>
            </asp:SqlDataSource>
        </div>
        <div class="form-group">
            <asp:DropDownList ID="ClassDropDownList" onfocus="SelectedItemCLR(this);" runat="server" CssClass="form-control" AppendDataBoundItems="True" AutoPostBack="True" DataSourceID="ClassNameSQL" DataTextField="Class" DataValueField="ClassID" OnSelectedIndexChanged="ClassDropDownList_SelectedIndexChanged">
                <asp:ListItem Value="0">[ Select All students or Class ]</asp:ListItem>
                <asp:ListItem Value="-1">All Students</asp:ListItem>
            </asp:DropDownList>
            <asp:SqlDataSource ID="ClassNameSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT * FROM [CreateClass] WHERE ([SchoolID] = @SchoolID) ORDER BY SN">
                <SelectParameters>
                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                </SelectParameters>
            </asp:SqlDataSource>
            <asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" ControlToValidate="ClassDropDownList" ErrorMessage="RequiredFieldValidator" ForeColor="#CC3300" InitialValue="0" ValidationGroup="a">!</asp:RequiredFieldValidator>
        </div>
        <div class="form-group S_Show" style="display: none">
            <asp:DropDownList ID="SectionDropDownList" runat="server" AutoPostBack="True" CssClass="form-control" DataSourceID="SectionSQL" DataTextField="Section" DataValueField="SectionID" OnDataBound="SectionDropDownList_DataBound">
            </asp:DropDownList>
            <asp:SqlDataSource ID="SectionSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT DISTINCT CreateSection.Section, StudentsClass.SectionID FROM StudentsClass INNER JOIN Income_PayOrder ON StudentsClass.StudentClassID = Income_PayOrder.StudentClassID INNER JOIN CreateSection ON StudentsClass.SectionID = CreateSection.SectionID WHERE (Income_PayOrder.Is_Active = 1) AND (StudentsClass.SchoolID = @SchoolID) AND (StudentsClass.EducationYearID = @EducationYearID) AND (StudentsClass.ClassID = @ClassID)">
                <SelectParameters>
                    <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                    <asp:ControlParameter ControlID="Session_DropDownList" Name="EducationYearID" PropertyName="SelectedValue" />
                </SelectParameters>
            </asp:SqlDataSource>
        </div>
        <div class="form-group">
            <asp:TextBox ID="IDTextBox" placeholder="Separate the ID by comma" runat="server" CssClass="form-control" TextMode="MultiLine" Height="34px"></asp:TextBox>
        </div>
        <div class="form-group">
            <asp:Button ID="Find_ID_Button" runat="server" CssClass="btn btn-primary" ValidationGroup="Sr" Text="Find Student" OnClick="Find_ID_Button_Click" />
        </div>
    </div>

    <div class="Overflow-hide Students">
        <div class="alert-success">
            Select Students and Select Role
         <asp:CustomValidator ID="CV1" runat="server" ClientValidationFunction="Validate" ErrorMessage="You do not select any student from student list." ForeColor="Red" ValidationGroup="A"> </asp:CustomValidator>
        </div>
        <asp:GridView ID="StudentsGridView" runat="server" AlternatingRowStyle-CssClass="alt" AutoGenerateColumns="False" CssClass="mGrid" PagerStyle-CssClass="pgr" PageSize="60"
            DataKeyNames="StudentID,StudentClassID" DataSourceID="ShowStudentClassSQL" AllowSorting="True">
            <AlternatingRowStyle CssClass="alt" />
            <RowStyle CssClass="RowStyle" />
            <Columns>
                <asp:TemplateField>
                    <HeaderTemplate>
                        <asp:CheckBox ID="AllIteamCheckBox" runat="server" Text="All" />
                    </HeaderTemplate>
                    <ItemTemplate>
                        <asp:CheckBox ID="SingleCheckBox" runat="server" Text=" " />
                    </ItemTemplate>
                    <ItemStyle Width="50px" />
                </asp:TemplateField>
                <asp:BoundField DataField="Class" HeaderText="Class" SortExpression="Class" />
                <asp:BoundField DataField="ID" HeaderText="ID" SortExpression="ID" />
                <asp:BoundField DataField="RollNo" HeaderText="Roll No" SortExpression="RollNo"></asp:BoundField>
                <asp:BoundField DataField="StudentsName" HeaderText="Name" SortExpression="StudentsName" />
                <asp:BoundField DataField="Status" HeaderText="Status" SortExpression="Status" />
            </Columns>

            <PagerStyle CssClass="pgr" />
            <SelectedRowStyle CssClass="Selected" />
        </asp:GridView>
        <asp:SqlDataSource ID="ShowStudentClassSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="IF(@IDs &lt;&gt; '')
BEGIN 
SELECT DISTINCT Student.StudentID, StudentsClass.StudentClassID, CreateClass.Class, StudentsClass.RollNo, Student.ID, Student.StudentsName, Student.Status FROM   StudentsClass INNER JOIN Student ON StudentsClass.StudentID = Student.StudentID INNER JOIN  CreateClass ON StudentsClass.ClassID = CreateClass.ClassID INNER JOIN  Income_PayOrder ON StudentsClass.StudentClassID = Income_PayOrder.StudentClassID WHERE (StudentsClass.SchoolID = @SchoolID) AND (Income_PayOrder.EducationYearID = @EducationYearID) and (Income_PayOrder.PaidAmount &lt;= 0) AND (Student.ID IN  (SELECT id FROM  dbo.In_Function_Parameter(@IDs))) ORDER BY StudentsClass.RollNo 
END
ELSE
BEGIN
SELECT DISTINCT Student.StudentID, StudentsClass.StudentClassID, CreateClass.Class, StudentsClass.RollNo, Student.ID, Student.StudentsName, Student.Status FROM   StudentsClass INNER JOIN Student ON StudentsClass.StudentID = Student.StudentID INNER JOIN  CreateClass ON StudentsClass.ClassID = CreateClass.ClassID INNER JOIN  Income_PayOrder ON StudentsClass.StudentClassID = Income_PayOrder.StudentClassID WHERE (StudentsClass.SchoolID = @SchoolID) AND (StudentsClass.ClassID = @ClassID) AND (Income_PayOrder.EducationYearID = @EducationYearID) AND (StudentsClass.SectionID LIKE @SectionID) AND  (Income_PayOrder.PaidAmount &lt;= 0) ORDER BY StudentsClass.RollNo 
END
"
            CancelSelectOnNullParameter="False">
            <SelectParameters>
                <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                <asp:ControlParameter ControlID="Session_DropDownList" Name="EducationYearID" PropertyName="SelectedValue" />
                <asp:ControlParameter ControlID="SectionDropDownList" Name="SectionID" PropertyName="SelectedValue" />
                <asp:ControlParameter ControlID="IDTextBox" Name="IDs" PropertyName="Text" />
                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
            </SelectParameters>
        </asp:SqlDataSource>
    </div>

    <div class="Roles">
        <div class="alert-info">
            Select role to find students pay order:
         <asp:CustomValidator ID="CV" runat="server" ClientValidationFunction="Validate2" ErrorMessage="You do not select any role from list." ForeColor="Red" ValidationGroup="A"></asp:CustomValidator>
        </div>
        <asp:GridView ID="AddNewRoleGridView" runat="server" AutoGenerateColumns="False" DataSourceID="OtherRolesSQL" DataKeyNames="RoleID"
            CssClass="mGrid">
            <AlternatingRowStyle CssClass="alt" />
            <RowStyle CssClass="RowStyle" />
            <Columns>
                <asp:TemplateField>
                    <HeaderTemplate>
                        <asp:CheckBox ID="AllIteamCheckBox" runat="server" Text="All" />
                    </HeaderTemplate>
                    <ItemTemplate>
                        <asp:CheckBox ID="AddCheckBox" runat="server" Text=" " />
                    </ItemTemplate>
                    <ItemStyle Width="50px" />
                </asp:TemplateField>
                <asp:BoundField DataField="Role" HeaderText="Role" SortExpression="Role" />
            </Columns>
        </asp:GridView>
        <asp:SqlDataSource ID="OtherRolesSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="IF(@IDs &lt;&gt;'')
BEGIN
SELECT DISTINCT Income_PayOrder.RoleID, Income_Roles.Role FROM Income_PayOrder INNER JOIN Income_Roles ON Income_PayOrder.RoleID = Income_Roles.RoleID INNER JOIN Student ON [Income_PayOrder].StudentID = Student.StudentID WHERE (Income_PayOrder.SchoolID = @SchoolID) AND  (Income_PayOrder.[EducationYearID] = @EducationYearID) AND (Student.ID IN (SELECT id FROM  dbo.In_Function_Parameter(@IDs))) AND (Income_PayOrder.PaidAmount &lt;= 0) ORDER BY RoleID
END
ELSE
BEGIN
SELECT DISTINCT Income_PayOrder.RoleID, Income_Roles.Role FROM Income_PayOrder INNER JOIN Income_Roles ON Income_PayOrder.RoleID = Income_Roles.RoleID INNER JOIN Student ON [Income_PayOrder].StudentID = Student.StudentID WHERE (Income_PayOrder.SchoolID = @SchoolID) AND  (Income_PayOrder.[EducationYearID] = @EducationYearID) AND ((Income_PayOrder.ClassID = @ClassID) OR (@ClassID = -1)) AND (Income_PayOrder.PaidAmount &lt;= 0) ORDER BY RoleID
END"
            CancelSelectOnNullParameter="False">
            <SelectParameters>
                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                <asp:ControlParameter ControlID="IDTextBox" Name="IDs" PropertyName="Text" />
                <asp:ControlParameter ControlID="Session_DropDownList" Name="EducationYearID" PropertyName="SelectedValue" />
            </SelectParameters>
        </asp:SqlDataSource>
        <br />
        <div class="form-inline">
            <div class="form-group">
                <asp:DropDownList ID="PayForDropDownList" runat="server" CssClass="form-control" DataSourceID="PayForSQL" DataTextField="PayFor" DataValueField="PayFor" AppendDataBoundItems="True">
                    <asp:ListItem Value="%">Pay For</asp:ListItem>
                </asp:DropDownList>
                <asp:SqlDataSource ID="PayForSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="IF(@IDs &lt;&gt;'')
BEGIN
SELECT DISTINCT [PayFor] FROM [Income_PayOrder] INNER JOIN Student ON [Income_PayOrder].StudentID = Student.StudentID WHERE ([Income_PayOrder].[SchoolID] = @SchoolID) AND ([Income_PayOrder].[EducationYearID] = @EducationYearID) AND (Student.ID IN (SELECT id FROM dbo.In_Function_Parameter(@IDs))) AND (Income_PayOrder.PaidAmount &lt;= 0) ORDER BY PayFor
END
ELSE
BEGIN
SELECT DISTINCT [PayFor] FROM [Income_PayOrder] INNER JOIN Student ON [Income_PayOrder].StudentID = Student.StudentID WHERE ([Income_PayOrder].[SchoolID] = @SchoolID) AND ([Income_PayOrder].[EducationYearID] = @EducationYearID) AND ((Income_PayOrder.ClassID = @ClassID) OR (@ClassID = -1)) AND (Income_PayOrder.PaidAmount &lt;= 0) ORDER BY PayFor
END
"
                    CancelSelectOnNullParameter="False">
                    <SelectParameters>
                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                        <asp:ControlParameter ControlID="Session_DropDownList" Name="EducationYearID" PropertyName="SelectedValue" />
                        <asp:ControlParameter ControlID="IDTextBox" Name="IDs" PropertyName="Text" />
                        <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                    </SelectParameters>
                </asp:SqlDataSource>
            </div>
            <div class="form-group">
                <asp:TextBox ID="EndDateTextBox" placeholder="Pay order End Date" runat="server" CssClass="form-control Datetime" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false"></asp:TextBox>
            </div>
            <div class="form-group">
                <asp:Button ID="Role_Find_Button" runat="server" CssClass="btn btn-primary" OnClick="Role_Find_Button_Click" Text="Find student in roles" ValidationGroup="A" />
            </div>
        </div>
    </div>


    <!-- Modal -->
    <div class="modal fade" id="myModal" tabindex="-1" role="dialog" aria-labelledby="myModalLabel" aria-hidden="true">
        <div class="modal-dialog  modal-lg cascading-modal" role="document">
            <div class="modal-content">
                <div class="modal-header">
                    <h4 class="title">Student pay order</h4>
                    <button type="button" class="close" data-dismiss="modal" aria-label="Close"><span aria-hidden="true">&times;</span></button>
                </div>
                <div class="modal-body mb-0">
                    <div class="table-responsive">
                        <asp:GridView ID="DueGridView" runat="server" AutoGenerateColumns="False" CssClass="mGrid" PagerStyle-CssClass="pgr" DataKeyNames="PayOrderID">
                            <Columns>
                                <asp:TemplateField>
                                    <HeaderTemplate>
                                        <asp:CheckBox ID="AllIteamCheckBox" runat="server" Text="All" />
                                    </HeaderTemplate>
                                    <ItemTemplate>
                                        <asp:CheckBox ID="AddCheckBox" runat="server" Text=" " />
                                    </ItemTemplate>
                                    <ItemStyle Width="50px" />
                                </asp:TemplateField>
                                <asp:BoundField DataField="ID" HeaderText="ID" SortExpression="ID" />
                                <asp:BoundField DataField="Class" HeaderText="Class" SortExpression="Class" />
                                <asp:BoundField DataField="StudentsName" HeaderText="Name" SortExpression="StudentsName" />
                                <asp:BoundField DataField="Role" HeaderText="Role" SortExpression="Role"></asp:BoundField>
                                <asp:BoundField DataField="PayFor" HeaderText="Pay For" SortExpression="PayFor" />
                                <asp:BoundField DataField="Amount" HeaderText="Amount" SortExpression="Amount" />
                                <asp:BoundField DataField="Due" HeaderText="Due" ReadOnly="True" SortExpression="Due" />
                                <asp:BoundField DataField="StartDate" HeaderText="Start Date" SortExpression="StartDate" DataFormatString="{0:d MMM yyyy}" />
                                <asp:BoundField DataField="EndDate" HeaderText="End Date" SortExpression="EndDate" DataFormatString="{0:d MMM yyyy}" />
                            </Columns>
                            <PagerStyle CssClass="pgr" />
                            <EmptyDataTemplate>
                                No record(s) found !
                            </EmptyDataTemplate>
                        </asp:GridView>
                        <asp:SqlDataSource ID="DueSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                            SelectCommand="SELECT Income_PayOrder.PayOrderID, Student.ID, Student.StudentsName, Income_Roles.Role, Income_PayOrder.PayFor, Income_PayOrder.StartDate, Income_PayOrder.EndDate, Income_PayOrder.Amount, Income_PayOrder.Discount, Income_PayOrder.LateFee, Income_PayOrder.LateFee_Discount, Income_PayOrder.PaidAmount, Income_PayOrder.Receivable_Amount AS Due, Income_PayOrder.LastPaidDate, Income_PayOrder.NumberOfPayment, Income_PayOrder.ClassID, Income_PayOrder.RoleID, Income_PayOrder.StudentID, CreateClass.Class, Income_PayOrder.AssignRoleID FROM Income_PayOrder INNER JOIN Income_Roles ON Income_PayOrder.RoleID = Income_Roles.RoleID INNER JOIN Student ON Income_PayOrder.StudentID = Student.StudentID INNER JOIN CreateClass ON Income_PayOrder.ClassID = CreateClass.ClassID INNER JOIN StudentsClass ON Income_PayOrder.StudentClassID = StudentsClass.StudentClassID WHERE (Income_PayOrder.SchoolID = @SchoolID) AND (Income_PayOrder.EducationYearID = @EducationYearID) AND (Income_PayOrder.PayFor LIKE @PayFor) AND (Income_PayOrder.EndDate &lt;= ISNULL(@EndDate, '1-1-3000')) AND (Income_PayOrder.PaidAmount &lt;= 0)" DeleteCommand="DELETE FROM Income_PayOrder WHERE (PayOrderID = @PayOrderID)" CancelSelectOnNullParameter="False">
                            <DeleteParameters>
                                <asp:Parameter Name="PayOrderID" />
                            </DeleteParameters>

                            <SelectParameters>
                                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                                <asp:ControlParameter ControlID="Session_DropDownList" Name="EducationYearID" PropertyName="SelectedValue" />
                                <asp:ControlParameter ControlID="PayForDropDownList" DefaultValue="" Name="PayFor" PropertyName="SelectedValue" />
                                <asp:ControlParameter ControlID="EndDateTextBox" Name="EndDate" PropertyName="Text" DefaultValue="" />
                            </SelectParameters>
                        </asp:SqlDataSource>
                        <asp:SqlDataSource ID="RemovePayOrderSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT PayOrderID, SchoolID, RegistrationID, StudentID, ClassID, StudentClassID, Amount, PaidAmount, LateFee, Discount, LateFee_Discount, RoleID, PayFor, StartDate, EndDate, Status, CreatedDate, EducationYearID, LastPaidDate, NumberOfPayment FROM Income_PayOrder WHERE (SchoolID = @SchoolID)" DeleteCommand="DELETE FROM Income_PayOrder WHERE (PayOrderID = @PayOrderID)">
                            <DeleteParameters>
                                <asp:Parameter Name="PayOrderID" />
                            </DeleteParameters>
                            <SelectParameters>
                                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                            </SelectParameters>
                        </asp:SqlDataSource>
                    </div>
                </div>
                <div class="modal-footer">
                    <asp:CustomValidator ID="CV2" runat="server" ClientValidationFunction="Validate3" ErrorMessage="You do not select any student from student list." ForeColor="Red" ValidationGroup="R"></asp:CustomValidator><br />
                    <asp:Button ID="RemoveOrderButton" runat="server" Text="Remove Payorder" CssClass="btn btn-primary" OnClick="RemoveOrderButton_Click" ValidationGroup="R" />
                    <button type="button" class="btn btn-primary" data-dismiss="modal">Close</button>
                </div>
            </div>
        </div>
    </div>


    <script type="text/javascript">
        $(function () {
            $('.Datetime').datepicker({
                format: 'dd M yyyy',
                todayBtn: "linked",
                todayHighlight: true,
                autoclose: true
            });

            if (!$('[id*=StudentsGridView] tr').length) {
                $('.Students').hide();
            }

            if (!$('[id*=AddNewRoleGridView] tr').length) {
                $('.Roles').hide();
            }

            if ($('[id*=SectionDropDownList]').find('option').length > 1) {
                $(".S_Show").show();
            }

            //-Checkbox
            $("[id*=AllIteamCheckBox]").on("click", function () {
                var a = $(this), b = $(this).closest("table");
                $("input[type=checkbox]", b).each(function () {
                    a.is(":checked") ? ($(this).attr("checked", "checked"), $("td", $(this).closest("tr")).addClass("selected")) : ($(this).removeAttr("checked"), $("td", $(this).closest("tr")).removeClass("selected"));
                });
            });

            $("[id*=SingleCheckBox]").on("click", function () {
                var a = $(this).closest("table"), b = $("[id*=chkHeader]", a);
                $(this).is(":checked") ? ($("td", $(this).closest("tr")).addClass("selected"), $("[id*=chkRow]", a).length == $("[id*=chkRow]:checked", a).length && b.attr("checked", "checked")) : ($("td", $(this).closest("tr")).removeClass("selected"), b.removeAttr("checked"));
            });

            $("[id*=AddCheckBox]").on("click", function () {
                var a = $(this).closest("table"), b = $("[id*=chkHeader]", a);
                $(this).is(":checked") ? ($("td", $(this).closest("tr")).addClass("selected"), $("[id*=chkRow]", a).length == $("[id*=chkRow]:checked", a).length && b.attr("checked", "checked")) : ($("td", $(this).closest("tr")).removeClass("selected"), b.removeAttr("checked"));
            });
        });

        function SelectedItemCLR(a) {
            a.options[1].style.color = "rgb(255, 106, 0)";
        };

        function openModal() {
            $('#myModal').modal('show');
        }

        function isNumberKey(a) { a = a.which ? a.which : event.keyCode; return 46 != a && 31 < a && (48 > a || 57 < a) ? !1 : !0 };

        //Select at least one Checkbox From GridView
        function Validate(d, c) {
            for (var b = document.getElementById("<%=StudentsGridView.ClientID %>").getElementsByTagName("input"), a = 0; a < b.length; a++) {
                if ("checkbox" == b[a].type && b[a].checked) {
                    c.IsValid = !0;
                    return;
                }
            }
            c.IsValid = !1;
        };

        function Validate2(d, c) {
            for (var b = document.getElementById("<%=AddNewRoleGridView.ClientID %>").getElementsByTagName("input"), a = 0; a < b.length; a++) {
              if ("checkbox" == b[a].type && b[a].checked) {
                  c.IsValid = !0;
                  return;
              }
          }
          c.IsValid = !1;
      };

      function Validate3(d, c) {
          for (var b = document.getElementById("<%=DueGridView.ClientID %>").getElementsByTagName("input"), a = 0; a < b.length; a++) {
              if ("checkbox" == b[a].type && b[a].checked) {
                  c.IsValid = !0;
                  return;
              }
          }
          c.IsValid = !1;
      };

      //Prevent Re-Submission Button
      function DisableButton() {
          document.getElementById("<%=RemoveOrderButton.ClientID %>").disabled = !0;
      }
      window.onbeforeunload = DisableButton;
    </script>
</asp:Content>
