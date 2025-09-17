<%@ Page Title="Accounts" Language="C#" MasterPageFile="~/Basic_Student.Master" AutoEventWireup="true" CodeBehind="Accounts.aspx.cs" Inherits="EDUCATION.COM.Student.Accounts" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <style>
        #grand-total-fixed {
            display: none;
            opacity: 0.95;
            position: fixed;
            width: 87%;
            background: #fff;
            bottom: 0;
            box-shadow: 0 0 16px -1px rgba(40, 40, 40, 0.75);
            margin-left: -15px;
            text-align: center;
            font-size: 1.5rem;
            font-weight: bold;
            padding: 1.5rem 0;
            z-index: 999;
        }

        @media (max-width: 767.98px) {
            #grand-total-fixed {
                width: 100%;
            }
        }
    </style>
    <h3>Accounts <span style="margin-left:12%">পেমেন্ট দেয়ার জন্য মাসের নামে টিক দিন ও নিচে সাবমিট পেমেন্ট ক্লিক করুন</span> </h3>
    <div class="tab">
        <ul class="nav nav-tabs z-depth-1">
            <li><a class="nav-link active" href="#DueAmount" data-toggle="tab" role="tab" aria-expanded="true">Total Due</a></li>
            <li><a class="nav-link" href="#PresentDue" data-toggle="tab" role="tab" aria-expanded="false">Current Due</a></li>
            <li><a class="nav-link" href="#PaidAmount" data-toggle="tab" role="tab" aria-expanded="false">Total Paid</a></li>
            <li><a class="nav-link" href="#PaidRecord" data-toggle="tab" role="tab" aria-expanded="false">Paid Record</a></li>
            <li><a class="nav-link" href="#Concession" data-toggle="tab" role="tab" aria-expanded="false">Concession</a></li>
            <li><a class="nav-link" href="#Payorder" data-toggle="tab" role="tab" aria-expanded="false">All Pay order</a></li>
        </ul>

        <div class="tab-content card">
            <div id="DueAmount" class="tab-pane fade in active show" role="tabpanel" aria-expanded="true">
                <div class="alert alert-success Accounts-p-title">
                    <span id="Total_Due"></span>
                </div>
                <div id="payment-container" class="table-responsive">
                    <asp:GridView ID="DueGridView" runat="server" AutoGenerateColumns="False" DataKeyNames="PayOrderID,Amount,StudentID,StudentClassID,RoleID,PayFor,StartDate,EducationYearID" DataSourceID="DueSQL" CssClass="mGrid">
                        <Columns>
                            <asp:TemplateField>
                                <ItemTemplate>
                                    <asp:CheckBox ID="DueCheckBox" CssClass="due-checkbox" runat="server" Text=" " />
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:BoundField DataField="Role" HeaderText="Role" SortExpression="Role" />
                            <asp:BoundField DataField="PayFor" HeaderText="Pay For" SortExpression="PayFor" />
                            <asp:TemplateField HeaderText="Fee" SortExpression="Amount">
                                <ItemTemplate>
                                    <asp:Label ID="TotalFeesLabel" runat="server" Text='<%# Bind("Amount") %>'></asp:Label>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:BoundField DataField="Discount" HeaderText="Concession" SortExpression="Discount" />
                            <asp:BoundField DataField="EducationYear" HeaderText="Session" SortExpression="EducationYear" />
                            <asp:BoundField DataField="Class" HeaderText="Class" SortExpression="Class" />
                            <asp:BoundField DataField="StartDate" HeaderText="Start Date" SortExpression="StartDate" DataFormatString="{0:dd-MMM-yy}" />
                            <asp:BoundField DataField="EndDate" HeaderText="End Date" SortExpression="EndDate" DataFormatString="{0:dd-MMM-yy}"></asp:BoundField>
                            <asp:BoundField DataField="LateFee" HeaderText="Late Fee" SortExpression="LateFee" />
                            <asp:BoundField DataField="LateFee_Discount" HeaderText="LF.Conc" SortExpression="LateFee_Discount" />
                            <asp:TemplateField HeaderText="Paid" SortExpression="PaidAmount">
                                <ItemTemplate>
                                    <asp:Label ID="Label3" runat="server" Text='<%# Bind("PaidAmount") %>'></asp:Label>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Due" SortExpression="Due">
                                <ItemTemplate>
                                    <asp:Label ID="TotalDueLabel" CssClass="due-amount" runat="server" Text='<%# Bind("Due") %>'></asp:Label>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:BoundField DataField="LastPaidDate" HeaderText="Last Paid Date" SortExpression="LastPaidDate" DataFormatString="{0:dd-MMM-yyyy}" />
                        </Columns>
                        <FooterStyle CssClass="GridFooter" />
                        <PagerStyle CssClass="pgr" />
                        <SelectedRowStyle CssClass="Selected" />
                        <EmptyDataTemplate>
                            No record(s) found !
                        </EmptyDataTemplate>
                    </asp:GridView>
                    <asp:SqlDataSource ID="DueSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                        SelectCommand="SELECT        Income_PayOrder.PayOrderID, Income_PayOrder.StudentID, Income_PayOrder.EducationYearID, Income_PayOrder.StudentClassID, Income_PayOrder.ClassID, CreateClass.Class, 
						 Education_Year.EducationYear, Income_Roles.Role, Income_PayOrder.PayFor, Income_PayOrder.EndDate, Income_PayOrder.Amount, Income_PayOrder.Discount, Income_PayOrder.LateFee, 
						 Income_PayOrder.LateFee_Discount, Income_PayOrder.PaidAmount, CASE WHEN Income_PayOrder.EndDate &lt; GETDATE() - 1 THEN ISNULL(Income_PayOrder.Amount, 0) + ISNULL(Income_PayOrder.LateFee, 0) 
						 - ISNULL(Income_PayOrder.Discount, 0) - ISNULL(Income_PayOrder.PaidAmount, 0) - ISNULL(Income_PayOrder.LateFee_Discount, 0) ELSE ISNULL(Income_PayOrder.Amount, 0) 
						 - ISNULL(Income_PayOrder.Discount, 0) - ISNULL(Income_PayOrder.PaidAmount, 0) END AS Due, Income_PayOrder.RoleID, Income_PayOrder.StartDate,Income_PayOrder.LastPaidDate, Income_PayOrder.NumberOfPayment
