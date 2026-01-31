<%@ Page Title="Donor Login Management" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Create_Donor_Username_password.aspx.cs" Inherits="EDUCATION.COM.Committee.Create_Donor_Username_password" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .mGrid th { text-align: left; }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <h3>Donor Login Userid & Password</h3>


    <div class="row mb-3">
        <div class="col-md-4">
            <div class="form-group">
                <label>Donor Type</label>
                <asp:DropDownList ID="DonorTypeDropDownList" runat="server" CssClass="form-control" AppendDataBoundItems="True" AutoPostBack="True" DataSourceID="DonorTypeSQL" DataTextField="CommitteeMemberType" DataValueField="CommitteeMemberTypeId" OnSelectedIndexChanged="DonorTypeDropDownList_SelectedIndexChanged">
                    <asp:ListItem Value="">[ ALL TYPE ]</asp:ListItem>
                </asp:DropDownList>
                <asp:SqlDataSource ID="DonorTypeSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                    SelectCommand="SELECT CommitteeMemberTypeId, CommitteeMemberType FROM CommitteeMemberType WHERE (SchoolID = @SchoolID)">
                    <SelectParameters>
                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                    </SelectParameters>
                </asp:SqlDataSource>
            </div>
        </div>
        <div class="col-md-4">
            <div class="form-group">
                <label>Find Donor (Name/Phone)</label>
                <asp:TextBox ID="FindDonorTextBox" runat="server" CssClass="form-control" placeholder="Enter Name or Phone"></asp:TextBox>
            </div>
        </div>
        <div class="col-md-2">
            <div class="form-group" style="padding-top: 1.8rem">
                <asp:Button ID="FindButton" runat="server" CssClass="btn btn-primary btn-md m-0" Text="Find" OnClick="FindButton_Click" />
            </div>
        </div>
    </div>

    <div class="alert alert-info d-print-none">
        <strong>Debug Info:</strong> Total Donors Found: <asp:Label ID="DebugLabel" runat="server" Text="Loading..."></asp:Label>
    </div>

    <ul class="nav nav-tabs z-depth-1">
        <li class="nav-item">
            <a class="nav-link active" data-toggle="tab" href="#panel1" role="tab">Create Userid & Password</a>
        </li>
        <li class="nav-item">
            <a class="nav-link" data-toggle="tab" href="#panel2" role="tab">Already Created</a>
        </li>
    </ul>

    <div class="tab-content card">
        <!-- Create User Pane -->
        <div class="tab-pane fade show active" id="panel1" role="tabpanel">
            <div class="table-responsive">
                <asp:GridView ID="CreateUserGridView" runat="server" AutoGenerateColumns="False" CssClass="mGrid" 
                    DataKeyNames="CommitteeMemberId,MemberName,SmsNumber"
                    EmptyDataText="কোনো ডোনার পাওয়া যায়নি যাদের এখনও ইউজার আইডি তৈরি করা হয়নি।">
                    <Columns>
                        <asp:TemplateField>
                            <HeaderTemplate>
                                <asp:CheckBox ID="AllCheckBox" runat="server" Text=" " OnCheckedChanged="AllCheckBox_CheckedChanged" AutoPostBack="true" />
                            </HeaderTemplate>
                            <ItemTemplate>
                                <asp:CheckBox ID="SingleCheckBox" runat="server" Text=" " />
                            </ItemTemplate>
                            <ItemStyle Width="40px" />
                        </asp:TemplateField>
                        <asp:BoundField DataField="MemberName" HeaderText="Name" />
                        <asp:BoundField DataField="SmsNumber" HeaderText="Phone" />
                        <asp:BoundField DataField="CommitteeMemberType" HeaderText="Type" />
                        <asp:BoundField DataField="Address" HeaderText="Address" />
                    </Columns>
                    <EmptyDataRowStyle CssClass="alert alert-warning" />
                </asp:GridView>
            </div>
            <div class="card-footer mt-2">
                <asp:Button ID="CreateUserButton" runat="server" Text="Create Selected User Accounts" CssClass="btn btn-success" OnClick="CreateUserButton_Click" />
                <asp:Label ID="ErrorLabel" runat="server" CssClass="text-danger ml-3"></asp:Label>
            </div>
        </div>

        <!-- Already Created Pane -->
        <div class="tab-pane fade" id="panel2" role="tabpanel">
            <div class="table-responsive">
                <asp:GridView ID="AlreadyCreatedGridView" runat="server" AutoGenerateColumns="False" CssClass="mGrid" 
                    DataKeyNames="CommitteeMemberId,MemberName,SmsNumber,UserName,Password"
                    EmptyDataText="এখনও কোনো ডোনারের ইউজার আইডি তৈরি করা হয়নি।">
                    <Columns>
                        <asp:TemplateField>
                            <HeaderTemplate>
                                <asp:CheckBox ID="AllSMSCheckBox" runat="server" Text=" SMS" OnCheckedChanged="AllSMSCheckBox_CheckedChanged" AutoPostBack="true" />
                            </HeaderTemplate>
                            <ItemTemplate>
                                <asp:CheckBox ID="SingleSMSCheckBox" runat="server" Text=" " />
                            </ItemTemplate>
                            <ItemStyle Width="60px" />
                        </asp:TemplateField>
                        <asp:BoundField DataField="MemberName" HeaderText="Name" />
                        <asp:BoundField DataField="SmsNumber" HeaderText="Phone" />
                        <asp:BoundField DataField="UserName" HeaderText="Username" />
                        <asp:BoundField DataField="Password" HeaderText="Password" />
                        <asp:BoundField DataField="CreateDate" HeaderText="Created Date" DataFormatString="{0:d MMM yyyy}" />
                    </Columns>
                    <EmptyDataRowStyle CssClass="alert alert-info" />
                </asp:GridView>
            </div>
            <div class="card-footer mt-2">
                <asp:Button ID="SendSMSButton" runat="server" Text="Send SMS to Selected" CssClass="btn btn-info" OnClick="SendSMSButton_Click" />
            </div>
        </div>
    </div>

    <!-- Hidden DataSources for Insert/Update -->
    <asp:SqlDataSource ID="RegistrationSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
        InsertCommand="INSERT INTO Registration (SchoolID, UserName, CommitteeMemberId, Validation, Category, CreateDate) VALUES (@SchoolID, @UserName, @CommitteeMemberId, 'Valid', 'Donor', GETDATE()); SELECT @RegistrationID = SCOPE_IDENTITY()" OnInserted="RegistrationSQL_Inserted">
        <InsertParameters>
            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
            <asp:Parameter Name="UserName" />
            <asp:Parameter Name="CommitteeMemberId" Type="Int32" />
            <asp:Parameter Name="RegistrationID" Direction="Output" Type="Int32" />
        </InsertParameters>
    </asp:SqlDataSource>

    <asp:SqlDataSource ID="ASTSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
        InsertCommand="INSERT INTO AST (RegistrationID, SchoolID, UserName, Password, SmsNumber, Category) VALUES (@RegistrationID, @SchoolID, @UserName, @Password, @SmsNumber, 'Donor')">
        <InsertParameters>
            <asp:Parameter Name="RegistrationID" />
            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
            <asp:Parameter Name="UserName" />
            <asp:Parameter Name="Password" />
            <asp:Parameter Name="SmsNumber" />
        </InsertParameters>
    </asp:SqlDataSource>

    <asp:SqlDataSource ID="EduYearUserSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
        InsertCommand="INSERT INTO Education_Year_User (SchoolID, RegistrationID, EducationYearID) VALUES (@SchoolID, @RegistrationID, @EducationYearID)">
        <InsertParameters>
            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
            <asp:Parameter Name="RegistrationID" />
            <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
        </InsertParameters>
    </asp:SqlDataSource>

</asp:Content>
