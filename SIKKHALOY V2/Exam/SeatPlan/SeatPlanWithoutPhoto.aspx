<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/BASIC.Master" CodeBehind="SeatPlanWithoutPhoto.aspx.cs" Inherits="EDUCATION.COM.Exam.SeatPlan.SeatPlanWithoutPhoto" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        #wrapper { display: grid; grid-gap: 45px 30px; grid-template-columns: repeat(3, 1fr); }
        #wrapper > div { position: relative; border: 2px solid #333; /*width: 323.52px; height: 204px;*/ }

        #grid_Header { margin-bottom: 8px; border-bottom: 2px solid #333; color: #000; text-align: center; display: grid; grid-template-columns: 40px 1fr; }
        #grid_Header img { height: 30px; border-radius: 3px; }
        .Hidden_Ins_Name { position: absolute; visibility: hidden; height: auto; width: auto; white-space: nowrap; }
        .Ins_Name { margin-top: 5px; font-weight: bold; }

        .exName { color: #000; text-align: center; font-weight: bold;}
        .iCard-title { border: 1px solid #333; margin: auto; border-radius: 3px; color: #000; font-size: 15px; padding: 3px 10px; text-align: center; width: 126px;font-weight:bold;border-radius: 10px; }

        #user-info { margin: 5px; display: grid; grid-template-columns: 90px 1fr 90px; align-items: center; }
        .user-img { 
            text-align: center;
            border: 1px solid #cec5c5;
            border-radius: 50px;
            vertical-align: middle;
            background: #f4ebeb;
            height: 80px;
            width: 80px;
            font-size: 14px;
            font-weight: bold;
            margin-bottom: 10px;
            padding-top: 10px;
            display: flex;
            flex-direction: column;
            justify-content: center;
            align-items: center;
        }
        .seat-number {
            text-align: center;
            border: 2px solid #d32f2f;
            border-radius: 50px;
            background: #ffebee;
            height: 80px;
            width: 80px;
            font-size: 14px;
            font-weight: bold;
            margin-bottom: 10px;
            padding-top: 10px;
            display: flex;
            flex-direction: column;
            justify-content: center;
            align-items: center;
            color: #d32f2f;
        }
        #user-info .middle-content {
            margin: 0;
            padding: 0 10px;
        }
        #user-info ul { 
            margin: 0;
            padding-left: 5px;
            list-style: none;
        }
        #user-info ul li { 
            list-style: none; 
            font-size: 13px; 
            color: #000; 
            font-weight: 500; 
        }

        @page { margin: 15px; }

        @media print {
            #header, h3 { display: none; }
        }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <h3>আসন বিন্যাস</h3>

    <div class="form-inline NoPrint">
        <div class="form-group">
            <asp:DropDownList ID="ExamDropDownList" runat="server" CssClass="form-control" onchange="showMe(this);" DataSourceID="ExamSQL" DataTextField="ExamName" DataValueField="ExamID" AppendDataBoundItems="True" AutoPostBack="True" OnSelectedIndexChanged="ExamDropDownList_SelectedIndexChanged">
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
            <input id="ExamNameTextBox" class="form-control" type="text" value="" placeholder="Exam Name"/>
        </div>
        <div class="form-group">
            <asp:DropDownList ID="ClassDropDownList" runat="server" AppendDataBoundItems="True" AutoPostBack="True" CssClass="form-control" DataSourceID="ClassNameSQL" DataTextField="Class" DataValueField="ClassID" OnSelectedIndexChanged="ClassDropDownList_SelectedIndexChanged">
                <asp:ListItem Value="0">[ শ্রেণি নির্বাচন করুন ]</asp:ListItem>
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
            <asp:TextBox ID="Find_ID_TextBox" runat="server" CssClass="form-control" placeholder="Separate the ID by comma"></asp:TextBox>
            <asp:RequiredFieldValidator ControlToValidate="Find_ID_TextBox" ValidationGroup="F" ID="RequiredFieldValidator1" CssClass="EroorStar" runat="server" ErrorMessage="*"></asp:RequiredFieldValidator>
        </div>
        <div class="form-group">
            <asp:Button ID="FindButton" ValidationGroup="F" runat="server" Text="Find ID" class="btn btn-primary" OnClick="FindButton_Click" />
        </div>
        <div class="form-group">
            <input onclick="window.print()" type="button" value="Print" class="btn btn-brown" />
        </div>
    </div>

    <div class="alert alert-info NoPrint">Print orientation landscape</div>

    <div id="wrapper">
        <asp:Repeater ID="IDCardRepeater" runat="server" Visible="False">
            <ItemTemplate>
                <div>
                    <div id="grid_Header">
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
                        </div>
                    </div>

                    <div class="exName">
                        <label class="ExamName"></label>
                    </div>

                    <div class="iCard-title">
                      আসন বিন্যাস
                    </div>

                    <div id="user-info">
                        <div class="user-img">
                            <div style="font-size: 12px;">ID</div>
                            <div><strong style="font-size: 16px;"><%#Eval("ID") %></strong></div>
                        </div>
                        <div class="middle-content">
                            <ul>
                                <li><%# Eval("StudentsName")%></li>
                                <li>শ্রেণি: <%# Eval("Class") %>, রোল: <%# Eval("RollNo") %></li>
                                <li><%# Eval("Section","সেকশন: {0}") %></li>
                                <li><%# Eval("Shift"," শিফট: {0}") %></li>
                                <li><%# Eval("SubjectGroup","গ্রুপ: {0}") %></li>
                            </ul>
                        </div>
                        <div class="seat-number">
                            <div style="font-size: 12px;">আসন নং</div>
                            <div><strong style="font-size: 16px;"><%# String.IsNullOrEmpty(Eval("SeatNo").ToString()) ? "-" : Eval("SeatNo") %></strong></div>
                        </div>
                    </div>
                </div>
            </ItemTemplate>
        </asp:Repeater>
    </div>

    <asp:SqlDataSource ID="ICardInfoSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT Student.StudentsName, Student.ID, CreateSection.Section, CreateClass.Class, SchoolInfo.SchoolName, StudentsClass.RollNo, StudentsClass.SeatNo, SchoolInfo.SchoolID, StudentsClass.StudentID, CreateShift.Shift, CreateSubjectGroup.SubjectGroup FROM StudentsClass INNER JOIN Student ON StudentsClass.StudentID = Student.StudentID INNER JOIN CreateClass ON StudentsClass.ClassID = CreateClass.ClassID INNER JOIN SchoolInfo ON StudentsClass.SchoolID = SchoolInfo.SchoolID LEFT OUTER JOIN CreateSubjectGroup ON StudentsClass.SubjectGroupID = CreateSubjectGroup.SubjectGroupID LEFT OUTER JOIN CreateShift ON StudentsClass.ShiftID = CreateShift.ShiftID LEFT OUTER JOIN CreateSection ON StudentsClass.SectionID = CreateSection.SectionID WHERE (StudentsClass.ClassID = @ClassID) AND (StudentsClass.SectionID LIKE @SectionID) AND (StudentsClass.SubjectGroupID LIKE @SubjectGroupID) AND (StudentsClass.EducationYearID = @EducationYearID) AND (StudentsClass.ShiftID LIKE @ShiftID) AND (Student.Status = @Status) ORDER BY CASE WHEN ISNUMERIC(StudentsClass.RollNo) = 1 THEN CAST(REPLACE(REPLACE(StudentsClass.RollNo , '$' , '') , ',' , '') AS FLOAT) ELSE 0 END">
        <SelectParameters>
            <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
            <asp:ControlParameter ControlID="SectionDropDownList" Name="SectionID" PropertyName="SelectedValue" />
            <asp:ControlParameter ControlID="GroupDropDownList" Name="SubjectGroupID" PropertyName="SelectedValue" />
            <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
            <asp:ControlParameter ControlID="ShiftDropDownList" Name="ShiftID" PropertyName="SelectedValue" />
            <asp:Parameter DefaultValue="Active" Name="Status" />
        </SelectParameters>
    </asp:SqlDataSource>
    <asp:SqlDataSource ID="IDsSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT Student.ID, CreateSection.Section, CreateClass.Class, SchoolInfo.SchoolName, StudentsClass.RollNo, StudentsClass.SeatNo, SchoolInfo.SchoolID, StudentsClass.StudentID, Student.StudentsName, CreateShift.Shift, CreateSubjectGroup.SubjectGroup FROM StudentsClass INNER JOIN Student ON StudentsClass.StudentID = Student.StudentID INNER JOIN CreateClass ON StudentsClass.ClassID = CreateClass.ClassID INNER JOIN SchoolInfo ON StudentsClass.SchoolID = SchoolInfo.SchoolID LEFT OUTER JOIN CreateSubjectGroup ON StudentsClass.SubjectGroupID = CreateSubjectGroup.SubjectGroupID LEFT OUTER JOIN CreateShift ON StudentsClass.ShiftID = CreateShift.ShiftID LEFT OUTER JOIN CreateSection ON StudentsClass.SectionID = CreateSection.SectionID WHERE (StudentsClass.EducationYearID = @EducationYearID) AND (Student.Status = @Status) AND (SchoolInfo.SchoolID = @SchoolID) AND (Student.ID IN (SELECT id FROM dbo.In_Function_Parameter(@IDs) AS In_Function_Parameter_1))">
        <SelectParameters>
            <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
            <asp:Parameter DefaultValue="Active" Name="Status" />
            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
            <asp:ControlParameter ControlID="Find_ID_TextBox" Name="IDs" PropertyName="Text" />
        </SelectParameters>
    </asp:SqlDataSource>


    <script>
        $('.ExamName').text($('[id*=ExamDropDownList] :selected').text());
        $('#ExamNameTextBox').val($('[id*=ExamDropDownList] :selected').text());
        $("#ExamNameTextBox").on('keyup', function () {
            $(".ExamName").text($(this).val());
        });


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

            

            /*$('#ExamNameTextBox').val($('[id*=ExamDropDownList] :selected').text());*/
        });
    </script>
    <script type="text/javascript"> 
        function showMe(a) {
            $('#ExamNameTextBox').val($('[id*=ExamDropDownList] :selected').text());
        }
    </script>
</asp:Content>
