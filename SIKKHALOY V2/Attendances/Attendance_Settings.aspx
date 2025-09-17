<%@ Page Title="Attendance Settings" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Attendance_Settings.aspx.cs" Inherits="EDUCATION.COM.Attendances.Attendance_Settings" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        body { background: #eef5f9 !important; }
        .Setting .h5 { font-size: 1.25rem; color: #212529; margin-bottom: 0; }
        .Setting input[type="radio"] + label { font-size: 16px; color: #000; }
        .Setting .card ul li p { color: #52595D; font-size: 17px; }
        .Setting .card ul li small { color: #81888C; font-size: 13px; }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <div class="card mb-4">
        <div class="card-body d-sm-flex justify-content-between p-2">
            <h4 class="mb-0 pl-2" style="padding-top: 0.8rem">
                <span>
                    <i class="fa fa-cog" aria-hidden="true"></i>
                    Attendance Settings
                </span>
            </h4>
            <div>
                <button id="DownloadButton" onclick="Download();" type="button" class="btn btn-indigo">
                    <i class="fa fa-picture-o" aria-hidden="true"></i>
                    Download Photo
            <img id="loading" style="display: none;" src="CSS/Loading.gif" />
                </button>
                <asp:LinkButton ID="InfoLinkButton" runat="server" CssClass="btn btn-cyan" OnClick="InfoDownload_Click">
         <i class="fa fa-download" aria-hidden="true"></i> Download Info
                </asp:LinkButton>
            </div>
        </div>
    </div>

    <div class="alert alert-info">
        <i class="fa fa-info-circle"></i>
        If changes any setting, you have to re-start attendance software
    </div>

    <asp:FormView ID="SettingFormView" CssClass="Setting" DefaultMode="Edit" Width="100%" runat="server" DataKeyNames="AttendanceSettingID" DataSourceID="AttendanceSettingSQL">
        <EditItemTemplate>
            <div class="row">
                <div class="col-md-6">
                    <h5><i class="fa fa-cogs" aria-hidden="true"></i>
                        Basic Settings</h5>
                    <div class="card mb-4">
                        <div class="card-body">
                            <ul class="list-group list-group-flush mb-4">
                                <li class="list-group-item d-sm-flex justify-content-between align-items-center">
                                    <div>
                                        <p class="mb-0">ATTENDANCE FROM DEVICE</p>
                                        <small>If turn off this setting attendance activities will stop.</small>
                                    </div>
                                    <asp:RadioButtonList SelectedValue='<%# Bind("Is_Device_Attendance_Enable") %>' ID="AttenFromDeviceRb" runat="server" RepeatDirection="Horizontal" RepeatLayout="Flow">
                                        <asp:ListItem Value="False">OFF</asp:ListItem>
                                        <asp:ListItem Value="True">ON</asp:ListItem>
                                    </asp:RadioButtonList>
                                </li>
                                <li class="list-group-item d-sm-flex justify-content-between align-items-center">
                                    <div>
                                        <p class="mb-0">ALL ATTENDANCE SMS</p>
                                        <small>Turn off/on Abs,Late,Entry,Exit, all type of SMS.</small>
                                    </div>
                                    <asp:RadioButtonList ID="RB" SelectedValue='<%# Bind("Is_All_SMS_On") %>' runat="server" RepeatDirection="Horizontal" RepeatLayout="Flow">
                                        <asp:ListItem Value="False">OFF</asp:ListItem>
                                        <asp:ListItem Value="True">ON</asp:ListItem>
                                    </asp:RadioButtonList>
                                </li>
                                <li class="list-group-item d-sm-flex justify-content-between align-items-center">
                                    <div>
                                        <p class="mb-0">HOLIDAY</p>
                                        <small>Attendance count if holiday is off.</small>
                                    </div>
                                    <asp:RadioButtonList ID="RadioButtonList1" SelectedValue='<%# Bind("Is_Holiday_As_Offday") %>' runat="server" RepeatDirection="Horizontal" RepeatLayout="Flow">
                                        <asp:ListItem Value="False">OFF</asp:ListItem>
                                        <asp:ListItem Value="True">ON</asp:ListItem>
                                    </asp:RadioButtonList>
                                </li>
                                <li class="list-group-item d-sm-flex justify-content-between align-items-center">
                                    <div>
                                        <p class="mb-0">SETTING KEY</p>
                                        <small>This key need to access Device Software setting.</small>
                                    </div>
                                    <div class="form-group m-0">
                                        <asp:TextBox CssClass="form-control" ID="SettingKeyTextBox" runat="server" Text='<%# Bind("SettingKey") %>' />
                                    </div>
                                </li>
                                <li class="list-group-item d-sm-flex justify-content-between align-items-center">
                                    <div>
                                        <p class="mb-0">SMS LANGUAGE</p>
                                        <small>Select attendance SMS language.</small>
                                    </div>
                                    <asp:RadioButtonList ID="RadioButtonList13" SelectedValue='<%# Bind("Is_English_SMS") %>' runat="server" RepeatDirection="Horizontal" RepeatLayout="Flow">
                                        <asp:ListItem Value="False">BANGLA</asp:ListItem>
                                        <asp:ListItem Value="True">ENGLISH</asp:ListItem>
                                    </asp:RadioButtonList>
                                </li>
                                <li class="list-group-item d-sm-flex justify-content-between align-items-center">
                                    <div>
                                        <p class="mb-0">SMS SEND TIMEOUT MINUTE</p>
                                        <small>SMS don't send after this time.</small>
                                    </div>
                                    <div class="form-group m-0">
                                        <asp:TextBox CssClass="form-control" ID="SMS_TimeOut_MinuteTextBox" runat="server" Text='<%# Bind("SMS_TimeOut_Minute") %>' />
                                    </div>
                                </li>
                            </ul>

                            <asp:LinkButton ID="UpdateButton" CssClass="btn btn-primary" runat="server" CausesValidation="True" CommandName="Update" Text="Update" />
                        </div>
                    </div>

                    <h5><i class="fa fa-envelope-o" aria-hidden="true"></i>
                        Student Attendance Settings</h5>
                    <div class="card mb-3">
                        <div class="card-body">
                            <ul class="list-group list-group-flush mb-4">
                                <li class="list-group-item d-sm-flex justify-content-between align-items-center">
                                    <div>
                                        <p class="mb-0">ENABLE STUDENT ATTENDANCE</p>
                                        <small>Turn OFF/ON student atendance from device.</small>
                                    </div>
                                    <asp:RadioButtonList ID="RadioButtonList11" SelectedValue='<%# Bind("Is_Student_Attendance_Enable") %>' runat="server" RepeatDirection="Horizontal" RepeatLayout="Flow">
                                        <asp:ListItem Value="False">OFF</asp:ListItem>
                                        <asp:ListItem Value="True">ON</asp:ListItem>
                                    </asp:RadioButtonList>
                                </li>
                                <li class="list-group-item d-sm-flex justify-content-between align-items-center">
                                    <div>
                                        <p class="mb-0">STUDENT ALL ATTENDANCE SMS</p>
                                        <small>Turn OFF/ON all student atendance SMS.</small>
                                    </div>
                                    <asp:RadioButtonList ID="RadioButtonList2" SelectedValue='<%# Bind("Is_Student_All_SMS_Active") %>' runat="server" RepeatDirection="Horizontal" RepeatLayout="Flow">
                                        <asp:ListItem Value="False">OFF</asp:ListItem>
                                        <asp:ListItem Value="True">ON</asp:ListItem>
                                    </asp:RadioButtonList>
                                </li>
                                <li class="list-group-item d-sm-flex justify-content-between align-items-center">
                                    <div>
                                        <p class="mb-0">STUDENT ENTRY SMS</p>
                                        <small>Turn OFF/ON student entry SMS.</small>
                                    </div>
                                    <asp:RadioButtonList ID="RadioButtonList3" SelectedValue='<%# Bind("Is_Student_Entry_SMS_ON") %>' runat="server" RepeatDirection="Horizontal" RepeatLayout="Flow">
                                        <asp:ListItem Value="False">OFF</asp:ListItem>
                                        <asp:ListItem Value="True">ON</asp:ListItem>
                                    </asp:RadioButtonList>
                                </li>
                                <li class="list-group-item d-sm-flex justify-content-between align-items-center">
                                    <div>
                                        <p class="mb-0">STUDENT EXIT SMS</p>
                                        <small>Turn OFF/ON student exit SMS.</small>
                                    </div>
                                    <asp:RadioButtonList ID="RadioButtonList4" SelectedValue='<%# Bind("Is_Student_Exit_SMS_ON") %>' runat="server" RepeatDirection="Horizontal" RepeatLayout="Flow">
                                        <asp:ListItem Value="False">OFF</asp:ListItem>
                                        <asp:ListItem Value="True">ON</asp:ListItem>
                                    </asp:RadioButtonList>
                                </li>
                                <li class="list-group-item d-sm-flex justify-content-between align-items-center">
                                    <div>
                                        <p class="mb-0">STUDENT ABSENCE SMS</p>
                                        <small>Turn OFF/ON student absence SMS.</small>
                                    </div>
                                    <asp:RadioButtonList ID="RadioButtonList5" SelectedValue='<%# Bind("Is_Student_Abs_SMS_ON") %>' runat="server" RepeatDirection="Horizontal" RepeatLayout="Flow">
                                        <asp:ListItem Value="False">OFF</asp:ListItem>
                                        <asp:ListItem Value="True">ON</asp:ListItem>
                                    </asp:RadioButtonList>
                                </li>
                                <li class="list-group-item d-sm-flex justify-content-between align-items-center">
                                    <div>
                                        <p class="mb-0">STUDENT LATE SMS</p>
                                        <small>Turn OFF/ON student late SMS.</small>
                                    </div>
                                    <asp:RadioButtonList ID="RadioButtonList6" SelectedValue='<%# Bind("Is_Student_Late_SMS_ON") %>' runat="server" RepeatDirection="Horizontal" RepeatLayout="Flow">
                                        <asp:ListItem Value="False">OFF</asp:ListItem>
                                        <asp:ListItem Value="True">ON</asp:ListItem>
                                    </asp:RadioButtonList>
                                </li>
                            </ul>

                            <asp:LinkButton ID="LinkButton1" CssClass="btn btn-primary" runat="server" CausesValidation="True" CommandName="Update" Text="Update" />
                        </div>
                    </div>
                </div>
                <div class="col-md-6">
                    <h5><i class="fa fa-envelope-o" aria-hidden="true"></i>
                        Employee Attendance Settings</h5>
                    <div class="card mb-3">
                        <div class="card-body">
                            <ul class="list-group list-group-flush mb-4">
                                <li class="list-group-item d-sm-flex justify-content-between align-items-center">
                                    <div>
                                        <p class="mb-0">ENABLE EMPLOYEE ATTENDANCE</p>
                                        <small>Turn OFF/ON employee atendance from device.</small>
                                    </div>
                                    <asp:RadioButtonList ID="RadioButtonList12" SelectedValue='<%# Bind("Is_Employee_Attendance_Enable") %>' runat="server" RepeatDirection="Horizontal" RepeatLayout="Flow">
                                        <asp:ListItem Value="False">OFF</asp:ListItem>
                                        <asp:ListItem Value="True">ON</asp:ListItem>
                                    </asp:RadioButtonList>
                                </li>
                                <li class="list-group-item d-sm-flex justify-content-between align-items-center">
                                    <div>
                                        <p class="mb-0">ALL EMPLOYEE SMS</p>
                                        <small>Stop all employee attendance SMS.</small>
                                    </div>
                                    <asp:RadioButtonList ID="RadioButtonList7" SelectedValue='<%# Bind("Is_Employee_SMS_Active") %>' runat="server" RepeatDirection="Horizontal" RepeatLayout="Flow">
                                        <asp:ListItem Value="False">OFF</asp:ListItem>
                                        <asp:ListItem Value="True">ON</asp:ListItem>
                                    </asp:RadioButtonList>
                                </li>
                                <li class="list-group-item d-sm-flex justify-content-between align-items-center">
                                    <div>
                                        <p class="mb-0">EMPLOYEE ABSENCE SMS</p>
                                        <small>Turn ON/OFF employee Abs SMS.</small>
                                    </div>
                                    <asp:RadioButtonList ID="RadioButtonList8" SelectedValue='<%# Bind("Is_Employee_Abs_SMS_ON") %>' runat="server" RepeatDirection="Horizontal" RepeatLayout="Flow">
                                        <asp:ListItem Value="False">OFF</asp:ListItem>
                                        <asp:ListItem Value="True">ON</asp:ListItem>
                                    </asp:RadioButtonList>
                                </li>
                                <li class="list-group-item d-sm-flex justify-content-between align-items-center">
                                    <div>
                                        <p class="mb-0">EMPLOYEE LATE SMS</p>
                                        <small>Turn ON/OFF employee late SMS.</small>
                                    </div>
                                    <asp:RadioButtonList ID="RadioButtonList9" SelectedValue='<%# Bind("Is_Employee_Late_SMS_ON") %>' runat="server" RepeatDirection="Horizontal" RepeatLayout="Flow">
                                        <asp:ListItem Value="False">OFF</asp:ListItem>
                                        <asp:ListItem Value="True">ON</asp:ListItem>
                                    </asp:RadioButtonList>
                                </li>
                                <li class="list-group-item d-sm-flex justify-content-between align-items-center">
                                    <div>
                                        <p class="mb-0">EMPLOYEE ATTENDANCE SMS RECEIVER</p>
                                        <small>If employee abs/late, send SMS to the selected method.</small>
                                    </div>
                                    <asp:RadioButtonList ID="RadioButtonList10" SelectedValue='<%# Bind("Is_Employee_SMS_OwnNumber") %>' runat="server" RepeatDirection="Horizontal" RepeatLayout="Flow">
                                        <asp:ListItem Value="False">AUTHORITY</asp:ListItem>
                                        <asp:ListItem Value="True">EMPLOYEE</asp:ListItem>
                                    </asp:RadioButtonList>
                                </li>
                                <li class="list-group-item d-sm-flex justify-content-between align-items-center">
                                    <div>
                                        <p class="mb-0">SMS NUMBER</p>
                                        <small>Who receive employee attendance SMS.</small>
                                    </div>
                                    <div class="form-group m-0">
                                        <asp:TextBox CssClass="form-control" ID="Employee_SMS_NumberTextBox" runat="server" Text='<%# Bind("Employee_SMS_Number") %>' />
                                    </div>
                                </li>
                            </ul>
                            <asp:LinkButton ID="LinkButton2" CssClass="btn btn-primary" runat="server" CausesValidation="True" CommandName="Update" Text="Update" />
                        </div>
                    </div>
                </div>
            </div>
        </EditItemTemplate>
        <EmptyDataTemplate>
            <div class="alert alert-danger">Attendance device is not active in your institution.</div>
        </EmptyDataTemplate>
    </asp:FormView>

    <asp:SqlDataSource ID="AttendanceSettingSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT AttendanceSettingID, SchoolID, UserName, Password, IsActive, InsertDate, SettingKey, Image_Link, Is_Device_Attendance_Enable, Is_All_SMS_On, Is_Holiday_As_Offday, Is_Student_All_SMS_Active, Is_Student_Entry_SMS_ON, Is_Student_Exit_SMS_ON, Is_Student_Abs_SMS_ON, Is_Student_Late_SMS_ON, Is_Employee_SMS_Active, Is_Employee_Abs_SMS_ON, Is_Employee_Late_SMS_ON, Is_Employee_SMS_OwnNumber, Employee_SMS_Number, SMS_TimeOut_Minute, Is_Student_Attendance_Enable, Is_Employee_Attendance_Enable, Is_English_SMS FROM Attendance_Device_Setting WHERE (SchoolID = @SchoolID) AND (IsActive = 1)" UpdateCommand="UPDATE Attendance_Device_Setting SET SettingKey = @SettingKey, Image_Link = @Image_Link, Is_Device_Attendance_Enable = @Is_Device_Attendance_Enable, Is_All_SMS_On = @Is_All_SMS_On, Is_Holiday_As_Offday = @Is_Holiday_As_Offday, Is_Student_All_SMS_Active = @Is_Student_All_SMS_Active, Is_Student_Entry_SMS_ON = @Is_Student_Entry_SMS_ON, Is_Student_Exit_SMS_ON = @Is_Student_Exit_SMS_ON, Is_Student_Abs_SMS_ON = @Is_Student_Abs_SMS_ON, Is_Student_Late_SMS_ON = @Is_Student_Late_SMS_ON, Is_Employee_SMS_Active = @Is_Employee_SMS_Active, Is_Employee_Abs_SMS_ON = @Is_Employee_Abs_SMS_ON, Is_Employee_Late_SMS_ON = @Is_Employee_Late_SMS_ON, Is_Employee_SMS_OwnNumber = @Is_Employee_SMS_OwnNumber, Employee_SMS_Number = @Employee_SMS_Number, SMS_TimeOut_Minute = @SMS_TimeOut_Minute, Is_Student_Attendance_Enable = @Is_Student_Attendance_Enable, Is_Employee_Attendance_Enable = @Is_Employee_Attendance_Enable, Is_English_SMS = @Is_English_SMS WHERE (AttendanceSettingID = @AttendanceSettingID)">
        <SelectParameters>
            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
        </SelectParameters>
        <UpdateParameters>
            <asp:Parameter Name="SettingKey" Type="String" />
            <asp:Parameter Name="Image_Link" Type="String" />
            <asp:Parameter Name="Is_Device_Attendance_Enable" Type="Boolean" />
            <asp:Parameter Name="Is_All_SMS_On" Type="Boolean" />
            <asp:Parameter Name="Is_Holiday_As_Offday" Type="Boolean" />
            <asp:Parameter Name="Is_Student_All_SMS_Active" Type="Boolean" />
            <asp:Parameter Name="Is_Student_Entry_SMS_ON" Type="Boolean" />
            <asp:Parameter Name="Is_Student_Exit_SMS_ON" Type="Boolean" />
            <asp:Parameter Name="Is_Student_Abs_SMS_ON" Type="Boolean" />
            <asp:Parameter Name="Is_Student_Late_SMS_ON" Type="Boolean" />
            <asp:Parameter Name="Is_Employee_SMS_Active" Type="Boolean" />
            <asp:Parameter Name="Is_Employee_Abs_SMS_ON" Type="Boolean" />
            <asp:Parameter Name="Is_Employee_Late_SMS_ON" Type="Boolean" />
            <asp:Parameter Name="Is_Employee_SMS_OwnNumber" Type="Boolean" />
            <asp:Parameter Name="Employee_SMS_Number" Type="String" />
            <asp:Parameter Name="SMS_TimeOut_Minute" Type="Int32" />
            <asp:Parameter Name="AttendanceSettingID" Type="Int32" />
            <asp:Parameter Name="Is_Student_Attendance_Enable" Type="Boolean" />
            <asp:Parameter Name="Is_Employee_Attendance_Enable" Type="Boolean" />
            <asp:Parameter Name="Is_English_SMS" />
        </UpdateParameters>
    </asp:SqlDataSource>

    <script src="/JS/Zip_File/FileServer.js"></script>
    <script src="/JS/Zip_File/jszip.min.js"></script>
    
    <script>
        function Download() {
            $.ajax({
                type: "POST",
                url: "Attendance_Settings.aspx/Get_Image_file",
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                beforeSend: function () {
                    $("#loading").show();
                    $("#DownloadButton").attr("disabled", true);
                },
                complete: function () {
                    console.log("Request completed.");
                },
                success: function (r) {
                    const response = JSON.parse(r.d);
                    zipGenerate(response);
                },
                error: function (err) {
                    console.log(err);
                }
            });
        }

        function zipGenerate(response) {
            var zip = new JSZip();

            response.forEach(function (data) {
                var name = data.ID + ".jpg";
                var photo = b64toFile(data.Image);
                zip.file(name, photo, { base64: true });
            });

            zip.generateAsync({ type: "blob" }).then(function (content) {
                saveAs(content, "Attendance_Photo.zip");
                $("#loading").hide();
                $("#DownloadButton").attr("disabled", false);
            });
        }

        function b64toFile(b64Data) {
            const sliceSize = 512;
            const byteCharacters = atob(b64Data);
            const byteArrays = [];

            for (let offset = 0; offset < byteCharacters.length; offset += sliceSize) {
                const slice = byteCharacters.slice(offset, offset + sliceSize);

                const byteNumbers = new Array(slice.length);
                for (let i = 0; i < slice.length; i++) {
                    byteNumbers[i] = slice.charCodeAt(i);
                }

                const byteArray = new Uint8Array(byteNumbers);

                byteArrays.push(byteArray);
            }

            return new Blob(byteArrays, { type: 'image/png' });
        }
    </script>
</asp:Content>