FROM            Income_PayOrder INNER JOIN
						 Income_Roles ON Income_PayOrder.RoleID = Income_Roles.RoleID INNER JOIN
						 Student ON Income_PayOrder.StudentID = Student.StudentID INNER JOIN
						 Education_Year ON Income_PayOrder.EducationYearID = Education_Year.EducationYearID INNER JOIN
						 CreateClass ON Income_PayOrder.ClassID = CreateClass.ClassID 
WHERE        (Income_PayOrder.Status = 'Due') AND (Income_PayOrder.StudentID = @StudentID) AND (Student.Status = N'Active') AND (Income_PayOrder.SchoolID = @SchoolID) 
ORDER BY Income_PayOrder.EndDate">
                        <SelectParameters>
                            <asp:SessionParameter DefaultValue="" Name="StudentID" SessionField="StudentID" />
                            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                        </SelectParameters>
                    </asp:SqlDataSource>
                </div>
            </div>

            <div id="PresentDue" class="tab-pane fade" role="tabpanel" aria-expanded="false">
                <div class="alert alert-success Accounts-p-title">
                    <span id="Current_Due"></span>
                </div>
                <div class="table-responsive">
                    <asp:GridView ID="PresentDueGridView" runat="server" AutoGenerateColumns="False" DataSourceID="PresentDueeSQL" CssClass="mGrid">
                        <Columns>
                            <asp:BoundField DataField="EducationYear" HeaderText="Session" SortExpression="EducationYear" />
                            <asp:BoundField DataField="Class" HeaderText="Class" SortExpression="Class" />
                            <asp:BoundField DataField="Role" HeaderText="Role" SortExpression="Role" />
                            <asp:BoundField DataField="PayFor" HeaderText="Pay For" SortExpression="PayFor" />
                            <asp:BoundField DataField="PaidAmount" HeaderText="Paid " SortExpression="PaidAmount" />
                            <asp:TemplateField HeaderText="Due" SortExpression="Due">
                                <ItemTemplate>
                                    <asp:Label ID="PresentDueLabel" runat="server" Text='<%# Bind("Due") %>'></asp:Label>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:BoundField DataField="EndDate" HeaderText="End Date" SortExpression="EndDate" DataFormatString="{0:dd MMM yyyy}" />
                        </Columns>
                    </asp:GridView>
                    <asp:SqlDataSource ID="PresentDueeSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT  CreateClass.Class,Education_Year.EducationYear,Income_Roles.Role, Income_PayOrder.PayFor, Income_PayOrder.PaidAmount, CASE WHEN Income_PayOrder.EndDate &lt; GETDATE() - 1 THEN ISNULL(Amount , 0) + ISNULL(LateFee , 0) - ISNULL(Discount , 0) - ISNULL(PaidAmount , 0) - ISNULL(LateFee_Discount , 0) ELSE ISNULL(Amount , 0) - ISNULL(Discount , 0) - ISNULL(PaidAmount , 0) END AS Due, Income_PayOrder.EndDate, Income_PayOrder.StudentClassID FROM Income_PayOrder INNER JOIN Education_Year ON Income_PayOrder.EducationYearID = Education_Year.EducationYearID INNER JOIN Income_Roles ON Income_PayOrder.RoleID = Income_Roles.RoleID INNER JOIN CreateClass ON Income_PayOrder.ClassID = CreateClass.ClassID  WHERE (Income_PayOrder.Status = 'Due') AND (Income_PayOrder.EndDate &lt; GETDATE()) AND (Income_PayOrder.StudentID = @StudentID) ORDER BY Income_PayOrder.EndDate">
                        <SelectParameters>
                            <asp:SessionParameter Name="StudentID" SessionField="StudentID" />
                        </SelectParameters>
                    </asp:SqlDataSource>
                </div>
            </div>

            <div id="PaidAmount" class="tab-pane fade" role="tabpanel" aria-expanded="false">
                <div class="alert alert-success Accounts-p-title">
                    <span class="Total_Paid"></span>
                </div>
                <div class="table-responsive">
                    <asp:GridView ID="PaidGridView" runat="server" AutoGenerateColumns="False" DataSourceID="PaidSQL" CssClass="mGrid">
                        <Columns>
                            <asp:BoundField DataField="Role" HeaderText="Role" SortExpression="Role" />
                            <asp:BoundField DataField="PayFor" HeaderText="Pay For" SortExpression="PayFor" />
                            <asp:BoundField DataField="StartDate" HeaderText="Start Date" SortExpression="StartDate" DataFormatString="{0:dd-MMM-yy}" />
                            <asp:BoundField DataField="EndDate" HeaderText="End Date" SortExpression="EndDate" DataFormatString="{0:dd-MMM-yy}" />
                            <asp:TemplateField HeaderText="Fee" SortExpression="Amount">
                                <ItemTemplate>
                                    <asp:Label ID="Label1" runat="server" Text='<%# Bind("Amount") %>'></asp:Label>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:BoundField DataField="Discount" HeaderText="Concession" SortExpression="Discount"></asp:BoundField>
                            <asp:TemplateField HeaderText="Paid" SortExpression="PaidAmount">
                                <ItemTemplate>
                                    <asp:Label ID="Total_Paid_Label" runat="server" Text='<%# Bind("PaidAmount") %>'></asp:Label>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Due" SortExpression="Due">
                                <ItemTemplate>
                                    <asp:Label ID="Label3" runat="server" Text='<%# Bind("Due") %>'></asp:Label>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:BoundField DataField="LateFee" HeaderText="Late Fee" SortExpression="LateFee" />
                            <asp:BoundField DataField="LateFee_Discount" HeaderText="LF.Conc" SortExpression="LateFee_Discount" />
                            <asp:BoundField DataField="LastPaidDate" HeaderText="Last Paid Date" SortExpression="LastPaidDate" DataFormatString="{0:dd-MMM-yyyy}" />
                        </Columns>
                        <EmptyDataTemplate>
                            No record(s) found !
                        </EmptyDataTemplate>
                    </asp:GridView>
                    <asp:SqlDataSource ID="PaidSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                        SelectCommand="SELECT Income_Roles.Role, Income_PayOrder.PayFor, Income_PayOrder.StartDate, Income_PayOrder.EndDate, Income_PayOrder.Amount, Income_PayOrder.Discount, Income_PayOrder.LateFee, Income_PayOrder.LateFee_Discount, Income_PayOrder.PaidAmount, CASE WHEN Income_PayOrder.EndDate &lt; GETDATE() - 1 THEN ISNULL(Income_PayOrder.Amount , 0) + ISNULL(Income_PayOrder.LateFee , 0) - ISNULL(Income_PayOrder.Discount , 0) - ISNULL(Income_PayOrder.PaidAmount , 0) - ISNULL(Income_PayOrder.LateFee_Discount , 0) ELSE ISNULL(Income_PayOrder.Amount , 0) - ISNULL(Income_PayOrder.Discount , 0) - ISNULL(Income_PayOrder.PaidAmount , 0) END AS Due, Income_PayOrder.LastPaidDate, Income_PayOrder.NumberOfPayment, Income_PayOrder.PayOrderID, Income_PayOrder.StudentClassID FROM Income_PayOrder INNER JOIN Income_Roles ON Income_PayOrder.RoleID = Income_Roles.RoleID WHERE (Income_PayOrder.Status = @Status) AND (Income_PayOrder.StudentID = @StudentID) AND (Income_PayOrder.EducationYearID = @EducationYearID) AND (Income_PayOrder.StudentClassID = @StudentClassID) AND (Income_PayOrder.PaidAmount &lt;&gt; 0) ORDER BY Income_PayOrder.LastPaidDate DESC">
                        <SelectParameters>
                            <asp:Parameter DefaultValue="Paid" Name="Status" />
                            <asp:SessionParameter DefaultValue="" Name="StudentID" SessionField="StudentID" />
                            <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                            <asp:SessionParameter DefaultValue="" Name="StudentClassID" SessionField="StudentClassID" />
                        </SelectParameters>
                    </asp:SqlDataSource>
                </div>
            </div>

            <div id="PaidRecord" class="tab-pane fade" role="tabpanel" aria-expanded="false">
                <div class="alert alert-success Accounts-p-title">
                    <span class="Total_Paid"></span>
                </div>
                <asp:UpdatePanel ID="UpdatePanel6" runat="server">
                    <ContentTemplate>
                        <div class="table-responsive">
                            <asp:GridView ID="PaidRecordGridView" runat="server" AutoGenerateColumns="False" DataSourceID="MoneyReceiptSQL" CssClass="mGrid">
                                <Columns>
                                    <asp:BoundField DataField="MoneyReceipt_SN" HeaderText="Receipt No" />
                                    <asp:BoundField DataField="PaidDate" HeaderText="Paid Date" SortExpression="PaidDate" DataFormatString="{0:dd-MMM-yy}" />
                                    <asp:BoundField DataField="TotalAmount" HeaderText="Paid" SortExpression="TotalAmount" />
                                    <asp:TemplateField>
                                        <ItemTemplate>
                                            <asp:LinkButton ID="MreceiptLinkButton" runat="server" CommandArgument='<%#Eval("MoneyReceiptID") %>' OnCommand="MreceiptLinkButton_Command" Text="Details" />
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                </Columns>
                                <EmptyDataTemplate>
                                    No record(s) found !
                                </EmptyDataTemplate>
                                <FooterStyle CssClass="GridFooter" />
                            </asp:GridView>
                            <asp:SqlDataSource ID="MoneyReceiptSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT PaidDate, TotalAmount, PaymentBy, MoneyReceipt_SN,MoneyReceiptID FROM Income_MoneyReceipt WHERE (StudentID = @StudentID) AND (EducationYearID = @EducationYearID) AND (StudentClassID = @StudentClassID)">
                                <SelectParameters>
                                    <asp:SessionParameter Name="StudentID" SessionField="StudentID" Type="Int32" />
                                    <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                                    <asp:SessionParameter Name="StudentClassID" SessionField="StudentClassID" />
                                </SelectParameters>
                            </asp:SqlDataSource>
                        </div>
                    </ContentTemplate>
                </asp:UpdatePanel>
            </div>

            <div id="Concession" class="tab-pane fade" role="tabpanel" aria-expanded="false">
                <div class="alert alert-success Accounts-p-title">
                    <span id="Total_Concession"></span>
                </div>
                <div class="table-responsive">
                    <asp:GridView ID="LessPayGridView" runat="server" AutoGenerateColumns="False" DataSourceID="DiscountSQL" CssClass="mGrid">
                        <Columns>
                            <asp:BoundField DataField="Role" HeaderText="Role" SortExpression="Role" />
                            <asp:BoundField DataField="PayFor" HeaderText="Pay For" SortExpression="PayFor" />
                            <asp:BoundField DataField="StartDate" HeaderText="Start Date" SortExpression="StartDate" DataFormatString="{0:dd-MMM-yy}" />
                            <asp:BoundField DataField="EndDate" HeaderText="End Date" SortExpression="EndDate" DataFormatString="{0:dd-MMM-yy}" />
                            <asp:BoundField DataField="Amount" HeaderText="Fee" SortExpression="Amount" />
                            <asp:BoundField DataField="LateFee" HeaderText="Late Fee" SortExpression="LateFee" />
                            <asp:TemplateField HeaderText="Concession" SortExpression="Total_Discount">
                                <ItemTemplate>
                                    <asp:Label ID="Concession_Label" runat="server" Text='<%# Bind("Total_Discount") %>'></asp:Label>
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                        <PagerStyle CssClass="pgr" />
                        <EmptyDataTemplate>
                            No record(s) found !
                        </EmptyDataTemplate>
                    </asp:GridView>
                    <asp:SqlDataSource ID="DiscountSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                        SelectCommand="SELECT Income_Roles.Role, Income_PayOrder.PayFor, Income_PayOrder.Amount, Income_PayOrder.LateFee, Income_PayOrder.Total_Discount, Income_PayOrder.StartDate, Income_PayOrder.EndDate FROM Income_PayOrder INNER JOIN Income_Roles ON Income_PayOrder.RoleID = Income_Roles.RoleID WHERE (Income_PayOrder.StudentID = @StudentID) AND (Income_PayOrder.EducationYearID = @EducationYearID) AND (Income_PayOrder.StudentClassID = @StudentClassID) AND (Income_PayOrder.Total_Discount &lt;&gt; 0) ORDER BY Income_PayOrder.StartDate ">
                        <SelectParameters>
                            <asp:SessionParameter Name="StudentID" SessionField="StudentID" Type="Int32" />
                            <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                            <asp:SessionParameter Name="StudentClassID" SessionField="StudentClassID" />
                        </SelectParameters>
                    </asp:SqlDataSource>

                    <%if (DisCountLateFeeGridView.Rows.Count > 0)
                        { %>
                    <br />
                    <div class="alert alert-info">Concession Late Fee Info: </div>
                    <asp:GridView ID="DisCountLateFeeGridView" runat="server" AutoGenerateColumns="False" DataSourceID="LateFeeSQL" CssClass="mGrid" DataKeyNames="LateFeeDiscountID">
                        <Columns>
                            <asp:BoundField DataField="PreviousAmount" HeaderText="Prev. Concession" SortExpression="PreviousAmount" />
                            <asp:BoundField DataField="PostAmount" HeaderText="Post Concession" SortExpression="PostAmount" />
                            <asp:BoundField DataField="Reason" HeaderText="Reason" SortExpression="Reason" />
                            <asp:BoundField DataField="Date" HeaderText="Date" SortExpression="Date" DataFormatString="{0:dd-MMM-yy}" />
                        </Columns>
                        <EmptyDataTemplate>
                            No record(s) found !
                        </EmptyDataTemplate>
                    </asp:GridView>
                    <asp:SqlDataSource ID="LateFeeSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT LateFeeDiscountID, SchoolID, EducationYearID, StudentID, StudentClassID, RegistrationID, PayOrderID, Reason, PreviousAmount, PostAmount, Date FROM Income_LateFee_Discount_Record WHERE (StudentID = @StudentID) AND (EducationYearID = @EducationYearID) AND (StudentClassID = @StudentClassID)">
                        <SelectParameters>
                            <asp:SessionParameter Name="StudentID" SessionField="StudentID" Type="Int32" />
                            <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                            <asp:SessionParameter Name="StudentClassID" SessionField="StudentClassID" />
                        </SelectParameters>
                    </asp:SqlDataSource>
                    <%} %>
                    <%if (ChargeGridView.Rows.Count > 0)
                        { %>
                    <br />
                    <div class="alert alert-info">Charge Late Fee Info </div>
                    <asp:GridView ID="ChargeGridView" runat="server" AutoGenerateColumns="False" DataSourceID="ChargeLateFSQL" AlternatingRowStyle-CssClass="alt" CssClass="mGrid" DataKeyNames="LateFeeChangeID">
                        <Columns>
                            <asp:BoundField DataField="PreviousAmount" HeaderText="Prev. Concession" SortExpression="PreviousAmount" />
                            <asp:BoundField DataField="PostAmount" HeaderText="Post Concession" SortExpression="PostAmount" />
                            <asp:BoundField DataField="Date" DataFormatString="{0:dd-MMM-yy}" HeaderText="Date" SortExpression="Date" />
                        </Columns>
                        <EmptyDataTemplate>
                            No record(s) found !
                        </EmptyDataTemplate>
                    </asp:GridView>
                    <asp:SqlDataSource ID="ChargeLateFSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT LateFeeChangeID, SchoolID, EducationYearID, StudentID, StudentClassID, RegistrationID, PayOrderID, PreviousAmount, PostAmount, Date FROM Income_LateFee_Change_Record WHERE (StudentID = @StudentID) AND (EducationYearID = @EducationYearID) AND (StudentClassID = @StudentClassID)">
                        <SelectParameters>
                            <asp:SessionParameter Name="StudentID" SessionField="StudentID" Type="Int32" />
                            <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                            <asp:SessionParameter Name="StudentClassID" SessionField="StudentClassID" />
                        </SelectParameters>
                    </asp:SqlDataSource>
                    <%} %>
                </div>
            </div>

            <div id="Payorder" class="tab-pane fade" role="tabpanel" aria-expanded="false">
                <asp:FormView ID="P_SumFormView" runat="server" DataSourceID="PayOrderSummarySQL" Width="100%">
                    <ItemTemplate>
                        <h4 class="Accounts-p-title mb-2">
                            <span class="badge badge-primary">Total Fee: <%# Eval("TotalFee","{0:n0}") %> Tk</span>
                            <span class="badge badge-secondary">Total Concession: <%# Eval("TotalDiscount","{0:n0}") %> Tk</span>
                            <span class="badge badge-warning">Total Late Fee: <%# Eval("TotalLateFee","{0:n0}") %> Tk</span>
                            <span class="badge badge-success">Total Paid: <%# Eval("TotalPaid","{0:n0}") %> Tk</span>
                            <span class="badge badge-danger">Total Due: <%# Eval("Unpaid","{0:n0}") %> Tk</span>
                        </h4>
                    </ItemTemplate>
                </asp:FormView>
                <asp:SqlDataSource ID="PayOrderSummarySQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT SUM(Amount) AS TotalFee, 
	 SUM(LateFeeCountable) AS TotalLateFee, 
	 SUM(Total_Discount) AS TotalDiscount, 
	 SUM(ISNULL(PaidAmount, 0)) AS TotalPaid,
	 SUM(Receivable_Amount) AS Unpaid
