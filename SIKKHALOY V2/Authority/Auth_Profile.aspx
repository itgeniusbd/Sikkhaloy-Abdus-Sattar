<%@ Page Title="Authority Profile" Language="C#" MasterPageFile="~/Basic_Authority.Master" AutoEventWireup="true" CodeBehind="Auth_Profile.aspx.cs" Inherits="EDUCATION.COM.Authority.Auth_Profile" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .mGrid { text-align: left; }
        .Invaid_Ins td { color: #ff2b2b; }
        .Invaid_Ins td a { color: #ff2b2b; }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <div class="form-inline">
        <div class="md-form">
            <asp:TextBox ID="SearchTextBox" placeholder="Institution, Username" CssClass="form-control" runat="server"></asp:TextBox>
        </div>
        <div class="md-form">
            <asp:Button ID="FIndButton" runat="server" Text="Find" CssClass="btn btn-primary btn-sm" />
        </div>
        <div class="md-form">
            <button type="button" class="btn btn-cyan btn-sm" data-toggle="modal" data-target="#exampleModal"><i class="fa fa-bullhorn mr-1" aria-hidden="true"></i>Add Notice</button>
        </div>
    </div>

    <div class="table-responsive">
        <asp:GridView ID="SchoolGridView" CssClass="mGrid" runat="server" AutoGenerateColumns="False" DataKeyNames="SchoolID" DataSourceID="InstitutionSQL" AllowSorting="True">
            <Columns>
                <asp:BoundField DataField="SchoolID" HeaderText="School ID" SortExpression="SchoolID" />
                <asp:HyperLinkField SortExpression="SchoolName" DataNavigateUrlFields="SchoolID" DataNavigateUrlFormatString="Institutions/Institution_Details.aspx?SchoolID={0}" DataTextField="SchoolName" HeaderText="Select" />
                <asp:BoundField DataField="UserName" HeaderText="User id" SortExpression="UserName" />
                <asp:BoundField DataField="Phone" HeaderText="Phone" SortExpression="Phone" />
                <asp:BoundField DataField="Validation" HeaderText="Validation" SortExpression="Validation" />
                <asp:TemplateField HeaderText="Act. Session" SortExpression="EducationYear">
                    <ItemTemplate>
                        <asp:HiddenField ID="SchoolIDHF" runat="server" Value='<%#Eval("SchoolID") %>' />
                        <asp:Repeater ID="SessionRepeater" runat="server" DataSourceID="AcSessionSQL">
                            <HeaderTemplate>
                                <ul class="list-group">
                            </HeaderTemplate>
                            <ItemTemplate>
                                <li class="list-group-item p-0 border-0">
                                    <i class="fa fa-check-square-o" aria-hidden="true"></i>
                                    <%#Eval("EducationYear") %></li>
                            </ItemTemplate>
                            <FooterTemplate>
                                </ul>
                            </FooterTemplate>
                        </asp:Repeater>
                        <asp:SqlDataSource ID="AcSessionSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT EducationYear FROM Education_Year WHERE (SchoolID = @SchoolID) AND (IsActive = 1)">
                            <SelectParameters>
                                <asp:ControlParameter ControlID="SchoolIDHF" Name="SchoolID" PropertyName="Value" />
                            </SelectParameters>
                        </asp:SqlDataSource>
                    </ItemTemplate>
                </asp:TemplateField>

            </Columns>
            <EmptyDataTemplate>
                No Found !
            </EmptyDataTemplate>
        </asp:GridView>
        <asp:SqlDataSource ID="InstitutionSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT SchoolID, SchoolName, Phone, Validation, Date, UserName FROM SchoolInfo AS Sch ORDER BY SchoolID"
            FilterExpression="SchoolName like '{0}%' OR UserName like '{0}%'">
            <FilterParameters>
                <asp:ControlParameter ControlID="SearchTextBox" Name="Find" PropertyName="Text" />
            </FilterParameters>
        </asp:SqlDataSource>
    </div>

    <!-- Modal -->
    <div class="modal fade" id="exampleModal" tabindex="-1" role="dialog" aria-labelledby="exampleModalLabel" aria-hidden="true">
        <div class="modal-dialog" role="document">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title" id="exampleModalLabel">Add Notice For All Isntitution</h5>
                    <button type="button" class="close" data-dismiss="modal" aria-label="Close">
                        <span aria-hidden="true">&times;</span>
                    </button>
                </div>
                <div class="modal-body">
                    <asp:UpdatePanel ID="UpdatePanel1" runat="server">
                        <ContentTemplate>
                            <div class="form-group">
                                <label>Notice Title<asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" ControlToValidate="Notice_TitleTextBox" CssClass="EroorStar" ErrorMessage="Required" ValidationGroup="N"></asp:RequiredFieldValidator></label>
                                <asp:TextBox ID="Notice_TitleTextBox" placeholder="Notice Title" runat="server" CssClass="form-control"></asp:TextBox>
                            </div>
                            <div class="form-group">
                                <label>Show From Date<asp:RequiredFieldValidator ID="RequiredFieldValidator2" runat="server" ControlToValidate="ShowFromDateTextBox" CssClass="EroorStar" ErrorMessage="Required" ValidationGroup="N"></asp:RequiredFieldValidator></label>
                                <asp:TextBox ID="ShowFromDateTextBox" placeholder="From Date" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false" runat="server" CssClass="form-control datepicker"></asp:TextBox>
                            </div>
                            <div class="form-group">
                                <label>Show To Date<asp:RequiredFieldValidator ID="RequiredFieldValidator3" runat="server" ControlToValidate="ShowToDateTextBox" CssClass="EroorStar" ErrorMessage="Required" ValidationGroup="N"></asp:RequiredFieldValidator></label>
                                <asp:TextBox ID="ShowToDateTextBox" placeholder="To Date" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false" runat="server" CssClass="form-control datepicker"></asp:TextBox>
                            </div>
                            <div class="form-group">
                                <label>Notice (Text)</label>
                                <asp:TextBox ID="NoticeTextBox" placeholder="Notice Text" runat="server" CssClass="form-control" TextMode="MultiLine"></asp:TextBox>
                            </div>

                            <asp:Button ID="SubmitButton" runat="server" CssClass="btn btn-primary" Text="Submit" OnClick="SubmitButton_Click" ValidationGroup="N" />
                            <asp:SqlDataSource ID="NoticeSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" DeleteCommand="DELETE FROM Notice_Admin WHERE [AdminNoticeID] = @AdminNoticeID" InsertCommand="INSERT INTO Notice_Admin(Notice_Title, Notice, Show_Date, End_Date, RegistrationID) VALUES (@Notice_Title, @Notice, @Show_Date, @End_Date, @RegistrationID)" SelectCommand="SELECT * FROM Notice_Admin" UpdateCommand="UPDATE Notice_Admin SET Notice_Title = @Notice_Title, Notice = @Notice, Show_Date = @Show_Date, End_Date = @End_Date WHERE (AdminNoticeID = @AdminNoticeID)">
                                <DeleteParameters>
                                    <asp:Parameter Name="AdminNoticeID" Type="Int32" />
                                </DeleteParameters>
                                <InsertParameters>
                                    <asp:ControlParameter ControlID="Notice_TitleTextBox" Name="Notice_Title" PropertyName="Text" Type="String" />
                                    <asp:ControlParameter ControlID="NoticeTextBox" Name="Notice" PropertyName="Text" Type="String" />
                                    <asp:ControlParameter ControlID="ShowFromDateTextBox" DbType="Date" Name="Show_Date" PropertyName="Text" />
                                    <asp:ControlParameter ControlID="ShowToDateTextBox" DbType="Date" Name="End_Date" PropertyName="Text" />
                                    <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" />
                                </InsertParameters>
                                <UpdateParameters>
                                    <asp:Parameter Name="Notice_Title" Type="String" />
                                    <asp:Parameter Name="Notice" Type="String" />
                                    <asp:Parameter DbType="Date" Name="Show_Date" />
                                    <asp:Parameter DbType="Date" Name="End_Date" />
                                    <asp:Parameter Name="AdminNoticeID" Type="Int32" />
                                </UpdateParameters>
                            </asp:SqlDataSource>

                            <div class="table-responsive">
                                <asp:GridView ID="Notice_GridView" runat="server" CssClass="mGrid" AutoGenerateColumns="False" DataKeyNames="AdminNoticeID" DataSourceID="NoticeSQL">
                                    <Columns>
                                        <asp:TemplateField HeaderText="Notice">
                                            <ItemTemplate>
                                                <div>
                                                    <h4>
                                                        <asp:Label ID="Label3" runat="server" Text='<%# Bind("Notice_Title") %>'></asp:Label></h4>
                                                </div>

                                                <asp:Label ID="Label4" runat="server" Text='<%# Bind("Notice") %>'></asp:Label>

                                                <div>
                                                    <div><strong>Display Date</strong></div>
                                                    <asp:Label ID="Label1" runat="server" Text='<%# Bind("Show_Date", "{0:d MMM yyyy}") %>'></asp:Label>
                                                    TO
                            <asp:Label ID="Label2" runat="server" Text='<%# Bind("End_Date", "{0:d MMM yyyy}") %>'></asp:Label>
                                                </div>

                                                Add Date:
                            <asp:Label ID="Label5" runat="server" Text='<%# Bind("Insert_Date", "{0:d MMM yyyy}") %>'></asp:Label>

                                                <div>
                                                    <asp:LinkButton ID="LinkButton1" runat="server" CausesValidation="False" CommandName="Edit" Text="Edit Notice"></asp:LinkButton>
                                                    |
                            <asp:LinkButton ID="LinkButton4" runat="server" CausesValidation="False" CommandName="Delete" Text="Delete" OnClientClick="return confirm('are you sure want to delete?')"></asp:LinkButton>
                                                </div>
                                            </ItemTemplate>
                                            <EditItemTemplate>
                                                <div class="form-group">
                                                    <label>Notice Title</label>
                                                    <asp:TextBox ID="TextBox3" CssClass="form-control" runat="server" Text='<%# Bind("Notice_Title") %>'></asp:TextBox>
                                                </div>
                                                <div class="form-group">
                                                    <label>Notice</label>
                                                    <asp:TextBox ID="TextBox4" CssClass="form-control" runat="server" TextMode="MultiLine" Text='<%# Bind("Notice") %>'></asp:TextBox>
                                                </div>
                                                <div class="form-group">
                                                    <label>Display From Date</label>
                                                    <asp:TextBox ID="TextBox1" CssClass="form-control datepicker" runat="server" Text='<%# Bind("Show_Date", "{0:d MMM yyyy}") %>'></asp:TextBox>
                                                </div>
                                                <div class="form-group">
                                                    <label>Display To Date</label>
                                                    <asp:TextBox ID="TextBox2" CssClass="form-control datepicker" runat="server" Text='<%# Bind("End_Date", "{0:d MMM yyyy}") %>'></asp:TextBox>
                                                </div>

                                                <asp:LinkButton ID="LinkButton2" runat="server" CausesValidation="True" CommandName="Update" Text="Update"></asp:LinkButton>
                                                <asp:LinkButton ID="LinkButton3" runat="server" CausesValidation="True" CommandName="Cancel" Text="Cancel"></asp:LinkButton>
                                            </EditItemTemplate>
                                        </asp:TemplateField>
                                    </Columns>
                                </asp:GridView>
                            </div>
                        </ContentTemplate>
                    </asp:UpdatePanel>
                </div>
            </div>
        </div>
    </div>


    <script type='text/javascript'>
        $(function () {
            $('.mGrid tr').each(function () {
                if ($(this).find('td:nth-child(5)').text().trim() === "Invalid") {
                    $(this).addClass("Invaid_Ins");
                }
            });

            $('.datepicker').datepicker({
                format: 'dd M yyyy',
                todayBtn: "linked",
                todayHighlight: true,
                autoclose: true
            });
        });

        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function (a, b) {
            $('.datepicker').datepicker({
                format: 'dd M yyyy',
                todayBtn: "linked",
                todayHighlight: true,
                autoclose: true
            });
        });
    </script>
</asp:Content>
