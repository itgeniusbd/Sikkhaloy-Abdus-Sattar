const attendance = (function() {
    // CONFIGURATION: Update this URL based on your Attendance_API deployment
    // For local IIS: Use http://localhost:port or http://server-ip:port
    // পরিবর্তন করুন:
    // const baseUrl = "http://localhost:19362";  // Comment করুন
    const baseUrl = "https://api.sikkhaloy.com";  // Enable করুন


    //create new user
    const registerNewDeviceUser = function () {
        var isSend = false;
        const index = $("[id*=School_DropDownList]").get(0).selectedIndex;
        const password = $('[id*=Password_TextBox]').val();

        if (index > 0 && password !== '') {
            const users = {
                username: $('[id*=School_DropDownList]').val(),
                email: $('[id*=School_DropDownList]').val() + "@gmail.com",
                password: password,
                confirmPassword: password
            };

            $.ajax({
                url: `${baseUrl}/api/account/register`,
                method: 'POST',
                async: false,
                contentType: 'application/json',
                data: JSON.stringify(users),
                success: function () {
                    isSend = true;
                },
                error: function (err) {
                    var response = null;
                    const errors = [];
                    var errorsString = "";

                    if (err.status === 400) {
                        try {
                            response = JSON.parse(err.responseText);
                        } catch (e) { }
                    }
                    if (response != null) {
                        const modelState = response.ModelState;

                        for (let key in modelState) {
                            if (modelState.hasOwnProperty(key)) {
                                errorsString = (errorsString === "" ? "" : errorsString + "<br/>") + modelState[key];
                                errors.push(modelState[key]); //list of error messages in an array
                            }
                        }
                    }

                    //DISPLAY THE LIST OF ERROR MESSAGES 
                    if (errorsString !== "") {
                        $("#divErrorText").html(errorsString);
                        $('#divError').show('fade');
                    }
                }
            });
        }
        return isSend;
    }

    //change device password
    const changeDevicePassword = function (user) {
        $.ajax({
            url: `${baseUrl}/token`,
            method: 'POST',
            contentType: 'application/x-www-form-urlencoded',
            data: user,
            success: token => postChangePassword(token, user),
            error: function (err) {
                console.log(err)

                const { error_description } = JSON.parse(err.responseText);
                $.notify(error_description, "error");
            }
        });
    }

    //post change password
    function postChangePassword(token, user) {
        const model = {
            OldPassword: user.password,
            NewPassword: user.newPassword,
            ConfirmPassword: user.newPassword
        }
        if (!token) return;

        $.ajax({
            url: `${baseUrl}/api/account/ChangePassword`,
            method: 'POST',
            headers: {
                Authorization: `Bearer ${token.access_token}`
            },
            contentType: 'application/json',
            data: JSON.stringify(model),
            success: function () {
                updatePassword(user.username, user.newPassword);
            },
            error: function (err) {
                var response = null;
                const errors = [];
                var errorsString = "";

                if (err.status === 400) {
                    try {
                        response = JSON.parse(err.responseText);
                    } catch (e) {}
                }

                if (response != null) {
                    const modelState = response.ModelState;

                    for (let key in modelState) {
                        if (modelState.hasOwnProperty(key)) {
                            errorsString = (errorsString === "" ? "" : errorsString + "<br/>") + modelState[key];
                            errors.push(modelState[key]); //list of error messages in an array
                        }
                    }
                }

                //DISPLAY THE LIST OF ERROR MESSAGES 
                if (errorsString !== "") {
                    $.notify(errorsString, "error");
                }
            }
        });
    }

    //update password in sikkhaloy db
    function updatePassword(userName, password) {
        $.ajax({
            url: "InstitutionRegister.aspx/UpdatePassword",
            method: 'POST',
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            data: JSON.stringify({ userName, password }),
            success: function () {
                location.reload();
            },
            error: function (err) {
                console.log(err)
            }
        });
    }



    const obj = {};
    obj.registerNewDeviceUser = registerNewDeviceUser;
    obj.changeDevicePassword = changeDevicePassword;


    return obj;
})();