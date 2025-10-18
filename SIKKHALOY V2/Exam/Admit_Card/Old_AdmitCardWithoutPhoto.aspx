<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/BASIC.Master" CodeBehind="Old_AdmitCardWithoutPhoto.aspx.cs" Inherits="EDUCATION.COM.Exam.Admit_Card.Old_AdmitCardWithoutPhoto" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="css/Old_Admit.css?v=10" rel="stylesheet" />
    <style>
        .idcardborder {
    border: 2px solid #0075d2;
}
.user-img {
  text-align: center;
  border: 1px solid #cec5c5;
  border-radius: 68px;
  vertical-align: middle;
  background: #f4ebeb;
  height: 135px;
  width: 135px;
  font-size: 24px;
  font-weight: bold;
  margin-bottom: 10px;
  padding-top: 46px;
  margin-left: 15px;
}
.Card_Title {
  margin-top: 10px;
  font-size: 17pt;
  font-weight: bold;
  text-align: center;
  padding-bottom: 3px;
}
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <h3>Admit Card</h3>
    <a class="d-print-none" href="Multiple_Admit_Card.aspx"><< Back to Previous</a>

    <div class="form-inline NoPrint">
        <div class="form-group">
            <asp:DropDownList ID="ExamDropDownList" runat="server" CssClass="form-control" DataSourceID="ExamSQL" DataTextField="ExamName" DataValueField="ExamID" AppendDataBoundItems="True" AutoPostBack="True">
                <asp:ListItem Value="0">[ পরীক্ষা নির্বাচন করুন ]</asp:ListItem>
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
                <asp:ListItem Value="0">[ শ্রেণি নির্বাচন করুন ]</asp:ListItem>
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
                <asp:ListItem Value="0">[ সকল শিক্ষার্থী ]</asp:ListItem>
                <asp:ListItem>যারা টাকা পরিশোধ করেছেন</asp:ListItem>
                <asp:ListItem>যাদের বকেয়া আছে </asp:ListItem>
            </asp:DropDownList>
        </div>
    </div>

    <div class="form-inline NoPrint Card-space">
        <div class="form-group">
            <asp:TextBox ID="TeacherSignTextBox" Text="পরীক্ষা নিয়ন্ত্রক" runat="server" placeholder="পরীক্ষা নিয়ন্ত্রক" CssClass="form-control" autocomplete="off" onDrop="blur();return false;" onpaste="return false"></asp:TextBox>
            <label class="btn btn-grey btn-file">
                স্বাক্ষর যুক্ত করুন
            <input id="Tfileupload" type="file" style="display: none;" />
            </label>
        </div>
        <div class="form-group">
            <asp:TextBox ID="HeadTeacherSignTextBox" Text="প্রিন্সিপাল" runat="server" placeholder="প্রিন্সিপাল" CssClass="form-control" autocomplete="off" onDrop="blur();return false;" onpaste="return false"></asp:TextBox>
            <label class="btn btn-grey btn-file">
               স্বাক্ষর যুক্ত করুন
            <input id="Hfileupload" type="file" style="display: none;" />
            </label>
        </div>
        <div class="form-group">
             <div class="dropdown">
     <button class="btn btn-primary " type="button" data-toggle="dropdown">
        কালার নির্বাচন করুন
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
         <li class="divider"></li>
         <li style="text-align: center">
             <button type="button" class="btn btn-sm btn-warning" id="resetColorsBtn">Reset Colors</button>
         </li>
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

                    <div class="Card_Title">
                        <img src="../../CSS/Image/admintpic1.png" /></div>

                    <div class="student-info">
                        <div class="user-img">
                           <%-- <img src="/Handeler/Student_Id_Based_Photo.ashx?StudentID=<%#Eval("StudentID") %>" class="img-thumbnail rounded-circle" />--%>
                            <strong>ID: <%# Eval("ID") %></strong>
                        </div>
                        <div class="Info">
                            <table>
                                <tr>
                                    <td><b>পরীক্ষার নাম</b></td>
                                    <td>:</td>
                                    <td>
                                        <label class="ExamName"></label>
                                    </td>
                                     
                                </tr>
                                <tr>
                                    <td><b>শিক্ষার্থীর নাম</b></td>
                                    <td>:</td>
                                    <td>
                                        <%# Eval("StudentsName")%>
                                    </td>
                                </tr>

                                <tr>
                                    <td><b>শ্রেণি</b></td>
                                    <td>:</td>
                                    <td>
                                        <%# Eval("Class") %>
                                    </td>
                                </tr>
                                <tr>
                                    <td><b>রোল</b></td>
                                    <td>:</td>
                                    <td>
                                        <%# Eval("RollNo") %>
                                    </td>
                                </tr>

                                <tr class="Group" style="display: none;">
                                    <td><b>গ্রুপ</b></td>
                                    <td>:</td>
                                    <td><%# Eval("SubjectGroup") %></td>
                                </tr>
                                <tr class="Section" style="display: none;">
                                    <td><b>সেকশন</b></td>
                                    <td>:</td>
                                    <td><%# Eval("Section") %></td>
                                </tr>
                                <tr class="Shift" style="display: none;">
                                    <td><b>শিফট</b></td>
                                    <td>:</td>
                                    <td><%# Eval("Shift") %></td>
                                </tr>
                                      <tr>
                                    <td><b>শিক্ষাবর্ষ</b></td>
                                    <td>:</td>
                                    <td><%# Eval("EducationYear")%></td>
                                </tr>
                               
                            </table>
                        </div>
                    </div>

                    <div class="Sign">
                        <div class="pull-left">
                            <div class="SignTeacher"></div>
                            <label class="Teacher">পরীক্ষা নিয়ত্রক</label>
                        </div>
                        <div class="text-right pull-right">
                            <div class="SignHead"></div>
                            <label class="Head">প্রিন্সিপাল</label>
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

            // Apply saved colors after all initialization
            setTimeout(function() {
                if (typeof applySavedColors === 'function') {
                    applySavedColors();
                }
            }, 300);
            
            // Set up MutationObserver to reapply colors when DOM changes
            if (typeof MutationObserver !== 'undefined') {
                var observer = new MutationObserver(function(mutations) {
                    var shouldReapply = false;
                    mutations.forEach(function(mutation) {
                        if (mutation.type === 'childList' && mutation.addedNodes.length > 0) {
                            for (var i = 0; i < mutation.addedNodes.length; i++) {
                                var node = mutation.addedNodes[i];
                                if (node.nodeType === 1 && (node.classList.contains('color-output') || node.classList.contains('idcardborder'))) {
                                    shouldReapply = true;
                                    break;
                                }
                            }
                        }
                    });
                    
                    if (shouldReapply) {
                        setTimeout(function() {
                            if (typeof applySavedColors === 'function') {
                                applySavedColors();
                            }
                        }, 100);
                    }
                });
                
                observer.observe(document.getElementById('wrapper'), {
                    childList: true,
                    subtree: true
                });
            }
        });

        // Background Color
        $(document).on("change", ".getColor", function () {
            //Get Color
            var color = $(".getColor").val();
            
            // Update window variable
            window.savedBgColor = color;
            
            // Save to localStorage
            try {
                localStorage.setItem('admitCard_bgColor_' + window.userColorKey, color);
            } catch(e) {
                console.log('LocalStorage not available');
            }
            
            //apply current color to div
            $(".color-output").css("background", color);
            $(".idcardborder").css("border-color", color);
            $(".headcolor").css("background", color);
            
            // Save color to session
            $.ajax({
                url: "Old_AdmitCardWithoutPhoto.aspx/SaveBackgroundColor",
                data: JSON.stringify({ 'color': color }),
                dataType: "json",
                type: "POST",
                contentType: "application/json; charset=utf-8",
                success: function (response) {
                    console.log("Background color saved: " + color);
                },
                error: function (xhr) {
                    var err = JSON.parse(xhr.responseText);
                    console.log("Error saving background color: " + err.message);
                }
            });
        });

        //  Font Color
        $(document).on("change", ".getfontColor", function () {
            //Get Color
            var color = $(".getfontColor").val();
            
            // Update window variable
            window.savedFontColor = color;
            
            // Save to localStorage
            try {
                localStorage.setItem('admitCard_fontColor_' + window.userColorKey, color);
            } catch(e) {
                console.log('LocalStorage not available');
            }
            
            //apply current color to font
            $(".color-output").css("color", color);
            
            // Save font color to session
            $.ajax({
                url: "Old_AdmitCardWithoutPhoto.aspx/SaveFontColor",
                data: JSON.stringify({ 'color': color }),
                dataType: "json",
                type: "POST",
                contentType: "application/json; charset=utf-8",
                success: function (response) {
                    console.log("Font color saved: " + color);
                },
                error: function (xhr) {
                    var err = JSON.parse(xhr.responseText);
                    console.log("Error saving font color: " + err.message);
                }
            });
        });

        // Reset Colors functionality
        $(document).on("click", "#resetColorsBtn", function (e) {
            e.preventDefault();
            
            // Update window variables
            window.savedBgColor = "#0075d2";
            window.savedFontColor = "#ffffff";
            
            // Clear localStorage
            try {
                localStorage.removeItem('admitCard_bgColor_' + window.userColorKey);
                localStorage.removeItem('admitCard_fontColor_' + window.userColorKey);
            } catch(e) {
                console.log('LocalStorage not available');
            }
            
            // Reset colors to default
            $(".getColor").val("#0075d2");
            $(".getfontColor").val("#ffffff");
            
            // Apply default colors
            $(".color-output").css("background", "#0075d2");
            $(".idcardborder").css("border-color", "#0075d2");
            $(".headcolor").css("background", "#0075d2");
            $(".color-output").css("color", "#ffffff");
            
            // Clear session colors
            $.ajax({
                url: "Old_AdmitCardWithoutPhoto.aspx/ResetColors",
                data: JSON.stringify({}),
                dataType: "json",
                type: "POST",
                contentType: "application/json; charset=utf-8",
                success: function (response) {
                    console.log("Colors reset successfully");
                    alert("Colors have been reset to default!");
                },
                error: function (xhr) {
                    var err = JSON.parse(xhr.responseText);
                    console.log("Error resetting colors: " + err.message);
                }
            });
        });
    </script>
</asp:Content>