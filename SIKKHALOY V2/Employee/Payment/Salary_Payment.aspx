<%@ Page Title="Salary Payment" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Salary_Payment.aspx.cs" Inherits="EDUCATION.COM.Employee.Payment.Salary_Payment" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .row-muted {
            background-color: #d1d1d1;
        }

        .hiddenPaid {
            display: none
        }

        .btn-unpaid {
            color: #0063e3 !important;
            font-weight: 400;
        }

        .delete-salary {
            color: red;
            cursor: pointer
        }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <h3>Employee Salary Payment
        <label id="MName"></label>
    </h3>

    <div class="form-inline d-print-none">
        <div class="form-group">
            <asp:DropDownList ID="PayorderNameDropDownList" runat="server" CssClass="form-control" DataSourceID="PayorderNameSQL" DataTextField="Payorder_Name" DataValueField="Employee_Payorder_NameID" AppendDataBoundItems="True" AutoPostBack="True">
                <asp:ListItem Value="0">[ PAYORDER NAME ]</asp:ListItem>
            </asp:DropDownList>
            <asp:SqlDataSource ID="PayorderNameSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                SelectCommand="SELECT Employee_Payorder_NameID, SchoolID, RegistrationID, Payorder_Name, CreateDate FROM Employee_Payorder_Name WHERE (SchoolID = @SchoolID)">
                <SelectParameters>
                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                </SelectParameters>
            </asp:SqlDataSource>
        </div>
        <div class="form-group mx-1">
            <asp:DropDownList ID="MonthNameDropDownList" AutoPostBack="true" runat="server" CssClass="form-control" DataSourceID="MonthNameSQL" DataTextField="Month_Name" DataValueField="date" AppendDataBoundItems="True">
                <asp:ListItem Value="0">[ SELECT MONTH ]</asp:ListItem>
            </asp:DropDownList>
            <asp:SqlDataSource ID="MonthNameSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                SelectCommand="with months (date) AS (SELECT StartDate FROM  Education_Year WHERE (EducationYearID = @EducationYearID)
UNION ALL SELECT DATEADD(month,1,date) from months where DATEADD(month,1,date)&lt;= (SELECT EndDate FROM  Education_Year WHERE (EducationYearID = @EducationYearID)))
select  FORMAT(Date,'MMM yyyy') as Month_Name, date from months">
                <SelectParameters>
                    <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                </SelectParameters>
            </asp:SqlDataSource>
        </div>
        <div class="form-group">
            <asp:RadioButtonList CssClass="form-control" ID="EmpTypeRadioButtonList" runat="server" AutoPostBack="True" RepeatDirection="Horizontal">
                <asp:ListItem Selected="True" Value="%">All Employee</asp:ListItem>
                <asp:ListItem>Teacher</asp:ListItem>
                <asp:ListItem>Staff</asp:ListItem>
            </asp:RadioButtonList>
        </div>
        <div class="form-group">
            <input type="button" class="btn btn-green" value="Print" onclick="window.print();" />
        </div>
    </div>

    <div class="table-responsive">
        <asp:GridView ID="EmployeeGridView" AllowSorting="True" runat="server" CssClass="mGrid" AutoGenerateColumns="False" DataKeyNames="MonthlyPayorderID,Employee_PayorderID,ID,Name,Phone,EmployeeID,Due,PaidStatus,PaidAmount" DataSourceID="EmplyoeePayOrderSQL" OnRowDataBound="EmployeeGridView_RowDataBound" ShowFooter="True">
            <Columns>
                <asp:TemplateField>
                    <HeaderTemplate>
                        <asp:CheckBox ID="AllCheckBox" runat="server" Text=" " />
                    </HeaderTemplate>
                    <ItemTemplate>
                        <asp:CheckBox ID="AddCheckBox" runat="server" Text=" " Enabled='<%# Eval("PaidStatus").ToString() != "Paid" %>' />
                    </ItemTemplate>
                    <HeaderStyle CssClass="d-print-none" />
                    <ItemStyle CssClass="d-print-none" />
                    <FooterStyle CssClass="d-print-none" />
                </asp:TemplateField>
                <asp:BoundField DataField="ID" HeaderText="ID" SortExpression="ID" />
                <asp:BoundField DataField="Name" HeaderText="Name" SortExpression="Name" ReadOnly="True">
                    <ItemStyle HorizontalAlign="Left" />
                </asp:BoundField>
                <asp:TemplateField HeaderText="Salary" SortExpression="PayorderAmount">
                    <ItemTemplate>
                        <span class="PayorderAmount"><%# Eval("PayorderAmount") %></span>
                    </ItemTemplate>
                    <FooterTemplate>
                        <label class="GTPayorderAmount"></label>
                    </FooterTemplate>
                    <ItemStyle HorizontalAlign="Right" />
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Allowance" SortExpression="Allowance">
                    <ItemTemplate>
                        <span class="Allowance"><%# Eval("Allowance") %></span>
                    </ItemTemplate>
                    <FooterTemplate>
                        <label class="GTAllowance"></label>
                    </FooterTemplate>
                    <ItemStyle HorizontalAlign="Right" />
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Bonus" SortExpression="Bonus">
                    <ItemTemplate>
                        <span class="Bonus"><%# Eval("Bonus") %></span>
                    </ItemTemplate>
                    <FooterTemplate>
                        <label class="GTBonus"></label>
                    </FooterTemplate>
                    <ItemStyle HorizontalAlign="Right" />
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Gross Salary" SortExpression="GrossSalary">
                    <ItemTemplate>
                        <span class="GrossSalary"><%# Eval("GrossSalary") %></span>
                    </ItemTemplate>
                    <FooterTemplate>
                        <label class="GTGrossSalary"></label>
                    </FooterTemplate>
                    <ItemStyle HorizontalAlign="Right" />
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Diduction" SortExpression="Diduction">
                    <ItemTemplate>
                        <span class="Diduction"><%# Eval("Diduction") %></span>
                    </ItemTemplate>
                    <FooterTemplate>
                        <label class="GTDiduction"></label>
                    </FooterTemplate>
                    <ItemStyle HorizontalAlign="Right" />
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Fine" SortExpression="Fine">
                    <ItemTemplate>
                        <span class="Fine"><%# Eval("Fine") %></span>
                    </ItemTemplate>
                    <FooterTemplate>
                        <label class="GTFine"></label>
                    </FooterTemplate>
                    <ItemStyle HorizontalAlign="Right" />
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Net Salary" SortExpression="InTotalSalary">
                    <ItemTemplate>
                        <span class="InTotalSalary"><%# Eval("InTotalSalary") %></span>
                    </ItemTemplate>
                    <FooterTemplate>
                        <label class="GTInTotalSalary"></label>
                    </FooterTemplate>
                    <ItemStyle HorizontalAlign="Right" />
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Paid" SortExpression="PaidAmount">
                    <ItemTemplate>
                        <span class="PaidAmount"><%# Eval("PaidAmount") %></span>
                    </ItemTemplate>
                    <FooterTemplate>
                        <label class="GTPaidAmount"></label>
                    </FooterTemplate>
                    <ItemStyle HorizontalAlign="Right" />
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Due" SortExpression="Due">
                    <ItemTemplate>
                        <span class="DueAmount"><%#Eval("Due") %></span>
                    </ItemTemplate>
                    <FooterTemplate>
                        <label class="GTDueAmount"></label>
                    </FooterTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Pay">
                    <ItemTemplate>
                        <asp:TextBox ID="PayTextBox" Enabled="false" CssClass="form-control" runat="server" onkeypress="return isNumberKey(event)" autocomplete="off" Visible='<%# Eval("PaidStatus").ToString() != "Paid" %>'></asp:TextBox>

                        <a data-name="<%# Eval("Name")%>" data-id1="<%# Eval("EmployeeID")%>" data-id2="<%# Eval("Employee_PayorderID")%>" class="btn-unpaid <%# Eval("PaidStatus").ToString() == "Paid" ? "" : "hiddenPaid" %>">Unpaid</a>
                        <a data-name="<%# Eval("Name")%>" data-id1="<%# Eval("EmployeeID")%>" data-id2="<%# Eval("Employee_PayorderID")%>" class="btn-unpaid <%# Eval("PaidStatus").ToString() != "Paid" && Convert.ToDouble(Eval("PaidAmount")) > 0 ? "" : "hiddenPaid" %>">Unpaid</a>
                    </ItemTemplate>
                    <HeaderStyle CssClass="d-print-none" />
                    <ItemStyle Width="100px" CssClass="d-print-none" />
                    <FooterStyle CssClass="d-print-none" />
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Signature">
                    <ItemTemplate>
                    </ItemTemplate>
                    <FooterStyle CssClass="d-none d-print-table-cell" />
                    <HeaderStyle CssClass="d-none d-print-table-cell" />
                    <ItemStyle Width="100px" CssClass="d-none d-print-table-cell" />
                </asp:TemplateField>
            </Columns>
            <FooterStyle CssClass="GridFooter" />
        </asp:GridView>

        <asp:SqlDataSource ID="EmplyoeePayOrderSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT Employee_Payorder_Monthly.MonthlyPayorderID, VW_Emp_Info.ID, VW_Emp_Info.FirstName + ' ' + VW_Emp_Info.LastName AS Name,VW_Emp_Info.Phone, VW_Emp_Info.Designation, VW_Emp_Info.Bank_AccNo, Employee_Payorder_Monthly.WorkingDays, Employee_Payorder.PayorderAmount, Employee_Payorder.Allowance, Employee_Payorder.Bonus, Employee_Payorder.Diduction, Employee_Payorder.Fine, Employee_Payorder.InTotalSalary, Employee_Payorder.Employee_PayorderID, Employee_Payorder.EmployeeID, Employee_Payorder.GrossSalary, Employee_Payorder_Monthly.FineCountDays, Employee_Payorder.PaidAmount, Employee_Payorder.Due, Employee_Payorder.PaidStatus FROM Employee_Payorder_Monthly INNER JOIN Employee_Payorder ON Employee_Payorder_Monthly.Employee_PayorderID = Employee_Payorder.Employee_PayorderID INNER JOIN VW_Emp_Info ON Employee_Payorder.EmployeeID = VW_Emp_Info.EmployeeID WHERE (Employee_Payorder.Employee_Payorder_NameID = @Employee_Payorder_NameID) AND (Employee_Payorder_Monthly.MonthName = @MonthName) AND (Employee_Payorder.SchoolID = @SchoolID) AND (VW_Emp_Info.EmployeeType LIKE @EmployeeType)" UpdateCommand="UPDATE Employee_Payorder SET PaidAmount = PaidAmount + @PaidAmount WHERE (Employee_PayorderID = @Employee_PayorderID) AND (SchoolID = @SchoolID)">
            <SelectParameters>
                <asp:ControlParameter ControlID="PayorderNameDropDownList" Name="Employee_Payorder_NameID" PropertyName="SelectedValue" />
                <asp:ControlParameter ControlID="MonthNameDropDownList" Name="MonthName" PropertyName="SelectedItem.Text" />
                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                <asp:ControlParameter ControlID="EmpTypeRadioButtonList" Name="EmployeeType" PropertyName="SelectedValue" />
            </SelectParameters>
            <UpdateParameters>
                <asp:Parameter Name="PaidAmount" />
                <asp:Parameter Name="Employee_PayorderID" />
                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
            </UpdateParameters>
        </asp:SqlDataSource>
        <asp:SqlDataSource ID="Employee_Payorder_RecordsSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" InsertCommand="INSERT INTO Employee_Payorder_Records(Employee_PayorderID, SchoolID, RegistrationID, EducationYearID, EmployeeID, AccountID, Amount, Paid_For, Paid_date) VALUES (@Employee_PayorderID, @SchoolID, @RegistrationID, @EducationYearID, @EmployeeID, @AccountID, @Amount, @Paid_For, @Paid_date)" SelectCommand="SELECT * FROM Employee_Payorder_Records" DeleteCommand="DELETE FROM Employee_Payorder_Records WHERE (SchoolID = @SchoolID) AND (EmployeeID = @EmployeeID) AND (Employee_PayorderID = @Employee_PayorderID)
