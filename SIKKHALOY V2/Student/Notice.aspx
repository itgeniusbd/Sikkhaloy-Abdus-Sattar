<%@ Page Title="Notice" Language="C#" MasterPageFile="~/Basic_Student.Master" AutoEventWireup="true" CodeBehind="Notice.aspx.cs" Inherits="EDUCATION.COM.Student.Notice" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <h3>Notice Board</h3>

    <div class="tab">
        <ul class="nav nav-tabs z-depth-1">
            <li><a class="nav-link active" href="#general-notice" data-toggle="tab" role="tab" aria-expanded="true">General Notice</a></li>
            <li><a class="nav-link" href="#home-work" data-toggle="tab" role="tab" aria-expanded="false">Home Work</a></li>
        </ul>

        <div class="tab-content card">
            <div id="general-notice" class="tab-pane fade in active show" role="tabpanel" aria-expanded="true">
                <asp:Repeater ID="NoticeRepeater" runat="server" DataSourceID="GeneralNoticeSQL">
                    <ItemTemplate>
                        <div class="mb-3">
                            <h4 class="font-weight-bold"><%# Eval("NoticeTitle") %></h4>
                            <p><%# Eval("Notice") %></p>

                          <div style="margin-bottom:20px;"><asp:LinkButton ID="lnkDownload" Text='<%# Eval("Notice_file").ToString() == "" ? "" : "Download" %>' CommandArgument='<%# Eval("Notice_file") %>' runat="server" OnClick="DownloadFile"></asp:LinkButton></div>

                            <small>Post Date: <%# Eval("InsertDate","{0:d MMM yyyy hh:mm tt}") %></small>
                        </div>
                    </ItemTemplate>
                </asp:Repeater>
                <asp:SqlDataSource ID="GeneralNoticeSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                    SelectCommand="SELECT StudentNotice.NoticeTitle, StudentNotice.Notice, StudentNotice.InsertDate,StudentNotice.Notice_file FROM StudentNoticeClass INNER JOIN StudentNotice ON StudentNoticeClass.StudentNoticeId = StudentNotice.StudentNoticeId INNER JOIN StudentsClass ON StudentNoticeClass.ClassId = StudentsClass.ClassID AND StudentNotice.EducationYearId = StudentsClass.EducationYearID WHERE (StudentNotice.EducationYearId = @EducationYearId) AND (StudentNotice.SchoolId = @SchoolId) AND (StudentsClass.StudentClassID = @StudentClassID) AND (IsHomeWork = 0) ORDER BY StudentNotice.InsertDate DESC">
                    <SelectParameters>
                        <asp:SessionParameter Name="EducationYearId" SessionField="Edu_Year" />
                        <asp:SessionParameter Name="SchoolId" SessionField="SchoolID" />
                        <asp:SessionParameter Name="StudentClassID" SessionField="StudentClassID" />
                    </SelectParameters>
                </asp:SqlDataSource>
            </div>

            <div id="home-work" class="tab-pane fade" role="tabpanel" aria-expanded="false">
                <asp:Repeater ID="HomeWorkRepeater" runat="server" DataSourceID="HomeWorkSQL">
                    <ItemTemplate>
                        <div class="mb-3">
                            <h4 class="font-weight-bold"><%# Eval("NoticeTitle") %></h4>
                            <p><%# Eval("Notice") %></p>
                            <small>Post Date: <%# Eval("InsertDate","{0:d MMM yyyy hh:mm tt}") %></small>
                        </div>
                    </ItemTemplate>
                </asp:Repeater>
                <asp:SqlDataSource ID="HomeWorkSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                    SelectCommand="SELECT StudentNotice.NoticeTitle, StudentNotice.Notice, StudentNotice.InsertDate FROM StudentNoticeClass INNER JOIN StudentNotice ON StudentNoticeClass.StudentNoticeId = StudentNotice.StudentNoticeId INNER JOIN StudentsClass ON StudentNoticeClass.ClassId = StudentsClass.ClassID AND StudentNotice.EducationYearId = StudentsClass.EducationYearID WHERE (StudentNotice.EducationYearId = @EducationYearId) AND (StudentNotice.SchoolId = @SchoolId) AND (StudentsClass.StudentClassID = @StudentClassID) AND (IsHomeWork = 1) ORDER BY StudentNotice.InsertDate DESC">
                    <SelectParameters>
                        <asp:SessionParameter Name="EducationYearId" SessionField="Edu_Year" />
                        <asp:SessionParameter Name="SchoolId" SessionField="SchoolID" />
                        <asp:SessionParameter Name="StudentClassID" SessionField="StudentClassID" />
                    </SelectParameters>
                </asp:SqlDataSource>
            </div>
        </div>
    </div>


    <script>
        $(function () {
            $("#_10").addClass("active");
        });
    </script>
</asp:Content>
