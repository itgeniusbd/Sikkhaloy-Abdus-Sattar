<%@ Page Title="ID Cards" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="All_ID_Cards.aspx.cs" Inherits="EDUCATION.COM.ID_CARDS.All_ID_Cards" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="CSS/IDCardMordern.css" rel="stylesheet" />
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <div class="row">
        <div class="col-sm-12">
            <h3>Students ID Cards  <a style="float: right" href="../ID_Cards/Student_ID_Cards.aspx">Student Custom ID Card</a> <a style="float: right" href="../ID_Cards/Card.aspx"> Gurdian Card ---</a> <a style="float: right" href="../ID_Cards/Student_Card.aspx"> Student ID Card ---</a></h3>
            
        </div>
    </div>
    

    <div class="form-inline NoPrint">
        <div class="form-group">
            <asp:DropDownList ID="ClassDropDownList" runat="server" AppendDataBoundItems="True" AutoPostBack="True" CssClass="form-control" DataSourceID="ClassNameSQL" DataTextField="Class" DataValueField="ClassID" OnSelectedIndexChanged="ClassDropDownList_SelectedIndexChanged">
                <asp:ListItem Value="0">[ SELECT CLASS ]</asp:ListItem>
            </asp:DropDownList>
            <asp:SqlDataSource ID="ClassNameSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT * FROM [CreateClass] WHERE ([SchoolID] = @SchoolID) ORDER BY SN">
                <SelectParameters>
                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                </SelectParameters>
            </asp:SqlDataSource>
        </div>
        <div class="form-group">
            <asp:DropDownList ID="GroupDropDownList" runat="server" AutoPostBack="True" CssClass="form-control" DataSourceID="GroupSQL" DataTextField="SubjectGroup" DataValueField="SubjectGroupID" OnDataBound="GroupDropDownList_DataBound" OnSelectedIndexChanged="GroupDropDownList_SelectedIndexChanged">
            </asp:DropDownList>
            <asp:SqlDataSource ID="GroupSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT DISTINCT [Join].SubjectGroupID, CreateSubjectGroup.SubjectGroup FROM [Join] INNER JOIN CreateSubjectGroup ON [Join].SubjectGroupID = CreateSubjectGroup.SubjectGroupID WHERE ([Join].ClassID = @ClassID) AND ([Join].SectionID LIKE N'%' + @SectionID + N'%') AND ([Join].ShiftID LIKE N'%' + @ShiftID + N'%')">
                <SelectParameters>
                    <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                    <asp:ControlParameter ControlID="SectionDropDownList" Name="SectionID" PropertyName="SelectedValue" />
                    <asp:ControlParameter ControlID="ShiftDropDownList" Name="ShiftID" PropertyName="SelectedValue" />
                </SelectParameters>
            </asp:SqlDataSource>
        </div>
        <div class="form-group">
            <asp:DropDownList ID="SectionDropDownList" runat="server" AutoPostBack="True" CssClass="form-control" DataSourceID="SectionSQL" DataTextField="Section" DataValueField="SectionID" OnDataBound="SectionDropDownList_DataBound" OnSelectedIndexChanged="SectionDropDownList_SelectedIndexChanged">
            </asp:DropDownList>
            <asp:SqlDataSource ID="SectionSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT DISTINCT [Join].SectionID, CreateSection.Section FROM [Join] INNER JOIN CreateSection ON [Join].SectionID = CreateSection.SectionID WHERE ([Join].ClassID = @ClassID) AND ([Join].SubjectGroupID LIKE N'%' + @SubjectGroupID + N'%') AND ([Join].ShiftID LIKE N'%' + @ShiftID + N'%')">
                <SelectParameters>
                    <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                    <asp:ControlParameter ControlID="GroupDropDownList" Name="SubjectGroupID" PropertyName="SelectedValue" />
                    <asp:ControlParameter ControlID="ShiftDropDownList" Name="ShiftID" PropertyName="SelectedValue" />
                </SelectParameters>
            </asp:SqlDataSource>
        </div>
        <div class="form-group">
            <asp:DropDownList ID="ShiftDropDownList" runat="server" AutoPostBack="True" CssClass="form-control" DataSourceID="ShiftSQL" DataTextField="Shift" DataValueField="ShiftID" OnDataBound="ShiftDropDownList_DataBound" OnSelectedIndexChanged="ShiftDropDownList_SelectedIndexChanged">
            </asp:DropDownList>
            <asp:SqlDataSource ID="ShiftSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT DISTINCT [Join].ShiftID, CreateShift.Shift FROM [Join] INNER JOIN CreateShift ON [Join].ShiftID = CreateShift.ShiftID WHERE ([Join].SubjectGroupID LIKE N'%' + @SubjectGroupID + N'%') AND ([Join].SectionID LIKE N'%' + @SectionID + N'%') AND ([Join].ClassID = @ClassID)">
                <SelectParameters>
                    <asp:ControlParameter ControlID="GroupDropDownList" Name="SubjectGroupID" PropertyName="SelectedValue" />
                    <asp:ControlParameter ControlID="SectionDropDownList" Name="SectionID" PropertyName="SelectedValue" />
                    <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                </SelectParameters>
            </asp:SqlDataSource>
        </div>
        <div class="form-group">
            <asp:TextBox ID="Find_ID_TextBox" runat="server" CssClass="form-control modern-input" placeholder="🔍 Search by Student ID (comma separated)"></asp:TextBox>
        </div>
        <div class="form-group">
            <asp:Button ID="FindButton" runat="server" Text="🔍 Find Students" CssClass="btn btn-search modern-btn" OnClick="FindButton_Click" />
        </div>
        <div class="form-group">
            <label class="btn btn-upload modern-btn" style="margin-bottom: 0;">
                📝 Upload Signature
                <input id="Hfileupload" type="file" style="display: none;" />
            </label>
        </div>
        <div class="form-group">
            <button type="button" onclick="window.print()" class="btn btn-print modern-btn">
                🖨️ Print Cards
            </button>
        </div>
        <div class="form-group">
            <input id="HeadlineText" type="text" placeholder="✏️ Customize Card Title" class="form-control modern-input" />
        </div>
        <div class="form-group">
            <div class="dropdown">
                <button class="btn btn-color modern-btn" type="button" data-toggle="dropdown">
                    🎨 Choose Colors
                    <span class="caret"></span>
                </button>
                <ul class="dropdown-menu modern-dropdown">
                    <li style="text-align: center" class="dropdown-header"><strong>🎨 Background Colors</strong></li>
                    <li class="divider"></li>
                    <asp:Table runat="server" CssClass="table color-table">
                        <asp:TableRow>
                            <asp:TableCell CssClass="color-label">Card Elements</asp:TableCell>
                            <asp:TableCell><li class="color-input-wrapper"><input type="color" class="getColor modern-color-picker" /></li></asp:TableCell>
                        </asp:TableRow>
                    </asp:Table>
                    <li class="divider"></li>
                    <li style="text-align: center" class="dropdown-header"><strong>✒️ Font Colors</strong></li>
                    <li class="divider"></li>
                    <asp:Table runat="server" CssClass="table color-table">
                        <asp:TableRow>
                            <asp:TableCell CssClass="color-label">Card Elements</asp:TableCell>
                            <asp:TableCell><li class="color-input-wrapper"><input type="color" class="getfontColor modern-color-picker" /></li></asp:TableCell>
                        </asp:TableRow>
                    </asp:Table>
                    <li class="divider"></li>
                    <li style="text-align: center">
                        <button type="button" class="btn btn-reset modern-btn" id="resetColorsBtn">🔄 Reset Colors</button>
                    </li>
                </ul>
            </div>
        </div>
    </div>

    <div class="alert alert-info NoPrint">print orientation landscape</div>


    <div id="wrapper">
        <asp:Repeater ID="IDCardRepeater" runat="server">
            <ItemTemplate>
                <div>
                    <div class="grid_Header">
                        <div style="padding: 5px 0;">
                            <img alt="No Logo" src="/Handeler/SchoolLogo.ashx?SLogo=<%#Eval("SchoolID") %>" />
                        </div>
                        <div>
                            <div class="Ins_Name">
                                <%# Eval("SchoolName") %>
                            </div>
                            <div class="Hidden_Ins_Name">
                                <%# Eval("SchoolName") %>
                            </div>

                            <div class="Institution_Dialog">
                                <asp:Label ID="Label1" CssClass="Instit_Dialog" runat="server" Text='<%# Eval("Institution_Dialog") %>' />
                            </div>
                        </div>
                    </div>
                    <div class="iCard-title"></div>

                    <div id="user-info">
                        <div style="text-align: center;">
                            <img src="/Handeler/Student_Id_Based_Photo.ashx?StudentID=<%#Eval("StudentID") %>" class="rounded-circle img-thumbnail" /><br />
                            <strong class="d-block"><%#Eval("ID") %></strong>
                        </div>
                        <div>
                            <ul>
                                <li class="c-user-name"> Name: <%# Eval("StudentsName")%> </li>
                                <li> Father's Name: <%# Eval("FathersName")%>  </li>
                                <li>Class: <%# Eval("Class") %>, Roll No: <%# Eval("RollNo") %></li>
                                <li>Phone: <%# Eval("SMSPhoneNo") %></li>
                                <li>Blood Group: <%# Eval("BloodGroup") %></li>
                                <li>D.O.B: <%# Eval("DateofBirth","{0:d MMM yyyy}") %></li>
                            </ul>
                        </div>
                    </div>

                    <div class="sign">Principal signature</div>
                    <div class="c-address">
                        <%# Eval("Address") %>
                    </div>
                </div>
            </ItemTemplate>
        </asp:Repeater>
    </div>

    <asp:SqlDataSource ID="ICardInfoSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT Student.StudentsName, Student.ID, Student.FathersName, CreateSection.Section, CreateClass.Class, SchoolInfo.SchoolName, SchoolInfo.Address, CreateShift.Shift, ISNULL(CreateSubjectGroup.SubjectGroup, N'No Group') AS SubjectGroup, StudentsClass.RollNo, StudentsClass.StudentID, SchoolInfo.SchoolID, Student.SMSPhoneNo, Student.BloodGroup, SchoolInfo.Institution_Dialog, Student.DateofBirth FROM StudentsClass INNER JOIN Student ON StudentsClass.StudentID = Student.StudentID INNER JOIN CreateClass ON StudentsClass.ClassID = CreateClass.ClassID INNER JOIN SchoolInfo ON StudentsClass.SchoolID = SchoolInfo.SchoolID LEFT OUTER JOIN CreateSection ON StudentsClass.SectionID = CreateSection.SectionID LEFT OUTER JOIN CreateShift ON StudentsClass.ShiftID = CreateShift.ShiftID LEFT OUTER JOIN CreateSubjectGroup ON StudentsClass.SubjectGroupID = CreateSubjectGroup.SubjectGroupID WHERE (StudentsClass.ClassID = @ClassID) AND (StudentsClass.SectionID LIKE @SectionID) AND (StudentsClass.SubjectGroupID LIKE @SubjectGroupID) AND (StudentsClass.EducationYearID = @EducationYearID) AND (StudentsClass.ShiftID LIKE @ShiftID) AND (Student.Status = @Status)">
        <SelectParameters>
            <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
            <asp:ControlParameter ControlID="SectionDropDownList" Name="SectionID" PropertyName="SelectedValue" />
            <asp:ControlParameter ControlID="GroupDropDownList" Name="SubjectGroupID" PropertyName="SelectedValue" />
            <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
            <asp:ControlParameter ControlID="ShiftDropDownList" Name="ShiftID" PropertyName="SelectedValue" />
            <asp:Parameter DefaultValue="Active" Name="Status" />
        </SelectParameters>
    </asp:SqlDataSource>
    <asp:SqlDataSource ID="IDsSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT Student.StudentsName, Student.ID, Student.FathersName, CreateSection.Section, CreateClass.Class, SchoolInfo.SchoolName, SchoolInfo.Address, CreateShift.Shift, ISNULL(CreateSubjectGroup.SubjectGroup, N'No Group') AS SubjectGroup, StudentsClass.RollNo, StudentsClass.StudentID, SchoolInfo.SchoolID, Student.SMSPhoneNo, Student.BloodGroup, SchoolInfo.Institution_Dialog, Student.DateofBirth FROM StudentsClass INNER JOIN Student ON StudentsClass.StudentID = Student.StudentID INNER JOIN CreateClass ON StudentsClass.ClassID = CreateClass.ClassID INNER JOIN SchoolInfo ON StudentsClass.SchoolID = SchoolInfo.SchoolID LEFT OUTER JOIN CreateSection ON StudentsClass.SectionID = CreateSection.SectionID LEFT OUTER JOIN CreateShift ON StudentsClass.ShiftID = CreateShift.ShiftID LEFT OUTER JOIN CreateSubjectGroup ON StudentsClass.SubjectGroupID = CreateSubjectGroup.SubjectGroupID WHERE (StudentsClass.EducationYearID = @EducationYearID) AND (Student.Status = @Status) AND (SchoolInfo.SchoolID = @SchoolID) AND (Student.ID IN(SELECT  id from [dbo].[In_Function_Parameter] (@IDs)))">
        <SelectParameters>
            <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
            <asp:Parameter DefaultValue="Active" Name="Status" />
            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
            <asp:ControlParameter ControlID="Find_ID_TextBox" Name="IDs" PropertyName="Text" />
        </SelectParameters>
    </asp:SqlDataSource>


    <script>
        $(function () {
            var Default_fontSize = 13;
            var Max_fontSize = 20;

            var test = document.getElementsByClassName("Hidden_Ins_Name")[0];
            var Show = document.getElementsByClassName("Ins_Name")[0];

            var New_fontSize = Math.round(((Default_fontSize * parseFloat(Show.clientWidth)) / parseFloat(test.clientWidth)));
            if (New_fontSize > Max_fontSize) {
                New_fontSize = Max_fontSize;
            }
            var width = (test.clientWidth) + "px";

            $('.Ins_Name').css('font-size', New_fontSize);


            if (!$('.Instit_Dialog').text()) {
                $('.Institution_Dialog').hide();
                $('.Hidden_Ins_Name').hide();
            }

            // Force apply button styles after page load
            setTimeout(function() {
                // Apply modern button styles
                $('.btn-search').css({
                    'background': 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
                    'color': 'white',
                    'border': 'none'
                });
                
                $('.btn-upload').css({
                    'background': 'linear-gradient(135deg, #f093fb 0%, #f5576c 100%)',
                    'color': 'white',
                    'border': 'none'
                });
                
                $('.btn-print').css({
                    'background': 'linear-gradient(135deg, #4facfe 0%, #00f2fe 100%)',
                    'color': 'white',
                    'border': 'none'
                });
                
                $('.btn-color').css({
                    'background': 'linear-gradient(135deg, #fa709a 0%, #fee140 100%)',
                    'color': 'white',
                    'border': 'none'
                });
                
                $('.btn-reset').css({
                    'background': 'linear-gradient(135deg, #ffecd2 0%, #fcb69f 100%)',
                    'color': '#8b5cf6',
                    'border': '1px solid #f59e0b'
                });
            }, 100);

            // Sign upload
            $("#Hfileupload").change(function () {
                if (typeof (FileReader) != "undefined") {
                    var dvPreview = $(".sign");
                    dvPreview.html("");
                    var regex = /^([a-zA-Z0-9\s_\\.\-:])+(.jpg|.jpeg|.gif|.png|.bmp)$/;
                    $($(this)[0].files).each(function () {
                        var file = $(this);
                        if (regex.test(file[0].name.toLowerCase())) {
                            var reader = new FileReader();
                            reader.onload = function (e) {
                                var img = $("<img />");
                                img.attr("style", "height: 24px;width: 75px;position: absolute;right: 0;bottom: 15px;");
                                img.attr("src", e.target.result);
                                dvPreview.append(img);
                                dvPreview.append("Principal signature");
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

            //save headline
            $("#HeadlineText").on("keyup", function () {
                $(".iCard-title").text($(this).val());
                localStorage.Headline = $(this).val();
            });

            //read headline
            if (localStorage.Headline) {
                $(".iCard-title").text(localStorage.Headline);
            }
            else {
                $(".iCard-title").text("Student ID Card");
            }

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
                                if (node.nodeType === 1 && (node.classList.contains('grid_Header') || node.classList.contains('iCard-title'))) {
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
            
            // Save to localStorage (using same ID Card key system)
            try {
                localStorage.setItem('idCard_bgColor_' + window.userColorKey, color);
            } catch(e) {
                console.log('LocalStorage not available');
            }
            
            //apply current color to elements - Fixed selectors
            $("#wrapper .grid_Header").css("background-color", color);
            $("#wrapper .iCard-title").css("background-color", color);
            $("#wrapper .c-address").css("background-color", color);
            $("#wrapper > div").css("border-color", color);
            
            // Save color to session
            $.ajax({
                url: "All_ID_Cards.aspx/SaveBackgroundColor",
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

        // Font Color
        $(document).on("change", ".getfontColor", function () {
            //Get Color
            var color = $(".getfontColor").val();
            
            // Update window variable
            window.savedFontColor = color;
            
            // Save to localStorage (using same ID Card key system)
            try {
                localStorage.setItem('idCard_fontColor_' + window.userColorKey, color);
            } catch(e) {
                console.log('LocalStorage not available');
            }
            
            //apply current color to font elements - Fixed selectors
            $("#wrapper .grid_Header").css("color", color);
            $("#wrapper .iCard-title").css("color", color);
            $("#wrapper .c-address").css("color", color);
            
            // Save color to session
            $.ajax({
                url: "All_ID_Cards.aspx/SaveFontColor",
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
            
            // Update window variables to default
            window.savedBgColor = "#0075d2";
            window.savedFontColor = "#ffffff";
            
            // Clear localStorage (using same ID Card key system)
            try {
                localStorage.removeItem('idCard_bgColor_' + window.userColorKey);
                localStorage.removeItem('idCard_fontColor_' + window.userColorKey);
            } catch(e) {
                console.log('LocalStorage not available');
            }
            
            // Reset color inputs to default
            $(".getColor").val("#0075d2");
            $(".getfontColor").val("#ffffff");
            
            // Apply default colors - Fixed selectors
            $("#wrapper .grid_Header").css("background-color", "#0075d2");
            $("#wrapper .iCard-title").css("background-color", "#0075d2");
            $("#wrapper .c-address").css("background-color", "#0075d2");
            $("#wrapper > div").css("border-color", "#0075d2");
            $("#wrapper .grid_Header").css("color", "#ffffff");
            $("#wrapper .iCard-title").css("color", "#ffffff");
            $("#wrapper .c-address").css("color", "#ffffff");
            
            // Clear session colors
            $.ajax({
                url: "All_ID_Cards.aspx/ResetColors",
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
