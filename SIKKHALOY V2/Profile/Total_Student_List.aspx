<%@ Page Title="Total Student" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Total_Student_List.aspx.cs" Inherits="EDUCATION.COM.Profile.Total_Student_List" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .Bold-footer td { font-weight: bold; }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <h3>Total Student</h3>
    <asp:Repeater runat="server" DataSourceID="OldNewStudentSQL">
        <HeaderTemplate>
            <table id="totalMe" class="table table-sm">
                <thead>
                    <th><i class="fa fa-book" aria-hidden="true"></i>
                        Class</th>
                    <th><i class="fa fa-user-plus" aria-hidden="true"></i>
                        New Student</th>
                    <th><i class="fa fa-user" aria-hidden="true"></i>
                        Old Student</th>

                </thead>
                <tbody>
        </HeaderTemplate>
        <ItemTemplate>
            <tr>
                <th><%#Eval("Class") %></th>
                <td><%#Eval("New_Student") %></td>
                <td><%#Eval("Old_Student") %></td>
            </tr>
        </ItemTemplate>
        <FooterTemplate>
            </tbody>
                            </table>
        </FooterTemplate>
    </asp:Repeater>
    <asp:SqlDataSource ID="OldNewStudentSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT CreateClass.Class, SUM(CAST(StudentsClass.Is_New AS int)) AS New_Student, SUM(CAST(~ StudentsClass.Is_New AS int)) AS Old_Student FROM Student INNER JOIN StudentsClass ON Student.StudentID = StudentsClass.StudentID INNER JOIN CreateClass ON StudentsClass.ClassID = CreateClass.ClassID WHERE (Student.Status = N'Active') AND (StudentsClass.EducationYearID = @EducationYearID) AND (Student.SchoolID = @SchoolID) GROUP BY CreateClass.Class, StudentsClass.ClassID ORDER BY StudentsClass.ClassID">
        <SelectParameters>
            <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
        </SelectParameters>
    </asp:SqlDataSource>

    <input type="button" value="Print" class="btn btn-dark-green mt-2 d-print-none" onclick="window.print();" />


    <script src="/JS/jquery.tableTotal.js"></script>
    <script>
        $(function () {
            $('#totalMe').tableTotal();
        });
    </script>
</asp:Content>