FROM Income_PayOrder WHERE (SchoolID = @SchoolID) AND (EducationYearID = @EducationYearID) AND StudentClassID = @StudentClassID ">
                    <SelectParameters>
                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                        <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                        <asp:SessionParameter Name="StudentClassID" SessionField="StudentClassID" />
                    </SelectParameters>
                </asp:SqlDataSource>

                <div class="table-responsive">
                    <asp:GridView ID="PayOrderGridView" AllowSorting="true" runat="server" AutoGenerateColumns="False" DataSourceID="PayOrderSQL" CssClass="mGrid" DataKeyNames="PayOrderID">
                        <Columns>
                            <asp:BoundField DataField="Role" HeaderText="Role" SortExpression="Role" />
                            <asp:BoundField DataField="PayFor" HeaderText="Pay For" SortExpression="PayFor" />
                            <asp:BoundField DataField="StartDate" DataFormatString="{0:d MMM yyyy}" HeaderText="Start Date" SortExpression="StartDate" />
                            <asp:BoundField DataField="EndDate" DataFormatString="{0:d MMM yyyy}" HeaderText="End Date" SortExpression="EndDate" />
                            <asp:BoundField DataField="Amount" HeaderText="Fee" SortExpression="Amount" />
                            <asp:BoundField DataField="Discount" HeaderText="Concession" SortExpression="Discount" />
                            <asp:BoundField DataField="LateFee" HeaderText="Late Fee" SortExpression="LateFee" />
                            <asp:BoundField DataField="LateFee_Discount" HeaderText="L.F. Con" SortExpression="LateFee_Discount" />
                            <asp:BoundField DataField="PaidAmount" HeaderText="Paid" SortExpression="PaidAmount" />
                            <asp:BoundField DataField="Due" HeaderText="Due" ReadOnly="True" SortExpression="Due" />
                            <asp:BoundField DataField="LastPaidDate" DataFormatString="{0:d MMM yyyy}" HeaderText="Last Paid Date" SortExpression="LastPaidDate" />
                        </Columns>
                        <EmptyDataTemplate>
                            No record(s) found !
                        </EmptyDataTemplate>
                        <FooterStyle CssClass="GridFooter" />
                    </asp:GridView>
                    <asp:SqlDataSource ID="PayOrderSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                        SelectCommand="SELECT Income_Roles.Role, Income_PayOrder.PayFor, Income_PayOrder.StartDate, Income_PayOrder.EndDate, Income_PayOrder.Amount, Income_PayOrder.Discount, Income_PayOrder.LateFee, Income_PayOrder.LateFee_Discount, Income_PayOrder.PaidAmount, CASE WHEN Income_PayOrder.EndDate &lt; GETDATE() - 1 THEN ISNULL(Income_PayOrder.Amount , 0) + ISNULL(Income_PayOrder.LateFee , 0) - ISNULL(Income_PayOrder.Discount , 0) - ISNULL(Income_PayOrder.PaidAmount , 0) - ISNULL(Income_PayOrder.LateFee_Discount , 0) ELSE ISNULL(Income_PayOrder.Amount , 0) - ISNULL(Income_PayOrder.Discount , 0) - ISNULL(Income_PayOrder.PaidAmount , 0) END AS Due, Income_PayOrder.LastPaidDate, Income_PayOrder.NumberOfPayment, Income_PayOrder.PayOrderID, Income_PayOrder.StudentClassID FROM Income_PayOrder INNER JOIN Income_Roles ON Income_PayOrder.RoleID = Income_Roles.RoleID WHERE   (Income_PayOrder.StudentID = @StudentID) AND (Income_PayOrder.EducationYearID = @EducationYearID) AND (Income_PayOrder.StudentClassID = @StudentClassID) ORDER BY Income_PayOrder.StartDate ">
                        <SelectParameters>
                            <asp:SessionParameter DefaultValue="" Name="StudentID" SessionField="StudentID" />
                            <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                            <asp:SessionParameter DefaultValue="" Name="StudentClassID" SessionField="StudentClassID" />
                        </SelectParameters>
                    </asp:SqlDataSource>
                </div>
            </div>
        </div>
    </div>

    <!--Money receipt-->
    <asp:UpdatePanel ID="UpdatePanel9" runat="server">
        <ContentTemplate>
            <div class="modal fade" id="myModal" tabindex="-1" role="dialog" aria-labelledby="myModalLabel" aria-hidden="true">
                <div class="modal-dialog cascading-modal" role="document">
                    <div class="modal-content">
                        <div class="modal-header">
                            <h4 class="title">Money Receipt Details</h4>
                            <button type="button" class="close" data-dismiss="modal" aria-label="Close"><span aria-hidden="true">&times;</span></button>
                        </div>
                        <div class="modal-body mb-0">
                            <asp:GridView ID="AllPaidRGridView" runat="server" AutoGenerateColumns="False" DataSourceID="AllPayRecordSQL" CssClass="mGrid">
                                <Columns>
                                    <asp:BoundField DataField="Role" HeaderText="Role" SortExpression="Role" />
                                    <asp:BoundField DataField="PayFor" HeaderText="Pay For" SortExpression="PayFor" />
                                    <asp:BoundField DataField="PaidAmount" HeaderText="Paid" SortExpression="PaidAmount" />
                                    <asp:BoundField DataField="PaidDate" HeaderText="Paid Date" SortExpression="PaidDate" DataFormatString="{0:dd-MMM-yyyy}" />
                                </Columns>
                            </asp:GridView>
                            <asp:SqlDataSource ID="AllPayRecordSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                                SelectCommand="SELECT Income_PaymentRecord.PayOrderID, Income_PaymentRecord.PaidAmount, Income_PaymentRecord.PayFor, Income_PaymentRecord.PaidDate, Income_Roles.Role FROM Income_PaymentRecord INNER JOIN Income_Roles ON Income_PaymentRecord.RoleID = Income_Roles.RoleID WHERE (Income_PaymentRecord.MoneyReceiptID = @MoneyReceiptID)">
                                <SelectParameters>
                                    <asp:Parameter Name="MoneyReceiptID" Type="Int32" />
                                </SelectParameters>
                            </asp:SqlDataSource>
                        </div>
                    </div>

                </div>
            </div>
        </ContentTemplate>
    </asp:UpdatePanel>

    <!--submit button-->
    <div id="payment-submit" class="mt-4">
        <h4 id="total-pay-amount"></h4>

        <div class="form-inline">
            <div class="form-group">
                <asp:Button ID="PayButton" runat="server" Text="Submit Payment" OnClick="PayButton_Click" CssClass="btn btn-primary" />
            </div>
        </div>
    </div>

    <!--bottom sticky total amount-->
    <div id="grand-total-fixed"></div>


    <script>
        $(function () {
            $("#_4").addClass("active");

            //Total Due
            var Total_Due = 0;
            $("[id*=TotalDueLabel]").each(function () {
                Total_Due = Total_Due + parseFloat($(this).html());
            });
            $("#Total_Due").html("Total Due: " + Total_Due.toString() + " Tk");

            //Current Due
            var Current_Due = 0;
            $("[id*=PresentDueLabel]").each(function () {
                Current_Due = Current_Due + parseFloat($(this).html());
            });
            $("#Current_Due").html("Total Current Due: " + Current_Due.toString() + " Tk");

            //Total_Paid
            var Total_Paid = 0;
            $("[id*=Total_Paid_Label]").each(function () {
                Total_Paid = Total_Paid + parseFloat($(this).html());
            });
            $(".Total_Paid").html("Total Paid: " + Total_Paid.toString() + " Tk");

            //Total_Concession
            var Total_Concession = 0;
            $("[id*=Concession_Label]").each(function () {
                Total_Concession = Total_Concession + parseFloat($(this).html());
            });

            $("#Total_Concession").html("Total Concession: " + Total_Concession.toString() + " Tk");
        });

        function openModal() {
            $('#myModal').modal('show');
        }


        (function () {

            document.getElementById('<%=PayButton.ClientID %>').disabled = true;

            function calculateTotal() {
                const inputedDues = document.querySelectorAll('input[type="checkbox"]:checked');
                let total = 0;
                inputedDues.forEach((item => {
                    total += Number(item.value);
                }));

                if (total <= 0) {
                    document.getElementById('<%=PayButton.ClientID %>').disabled = true;
                } else {
                    document.getElementById('<%=PayButton.ClientID %>').disabled = false;
                }

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
                    const input = element.closest("tr").querySelector('.due-amount');
                    element.value = input.textContent;
                }

                const total = `Total Amount: <span id="total-amount-pay">${calculateTotal()}</span> Tk`;
                totalPayAmount.innerHTML = total;
                totalPayAmountFixed.innerHTML = total;
            });
        })();

        //show sticky bottom grand total
        $(window).scroll(function () {
            const totalPayAmount = +document.getElementById("total-amount-pay").textContent;
            if (totalPayAmount === 0) {
                $('#grand-total-fixed').fadeOut();
                return;
            }

            if ($(window).scrollTop() + $(window).height() > $(document).height() - 300) {
                $('#grand-total-fixed').fadeOut();
            }
            else {
                // alert("totalPayAmount..." + totalPayAmount)
                $('#grand-total-fixed').fadeIn();
            }
        });

    </script>
</asp:Content>
