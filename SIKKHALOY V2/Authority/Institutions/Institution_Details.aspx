<%@ Page Title="Institution Details" Language="C#" MasterPageFile="~/Basic_Authority.Master" AutoEventWireup="true" CodeBehind="Institution_Details.aspx.cs" Inherits="EDUCATION.COM.Authority.Institutions.Institution_Details" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .Show, .IS { display: none; }
        .Info ul { margin: 10px 0; padding: 0; }
        .Info ul li { list-style: none; }
        .badge { padding: 5px 10px; border-radius: 3px; font-size: 12px; font-weight: bold; }
        .badge-success { background-color: #28a745; color: white; }
        .badge-warning { background-color: #ffc107; color: #000; }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <div class="container-fluid">
        <asp:FormView ID="SchoolFormView" runat="server" DataKeyNames="SchoolID,SchoolName" DataSourceID="School_SQL" Width="100%" CssClass="text-center">
            <ItemTemplate>
                <div class="Info">
                    <ul>
                        <li>
                            <h2 class="blue-text">
                                <%# Eval("SchoolName") %>
                            </h2>
                        </li>
                        <li>
                            <i class="fa fa-user" aria-hidden="true"></i>
                            <%#Eval("Principal") %></li>
                        <li><i class="fa fa-phone" aria-hidden="true"></i>
                            <%# Eval("Phone") %></li>
                        <li><i class="fa fa-envelope" aria-hidden="true"></i>
                            <%# Eval("Email") %></li>
                        <li><i class="fa fa-map-marker" aria-hidden="true"></i>
                            <%# Eval("Address") %></li>
                    </ul>
                </div>
            </ItemTemplate>
        </asp:FormView>
        <asp:SqlDataSource ID="School_SQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT * FROM [SchoolInfo] WHERE ([SchoolID] = @SchoolID)">
            <SelectParameters>
                <asp:QueryStringParameter Name="SchoolID" QueryStringField="SchoolID" Type="Int32" />
            </SelectParameters>
        </asp:SqlDataSource>

        <ul class="nav nav-tabs nav-justified z-depth-1">
            <li class="nav-item"><a class="nav-link active" data-toggle="tab" role="tab" href="#TotalStudent">Total Student</a></li>
            <li class="nav-item"><a class="nav-link" data-toggle="tab" role="tab" href="#SMS">SMS Recharge</a></li>
            <li class="nav-item"><a class="nav-link" data-toggle="tab" role="tab" href="#DueInvoiceSettings">Due Invoice Settings</a></li>
            <li class="nav-item"><a class="nav-link" data-toggle="tab" role="tab" href="#ManagemeUser">Manage User</a></li>
            <li class="nav-item"><a class="nav-link" data-toggle="tab" role="tab" href="#DeleteID">Student ID Delete</a></li>
            <li class="nav-item"><a class="nav-link" data-toggle="tab" role="tab" href="#IDChange">Student ID Change</a></li>
            <li class="nav-item"><a class="nav-link" data-toggle="tab" role="tab" href="#ReceiptDelete">Money Receipt delete</a></li>
        </ul>

        <div class="tab-content card">
            <div class="tab-pane fade in show active" role="tabpanel" id="TotalStudent">
                <div class="table-responsive mb-2">
                    <asp:GridView ID="Total_StudentGridView" runat="server" AutoGenerateColumns="False" DataSourceID="Total_StudentSQL" CssClass="mGrid" DataKeyNames="EducationYearID">
                        <Columns>
                            <asp:TemplateField HeaderText="Session SN.">
                                <ItemTemplate>
                                    <asp:TextBox ID="SessionSNTextBox" CssClass="form-control" Text='<%#Bind("SN") %>' runat="server"></asp:TextBox>
                                </ItemTemplate>
                                <ItemStyle Width="80px" />
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Payment Active">
                                <ItemTemplate>
                                    <asp:CheckBox ID="IsActiveCheckbox" Text=" " runat="server" Checked='<%#Bind("IsActive") %>' />
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:BoundField DataField="EducationYear" HeaderText="Education Year" SortExpression="EducationYear" />
                            <asp:BoundField DataField="TotalStudent" HeaderText="Total Student" ReadOnly="True" SortExpression="TotalStudent" />
                            <asp:TemplateField HeaderText="Login">
                                <ItemTemplate>
                                    <asp:Button ID="Login_Button" runat="server" Text="Login" CssClass="btn btn-green btn-sm" OnClick="Login_Button_Click" />
                                </ItemTemplate>
                                <ItemStyle Width="80px" />
                            </asp:TemplateField>
                        </Columns>
                    </asp:GridView>
                    <asp:SqlDataSource ID="Total_StudentSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT Education_Year.SN, Education_Year.IsActive, Education_Year.EducationYear, COUNT(*) AS TotalStudent, Education_Year.EducationYearID FROM Education_Year INNER JOIN StudentsClass ON Education_Year.EducationYearID = StudentsClass.EducationYearID INNER JOIN Student ON StudentsClass.StudentID = Student.StudentID WHERE (Student.Status = N'Active') AND (Education_Year.SchoolID = @SchoolID) GROUP BY Education_Year.SN,Education_Year.EducationYear, Education_Year.IsActive, Education_Year.EducationYearID ORDER BY Education_Year.EducationYearID" UpdateCommand="UPDATE Education_Year SET IsActive = @IsActive, SN = @SN WHERE (SchoolID = @SchoolID) AND (EducationYearID = @EducationYearID)">
                        <SelectParameters>
                            <asp:QueryStringParameter Name="SchoolID" QueryStringField="SchoolID" />
                        </SelectParameters>
                        <UpdateParameters>
                            <asp:Parameter Name="IsActive" />
                            <asp:QueryStringParameter Name="SchoolID" QueryStringField="SchoolID" />
                            <asp:Parameter Name="EducationYearID" />
                            <asp:Parameter Name="SN" />
                        </UpdateParameters>
                    </asp:SqlDataSource>
                </div>
                <asp:Button ID="SNButton" runat="server" Text="Update" CssClass="btn btn-green" OnClick="SNButton_Click" />
            </div>

            <div class="tab-pane fade" role="tabpanel" id="SMS">
                <div class="form-inline">
                    <div class="form-group">
                        <asp:TextBox ID="RechargeSMSTextBox" placeholder="SMS Quantity" runat="server" CssClass="form-control"></asp:TextBox>
                        <asp:RequiredFieldValidator ID="RequiredFieldValidator7" runat="server" ControlToValidate="RechargeSMSTextBox" CssClass="EroorSummer" ErrorMessage="*" ValidationGroup="SMS"></asp:RequiredFieldValidator>
                    </div>
                    <div class="form-group">
                        <asp:TextBox ID="PerSMS_PriceTextBox" placeholder="Per SMS Price" runat="server" CssClass="form-control"></asp:TextBox>
                        <asp:RequiredFieldValidator ID="RequiredFieldValidator8" runat="server" ControlToValidate="PerSMS_PriceTextBox" CssClass="EroorSummer" ErrorMessage="*" ValidationGroup="SMS"></asp:RequiredFieldValidator>
                    </div>
                    <div class="form-group">
                        <asp:Button ID="SMSRechargeButton" runat="server" CssClass="btn btn-info" Text="Recharge" OnClick="SMSRechargeButton_Click" ValidationGroup="SMS" />
                    </div>
                </div>

                <asp:FormView ID="SMSBalanceFormView" runat="server" DataKeyNames="SMSID" DataSourceID="SMSBalanceSQL" Width="100%">
                    <ItemTemplate>
                        <div class="alert alert-info">
                            Current Balance: <%# Eval("SMS_Balance") %>
                        </div>
                    </ItemTemplate>
                </asp:FormView>
                <asp:SqlDataSource ID="SMSBalanceSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT * FROM [SMS] WHERE ([SchoolID] = @SchoolID)">
                    <SelectParameters>
                        <asp:QueryStringParameter Name="SchoolID" QueryStringField="SchoolID" Type="Int32" />
                    </SelectParameters>
                </asp:SqlDataSource>

                <div class="table-responsive">
                    <asp:GridView ID="SMSGridView" runat="server" AutoGenerateColumns="False" DataKeyNames="SMS_Recharge_RecordID" DataSourceID="SMS_SQL" CssClass="mGrid" AllowPaging="True" AllowSorting="True">
                        <Columns>
                            <asp:BoundField DataField="RechargeSMS" HeaderText="Recharge SMS" SortExpression="RechargeSMS" />
                            <asp:BoundField DataField="PerSMS_Price" HeaderText="Per SMS Price" SortExpression="PerSMS_Price" />
                            <asp:BoundField DataField="Total_Price" HeaderText="Total" ReadOnly="True" SortExpression="Total_Price" />
                            <asp:BoundField DataField="Date" HeaderText="Date" SortExpression="Date" DataFormatString="{0:d MMM yyyy}" />
                            <asp:BoundField DataField="UserName" HeaderText="Recharged By" SortExpression="UserName" />
                            <asp:TemplateField HeaderText="Payment Status" SortExpression="Is_Paid">
                                <ItemTemplate>
                                    <%# 
                                        Eval("Is_Paid") != DBNull.Value && Convert.ToBoolean(Eval("Is_Paid")) 
                                        ? "<span class='badge badge-success'>Paid</span>" 
                                        : "<span class='badge badge-warning'>Unpaid</span>"
                                    %>
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                        <PagerStyle CssClass="pgr" />
                    </asp:GridView>
                    <asp:SqlDataSource ID="SMS_SQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT SMS_Recharge_Record.SMS_Recharge_RecordID, SMS_Recharge_Record.SchoolID, SMS_Recharge_Record.RechargeSMS, SMS_Recharge_Record.PerSMS_Price, SMS_Recharge_Record.Total_Price, SMS_Recharge_Record.Date, SMS_Recharge_Record.Is_Paid, Registration.UserName FROM SMS_Recharge_Record LEFT OUTER JOIN Registration ON SMS_Recharge_Record.RegistrationID = Registration.RegistrationID WHERE (SMS_Recharge_Record.SchoolID = @SchoolID) ORDER BY SMS_Recharge_Record.Date DESC" InsertCommand="INSERT INTO SMS_Recharge_Record(SchoolID, RechargeSMS, PerSMS_Price, Date, Is_Paid, RegistrationID) VALUES (@SchoolID, @RechargeSMS, @PerSMS_Price, GETDATE(), 0, @RegistrationID)">
                        <InsertParameters>
                            <asp:QueryStringParameter Name="SchoolID" QueryStringField="SchoolID" Type="Int32" />
                            <asp:ControlParameter ControlID="RechargeSMSTextBox" Name="RechargeSMS" PropertyName="Text" Type="Int32" />
                            <asp:ControlParameter ControlID="PerSMS_PriceTextBox" Name="PerSMS_Price" PropertyName="Text" Type="Double" />
                            <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" Type="Int32" />
                        </InsertParameters>
                        <SelectParameters>
                            <asp:QueryStringParameter Name="SchoolID" QueryStringField="SchoolID" />
                        </SelectParameters>
                    </asp:SqlDataSource>
                </div>
            </div>

            <div class="tab-pane fade" role="tabpanel" id="DueInvoiceSettings">
                <div class="card-body">
                    <h5 class="card-title mb-4">
                        <i class="fa fa-bell text-warning"></i> বকেয়া নোটিফিকেশন সেটিংস
                    </h5>
                    
                    <div class="alert alert-info">
                        <i class="fa fa-info-circle"></i> 
                        <strong>তথ্য:</strong> ডিফল্টভাবে সব প্রতিষ্ঠানের বকেয়া নোটিশ বন্ধ থাকে। শুধু যেসবে প্রতিষ্ঠানগুলোর জন্য নোটিশ চালু করবেন, শুধু সেগুলোর Dashboard এ বকেয়া নোটিশ দেখাবে。
                    </div>

                    <asp:UpdatePanel ID="DueNoticeUpdatePanel" runat="server">
                        <ContentTemplate>
                            <div class="form-group">
                                <div class="custom-control custom-checkbox">
                                    <asp:CheckBox ID="EnableDueNoticeCheckBox" runat="server" 
                                        CssClass="custom-control-input" 
                                        AutoPostBack="True" 
                                        OnCheckedChanged="EnableDueNoticeCheckBox_CheckedChanged" />
                                    <label class="custom-control-label" for="<%= EnableDueNoticeCheckBox.ClientID %>">
                                        <strong>বকেয়া নোটিশ চালু করুন</strong>
                                    </label>
                                </div>
                                <small class="form-text text-muted">
                                    চেক করলে এই প্রতিষ্ঠানের Dashboard এ বকেয়া নোটিশ দেখাবে。
                                </small>
                            </div>

                            <div id="HideDatePanel" runat="server" visible="false" class="mt-3">
                                <div class="alert alert-success">
                                    <i class="fa fa-check-circle"></i> 
                                    বকেয়া নোটিশ বর্তমানে <strong>চালু</strong> আছে。
                                </div>
                                
                                <div class="form-group">
                                    <label><i class="fa fa-calendar"></i> সাময়িকভাবে লুকাতে চান? (ঐচ্ছিক)</label>
                                    <asp:TextBox ID="HideUntilDateTextBox" runat="server" 
                                        CssClass="form-control datepicker" 
                                        placeholder="তারিখ নির্বাচন করুন (e.g., 31 Jan 2025)" 
                                        autocomplete="off"></asp:TextBox>
                                    <small class="form-text text-muted">
                                        এই তারিখ পর্যন্ত নোটিশ লুকানো থাকবে। খালি থাকলে নোটিশ সবসময় দেখাবে。
                                    </small>
                                </div>
                                
                                <div class="form-group">
                                    <label><i class="fa fa-comment"></i> নোট (ঐচ্ছিক)</label>
                                    <asp:TextBox ID="HideReasonTextBox" runat="server" 
                                        CssClass="form-control" 
                                        TextMode="MultiLine" 
                                        Rows="3"
                                        placeholder="উদাহরণ: ৫০,০০০ টাকা পরিশোধ করেছে, ১ মাসের জন্য নোটিশ লুকানো"></asp:TextBox>
                                </div>

                                <asp:Button ID="SaveDueSettingsButton" runat="server" 
                                    Text="সংরক্ষণ করুন" 
                                    CssClass="btn btn-primary" 
                                    OnClick="SaveDueSettingsButton_Click" />
                            </div>

                            <asp:Label ID="DueSettingsMessageLabel" runat="server" 
                                CssClass="d-block mt-3" 
                                ForeColor="Green"></asp:Label>

                            <!-- Current Status Display -->
                            <asp:FormView ID="DueNoticeStatusFormView" runat="server" 
                                DataSourceID="DueNoticeStatusSQL" 
                                Width="100%"
                                Visible="false">
                                <ItemTemplate>
                                    <div class="alert alert-success mt-3">
                                        <h6><i class="fa fa-bell"></i> বর্তমান স্থিতি</h6>
                                        <ul class="mb-0">
                                            <li><strong>নোটিশ স্ট্যাটাস:</strong> চালু আছে</li>
                                            <li><strong>চালু করা হয়েছে:</strong> <%# Eval("CreatedDate", "{0:dd MMM yyyy hh:mm tt}") %></li>
                                            <%# Eval("HideUntilDate") != DBNull.Value ? 
                                                "<li><strong>সাময়িক লুকানো পর্যন্ত:</strong> " + Eval("HideUntilDate", "{0:dd MMM yyyy}") + "</li>" : 
                                                "<li><strong>স্থিতি:</strong> সবসময় দেখাবে</li>" %>
                                            <%# !string.IsNullOrEmpty(Eval("Reason").ToString()) ? 
                                                "<li><strong>নোট:</strong> " + Eval("Reason") + "</li>" : "" %>
                                        </ul>
                                    </div>
                                </ItemTemplate>
                            </asp:FormView>

                            <asp:SqlDataSource ID="DueNoticeStatusSQL" runat="server" 
                                ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" 
                                SelectCommand="SELECT TOP 1 * FROM SchoolInfo_DueNoticeSettings WHERE SchoolID = @SchoolID AND IsEnabled = 1 ORDER BY CreatedDate DESC">
                                <SelectParameters>
                                    <asp:QueryStringParameter Name="SchoolID" QueryStringField="SchoolID" Type="Int32" />
                                </SelectParameters>
                            </asp:SqlDataSource>
                        </ContentTemplate>
                    </asp:UpdatePanel>
                </div>
            </div>

            <div class="tab-pane fade" role="tabpanel" id="ManagemeUser">
                <asp:UpdatePanel ID="UpdatePanel1" runat="server">
                    <ContentTemplate>
                        <div class="form-inline mb-3">
                            <div class="form-group">
                                <asp:DropDownList ID="UserRoleDropDownList" runat="server" AutoPostBack="True" CssClass="form-control">
                                    <asp:ListItem Value="%">[ SELECT ROLE ]</asp:ListItem>
                                    <asp:ListItem>Admin</asp:ListItem>
                                    <asp:ListItem>Sub-Admin</asp:ListItem>
                                    <asp:ListItem>Teacher</asp:ListItem>
                                    <asp:ListItem>Student</asp:ListItem>
                                </asp:DropDownList>
                            </div>
                        </div>
                        <div class="table-responsive">
                            <asp:GridView ID="UserGridView" runat="server" AutoGenerateColumns="False" DataKeyNames="RegistrationID,UserName" DataSourceID="UserSQL" AllowPaging="True" AllowSorting="True" CssClass="mGrid" PageSize="20">
                                <Columns>
                                    <asp:BoundField DataField="UserName" HeaderText="Username" SortExpression="UserName" />
                                    <asp:BoundField DataField="Validation" HeaderText="Validation" SortExpression="Validation" />
                                    <asp:TemplateField HeaderText="Is Approved" SortExpression="IsApproved">
                                        <ItemTemplate>
                                            <asp:CheckBox ID="ISApprovedCheckBox" runat="server" Checked='<%# Bind("IsApproved") %>' Text=" " AutoPostBack="True" OnCheckedChanged="ISApprovedCheckBox_CheckedChanged" />
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="Is Locked Out" SortExpression="IsLockedOut">

                                        <ItemTemplate>
                                            <asp:CheckBox ID="IsLockedOutCheckBox" runat="server" Checked='<%# Bind("IsLockedOut") %>' Text=" " AutoPostBack="True" OnCheckedChanged="IsLockedOutCheckBox_CheckedChanged" />
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:BoundField DataField="Email" HeaderText="Email" SortExpression="Email" />
                                    <asp:BoundField DataField="CreateDate" HeaderText="Create Date" SortExpression="CreateDate" DataFormatString="{0:d MMM yyyy}" />
                                </Columns>
                                <PagerStyle CssClass="pgr" />
                            </asp:GridView>
                            <asp:SqlDataSource ID="UserSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT Registration.RegistrationID, Registration.SchoolID, Registration.UserName, Registration.Validation, Registration.CreateDate, aspnet_Membership.IsApproved, aspnet_Membership.IsLockedOut, aspnet_Membership.Email FROM aspnet_Users INNER JOIN aspnet_Membership ON aspnet_Users.UserId = aspnet_Membership.UserId INNER JOIN Registration ON aspnet_Users.UserName = Registration.UserName WHERE (Registration.SchoolID = @SchoolID) AND (Registration.Category = @Category)">
                                <SelectParameters>
                                    <asp:QueryStringParameter Name="SchoolID" QueryStringField="SchoolID" Type="Int32" />
                                    <asp:ControlParameter ControlID="UserRoleDropDownList" Name="Category" PropertyName="SelectedValue" Type="String" />
                                </SelectParameters>
                            </asp:SqlDataSource>
                            <asp:SqlDataSource ID="UpdateRegSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT * FROM [Registration]" UpdateCommand="UPDATE SchoolInfo SET Validation = @Validation WHERE (UserName = @UserName)">
                                <UpdateParameters>
                                    <asp:Parameter Name="Validation" />
                                    <asp:Parameter Name="UserName" Type="String" />
                                </UpdateParameters>
                            </asp:SqlDataSource>
                        </div>
                    </ContentTemplate>
                </asp:UpdatePanel>
            </div>

            <div class="tab-pane fade" role="tabpanel" id="DeleteID">
                <asp:UpdatePanel ID="UpdatePanel2" runat="server">
                    <ContentTemplate>
                        <div class="form-inline">
                            <div class="form-group">
                                <asp:TextBox ID="StudentIDTextBox" placeholder="Enter ID" runat="server" CssClass="form-control"></asp:TextBox>
                            </div>
                            <div class="form-group">
                                <asp:Button ID="DeleteIDButton" runat="server" OnClick="DeleteIDButton_Click" Text="Student Delete" CssClass="btn btn-info" />
                            </div>
                            <asp:Label ID="IDerrorLabel" runat="server" ForeColor="#33CC33"></asp:Label>
                        </div>
                        <asp:SqlDataSource ID="DeleteStudentSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" DeleteCommand="if not exists(SELECT StudentsClass.StudentID FROM StudentsClass INNER JOIN Student ON StudentsClass.StudentID = Student.StudentID WHERE (Student.SchoolID = @SchoolID) AND (Student.ID = @ID))
BEGIN
DELETE FROM Student_Image FROM Student_Image INNER JOIN Student ON Student_Image.StudentImageID = Student.StudentImageID WHERE (Student.SchoolID = @SchoolID) AND (Student.ID = @ID)
DELETE FROM Student WHERE (Student.SchoolID = @SchoolID) AND (Student.ID = @ID)
END"
                            SelectCommand="SELECT FROM Student">
                            <DeleteParameters>
                                <asp:QueryStringParameter Name="SchoolID" QueryStringField="SchoolID" />
                                <asp:ControlParameter ControlID="StudentIDTextBox" Name="ID" PropertyName="Text" />
                            </DeleteParameters>
                        </asp:SqlDataSource>
                    </ContentTemplate>
                </asp:UpdatePanel>
            </div>

            <div class="tab-pane fade" role="tabpanel" id="IDChange">
                <asp:UpdatePanel ID="UpdatePanel3" runat="server">
                    <ContentTemplate>
                        <div class="form-inline">
                            <div class="form-group">
                                <asp:TextBox ID="OldID_TextBox" placeholder="Enter Old ID" runat="server" CssClass="form-control"></asp:TextBox>
                            </div>
                            <div class="form-group">
                                <asp:Button ID="IDChangeFindButton" runat="server" Text="Find" CssClass="btn btn-amber" />
                            </div>
                        </div>

                        <asp:FormView DefaultMode="Edit" ID="IDChangeInfoFV" runat="server" OnItemUpdated="IDChangeInfoFV_ItemUpdated" DataSourceID="IDChangeInfoSQL" DataKeyNames="StudentID" Width="100%">
                            <EditItemTemplate>
                                <ul class="list-group list-group-flush">
                                    <li class="list-group-item"><%# Eval("StudentsName") %></li>
                                    <li class="list-group-item">Class: <%# Eval("Class") %></li>
                                    <li class="list-group-item">RollNo: <%# Eval("RollNo") %></li>
                                    <li class="list-group-item">Status: <%# Eval("Status") %></li>
                                    <li class="list-group-item">
                                        <asp:TextBox ID="IDTextBox" runat="server" CssClass="form-control" placeholder="New ID" Text='<%# Bind("ID") %>' /></li>
                                </ul>

                                <asp:LinkButton ID="UpdateButton" runat="server" CausesValidation="True" CommandName="Update" CssClass="btn btn-blue-grey" Text="Update" />
                            </EditItemTemplate>
                        </asp:FormView>
                        <asp:SqlDataSource ID="IDChangeInfoSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT Student.ID, Student.StudentsName, CreateClass.Class, Student.StudentID, Student.Status, StudentsClass.RollNo FROM StudentsClass INNER JOIN CreateClass ON StudentsClass.ClassID = CreateClass.ClassID INNER JOIN Student ON StudentsClass.StudentID = Student.StudentID WHERE (Student.SchoolID = @SchoolID) AND (Student.ID = @ID)" UpdateCommand="IF NOT  EXISTS(SELECT ID FROM Employee_Info WHERE (SchoolID = @SchoolID) AND (ID = @ID) UNION SELECT ID FROM Student WHERE (SchoolID = @SchoolID) AND (ID = @ID))
BEGIN
UPDATE Student SET ID = @ID WHERE (SchoolID = @SchoolID) AND (ID = @Old_ID)
END">
                            <SelectParameters>
                                <asp:QueryStringParameter Name="SchoolID" QueryStringField="SchoolID" />
                                <asp:ControlParameter ControlID="OldID_TextBox" Name="ID" PropertyName="Text" />
                            </SelectParameters>
                            <UpdateParameters>
                                <asp:QueryStringParameter Name="SchoolID" QueryStringField="SchoolID" />
                                <asp:ControlParameter ControlID="OldID_TextBox" Name="Old_ID" PropertyName="Text" />
                                <asp:Parameter Name="ID" />
                            </UpdateParameters>
                        </asp:SqlDataSource>
                        <asp:SqlDataSource ID="Device_DataUpdateSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" InsertCommand="IF NOT EXISTS(SELECT DateUpdateID FROM  Attendance_Device_DataUpdateList WHERE (SchoolID = @SchoolID) AND (UpdateType = @UpdateType))
BEGIN
INSERT INTO Attendance_Device_DataUpdateList(SchoolID, RegistrationID, UpdateType, UpdateDescription) VALUES (@SchoolID, @RegistrationID, @UpdateType, @UpdateDescription)
END"
                            SelectCommand="SELECT * FROM [Attendance_Device_DataUpdateList]">
                            <InsertParameters>
                                <asp:QueryStringParameter Name="SchoolID" QueryStringField="SchoolID" Type="Int32" />
                                <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" Type="Int32" />
                                <asp:Parameter DefaultValue="Student ID Change" Name="UpdateType" Type="String" />
                                <asp:Parameter DefaultValue="Student ID Change by authority" Name="UpdateDescription" Type="String" />
                            </InsertParameters>
                        </asp:SqlDataSource>
                    </ContentTemplate>
                </asp:UpdatePanel>
            </div>

            <div class="tab-pane fade" role="tabpanel" id="ReceiptDelete">
                <asp:UpdatePanel ID="UpdatePanel4" runat="server">
                    <ContentTemplate>
                        <div class="form-inline">
                            <div class="form-group">
                                <asp:TextBox ID="ReceiptTextBox" placeholder="Receipt No" CssClass="form-control" runat="server"></asp:TextBox>
                            </div>
                            <div class="form-group">
                                <asp:Button ID="FindButton" runat="server" Text="Find" CssClass="btn btn-black" />
                            </div>
                        </div>

                        <asp:FormView ID="StudentInfoFormView" runat="server" DataSourceID="StudentInfoSQL" DataKeyNames="SMSPhoneNo,StudentID" Width="100%">
                            <ItemTemplate>
                                <div class="Info">
                                    <ul>
                                        <li>
                                            <span class="btn btn-info">ID: <%# Eval("ID") %></span>
                                            <span class="btn btn-info">Name: <%# Eval("StudentsName") %> </span>
                                            <span class="btn btn-info">Class: <%# Eval("Class") %></span>
                                        </li>
                                    </ul>
                                </div>
                            </ItemTemplate>
                        </asp:FormView>
                        <asp:SqlDataSource ID="StudentInfoSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT Student.ID, Student.SMSPhoneNo, Student.StudentsName, CreateClass.Class, Student.StudentID, Income_MoneyReceipt.MoneyReceipt_SN, Income_MoneyReceipt.MoneyReceiptID FROM StudentsClass INNER JOIN CreateClass ON StudentsClass.ClassID = CreateClass.ClassID INNER JOIN Student ON StudentsClass.StudentID = Student.StudentID INNER JOIN Income_MoneyReceipt ON StudentsClass.StudentClassID = Income_MoneyReceipt.StudentClassID WHERE (Student.SchoolID = @SchoolID)  AND (Income_MoneyReceipt.MoneyReceipt_SN = @MoneyReceipt_SN)">
                            <SelectParameters>
                                <asp:QueryStringParameter Name="SchoolID" QueryStringField="SchoolID" />
                                <asp:ControlParameter ControlID="ReceiptTextBox" Name="MoneyReceipt_SN" PropertyName="Text" />
                            </SelectParameters>
                        </asp:SqlDataSource>

                        <asp:FormView ID="ReceiptFormView" runat="server" DataSourceID="MoneyRSQL" Width="100%" DataKeyNames="TotalAmount">
                            <ItemTemplate>
                                <div class="Info">
                                    <ul>
                                        <li>
                                            <span class="btn btn-default">Receipt No: <%# Eval("MoneyReceipt_SN") %> </span>
                                            <span class="btn btn-danger">Total Amount: <%# Eval("TotalAmount") %> Tk</span>
                                            <span class="btn btn-default">Paid Date: <%# Eval("PaidDate","{0:d-MMM-yy (hh:mm tt)}") %></span>
                                        </li>
                                    </ul>
                                </div>
                            </ItemTemplate>
                        </asp:FormView>
                        <asp:SqlDataSource ID="MoneyRSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                            SelectCommand="SELECT PaidDate, MoneyReceipt_SN, TotalAmount FROM Income_MoneyReceipt WHERE (SchoolID = @SchoolID) AND (MoneyReceipt_SN = @MoneyReceipt_SN)" DeleteCommand="UPDATE Income_PayOrder SET PaidAmount = Income_PayOrder.PaidAmount - Income_PaymentRecord.PaidAmount ,NumberOfPayment = 0, LastPaidDate = NULL 
FROM Income_MoneyReceipt INNER JOIN Income_PaymentRecord ON Income_MoneyReceipt.MoneyReceiptID = Income_PaymentRecord.MoneyReceiptID INNER JOIN Income_PayOrder
ON Income_PaymentRecord.PayOrderID = Income_PayOrder.PayOrderID
WHERE (Income_MoneyReceipt.SchoolID = @SchoolID) AND (Income_MoneyReceipt.MoneyReceipt_SN = @MoneyReceipt_SN)

DELETE FROM Income_PaymentRecord FROM Income_MoneyReceipt INNER JOIN Income_PaymentRecord ON Income_MoneyReceipt.MoneyReceiptID = Income_PaymentRecord.MoneyReceiptID 
WHERE (Income_MoneyReceipt.SchoolID = @SchoolID) AND (Income_MoneyReceipt.MoneyReceipt_SN = @MoneyReceipt_SN)

DELETE FROM Income_MoneyReceipt WHERE (SchoolID = @SchoolID) AND (MoneyReceipt_SN = @MoneyReceipt_SN)">
                            <DeleteParameters>
                                <asp:QueryStringParameter Name="SchoolID" QueryStringField="SchoolID" />
                                <asp:ControlParameter ControlID="ReceiptTextBox" Name="MoneyReceipt_SN" PropertyName="Text" />
                            </DeleteParameters>
                            <SelectParameters>
                                <asp:QueryStringParameter Name="SchoolID" QueryStringField="SchoolID" />
                                <asp:ControlParameter ControlID="ReceiptTextBox" Name="MoneyReceipt_SN" PropertyName="Text" />
                            </SelectParameters>
                        </asp:SqlDataSource>

                        <div class="table-responsive mb-2">
                            <asp:GridView ID="PaidDetailsGridView" runat="server" AutoGenerateColumns="False" DataSourceID="PaidDetailsSQL" CssClass="mGrid" ShowFooter="True" Font-Bold="False" RowStyle-CssClass="Rows">
                                <Columns>
                                    <asp:BoundField DataField="PayFor" HeaderText="Pay For" />
                                    <asp:TemplateField HeaderText="Fee">
                                        <ItemTemplate>
                                            <asp:Label ID="RoleLabel" runat="server" Text='<%# Bind("Role")%>' />
                                            : 
                                        <asp:Label ID="Label2" runat="server" Text='<%# Bind("Amount") %>' />
                                        </ItemTemplate>
                                        <ItemStyle HorizontalAlign="Right" />
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="Paid">
                                        <ItemTemplate>
                                            <asp:Label ID="PaidAmountLabel" runat="server" Text='<%# Bind("PaidAmount") %>'></asp:Label>
                                        </ItemTemplate>
                                        <FooterTemplate>
                                            <label id="PGTLabel"></label>
                                            <asp:HiddenField ID="PaidHF" runat="server" />
                                        </FooterTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="Due">
                                        <ItemTemplate>
                                            <asp:Label ID="Label1" runat="server" Text='<%# Bind("Due") %>'></asp:Label>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                </Columns>
                                <FooterStyle CssClass="GVfooter" />

                                <RowStyle CssClass="Rows"></RowStyle>
                            </asp:GridView>
                            <asp:SqlDataSource ID="PaidDetailsSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                                SelectCommand="SELECT Income_PaymentRecord.MoneyReceiptID, Income_Roles.Role, Income_PaymentRecord.PayFor, Income_PaymentRecord.PaidAmount, Income_PayOrder.Receivable_Amount AS Due, Income_PaymentRecord.PaidDate, Income_PayOrder.Amount, Income_MoneyReceipt.MoneyReceipt_SN FROM Income_PaymentRecord INNER JOIN Income_Roles ON Income_PaymentRecord.RoleID = Income_Roles.RoleID INNER JOIN Income_PayOrder ON Income_PaymentRecord.PayOrderID = Income_PayOrder.PayOrderID INNER JOIN Income_MoneyReceipt ON Income_PaymentRecord.MoneyReceiptID = Income_MoneyReceipt.MoneyReceiptID WHERE (Income_PaymentRecord.SchoolID = @SchoolID) AND (Income_MoneyReceipt.MoneyReceipt_SN = @MoneyReceipt_SN)">
                                <SelectParameters>
                                    <asp:QueryStringParameter Name="SchoolID" QueryStringField="SchoolID" />
                                    <asp:ControlParameter ControlID="ReceiptTextBox" Name="MoneyReceipt_SN" PropertyName="Text" />
                                </SelectParameters>
                            </asp:SqlDataSource>
                        </div>

                        <%if (PaidDetailsGridView.Rows.Count > 0)
                            {%>
                        <asp:Button ID="DeleteReceiptButton" runat="server" OnClick="DeleteReceiptButton_Click" Text="Receipt Delete" CssClass="btn btn-info" />
                        <%} %>
                    </ContentTemplate>
                </asp:UpdatePanel>
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

    <script>
        $(function () {
            if ($("[id*=NewInvoiceGridView] tr").length) {
                $(".Show").show();
            }

            if ($("[id*=PaidRecordGridView] tr").length) {
                $(".IS").show();
            }

            // Initialize Bootstrap Datepicker
            $('.datepicker').datepicker({
                format: 'dd M yyyy',
                autoclose: true,
                todayHighlight: true,
                orientation: 'bottom auto'
            });
        });

        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function (a, b) {
            //Space not allow
            $('[id*=IDTextBox]').on("keypress", function (e) {
                if (e.which === 32) {
                    return false;
                }
            });

            // Re-initialize datepicker after UpdatePanel postback
            $('.datepicker').datepicker({
                format: 'dd M yyyy',
                autoclose: true,
                todayHighlight: true,
                orientation: 'bottom auto'
            });
        })
    </script>
</asp:Content>
