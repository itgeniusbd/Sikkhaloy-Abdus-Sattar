<%@ Page Title="Collect Payment" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Payment_Collection.aspx.cs" Inherits="EDUCATION.COM.ACCOUNTS.Payment.Payment_Collection" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="CSS/Payment_Collection.css?version=1.0.0" rel="stylesheet" />

    <meta http-equiv="content-type" content="application/xhtml+xml; charset=UTF-8" />
    <style>
        .PD_Name_Class {
            color: #282828;
            font-size: 18px;
        }

        .modal-body {
            max-height: 500px;
            overflow: auto;
        }

        .Print_ins_Name {
            text-align: center;
            margin-bottom: 10px;
            color: #000;
            padding-bottom: 5px;
            border-bottom: 1px solid #000;
            display: none;
        }

        #Print_InsName {
            font-size: 30px;
        }

        #P_ClassName {
            font-size: 15px;
        }

        .info {
            width: 100%;
        }

            .info ul {
                margin: 0;
                padding: 0;
            }

                .info ul li {
                    border-bottom: 1px solid #d6e0eb;
                    color: #5d6772;
                    font-size: 15px;
                    line-height: 23px;
                    list-style: outside none none;
                    margin: 6px 0 0;
                    padding-bottom: 5px;
                    padding-left: 2px;
                }

                    .info ul li:last-child {
                        border-bottom: none;
                    }

        .modalPopup {
            background-color: #FFFFFF;
            width: 100%;
            border: 3px solid #0DA9D0;
            border-radius: 12px;
            padding: 0;
        }

            .modalPopup .header {
                background-color: #2FBDF1;
                height: 30px;
                color: White;
                line-height: 30px;
                text-align: center;
                font-weight: bold;
            }

            .modalPopup .body {
                min-height: 50px;
                line-height: 30px;
                text-align: center;
                padding: 5px;
            }

            .modalPopup .footer {
                padding: 3px;
            }

            .modalPopup .button {
                height: 23px;
                color: White;
                line-height: 23px;
                text-align: center;
                font-weight: bold;
                cursor: pointer;
                background-color: #9F9F9F;
                border: 1px solid #5C5C5C;
            }

        @media print {
            .noprint {
                display: none
            }
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <h3>Collect Payment</h3>

    <!--find student student id-->
    <div class="form-inline">
        <div class="form-group">
            <asp:TextBox ID="SearchIDTextBox" autocomplete="off" runat="server" CssClass="form-control" placeholder="Enter ID"></asp:TextBox>
        </div>
        <div class="form-group">
            <asp:Button ID="SearchButton" runat="server" CssClass="btn btn-primary" Text="Find" ValidationGroup="A" />
            <asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" ControlToValidate="SearchIDTextBox" CssClass="EroorStar" ErrorMessage="Enter Student ID" ValidationGroup="A"></asp:RequiredFieldValidator>
        </div>
    </div>

    <!--student info and payment record-->
    <div class="row">
        <div class="col-lg-8">
            <asp:FormView ID="StudentInfoFormView" runat="server" DataKeyNames="SMSPhoneNo,StudentID,StudentsName,StudentClassID,ClassID,EducationYearID,ID" DataSourceID="StudentInfoSQL" RenderOuterTable="false">
                <ItemTemplate>
                    <div class="z-depth-1 p-3 mb-4">
                        <div class="d-flex flex-sm-row flex-column text-center text-sm-left">
                            <div class="student-photo">
                                <img alt="No Image" src="/Handeler/Student_Photo.ashx?SID=<%#Eval("StudentImageID") %>" class="img-thumbnail rounded-circle img-fluid z-depth-1" />
                                <div class="student-activation <%# Eval("Status").ToString() == "Active" ?"active-status":"in-active-status" %>">
                                    <%# Eval("Status") %>
                                </div>
                            </div>
                            <div class="info">
                                <ul>
                                    <li>
                                        <strong>(<span id="IDLabel"><%# Eval("ID") %></span>) <%# Eval("StudentsName") %></strong>
                                    </li>
                                    <li><b>Fathers Name: </b><%# Eval("FathersName") %>
                                    </li>
                                    <li>
                                        <b>Class:</b>
                                        <%# Eval("Class") %>
                                    </li>
                                    <li>Roll No:<%# Eval("RollNo") %><%#Eval("Section",", Section: {0}") %><%# Eval("Shift",", Shift: {0}") %></li>
                                    <li><b>Phone: </b><%# Eval("SMSPhoneNo") %></li>
                                    <li><b>Session: </b><%# Eval("EducationYear") %>  <i class="fa fa-hand-o-right"></i><a target="_blank" href="/Admission/Student_Report/Report.aspx?Student=<%# Eval("StudentID") %>&Student_Class=<%# Eval("StudentClassID") %>">Full Details</a> -------- <i class="fa fa-hand-o-right"></i><a target="_blank" href="../../Admission/New_Student_Admission/Admission_Form.aspx?Student=<%# Eval("StudentID") %>&StudentClass=<%# Eval("StudentClassID") %>">Print Admission Form</a></li>

                                </ul>


                                <button type="button" data-toggle="modal" data-target="#Others_Modal" class="btn btn-outline-success btn-md m-0">Add More Payment</button>
                            </div>
                        </div>
                    </div>
                </ItemTemplate>
            </asp:FormView>
            <asp:SqlDataSource ID="StudentInfoSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                SelectCommand="SELECT Student.StudentID, StudentsClass.StudentClassID, StudentsClass.ClassID, Student.StudentImageID, Student.ID, Student.StudentsName, Student.SMSPhoneNo, CreateClass.Class, CreateSection.Section, CreateSubjectGroup.SubjectGroup, CreateShift.Shift, StudentsClass.RollNo, Student.FathersName, Education_Year.EducationYearID, Education_Year.EducationYear, Student.Status FROM StudentsClass INNER JOIN Student ON StudentsClass.StudentID = Student.StudentID INNER JOIN Education_Year ON StudentsClass.EducationYearID = Education_Year.EducationYearID LEFT OUTER JOIN CreateShift ON StudentsClass.ShiftID = CreateShift.ShiftID LEFT OUTER JOIN CreateSubjectGroup ON StudentsClass.SubjectGroupID = CreateSubjectGroup.SubjectGroupID LEFT OUTER JOIN CreateSection ON StudentsClass.SectionID = CreateSection.SectionID LEFT OUTER JOIN CreateClass ON StudentsClass.ClassID = CreateClass.ClassID WHERE (Student.ID = @ID) AND (StudentsClass.SchoolID = @SchoolID) AND (StudentsClass.Class_Status IS NULL)">
                <SelectParameters>
                    <asp:ControlParameter ControlID="SearchIDTextBox" Name="ID" PropertyName="Text" />
                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                </SelectParameters>
            </asp:SqlDataSource>
        </div>

        <div class="col-lg-4">
            <asp:UpdatePanel ID="UpdatePanel2" runat="server">
                <ContentTemplate>
                    <div class="mb-4">
                        <asp:GridView ID="PaidRecordGridView" runat="server" AutoGenerateColumns="False" CssClass="mGrid" DataSourceID="PRecordSQL" AllowPaging="True" PageSize="4">
                            <Columns>
                                <asp:TemplateField HeaderText="Receipt">
                                    <ItemTemplate>
                                        <asp:LinkButton ID="MSNLinkButton" runat="server" CommandArgument='<%# Eval("MoneyReceiptID") %>' Text='<%# Eval("MoneyReceipt_SN") %>' ToolTip="Click To Details" OnCommand="MSNLinkButton_Command" />
                                        <small class="d-block"><%# Eval("PaidDate", "{0:d-MMM-yy (hh:mm tt)}") %></small>
                                    </ItemTemplate>
                                    <HeaderStyle HorizontalAlign="Left" />
                                    <ItemStyle HorizontalAlign="Left" />
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Paid">
                                    <FooterTemplate>
                                        <button id="Viwallbutton0" class="btn btn-outline-success btn-md m-0" data-target="#myModal" data-toggle="modal" type="button">
                                            View All
                                        </button>
                                    </FooterTemplate>
                                    <ItemTemplate>
                                        <%# Eval("TotalAmount") %> Tk
                                     <small class="d-block">
                                         <asp:LinkButton ID="Print_LinkButton" runat="server" CommandArgument='<%# Eval("MoneyReceiptID") %>' ToolTip="Click To Print" OnCommand="Print_LinkButton_Command"><i class="fa fa-print"></i> Print</asp:LinkButton></small>
                                    </ItemTemplate>
                                    <HeaderStyle HorizontalAlign="Right" />
                                    <ItemStyle HorizontalAlign="Right" />
                                </asp:TemplateField>
                            </Columns>
                            <PagerStyle CssClass="pgr" />
                        </asp:GridView>
                        <asp:SqlDataSource ID="PRecordSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                            SelectCommand="SELECT Income_MoneyReceipt.MoneyReceipt_SN, Income_MoneyReceipt.TotalAmount, Income_MoneyReceipt.PaidDate, Income_MoneyReceipt.MoneyReceiptID FROM Income_MoneyReceipt INNER JOIN Student ON Income_MoneyReceipt.StudentID = Student.StudentID WHERE (Income_MoneyReceipt.EducationYearID = @EducationYearID) AND (Student.ID = @ID) AND (Income_MoneyReceipt.SchoolID = @SchoolID)
ORDER BY Income_MoneyReceipt.PaidDate DESC">
                            <SelectParameters>
                                <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                                <asp:ControlParameter ControlID="SearchIDTextBox" Name="ID" PropertyName="Text" />
                                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                            </SelectParameters>
                        </asp:SqlDataSource>

                    </div>

                    <!--Paid Record Modal -->
                    <div class="modal fade" id="paid-record-modal" tabindex="-1" role="dialog" aria-labelledby="myModalLabel" aria-hidden="true">
                        <div class="modal-dialog modal-lg" role="document">
                            <div class="modal-content">
                                <div class="modal-header">
                                    <div class="title">Paid Record Details</div>
                                    <button type="button" class="close" data-dismiss="modal" aria-label="Close"><span aria-hidden="true">&times;</span></button>
                                </div>
                                <div class="modal-body">
                                    <asp:GridView ID="PaidDetailsGridView" runat="server" AutoGenerateColumns="False" DataSourceID="PaidRecordsSQL" CssClass="mGrid" ShowFooter="True" RowStyle-CssClass="Rows">
                                        <Columns>
                                            <asp:BoundField DataField="PayFor" HeaderText="Pay For" />
                                            <asp:TemplateField HeaderText="Role">
                                                <ItemTemplate>
                                                    <asp:Label ID="RoleLabel" runat="server" Text='<%# Eval("Role") %>'></asp:Label>
                                                </ItemTemplate>
                                            </asp:TemplateField>
                                            <asp:TemplateField HeaderText="Paid" SortExpression="PaidAmount">
                                                <ItemTemplate>
                                                    <label class="paid-record-paid-amount"><%# Eval("PaidAmount") %></label>
                                                </ItemTemplate>
                                                <FooterTemplate>
                                                    <label id="paid-record-grand-total"></label>
                                                </FooterTemplate>
                                            </asp:TemplateField>
                                        </Columns>

                                        <RowStyle CssClass="Rows" />
                                    </asp:GridView>
                                    <asp:SqlDataSource ID="PaidRecordsSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT Income_PaymentRecord.PaidAmount, Income_PaymentRecord.PayFor + ' (' + Education_Year.EducationYear + ')' AS PayFor, Income_PaymentRecord.PaidDate, Income_Roles.Role FROM Income_PaymentRecord INNER JOIN Income_Roles ON Income_PaymentRecord.RoleID = Income_Roles.RoleID INNER JOIN Income_MoneyReceipt ON Income_PaymentRecord.MoneyReceiptID = Income_MoneyReceipt.MoneyReceiptID INNER JOIN Education_Year ON Income_PaymentRecord.EducationYearID = Education_Year.EducationYearID WHERE (Income_PaymentRecord.SchoolID = @SchoolID) AND (Income_MoneyReceipt.MoneyReceiptID = @MoneyReceiptID)">
                                        <SelectParameters>
                                            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                                            <asp:Parameter Name="MoneyReceiptID" />
                                        </SelectParameters>
                                    </asp:SqlDataSource>

                                    <asp:FormView ID="RByFormView" runat="server" CssClass="Hide" DataSourceID="ReceivedBySQL" Width="100%">
                                        <ItemTemplate>
                                            <div class="RecvBy">
                                                Received By: <%# Eval("Name") %> (© Sikkhaloy.com)
                                            </div>
                                        </ItemTemplate>
                                    </asp:FormView>
                                    <asp:SqlDataSource ID="ReceivedBySQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT Admin.FirstName + ' ' + Admin.LastName AS Name FROM Admin INNER JOIN Income_MoneyReceipt ON Admin.RegistrationID = Income_MoneyReceipt.RegistrationID WHERE (Income_MoneyReceipt.SchoolID = @SchoolID) AND (Income_MoneyReceipt.MoneyReceiptID = @MoneyReceiptID)">
                                        <SelectParameters>
                                            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                                            <asp:Parameter Name="MoneyReceiptID" />
                                        </SelectParameters>
                                    </asp:SqlDataSource>
                                </div>
                            </div>
                        </div>
                    </div>
                </ContentTemplate>
            </asp:UpdatePanel>


            <%if (PaidRecordGridView.Rows.Count > 0)
                { %>
            <div class="form-group">

                <button id="Viwallbutton" type="button" data-toggle="modal" data-target="#myModal" class="btn btn-outline-success btn-md m-0">View All</button>

            </div>

            <%} %>
        </div>
    </div>

    <!--current due-->
    <asp:FormView ID="CurrentDueFormView" runat="server" DataSourceID="TotalDue_ByID_ODS" RenderOuterTable="false">
        <ItemTemplate>
            <div class="current-due-total">
                CURRENT DUE: <%# Eval("Due") %> TK
            </div>
        </ItemTemplate>
    </asp:FormView>
    <asp:ObjectDataSource ID="TotalDue_ByID_ODS" runat="server" OldValuesParameterFormatString="original_{0}" SelectMethod="GetData" TypeName="EDUCATION.COM.PaymentDataSetTableAdapters.TotalDue_ByIDTableAdapter">
        <SelectParameters>
            <asp:ControlParameter ControlID="SearchIDTextBox" DefaultValue="0" Name="ID" PropertyName="Text" Type="String" />
            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" Type="Int32" />
        </SelectParameters>
    </asp:ObjectDataSource>

    <!--due gridview-->
    <div id="payment-container" class="table-responsive">
        <asp:GridView ID="DueGridView" runat="server" AutoGenerateColumns="False" CssClass="mGrid" DataKeyNames="Role,PayOrderID,Amount,StudentID,StudentClassID,RoleID,PayFor,StartDate,EducationYearID" DataSourceID="DueSQL" OnRowDataBound="DueGridView_RowDataBound">
            <Columns>
                <asp:TemplateField>
                    <ItemTemplate>
                        <asp:CheckBox ID="DueCheckBox" CssClass="due-checkbox" runat="server" Text=" " />
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:BoundField DataField="EducationYear" HeaderText="Session" SortExpression="EducationYear" />
                <asp:BoundField DataField="Class" HeaderText="Class" SortExpression="Class" />
                <asp:BoundField DataField="Role" HeaderText="Role" SortExpression="Role" />
                <asp:BoundField DataField="PayFor" HeaderText="Pay For" SortExpression="PayFor" />
                <asp:BoundField DataField="EndDate" HeaderText="End Date" SortExpression="EndDate" DataFormatString="{0:d MMM yyyy}" />
                <asp:BoundField DataField="Amount" HeaderText="Fee" SortExpression="Amount" />
                <asp:TemplateField HeaderText="Concession" SortExpression="Concession" ValidateRequestMode="Disabled">
                    <ItemTemplate>
                        <asp:TextBox ID="ConcessionTextBox" runat="server" type="number" Enabled="false" CssClass="form-control concession-input" Text='<%# Eval("Discount") %>' autocomplete="off"></asp:TextBox>
                    </ItemTemplate>
                    <ItemStyle Width="150px" />
                </asp:TemplateField>

                <asp:BoundField DataField="LateFee" HeaderText="Late Fee" SortExpression="LateFee" />
                <asp:BoundField DataField="PaidAmount" HeaderText="Paid" SortExpression="PaidAmount" />
                <asp:TemplateField HeaderText="Due" SortExpression="Due">
                    <ItemTemplate>
                        <%# Eval("Due") %>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Pay" SortExpression="Due">
                    <ItemTemplate>
                        <asp:TextBox ID="DueAmountTextBox" type="number" step="0.01" min="0" max='<%# Eval("Due") %>' required="" Enabled="false" CssClass="form-control due-input" runat="server" Text='<%# Eval("Due") %>' autocomplete="off" />
                    </ItemTemplate>
                    <ItemStyle Width="150px" />
                </asp:TemplateField>
            </Columns>
        </asp:GridView>
        <asp:SqlDataSource ID="DueSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
            SelectCommand="SELECT Income_PayOrder.PayOrderID, Income_PayOrder.StudentID, Income_PayOrder.EducationYearID, Income_PayOrder.StudentClassID, Income_PayOrder.ClassID, CreateClass.Class, 
                         Education_Year.EducationYear, Income_Roles.Role, Income_PayOrder.PayFor, Income_PayOrder.EndDate, Income_PayOrder.Amount, Income_PayOrder.Discount, Income_PayOrder.LateFee, 
                         Income_PayOrder.LateFee_Discount, Income_PayOrder.PaidAmount, CASE WHEN Income_PayOrder.EndDate &lt; GETDATE() - 1 THEN ISNULL(Income_PayOrder.Amount, 0) + ISNULL(Income_PayOrder.LateFee, 0) 
                         - ISNULL(Income_PayOrder.Discount, 0) - ISNULL(Income_PayOrder.PaidAmount, 0) - ISNULL(Income_PayOrder.LateFee_Discount, 0) ELSE ISNULL(Income_PayOrder.Amount, 0) 
                         - ISNULL(Income_PayOrder.Discount, 0) - ISNULL(Income_PayOrder.PaidAmount, 0) END AS Due, Income_PayOrder.RoleID, Income_PayOrder.StartDate
FROM            Income_PayOrder INNER JOIN
                         Income_Roles ON Income_PayOrder.RoleID = Income_Roles.RoleID INNER JOIN
                         Student ON Income_PayOrder.StudentID = Student.StudentID INNER JOIN
                         Education_Year ON Income_PayOrder.EducationYearID = Education_Year.EducationYearID INNER JOIN
                         CreateClass ON Income_PayOrder.ClassID = CreateClass.ClassID 
WHERE        (Income_PayOrder.Status = 'Due') AND (Student.ID = @ID)  AND (Income_PayOrder.SchoolID = @SchoolID) AND (Income_PayOrder.EducationYearID = @EducationYearID)
ORDER BY Income_PayOrder.EndDate"
            UpdateCommand="UPDATE Income_PayOrder SET Discount = @Discount WHERE (PayOrderID = @PayOrderID)">
            <SelectParameters>
                <asp:ControlParameter ControlID="SearchIDTextBox" DefaultValue="" Name="ID" PropertyName="Text" />
                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
            </SelectParameters>
            <UpdateParameters>
                <asp:Parameter Name="Discount" />
                <asp:Parameter Name="PayOrderID" />
            </UpdateParameters>
        </asp:SqlDataSource>
        <asp:SqlDataSource ID="Fee_DiscountSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
            SelectCommand="SELECT * FROM [Income_Discount_Record]" UpdateCommand="UPDATE Income_PayOrder SET Discount = @Discount WHERE (PayOrderID = @PayOrderID)">

            <UpdateParameters>
                <asp:Parameter Name="Discount" />
                <asp:Parameter Name="PayOrderID" />
            </UpdateParameters>
        </asp:SqlDataSource>

        <%if (OtherSessionGridView.Rows.Count > 0)
            {%>
        <h5 class="font-weight-bold mt-3">OTHERS SESSION DUE</h5>
        <asp:GridView ID="OtherSessionGridView" runat="server" AutoGenerateColumns="False" CssClass="mGrid" DataKeyNames="PayOrderID,Amount,StudentID,StudentClassID,RoleID,PayFor,StartDate,EducationYearID" DataSourceID="OtherSessionSQL" OnRowDataBound="OtherSessionGridView_RowDataBound">
            <Columns>
                <asp:TemplateField>
                    <ItemTemplate>
                        <asp:CheckBox ID="Other_Session_CheckBox" CssClass="due-checkbox" runat="server" Text=" " />
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:BoundField DataField="EducationYear" HeaderText="Session" SortExpression="EducationYear" />
                <asp:BoundField DataField="Class" HeaderText="Class" SortExpression="Class" />
                <asp:BoundField DataField="Role" HeaderText="Role" SortExpression="Role" />
                <asp:BoundField DataField="PayFor" HeaderText="Pay For" SortExpression="PayFor" />
                <asp:BoundField DataField="EndDate" HeaderText="End Date" SortExpression="EndDate" DataFormatString="{0:d MMM yyyy}" />
                <asp:BoundField DataField="Amount" HeaderText="Fee" SortExpression="Amount" />
                <asp:TemplateField HeaderText="Concession" SortExpression="Concession" ValidateRequestMode="Disabled">
                    <ItemTemplate>
                        <asp:TextBox ID="ConcessionTextBox" runat="server" type="number" Enabled="false" CssClass="form-control concession-input" Text='<%# Eval("Discount") %>' autocomplete="off"></asp:TextBox>
                    </ItemTemplate>
                    <ItemStyle Width="150px" />
                </asp:TemplateField>

                <asp:BoundField DataField="LateFee" HeaderText="Late Fee" SortExpression="LateFee" />
                <asp:BoundField DataField="PaidAmount" HeaderText="Paid" SortExpression="PaidAmount" />
                <asp:TemplateField HeaderText="Due" SortExpression="Due">
                    <ItemTemplate>
                        <%# Eval("Due") %>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Pay" SortExpression="Due">
                    <ItemTemplate>
                        <asp:TextBox ID="Other_Session_AmountTextBox" type="number" step="0.01" min="0" max='<%# Eval("Due") %>' required="" Enabled="false" CssClass="form-control due-input" runat="server" Text='<%# Eval("Due") %>' autocomplete="off" />
                    </ItemTemplate>
                    <ItemStyle Width="150px" />
                </asp:TemplateField>
            </Columns>
        </asp:GridView>
        <asp:SqlDataSource ID="OtherSessionSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
            SelectCommand="SELECT        Income_PayOrder.PayOrderID, Income_PayOrder.StudentID, Income_PayOrder.EducationYearID, Income_PayOrder.StudentClassID, Income_PayOrder.ClassID, CreateClass.Class, 
                         Education_Year.EducationYear, Income_Roles.Role, Income_PayOrder.PayFor, Income_PayOrder.EndDate, Income_PayOrder.Amount, Income_PayOrder.Discount, Income_PayOrder.LateFee, 
                         Income_PayOrder.LateFee_Discount, Income_PayOrder.PaidAmount, CASE WHEN Income_PayOrder.EndDate &lt; GETDATE() - 1 THEN ISNULL(Income_PayOrder.Amount, 0) + ISNULL(Income_PayOrder.LateFee, 0) 
                         - ISNULL(Income_PayOrder.Discount, 0) - ISNULL(Income_PayOrder.PaidAmount, 0) - ISNULL(Income_PayOrder.LateFee_Discount, 0) ELSE ISNULL(Income_PayOrder.Amount, 0) 
                         - ISNULL(Income_PayOrder.Discount, 0) - ISNULL(Income_PayOrder.PaidAmount, 0) END AS Due, Income_PayOrder.RoleID, Income_PayOrder.StartDate
FROM            Income_PayOrder INNER JOIN
                         Income_Roles ON Income_PayOrder.RoleID = Income_Roles.RoleID INNER JOIN
                         Student ON Income_PayOrder.StudentID = Student.StudentID INNER JOIN
                         Education_Year ON Income_PayOrder.EducationYearID = Education_Year.EducationYearID INNER JOIN
                         CreateClass ON Income_PayOrder.ClassID = CreateClass.ClassID 
WHERE        (Income_PayOrder.Status = 'Due') AND (Student.ID = @ID)  AND (Income_PayOrder.SchoolID = @SchoolID) AND (Income_PayOrder.EducationYearID &lt;&gt; @EducationYearID)
ORDER BY Income_PayOrder.EndDate"
            UpdateCommand="UPDATE Income_PayOrder SET Discount = @Discount WHERE (PayOrderID = @PayOrderID)">
            <SelectParameters>
                <asp:ControlParameter ControlID="SearchIDTextBox" DefaultValue="" Name="ID" PropertyName="Text" />
                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
            </SelectParameters>
            <UpdateParameters>
                <asp:Parameter Name="Discount" />
                <asp:Parameter Name="PayOrderID" />
            </UpdateParameters>
        </asp:SqlDataSource>
        <%}%>
    </div>

    <!--add more Payment Modal-->
    <div class="modal fade" id="Others_Modal" tabindex="-1" role="dialog" aria-hidden="true">
        <div class="modal-dialog" role="document">
            <div class="modal-content">
                <div class="modal-header">
                    <div class="title">Add More Payment</div>
                    <button type="button" class="close" data-dismiss="modal" aria-label="Close"><span aria-hidden="true">&times;</span></button>
                </div>
                <div class="modal-body">
                    <div class="form-group">
                        <label>
                            Role
                            <a href="Create_Payment_Roles.aspx">Add New Role</a>
                            <asp:RequiredFieldValidator ID="RequiredFieldValidator2" runat="server" ControlToValidate="PayRoleDropDownList" CssClass="EroorSummer" ErrorMessage="Required" ValidationGroup="OP" InitialValue="0"></asp:RequiredFieldValidator>
                        </label>
                        <asp:DropDownList ID="PayRoleDropDownList" runat="server" AppendDataBoundItems="True" CssClass="form-control" DataSourceID="PayRoleSQL" DataTextField="Role" DataValueField="RoleID">
                            <asp:ListItem Value="0">[ SELECT ]</asp:ListItem>
                        </asp:DropDownList>
                        <asp:SqlDataSource ID="PayRoleSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT [RoleID], [Role] FROM [Income_Roles] WHERE ([SchoolID] = @SchoolID)">
                            <SelectParameters>
                                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" Type="Int32" />
                            </SelectParameters>
                        </asp:SqlDataSource>
                    </div>
                    <div class="form-group">
                        <label>Pay For<asp:RequiredFieldValidator ID="RequiredFieldValidator3" runat="server" ControlToValidate="OPayforTextBox" CssClass="EroorSummer" ErrorMessage="Required" ValidationGroup="OP"></asp:RequiredFieldValidator></label>
                        <asp:TextBox ID="OPayforTextBox" runat="server" CssClass="form-control" placeholder="Input Pay For"></asp:TextBox>
                    </div>
                    <div class="form-group">
                        <label>Amount<asp:RequiredFieldValidator ID="RequiredFieldValidator4" runat="server" ControlToValidate="OAmountTextBox" CssClass="EroorSummer" ErrorMessage="Required" ValidationGroup="OP"></asp:RequiredFieldValidator></label>
                        <asp:TextBox ID="OAmountTextBox" runat="server" CssClass="form-control" type="number" step="0.01" min="0" placeholder="Input amount"></asp:TextBox>
                    </div>
                    <div class="form-group">
                        <label>Concession</label>
                        <asp:TextBox ID="OConcessiontBox" runat="server" CssClass="form-control" type="number" step="0.01" min="0" autocomplete="off" placeholder="Input Concession"></asp:TextBox>
                    </div>
                    <div class="form-group">
                        <asp:Button ID="OthersPaymentButton" runat="server" CssClass="btn btn-primary" Text="Add Payment" ValidationGroup="OP" OnClick="OthersPaymentButton_Click" />
                        <asp:SqlDataSource ID="OthersPaymentSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" InsertCommand="INSERT INTO Income_PayOrder(SchoolID, RegistrationID, StudentID, ClassID, StudentClassID, Amount, Discount, LateFee, RoleID, PayFor, StartDate, EndDate, CreatedDate, EducationYearID) VALUES (@SchoolID, @RegistrationID, @StudentID, @ClassID, @StudentClassID, @Amount, @Discount, @LateFee, @RoleID, @PayFor, GETDATE(), GETDATE(), GETDATE(), @EducationYearID)" SelectCommand="SELECT * FROM [Income_PayOrder]">
                            <InsertParameters>
                                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" Type="Int32" />
                                <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" Type="Int32" />
                                <asp:Parameter Name="EducationYearID" Type="Int32" />
                                <asp:Parameter Name="StudentID" Type="Int32" />
                                <asp:Parameter Name="ClassID" Type="Int32" />
                                <asp:Parameter Name="StudentClassID" Type="Int32" />
                                <asp:Parameter Name="LateFee" />
                                <asp:ControlParameter ControlID="OAmountTextBox" Name="Amount" PropertyName="Text" Type="Double" />
                                <asp:ControlParameter ControlID="OConcessiontBox" Name="Discount" PropertyName="Text" Type="Double" />
                                <asp:ControlParameter ControlID="PayRoleDropDownList" Name="RoleID" PropertyName="SelectedValue" Type="Int32" />
                                <asp:ControlParameter ControlID="OPayforTextBox" Name="PayFor" PropertyName="Text" Type="String" />
                            </InsertParameters>
                        </asp:SqlDataSource>
                    </div>
                </div>
            </div>
        </div>
    </div>


    <!--submit button-->
    <div id="payment-submit" class="mt-4">
        <h4 id="total-pay-amount"></h4>

        <div class="form-inline">
            <div class="form-group">
                <asp:DropDownList ID="AccountDropDownList" runat="server" CssClass="form-control" DataSourceID="AccountSQL" DataTextField="AccountName" DataValueField="AccountID">
                </asp:DropDownList>
                <asp:SqlDataSource ID="AccountSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT [AccountID], [AccountName] FROM [Account] WHERE ([SchoolID] = @SchoolID)">
                    <SelectParameters>
                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" Type="Int32" />
                    </SelectParameters>
                </asp:SqlDataSource>
            </div>

            <div class="form-group">
                <button type="button" data-toggle="modal" data-target="#Others_Modal" class="btn btn-outline-success btn-md">Add More Payment</button>
            </div>

            <div class="form-group d-print-none">

                <asp:RadioButton ID="rbActive" runat="server" Text="SMS Active" GroupName="Status" Value="1" />
                <asp:RadioButton ID="rbInactive" runat="server" Text="SMS Inactive" GroupName="Status" Value="0" />

                <asp:CheckBox ID="RoleCheckBox" runat="server" Text="Send Payment Roles" />


                <asp:RequiredFieldValidator ID="RequiredFieldValidator6" runat="server" ControlToValidate="AccountDropDownList" CssClass="EroorStar" ErrorMessage="*" ValidationGroup="PaY"></asp:RequiredFieldValidator>
                <asp:Button ID="PayButton" runat="server" Text="Pay" OnClick="PayButton_Click" OnClientClick="return validateForm()" ValidationGroup="PaY" CssClass="btn btn-primary" />
                <asp:Button ID="UpdateConcessionButton" runat="server" Text="Update Concession" OnClick="UpdateConcessionButton_Click" OnClientClick="return validateConcession()" CssClass="btn btn-primary" />
            </div>




            <asp:Label ID="ErrorLabel" runat="server" CssClass="EroorSummer"></asp:Label>
        </div>

       
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

        <!--SMS Enable -->

        <asp:SqlDataSource ID="SmsSettingSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EduConnectionString %>" SelectCommand="SELECT TOP 1 PAY_Buttton_SMS_Enable_Disable FROM Account Where SchoolID=@SchoolID " UpdateCommand="UPDATE Account SET PAY_Buttton_SMS_Enable_Disable = @PAY_Buttton_SMS_Enable_Disable Where SchoolID=@SchoolID">
            <SelectParameters>
                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" Type="Int32" />
            </SelectParameters>
            <UpdateParameters>
                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" Type="Int32" />
            </UpdateParameters>
        </asp:SqlDataSource>
    </div>
    </div>




    <!--bottom sticky total amount-->
    <div id="grand-total-fixed"></div>


    <!-- Modal -->
    <div id="myModal" class="modal fade" role="dialog" tabindex="-1" style="width: 100%">
        <div class="modal-dialog modal-lg">
            <div class="modal-content">
                <div class="modal-header">
                    <h4 class="modal-title">Student's Paid Records</h4>
                    <button type="button" class="close" data-dismiss="modal">&times;</button>
                </div>
                <div class="modal-body" id="modalDiv">
                    <asp:UpdatePanel ID="upnlUsers" runat="server">
                        <ContentTemplate>
                            <asp:Panel ID="ExportPanel" CssClass="AllDueP" runat="server">
                                <div class="Print_ins_Name" style="text-align: center; font-size: 20px; font-weight: bold; padding-bottom: 5px">
                                    <label id="Print_InsName"></label>

                                </div>
                                <div style="text-align: center">
                                    <asp:GridView ID="GridView1" runat="server" AutoGenerateColumns="False" HorizontalAlign="Center" CssClass="modalPopup" DataSourceID="AllPaidRecordSQL">
                                        <Columns>
                                            <asp:TemplateField HeaderText="Receipt Number">
                                                <ItemTemplate>
                                                    <asp:LinkButton ID="MSNLinkButton" runat="server" CommandArgument='<%# Eval("MoneyReceiptID") %>' Text='<%# Eval("MoneyReceipt_SN") %>' ToolTip="Click To Details" OnCommand="MSNLinkButton_Command" />

                                                </ItemTemplate>

                                                <HeaderStyle HorizontalAlign="Center" />
                                                <ItemStyle HorizontalAlign="Center" />
                                            </asp:TemplateField>
                                            <asp:TemplateField HeaderText="Paid Date & Time">
                                                <ItemTemplate>

                                                    <small class="d-block"><%# Eval("PaidDate", "{0:d-MMM-yy (hh:mm tt)}") %></small>
                                                </ItemTemplate>

                                                <HeaderStyle HorizontalAlign="Center" />
                                                <ItemStyle HorizontalAlign="Center" />
                                            </asp:TemplateField>


                                            <asp:TemplateField HeaderText="Paid Amount">
                                                <ItemTemplate>
                                                    <%# Eval("TotalAmount") %> Tk
                                              <small class="d-block">
                                                </ItemTemplate>
                                                <HeaderStyle HorizontalAlign="Center" />
                                                <ItemStyle HorizontalAlign="Center" />
                                            </asp:TemplateField>

                                            <asp:TemplateField HeaderText=" Re-Print">
                                                <ItemTemplate>

                                                    <asp:LinkButton ID="Print_LinkButton" runat="server" CommandArgument='<%# Eval("MoneyReceiptID") %>' ToolTip="Click To Print" OnCommand="Print_LinkButton_Command"><i class="fa fa-print"></i> Print</asp:LinkButton></small>
                                                </ItemTemplate>
                                                <HeaderStyle HorizontalAlign="Center" />
                                                <ItemStyle HorizontalAlign="Center" />
                                            </asp:TemplateField>
                                            <asp:TemplateField HeaderText=" Received By">
                                                <ItemTemplate>
                                                    <%# Eval("Name") %>
                                                </ItemTemplate>
                                                <HeaderStyle HorizontalAlign="Center" />
                                                <ItemStyle HorizontalAlign="Center" />
                                            </asp:TemplateField>
                                        </Columns>
                                    </asp:GridView>
                                </div>

                                <asp:SqlDataSource ID="AllPaidRecordSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                                    SelectCommand="SELECT Income_MoneyReceipt.MoneyReceipt_SN, Income_MoneyReceipt.TotalAmount, Income_MoneyReceipt.PaidDate, Income_MoneyReceipt.MoneyReceiptID,Admin.FirstName + ' ' + Admin.LastName AS Name FROM Income_MoneyReceipt INNER JOIN Student ON Income_MoneyReceipt.StudentID = Student.StudentID Inner join Admin ON Income_MoneyReceipt.RegistrationID=Admin.RegistrationID WHERE (Income_MoneyReceipt.EducationYearID = @EducationYearID) AND (Student.ID = @ID) AND (Income_MoneyReceipt.SchoolID = @SchoolID)
                                    ORDER BY Income_MoneyReceipt.PaidDate DESC">
                                    <SelectParameters>
                                        <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                                        <asp:ControlParameter ControlID="SearchIDTextBox" Name="ID" PropertyName="Text" />
                                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                                    </SelectParameters>
                                </asp:SqlDataSource>
                            </asp:Panel>
                        </ContentTemplate>

                    </asp:UpdatePanel>

                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-primary print" onclick="PrintContent();">Print</button>
                    <button type="button" class="btn btn-danger" data-dismiss="modal">Close</button>
                </div>
            </div>
        </div>
    </div>

    <script>
        const inputFindId = document.getElementById("<%=SearchIDTextBox.ClientID%>");
        const searchButton = document.getElementById("<%=SearchButton.ClientID%>");

        //find student ids
        $(`#${inputFindId.id}`).typeahead({
            source: function (request, result) {
                $.ajax({
                    url: "/Handeler/Student_IDs.asmx/GetStudentID",
                    data: JSON.stringify({ 'ids': request }),
                    dataType: "json",
                    type: "POST",
                    contentType: "application/json; charset=utf-8",
                    success: function (response) { result(JSON.parse(response.d)); },
                    error: function (err) { console.log(err) }
                });
            },
        });

        function PrintContent() {
            $(".Print_ins_Name").show();
            $("#Print_InsName").text($("#InstitutionName").text());

            var DocumentContainer = document.getElementById('modalDiv');
            var WindowObject = window.open("", "PrintWindow",
                "width=800,height=650,top=50, left=50,toolbars=no,scrollbars=yes,status=no,resizable=yes");
            WindowObject.document.write();
            WindowObject.document.write('<link rel="stylesheet" type="text/css" href="path-to-my-stylesheet.css">')
            WindowObject.document.writeln(DocumentContainer.innerHTML);
            WindowObject.document.close();
            WindowObject.focus();
            WindowObject.print();
            WindowObject.close();
        }



        function Modal_Info_Prnt() {
            /*$(".Print_ins_Name").show();*/
            /*$("#Print_InsName").text($("#InstitutionName").text());*/


            $('#modalDiv').css({ 'height': 'auto', 'overflow': 'auto' }).removeClass('modal-body');
            /*$('#myModal').modal('hide');*/

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
        //student id press enter to submit
        inputFindId.addEventListener("keyup", function (event) {
            if (event.keyCode === 13) {
                searchButton.click();
                return false;
            }
        });

        //show payment submit area if dues
        const currentSessionDue = document.getElementById("<%=DueGridView.ClientID%>");
        const othersSessionDue = document.getElementById("<%=OtherSessionGridView.ClientID%>");
        const paymentSubmit = document.getElementById("payment-submit");

        if (currentSessionDue && currentSessionDue.rows.length || othersSessionDue && othersSessionDue.rows.length) {
            paymentSubmit.style.display = "block";
        }

        //Uncheck due select checkbox if page reload
        const checkboxes = document.querySelectorAll("input[type='checkbox']");
        for (const checkbox of checkboxes) {
            checkbox.checked = false;
        }

        //due checkbox and input due amount
        (function () {
            //calculate total dues
            function calculateTotal() {
                const inputedDues = document.querySelectorAll(".due-input:not([disabled])");
                let total = 0;
                inputedDues.forEach((item => {
                    total += Number(item.value);
                }));

                return total;
            }

            //click checkbox and input dues
            const totalPayAmount = document.getElementById("total-pay-amount");
            const totalPayAmountFixed = document.getElementById("grand-total-fixed");
            const paymentTable = document.getElementById("payment-container");

            paymentTable.addEventListener("input", function (evt) {
                const element = evt.target;

                if (element.type === "checkbox") {
                    element.closest("tr").classList.toggle("row-selected");

                    const input = element.closest("tr").querySelector('.due-input');
                const consinput = element.closest("tr").querySelector('.concession-input');
                input.disabled = !element.checked;
     consinput.disabled = !element.checked;
      }

     // Real-time validation for concession input
  if (element.classList.contains('concession-input')) {
           const row = element.closest("tr");
  const feeCell = row.cells[6]; // Fee column
        const paidCell = row.cells[9]; // Paid column
              const dueCell = row.cells[10]; // Due column
       
         const fee = parseFloat(feeCell.innerText) || 0;
        const paid = parseFloat(paidCell.innerText) || 0;
             const due = parseFloat(dueCell.innerText) || 0;
        const concession = parseFloat(element.value) || 0;
       
   if (concession > due) {
            element.style.borderColor = 'red';
         element.style.backgroundColor = '#fff5f5';
                 element.title = 'কনসেশন এমাউন্ট ডিয়ু এমাউন্টের চেয়ে বেশি হতে পারবে না!';
                    } else {
    element.style.borderColor = '';
    element.style.backgroundColor = '';
     element.title = '';
   }
   }

         const total = `Total Amount: <span id="total-amount-pay">${calculateTotal()}</span> Tk`;

     totalPayAmount.innerHTML = total;
       totalPayAmountFixed.innerHTML = total;
   });
     })();

        //paid-record-modal
        function openModal() {
            $('#paid-record-modal').modal('show');

            //calculate total rows paid amount
            const paids = document.querySelectorAll(".paid-record-paid-amount");
            const paidGrandTotal = document.getElementById("paid-record-grand-total");

            let total = 0;
            paids.forEach((item => {
                total += Number(item.textContent);
            }));

            if (paidGrandTotal)
                paidGrandTotal.textContent = `Total: ${total} tk`;
        }


        //validate form before submit pay
        function validateForm() {
            const isChecked = [...checkboxes].some(item => item.checked);

            if (isChecked) {
      return true;
            }

  $("#payment-submit").notify("Select payment to pay!", { position: "top left" });
            return false;
        }

     //validate concession before update
   function validateConcession() {
     const paymentTable = document.getElementById("payment-container");
   const concessionInputs = paymentTable.querySelectorAll('.concession-input:not([disabled])');
     
     for (let concessionInput of concessionInputs) {
    const row = concessionInput.closest("tr");
   const feeCell = row.cells[6]; // Fee column
        const paidCell = row.cells[9]; // Paid column
          const dueCell = row.cells[10]; // Due column
         
    const fee = parseFloat(feeCell.innerText) || 0;
    const paid = parseFloat(paidCell.innerText) || 0;
      const due = parseFloat(dueCell.innerText) || 0;
        const concession = parseFloat(concessionInput.value) || 0;
 
             // Check if concession is greater than due amount
            if (concession > due) {
          alert('কনসেশন এমাউন্ট ডিয়ু এমাউন্টের চেয়ে বেশি হতে পারবে না!\nConcession amount cannot be greater than due amount!');
           concessionInput.focus();
           return false;
         }
        }
    return true;
  }

        //show sticky bottom grand total
   $(window).scroll(function () {
  const totalAmountElement = document.getElementById("total-amount-pay");
if (!totalAmountElement) return;
    
    const totalPayAmount = parseFloat(totalAmountElement.textContent) || 0;
    if (totalPayAmount === 0) {
    $('#grand-total-fixed').fadeOut();
             return;
     }

 if ($(window).scrollTop() + $(window).height() > $(document).height() - 300) {
          $('#grand-total-fixed').fadeOut();
   }
            else {
       $('#grand-total-fixed').fadeIn();
            }
        });

     $(document).ready(function () {
            function openModal() {
         $('#myModal').modal('show');
     }
        });
    </script>
</asp:Content>
