<%@ Page Title="Exam Control" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Exam_Publish_Settings.aspx.cs" Inherits="EDUCATION.COM.Exam.ExamSetting.Exam_Publish_Settings" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <h3>Exam Control</h3>

    <ul class="nav nav-tabs z-depth-1">
        <li class="nav-item">
            <a class="nav-link active" data-toggle="tab" href="#panel1" role="tab" aria-expanded="true">Individual Exam</a>
        </li>
        <li class="nav-item">
            <a class="nav-link" data-toggle="tab" href="#panel2" role="tab" aria-expanded="false">Cumulative Exam</a>
        </li>
    </ul>
    <div class="tab-content card">
        <div class="tab-pane fade in active show" id="panel1" role="tabpanel" aria-expanded="true">
            <asp:UpdatePanel ID="ContainUpdatePanel" runat="server">
                <ContentTemplate>
                    <label>Individual Exam</label>
                    <div class=" form-inline d-print-none">
                        <div class="form-group">
                            <asp:DropDownList ID="ExamDropDownList" runat="server" AutoPostBack="True" CssClass="form-control" DataSourceID="ExamSQL" DataTextField="ExamName" DataValueField="ExamID" AppendDataBoundItems="True">
                                <asp:ListItem Value="0">[ SELECT EXAM ]</asp:ListItem>
                            </asp:DropDownList>
                            <asp:SqlDataSource ID="ExamSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT DISTINCT Exam_Name.ExamID, Exam_Name.ExamName FROM Exam_Publish_Setting INNER JOIN Exam_Name ON Exam_Publish_Setting.ExamID = Exam_Name.ExamID WHERE (Exam_Publish_Setting.SchoolID = @SchoolID) AND (Exam_Publish_Setting.EducationYearID = @EducationYearID) ORDER BY Exam_Name.ExamName">
                                <SelectParameters>
                                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                                    <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                                </SelectParameters>
                            </asp:SqlDataSource>
                        </div>
                    </div>

                    <asp:GridView ID="ClassGridView" CssClass="mGrid" DataKeyNames="ClassID" runat="server" AutoGenerateColumns="False" DataSourceID="ClassSQL">
                        <Columns>
                            <asp:BoundField DataField="Class" HeaderText="Class" SortExpression="Class" />
                            <asp:BoundField DataField="Last_Published_Date" DataFormatString="{0:d MMM yyyy}" HeaderText="Last Create Date" SortExpression="Last_Published_Date" />
                            <asp:TemplateField HeaderText="Input Locked" SortExpression="Marks_Input_Locked">
                                <ItemTemplate>
                                    <asp:CheckBox ID="LockedCheckBox" Text=" " runat="server" Checked='<%# Bind("Marks_Input_Locked") %>' />
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Result Published" SortExpression="IS_Published">
                                <ItemTemplate>
                                    <asp:CheckBox ID="PublishedCheckBox" Text=" " runat="server" Checked='<%# Bind("IS_Published") %>' />
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                    </asp:GridView>
                    <asp:SqlDataSource ID="ClassSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT CreateClass.Class, Exam_Publish_Setting.Marks_Input_Locked, Exam_Publish_Setting.IS_Published, Exam_Publish_Setting.Last_Published_Date, Exam_Publish_Setting.ClassID FROM Exam_Publish_Setting INNER JOIN CreateClass ON Exam_Publish_Setting.ClassID = CreateClass.ClassID WHERE (Exam_Publish_Setting.SchoolID = @SchoolID) AND (Exam_Publish_Setting.EducationYearID = @EducationYearID) AND (Exam_Publish_Setting.ExamID = @ExamID) ORDER BY CreateClass.SN" UpdateCommand="UPDATE Exam_Publish_Setting SET IS_Published = @IS_Published, Marks_Input_Locked = @Marks_Input_Locked WHERE (SchoolID = @SchoolID) AND (EducationYearID = @EducationYearID) AND (ClassID = @ClassID) AND (ExamID = @ExamID)">
                        <SelectParameters>
                            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                            <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                            <asp:ControlParameter ControlID="ExamDropDownList" Name="ExamID" PropertyName="SelectedValue" />
                        </SelectParameters>
                        <UpdateParameters>
                            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                            <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                            <asp:ControlParameter ControlID="ExamDropDownList" Name="ExamID" PropertyName="SelectedValue" />
                            <asp:Parameter Name="ClassID" />
                            <asp:Parameter Name="IS_Published" />
                            <asp:Parameter Name="Marks_Input_Locked" />
                        </UpdateParameters>
                    </asp:SqlDataSource>

                    <asp:Button ID="UpdateButton" OnClick="UpdateButton_Click" runat="server" CssClass="btn btn-primary btnInd" Text="Update" />
                </ContentTemplate>
            </asp:UpdatePanel>
        </div>

        <div class="tab-pane fade" id="panel2" role="tabpanel" aria-expanded="false">
            <asp:UpdatePanel ID="UpdatePanel1" runat="server">
                <ContentTemplate>
                    <label>Cumulative Exam</label>
                    <div class=" form-inline d-print-none">
                        <div class="form-group">
                            <asp:DropDownList ID="Cumulative_ExamDropDownList" runat="server" AutoPostBack="True" CssClass="form-control" DataSourceID="CumulativeExamSQL" DataTextField="CumulativeResultName" DataValueField="CumulativeNameID" AppendDataBoundItems="True">
                                <asp:ListItem Value="0">[ SELECT EXAM ]</asp:ListItem>
                            </asp:DropDownList>
                            <asp:SqlDataSource ID="CumulativeExamSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT DISTINCT Exam_Cumulative_Name.CumulativeResultName, Exam_Cumulative_Name.CumulativeNameID FROM Exam_Cumulative_Name INNER JOIN Exam_Cumulative_Setting ON Exam_Cumulative_Name.CumulativeNameID = Exam_Cumulative_Setting.CumulativeNameID WHERE (Exam_Cumulative_Setting.SchoolID = @SchoolID) AND (Exam_Cumulative_Setting.EducationYearID = @EducationYearID)">
                                <SelectParameters>
                                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                                    <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                                </SelectParameters>
                            </asp:SqlDataSource>
                        </div>
                    </div>

                    <asp:GridView ID="CumulativeClassGridView" CssClass="mGrid" DataKeyNames="ClassID" runat="server" AutoGenerateColumns="False" DataSourceID="CumulativeClassSQL">
                        <Columns>
                            <asp:BoundField DataField="Class" HeaderText="Class" SortExpression="Class" />
                            <asp:BoundField DataField="Last_Published_Date" DataFormatString="{0:d MMM yyyy}" HeaderText="Last Create Date" SortExpression="Last_Published_Date" />
                            <asp:TemplateField HeaderText="Result Published" SortExpression="IS_Published">
                                <ItemTemplate>
                                    <asp:CheckBox ID="PublishedCheckBox" Text=" " runat="server" Checked='<%# Bind("IS_Published") %>' />
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                    </asp:GridView>
                    <asp:SqlDataSource ID="CumulativeClassSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT Exam_Cumulative_Setting.ClassID, CreateClass.Class, Exam_Cumulative_Setting.IS_Published, Exam_Cumulative_Setting.Last_Published_Date FROM CreateClass INNER JOIN Exam_Cumulative_Setting ON CreateClass.ClassID = Exam_Cumulative_Setting.ClassID WHERE (Exam_Cumulative_Setting.SchoolID = @SchoolID) AND (Exam_Cumulative_Setting.EducationYearID = @EducationYearID) AND (Exam_Cumulative_Setting.CumulativeNameID = @CumulativeNameID) ORDER BY CreateClass.SN" UpdateCommand="UPDATE Exam_Cumulative_Setting SET IS_Published = @IS_Published WHERE (SchoolID = @SchoolID) AND (EducationYearID = @EducationYearID) AND (ClassID = @ClassID) AND (CumulativeNameID = @CumulativeNameID)">
                        <SelectParameters>
                            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                            <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                            <asp:ControlParameter ControlID="Cumulative_ExamDropDownList" Name="CumulativeNameID" PropertyName="SelectedValue" />
                        </SelectParameters>
                        <UpdateParameters>
                            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                            <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                            <asp:ControlParameter ControlID="Cumulative_ExamDropDownList" Name="CumulativeNameID" PropertyName="SelectedValue" />
                            <asp:Parameter Name="ClassID" />
                            <asp:Parameter Name="IS_Published" />
                        </UpdateParameters>
                    </asp:SqlDataSource>

                    <asp:Button ID="CumulativeUpdate_Button" OnClick="CumulativeUpdate_Button_Click" runat="server" CssClass="btn btn-primary btnCumu" Text="Update" />
                </ContentTemplate>
            </asp:UpdatePanel>
        </div>

        <label id="SuccMsg" class="alert-success"></label>
    </div>

    <asp:UpdateProgress ID="UpdateProgress" runat="server">
        <ProgressTemplate>
            <div id="progress_BG"></div>
            <div id="progress">
                <img src="../CSS/loading.gif" alt="Loading..." />
                <br />
                <b>Loading...</b>
            </div>
        </ProgressTemplate>
    </asp:UpdateProgress>

    <script type="text/javascript">
        //GridView is empty
        if (!$('[id*=ClassGridView] tr').length) {
            $(".btnInd").hide();
        }
        if (!$('[id*=CumulativeClassGridView] tr').length) {
            $(".btnCumu").hide();
        }

        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function (a, b) {
            //GridView is empty
            if (!$('[id*=ClassGridView] tr').length) {
                $(".btnInd").hide();
            }
            if (!$('[id*=CumulativeClassGridView] tr').length) {
                $(".btnCumu").hide();
            }
        });


        function Success() {
            var e = $('#SuccMsg');
            e.text("Update successfully!!");
            e.fadeIn();
            e.queue(function () { setTimeout(function () { e.dequeue() }, 4000) });
            e.fadeOut('slow');
        }
    </script>
</asp:Content>