UPDATE Employee_Payorder SET PaidAmount = 0 WHERE (SchoolID = @SchoolID) AND (EmployeeID = @EmployeeID) AND (Employee_PayorderID = @Employee_PayorderID)">
            <DeleteParameters>
                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                <asp:Parameter Name="EmployeeID" />
                <asp:Parameter Name="Employee_PayorderID" />
            </DeleteParameters>
            <InsertParameters>
                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" />
                <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                <asp:ControlParameter ControlID="AccountDropDownList" Name="AccountID" PropertyName="SelectedValue" />
                <asp:ControlParameter ControlID="Paid_Date_TextBox" Name="Paid_date" PropertyName="Text" />
                <asp:Parameter Name="Employee_PayorderID" />
                <asp:Parameter Name="EmployeeID" />
                <asp:Parameter Name="Amount" />
                <asp:Parameter Name="Paid_For" />
            </InsertParameters>
        </asp:SqlDataSource>

        <asp:SqlDataSource ID="SMS_OtherInfoSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
            InsertCommand="INSERT INTO SMS_OtherInfo(SMS_Send_ID, SchoolID, StudentID, TeacherID, EducationYearID) VALUES (@SMS_Send_ID, @SchoolID, @StudentID, @TeacherID, @EducationYearID)" SelectCommand="SELECT * FROM [SMS_OtherInfo]">
            <InsertParameters>
                <asp:Parameter Name="SMS_Send_ID" DbType="Guid" />
                <asp:Parameter Name="SchoolID" />
                <asp:Parameter Name="StudentID" />
                <asp:Parameter Name="TeacherID" />
                <asp:Parameter Name="EducationYearID" />
            </InsertParameters>
        </asp:SqlDataSource>




        <% if (EmployeeGridView.Rows.Count > 0)
            { %>
        <label id="SalaryTotal" class="mt-2 green-text"></label>
        <asp:CustomValidator ID="CV" runat="server" ClientValidationFunction="Validate" ErrorMessage="You do not select any employee from employee list." ForeColor="Red" ValidationGroup="A"> </asp:CustomValidator>

        <div class="form-inline d-print-none">
            <div class="form-group">
                <asp:DropDownList ID="AccountDropDownList" runat="server" CssClass="form-control" DataSourceID="AccountSQL" DataTextField="AccountName" DataValueField="AccountID">
                </asp:DropDownList>
                <asp:SqlDataSource ID="AccountSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT AccountID,AccountName + Format(AccountBalance,' (##,###.## tk)') as AccountName FROM [Account] WHERE ([SchoolID] = @SchoolID) AND (AccountBalance &lt;&gt; 0)">
                    <SelectParameters>
                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" Type="Int32" />
                    </SelectParameters>
                </asp:SqlDataSource>
                <asp:RequiredFieldValidator ID="RequiredFieldValidator6" runat="server" ControlToValidate="AccountDropDownList" CssClass="EroorStar" ErrorMessage="*" ValidationGroup="A"></asp:RequiredFieldValidator>
            </div>
            <div class="form-group">
                <asp:TextBox ID="Paid_Date_TextBox" placeholder="Paid Date" runat="server" autocomplete="off" CssClass="form-control Datetime" onDrop="blur();return false;" onkeypress="return isNumberKey(event)" onpaste="return false"></asp:TextBox>
                <asp:RequiredFieldValidator ID="dRfv" runat="server" ControlToValidate="Paid_Date_TextBox" CssClass="EroorStar" ErrorMessage="*" ValidationGroup="A"></asp:RequiredFieldValidator>
            </div>
            <div class="form-group">
                <%--<asp:CheckBox runat="server" ID="SMSCheckBox" Text="Send SMS"/>--%>
                <label>SMS Send :</label>&nbsp
                <asp:RadioButtonList CssClass="form-control" ID="SMSRadioButtonList" runat="server" RepeatDirection="Horizontal">
                    <asp:ListItem Selected="True" Value="Yes">Yes</asp:ListItem>
                    <asp:ListItem Value="No">No</asp:ListItem>                    
                </asp:RadioButtonList>


                <asp:Button ID="PayButton" ValidationGroup="A" runat="server" Text="Pay Salary" CssClass="btn btn-primary" OnClick="PayButton_Click" />
                <label id="Error" class="red-text"></label>
                <asp:Label ID="AccerrorLabel" CssClass="EroorSummer" runat="server"></asp:Label>
                <asp:Label ID="ErrorLabel" runat="server" CssClass="EroorSummer"></asp:Label>
            </div>
        </div>
        <% } %>
    </div>

    <div class="modal fade" data-keyboard="false" data-backdrop="static" id="unpaidSalaryModal" tabindex="-1" role="dialog" aria-hidden="true">
        <div class="modal-dialog modal-lg" role="document">
            <div class="modal-content">
                <div class="modal-header">
                    <div id="employeeName" class="title">
                    </div>

                    <button type="button" class="close" data-dismiss="modal" aria-label="Close"><span aria-hidden="true">&times;</span></button>
                </div>
                <div class="modal-body">
                    <table class="table table-sm text-dark">
                        <thead>
                            <tr>
                                <th><strong>Amount</strong></th>
                                <th><strong>Paid Date</strong></th>
                                <th><strong>Account Name</strong></th>
                                <th><strong>Delete</strong></th>
                            </tr>
                        </thead>
                        <tbody id="tBody">
                        </tbody>
                    </table>
                </div>
            </div>
        </div>
    </div>


    <asp:HiddenField ID="GrandTotalHF" runat="server" />
    <script>
        $(document).ready(function () {
            $('#SMSCheckBox').attr('checked', true);

        });


        $(function () {
            $(".Datetime").datepicker({
                format: 'dd M yyyy',
                todayBtn: "linked",
                todayHighlight: true,
                autoclose: true
            }).datepicker("setDate", "0");

            if ($('[id*=MonthNameDropDownList] :selected').index() > 0) {
                $('#MName').text(` For: ${$('[id*=MonthNameDropDownList] :selected').text()}`);
            }

            $('input[type=checkbox]').prop("checked", false);



            //Select All Checkbox 

            $("[id*=AllCheckBox]").change(function () {
                var AllCheckBox = $(this);
                const grid = $(this).closest("table");

                $("input[type=checkbox]", grid).each(function () {
                    if (AllCheckBox.is(":checked")) {
                        $('input[type="text"]', $(this).closest("tr")).val($('.DueAmount', $(this).closest("tr")).text()).attr('required', 'required').removeAttr("disabled");
                        $(this).prop('checked', true).attr("checked", "checked");
                        $($(this).closest("tr")).addClass("selected");
                    } else {
                        $('input[type="text"]', $(this).closest("tr")).removeAttr('required', 'required').val('').attr("disabled", "disabled");
                        $(this).prop('checked', false).removeAttr("checked");
                        $($(this).closest("tr")).removeClass("selected");
                    }
                });

                GrandTotal();
            });

            $("[id*=AddCheckBox]").change(function () {
                const grid = $(this).closest("table");
                const AllCheckBox = $("[id*=AllCheckBox]", grid);

                if (!$(this).is(":checked")) {
                    $('input[type="text"]', $(this).closest("tr")).removeAttr('required', 'required').val('').attr("disabled", "disabled");
                    $($(this).closest("tr")).removeClass("selected");
                    AllCheckBox.removeAttr("checked");
                } else {
                    $('input[type="text"]', $(this).closest("tr")).val($('.DueAmount', $(this).closest("tr")).text()).attr('required', 'required').removeAttr("disabled");
                    $($(this).closest("tr")).addClass("selected");
                    if ($("[id*=AddCheckBox]", grid).length == $("[id*=AddCheckBox]:checked", grid).length) {
                        AllCheckBox.attr("checked", "checked");
                    }
                }

                GrandTotal();
            });

            $("[id*=PayTextBox]").on("keyup keypress paste drop change", function () {
                const td = $("td", $(this).closest("tr"));
                const due = parseFloat($('.DueAmount', td).text());
                const paid = parseFloat($(this).val());

                if (paid > due) {
                    $("#Error").text("Paid Amount Greater than due amount");
                    $("[id*=PayButton]").prop("disabled", !0).removeClass("btn btn-primary");
                    $('.DueAmount', td).css("color", "red");
                    $($(this), td).css("color", "red");
                }
                else {
                    $("#Error").text("");
                    $("[id*=PayButton]").prop("disabled", !1).addClass("btn btn-primary");
                    $('.DueAmount', td).css("color", "#333");
                    $($(this), td).css("color", "#333");
                }
                GrandTotal();
            });


            //Sum Total  
            var payOrderAmount = 0;
            $(".PayorderAmount").each(function () {
                payOrderAmount = payOrderAmount + parseFloat($(this).html());
            });
            $(".GTPayorderAmount").html(`৳${payOrderAmount}`);

            var Allowance = 0;
            $(".Allowance").each(function () {
                Allowance = Allowance + parseFloat($(this).html());
            });
            $(".GTAllowance").html(`৳${Allowance}`);

            var Bonus = 0;
            $(".Bonus").each(function () {
                Bonus = Bonus + parseFloat($(this).html());
            });
            $(".GTBonus").html(`৳${Bonus}`);

            var GrossSalary = 0;
            $(".GrossSalary").each(function () {
                GrossSalary = GrossSalary + parseFloat($(this).html());
            });
            $(".GTGrossSalary").html(`৳${GrossSalary}`);


            var deduction = 0;
            $(".Diduction").each(function () {
                deduction = deduction + parseFloat($(this).html());
            });
            $(".GTDiduction").html(`৳${deduction}`);


            var Fine = 0;
            $(".Fine").each(function () {
                Fine = Fine + parseFloat($(this).html());
            });
            $(".GTFine").html(`৳${Fine}`);


            var InTotalSalary = 0;
            $(".InTotalSalary").each(function () {
                InTotalSalary = InTotalSalary + parseFloat($(this).html());
            });
            $(".GTInTotalSalary").html(`৳${InTotalSalary}`);

            var PaidAmount = 0;
            $(".PaidAmount").each(function () {
                PaidAmount = PaidAmount + parseFloat($(this).html());
            });
            $(".GTPaidAmount").html(`৳${PaidAmount}`);

            var DueAmount = 0;
            $(".DueAmount").each(function () {
                DueAmount = DueAmount + parseFloat($(this).html());
            });
            $(".GTDueAmount").html(`৳${DueAmount}`);
        });

        //calculate grand total
        function GrandTotal() {
            var sum = 0;
            $("[id*=PayTextBox]").each(function () {
                if ($(this).val() !== "") {
                    sum += +$(this).val();
                }
            });

            $("[id*=GrandTotalHF]").val(sum);
            $("#SalaryTotal").text(`Total: ${sum} Tk`);
        }


        //select at least one checkbox students gridView
        function Validate(d, c) {
            if ($('[id*=EmployeeGridView] tr').length) {
                for (var b = document.getElementById("<%=EmployeeGridView.ClientID %>").getElementsByTagName("input"), a = 0; a < b.length; a++) {
                    if ("checkbox" === b[a].type && b[a].checked) {
                        c.IsValid = true;
                        return;
                    }
                }
                c.IsValid = false;
            }
        }

        //input number only
        function isNumberKey(a) { a = a.which ? a.which : event.keyCode; return 46 !== a && 31 < a && (48 > a || 57 < a) ? false : true };


        //unpaid salary
        const onTable = document.querySelector(".mGrid");
        const unpaidSalaryModal = $("#unpaidSalaryModal");

        if (onTable) {
            onTable.addEventListener("click", function (evt) {
                const element = evt.target;
                const onUnpaid = element.classList.contains("btn-unpaid");

                if (!onUnpaid) return;

                const employeeName = document.getElementById("employeeName");
                const month = document.getElementById("body_MonthNameDropDownList");

                const info = `<h5 class="title mb-0"><strong>${element.getAttribute("data-name")}</strong></h5><i>Salary For: ${month.options[month.selectedIndex].text}</i>`
                employeeName.innerHTML = info;

                getPaidSalary(element.getAttribute("data-id1"), element.getAttribute("data-id2"));
            });
        }

        //get data
        function getPaidSalary(employeeId, employeePayOrderId) {
            $.ajax({
                type: "POST",
                url: "Salary_Payment.aspx/GetPaidRecords",
                data: JSON.stringify({ employeeId, employeePayOrderId }),
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: function (response) {
                    bindTableData(response.d);
                    unpaidSalaryModal.modal("show");
                },
                error: function (response) {
                    console.log(response.d);
                }
            });
        }

        //bind table data
        const tBody = document.getElementById("tBody");
        function bindTableData(response) {
            const data = JSON.parse(response);

            let html = '';
            data.forEach(item => {
                html += `<tr>
                            <td>${item.Amount}</td>
                            <td>${item.PaidDate}</td>
                            <td>${item.AccountName}</td>
                            <td class="text-center">
                                <i id="${item.EmployeePayOrderRecordId}" class="delete-salary fa fa-trash"></i>
                            </td>
                        </tr>`;

            });

            tBody.innerHTML = html;
        }

        //on delete 
        tBody.addEventListener("click", function (evt) {
            const element = evt.target;
            const onClick = element.classList.contains("delete-salary");

            if (!onClick) return;

            $.ajax({
                type: "POST",
                url: "Salary_Payment.aspx/PaidRecordDelete",
                data: JSON.stringify({ id: element.id }),
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: function (response) {
                    if (response.d) {
                        element.parentElement.parentElement.remove();
                    }
                },
                error: function (response) {
                    console.log(response.d);
                }
            });
        });

        //on close modal
        unpaidSalaryModal.on('hidden.bs.modal', function () {
            location.href = location.pathname + location.search + location.hash;
        });
    </script>
</asp:Content>
