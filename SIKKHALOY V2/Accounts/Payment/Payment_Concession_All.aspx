<%@ Page Title="Concession" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Payment_Concession_All.aspx.cs" Inherits="EDUCATION.COM.Accounts.Payment.Payment_Concession_All" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="CSS/Payment_Collection.css?v=3" rel="stylesheet" />
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <h3>Concession</h3>

    <div class="form-inline">
        <div class="form-group">
            <asp:TextBox ID="SearchIDTextBox" autocomplete="off" runat="server" CssClass="form-control" placeholder="Enter ID"></asp:TextBox>
        </div>
        <div class="form-group">
            <asp:Button ID="SearchButton" runat="server" CssClass="btn btn-primary" Text="Find" ValidationGroup="A" />
            <asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" ControlToValidate="SearchIDTextBox" CssClass="EroorStar" ErrorMessage="Enter Student ID" ValidationGroup="A"></asp:RequiredFieldValidator>
        </div>
    </div>

    <div class="row">
        <div class="col-lg-8">
            <asp:FormView ID="StudentInfoFormView" runat="server" DataKeyNames="StudentID,StudentClassID,ClassID,EducationYearID,ID" DataSourceID="StudentInfoSQL" Width="100%">
                <ItemTemplate>
                    <div class="z-depth-1 p-3 mb-4">
                        <div class="d-flex flex-sm-row flex-column text-center text-sm-left">
                            <div class="p-image">
                                <img alt="No Image" src="/Handeler/Student_Photo.ashx?SID=<%#Eval("StudentImageID") %>" class="img-thumbnail rounded-circle img-fluid z-depth-1" />
                            </div>
                            <div class="info">
                                <ul>
                                    <li>
                                        <strong>(<span id="IDLabel"><%# Eval("ID") %></span>) <%# Eval("StudentsName") %></strong>
                                    </li>
                                    <li>
                                        <b>Class:</b>
                                        <%# Eval("Class") %>
                                    </li>
                                     <li>
                                       Roll No:<%# Eval("RollNo") %>
                                        <%#Eval("Section",", Section: {0}") %>
                                        <%# Eval("Shift",", Shift: {0}") %>
                                    </li>
                                    <li><b>Phone: </b><%# Eval("SMSPhoneNo") %></li>
                                    <li><b>Session: </b><%# Eval("EducationYear") %> <i class="fa fa-hand-o-right"></i><a target="_blank" href="/Admission/Student_Report/Report.aspx?Student=<%# Eval("StudentID") %>&Student_Class=<%# Eval("StudentClassID") %>">Full Details</a>
                                    </li>
                                </ul>
                            </div>
                        </div>
                    </div>
                </ItemTemplate>
            </asp:FormView>
            <asp:SqlDataSource ID="StudentInfoSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                SelectCommand="SELECT Student.StudentID, StudentsClass.StudentClassID, StudentsClass.ClassID, Student.StudentImageID, Student.ID, Student.StudentsName, Student.SMSPhoneNo, CreateClass.Class,CreateSection.Section,CreateSubjectGroup.SubjectGroup,CreateShift.Shift, StudentsClass.RollNo, Student.FathersName, Education_Year.EducationYearID, Education_Year.EducationYear FROM StudentsClass INNER JOIN Student ON StudentsClass.StudentID = Student.StudentID INNER JOIN Education_Year ON StudentsClass.EducationYearID = Education_Year.EducationYearID LEFT OUTER JOIN CreateShift ON StudentsClass.ShiftID = CreateShift.ShiftID LEFT OUTER JOIN CreateSubjectGroup ON StudentsClass.SubjectGroupID = CreateSubjectGroup.SubjectGroupID LEFT OUTER JOIN CreateSection ON StudentsClass.SectionID = CreateSection.SectionID LEFT OUTER JOIN CreateClass ON StudentsClass.ClassID = CreateClass.ClassID WHERE (Student.ID = @ID) AND (StudentsClass.SchoolID = @SchoolID) AND (StudentsClass.Class_Status IS NULL)">
                <SelectParameters>
                    <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                    <asp:ControlParameter ControlID="SearchIDTextBox" Name="ID" PropertyName="Text" />
                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                </SelectParameters>
            </asp:SqlDataSource>
        </div>

        <div class="col-lg-4">
            <asp:UpdatePanel ID="UpdatePanel2" runat="server">
                <ContentTemplate>
                    <div class="mb-4">
                        <asp:GridView ID="PaidRecordGridView" runat="server" AutoGenerateColumns="False" CssClass="mGrid P-R" DataSourceID="PRecordSQL" AllowPaging="True" PageSize="3">
                            <Columns>
                                <asp:TemplateField HeaderText="Receipt">
                                    <ItemTemplate>
                                        <asp:LinkButton ID="MSNLinkButton" runat="server" CommandArgument='<%# Eval("MoneyReceiptID") %>' Text='<%# Eval("MoneyReceipt_SN") %>' ToolTip="Click To Details" OnCommand="MSNLinkButton_Command" />
                                        <small class="d-block"><%# Eval("PaidDate", "{0:d-MMM-yy (hh:mm tt)}") %></small>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Paid">
                                    <ItemTemplate>
                                        <%# Eval("TotalAmount") %> Tk
                                    </ItemTemplate>
                                </asp:TemplateField>
                            </Columns>
                            <PagerStyle CssClass="pgr" />
                        </asp:GridView>
                        <asp:SqlDataSource ID="PRecordSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                            SelectCommand="SELECT Income_MoneyReceipt.MoneyReceipt_SN, Income_MoneyReceipt.TotalAmount, Income_MoneyReceipt.PaidDate, Income_MoneyReceipt.MoneyReceiptID FROM Income_MoneyReceipt INNER JOIN Student ON Income_MoneyReceipt.StudentID = Student.StudentID WHERE (Income_MoneyReceipt.EducationYearID = @EducationYearID) AND (Student.ID = @ID) AND (Income_MoneyReceipt.SchoolID = @SchoolID)">
                            <SelectParameters>
                                <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                                <asp:ControlParameter ControlID="SearchIDTextBox" Name="ID" PropertyName="Text" />
                                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                            </SelectParameters>
                        </asp:SqlDataSource>
                    </div>
                    <!--Paid Record Modal -->
                    <div class="modal fade" id="myModal" tabindex="-1" role="dialog" aria-labelledby="myModalLabel" aria-hidden="true">
                        <div class="modal-dialog modal-lg" role="document">
                            <div class="modal-content">
                                <div class="modal-header">
                                    <div class="modal-title">Paid Record Details</div>
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
                                                    <asp:Label ID="PaidAmountLabel" runat="server" Text='<%# Eval("PaidAmount") %>'></asp:Label>
                                                </ItemTemplate>
                                                <FooterTemplate>
                                                    <label id="PGTLabel"></label>
                                                </FooterTemplate>
                                            </asp:TemplateField>
                                        </Columns>
                                        <FooterStyle CssClass="GridFooter" />
                                        <RowStyle CssClass="Rows"></RowStyle>
                                    </asp:GridView>
                                    <asp:SqlDataSource ID="PaidRecordsSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT Income_PaymentRecord.PaidAmount, Income_PaymentRecord.PayFor, Income_PaymentRecord.PaidDate, Income_Roles.Role FROM Income_PaymentRecord INNER JOIN Income_Roles ON Income_PaymentRecord.RoleID = Income_Roles.RoleID INNER JOIN Income_MoneyReceipt ON Income_PaymentRecord.MoneyReceiptID = Income_MoneyReceipt.MoneyReceiptID WHERE (Income_PaymentRecord.SchoolID = @SchoolID) AND (Income_MoneyReceipt.MoneyReceiptID = @MoneyReceiptID)">
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
        </div>
    </div>

    <asp:FormView ID="PDue" runat="server" DataSourceID="TotalDue_ByID_ODS" Width="100%">
        <ItemTemplate>
            <div class="Total_p-due">
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


    <div class="table-responsive mb-3">
        <asp:GridView ID="DueGridView" runat="server" AutoGenerateColumns="False" DataSourceID="DueSQL"
            DataKeyNames="PayOrderID,Amount,StudentID,StudentClassID,EducationYearID,RoleID,PayFor"
            CssClass="mGrid paid-gv" OnRowDataBound="DueGridView_RowDataBound">
            <Columns>
                <asp:BoundField DataField="Role" HeaderText="Role" SortExpression="Role" ReadOnly="True" />
                <asp:BoundField DataField="PayFor" HeaderText="For" SortExpression="PayFor" ReadOnly="True" />
                <asp:BoundField DataField="Amount" HeaderText="Fee" SortExpression="Amount" ReadOnly="True" />
                <asp:BoundField DataField="PaidAmount" HeaderText="Paid" ReadOnly="True" SortExpression="PaidAmount" />
                <asp:TemplateField HeaderText="Due" SortExpression="Due">
                    <ItemTemplate>
                        <asp:Label ID="DueLabel" runat="server" Text='<%# Bind("Due") %>'></asp:Label>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:BoundField DataField="EndDate" HeaderText="End Date" SortExpression="EndDate" DataFormatString="{0:d MMM yy}" ReadOnly="True" />
                <asp:TemplateField HeaderText="Fee Conc." SortExpression="Discount">
                    <HeaderTemplate>
                        <input id="conAllTextBox" type="text" class="form-control" placeholder="Fee Concession" onkeypress="return isNumberKey(event)" autocomplete="off" />
                    </HeaderTemplate>
                    <ItemTemplate>
                        <asp:TextBox ID="DiscountTextBox" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false" runat="server" CssClass="form-control FeeConc" placeholder="Concession" Text='<%# Bind("Discount") %>'></asp:TextBox>
                        <asp:Label ID="DiscountLabel" runat="server" Text='<%# Eval("Discount") %>' Visible="False"></asp:Label>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Late Fee">
                    <ItemTemplate>
                        <asp:TextBox ID="LateFeeTextBox" runat="server" Text='<%# Bind("LateFee") %>' onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false" CssClass="form-control" placeholder="Charge"></asp:TextBox>
                        <asp:Label ID="PrevLateFeeLabel" runat="server" Text='<%# Eval("LateFee") %>' Visible="False"></asp:Label>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="L.Fee Conc.">
                    <ItemTemplate>
                        <asp:TextBox ID="LateFeeDiscountTextBox" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false" runat="server" CssClass="form-control" placeholder="Concession" Text='<%# Bind("LateFee_Discount") %>'></asp:TextBox>
                        <asp:Label ID="LateFeeDiscountLable" runat="server" Text='<%# Eval("LateFee_Discount") %>' Visible="False"></asp:Label>
                    </ItemTemplate>
                </asp:TemplateField>
            </Columns>
        </asp:GridView>
        <asp:SqlDataSource ID="DueSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
            SelectCommand="SELECT Income_PayOrder.PayOrderID, Income_PayOrder.StudentID, Income_PayOrder.ClassID, Income_PayOrder.StudentClassID, Income_PayOrder.RegistrationID, Income_PayOrder.RoleID, Student.ID, Student.StudentsName, CreateClass.Class, Income_Roles.Role, Income_PayOrder.PayFor, Income_PayOrder.StartDate, Income_PayOrder.EndDate, Income_PayOrder.Amount, Income_PayOrder.Discount, Income_PayOrder.LateFee, Income_PayOrder.LateFee_Discount, Income_PayOrder.PaidAmount, CASE WHEN Income_PayOrder.EndDate &lt; GETDATE() - 1 THEN ISNULL(Income_PayOrder.Amount , 0) + ISNULL(Income_PayOrder.LateFee , 0) - ISNULL(Income_PayOrder.Discount , 0) - ISNULL(Income_PayOrder.PaidAmount , 0) - ISNULL(Income_PayOrder.LateFee_Discount , 0) ELSE ISNULL(Income_PayOrder.Amount , 0) - ISNULL(Income_PayOrder.Discount , 0) - ISNULL(Income_PayOrder.PaidAmount , 0) END AS Due, Income_PayOrder.Status, Income_PayOrder.CreatedDate, Income_PayOrder.EducationYearID, Income_PayOrder.LastPaidDate, Income_PayOrder.NumberOfPayment, Income_PayOrder.SchoolID FROM Income_PayOrder INNER JOIN Income_Roles ON Income_PayOrder.RoleID = Income_Roles.RoleID INNER JOIN Student ON Income_PayOrder.StudentID = Student.StudentID INNER JOIN CreateClass ON Income_PayOrder.ClassID = CreateClass.ClassID WHERE (Income_PayOrder.Status = 'Due' or  Income_PayOrder.PaidAmount = 0) AND (Student.ID = @ID) AND (Income_PayOrder.SchoolID = @SchoolID) AND (Income_PayOrder.EducationYearID = @EducationYearID) ORDER BY Income_PayOrder.EndDate">
            <SelectParameters>
                <asp:ControlParameter ControlID="SearchIDTextBox" DefaultValue="" Name="ID" PropertyName="Text" />
                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
            </SelectParameters>
        </asp:SqlDataSource>

        <%if (OtherSessionGridView.Rows.Count > 0)
            {%>
        <div class="alert alert-info mt-3">OTHERS SESSION DUE</div>
        <asp:GridView ID="OtherSessionGridView" runat="server" AutoGenerateColumns="False" CssClass="mGrid paid-gv" DataKeyNames="PayOrderID,Amount,StudentID,StudentClassID,RoleID,PayFor,StartDate,EducationYearID" DataSourceID="OtherSessionSQL" OnRowDataBound="OtherSessionGridView_RowDataBound">
            <Columns>
                <asp:BoundField DataField="EducationYear" HeaderText="Session" SortExpression="EducationYear" />
                <asp:BoundField DataField="Class" HeaderText="Class" SortExpression="Class" />
                <asp:BoundField DataField="Role" HeaderText="Role" SortExpression="Role" ReadOnly="True" />
                <asp:BoundField DataField="PayFor" HeaderText="For" SortExpression="PayFor" ReadOnly="True" />
                <asp:BoundField DataField="Amount" HeaderText="Fee" SortExpression="Amount" ReadOnly="True" />
                <asp:BoundField DataField="PaidAmount" HeaderText="Paid" ReadOnly="True" SortExpression="PaidAmount" />
                <asp:TemplateField HeaderText="Due" SortExpression="Due">
                    <ItemTemplate>
                        <asp:Label ID="DueLabel" runat="server" Text='<%# Bind("Due") %>'></asp:Label>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:BoundField DataField="EndDate" HeaderText="End Date" SortExpression="EndDate" DataFormatString="{0:d MMM yy}" ReadOnly="True" />
                <asp:TemplateField HeaderText="Fee Conc." SortExpression="Discount">
                    <ItemTemplate>
                        <asp:TextBox ID="DiscountTextBox" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false" runat="server" CssClass="form-control OthersConc" placeholder="Concession" Text='<%# Bind("Discount") %>'></asp:TextBox>
                        <asp:Label ID="DiscountLabel" runat="server" Text='<%# Eval("Discount") %>' Visible="False"></asp:Label>
                    </ItemTemplate>
                    <HeaderTemplate>
                        <input id="Others_conAll" type="text" class="form-control" placeholder="Fee Concession" onkeypress="return isNumberKey(event)" autocomplete="off" />
                    </HeaderTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Late Fee">
                    <ItemTemplate>
                        <asp:TextBox ID="LateFeeTextBox" runat="server" Text='<%# Bind("LateFee") %>' onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false" CssClass="form-control" placeholder="Charge"></asp:TextBox>
                        <asp:Label ID="PrevLateFeeLabel" runat="server" Text='<%# Eval("LateFee") %>' Visible="False"></asp:Label>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="L.Fee Conc.">
                    <ItemTemplate>
                        <asp:TextBox ID="LateFeeDiscountTextBox" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false" runat="server" CssClass="form-control" placeholder="Concession" Text='<%# Bind("LateFee_Discount") %>'></asp:TextBox>
                        <asp:Label ID="LateFeeDiscountLable" runat="server" Text='<%# Eval("LateFee_Discount") %>' Visible="False"></asp:Label>
                    </ItemTemplate>
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
WHERE        (Income_PayOrder.Status = 'Due' or  Income_PayOrder.PaidAmount = 0) AND (Student.ID = @ID) AND (Income_PayOrder.SchoolID = @SchoolID) AND (Income_PayOrder.EducationYearID &lt;&gt; @EducationYearID)
ORDER BY Income_PayOrder.EndDate">
            <SelectParameters>
                <asp:ControlParameter ControlID="SearchIDTextBox" DefaultValue="" Name="ID" PropertyName="Text" />
                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
            </SelectParameters>
        </asp:SqlDataSource>
        <%} %>
    </div>

    <asp:SqlDataSource ID="Fee_DiscountSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
        InsertCommand="INSERT INTO Income_Discount_Record(SchoolID, RegistrationID, EducationYearID, StudentID, PayOrderID, Reason, PreviousAmount, PostAmount, Date, StudentClassID) VALUES (@SchoolID, @RegistrationID, @EducationYearID, @StudentID, @PayOrderID, @Reason, @PreviousAmount, @PostAmount, GETDATE(), @StudentClassID)"
        SelectCommand="SELECT * FROM [Income_Discount_Record]" UpdateCommand="UPDATE Income_PayOrder SET Discount = @Discount WHERE (PayOrderID = @PayOrderID)">
        <InsertParameters>
            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
            <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" />
            <asp:Parameter Name="EducationYearID" />
            <asp:Parameter Name="StudentID" Type="Int32" />
            <asp:Parameter Name="PayOrderID" Type="Int32" />
            <asp:Parameter Name="Reason" Type="String" />
            <asp:Parameter Name="PreviousAmount" Type="Double" />
            <asp:Parameter Name="PostAmount" Type="Double" />
            <asp:Parameter Name="StudentClassID" Type="Int32" />
        </InsertParameters>
        <UpdateParameters>
            <asp:Parameter Name="Discount" />
            <asp:Parameter Name="PayOrderID" />
        </UpdateParameters>
    </asp:SqlDataSource>
    <asp:SqlDataSource ID="LateFee_DiscountSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
        InsertCommand="INSERT INTO Income_LateFee_Discount_Record(SchoolID, RegistrationID, EducationYearID, StudentID, StudentClassID, PayOrderID, PreviousAmount, PostAmount, Date, Reason) VALUES (@SchoolID, @RegistrationID, @EducationYearID, @StudentID, @StudentClassID, @PayOrderID, @PreviousAmount, @PostAmount, GETDATE(), @Reason)"
        SelectCommand="SELECT * FROM [Income_LateFee_Discount_Record]" UpdateCommand="UPDATE Income_PayOrder SET LateFee_Discount = @LateFee_Discount WHERE (PayOrderID = @PayOrderID)">
        <InsertParameters>
            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
            <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" />
            <asp:Parameter Name="EducationYearID" />
            <asp:Parameter Name="StudentID" Type="Int32" />
            <asp:Parameter Name="PayOrderID" Type="Int32" />
            <asp:Parameter Name="PreviousAmount" Type="Double" />
            <asp:Parameter Name="PostAmount" Type="Double" />
            <asp:Parameter Name="StudentClassID" Type="Int32" />
            <asp:Parameter Name="Reason" />
        </InsertParameters>
        <UpdateParameters>
            <asp:Parameter Name="LateFee_Discount" />
            <asp:Parameter Name="PayOrderID" />
        </UpdateParameters>
    </asp:SqlDataSource>
    <asp:SqlDataSource ID="LateFeeChangeSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
        InsertCommand="INSERT INTO Income_LateFee_Change_Record(SchoolID, RegistrationID, EducationYearID, StudentClassID, StudentID, PayOrderID, PreviousAmount, PostAmount, Date) VALUES (@SchoolID, @RegistrationID, @EducationYearID, @StudentClassID, @StudentID, @PayOrderID, @PreviousAmount, @PostAmount, GETDATE())"
        SelectCommand="SELECT * FROM [Income_LateFee_Change_Record]" UpdateCommand="UPDATE Income_PayOrder SET LateFee = @LateFee WHERE (PayOrderID = @PayOrderID)">
        <InsertParameters>
            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
            <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" />
            <asp:Parameter Name="EducationYearID" />
            <asp:Parameter Name="StudentID" Type="Int32" />
            <asp:Parameter Name="PayOrderID" Type="Int32" />
            <asp:Parameter Name="PreviousAmount" Type="Double" />
            <asp:Parameter Name="PostAmount" Type="Double" />
            <asp:Parameter Name="StudentClassID" Type="Int32" />
        </InsertParameters>
        <UpdateParameters>
            <asp:Parameter Name="LateFee" />
            <asp:Parameter Name="PayOrderID" />
        </UpdateParameters>
    </asp:SqlDataSource>
    <asp:Button ID="SubmitButton" runat="server" OnClick="SubmitButton_Click" Text="Concession" CssClass="btn btn-primary" ValidationGroup="1" />


    <script type="text/javascript">
        $(function () {
            $('[id*=SearchIDTextBox]').typeahead({
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

            //Due GridView is empty
            if (!$('[id*=DueGridView] tr').length & !$('[id*=OtherSessionGridView] tr').length) {
                $("[id*=SubmitButton]").hide();
            }

            //Assign Amount to All
            $("#conAllTextBox").keyup(function () {
                $(".FeeConc").val($.trim($(this).val()));
            });
            //others session
            $("#Others_conAll").keyup(function () {
                $(".OthersConc").val($.trim($(this).val()));
            });
        });

        function openModal() {
            $('#myModal').modal('show');
        }
        function isNumberKey(a) { a = a.which ? a.which : event.keyCode; return 46 != a && 31 < a && (48 > a || 57 < a) ? !1 : !0 };
        function DisablePay() { document.getElementById("<%=SubmitButton.ClientID %>").disabled = true; }
        window.onbeforeunload = DisablePay;
    </script>
</asp:Content>
