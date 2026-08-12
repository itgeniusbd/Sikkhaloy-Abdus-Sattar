<%@ Page Title="Remove Pay Order" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Remove_Pay_order.aspx.cs" Inherits="EDUCATION.COM.ACCOUNTS.Payment.Remove_Pay_order" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="CSS/RemovePayorder.css" rel="stylesheet" />
    <style>
        .modal-body { max-height: 500px; overflow: auto; }
        #removeProgressPanel {
            width: 100%;
            margin-bottom: 10px;
            padding: 10px 12px;
            border: 1px solid #c5d8f0;
            border-radius: 8px;
            background: #f7faff;
        }
        #removeProgressPanel .progress {
            height: 24px;
            margin-bottom: 8px;
            background: #e9ecef;
            border-radius: 6px;
            overflow: hidden;
        }
        #removeProgressPanel .progress-bar {
            line-height: 24px;
            font-size: 12px;
            font-weight: 700;
        }
        .prog-stats {
            font-size: 13px;
            color: #333;
            margin-bottom: 6px;
        }
        .prog-stats strong { color: #1a6fc4; }
        #progStatus { font-size: 12px; color: #555; }
        #progStatus.error { color: #c0392b; font-weight: 600; }
        #progStatus.success { color: #0e6640; font-weight: 600; }
        .PayForBox { max-height: 180px; overflow: auto; margin-bottom: 12px; border: 1px solid #ddd; border-radius: 4px; background: #fff; }
        .PayForBox .mGrid { margin-bottom: 0; }
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
        <div class="alert-info">
            Select Pay For (optional — একাধিক মাস/ফি টিক দিন; কিছু না দিলে সব Pay For দেখাবে):
        </div>
        <div class="PayForBox">
            <asp:GridView ID="PayForGridView" runat="server" AutoGenerateColumns="False" DataSourceID="PayForSQL" DataKeyNames="PayFor" CssClass="mGrid">
                <AlternatingRowStyle CssClass="alt" />
                <RowStyle CssClass="RowStyle" />
                <Columns>
                    <asp:TemplateField>
                        <HeaderTemplate>
                            <asp:CheckBox ID="AllIteamCheckBox" runat="server" Text="All" />
                        </HeaderTemplate>
                        <ItemTemplate>
                            <asp:CheckBox ID="PayForCheckBox" runat="server" Text=" " />
                        </ItemTemplate>
                        <ItemStyle Width="50px" />
                    </asp:TemplateField>
                    <asp:BoundField DataField="PayFor" HeaderText="Pay For" SortExpression="PayFor" />
                </Columns>
            </asp:GridView>
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
        <div class="form-inline">
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
                        <asp:GridView ID="DueGridView" runat="server" AutoGenerateColumns="False" CssClass="mGrid" PagerStyle-CssClass="pgr" DataKeyNames="PayOrderID" EnableViewState="false">
                            <Columns>
                                <asp:TemplateField>
                                    <HeaderTemplate>
                                        <asp:CheckBox ID="AllIteamCheckBox" runat="server" Text="All" />
                                    </HeaderTemplate>
                                    <ItemTemplate>
                                        <asp:CheckBox ID="AddCheckBox" runat="server" Text=" " />
                                        <asp:HiddenField ID="PayOrderIDHidden" runat="server" Value='<%# Eval("PayOrderID") %>' />
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
                            SelectCommand="SELECT Income_PayOrder.PayOrderID, Student.ID, Student.StudentsName, Income_Roles.Role, Income_PayOrder.PayFor, Income_PayOrder.StartDate, Income_PayOrder.EndDate, Income_PayOrder.Amount, Income_PayOrder.Discount, Income_PayOrder.LateFee, Income_PayOrder.LateFee_Discount, Income_PayOrder.PaidAmount, Income_PayOrder.Receivable_Amount AS Due, Income_PayOrder.LastPaidDate, Income_PayOrder.NumberOfPayment, Income_PayOrder.ClassID, Income_PayOrder.RoleID, Income_PayOrder.StudentID, CreateClass.Class, Income_PayOrder.AssignRoleID FROM Income_PayOrder INNER JOIN Income_Roles ON Income_PayOrder.RoleID = Income_Roles.RoleID INNER JOIN Student ON Income_PayOrder.StudentID = Student.StudentID INNER JOIN CreateClass ON Income_PayOrder.ClassID = CreateClass.ClassID INNER JOIN StudentsClass ON Income_PayOrder.StudentClassID = StudentsClass.StudentClassID WHERE (Income_PayOrder.SchoolID = @SchoolID) AND (Income_PayOrder.EducationYearID = @EducationYearID) AND (Income_PayOrder.EndDate &lt;= ISNULL(@EndDate, '1-1-3000')) AND (Income_PayOrder.PaidAmount &lt;= 0)" DeleteCommand="DELETE FROM Income_PayOrder WHERE (PayOrderID = @PayOrderID)" CancelSelectOnNullParameter="False">
                            <DeleteParameters>
                                <asp:Parameter Name="PayOrderID" />
                            </DeleteParameters>

                            <SelectParameters>
                                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                                <asp:ControlParameter ControlID="Session_DropDownList" Name="EducationYearID" PropertyName="SelectedValue" />
                                <asp:ControlParameter ControlID="EndDateTextBox" Name="EndDate" PropertyName="Text" DefaultValue="" />
                            </SelectParameters>
                        </asp:SqlDataSource>
                    </div>
                </div>
                <div class="modal-footer">
                    <div id="removeProgressPanel" style="display:none;">
                        <div class="prog-stats">
                            <strong>মোট নির্বাচিত:</strong> <span id="progTotal">0</span> |
                            <strong>মুছে ফেলা:</strong> <span id="progDone">0</span> |
                            <strong>বাকি:</strong> <span id="progLeft">0</span> |
                            <strong>অতিবাহিত:</strong> <span id="progTime">0s</span>
                            <span id="progEta"></span>
                        </div>
                        <div class="progress">
                            <div id="progBar" class="progress-bar progress-bar-striped active" role="progressbar" style="width:0%;">0%</div>
                        </div>
                        <div id="progStatus">প্রস্তুত হচ্ছে...</div>
                    </div>
                    <div id="resultMessageBox" class="alert alert-success" style="display:none;width:100%;margin-bottom:8px;"></div>
                    <asp:CustomValidator ID="CV2" runat="server" ClientValidationFunction="Validate3" ErrorMessage="You do not select any student from student list." ForeColor="Red" ValidationGroup="R"></asp:CustomValidator><br />
                    <button type="button" id="btnRemovePayOrders" class="btn btn-primary">Remove Payorder</button>
                    <asp:Button ID="RefreshDueGridButton" runat="server" Text="RefreshDueGrid" OnClick="RefreshDueGridButton_Click" Style="display:none;" />
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

            if (!$('[id*=PayForGridView] tr').length) {
                $('.PayForBox').hide();
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

            $("[id*=AddCheckBox], [id*=PayForCheckBox]").on("click", function () {
                var a = $(this).closest("table"), b = $("[id*=chkHeader]", a);
                $(this).is(":checked") ? ($("td", $(this).closest("tr")).addClass("selected"), $("[id*=chkRow]", a).length == $("[id*=chkRow]:checked", a).length && b.attr("checked", "checked")) : ($("td", $(this).closest("tr")).removeClass("selected"), b.removeAttr("checked"));
            });

            $("#btnRemovePayOrders").on("click", startRemovePayOrders);
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

      function getRowPayOrderId(row) {
          var hid = row.querySelector("input[type=hidden][id*='PayOrderIDHidden']");
          if (hid && hid.value) return hid.value;
          var hids = row.querySelectorAll("input[type=hidden]");
          for (var i = 0; i < hids.length; i++) {
              if (hids[i].value && /^\d+$/.test(hids[i].value)) return hids[i].value;
          }
          return "";
      }

      function collectSelectedPayOrderIds() {
          var ids = [];
          var grid = document.getElementById("<%=DueGridView.ClientID %>");
          if (!grid) return "";
          var rows = grid.getElementsByTagName("tr");
          for (var i = 0; i < rows.length; i++) {
              var cb = rows[i].querySelector("input[type=checkbox]");
              if (cb && cb.checked) {
                  var payOrderId = getRowPayOrderId(rows[i]);
                  if (payOrderId) ids.push(payOrderId);
              }
          }
          return ids.join(",");
      }

      function showResultMessage(text, isSuccess) {
          var msg = document.getElementById("resultMessageBox");
          if (!msg) return;
          msg.className = isSuccess ? "alert alert-success" : "alert alert-danger";
          msg.innerText = text;
          msg.style.display = "block";
      }

      var removePayOrderState = {
          running: false,
          batchSize: 50,
          startTime: 0
      };

      function formatDuration(totalSeconds) {
          totalSeconds = Math.max(0, Math.floor(totalSeconds));
          if (totalSeconds < 60) return totalSeconds + "s";
          var mins = Math.floor(totalSeconds / 60);
          var secs = totalSeconds % 60;
          return mins + "m " + secs + "s";
      }

      function updateRemoveProgress(processed, total, deleted, statusText) {
          var left = Math.max(0, total - processed);
          var pct = total > 0 ? Math.round((processed / total) * 100) : 0;
          var elapsed = (Date.now() - removePayOrderState.startTime) / 1000;
          var etaText = "";

          document.getElementById("progTotal").innerText = total;
          document.getElementById("progDone").innerText = deleted;
          document.getElementById("progLeft").innerText = left;
          document.getElementById("progTime").innerText = formatDuration(elapsed);
          document.getElementById("progBar").style.width = pct + "%";
          document.getElementById("progBar").innerText = pct + "%";

          if (processed > 0 && left > 0) {
              var eta = (elapsed / processed) * left;
              etaText = " | আনু. বাকি: " + formatDuration(eta);
          }
          document.getElementById("progEta").innerText = etaText;

          var status = document.getElementById("progStatus");
          status.className = "";
          status.innerText = statusText || ("প্রক্রিয়াকরণ: " + processed + " / " + total);
      }

      function removeDeletedRowsFromGrid(batchIds) {
          var idMap = {};
          for (var i = 0; i < batchIds.length; i++) idMap[batchIds[i]] = true;

          var grid = document.getElementById("<%=DueGridView.ClientID %>");
          if (!grid) return;

          var rows = grid.getElementsByTagName("tr");
          for (var r = rows.length - 1; r >= 0; r--) {
              var hid = rows[r].querySelector("input[type=hidden][id*='PayOrderIDHidden']");
              if (hid && idMap[hid.value]) {
                  rows[r].parentNode.removeChild(rows[r]);
              }
          }
      }

      function finishRemovePayOrders(deleted, total) {
          removePayOrderState.running = false;
          var btn = document.getElementById("btnRemovePayOrders");
          if (btn) {
              btn.disabled = false;
              btn.innerText = "Remove Payorder";
          }

          updateRemoveProgress(total, total, deleted, "সম্পন্ন! মোট " + deleted + " টি pay order মুছে ফেলা হয়েছে।");
          document.getElementById("progStatus").className = "success";
          showResultMessage(deleted + " টি pay order সফলভাবে মুছে ফেলা হয়েছে।", true);
      }

      function failRemovePayOrders(message) {
          removePayOrderState.running = false;
          var btn = document.getElementById("btnRemovePayOrders");
          if (btn) {
              btn.disabled = false;
              btn.innerText = "Remove Payorder";
          }

          var status = document.getElementById("progStatus");
          status.className = "error";
          status.innerText = message || "Remove failed.";
          showResultMessage(message || "Remove failed.", false);
      }

      function resetRemoveButton() {
          var btn = document.getElementById("btnRemovePayOrders");
          if (btn) {
              btn.disabled = false;
              btn.innerText = "Remove Payorder";
          }
          removePayOrderState.running = false;
      }

      function startRemovePayOrders() {
          if (removePayOrderState.running) return;

          try {
              if (typeof (Page_ClientValidate) === "function" && !Page_ClientValidate("R")) {
                  return;
              }

              var allIds = collectSelectedPayOrderIds().split(",").filter(function (x) { return x; });
              if (!allIds.length) {
                  alert("কোনো pay order নির্বাচন করা হয়নি।");
                  return;
              }

              if (!confirm("মোট " + allIds.length + " টি pay order মুছে ফেলতে চান?")) {
                  return;
              }

              removePayOrderState.running = true;
              removePayOrderState.startTime = Date.now();

              var btn = document.getElementById("btnRemovePayOrders");
              if (btn) {
                  btn.disabled = true;
                  btn.innerText = "Removing...";
              }

              document.getElementById("removeProgressPanel").style.display = "block";
              var msgBox = document.getElementById("resultMessageBox");
              if (msgBox) msgBox.style.display = "none";

              var total = allIds.length;
              var processed = 0;
              var deleted = 0;
              var index = 0;
              var batchNo = 0;
              var totalBatches = Math.ceil(total / removePayOrderState.batchSize);

              updateRemoveProgress(0, total, 0, "শুরু হচ্ছে... মোট " + total + " টি, " + totalBatches + " batch এ মুছবে");
              runRemovePayOrderBatches(allIds, total, processed, deleted, index, batchNo, totalBatches);
          } catch (ex) {
              failRemovePayOrders("JavaScript error: " + ex.message);
          }
      }

      function runRemovePayOrderBatches(allIds, total, processed, deleted, index, batchNo, totalBatches) {
          if (index >= allIds.length) {
              finishRemovePayOrders(deleted, total);
              return;
          }

          batchNo++;
          var batch = allIds.slice(index, index + removePayOrderState.batchSize);
          index += removePayOrderState.batchSize;

          updateRemoveProgress(processed, total, deleted,
              "Batch " + batchNo + " / " + totalBatches + " চলছে... (" + batch.length + " টি)");

          $.ajax({
              url: "Remove_PayOrder_Batch.ashx",
              type: "POST",
              data: { ids: batch.join(",") },
              dataType: "json",
              timeout: 180000,
              success: function (res) {
                  if (res && res.ok) {
                      deleted += (res.deleted || 0);
                      processed += batch.length;
                      removeDeletedRowsFromGrid(batch);
                      updateRemoveProgress(processed, total, deleted,
                          "Batch " + batchNo + " / " + totalBatches + " সম্পন্ন");
                      runRemovePayOrderBatches(allIds, total, processed, deleted, index, batchNo, totalBatches);
                  } else {
                      failRemovePayOrders((res && res.message) ? res.message : "Delete failed.");
                  }
              },
              error: function (xhr, statusText) {
                  var err = "Network/server error";
                  if (statusText === "timeout") err = "সময় শেষ — server response পায়নি। আবার চেষ্টা করুন।";
                  else if (xhr && xhr.responseText) err = xhr.responseText.substring(0, 300);
                  failRemovePayOrders(err);
              }
          });
      }
    </script>
</asp:Content>
