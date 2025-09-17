<%@ Page Title="Student Notice" Language="C#" MasterPageFile="~/Basic_Teacher.Master" AutoEventWireup="true" CodeBehind="StudentNotice.aspx.cs" Inherits="EDUCATION.COM.Teacher.StudentNotice" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <div class="col-xl-10 mx-auto">
        <div class="card card-body">
            <h4 class="font-weight-bold mb-3">Class Based Notice</h4>

            <asp:CheckBoxList ID="ClassCheckBoxList" CssClass="DefaultCheckBoxList" runat="server" DataSourceID="ClassNameSQL" DataTextField="Class" DataValueField="ClassID" RepeatDirection="Horizontal" RepeatLayout="Flow">
            </asp:CheckBoxList>
            <asp:SqlDataSource ID="ClassNameSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT * FROM [CreateClass] WHERE ([SchoolID] = @SchoolID) ORDER BY SN">
                <SelectParameters>
                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                </SelectParameters>
            </asp:SqlDataSource>

            <div class="form-group mt-3">
                <label>Notice Type</label>
                <asp:RadioButtonList ID="NoticeTypeRadioButton" runat="server" RepeatDirection="Horizontal">
                    <asp:ListItem Value="0" Selected="True">General Notice</asp:ListItem>
                    <asp:ListItem Value="1">Home Work</asp:ListItem>
                </asp:RadioButtonList>
            </div>

            <div class="form-group mt-3">
                <label>Notice Title</label>
                <asp:TextBox ID="NoticeTitleTextBox" runat="server" CssClass="form-control" placeholder="Notice Title"></asp:TextBox>
            </div>

            <div class="form-group">
                <label>Notice</label>
                <asp:TextBox ID="NoticeTextBox" runat="server" CssClass="form-control" Height="90px" TextMode="MultiLine" placeholder="Notice Text Write here"></asp:TextBox>
            </div>
            <div class="form-group">
                <label>Upload File </label>
                <asp:FileUpload ID="FileUploadPDF" runat="server" />
            </div>
            <div class="mb-3">
                <%--<asp:Button ID="NoticeButton" OnClick="NoticeButton_Click" runat="server" Text="Submit" ValidationGroup="1" CssClass="btn btn-primary ml-0 mt-3" />--%>

                <div class="row">
                    <div class="col-sm-6">
                        <asp:Button ID="NoticeButton" OnClick="NoticeButton_Click" runat="server" Text="Submit" CssClass="btn btn-primary ml-0 mt-3" />
                    </div>
                    <div class="col-sm-6" style="text-align: right">
                        <asp:Button OnClick="DeleteButton_Click" runat="server" Text="Delete Notice" CssClass="btn btn-primary ml-0 mt-3" />
                    </div>
                </div>

                <asp:SqlDataSource ID="StudentNoticeSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                    InsertCommand="INSERT INTO [StudentNotice] ([RegistrationId], [SchoolId], [EducationYearId], [NoticeTitle], [Notice],[Notice_file], IsHomeWork ) 
                 VALUES (@RegistrationId, @SchoolId, @EducationYearId, @NoticeTitle, @Notice,@Notice_file,@IsHomeWork);
                 SELECT @StudentNoticeId = SCOPE_IDENTITY();"
                    SelectCommand="SELECT StudentNoticeId, NoticeTitle, Notice,Notice_file,EducationYearId FROM StudentNotice WHERE (SchoolId = @SchoolId) AND (RegistrationId = @RegistrationId) AND (EducationYearId = @EducationYearId) ORDER BY InsertDate DESC;"
                    
                    OnInserted="StudentNoticeSQL_Inserted">
                    
                    <InsertParameters>
                        <asp:SessionParameter Name="RegistrationId" SessionField="RegistrationID" Type="Int32" />
                        <asp:SessionParameter Name="SchoolId" SessionField="SchoolID" Type="Int32" />
                        <asp:SessionParameter Name="EducationYearId" SessionField="Edu_Year" Type="Int32" />
                        <asp:ControlParameter ControlID="NoticeTitleTextBox" Name="NoticeTitle" PropertyName="Text" Type="String" />
                        <asp:ControlParameter ControlID="NoticeTextBox" Name="Notice" PropertyName="Text" Type="String" />
                        <asp:Parameter Name="Notice_file" Type="String" />
                        <%--<asp:ControlParameter ControlID="FileUploadPDF" Name="FileUploadPDF" PropertyName="FileUploadPDF" Type="String" />--%>
                        <asp:ControlParameter ControlID="NoticeTypeRadioButton" Name="IsHomeWork" PropertyName="SelectedValue" />
                        <asp:Parameter Name="StudentNoticeId" Direction="Output" Size="50" />
                    </InsertParameters>
                    <SelectParameters>
                        <asp:SessionParameter Name="SchoolId" SessionField="SchoolId" />
                        <asp:SessionParameter Name="RegistrationId" SessionField="RegistrationID" />
                        <asp:SessionParameter Name="EducationYearId" SessionField="Edu_Year" />
                    </SelectParameters>
                </asp:SqlDataSource>

                <asp:SqlDataSource ID="StudentNoticeClassSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                    InsertCommand="INSERT INTO [StudentNoticeClass] ([StudentNoticeId], [ClassId]) VALUES (@StudentNoticeId, @ClassId)"
                    SelectCommand="SELECT * FROM [StudentNoticeClass]">
                    <InsertParameters>
                        <asp:Parameter Name="StudentNoticeId" Type="Int32" />
                        <asp:Parameter Name="ClassId" Type="Int32" />
                    </InsertParameters>
                </asp:SqlDataSource>
            </div>

            <asp:GridView ID="NoticeGridView" runat="server" CssClass="mGrid" AutoGenerateColumns="False" DataKeyNames="StudentNoticeId" DataSourceID="StudentNoticeSQL">
                <Columns>
                    <asp:BoundField DataField="NoticeTitle" HeaderText="Notice Title" SortExpression="NoticeTitle" />
                    <asp:BoundField DataField="Notice" HeaderText="Notice" SortExpression="Notice" />
                    <asp:TemplateField HeaderText="Classes">
                        <ItemTemplate>
                            <asp:HiddenField ID="IdHiddenField" runat="server" Value='<%# Eval("StudentNoticeId") %>' />
                            <asp:Repeater ID="ClassRepeater" runat="server" DataSourceID="ClassSQL">
                                <ItemTemplate>
                                    <span class="badge badge-pill badge-primary"><%# Eval("Class") %></span>
                                </ItemTemplate>
                            </asp:Repeater>
                            <asp:SqlDataSource ID="ClassSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT CreateClass.Class FROM StudentNoticeClass INNER JOIN CreateClass ON StudentNoticeClass.ClassId = CreateClass.ClassID WHERE (StudentNoticeClass.StudentNoticeId = @StudentNoticeId) ORDER BY CreateClass.SN">
                                <SelectParameters>
                                    <asp:ControlParameter ControlID="IdHiddenField" Name="StudentNoticeId" PropertyName="Value" />
                                </SelectParameters>
                            </asp:SqlDataSource>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Download" HeaderStyle-Width="7%">
                        <ItemTemplate>
                            <asp:LinkButton ID="lnkDownload" Text='<%# Eval("Notice_file").ToString() == "" ? "" : "Notice" %>' CommandArgument='<%# Eval("Notice_file") %>' runat="server" OnClick="DownloadFile"></asp:LinkButton>





                        </ItemTemplate>
                    </asp:TemplateField>

                    <%--<asp:CommandField HeaderText="Delete" ShowDeleteButton="True" />--%>
                    <asp:TemplateField>
                        <HeaderTemplate>
                            <asp:CheckBox ID="AllIteamCheckBox" runat="server" Text="All" />
                        </HeaderTemplate>
                        <ItemTemplate>
                            <asp:CheckBox ID="SingleCheckBox" runat="server" Text=" " />
                        </ItemTemplate>
                        <ItemStyle Width="50px" />
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>
        </div>
    </div>


    <script>
        $(document).ready(function () {
            $("[id*=AllIteamCheckBox]").on("click", function () {
                var a = $(this), b = $(this).closest("table");
                $("input[type=checkbox]", b).each(function () {
                    a.is(":checked") ? ($(this).attr("checked", "checked"), $("td", $(this).closest("tr")).addClass("selected")) : ($(this).removeAttr("checked"), $("td", $(this).closest("tr")).removeClass("selected"));
                });
            });

            $("[id*=SingleCheckBox]").on("click", function () {
                var a = $(this).closest("table"), b = $("[id*=AllIteamCheckBox]", a);
                $(this).is(":checked") ? ($("td", $(this).closest("tr")).addClass("selected"), $("[id*=SingleCheckBox]", a).length == $("[id*=SingleCheckBox]:checked", a).length && b.attr("checked", "checked")) : ($("td", $(this).closest("tr")).removeClass("selected"), b.removeAttr("checked"));
            });
        });
        $(function () {
            $("#_7").addClass("active");
        });
    </script>
</asp:Content>
