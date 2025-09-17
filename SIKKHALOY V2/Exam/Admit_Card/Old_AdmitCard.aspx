<%@ Page Title="Admit Card" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Old_AdmitCard.aspx.cs" Inherits="EDUCATION.COM.Exam.Admit_Card.Old_AdmitCard" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="css/Old_Admit.css?v=10" rel="stylesheet" />
    <style>
        .idcardborder {
    border: 2px solid #0075d2;
}
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <h3>Admit Card</h3>
    <a class="d-print-none" href="Multiple_Admit_Card.aspx"><< Back to Previous</a>

    <div class="form-inline NoPrint">
        <div class="form-group">
            <asp:DropDownList ID="ExamDropDownList" runat="server" CssClass="form-control" DataSourceID="ExamSQL" DataTextField="ExamName" DataValueField="ExamID" AppendDataBoundItems="True" AutoPostBack="True">
                <asp:ListItem Value="0">[ EXAM ]</asp:ListItem>
            </asp:DropDownList>
            <asp:SqlDataSource ID="ExamSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT ExamID, ExamName FROM Exam_Name WHERE (SchoolID = @SchoolID) AND (EducationYearID = @EducationYearID)">
                <SelectParameters>
                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                    <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                </SelectParameters>
            </asp:SqlDataSource>
        </div>

        <div class="form-group">
            <asp:DropDownList ID="ClassDropDownList" runat="server" AppendDataBoundItems="True" CssClass="form-control" DataSourceID="ClassNameSQL" DataTextField="Class" DataValueField="ClassID" AutoPostBack="True">
                <asp:ListItem Value="0">[ CLASS ]</asp:ListItem>
            </asp:DropDownList>
            <asp:SqlDataSource ID="ClassNameSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT * FROM [CreateClass] WHERE ([SchoolID] = @SchoolID) ORDER BY SN">
                <SelectParameters>
                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                </SelectParameters>
            </asp:SqlDataSource>
        </div>
        <%if (SectionDropDownList.Items.Count > 1)
            { %>
        <div class="form-group">
            <asp:DropDownList ID="SectionDropDownList" runat="server" AutoPostBack="True" CssClass="form-control" DataSourceID="SectionSQL" DataTextField="Section" DataValueField="SectionID" OnDataBound="SectionDropDownList_DataBound">
            </asp:DropDownList>
            <asp:SqlDataSource ID="SectionSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT DISTINCT [Join].SectionID, CreateSection.Section FROM [Join] INNER JOIN CreateSection ON [Join].SectionID = CreateSection.SectionID WHERE ([Join].ClassID = @ClassID)">
                <SelectParameters>
                    <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                </SelectParameters>
            </asp:SqlDataSource>
        </div>
        <%}%>
        <div class="form-group">
            <asp:DropDownList ID="Paid_DropDownList" CssClass="form-control" runat="server" AutoPostBack="True">
                <asp:ListItem Value="0">[ ALL STUDENT ]</asp:ListItem>
                <asp:ListItem>Paid</asp:ListItem>
                <asp:ListItem>Due</asp:ListItem>
            </asp:DropDownList>
        </div>
    </div>

    <div class="form-inline NoPrint Card-space">
        <div class="form-group">
            <asp:TextBox ID="TeacherSignTextBox" Text="Teacher's Signature" runat="server" placeholder="Teacher's Signature" CssClass="form-control" autocomplete="off" onDrop="blur();return false;" onpaste="return false"></asp:TextBox>
            <label class="btn btn-grey btn-file">
                Browse
            <input id="Tfileupload" type="file" style="display: none;" />
            </label>
        </div>
        <div class="form-group">
            <asp:TextBox ID="HeadTeacherSignTextBox" Text="Head Teacher Signature" runat="server" placeholder="Head Teacher Signature" CssClass="form-control" autocomplete="off" onDrop="blur();return false;" onpaste="return false"></asp:TextBox>
            <label class="btn btn-grey btn-file">
                Browse
            <input id="Hfileupload" type="file" style="display: none;" />
            </label>
        </div>
        <div class="form-group">
             <div class="dropdown">
     <button class="btn btn-primary " type="button" data-toggle="dropdown">
         Choose Color
 <span class="caret"></span>
     </button>
     <ul class="dropdown-menu">
         <li style="text-align: center"><a><b>Background Color</b></a></li>
         <li class="divider"></li>
         <asp:Table runat="server" CssClass="table">
             <asp:TableRow>
                 <asp:TableCell>Head</asp:TableCell>
                 <asp:TableCell><li><input type="color" class="getColor" /></li></asp:TableCell>
             </asp:TableRow>
         </asp:Table>
         <li class="divider"></li>
         <li style="text-align: center"><a><b>Font Color</b></a></li>
         <li class="divider"></li>
         <asp:Table runat="server" CssClass="table">
             <asp:TableRow>
                 <asp:TableCell>Head</asp:TableCell>
                 <asp:TableCell><li><input type="color" class="getfontColor" /></li></asp:TableCell>
             </asp:TableRow>
         </asp:Table>
     </ul>
 </div>
        </div>
        <div class="form-group">
            <button type="button" class="btn btn-primary hidden-print" onclick="window.print();">Print</button>
        </div>
    </div>

    <div id="wrapper">
        <asp:Repeater ID="IDCardrpt" runat="server" DataSourceID="ICardInfoSQL">
            <ItemTemplate>
                <div class="idcardborder">

                    <div class="card-header color-output">
                        <div class="sLogo">
                            <img src='/Handeler/SchoolLogo.ashx?SLogo=<%#Eval("SchoolID") %>' />
                        </div>
                        <div>
                            <h4><%# Eval("SchoolName") %></h4>
                            <p><%# Eval("Address") %></p>
                        </div>
                    </div>

                    <div class="Card_Title">ADMIT CARD</div>

                    <div class="student-info">
                        <div class="s-Photo">
                            <img src="/Handeler/Student_Id_Based_Photo.ashx?StudentID=<%#Eval("StudentID") %>" class="img-thumbnail rounded-circle" />
                            <strong>ID: <%# Eval("ID") %></strong>
                        </div>
                        <div class="Info">
                            <table>
                                <tr>
                                    <td><b>Exam</b></td>
                                    <td>:</td>
                                    <td>
                                        <label class="ExamName"></label>
                                    </td>
                                </tr>
                                <tr>
                                    <td><b>Session</b></td>
                                    <td>:</td>
                                    <td><%# Eval("EducationYear")%></td>
                                </tr>
                                <tr>
                                    <td><b>Name</b></td>
                                    <td>:</td>
                                    <td>
                                        <%# Eval("StudentsName")%>
                                    </td>
                                </tr>
                                <tr>
                                    <td><b>Class</b></td>
                                    <td>:</td>
                                    <td>
                                        <%# Eval("Class") %>
                                    </td>
                                </tr>
                                <tr>
                                    <td><b>Roll No</b></td>
                                    <td>:</td>
                                    <td>
                                        <%# Eval("RollNo") %>
                                    </td>
                                </tr>

                                <tr class="Group" style="display: none;">
                                    <td><b>Group</b></td>
                                    <td>:</td>
                                    <td><%# Eval("SubjectGroup") %></td>
                                </tr>
                                <tr class="Section" style="display: none;">
                                    <td><b>Section</b></td>
                                    <td>:</td>
                                    <td><%# Eval("Section") %></td>
                                </tr>
                                <tr class="Shift" style="display: none;">
                                    <td><b>Shift</b></td>
                                    <td>:</td>
                                    <td><%# Eval("Shift") %></td>
                                </tr>
                            </table>
                        </div>
                    </div>

                    <div class="Sign">
                        <div class="pull-left">
                            <div class="SignTeacher"></div>
                            <label class="Teacher">Teacher</label>
                        </div>
                        <div class="text-right pull-right">
                            <div class="SignHead"></div>
                            <label class="Head">Principal</label>
                        </div>
                        <div class="clearfix"></div>
                    </div>
                </div>
            </ItemTemplate>
        </asp:Repeater>
    </div>
    <asp:SqlDataSource ID="ICardInfoSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT Student.ID, SchoolInfo.SchoolID, StudentsClass.StudentID, SchoolInfo.SchoolName, Student.StudentsName, Student.FathersName, CreateClass.Class, CreateSection.Section, SchoolInfo.Address, CreateShift.Shift, StudentsClass.RollNo, Education_Year.EducationYear, CreateSubjectGroup.SubjectGroup FROM StudentsClass INNER JOIN Student ON StudentsClass.StudentID = Student.StudentID INNER JOIN CreateClass ON StudentsClass.ClassID = CreateClass.ClassID INNER JOIN SchoolInfo ON StudentsClass.SchoolID = SchoolInfo.SchoolID INNER JOIN Education_Year ON StudentsClass.EducationYearID = Education_Year.EducationYearID LEFT OUTER JOIN CreateSection ON StudentsClass.SectionID = CreateSection.SectionID LEFT OUTER JOIN CreateShift ON StudentsClass.ShiftID = CreateShift.ShiftID LEFT OUTER JOIN CreateSubjectGroup ON StudentsClass.SubjectGroupID = CreateSubjectGroup.SubjectGroupID WHERE (StudentsClass.EducationYearID = @EducationYearID) AND (Student.Status = 'Active') AND (StudentsClass.SchoolID = @SchoolID) AND (StudentsClass.ClassID = @ClassID) AND (StudentsClass.SectionID LIKE @SectionID)">
        <SelectParameters>
            <asp:SessionParameter DefaultValue="" Name="SchoolID" SessionField="SchoolID" />
            <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
            <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
            <asp:ControlParameter ControlID="SectionDropDownList" Name="SectionID" PropertyName="SelectedValue" />
        </SelectParameters>
    </asp:SqlDataSource>


   

   

    <script>

        

        



        $(function () {
            if ($('.Group td').eq(2).text() != "") {
                $('.Group').show();
            }
            if ($('.Section td').eq(2).text() != "") {
                $('.Section').show();
            }
            if ($('.Shift td').eq(2).text() != "") {
                $('.Shift').show();
            }

            $("#Tfileupload").change(function () {
                if (typeof (FileReader) != "undefined") {
                    var dvPreview = $(".SignTeacher");
                    dvPreview.html("");
                    var regex = /^([a-zA-Z0-9\s_\\.\-:])+(.jpg|.jpeg|.gif|.png|.bmp)$/;
                    $($(this)[0].files).each(function () {
                        var file = $(this);
                        if (regex.test(file[0].name.toLowerCase())) {
                            var reader = new FileReader();
                            reader.onload = function (e) {
                                var img = $("<img />");
                                img.attr("style", "height:30px;width:80px");
                                img.attr("src", e.target.result);
                                dvPreview.append(img);
                            }
                            reader.readAsDataURL(file[0]);
                        } else {
                            alert(file[0].name + " is not a valid image file.");
                            dvPreview.html("");
                            return false;
                        }
                    });
                } else {
                    alert("This browser does not support HTML5 FileReader.");
                }
            });

            $("#Hfileupload").change(function () {
                if (typeof (FileReader) != "undefined") {
                    var dvPreview = $(".SignHead");
                    dvPreview.html("");
                    var regex = /^([a-zA-Z0-9\s_\\.\-:])+(.jpg|.jpeg|.gif|.png|.bmp)$/;
                    $($(this)[0].files).each(function () {
                        var file = $(this);
                        if (regex.test(file[0].name.toLowerCase())) {
                            var reader = new FileReader();
                            reader.onload = function (e) {
                                var img = $("<img />");
                                img.attr("style", "height:30px;width:80px");
                                img.attr("src", e.target.result);
                                dvPreview.append(img);
                            }
                            reader.readAsDataURL(file[0]);
                        } else {
                            alert(file[0].name + " is not a valid image file.");
                            dvPreview.html("");
                            return false;
                        }
                    });
                } else {
                    alert("This browser does not support HTML5 FileReader.");
                }
            });

            $('.ExamName').text($('[id*=ExamDropDownList] :selected').text());

            $(".Teacher").text($("[id*=TeacherSignTextBox]").val());

            $("[id*=TeacherSignTextBox]").on('keyup', function () {
                $(".Teacher").text($("[id*=TeacherSignTextBox]").val());
            });

            $(".Head").text($("[id*=HeadTeacherSignTextBox]").val());

            $("[id*=HeadTeacherSignTextBox]").on('keyup', function () {
                $(".Head").text($("[id*=HeadTeacherSignTextBox]").val());
            });
        });
        // Background Color

        $(".getColor").on("change", function () {
            //Get Color
            var color = $(".getColor").val();
            //apply cuurent color to div
            $(".color-output").css("background", color);
            $(".idcardborder").css("border-color", color);
            $(".headcolor").css("background", color);
        })


        //  forcolor

        $(".getfontColor").on("change", function () {
            //Get Color
            var color = $(".getfontColor").val();
            //apply cuurent color to font
            $(".color-output").css("color", color);

        })
    </script>
</asp:Content>
