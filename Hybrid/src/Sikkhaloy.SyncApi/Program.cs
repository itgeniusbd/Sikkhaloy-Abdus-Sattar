using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Sikkhaloy.SyncApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddHttpLogging();
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();
builder.Services.AddSingleton<EduConnectionFactory>();
builder.Services.AddSingleton<LocalOfficeMode>();
builder.Services.AddSingleton<JwtTokenService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<StudentSyncService>();
builder.Services.AddScoped<ClassStructureService>();
builder.Services.AddScoped<PageAccessService>();
builder.Services.AddScoped<SubAdminService>();
builder.Services.AddScoped<SubjectService>();
builder.Services.AddScoped<SubjectAssignService>();
builder.Services.AddScoped<InstitutionService>();
builder.Services.AddScoped<HolidayService>();
builder.Services.AddScoped<EducationYearService>();
builder.Services.AddScoped<EmployeeService>();
builder.Services.AddScoped<SalaryService>();
builder.Services.AddScoped<StudentInfoService>();
builder.Services.AddScoped<StudentManagementService>();
builder.Services.AddScoped<AttendanceService>();
builder.Services.AddScoped<OfficeSmsGateway>();
builder.Services.AddScoped<PaymentSmsService>();
builder.Services.AddScoped<AccountsService>();
builder.Services.AddScoped<ReportsService>();
builder.Services.AddScoped<BalanceSubmissionService>();
builder.Services.AddScoped<MasterDataService>();
builder.Services.AddScoped<ExamService>();
builder.Services.AddScoped<DashboardService>();
builder.Services.AddScoped<StudentPortalService>();
builder.Services.AddScoped<SmsOfficeService>();
builder.Services.AddScoped<SmsTemplateService>();
builder.Services.AddScoped<RoutineService>();
builder.Services.AddScoped<CommitteeService>();
builder.Services.AddScoped<InventoryService>();
builder.Services.AddScoped<PlatformInvoiceService>();
builder.Services.AddScoped<SupportService>();
builder.Services.AddScoped<AuthorityService>();
builder.Services.AddScoped<AuthorityBasicService>();
builder.Services.AddScoped<AuthorityInvoiceService>();
builder.Services.AddScoped<AuthorityAdminService>();

var jwtKey = builder.Configuration["Jwt:Key"]
             ?? throw new InvalidOperationException("Jwt:Key is missing. Set it in user secrets or environment.");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "sikkhaloy-hybrid";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtIssuer,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());
});

var app = builder.Build();

app.UseHttpLogging();

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var error = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>()?.Error;
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new { error = error?.Message ?? "Unexpected error." });
    });
});

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.Use(async (context, next) =>
{
    if (context.User.Identity?.IsAuthenticated == true)
    {
        var role = context.User.FindFirst(JwtTokenService.RoleClaim)?.Value;
        var path = context.Request.Path.Value ?? "";
        if (string.Equals(role, "Student", StringComparison.OrdinalIgnoreCase)
            && path.StartsWith("/api/sync", StringComparison.OrdinalIgnoreCase)
            && !path.StartsWith("/api/sync/student-portal", StringComparison.OrdinalIgnoreCase)
            && !path.Equals("/api/sync/profile/password", StringComparison.OrdinalIgnoreCase))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsJsonAsync(new { error = "auth.forbidden" });
            return;
        }
    }

    await next();
});
app.MapControllers();
app.Run();
