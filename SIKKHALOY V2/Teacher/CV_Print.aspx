<%@ Page Title="Print CV" Language="C#" MasterPageFile="~/Basic_Teacher.Master" AutoEventWireup="true" CodeBehind="CV_Print.aspx.cs" Inherits="EDUCATION.COM.Teacher.CV_Print" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .b-info { display: flex; margin-bottom: 20px; padding: 10px 15px; background-color: #444444; color: #E9ECED; }
        .ContactInfo h4 { margin: 7px 0 0 0; text-transform: uppercase; font-weight: 500; }
        .ContactInfo p { margin-bottom: 8px; font-style: italic; }
        .ContactInfo ul { margin: 0; padding: 0; }
        .ContactInfo ul li { list-style: none; font-size: 0.82rem; line-height: 22px; }

        .Objective { margin: 0 30px; font-size: 0.85rem; line-height: 21px; color: #333; }
        .info-wraper { margin: 15px 20px 0 20px; padding: 10px 10px 0 10px; }
        .info-wraper h4 { border-bottom: 1px solid #c8c8c8; font-weight: 400; font-size: 1.2rem; color: #747474; padding-bottom: 5px; }
        .info-wraper h5 { line-height: 10px; margin-bottom: 0; text-transform: uppercase; font-size: 13px; font-weight: 400; color: #333; margin-top: 15px; }
        .info-wraper span { font-style: italic; font-size: 0.8rem; }

        .Personal ul li { font-size: 0.9rem; line-height: 22px; }

        @page { margin: 0; }

        @media print {
            #header { display: none; }
        }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <asp:Repeater ID="BasicInfo_Repeater" runat="server" DataSourceID="BasicInfoSQL">
        <ItemTemplate>
            <div class="b-info">
                <div class="Photo">
                    <img class="img-thumbnail rounded-circle" style="height: 140px; width: 140px" alt="No Image" src="/Handeler/Teachers.ashx?Img=<%#Eval("TeacherID") %>" />
                </div>
                <div class="ContactInfo ml-4">
                    <h4><%# Eval("Name") %></h4>
                    <p><%# Eval("Designation") %></p>

                    <ul>
                        <li><i class="fa fa-phone-square" aria-hidden="true"></i>
                            <%# Eval("Phone") %></li>
                        <li><i class="fa fa-envelope" aria-hidden="true"></i>
                            <%# Eval("Email") %></li>
                    </ul>
                </div>
            </div>

            <div class="Objective">
                <%#Eval("CareerObjective") %>
            </div>
        </ItemTemplate>
    </asp:Repeater>
    <asp:SqlDataSource ID="BasicInfoSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT Teacher.FirstName + ' ' + Teacher.LastName AS Name, Teacher.Address, Teacher.Phone, Teacher.Email, Teacher.TeacherID, Teacher.Designation, Teacher_Career_Objective.CareerObjective FROM Teacher LEFT OUTER JOIN Teacher_Career_Objective ON Teacher.TeacherID = Teacher_Career_Objective.TeacherID WHERE (Teacher.SchoolID = @SchoolID) AND (Teacher.TeacherID = @TeacherID)">
        <SelectParameters>
            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" Type="Int32" />
            <asp:SessionParameter Name="TeacherID" SessionField="TeacherID" Type="Int32" />
        </SelectParameters>
    </asp:SqlDataSource>

    <div class="info-wraper">
        <%if (Experience_Repeater.Items.Count > 0)
            { %>
        <h4><i class="fa fa-briefcase" aria-hidden="true"></i>
            EXPERIENCE</h4>
        <%} %>
        <asp:Repeater ID="Experience_Repeater" runat="server" DataSourceID="ExperienceSQL">
            <ItemTemplate>
                <div class="Experience">
                    <div class="row no-gutters">
                        <div class="col">
                            <h5><%#Eval("InstitutionName") %></h5>
                            <span><%#Eval("Position") %></span>
                        </div>
                        <div class="col text-right">
                            <h5><%#Eval("JobYear") %></h5>
                        </div>
                    </div>



                </div>
            </ItemTemplate>
        </asp:Repeater>
        <asp:SqlDataSource ID="ExperienceSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT [InstitutionName], [Position], [Responsibilitie], [JobType], [JobStatus], [JobYear], [Address], [Phone], [Email] FROM [Teacher_JobInfo] WHERE (([SchoolID] = @SchoolID) AND ([TeacherID] = @TeacherID))">
            <SelectParameters>
                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" Type="Int32" />
                <asp:SessionParameter Name="TeacherID" SessionField="TeacherID" Type="Int32" />
            </SelectParameters>
        </asp:SqlDataSource>

        <h4 class="mt-4"><i class="fa fa-graduation-cap" aria-hidden="true"></i>
            EDUCATION</h4>
        <asp:Repeater ID="Education_Repeater" runat="server" DataSourceID="EducationSQL">
            <ItemTemplate>
                <div class="Education">
                    <div class="Experience">
                        <div class="row no-gutters">
                            <div class="col">
                                <h5><%#Eval("InstitutionName") %></h5>
                                <span>RESULT: <%#Eval("Result") %></span>
                            </div>
                            <div class="col text-right">
                                <h5><%#Eval("ExamName") %></h5>
                                <span><%#Eval("ExamYear") %></span>
                            </div>
                        </div>
                    </div>
                </div>
            </ItemTemplate>
        </asp:Repeater>
        <asp:SqlDataSource ID="EducationSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT InstitutionName, ExamName, ExamYear, Result FROM Teacher_EducationInfo WHERE (SchoolID = @SchoolID) AND (TeacherID = @TeacherID)">
            <SelectParameters>
                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" Type="Int32" />
                <asp:SessionParameter Name="TeacherID" SessionField="TeacherID" Type="Int32" />
            </SelectParameters>
        </asp:SqlDataSource>

        <%if (Skils_Repeater.Items.Count > 0)
            { %>
        <h4 class="mt-4"><i class="fa fa-puzzle-piece" aria-hidden="true"></i>
            SKILLS</h4>
        <%} %>
        <asp:Repeater ID="Skils_Repeater" runat="server" DataSourceID="SkilsSQL">
            <ItemTemplate>
                <div class="Skills">
                    <i class="d-block"><%#Eval("SkilName") %></i>
                </div>
            </ItemTemplate>
        </asp:Repeater>
        <asp:SqlDataSource ID="SkilsSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT SkilName FROM Teacher_Skill WHERE (TeacherID = @TeacherID) AND (SchoolID = @SchoolID)">
            <SelectParameters>
                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" Type="Int32" />
                <asp:SessionParameter Name="TeacherID" SessionField="TeacherID" Type="Int32" />
            </SelectParameters>
        </asp:SqlDataSource>


        <h4 class="mt-4"><i class="fa fa-flag" aria-hidden="true"></i>
            LANGUAGE</h4>
        <asp:Repeater ID="Language_Repeater" runat="server" DataSourceID="LanguageSQL">
            <ItemTemplate>
                <div class="Language">
                    <%#Eval("LanguageName") %> <i class="pull-right"><%#Eval("Level") %></i>
                </div>
            </ItemTemplate>
        </asp:Repeater>
        <asp:SqlDataSource ID="LanguageSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT LanguageName, Level FROM Teacher_Language WHERE (SchoolID = @SchoolID) AND (TeacherID = @TeacherID)">
            <SelectParameters>
                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" Type="Int32" />
                <asp:SessionParameter Name="TeacherID" SessionField="TeacherID" Type="Int32" />
            </SelectParameters>
        </asp:SqlDataSource>

        <h4 class="mt-4"><i class="fa fa-user" aria-hidden="true"></i>
            PERSONAL INFO</h4>
        <asp:Repeater ID="PersonalRepeater" runat="server" DataSourceID="PersonalSQL">
            <ItemTemplate>
                <div class="Personal">
                    <ul class="list-unstyled">
                        <li><b>Father's Name:</b> <%#Eval("FatherName") %></li>
                        <li><b>Mother's Name:</b> <%#Eval("MothersName") %></li>
                        <li><b>Date of Birth:</b> <%#Eval("DateofBirth") %></li>
                        <li><b>Religion:</b> <%#Eval("Religion") %></li>
                        <li><b>Present Address:</b> <%#Eval("Address") %></li>
                        <li><b>Permanent Address:</b> <%#Eval("PermanentAddress") %></li>
                    </ul>

                </div>
            </ItemTemplate>
        </asp:Repeater>
        <asp:SqlDataSource ID="PersonalSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT MothersName, FatherName,PermanentAddress, DateofBirth, Religion, Address FROM Teacher WHERE (SchoolID = @SchoolID) AND (TeacherID = @TeacherID)">
            <SelectParameters>
                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" Type="Int32" />
                <asp:SessionParameter Name="TeacherID" SessionField="TeacherID" Type="Int32" />
            </SelectParameters>
        </asp:SqlDataSource>
    </div>

    <script>
        $(function () {
            $("#_6").addClass("active");
        });
    </script>
</asp:Content>
