<%@ Page Title="Current Dues" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Present_Due.aspx.cs" Inherits="EDUCATION.COM.ACCOUNTS.AccountsAnalysis.Present_Due" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <meta http-equiv="content-type" content="application/xhtml+xml; charset=UTF-8" />
    <style>
        .PD_Name_Class { color: #282828; font-size: 18px; }
        .modal-body { max-height: 500px; overflow: auto; }

        .Print_ins_Name { text-align: center; margin-bottom: 10px; color: #000; padding-bottom: 5px; border-bottom: 1px solid #000; display: none; }
        #Print_InsName { font-size: 30px; }
        #P_ClassName { font-size: 15px; }

        .info { width: 100%; }
        .info ul { margin: 0; padding: 0; }
        .info ul li { border-bottom: 1px solid #d6e0eb; color: #5d6772; font-size: 15px; line-height: 23px; list-style: outside none none; margin: 6px 0 0; padding-bottom: 5px; padding-left: 2px; }
        .info ul li:last-child { border-bottom: none; }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <asp:FormView ID="TotalPDueFormView" runat="server" DataSourceID="IntotalPDueSQL" Width="100%">
        <ItemTemplate>
            <h3>Total Current Dues In Institution: <%# Eval("PresentDue") %> Tk</h3>
        </ItemTemplate>
    </asp:FormView>
    <asp:SqlDataSource ID="IntotalPDueSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
        SelectCommand="SELECT ISNULL(SUM(CASE WHEN EndDate &lt; GETDATE() - 1 THEN ISNULL(Amount , 0) + ISNULL(LateFee , 0) - ISNULL(Discount , 0) - ISNULL(PaidAmount , 0) - ISNULL(LateFee_Discount , 0) ELSE ISNULL(Amount , 0) - ISNULL(Discount , 0) - ISNULL(PaidAmount , 0) END), 0) AS PresentDue FROM Income_PayOrder INNER JOIN Student ON Income_PayOrder.StudentID = Student.StudentID WHERE (Income_PayOrder.Status = 'Due') AND (Income_PayOrder.EndDate &lt; GETDATE()) AND (Income_PayOrder.SchoolID = @SchoolID) AND (Income_PayOrder.EducationYearID = @EducationYearID) AND (Student.Status = N'Active')">
        <SelectParameters>
            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
            <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
        </SelectParameters>
    </asp:SqlDataSource>

    <div class="NoPrint form-inline">
        <div class="form-group">
            <asp:RadioButtonList ID="DueRadioButtonList" runat="server" RepeatDirection="Horizontal" AutoPostBack="True" OnSelectedIndexChanged="DueRadioButtonList_SelectedIndexChanged" CssClass="form-control">
                <asp:ListItem Selected="True">Find By Class</asp:ListItem>
                <asp:ListItem>Find By ID</asp:ListItem>
            </asp:RadioButtonList>
        </div>
    </div>

    <asp:MultiView ID="DueMultiView" runat="server">
        <asp:View ID="ClassView" runat="server">
            <asp:UpdatePanel ID="UpdatePanel2" runat="server">
                <ContentTemplate>
                    <div class="Print_Due">
                        <div class="NoPrint form-inline">
                            <div class="form-group">
                                <asp:DropDownList ID="ClassDropDownList" runat="server" CssClass="form-control" AppendDataBoundItems="True" AutoPostBack="True" DataSourceID="ClassNameSQL" DataTextField="Class" DataValueField="ClassID">
                                    <asp:ListItem Value="0">[ SELECT CLASS ]</asp:ListItem>
                                </asp:DropDownList>
                                <asp:RequiredFieldValidator ID="RequiredFieldValidator6" runat="server" ControlToValidate="ClassDropDownList" CssClass="EroorSummer" ErrorMessage="*" InitialValue="0" ValidationGroup="A"></asp:RequiredFieldValidator>
                                <asp:SqlDataSource ID="ClassNameSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT * FROM [CreateClass] WHERE ([SchoolID] = @SchoolID) ORDER BY SN">
                                    <SelectParameters>
                                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                                    </SelectParameters>
                                </asp:SqlDataSource>
                            </div>
                            <div class="form-group S_Show" style="display: none">
                                <asp:DropDownList ID="SectionDropDownList" runat="server" AutoPostBack="True" CssClass="form-control" DataSourceID="SectionSQL" DataTextField="Section" DataValueField="SectionID" OnDataBound="SectionDropDownList_DataBound">
                                </asp:DropDownList>
                                <asp:SqlDataSource ID="SectionSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT DISTINCT [Join].SectionID, CreateSection.Section FROM [Join] INNER JOIN CreateSection ON [Join].SectionID = CreateSection.SectionID WHERE ([Join].ClassID = @ClassID)">
                                    <SelectParameters>
                                        <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                                    </SelectParameters>
                                </asp:SqlDataSource>
                            </div>
                            <div class="form-group">
                                <asp:DropDownList ID="RoleDropDownList" runat="server" AutoPostBack="True" CssClass="form-control" DataSourceID="RoleSQL" DataTextField="Role" DataValueField="RoleID" OnDataBound="RoleDropDownList_DataBound">
                                </asp:DropDownList>
                                <asp:SqlDataSource ID="RoleSQL" runat="server" CancelSelectOnNullParameter="False" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT DISTINCT Income_Roles.Role, Income_Roles.RoleID
FROM            Income_PayOrder INNER JOIN
                         Income_Roles ON Income_PayOrder.RoleID = Income_Roles.RoleID
WHERE        (Income_PayOrder.SchoolID = @SchoolID) AND (Income_PayOrder.EndDate &lt; GETDATE()) AND (Income_PayOrder.Receivable_Amount &gt; 0) AND (Income_PayOrder.StudentID IN
                             (SELECT        Student.StudentID
                               FROM            Student INNER JOIN
                                                         StudentsClass ON Student.StudentID = StudentsClass.StudentID
                               WHERE        (Student.SchoolID = @SchoolID) AND (StudentsClass.EducationYearID = @EducationYearID) AND (Student.Status = N'Active') AND (StudentsClass.ClassID = @ClassID)))
ORDER BY Income_Roles.Role">
                                    <SelectParameters>
                                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                                        <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                                        <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                                    </SelectParameters>
                                </asp:SqlDataSource>
                            </div>
                        </div>

                        <div class="PDby_Class">
                            <label id="C_Name"></label>
                        </div>

                        <asp:GridView ID="TotalDueGridView" runat="server" AutoGenerateColumns="False" DataSourceID="TotalDueSQL"
                            DataKeyNames="ID,Due,SMSPhoneNo,StudentID,StudentsName,RollNo" CssClass="mGrid" OnRowDataBound="TotalDueGridView_RowDataBound" AllowSorting="True">
                            <Columns>
                                <asp:TemplateField>
                                    <HeaderTemplate>
                                        <asp:CheckBox ID="AllIteamCheckBox" runat="server" Text="All" />
                                    </HeaderTemplate>
                                    <ItemTemplate>
                                        <asp:CheckBox ID="SingleCheckBox" runat="server" Text=" " />
                                    </ItemTemplate>
                                    <HeaderStyle CssClass="NoPrint" />
                                    <ItemStyle Width="50px" CssClass="NoPrint" />
                                </asp:TemplateField>
                                <asp:BoundField DataField="ID" HeaderText="ID" SortExpression="ID" />
                                <asp:BoundField DataField="RollNo" HeaderText="Roll" SortExpression="RollNo" />
                                <asp:BoundField DataField="StudentsName" HeaderText="Name" SortExpression="StudentsName" />
                                <asp:TemplateField HeaderText="Due" SortExpression="Due">
                                    <ItemTemplate>
                                        <asp:Label ID="CTDueLabel" runat="server" Text='<%# Bind("Due") %>'></asp:Label>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:BoundField DataField="SMSPhoneNo" HeaderText="Phone" SortExpression="SMSPhoneNo" />
                            </Columns>
                            <HeaderStyle Font-Size="9pt" />
                            <FooterStyle CssClass="GridFooter" />
                        </asp:GridView>
                        <asp:SqlDataSource ID="TotalDueSQL" runat="server" CancelSelectOnNullParameter="False" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT Income_PayOrder.StudentID, Student.SchoolID, Student.ID, Student.StudentsName, StudentsClass.RollNo, SUM(Income_PayOrder.Receivable_Amount) AS Due, Student.SMSPhoneNo FROM Income_PayOrder INNER JOIN Student ON Income_PayOrder.StudentID = Student.StudentID INNER JOIN StudentsClass ON Student.StudentID = StudentsClass.StudentID WHERE (Income_PayOrder.Status = 'Due') AND (Income_PayOrder.EndDate &lt; GETDATE()) AND (Student.SchoolID = @SchoolID) AND (StudentsClass.EducationYearID = @EducationYearID) AND (Student.Status = N'Active') AND (StudentsClass.ClassID = @ClassID) AND StudentsClass.SectionID LIKE @SectionID AND (Income_PayOrder.RoleID LIKE @RoleID) AND (Income_PayOrder.Is_Active = 1) GROUP BY Student.StudentsName, Student.ID, Income_PayOrder.StudentID, Student.SMSPhoneNo, StudentsClass.RollNo, Student.SchoolID ORDER BY CASE WHEN ISNUMERIC(StudentsClass.RollNo) = 1 THEN CAST(REPLACE(REPLACE(StudentsClass.RollNo, '$', ''), ',', '') AS INT) ELSE 0 END">
                            <SelectParameters>
                                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                                <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                                <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                                <asp:ControlParameter ControlID="RoleDropDownList" Name="RoleID" PropertyName="SelectedValue" />
                                <asp:ControlParameter ControlID="SectionDropDownList" Name="SectionID" PropertyName="SelectedValue" />
                            </SelectParameters>
                        </asp:SqlDataSource>

                        <asp:CustomValidator ID="CV" runat="server" ClientValidationFunction="Validate" ErrorMessage="You do not select any student from student list." ForeColor="Red" ValidationGroup="AD" CssClass="Class"></asp:CustomValidator>
                    </div>

                    <div class="form-inline Submit_Disable d-print-none">
                        <!-- SMS Template Info & Edit Link -->
                        <div class="form-group" style="flex: 1; margin-right: 15px;">
                            <div style="background-color: #fff3cd; border: 1px solid #ffc107; border-radius: 4px; padding: 8px 12px;">
                                <i class="fa fa-info-circle" style="color: #ff9800;"></i>
                                <small style="color: #856404; margin-right: 8px;">SMS Template: Due Notification</small>
                                <a href="/SMS/SMS_Template.aspx" class="btn btn-sm btn-warning" style="padding: 2px 8px; font-size: 11px;">
                                    <i class="fa fa-edit"></i> Edit
                                </a>
                            </div>
                        </div>

                        <div class="form-group">
                            <asp:Button ID="ClassSendButton" runat="server" CssClass="btn btn-primary" OnClick="ClassSendButton_Click" Text="Send SMS" ValidationGroup="AD" />
                        </div>
                        <div class="form-group">
                            <asp:Button ID="ViewAllDueButton" runat="server" CssClass="btn btn-grey" OnClick="ViewAllDueButton_Click" Text="View" ValidationGroup="AD" />
                        </div>
                        <div class="form-group">
                            <button type="button" class="btn btn-blue-grey" onclick="window.print();">Print</button>
                        </div>
                    </div>
                </ContentTemplate>
            </asp:UpdatePanel>
        </asp:View>

        <asp:View ID="IDView" runat="server">
            <div class="form-inline">
                <div class="form-group">
                    <asp:TextBox ID="IDTextBox" autocomplete="off" placeholder="Enter ID" runat="server" CssClass="form-control"></asp:TextBox>
                </div>
                <div class="form-group">
                    <asp:Button ID="FindButton" runat="server" CssClass="btn btn-primary" Text="Find" />
                </div>
            </div>

            <asp:FormView ID="StudentInfoFormView" DataKeyNames="SMSPhoneNo,StudentsName,Due" runat="server" DataSourceID="TotalDue_ByID_ODS" Width="100%">
                <ItemTemplate>
                    <div class="z-depth-1 mb-4 p-3">
                        <div class="info">
                            <ul>
                                <li>
                                    <b>(<%# Eval("ID") %>)
                                        <%# Eval("StudentsName") %></b>
                                </li>
                                <li>
                                    <b>Class:</b>
                                    <%# Eval("Class") %>
                                </li>
                                <li><b>Roll No:</b>
                                    <%# Eval("RollNo") %>
                                </li>
                                <li><b>Phone:</b>
                                    <%# Eval("SMSPhoneNo") %>
                                </li>
                                <li class="alert-secondary p-2">
                                    <b>Due:</b>
                                   <%#Eval("Due") %>
                                    Tk
                                </li>
                            </ul>
                        </div>
                    </div>
                </ItemTemplate>
            </asp:FormView>
            <asp:ObjectDataSource ID="TotalDue_ByID_ODS" runat="server" OldValuesParameterFormatString="original_{0}" SelectMethod="GetData" TypeName="EDUCATION.COM.PaymentDataSetTableAdapters.TotalDue_ByIDTableAdapter">
                <SelectParameters>
                    <asp:ControlParameter ControlID="IDTextBox" DefaultValue="0" Name="ID" PropertyName="Text" Type="String" />
                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" Type="Int32" />
                </SelectParameters>
            </asp:ObjectDataSource>

            <div class="table-responsive mb-2">
                <asp:GridView ID="ID_DueDetailsGridView" runat="server" AutoGenerateColumns="False" DataSourceID="ID_DueDetailsODS" CssClass="mGrid">
                    <Columns>
                        <asp:BoundField DataField="Role" HeaderText="Role" SortExpression="Role" />
                        <asp:BoundField DataField="PayFor" HeaderText="Pay For" SortExpression="PayFor" />
                        <asp:BoundField DataField="PaidAmount" HeaderText="Paid" SortExpression="PaidAmount" />
                        <asp:BoundField DataField="Due" HeaderText="Due" SortExpression="Due" />
                        <asp:BoundField DataField="EndDate" HeaderText="End Date" SortExpression="EndDate" DataFormatString="{0:d MMM yyyy}" />
                    </Columns>
                    <HeaderStyle Font-Size="9pt" />
                    <FooterStyle CssClass="GridFooter" />
                </asp:GridView>
                <asp:ObjectDataSource ID="ID_DueDetailsODS" runat="server" OldValuesParameterFormatString="original_{0}" SelectMethod="GetData" TypeName="EDUCATION.COM.PaymentDataSetTableAdapters.DueDetailsTableAdapter">
                    <SelectParameters>
                        <asp:ControlParameter ControlID="IDTextBox" DefaultValue="0" Name="ID" PropertyName="Text" Type="String" />
                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" Type="Int32" />
                        <asp:Parameter DefaultValue="%" Name="RoleID" Type="String" />
                    </SelectParameters>
                </asp:ObjectDataSource>
            </div>

            <div class="id_hide" style="display: none;">
                <!-- SMS Template Info & Edit Link -->
                <div style="background-color: #e7f3ff; border: 1px solid #2196F3; border-radius: 4px; padding: 10px 15px; margin-bottom: 15px;">
     <i class="fa fa-info-circle" style="color: #2196F3;"></i>
      <strong style="color: #1976D2;">SMS Template:</strong>
<span style="color: #555; margin-left: 10px;">
      Due notification template. 
            <small style="color: #777;">(Placeholders: {StudentName}, {ID}, {TotalDue}, {DueDetails}, etc.)</small>
        </span>
        <a href="/SMS/SMS_Template.aspx" class="btn btn-info btn-sm ml-2" style="text-decoration: none;">
  <i class="fa fa-edit"></i> Edit Templates
     </a>
 </div>

                <div class="form-inline">
                    <div class="form-group">
                        <asp:Button ID="IDSendButton" runat="server" CssClass="btn btn-primary" OnClick="IDSendButton_Click" Text="Send SMS" />
                    </div>
                    <div class="form-group">
                        <button type="button" class="btn btn-primary hidden-print" onclick="window.print();">Print</button>
                        <asp:Label ID="ErrorLabel" runat="server" CssClass="EroorSummer"></asp:Label>
                    </div>
                </div>
            </div>
        </asp:View>
    </asp:MultiView>

    <asp:SqlDataSource ID="SMS_OtherInfoSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" InsertCommand="INSERT INTO SMS_OtherInfo(SMS_Send_ID, SchoolID, StudentID, TeacherID, EducationYearID) VALUES (@SMS_Send_ID, @SchoolID, @StudentID, @TeacherID, @EducationYearID)" SelectCommand="SELECT * FROM [SMS_OtherInfo]">
        <InsertParameters>
            <asp:Parameter Name="SMS_Send_ID" DbType="Guid" />
            <asp:Parameter Name="SchoolID" />
            <asp:Parameter Name="StudentID" />
            <asp:Parameter Name="TeacherID" />
            <asp:Parameter Name="EducationYearID" />
        </InsertParameters>
    </asp:SqlDataSource>

    <!-- Modal -->
    <div id="myModal" class="modal fade" role="dialog">
        <div class="modal-dialog modal-lg">
            <div class="modal-content">
                <div class="modal-header">
                    <h4 class="modal-title">Student's Due Details</h4>
                    <button type="button" class="close" data-dismiss="modal">&times;</button>
                </div>
                <div class="modal-body" id="modalDiv">
                    <asp:UpdatePanel ID="upnlUsers" runat="server">
                        <ContentTemplate>
                            <asp:Panel ID="ExportPanel" CssClass="AllDueP" runat="server">
                                <div class="Print_ins_Name">
                                    <label id="Print_InsName"></label>
                                    <br />
                                    <label id="P_ClassName"></label>
                                </div>
                                <asp:DataList ID="RoleDataList" runat="server" RepeatDirection="Horizontal" RepeatColumns="1" Width="100%">
                                    <ItemTemplate>
                                        <asp:Label ID="NameLabel" CssClass="PD_Name_Class" runat="server" Font-Names="Tahoma" />
                                        <asp:GridView ID="AllDueGridView" runat="server" Width="100%" AutoGenerateColumns="False" ShowFooter="True" OnRowDataBound="AllDueGridView_RowDataBound" Font-Names="Tahoma" ForeColor="#333333">
                                            <Columns>
                                                <asp:BoundField DataField="Role" HeaderText="Role" />
                                                <asp:BoundField DataField="PayFor" HeaderText="Pay For" />
                                                <asp:TemplateField HeaderText="Due">
                                                    <FooterTemplate>
                                                        <asp:Label ID="InSumLabel" runat="server"></asp:Label>
                                                    </FooterTemplate>
                                                    <ItemTemplate>
                                                        <asp:Label ID="SumAllDueLabel" runat="server" Text='<%# Bind("Due") %>' />
                                                    </ItemTemplate>
                                                </asp:TemplateField>
                                            </Columns>
                                            <FooterStyle BackColor="#F4F4F4" Font-Size="11pt" />
                                            <HeaderStyle BackColor="#F4F4F4" Font-Size="11pt" />
                                            <RowStyle Font-Size="11pt" />
                                        </asp:GridView>
                                        <br />
                                        <br />
                                        &nbsp;&nbsp;&nbsp;&nbsp;
                                    </ItemTemplate>
                                </asp:DataList>
                            </asp:Panel>
                        </ContentTemplate>
                        <Triggers>
                            <asp:PostBackTrigger ControlID="ExportWordButton" />
                        </Triggers>
                    </asp:UpdatePanel>
                </div>
                <div class="modal-footer">
                    <asp:Button ID="ExportWordButton" runat="server" CssClass="btn btn-success" OnClick="ExportWordButton_Click" Text="Export To Word" CausesValidation="true" />
                    <button type="button" class="btn btn-primary print" onclick="Modal_Info_Prnt();">Print</button>
                    <button type="button" class="btn btn-danger" data-dismiss="modal">Close</button>
                </div>
            </div>
        </div>
    </div>


    <asp:UpdateProgress ID="UpdateProgress" runat="server">
        <ProgressTemplate>
            <div id="progress_BG"></div>
            <div id="progress">
                <img src="../../CSS/loading.gif" alt="Loading..." />
                <br />
                <b>Loading...</b>
            </div>
        </ProgressTemplate>
    </asp:UpdateProgress>

    <script src="/JS/Print_This.js"></script>
    <script>
        $(function () {
            $("[id*=IDTextBox]").typeahead({
                minLength: 1,
                source: function (request, result) {
                    $.ajax({
                        url: "/Handeler/Student_IDs.asmx/GetStudentID",
                        data: JSON.stringify({ 'ids': request }),
                        dataType: "json",
                        type: "POST",
                        contentType: "application/json; charset=utf-8",
                        success: function (response) {
                            result($.map(JSON.parse(response.d), function (item) {
                                return item;
                            }));
                        }
                    });
                }
            });

            if ($('[id*=SectionDropDownList] option').length > 1) {
                $('.S_Show').show();

            }
            //Checkbox selected color
            $("[id*=AllIteamCheckBox]").on("click", function () {
                var isChecked = $(this).prop('checked');
                var grid = $(this).closest("table");
                
                $("input[type=checkbox]", grid).each(function () {
                    $(this).prop('checked', isChecked);
                  if (isChecked) {
 $("td", $(this).closest("tr")).addClass("selected");
  } else {
 $("td", $(this).closest("tr")).removeClass("selected");
 }
                });
            });

            $("[id*=SingleCheckBox]").on("click", function () {
    var grid = $(this).closest("table");
 var allCheckbox = $("[id*=AllIteamCheckBox]", grid);
    
    if ($(this).prop('checked')) {
        $("td", $(this).closest("tr")).addClass("selected");
  
    // Check if all individual checkboxes are checked
  var totalCheckboxes = $("[id*=SingleCheckBox]", grid).length;
  var checkedCheckboxes = $("[id*=SingleCheckBox]:checked", grid).length;
        
        if (totalCheckboxes === checkedCheckboxes) {
   allCheckbox.prop('checked', true);
      }
    } else {
  $("td", $(this).closest("tr")).removeClass("selected");
  allCheckbox.prop('checked', false);
    }
});

            //GridView Is Empty
            if (!$('[id*=TotalDueGridView] tr').length) {
                $(".Submit_Disable").hide();
                $("#C_Name").hide();
            }

            if ($('[id*=ID_DueDetailsGridView] tr').length) {
                $(".id_hide").show();
            }

            //Due Grand Total
            var DueTotal = 0;
            $("[id*=CTDueLabel]").each(function () { DueTotal = DueTotal + parseFloat($(this).text()) });

            var Role = "";
            if ($('[id*=RoleDropDownList] :selected').index() > 0) {
                Role = " For: " + $('[id*=RoleDropDownList] :selected').text();
            }

            $('#C_Name').text("Total Current Dues" + Role + " In Class " + $('[id*=ClassDropDownList] :selected').text() + ": " + DueTotal + " Tk");
        });

        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function (e, f) {
    //Checkbox selected color
    $("[id*=AllIteamCheckBox]").on("click", function () {
        var isChecked = $(this).prop('checked');
   var grid = $(this).closest("table");
        
   $("input[type=checkbox]", grid).each(function () {
       $(this).prop('checked', isChecked);
      if (isChecked) {
    $("td", $(this).closest("tr")).addClass("selected");
  } else {
   $("td", $(this).closest("tr")).removeClass("selected");
    }
     });
    });

    $("[id*=SingleCheckBox]").on("click", function () {
 var grid = $(this).closest("table");
   var allCheckbox = $("[id*=AllIteamCheckBox]", grid);
      
 if ($(this).prop('checked')) {
 $("td", $(this).closest("tr")).addClass("selected");
      
   // Check if all individual checkboxes are checked
 var totalCheckboxes = $("[id*=SingleCheckBox]", grid).length;
       var checkedCheckboxes = $("[id*=SingleCheckBox]:checked", grid).length;
 
  if (totalCheckboxes === checkedCheckboxes) {
    allCheckbox.prop('checked', true);
   }
   } else {
  $("td", $(this).closest("tr")).removeClass("selected");
     allCheckbox.prop('checked', false);
        }
    });

    if ($('[id*=SectionDropDownList] option').length > 1) {
        $('.S_Show').show();
    }

    //GridView Is Empty
    if (!$('[id*=TotalDueGridView] tr').length) {
        $(".Submit_Disable").hide();
   $("#C_Name").hide();
    }

    //Due Grand Total
    var DueTotal = 0;
    $("[id*=CTDueLabel]").each(function () { DueTotal = DueTotal + parseFloat($(this).text()) });

    var Role = "";
    if ($('[id*=RoleDropDownList] :selected').index() > 0) {
        Role = " For: " + $('[id*=RoleDropDownList] :selected').text();
  }

    $('#C_Name').text("Total Current Dues" + Role + " In Class " + $('[id*=ClassDropDownList] :selected').text() + ": " + DueTotal + " Tk");
});

        function openModal() {
            $('#myModal').modal('show');
        }

        function Modal_Info_Prnt() {
            $(".Print_ins_Name").show();
            $("#Print_InsName").text($("#InstitutionName").text());
            $("#P_ClassName").text("Current Dues For Class: " + $('[id*=ClassDropDownList] :selected').text());

            $('#modalDiv').css({ 'height': 'auto', 'overflow': 'auto' }).removeClass('modal-body');
            $('#myModal').modal('hide');

            setTimeout(function () {
                $('#modalDiv').addClass('modal-body');
            }, 1000);

            $("#modalDiv").printThis({
                debug: false,
                importCSS: true,
                importStyle: true,
                printContainer: true,
                //loadCSS: "CSS/Present_Due.css",
                pageTitle: "Current Due",
                removeInline: false,
                printDelay: 200,
                header: null,
                formValues: true
            });
        }

        /*--Select at least one Checkbox in GridView--*/
        function Validate(d, c) {
            for (var b = document.getElementById("<%=TotalDueGridView.ClientID %>").getElementsByTagName("input"), a = 0; a < b.length; a++) {
                if ("checkbox" == b[a].type && b[a].checked) {
                    c.IsValid = !0;
                    return;
                }
            }
            c.IsValid = !1;
        };
    </script>
</asp:Content>
