<%@ Page Title="Send SMS to Committee" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="SendSMS.aspx.cs" Inherits="EDUCATION.COM.Committee.SendSMS" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <div class="row">
        <div class="col-md-10 col-lg-8 mx-auto">
            <div class="card card-body">
                <h2 class="font-weight-bold mb-3">Send SMS To Committee</h2>

                <asp:GridView ID="MemberTypeGridView" runat="server" AutoGenerateColumns="False" DataKeyNames="CommitteeMemberTypeId" CssClass="mGrid" DataSourceID="MemberTypeSQL">
                    <Columns>
                        <asp:TemplateField HeaderText="Select">
                            <HeaderTemplate>
                                <asp:CheckBox ID="SelectAllCheckBox" onclick="allCheckBox(this)"  runat="server" Text=" " />
                            </HeaderTemplate>
                            <ItemTemplate>
                                <asp:CheckBox ID="SelectCheckBox"  runat="server" Text=" " />
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Type" SortExpression="CommitteeMemberType">
                            <ItemTemplate>
                                <%# Eval("CommitteeMemberType") %> (<%# Eval("TotalMember") %>)
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                    <EmptyDataTemplate>
                        Empty
                    </EmptyDataTemplate>
                    <PagerStyle CssClass="pgr"></PagerStyle>
                    <RowStyle CssClass="RowStyle" />
                </asp:GridView>
                <asp:SqlDataSource ID="MemberTypeSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                    SelectCommand="SELECT CommitteeMember.CommitteeMemberTypeId, CommitteeMemberType.CommitteeMemberType, COUNT(CommitteeMember.CommitteeMemberTypeId) AS TotalMember FROM CommitteeMember INNER JOIN CommitteeMemberType ON CommitteeMember.CommitteeMemberTypeId = CommitteeMemberType.CommitteeMemberTypeId WHERE (CommitteeMember.SchoolID = @SchoolID) GROUP BY CommitteeMember.CommitteeMemberTypeId, CommitteeMemberType.CommitteeMemberType">
                    <SelectParameters>
                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                    </SelectParameters>
                </asp:SqlDataSource>


                <div class="form-group mt-3">
                    <label>SMS Text</label>
                    <asp:TextBox ID="SMSTextBox" runat="server" CssClass="form-control" TextMode="MultiLine" required=""></asp:TextBox>
                </div>

                <asp:FormView ID="SMSFormView" runat="server" DataKeyNames="SMSID" DataSourceID="SMSSQL" RenderOuterTable="false">
                    <ItemTemplate>
                        Remaining SMS:<%# Eval("SMS_Balance") %>
                    </ItemTemplate>
                </asp:FormView>
                <asp:SqlDataSource ID="SMSSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT * FROM [SMS] WHERE ([SchoolID] = @SchoolID)">
                    <SelectParameters>
                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                    </SelectParameters>
                </asp:SqlDataSource>

                <asp:Button ID="SMSButton" runat="server" Text="Send SMS" CssClass="btn btn-primary ml-0" OnClick="SMSButton_Click" />
                <asp:Label ID="ErrorLabel" runat="server" CssClass="EroorSummer"></asp:Label>
                <asp:SqlDataSource ID="SMS_OtherInfoSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" InsertCommand="INSERT INTO SMS_OtherInfo(SMS_Send_ID, SchoolID, EducationYearID, CommitteeMemberId) VALUES (@SMS_Send_ID, @SchoolID, @EducationYearID, @CommitteeMemberId)" SelectCommand="SELECT * FROM [SMS_OtherInfo]">
                    <InsertParameters>
                        <asp:Parameter Name="SMS_Send_ID" DbType="Guid" />
                        <asp:Parameter Name="SchoolID" />
                        <asp:Parameter Name="StudentID" />
                        <asp:Parameter Name="TeacherID" />
                        <asp:Parameter Name="EducationYearID" />
                        <asp:Parameter Name="CommitteeMemberId" />
                    </InsertParameters>
                </asp:SqlDataSource>
            </div>
        </div>
    </div>


    <script>
        function allCheckBox(objRef) {
            const GridView = objRef.parentNode.parentNode.parentNode;
            const inputList = GridView.getElementsByTagName("input");

            for (let i = 0; i < inputList.length; i++) {
                if (inputList[i].type == "checkbox" && objRef !== inputList[i]) {

                    if (objRef.checked) {
                        inputList[i].checked = true;
                    }
                    else {
                        inputList[i].checked = false;
                    }
                }
            }
        }
    </script>
</asp:Content>
