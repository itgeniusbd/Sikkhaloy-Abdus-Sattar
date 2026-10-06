# Sikkhaloy Hybrid: AI সহকারী (AI Agent) পরিকল্পনা ও খরচের হিসাব

- **তারিখ:** ৬ অক্টোবর ২০২৬
- **হালনাগাদ:** ৬ অক্টোবর ২০২৬, সন্ধ্যা: প্রিপেইড AI প্যাক বিলিং যোগ হয়েছে (§৪), Phase 1-এর সময় বদলানো হয়েছে। ডিফল্ট প্যাক ২০০ কাজ = ২০০ ৳ করা হয়েছে, সব মান DB থেকে আসবে।
- **হালনাগাদ ২:** ৬ অক্টোবর ২০২৬, রাত: প্রতিষ্ঠান নিজেই AI মডেল বেছে নিতে পারবে। DB-চালিত মডেল ক্যাটালগ, মডেলভিত্তিক গুণক (ক্রেডিট = কাজের ওজন × গুণক), মার্জিনের সতর্কতা আর মডেল বাতিল হলে fallback যোগ হয়েছে (§৪.১০–৪.১২)। বিলিংয়ের একক এখন "ক্রেডিট"। Phase 1-এর সময় ৩–৪ দিন বেড়েছে।
- **অবস্থা:** শুধু পরিকল্পনা। এই ফাইল ছাড়া কোনো কোড, ডাটাবেস বা কনফিগ পরিবর্তন করা হয়নি।
- **কোড পর্যালোচনা:** `F:\SIKKHALOY-V3\Hybrid\src` (SIKKHALOY.Hybrid.sln) আর `F:\SIKKHALOY-maui-webview\MauiApps\HybridSikkhaloyApp`

---

## ১. সংক্ষেপে

হাইব্রিডে একটা "AI সহকারী" চ্যাট প্যানেল যোগ করা যাবে। শিক্ষক বা অফিস ইউজার বাংলা বা ইংরেজিতে লিখবেন (পরে ভয়েস আর ছবিও দেওয়া যাবে), যেমন:

- "৮ম শ্রেণির সবাইকে কাল ছুটির নোটিশ দাও"
- "৭ম শ্রেণির গণিত হোমওয়ার্ক: অনুশীলনী ৩.২, ১-৫ নম্বর"
- "৬ষ্ঠ শ্রেণির অভিভাবকদের SMS দাও: আগামীকাল অভিভাবক সভা সকাল ১০টায়"
- "অর্ধবার্ষিক পরীক্ষা, ৮ম শ্রেণি, গণিত: রোল ১ - ৭৮, রোল ২ - ৬৫, রোল ৩ অনুপস্থিত"
- "আজ কতজন অনুপস্থিত?" / "৯ম শ্রেণির কার কত বকেয়া?"

এজেন্ট নিজে থেকে নতুন কোনো ব্যবসায়িক লজিক বানাবে না। হাইব্রিডের **যে service method গুলো এখন পেজ থেকে চলে, সেগুলোই** ব্যবহার করবে (`SmsOfficeService.SendAsync`, `StudentManagementService.SaveNoticeAsync`, `ExamService.SaveInputMarksAsync` ইত্যাদি)। তাই SMS ব্যালেন্স কাটা, রেকর্ড রাখা, ফলাফল হিসাব আগের মতোই হবে।

**মূল নিয়ম:**
1. কিছু পাঠানো বা সেভ করার আগে এজেন্ট একটা **কনফার্মেশন কার্ড** দেখাবে। ইউজার "নিশ্চিত করুন" না চাপলে কিছুই পাঠানো বা সেভ হবে না।
2. যে ইউজারের যে পেজের অনুমতি আছে, এজেন্ট তাকে দিয়ে শুধু সেই কাজই করাবে। এই অনুমতি দেখা হবে **সার্ভারে**।
3. স্কুল (SchoolID) আর সেশন (EducationYearID) আসবে শুধু লগইনের JWT থেকে। AI কখনো অন্য স্কুলের ডেটা দেখবে না, বদলাতেও পারবে না।
4. AI-এর API key থাকবে শুধু সার্ভারের গোপন কনফিগে। ব্রাউজার বা মোবাইল অ্যাপে কখনো যাবে না।
5. প্রতিটা কাজের audit log থাকবে: কে, কখন, কী করল, কতটা SMS খরচ হলো।
6. **বিলিং হবে প্রিপেইড AI প্যাকে, SMS ব্যালেন্সের মতো।** প্রতি প্রতিষ্ঠান মাসে ডিফল্ট **১০০ ক্রেডিট ফ্রি** পাবে (Authority বদলাতে পারবেন)। এরপর প্যাক কিনতে হবে, ডিফল্ট প্যাক ২০০ ক্রেডিট = ২০০ ৳ (প্রতি ক্রেডিট ১ ৳), কেনা হবে বিদ্যমান ShurjoPay ফ্লো দিয়ে। **প্রতিষ্ঠান নিজের AI মডেল বেছে নেবে।** প্রতিটা কাজে কাটবে কাজের ওজন × মডেলের গুণক: সাধারণ মডেলে ×১, স্মার্ট মডেলে ×২–৩, প্রো মডেলে ×৬। মডেল ক্যাটালগ আর গুণক Authority প্যানেল থেকে চলবে। ফ্রি কোটা আর প্যাকের দাম বা আকার দুটোই DB-তে থাকবে, Authority প্যানেল থেকে বদলানো যাবে, কোডে কিছু hardcoded থাকবে না। কনফার্ম বা execute-এর মুহূর্তে atomic চেকসহ ব্যালেন্স কাটবে। বিস্তারিত §৪-এ।

---

## ২. কোড পর্যালোচনায় যা পাওয়া গেল

### ২.১ প্রজেক্ট কাঠামো

| Project | ভূমিকা | AI এজেন্টের জন্য যা হবে |
|---|---|---|
| `Sikkhaloy.SyncApi` | ASP.NET Core (.NET 9) API, JWT auth, সব business service (`Services/*.cs`), `Controllers/SyncController.cs` (~৩৯০০ লাইন) | নতুন `AgentController` আর `Services/Agent/*` এখানে যোগ হবে |
| `Sikkhaloy.Shared` | DTO, `Menu/MenuContracts.cs` (route ↔ permission) | নতুন `Agent/AgentDtos.cs` |
| `Sikkhaloy.App` | Blazor UI (Web আর Windows দুটোতেই একই), `Layout/MainLayout.razor`, `TeacherLayout.razor` | নতুন `Layout/AiAssistantPanel.razor` |
| `Sikkhaloy.LocalData` | SQLite অফলাইন + `Sync/SyncApiClient.cs` (API কল) | এজেন্ট API কলের মেথড |
| `Sikkhaloy.Web` | Blazor Server host (hybrid.sikkhaloy.com) | পরিবর্তন লাগবে না |
| `Sikkhaloy.Client` | WPF + BlazorWebView | পরিবর্তন লাগবে না (ভয়েস ছাড়া) |
| `HybridSikkhaloyApp` (MAUI) | hybrid.sikkhaloy.com-এর WebView wrapper | চ্যাট প্যানেল নিজে থেকেই দেখাবে। ভয়েসের জন্য মাইক্রোফোন পারমিশন সহ নতুন রিলিজ লাগবে |

### ২.২ লগইন আর স্কুল স্কোপ
- `JwtTokenService` এই claim গুলো টোকেনে রাখে: `school_id`, `education_year_id`, `registration_id`, `office_role`, `teacher_id`, `employee_id`। প্রতিটা endpoint `JwtTokenService.FromPrincipal(User)` দিয়ে `SessionSnapshot` বানায়।
- Service গুলোর SQL-এ `@SchoolID` আর `@EducationYearID` সবসময় session থেকে বসানো হয়। **এজেন্টও এই একই session ব্যবহার করবে।**
- Role গুলো হলো Admin, Sub-Admin, Teacher, Student, Authority আর Sub-Authority। `Program.cs`-এর middleware Teacher-কে শুধু `/api/sync/teacher-portal/*` আর Student-কে শুধু `/api/sync/student-portal/*` ব্যবহার করতে দেয়।

### ২.৩ পেজ পারমিশন (গুরুত্বপূর্ণ ফাইন্ডিং)
- পারমিশন রাখা হয় `dbo.Link_Pages` আর `dbo.Link_Users` টেবিলে। `MasterDataService.GetMenuAsync(session)` থেকে `MenuTreeDto` আসে। Admin সব পেজ পায়, অন্যরা শুধু `Link_Users`-এ যেগুলো আছে।
- পুরোনো `.aspx` পেজগুলোকে `HybridMenuRoutes.Apply` হাইব্রিড route-এ রূপান্তর করে, যেমন `/sms/send`, `/student-mgmt/notice`, `/exam/input`।
- ক্লায়েন্টে `MainLayout.EnforcePageAccess()` → `menu.CanOpenRoute(path)` চলে। অনুমতি না থাকলে ড্যাশবোর্ডে পাঠিয়ে দেয়।
- **সমস্যা:** সার্ভারে `SyncController`-এর বেশিরভাগ endpoint, যেমন `sms/send`, `student-mgmt/notices`, `exam/input`, `accounts/reports/due`, শুধু লগইন আছে কিনা দেখে। কোন পেজের অনুমতি আছে সেটা দেখে না। শুধু receipt-এর জন্য `CanUseReceiptAsync` আর কিছু Admin-only endpoint ব্যতিক্রম। মানে পেজ পারমিশন এখন আসলে UI-তেই সীমাবদ্ধ।
- **তাই এজেন্টের জন্য আলাদা server-side guard লাগবে** (`AgentPermissionGuard`)। প্রতিটা tool-এর জন্য দরকারি route ঠিক করা থাকবে, আর `GetMenuAsync(session).ContainsRoute(route)` দিয়ে যাচাই হবে। এই guard পরে বিদ্যমান endpoint-গুলোতেও লাগানো যায়, যা আলাদা একটা নিরাপত্তা উন্নতি হবে।

### ২.৪ ফিচার অনুযায়ী বিদ্যমান কোড

| কাজ | Endpoint (SyncApi) | Service method | টেবিল | Menu route (পারমিশন) | মন্তব্য |
|---|---|---|---|---|---|
| নোটিশ / হোমওয়ার্ক তৈরি | `POST api/sync/student-mgmt/notices` | `StudentManagementService.SaveNoticeAsync(SaveNoticeRequest{NoticeTitle, Notice, IsHomeWork, ClassIDs})` | `StudentNotice`, `StudentNoticeClass` | `/student-mgmt/notice` (HW/Notice) | Student portal-এ দেখায় (`StudentPortalService`)। স্কুল ওয়েবসাইটে শুধু `IsHomeWork=0` দেখায়। ClassIDs স্কুলের কিনা service যাচাই করে না, এজেন্ট করবে। নোটিশ দিলে কোনো SMS যায় না। |
| শিক্ষকের নোটিশ | `POST api/sync/teacher-portal/notices` | `TeacherPortalService.SaveNoticeAsync` | একই | Teacher role | শিক্ষক শুধু নিজের নোটিশ দেখেন |
| SMS ব্যালেন্স | `GET api/sync/sms/balance` | `SmsOfficeService.GetBalanceAsync` | `dbo.SMS.SMS_Balance` | `/sms/send` | |
| SMS প্রাপক | `GET sms/students`, `sms/teachers`, `sms/contacts` | `SmsOfficeService.GetStudentsAsync/GetTeachersAsync/GetContactsAsync` | `Student`, `StudentsClass`, `SMS_Group_*` | `/sms/send`, `/sms/contacts` | |
| SMS পাঠানো | `POST api/sync/sms/send` | `SmsOfficeService.SendAsync(SendOfficeSmsRequest{Mode, Text, StudentIds, TeacherIds, ContactIds, CommitteeMemberIds, Phones})` | `SMS_Send_Record`, `SMS_OtherInfo` | `/sms/send` | আগে ব্যালেন্স চেক করে (`sms.low`)। তারপর `OfficeSmsGateway` দিয়ে একটা একটা করে পাঠায় (Novocom ডিফল্ট, এছাড়া BanglaPhone/GreenWeb)। `SMS_OtherInfo`-তে insert হলে trigger **`Tr_SMS_Count`** ব্যালেন্স কাটে। SMS অংশের হিসাব: Unicode (বাংলা) হলে ৭০ অক্ষরে এক SMS, ইংরেজি হলে ১৬০। Dev/localhost-এ `LocalOfficeMode` আসল SMS পাঠায় না। |
| বকেয়া তালিকা | `GET accounts/reports/due` | `ReportsService.GetCurrentDueAsync` | `Income_PayOrder` ইত্যাদি | `/accounts/reports/current-due` | |
| বকেয়া SMS | `POST accounts/reports/due-sms` | `PaymentSmsService.SendDueSmsAsync(DueSmsRequest{Ids, RoleId})` | SMS টেবিল | `/accounts/reports/current-due` | |
| নম্বর এন্ট্রি (অফিস) | `GET/POST api/sync/exam/input`, `GET exam/filters` | `ExamService.GetInputSheetAsync` / `SaveInputMarksAsync(SaveInputMarksRequest{ClassID, ExamID, SubjectID, SubExamID, AllSubExams, Students[]})` | `Exam_Obtain_Marks`, `Exam_Full_Marks` | `/exam/input` | Full mark-এর বেশি দিলে `exam.overMark` আসে। **অফিসের save-এ `Marks_Input_Locked` চেক হয় না।** এজেন্ট এটা চেক করবে। |
| নম্বর এন্ট্রি (শিক্ষক) | `GET/POST api/sync/teacher-portal/marks` | `TeacherPortalService.SaveInputMarksAsync` | একই | Teacher role | `OwnsSubjectAsync` (শুধু নিজের বিষয়) আর `IsMarksLockedAsync` চেক করে |
| আজকের হাজিরা | `GET attendance/student/records`, `attendance/student/summary`, `dashboard/overview` | `AttendanceService.ListStudentRecordsAsync`, `DashboardService.GetOverviewAsync` (`AttendanceToday`) | `Attendance_Record` | `/attendance/student/records` | অনুপস্থিতির SMS এখনই trigger `Tr_Attendance_Record_SMS` → `Attendance_SMS` queue দিয়ে স্বয়ংক্রিয়ভাবে যায় |
| মেধা তালিকা / ফলাফল | `GET exam/merit`, `exam/result-cards`, `POST exam/merit/sms` | `ExamService` / `SmsOfficeService.ExamMerit` | `Exam_Result_of_Student` | `/exam/merit`, `/exam/result-card` | |
| জন্মদিনের SMS | `POST dashboard/birthday-sms` | `SmsOfficeService.SendBirthdaySmsAsync` | | dashboard | |

### ২.৫ UI আর মোবাইল
- `Sikkhaloy.App`-এর `MainLayout.razor` (অফিস) আর `TeacherLayout.razor` (শিক্ষক)-এ একটা ভাসমান "AI সহকারী" বাটন আর পাশে খোলার চ্যাট প্যানেল বসানো যাবে। Web, Windows অ্যাপ আর MAUI WebView তিনটাতেই একই কম্পোনেন্ট দেখাবে।
- MAUI অ্যাপের `AppWebChromeClient.cs` ইতিমধ্যে `<input type="file">` দিয়ে ক্যামেরা বা গ্যালারি থেকে ছবি দেওয়া সাপোর্ট করে। **তাই মার্কশিটের ছবি তোলা মোবাইলে এখনই কাজ করবে।**
- মাইক্রোফোন ইচ্ছা করেই বন্ধ রাখা আছে (`OnPermissionRequest`-এ শুধু camera, "never microphone")। AndroidManifest-এ `RECORD_AUDIO` নেই। ভয়েস চাইলে নতুন অ্যাপ রিলিজ আর Play Console-এর Data safety আপডেট লাগবে। সহজ বিকল্প হলো ফোনের কীবোর্ডের (Gboard) বাংলা ভয়েস টাইপিং, যাতে কোনো খরচ বা কোড লাগে না।
- হাইব্রিড অফলাইনে কাজ করে, কিন্তু **AI এজেন্ট শুধু অনলাইনে চলবে**, SMS-এর মতোই।

---

## ৩. প্রস্তাবিত ডিজাইন

### ৩.১ কাজের ধারা

```
[চ্যাট প্যানেল (Blazor)]
   │ POST /api/agent/chat  {conversationId, message, imageId?}
   ▼
[AgentController] ── JWT → SessionSnapshot (SchoolID, YearID, Role)
   │
   ▼
[AgentService]  ── ইউজারের menu থেকে অনুমোদিত tool বাছাই
   │   ├─► LLM (Gemini/OpenAI)  ← system prompt + অনুমোদিত tool schema + কথোপকথন
   │   │      └─ tool call (যেমন find_class, get_sms_balance, draft_sms)
   │   ├─► Read tool → বিদ্যমান service → ফলাফল আবার LLM-কে
   │   └─► Write tool → সরাসরি চলবে না → PendingAction তৈরি → কনফার্মেশন কার্ড
   ▼
[চ্যাট প্যানেল] কার্ড: প্রাপক ২৩৫ জন, SMS ৪৭০টা, বর্তমান ব্যালেন্স ৩২০০, পাঠানোর পর ২৭৩০
   │ ইউজার "নিশ্চিত করুন"
   │ POST /api/agent/confirm {pendingActionId}
   ▼
[AgentService] অনুমতি, ব্যালেন্স আর lock আবার চেক → বিদ্যমান service method → AI_Agent_Action-এ log
```

### ৩.২ নতুন ফাইল (পরিকল্পনা, এখনো তৈরি হয়নি)

| ফাইল | কাজ |
|---|---|
| `Sikkhaloy.SyncApi/Controllers/AgentController.cs` | `[Authorize] [Route("api/agent")]`: `chat`, `confirm`, `cancel`, `history`, `upload-image` (Phase 3), `transcribe` (Phase 3) |
| `Sikkhaloy.SyncApi/Services/Agent/AgentService.cs` | LLM লুপ (সর্বোচ্চ ৫টা tool round), PendingAction তৈরি আর execute |
| `.../Agent/AgentToolRegistry.cs` | প্রতিটা tool-এর নাম, JSON schema, Read/Write ধরন, দরকারি route, অনুমোদিত role, আর কোন service method চলবে |
| `.../Agent/AgentPermissionGuard.cs` | `MasterDataService.GetMenuAsync` + `ContainsRoute`। Teacher-এর জন্য `OwnsSubjectAsync` |
| `.../Agent/LlmClient.cs` + `LlmProviderFactory.cs` | Provider-নিরপেক্ষ (`Microsoft.Extensions.AI` `IChatClient`)। `AI_Model.Provider` অনুযায়ী Gemini বা OpenAI adapter (Anthropic পরে), key আসবে কনফিগ থেকে |
| `.../Agent/AiModelResolver.cs` | কোন অনুরোধে কোন মডেল চলবে (override → স্কুলের ডিফল্ট → fallback চেইন → গ্লোবাল ডিফল্ট), ক্রেডিটের কোট (ওজন × গুণক), মার্জিন হিসাব (§৪.১১–৪.১২) |
| `.../Agent/AgentAuditService.cs` | Audit, usage (টোকেন আর খরচ), rate limit কাউন্টার |
| `.../Agent/BanglaText.cs` | বাংলা সংখ্যা (০-৯ → 0-9) আর ক্লাসের নাম ("৮ম", "অষ্টম", "Eight", "Class 8") মেলানো |
| `Sikkhaloy.Shared/Agent/AgentDtos.cs` | `AgentChatRequest/Response`, `AgentConfirmCard`, `AgentConfirmRequest` |
| `Sikkhaloy.App/Layout/AiAssistantPanel.razor` (+ `.css`) | চ্যাট প্যানেল, কনফার্মেশন কার্ড, ছবি আপলোড বাটন |
| `Sikkhaloy.LocalData/Sync/SyncApiClient.cs` | `AgentChatAsync`, `AgentConfirmAsync` মেথড যোগ |
| `Sikkhaloy.SyncApi/Services/Agent/AiBillingService.cs` | Wallet, atomic কাটা আর ফেরত, মাসিক রিসেট, প্যাক কেনা শুরু, নোটিফিকেশন (§৪) |
| `Sikkhaloy.SyncApi/Services/ShurjoPayOrderClient.cs` | `SmsOfficeService.CreateShurjoPayOrderAsync` এখানে সরিয়ে SMS আর AI দুই জায়গা থেকেই ব্যবহার |
| `ShurjoPayCallbackService.cs` (পরিবর্তন) | `isAi` শাখা + `SaveAiPackAsync` |
| `Sikkhaloy.App/Pages/AiBillingPage.razor`, `AuthorityAiBillingPage.razor`, `PaymentResultPage.razor` (পরিবর্তন) | স্কুলের মিটার, মডেল বাছাই আর কেনার পেজ; Authority-র সেটিংস/প্যাক/**মডেল**/প্রতিষ্ঠান/রিপোর্ট পেজ; `kind=ai` |
| `Database/AI_Agent_Tables.sql` | §৩.৭ আর §৪.৯-এর টেবিল (আপনার অনুমতি নিয়ে চালানো হবে) |

### ৩.৩ Tool তালিকা আর পারমিশন ম্যাপিং

| Tool | ধরন | বিদ্যমান service | দরকারি route | Role |
|---|---|---|---|---|
| `list_classes` / `find_class` | Read | `ClassStructureService.GetAsync` | যেকোনো অফিস ইউজার | Admin, Sub-Admin, Teacher |
| `get_sms_balance` | Read | `SmsOfficeService.GetBalanceAsync` | `/sms/send` | Admin, Sub-Admin |
| `draft_notice` → `save_notice` | Write (কনফার্ম লাগবে) | `StudentManagementService.SaveNoticeAsync` (IsHomeWork=false) | `/student-mgmt/notice` | Admin, Sub-Admin। Teacher-এর জন্য `TeacherPortalService.SaveNoticeAsync` |
| `draft_homework` → `save_homework` | Write | একই (IsHomeWork=true) | `/student-mgmt/notice` | একই |
| `draft_sms` → `send_sms` | Write | `SmsOfficeService.SendAsync` | `/sms/send` (contact গ্রুপের জন্য `/sms/contacts`) | Admin, Sub-Admin |
| `list_exams`, `get_marks_sheet` | Read | `ExamService.GetFiltersAsync` / `GetInputSheetAsync` | `/exam/input` | Admin, Sub-Admin। Teacher-এর জন্য teacher-portal সংস্করণ |
| `draft_marks` → `save_marks` | Write | `ExamService.SaveInputMarksAsync` / `TeacherPortalService.SaveInputMarksAsync` | `/exam/input` | একই। lock চেক বাধ্যতামূলক |
| `today_attendance` / `absent_list` | Read | `AttendanceService.ListStudentRecordsAsync`, `DashboardService` | `/attendance/student/records` | Admin, Sub-Admin |
| `send_absent_guardian_sms` | Write | `SmsOfficeService.SendAsync` (অনুপস্থিত StudentIds দিয়ে) | `/attendance/student/records` **এবং** `/sms/send` | Admin, Sub-Admin |
| `due_list` | Read | `ReportsService.GetCurrentDueAsync` | `/accounts/reports/current-due` | Admin, Sub-Admin |
| `send_due_reminder` | Write | `PaymentSmsService.SendDueSmsAsync` | `/accounts/reports/current-due` | Admin, Sub-Admin |
| `exam_result_summary` / `merit_list` | Read | `ExamService` merit/result | `/exam/merit` | Admin, Sub-Admin |
| `open_page` | UI | কোনো API নয়। প্যানেলে লিংক দেখাবে (যেমন রিপোর্ট পেজ) | ঐ পেজের route | সবাই (`CanOpenRoute` অনুযায়ী) |

- LLM-কে শুধু সেই tool গুলোই দেখানো হবে যেগুলোর অনুমতি ইউজারের আছে। এরপর execute আর confirm দুই সময়েই সার্ভার আবার অনুমতি যাচাই করবে।
- Student আর Authority role-কে শুরুতে এজেন্ট দেওয়া হবে না।

### ৩.৪ স্কুল স্কোপ আর ID যাচাই
- `SchoolID` আর `EducationYearID` কোনো tool-এর parameter-এ থাকবে না। এগুলো সবসময় session থেকে আসবে।
- LLM যে `classId`, `studentId`, `examId`, `subjectId` দেবে, সার্ভার আগে যাচাই করবে সেগুলো এই স্কুল আর সেশনের কিনা। না মিললে বাতিল।
- নাম থেকে ID (যেমন "রোল ৫", "৮ম শ্রেণি") সার্ভারেই খোঁজা হবে। একাধিক মিল পেলে এজেন্ট ইউজারকে জিজ্ঞেস করবে।

### ৩.৫ বাধ্যতামূলক কনফার্মেশন
- কোনো Write tool সরাসরি চলবে না। সার্ভার একটা `PendingAction` বানাবে: JSON payload, payload-এর SHA-256 hash, মেয়াদ ১০ মিনিট, একবারই ব্যবহার করা যাবে।
- কার্ডে দেখাবে:
  - **SMS:** পুরো লেখা, প্রাপকের সংখ্যা, প্রথম ৫ জনের নাম, প্রতি SMS-এ কয়টা অংশ (একই `SmsCount` নিয়মে), মোট SMS, বর্তমান ব্যালেন্স আর পাঠানোর পরের ব্যালেন্স, ভুল বা ফাঁকা নম্বরের সংখ্যা।
  - **নোটিশ / হোমওয়ার্ক:** শিরোনাম, লেখা, কোন কোন ক্লাস।
  - **সব কার্ডে মডেল আর AI ক্রেডিট:** "মডেল: Gemini 3.1 Flash-Lite (সাধারণ) · কাটবে ১ × ১ = ১ ক্রেডিট (ফ্রি বাকি ৩৫/১০০, কেনা ১৮০)" (§৪.১১)।
  - **নম্বর:** পরীক্ষা, ক্লাস, বিষয়, আর একটা টেবিল (রোল, নাম, নম্বর, অনুপস্থিত)। আগে থেকে নম্বর থাকলে কোনগুলো বদলাবে তা হাইলাইট হবে। Full mark-এর বেশি হলে লাল চিহ্ন দেবে।
- বাটন: **নিশ্চিত করুন / সম্পাদনা / বাতিল**। "সম্পাদনা" চাপলে লেখা বা টেবিল প্যানেলেই বদলানো যাবে।
- Confirm চাপার সময় সার্ভার আবার অনুমতি, ব্যালেন্স আর marks lock চেক করবে, তারপর আসল service method চালাবে।

### ৩.৬ SMS খরচ আর ব্যালেন্স
- আগে থেকে হিসাব হবে বিদ্যমান নিয়মেই: বাংলা হলে ৭০ অক্ষরে এক SMS, ইংরেজি হলে ১৬০ অক্ষরে। নম্বর যাচাই হবে `IsValidBdMobile` দিয়ে।
- ব্যালেন্স কম হলে কার্ডেই জানিয়ে দেবে, আর রিচার্জ পেজের লিংক দেবে।
- এক কাজে সর্বোচ্চ প্রাপক, যেমন ৫০০ জন (কনফিগে বদলানো যাবে)। এর বেশি হলে শুধু Admin পারবেন।
- এজেন্ট দিয়ে প্রতি ইউজারের দৈনিক SMS সীমা থাকবে (কনফিগে)।
- আসল ব্যালেন্স কাটা আগের মতোই trigger `Tr_SMS_Count` করবে। এজেন্ট আলাদা করে কিছু কাটবে না।
- বড় তালিকায় `SendAsync` একটা একটা করে পাঠায়, সময় লাগে। পরে চাইলে এটা background queue-তে নেওয়া যায় (আলাদা কাজ)।

### ৩.৭ Audit টেবিল (খসড়া SQL, এখনো চালানো হয়নি)

```sql
CREATE TABLE dbo.AI_Agent_Conversation (
    ConversationID   UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    SchoolID         INT NOT NULL,
    RegistrationID   INT NOT NULL,
    StartedAt        DATETIME2 NOT NULL DEFAULT SYSDATETIME()
);

CREATE TABLE dbo.AI_Agent_Action (
    ActionID         BIGINT IDENTITY(1,1) PRIMARY KEY,
    ConversationID   UNIQUEIDENTIFIER NOT NULL,
    SchoolID         INT NOT NULL,
    EducationYearID  INT NOT NULL,
    RegistrationID   INT NOT NULL,
    UserName         NVARCHAR(128) NOT NULL,
    Role             NVARCHAR(50) NOT NULL,
    ToolName         NVARCHAR(64) NOT NULL,
    Status           NVARCHAR(20) NOT NULL,   -- Proposed / Confirmed / Executed / Cancelled / Failed / Expired
    PayloadJson      NVARCHAR(MAX) NULL,       -- ফোন নম্বর ছাড়া
    PayloadHash      CHAR(64) NULL,
    RecipientCount   INT NULL,
    SmsCount         INT NULL,
    ResultJson       NVARCHAR(MAX) NULL,
    Error            NVARCHAR(500) NULL,
    CreatedAt        DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    ExpiresAt        DATETIME2 NULL,
    ConfirmedAt      DATETIME2 NULL,
    ExecutedAt       DATETIME2 NULL
);
CREATE INDEX IX_AI_Agent_Action_School ON dbo.AI_Agent_Action (SchoolID, CreatedAt);

CREATE TABLE dbo.AI_Agent_Usage (
    UsageID          BIGINT IDENTITY(1,1) PRIMARY KEY,
    SchoolID         INT NOT NULL,
    RegistrationID   INT NOT NULL,
    Model            NVARCHAR(64) NOT NULL,
    InputTokens      INT NOT NULL,
    OutputTokens     INT NOT NULL,
    CostUsd          DECIMAL(10,6) NOT NULL,
    CreatedAt        DATETIME2 NOT NULL DEFAULT SYSDATETIME()
);
CREATE INDEX IX_AI_Agent_Usage_School ON dbo.AI_Agent_Usage (SchoolID, CreatedAt);
```

- Authority প্যানেলে স্কুলভিত্তিক মাসিক AI খরচ দেখানো যাবে (`AI_Agent_Usage` থেকে)।
- কথোপকথনের লেখা ৯০ দিন পর মুছে ফেলার job রাখা হবে।

### ৩.৮ Rate limit আর খরচ নিয়ন্ত্রণ
- .NET 9-এর built-in `Microsoft.AspNetCore.RateLimiting`: প্রতি ইউজার মিনিটে ১৫টা আর দিনে ৩০০টা মেসেজ (কনফিগে বদলানো যাবে)।
- খরচ নিয়ন্ত্রণ মূলত প্রিপেইড AI ব্যালেন্স দিয়ে হবে (§৪)। ব্যালেন্স না থাকলে LLM call-ই হবে না। এর বাইরে প্রতি কাজে টোকেনের সীমাও থাকবে।
- কনফিগ: `Agent:Enabled` (সব বন্ধের সুইচ), আর স্কুলভিত্তিক চালু/বন্ধ (যেমন শুধু পেইড স্কুলে)।
- এক মেসেজে সর্বোচ্চ ৫টা tool round, আর output-এ max token সীমা।
- System prompt আর tool schema প্রতিবার একই থাকে, তাই provider-এর prompt caching ব্যবহার করে খরচ কমানো যাবে।

### ৩.৯ API key আর কনফিগ
- `appsettings.Secrets.json` (ইতিমধ্যে `.gitignore`-এ আছে) অথবা সার্ভারের environment variable-এ থাকবে:
  `Agent:Providers:Google:ApiKey`, `Agent:Providers:OpenAI:ApiKey`, `Agent:Providers:Anthropic:ApiKey` (যেটা ব্যবহার হবে শুধু সেটা), `Agent:MaxRecipients`, `Agent:DailySmsLimit`
- কোন মডেল চলবে সেটা কনফিগে থাকবে না, থাকবে DB-র `AI_Model` ক্যাটালগে (§৪.১১)। DB-তে key থাকবে না, শুধু provider-এর নাম।
- Key কখনো ক্লায়েন্টে যাবে না, log-এ লেখা হবে না, এরর মেসেজেও দেখাবে না।

### ৩.১০ গোপনীয়তা আর নিরাপত্তা
- **শুধু পেইড API tier ব্যবহার করতে হবে।** Gemini-র free tier-এ পাঠানো ডেটা গুগল নিজেদের প্রোডাক্ট উন্নত করতে ব্যবহার করে ("Content used to improve our products")। ছাত্রছাত্রীর ডেটার জন্য এটা ঠিক না।
- LLM-কে ফোন নম্বর পাঠানো হবে না। নাম, রোল আর ID শুধু দরকার হলে পাঠানো হবে। প্রাপকের নম্বর সার্ভার নিজেই বের করবে।
- **Prompt injection থেকে সুরক্ষা:** ডাটাবেসের লেখা (নোটিশ, নাম) LLM-কে শুধু ডেটা হিসেবে দেওয়া হবে। কনফার্মেশন ছাড়া কোনো Write কাজ চলবে না, আর অনুমোদিত tool তালিকার বাইরে কিছু চলবে না।
- AI-এর লেখা সবসময় খসড়া। ইউজার পড়ে কনফার্ম করবেন।

### ৩.১১ ভাষা
- System prompt বাংলায় থাকবে। উত্তর আসবে ইউজার যে ভাষায় লিখবেন সেই ভাষায়।
- বাংলা সংখ্যা স্বয়ংক্রিয়ভাবে ইংরেজিতে রূপান্তর হবে। "অনুপস্থিত", "abs", "A" সবই অনুপস্থিত ধরা হবে।
- নোটিশ বা SMS-এর ভাষা ইউজার বেছে নিতে পারবেন। SMS ছোট রাখার পরামর্শও দেবে, যেমন "এই লেখা ৩টা SMS হবে, ছোট করব?"

---

## ৪. বিলিং: প্রিপেইড AI প্যাক (SMS ব্যালেন্সের মতো)

> **সিদ্ধান্ত:** প্রতিটা প্রতিষ্ঠান প্রতি মাসে কিছু **ফ্রি কোটা** পাবে, **ডিফল্ট ১০০ ক্রেডিট** (×১ মডেলে ১০০টা কাজ)। কোটা শেষ হলে প্রতিষ্ঠান **AI প্যাক** কিনবে। **ডিফল্ট প্যাক ২০০ ক্রেডিট = ২০০ টাকা (প্রতি ক্রেডিট ১ ৳)**। **প্রতিষ্ঠান নিজের AI মডেল বেছে নেবে, আর চার্জ মডেলের উপর নির্ভর করবে: কাটা ক্রেডিট = কাজের ওজন × মডেলের গুণক** (§৪.১০–৪.১২)। পুরো ব্যবস্থাটা SMS ব্যালেন্স কেনার বিদ্যমান ShurjoPay ফ্লো দিয়েই চলবে।
>
> **কোডে কোনো hardcoding থাকবে না, সব DB থেকে আসবে।** Authority প্যানেল থেকে বদলানো যাবে:
> - গ্লোবাল ডিফল্ট ফ্রি কোটা (`AI_Billing_Settings.DefaultMonthlyFreeQuota`)
> - প্রতিষ্ঠানভিত্তিক override (`AI_School_Wallet.FreeQuotaOverride`)
> - একাধিক প্যাক, প্রতিটার আকার, দাম, সক্রিয়/বন্ধ আর ক্রম (`AI_Pack`)
> - ৮০% সীমা আর টুলের ওজন
> - মডেল ক্যাটালগ: কোন মডেল চালু থাকবে, প্রতিটার গুণক, টাস্ক ওজন, আসল খরচ, fallback আর গ্লোবাল ডিফল্ট মডেল (`AI_Model`)
>
> কোডে শুধু "row না থাকলে নিরাপদ fallback" থাকবে। SQL seed-এ ডিফল্ট মানগুলো (১০০ আর ২০০/২০০) একবার বসানো হবে।

### ৪.১ "এক কাজ (action)" আর ক্রেডিট বলতে কী বোঝাবে
- **ক্রেডিট হলো বিলিংয়ের একক।** একটা কাজে কাটে **ওজন × মডেলের গুণক** ক্রেডিট। যেমন ডিফল্ট মডেলে (×১) সাধারণ কাজে ১ ক্রেডিট, আর ×৩ মডেলে OCR-এ (ওজন ২) ৬ ক্রেডিট। নিচের "১ কাজ" মানে ওজন ১।
- **Write কাজ:** কনফার্ম করার পর সফলভাবে চলা প্রতিটা কাজ, যেমন SMS পাঠানো, নোটিশ বা হোমওয়ার্ক সেভ, নম্বর সেভ। এখানে **১ কাজ** মানে একটা কনফার্ম, প্রাপক যতজনই হোক। SMS-এর খরচ আলাদাভাবে SMS ব্যালেন্স থেকে কাটবে।
- **Read কাজ:** প্রশ্নের উত্তর বা রিপোর্ট সফলভাবে দেওয়া হলে ১ কাজ কাটবে, যেমন "আজ কতজন অনুপস্থিত?"।
- **যা চার্জ হবে না:** খসড়া, সম্পাদনা আর বাতিল। তবে অপব্যবহার ঠেকাতে কোনো বিলযোগ্য কাজ ছাড়া এক কথোপকথনে ১০টার বেশি AI টার্ন হলে পরের টার্ন থেকে ১ কাজ কাটবে (Authority এটা বদলাতে পারবেন)।
- **ভারী কাজের ওজন:** Authority কনফিগ করতে পারবেন। প্রস্তাবিত ডিফল্ট হলো মার্কশিটের ছবি (OCR) = ২ কাজ, ভয়েস কমান্ড = বাড়তি ০ (Phase 3-এ খরচ দেখে ঠিক হবে)।
- **কোন কোটা আগে কাটবে:** আগে মাসের ফ্রি ক্রেডিট, শেষ হলে কেনা ক্রেডিট। ফ্রি কোটাও ক্রেডিটে গোনা হবে, তাই দামি মডেলে ফ্রি কোটা দ্রুত শেষ হবে।

### ৪.২ ফ্রি কোটা মাসিক রিসেট
- মাস গোনা হবে Asia/Dhaka সময় অনুযায়ী (১ তারিখ ০০:০০)। কোনো আলাদা job লাগবে না। মাসের প্রথম ব্যবহারে বা ব্যালেন্স দেখার সময় wallet-এর `PeriodMonth` বর্তমান মাস না হলে `FreeUsed = 0` হয়ে যাবে। এটা "lazy reset" আর ঐ একই atomic transaction-এর ভেতরেই হবে, সাথে ledger-এ `MonthlyReset` এন্ট্রি।
- ফ্রি কোটা পরের মাসে জমা হবে না।
- কেনা ব্যালেন্সের মেয়াদ নিয়ে সিদ্ধান্ত দরকার (§৮ দেখুন)। প্রস্তাব: মেয়াদ নেই, SMS ব্যালেন্সের মতোই।
- কার্যকর কোটা = `FreeQuotaOverride` (প্রতিষ্ঠানের জন্য আলাদা করে দেওয়া থাকলে), না থাকলে `AI_Billing_Settings.DefaultMonthlyFreeQuota` (**১০০ ক্রেডিট**)।

### ৪.৩ কাটার সময় আর atomic চেক
1. **AI call-এর আগে প্রি-চেক:** ব্যবহারযোগ্য ব্যালেন্স (ফ্রি বাকি + কেনা) কম হলে LLM call-ই হবে না, তাই আমাদের টোকেন খরচও হবে না। চ্যাটে "AI ব্যালেন্স শেষ, প্যাক কিনুন" কার্ড আর বাটন দেখাবে।
2. **কনফার্ম বা execute-এর মুহূর্তে কাটা:** কাটা আর PendingAction-এর স্ট্যাটাস বদলানো একটাই SQL transaction-এ হবে। এর ফলে দুইবার ক্লিক বা একসাথে দুইটা অনুরোধ এলেও দুইবার কাটবে না:

```sql
-- @Units = AI_Agent_Action.CreditsQuoted (= কাজের ওজন × মডেলের গুণক, প্রস্তাবের সময় কোট করা), @Month = বর্তমান মাসের ১ তারিখ (Asia/Dhaka), @Quota = কার্যকর ফ্রি কোটা
DECLARE @after TABLE (FreeUsed INT, PaidBalance INT, FromFree INT);
BEGIN TRAN;
UPDATE dbo.AI_Agent_Action SET Status = N'Executing'
 WHERE ActionID = @ActionID AND SchoolID = @SchoolID AND Status = N'Proposed' AND ExpiresAt > SYSDATETIME();
IF @@ROWCOUNT = 0 BEGIN ROLLBACK; /* আগেই চলেছে বা মেয়াদ শেষ */ RETURN; END

UPDATE w SET
    FreeUsed    = CASE WHEN w.PeriodMonth = @Month THEN w.FreeUsed ELSE 0 END + x.FromFree,
    PaidBalance = w.PaidBalance - (@Units - x.FromFree),
    PeriodMonth = @Month
OUTPUT inserted.FreeUsed, inserted.PaidBalance, x.FromFree INTO @after
FROM dbo.AI_School_Wallet AS w WITH (UPDLOCK, ROWLOCK)
CROSS APPLY (SELECT CASE
        WHEN @Quota - CASE WHEN w.PeriodMonth = @Month THEN w.FreeUsed ELSE 0 END >= @Units THEN @Units
        WHEN @Quota - CASE WHEN w.PeriodMonth = @Month THEN w.FreeUsed ELSE 0 END > 0
             THEN @Quota - CASE WHEN w.PeriodMonth = @Month THEN w.FreeUsed ELSE 0 END
        ELSE 0 END AS FromFree) AS x
WHERE w.SchoolID = @SchoolID AND w.IsEnabled = 1
  AND (@Quota - CASE WHEN w.PeriodMonth = @Month THEN w.FreeUsed ELSE 0 END)
      + w.PaidBalance >= @Units;
IF @@ROWCOUNT = 0 BEGIN ROLLBACK; /* ব্যালেন্স নেই → buy-pack কার্ড */ RETURN; END

INSERT INTO dbo.AI_Credit_Ledger (SchoolID, EntryType, FreeDelta, PaidDelta, FreeUsedAfter, PaidBalanceAfter, ActionID, RegistrationID)
SELECT @SchoolID, N'Consume', -FromFree, -(@Units - FromFree), FreeUsed, PaidBalance, @ActionID, @RegistrationID FROM @after;
COMMIT;
```

3. এরপর আসল service method চলবে (SMS, নোটিশ, নম্বর)। **পুরোপুরি ব্যর্থ হলে** (কিছুই সেভ বা পাঠানো হয়নি) একই পরিমাণ ফেরত যাবে: ledger-এ `Refund` এন্ট্রি, wallet আপডেট, আর action-এর স্ট্যাটাস `Failed`। আংশিক সফল হলে (যেমন কিছু SMS ব্যর্থ) কাজ কাটা থাকবে।
4. ledger-এ `UNIQUE (ActionID, EntryType)` থাকবে, যাতে একই কাজ দুইবার কাটা বা দুইবার ফেরত দেওয়া না যায়।

### ৪.৪ প্যাক কেনা: বিদ্যমান SMS রিচার্জ ফ্লো পুনর্ব্যবহার

**কোডে যা আছে (SMS রিচার্জ):**

| ধাপ | বিদ্যমান কোড |
|---|---|
| রিচার্জ পেজ | `Sikkhaloy.App/Pages/SmsRechargePage.razor` (`/sms/recharge`) → `GET api/sync/sms/recharge` → `SmsOfficeService.GetRechargeAsync` (ব্যালেন্স, রেট, `SMS_Recharge_Record`-এর ইতিহাস, `AAP_Invoice`-এর বকেয়া SMS ইনভয়েস) |
| কেনা শুরু | `POST api/sync/sms/recharge` `{Quantity}` → `SmsOfficeService.StartRechargeAsync`: দাম = qty × `PerSmsRate` (কোডে hardcoded ০.৩৬), gateway fee = দাম/১০০০ × ১৯ (১.৯%, ক্রেতার উপর যোগ হয়), `CreateShurjoPayOrderAsync`: `order_id = "SMSR_{SchoolID}_{yyyyMMddHHmmss}"`, `value1 = SchoolID`, `value2 = "SMS_RECHARGE|qty|RegistrationID|SchoolID"`, `value3 = invoice amount`, `return_url = https://hybrid.sikkhaloy.com/payment/callback`। Local mode-এ `sms.localPay` ফেরত দেয়, gateway-তে যায় না। |
| পেমেন্টের পর | `PaymentCallbackPage.razor` → `api/pay/shurjopay/callback` (`ShurjoPayController`, AllowAnonymous) → `ShurjoPayCallbackService.CompleteAsync`: প্রতি order-এর জন্য `SemaphoreSlim` lock, `VerifyPaymentAsync` (ShurjoPay verification API), `IsSuccessStatus`। তারপর note বা prefix দেখে `isSms` হলে `SaveSmsAsync`, না হলে `MarkInvoiceAsync`। |
| ব্যালেন্স যোগ | `SaveSmsAsync`: transaction-এর ভেতরে `SMS_Recharge_Record` আর `AAP_Invoice_OnlinePayment`-এ `SP_OrderID` দিয়ে `UPDLOCK,HOLDLOCK` duplicate চেক। তারপর `SMS_Recharge_Record` insert (trigger `Tr_SMS_Recharge_InsertUpdate` `SMS.SMS_Balance` বাড়ায়), আর `AAP_Invoice` (category 'SMS', paid), `AAP_Invoice_Receipt`, `AAP_Invoice_Payment_Record`, `AAP_Invoice_OnlinePayment` insert |
| ফলাফল | `BuildRedirectUrl` → `/payment-result?status=..&kind=sms` → `PaymentResultPage.razor` (kind=sms হলে `/sms/recharge`-এ ফেরত) |
| Authority-র ম্যানুয়াল রিচার্জ | `POST api/sync/authority/institutions/sms-recharge` → `AuthorityService.RechargeInstitutionSmsAsync`: `SMS_Recharge_Record` (Is_Paid=0) আর অপরিশোধিত `AAP_Invoice` (মেয়াদ ১৫ দিন) |

**AI প্যাকের জন্য একই ধাঁচে যা হবে:**

| ধাপ | নতুন (প্রস্তাবিত) |
|---|---|
| পেজ | `Sikkhaloy.App/Pages/AiBillingPage.razor` (`/ai/billing`) → `GET api/sync/ai/billing` → `AiBillingService.GetPageAsync`: ফ্রি কোটা (ব্যবহৃত/মোট), কেনা ব্যালেন্স, রিসেটের তারিখ, সক্রিয় প্যাকের তালিকা, কেনার ইতিহাস, এই মাসের ব্যবহার |
| কেনা শুরু | `POST api/sync/ai/billing/buy` `{PackId}` → `AiBillingService.StartPurchaseAsync`। দাম **DB-র `AI_Pack` থেকে** নেওয়া হবে, ক্লায়েন্টের পাঠানো দাম বিশ্বাস করা হবে না। প্রথমে `AI_Pack_Purchase` (Pending) তৈরি হবে, gateway fee-র সূত্র SMS-এর মতোই। ShurjoPay order বানানোর কোড (`CreateShurjoPayOrderAsync`) একটা শেয়ার্ড `ShurjoPayOrderClient`-এ সরিয়ে দুই জায়গা থেকেই ব্যবহার হবে। `order_id = "AIPK_{SchoolID}_{ts}"`, `value2 = "AI_PACK|PurchaseID|PackID|RegistrationID|SchoolID"`, একই `return_url`। Local mode-এ gateway-তে যাবে না। |
| পেমেন্টের পর | `ShurjoPayCallbackService.CompleteAsync`-এ তৃতীয় শাখা `isAi` যোগ হবে (note `AI_PACK|` বা prefix `AIPK_`) → `SaveAiPackAsync`। একই transaction আর `UPDLOCK` প্যাটার্ন: `AI_Pack_Purchase` (SP_OrderID/PurchaseID) Pending কিনা দেখা, **যত টাকা পাওয়া গেছে তা প্যাকের দামের সমান বা বেশি কিনা যাচাই**, তারপর Paid, `AI_School_Wallet.PaidBalance += Credits`, ledger-এ `Purchase`, আর `AAP_Invoice` (নতুন category **'AI'**, paid) + Receipt + Payment_Record + OnlinePayment। এর ফলে Authority-র Online Payment Report আর Paid Invoice পেজে এটা নিজে থেকেই দেখাবে। |
| ফলাফল | `kind=ai` → `PaymentResultPage.razor`-এ AI শাখা (`/ai/billing`-এ ফেরত) |
| Authority-র ম্যানুয়াল টপ-আপ | `POST api/sync/authority/ai/topup` → `AI_Pack_Purchase` (Source=Authority) + ledger + অপরিশোধিত `AAP_Invoice` (category AI, ১৫ দিন), **অথবা** বিনামূল্যে উপহার (`Adjust`, ইনভয়েস ছাড়া) |

**SMS ফ্লোর চেয়ে যা উন্নত করা হবে:**
- দাম hardcoded (০.৩৬-এর মতো) থাকবে না, DB থেকে আসবে।
- `SP_OrderID`-এ unique index থাকবে। SMS কোডের মতো runtime-এ কলাম আছে কিনা দেখার দরকার হবে না।
- প্রাপ্ত টাকার পরিমাণ যাচাই হবে।
- ShurjoPay credential config থেকে পড়া হবে (§৭-এর hardcoded সমস্যা)।

### ৪.৫ স্কুলের অ্যাডমিন UI: ব্যবহারের মিটার
- **পেজ `/ai/billing` ("AI ব্যালেন্স ও প্যাক"):** ফ্রি কোটার progress bar (যেমন ৬৫/১০০, রিসেট ১ নভেম্বর), কেনা ব্যালেন্স, প্যাকের কার্ড আর "কিনুন" বাটন, কেনার ইতিহাস, এই মাসের ব্যবহার (ইউজার আর কাজের ধরন অনুযায়ী, `AI_Credit_Ledger` + `AI_Agent_Action` থেকে)।
- **মেনুতে যোগ:** `EnsureAttendanceGuideline`-এর মতো হাইব্রিড-only লিংক (নেগেটিভ LinkID) SMS ক্যাটাগরিতে inject হবে। Admin ডিফল্টে পাবেন, Sub-Admin পাবেন page-access দিলে। Route permission `/ai/billing`।
- **মডেল বাছাই (একই পেজে):** চালু থাকা মডেলের কার্ড থেকে প্রতিষ্ঠানের ডিফল্ট মডেল আর ঐচ্ছিক OCR মডেল বাছাই, প্রতিটায় স্তর আর "প্রতি কাজে ×N ক্রেডিট" লেখা থাকবে (বিস্তারিত §৪.১১)।
- **চ্যাট প্যানেলের হেডারে ছোট মিটার:** "এই মাসে ফ্রি ৬৫/১০০ · কেনা ১৮০ · মডেল: 3.1 Flash-Lite ×১"।
- **ড্যাশবোর্ডে কার্ড:** SMS Remaining-এর পাশে "AI ব্যালেন্স"।

### ৪.৬ নোটিফিকেশন (৮০% আর শূন্য)
- **৮০%:** মাসের ফ্রি কোটার ৮০% শেষ হলে (যেমন ১০০-এর মধ্যে ৮০) আর কেনা ব্যালেন্স ০ থাকলে। অথবা কেনা ব্যালেন্স শেষ প্যাকের ২০%-এর নিচে নামলে।
- **শূন্য:** ফ্রি আর কেনা দুটোই শেষ হলে।
- **একবারই জানাবে:** প্রতিটা ঘটনা প্রতি মাসে বা প্রতি টপ-আপে একবার। এর জন্য wallet-এ `LowNotifiedAt` আর `ZeroNotifiedAt` থাকবে, রিসেট বা কেনার পর মুছে যাবে।
- **কোথায় দেখাবে:**
  1. MainLayout হেডারে banner (Admin আর `/ai/billing`-এর অনুমতি থাকা ইউজারদের), "প্যাক কিনুন" লিংক সহ
  2. চ্যাট প্যানেলে মেসেজ
  3. ড্যাশবোর্ড কার্ড লাল বা হলুদ
  4. (ঐচ্ছিক, সিদ্ধান্ত দরকার) প্রতিষ্ঠানের নিবন্ধিত মোবাইলে SMS
  5. Authority ড্যাশবোর্ডে "AI ব্যালেন্স শেষ এমন প্রতিষ্ঠান"-এর তালিকা
- `Notice_Admin` টেবিল সব স্কুলের জন্য একসাথে (global), তাই এখানে কাজে লাগবে না। নতুন টেবিল `AI_Billing_Notification` লাগবে।

### ৪.৭ ব্যালেন্স শেষ হলে ব্লক
- প্রি-চেকে ব্যালেন্স না থাকলে চ্যাট ইনপুটের জায়গায় দেখাবে: "এই মাসের ফ্রি কোটা আর কেনা ব্যালেন্স শেষ। [প্যাক কিনুন] → `/ai/billing`"। Admin ছাড়া অন্যদের দেখাবে: "অ্যাডমিনকে প্যাক কিনতে বলুন"।
- কনফার্মের সময় ব্যালেন্স শেষ হয়ে গেলে (অন্য ইউজার একই সময়ে ব্যবহার করলে) কাজ চলবে না। PendingAction সংরক্ষিত থাকবে, তাই প্যাক কিনে মেয়াদের (১০ মিনিট) মধ্যে আবার কনফার্ম করা যাবে।
- হাইব্রিডের সাধারণ পেজগুলো, যেমন হাতে SMS বা নোটিশ দেওয়া, আগের মতোই চলবে। শুধু AI সহকারী বন্ধ থাকবে।

### ৪.৮ প্ল্যাটফর্ম অ্যাডমিন (Authority) স্ক্রিন
নতুন পেজ `/authority/ai-billing` (AuthorityMenu-তে লিংক), endpoint `api/sync/authority/ai/*` (`AuthorityOrForbid`):
- **Settings:** ডিফল্ট মাসিক ফ্রি কোটা (**১০০ ক্রেডিট**), গ্লোবাল ডিফল্ট মডেল, বিনিময় হার (USD→৳), গড় টোকেন, মার্জিন সতর্কতার সীমা (২০%), বাতিলের নোটিশের দিন (১৪), নোটিফিকেশনের সীমা (৮০%), টুলভিত্তিক ওজন (OCR=২ ইত্যাদি), "বিলযোগ্য কাজ ছাড়া সর্বোচ্চ টার্ন" (১০), গ্লোবাল চালু/বন্ধ।
- **Packs:** প্যাক যোগ, এডিট আর বন্ধ করা (নাম, ক্রেডিটের সংখ্যা, দাম ৳, সক্রিয়, ক্রম)। ডিফল্ট seed: ২০০ ক্রেডিট = ২০০ ৳। একাধিক প্যাক রাখা যাবে, যেমন ৫০০ ক্রেডিট = ৪৫০ ৳, ১০০০ ক্রেডিট = ৮৫০ ৳। ছাড়ের প্যাক যোগ করলে প্যানেল সব মডেলের মার্জিন আবার হিসাব করে সতর্ক করবে। নতুন প্যাক যোগ, দাম বা আকার বদলানো, আর বন্ধ করা সবই এই স্ক্রিন থেকে হবে, কোড বদলাতে হবে না। দাম বদলালে আগের কেনাকাটায় প্রভাব পড়বে না, কারণ purchase row-এ তখনকার দাম রাখা থাকবে।
- **Models:** মডেল যোগ বা এডিট, চালু/বন্ধ, গুণক, টাস্ক ওজন, আসল খরচ, fallback, বাতিলের তারিখ, "টেস্ট" বাটন। লোকসান হলে লাল সতর্কতা, আর জোর করে সেভ করলে কারণসহ log থাকবে (§৪.১১)।
- **Institutions:** খোঁজা, প্রতিষ্ঠানের বর্তমান মডেল, প্রতিষ্ঠানভিত্তিক ফ্রি কোটা override (খালি = ডিফল্ট), AI চালু/বন্ধ, বর্তমান ব্যালেন্স, এই মাসের ব্যবহার, ম্যানুয়াল টপ-আপ বা সমন্বয় (নোট সহ, ledger-এ `Adjust`), ledger দেখা।
- **Reports:** মাসভিত্তিক আর মডেলভিত্তিক প্যাক বিক্রির আয় বনাম আসল LLM খরচ (`AI_Agent_Usage.CostUsd` × বিনিময় হার) বনাম ফ্রি কোটার খরচ, মানে মার্জিন।
- `AuthorityInstitutionDetailsPage.razor`-এ (SMS রিচার্জের পাশে) একটা ছোট "AI" অংশ থাকবে।

### ৪.৯ Ledger আর বিলিং টেবিল (খসড়া SQL, এখনো চালানো হয়নি)

```sql
CREATE TABLE dbo.AI_Billing_Settings (          -- একটাই row
    SettingsID               INT NOT NULL PRIMARY KEY DEFAULT 1 CHECK (SettingsID = 1),
    DefaultMonthlyFreeQuota  INT NOT NULL DEFAULT 100,
    LowBalancePercent        INT NOT NULL DEFAULT 80,
    MaxFreeTurnsPerAction    INT NOT NULL DEFAULT 10,
    ToolWeightsJson          NVARCHAR(1000) NOT NULL DEFAULT N'{"ocr_marks":2}',
    IsEnabled                BIT NOT NULL DEFAULT 1,
    UpdatedBy                INT NULL,
    UpdatedAt                DATETIME2 NOT NULL DEFAULT SYSDATETIME()
);

CREATE TABLE dbo.AI_Pack (
    PackID      INT IDENTITY(1,1) PRIMARY KEY,
    PackName    NVARCHAR(100) NOT NULL,
    Credits     INT NOT NULL CHECK (Credits > 0),   -- ক্রেডিট (কাজ নয়)
    PriceBdt    DECIMAL(10,2) NOT NULL CHECK (PriceBdt >= 0),
    IsActive    BIT NOT NULL DEFAULT 1,
    SortOrder   INT NOT NULL DEFAULT 0,
    CreatedAt   DATETIME2 NOT NULL DEFAULT SYSDATETIME()
);
-- ডিফল্ট seed (খসড়া; চালানো হবে অনুমতি নিয়ে):
-- INSERT INTO dbo.AI_Billing_Settings (SettingsID) VALUES (1);   -- DefaultMonthlyFreeQuota = 100 (DEFAULT থেকে)
-- INSERT INTO dbo.AI_Pack (PackName, Credits, PriceBdt, IsActive, SortOrder) VALUES (N'AI 200', 200, 200.00, 1, 1);
-- এরপর আরও প্যাক, দাম বা কোটা শুধু Authority প্যানেল থেকে বদলাবে

CREATE TABLE dbo.AI_School_Wallet (
    SchoolID           INT NOT NULL PRIMARY KEY,
    PeriodMonth        DATE NOT NULL,                 -- বর্তমান মাসের ১ তারিখ (Asia/Dhaka)
    FreeUsed           INT NOT NULL DEFAULT 0 CHECK (FreeUsed >= 0),
    PaidBalance        INT NOT NULL DEFAULT 0 CHECK (PaidBalance >= 0),
    FreeQuotaOverride  INT NULL CHECK (FreeQuotaOverride >= 0),   -- NULL = ডিফল্ট (১০০)
    LastPackCredits    INT NULL,                      -- ২০% নোটিফিকেশনের জন্য
    IsEnabled          BIT NOT NULL DEFAULT 1,
    LowNotifiedAt      DATETIME2 NULL,
    ZeroNotifiedAt     DATETIME2 NULL,
    UpdatedAt          DATETIME2 NOT NULL DEFAULT SYSDATETIME()
);

CREATE TABLE dbo.AI_Pack_Purchase (
    PurchaseID      INT IDENTITY(1,1) PRIMARY KEY,
    SchoolID        INT NOT NULL,
    PackID          INT NULL,
    Credits         INT NOT NULL,
    PriceBdt        DECIMAL(10,2) NOT NULL,        -- কেনার সময়ের দাম
    GatewayFeeBdt   DECIMAL(10,2) NOT NULL DEFAULT 0,
    TotalBdt        DECIMAL(10,2) NOT NULL,
    Source          NVARCHAR(20) NOT NULL,         -- ShurjoPay / Authority / Gift
    Status          NVARCHAR(20) NOT NULL,         -- Pending / Paid / Failed / Unpaid
    OrderID         NVARCHAR(64) NULL,
    SP_OrderID      NVARCHAR(64) NULL,
    InvoiceID       INT NULL,                      -- AAP_Invoice
    RegistrationID  INT NULL,
    CreatedAt       DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    PaidAt          DATETIME2 NULL
);
CREATE UNIQUE INDEX UX_AI_Pack_Purchase_SP ON dbo.AI_Pack_Purchase (SP_OrderID) WHERE SP_OrderID IS NOT NULL;
CREATE INDEX IX_AI_Pack_Purchase_School ON dbo.AI_Pack_Purchase (SchoolID, CreatedAt);

CREATE TABLE dbo.AI_Credit_Ledger (
    LedgerID          BIGINT IDENTITY(1,1) PRIMARY KEY,
    SchoolID          INT NOT NULL,
    EntryType         NVARCHAR(20) NOT NULL,       -- MonthlyReset / Consume / Refund / Purchase / Adjust
    FreeDelta         INT NOT NULL DEFAULT 0,
    PaidDelta         INT NOT NULL DEFAULT 0,
    FreeUsedAfter     INT NOT NULL,
    PaidBalanceAfter  INT NOT NULL,
    ActionID          BIGINT NULL,                 -- AI_Agent_Action
    PurchaseID        INT NULL,                    -- AI_Pack_Purchase
    RegistrationID    INT NULL,
    Note              NVARCHAR(300) NULL,
    CreatedAt         DATETIME2 NOT NULL DEFAULT SYSDATETIME()
);
CREATE UNIQUE INDEX UX_AI_Credit_Ledger_Action ON dbo.AI_Credit_Ledger (ActionID, EntryType) WHERE ActionID IS NOT NULL;
CREATE INDEX IX_AI_Credit_Ledger_School ON dbo.AI_Credit_Ledger (SchoolID, CreatedAt);

CREATE TABLE dbo.AI_Billing_Notification (
    NotificationID  BIGINT IDENTITY(1,1) PRIMARY KEY,
    SchoolID        INT NOT NULL,
    Kind            NVARCHAR(20) NOT NULL,         -- Low80 / Zero / Purchased
    Message         NVARCHAR(300) NOT NULL,
    CreatedAt       DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    ReadAt          DATETIME2 NULL
);

-- AAP_Invoice-এর জন্য নতুন category (SMS-এর মতো):
-- INSERT INTO dbo.AAP_Invoice_Category (InvoiceCategory) VALUES (N'AI');   -- কলামের নাম চালানোর আগে মিলিয়ে নিতে হবে
```

- wallet row না থাকলে প্রথম ব্যবহারে বানানো হবে (`PeriodMonth` = এই মাস, `FreeUsed` = ০)।
- `PaidBalance`-এ `CHECK >= 0` থাকায় DB নিজেও ঋণাত্মক ব্যালেন্স আটকাবে।

**মডেল ক্যাটালগ (খসড়া SQL, পুরোটা comment করা, চালানো হয়নি):**

```sql
-- CREATE TABLE dbo.AI_Model (
--     ModelID              INT IDENTITY(1,1) PRIMARY KEY,
--     Provider             NVARCHAR(20)  NOT NULL,          -- Google / OpenAI / Anthropic (key আসে কনফিগ থেকে, DB-তে নয়)
--     ProviderModelId      NVARCHAR(100) NOT NULL,          -- যেমন gemini-3.1-flash-lite
--     DisplayNameBn        NVARCHAR(100) NOT NULL,
--     DisplayNameEn        NVARCHAR(100) NOT NULL,
--     DescriptionBn        NVARCHAR(300) NULL,
--     Tier                 NVARCHAR(20)  NOT NULL,          -- Basic / Smart / SmartPlus / Pro
--     TierLabelBn          NVARCHAR(30)  NOT NULL,          -- সাধারণ / স্মার্ট / স্মার্ট+ / প্রো
--     Multiplier           INT NOT NULL DEFAULT 1 CHECK (Multiplier >= 1),
--     TaskWeightsJson      NVARCHAR(500) NULL,              -- মডেলভিত্তিক ওজন, যেমন {"ocr_marks":2}
--     SupportsVision       BIT NOT NULL DEFAULT 0,
--     SupportsAudio        BIT NOT NULL DEFAULT 0,
--     CostInputPer1M       DECIMAL(10,4) NOT NULL,          -- USD, মার্জিন রিপোর্টের জন্য
--     CostOutputPer1M      DECIMAL(10,4) NOT NULL,
--     NextCostInputPer1M   DECIMAL(10,4) NULL,              -- ঘোষিত ভবিষ্যৎ দাম
--     NextCostOutputPer1M  DECIMAL(10,4) NULL,
--     NextCostFrom         DATE NULL,
--     CostVerifiedAt       DATE NULL,
--     Status               NVARCHAR(20) NOT NULL DEFAULT N'Active',   -- Active / Deprecated / Retired / ProviderError
--     DeprecatedAfter      DATE NULL,
--     FallbackModelID      INT NULL REFERENCES dbo.AI_Model(ModelID),
--     IsEnabled            BIT NOT NULL DEFAULT 0,
--     SortOrder            INT NOT NULL DEFAULT 0,
--     UpdatedBy            INT NULL,
--     UpdatedAt            DATETIME2 NOT NULL DEFAULT SYSDATETIME()
-- );
-- CREATE UNIQUE INDEX UX_AI_Model_Provider ON dbo.AI_Model (Provider, ProviderModelId);
--
-- -- পরে (Phase 3+): ইউজার বা টাস্কভিত্তিক override
-- CREATE TABLE dbo.AI_Model_Override (
--     OverrideID      INT IDENTITY(1,1) PRIMARY KEY,
--     SchoolID        INT NOT NULL,
--     RegistrationID  INT NULL,          -- NULL = পুরো স্কুল
--     TaskKey         NVARCHAR(50) NULL, -- NULL = সব কাজ, যেমন ocr_marks
--     ModelID         INT NOT NULL REFERENCES dbo.AI_Model(ModelID)
-- );
--
-- -- Authority-র সব বিলিং/মডেল/কোটা পরিবর্তনের log (লোকসানে জোর করে সেভ করা সহ)
-- CREATE TABLE dbo.AI_Billing_AuditLog (
--     AuditID         BIGINT IDENTITY(1,1) PRIMARY KEY,
--     EntityType      NVARCHAR(30) NOT NULL,   -- Settings / Pack / Model / SchoolQuota / SchoolModel / ModelAutoSwitch
--     EntityID        INT NULL,
--     SchoolID        INT NULL,
--     OldJson         NVARCHAR(MAX) NULL,
--     NewJson         NVARCHAR(MAX) NULL,
--     NegativeMarginForced BIT NOT NULL DEFAULT 0,
--     Reason          NVARCHAR(300) NULL,
--     RegistrationID  INT NOT NULL,
--     CreatedAt       DATETIME2 NOT NULL DEFAULT SYSDATETIME()
-- );
--
-- -- আগের টেবিলগুলোতে যোগ হবে (উপরের খসড়া CREATE-এ বসিয়ে দেওয়া যাবে):
-- --   AI_Billing_Settings: DefaultModelID INT NULL, UsdToBdt DECIMAL(10,4) NOT NULL DEFAULT 123.20,
-- --                        AvgInputTokens INT NOT NULL DEFAULT 8000, AvgOutputTokens INT NOT NULL DEFAULT 800,
-- --                        OcrInputTokens INT NOT NULL DEFAULT 4000, OcrOutputTokens INT NOT NULL DEFAULT 1500,
-- --                        MinMarginWarnPercent INT NOT NULL DEFAULT 20, DeprecationNoticeDays INT NOT NULL DEFAULT 14
-- --   AI_School_Wallet:    DefaultModelID INT NULL, OcrModelID INT NULL
-- --   AI_Agent_Action:     ModelID INT NULL, TaskWeight INT NULL, Multiplier INT NULL, CreditsQuoted INT NULL
-- --   AI_Agent_Usage:      ModelID INT NULL (Model নামের কলামের পাশাপাশি)
-- --   AI_Credit_Ledger:    ModelID INT NULL, TaskWeight INT NULL, Multiplier INT NULL
--
-- -- ডিফল্ট ক্যাটালগ seed (দাম: §৬.১, যাচাই ৬ অক্টোবর ২০২৬; FallbackModelID পরে UPDATE করে বসবে):
-- INSERT INTO dbo.AI_Model (Provider, ProviderModelId, DisplayNameBn, DisplayNameEn, Tier, TierLabelBn, Multiplier, SupportsVision, CostInputPer1M, CostOutputPer1M, NextCostInputPer1M, NextCostOutputPer1M, NextCostFrom, CostVerifiedAt, IsEnabled, SortOrder) VALUES
--  (N'Google', N'gemini-3.1-flash-lite', N'জেমিনি ৩.১ ফ্ল্যাশ-লাইট (প্রস্তাবিত)', N'Gemini 3.1 Flash-Lite', N'Basic',     N'সাধারণ',   1, 1, 0.25, 1.50, NULL, NULL, NULL,         '2026-10-06', 1, 1),
--  (N'Google', N'gemini-2.5-flash-lite', N'জেমিনি ২.৫ ফ্ল্যাশ-লাইট (সাশ্রয়ী)', N'Gemini 2.5 Flash-Lite', N'Basic',     N'সাধারণ',   1, 1, 0.10, 0.40, NULL, NULL, NULL,         '2026-10-06', 1, 2),
--  (N'OpenAI', N'gpt-6-luna',            N'জিপিটি-৬ লুনা',                     N'GPT-6 Luna',            N'Basic',     N'সাধারণ',   1, 0, 0.10, 0.50, NULL, NULL, NULL,         '2026-10-06', 1, 3),
--  (N'OpenAI', N'gpt-4o-mini',           N'জিপিটি-৪o মিনি',                    N'GPT-4o mini',           N'Basic',     N'সাধারণ',   1, 1, 0.15, 0.60, NULL, NULL, NULL,         '2026-10-06', 0, 4),
--  (N'Google', N'gemini-3.5-flash-lite', N'জেমিনি ৩.৫ ফ্ল্যাশ-লাইট',            N'Gemini 3.5 Flash-Lite', N'Smart',     N'স্মার্ট',   2, 1, 0.30, 2.50, NULL, NULL, NULL,         '2026-10-06', 1, 5),
--  (N'Google', N'gemini-2.5-flash',      N'জেমিনি ২.৫ ফ্ল্যাশ',                 N'Gemini 2.5 Flash',      N'Smart',     N'স্মার্ট',   2, 1, 0.30, 2.50, NULL, NULL, NULL,         '2026-10-06', 0, 6),
--  (N'Google', N'gemini-3.7-flash',      N'জেমিনি ৩.৭ ফ্ল্যাশ',                 N'Gemini 3.7 Flash',      N'SmartPlus', N'স্মার্ট+',  3, 1, 0.75, 3.75, 1.50, 7.50, '2027-01-01', '2026-10-06', 1, 7),
--  (N'Anthropic', N'claude-haiku-4-5',   N'ক্লড হাইকু ৪.৫',                     N'Claude Haiku 4.5',      N'SmartPlus', N'স্মার্ট+',  3, 0, 1.00, 5.00, NULL, NULL, NULL,         '2026-10-06', 0, 8),
--  (N'OpenAI', N'gpt-6.1-sol',           N'জিপিটি-৬.১ সল (প্রো)',               N'GPT-6.1 Sol',           N'Pro',       N'প্রো',      6, 0, 2.00, 10.00, NULL, NULL, NULL,        '2026-10-06', 0, 9);
-- -- নোট: SupportsVision=0 মানে "এখনো যাচাই হয়নি"। Anthropic আর OpenAI-এর model id চালুর আগে provider-এর পেজে মিলিয়ে নিতে হবে।
```

### ৪.১০ মডেল ক্যাটালগ, গুণক (multiplier) আর মার্জিন
হিসাবের ভিত্তি:
- প্রতি কাজে input প্রায় ৮,০০০ আর output প্রায় ৮০০ টোকেন (§৬.২)
- দাম §৬.১-এর যাচাই করা অফিশিয়াল দাম; ১ USD = ১২৩.২০ ৳
- ডিফল্ট প্যাক ২০০ ক্রেডিট = ২০০ ৳, মানে **প্রতি ক্রেডিট ১.০০ ৳**
- **কাটা ক্রেডিট = কাজের ওজন × মডেলের গুণক**
- মার্জিন = ১ − (আসল খরচ ÷ গুণক)

**প্রস্তাবিত ডিফল্ট ক্যাটালগ** (seed। সব মান পরে Authority প্যানেল থেকে বদলানো যাবে):

| # | Model (Provider) | স্তর (Tier) | গুণক | আসল খরচ/কাজ | ×১ হলে মার্জিন | প্রস্তাবিত গুণকে মার্জিন | ডিফল্ট অবস্থা | Fallback |
|---|---|---|---|---|---|---|---|---|
| 1 | **Gemini 3.1 Flash-Lite** (Google) | সাধারণ (**প্রস্তাবিত ডিফল্ট**) | ×১ | প্রায় ০.৩৯ ৳ | ৬১% | **৬১%** | চালু | Gemini 2.5 Flash-Lite |
| 2 | Gemini 2.5 Flash-Lite (Google) | সাধারণ (সাশ্রয়ী) | ×১ | প্রায় ০.১৪ ৳ | ৮৬% | **৮৬%** | চালু | Gemini 3.1 Flash-Lite |
| 3 | gpt-6-luna (OpenAI) | সাধারণ | ×১ | প্রায় ০.১৫ ৳ | ৮৫% | **৮৫%** | চালু (OpenAI key থাকলে) | Gemini 3.1 Flash-Lite |
| 4 | gpt-4o-mini (OpenAI) | সাধারণ | ×১ | প্রায় ০.২১ ৳ | ৭৯% | **৭৯%** | বন্ধ (পুরোনো মডেল) | gpt-6-luna |
| 5 | Gemini 3.5 Flash-Lite (Google) | স্মার্ট | ×২ | প্রায় ০.৫৪ ৳ | ৪৬% | **৭৩%** | চালু | Gemini 3.1 Flash-Lite |
| 6 | Gemini 2.5 Flash (Google) | স্মার্ট | ×২ | প্রায় ০.৫৪ ৳ | ৪৬% | **৭৩%** | বন্ধ (পুরোনো প্রজন্ম) | Gemini 3.5 Flash-Lite |
| 7 | Gemini 3.7 Flash (Google) | স্মার্ট+ | ×৩ (**২০২৭ থেকে ×৪**) | প্রায় ১.১১ ৳ (২০২৬), ২.২২ ৳ (২০২৭) | **লোকসান** (−১১% / −১২২%) | **৬৩%** (২০২৬, ×৩); ২০২৭-এ ×৩ হলে ২৬%, ×৪ হলে ৪৫% | চালু | Gemini 3.5 Flash-Lite |
| 8 | Claude Haiku 4.5 (Anthropic) | স্মার্ট+ | ×৩ | প্রায় ১.৪৮ ৳ | **লোকসান** (−৪৮%) | **৫১%** | বন্ধ (Anthropic adapter আর key লাগবে) | Gemini 3.7 Flash |
| 9 | gpt-6.1-sol (OpenAI) | প্রো | ×৬ | প্রায় ২.৯৬ ৳ | **বড় লোকসান** (−১৯৬%) | **৫১%** | বন্ধ (পরে চালু হবে) | Gemini 3.7 Flash |

- **×১ গুণকে লোকসান হবে:** Gemini 3.7 Flash, Claude Haiku 4.5, gpt-6.1-sol। এগুলোর গুণক প্যানেল কখনো ×১ রাখতে দেবে না, সতর্কবার্তা দেবে (§৪.১১)।
- Gemini 3.5 Flash-Lite আর 2.5 Flash ×১-এও লাভজনক (৪৬%)। তবে এগুলো "স্মার্ট" স্তরের, তাই ×২ প্রস্তাব।
- **Gemini 3.7 Flash:** Google-এর পেজ অনুযায়ী ১ জানুয়ারি ২০২৭ থেকে দাম দ্বিগুণ হবে। ক্যাটালগে "পরবর্তী দাম" আর তার কার্যকর তারিখ রাখা থাকবে। প্যানেল ডিসেম্বরেই ×৪ করার কথা মনে করিয়ে দেবে।
- **OCR (মার্কশিটের ছবি, ওজন ২):** ক্রেডিট = ২ × গুণক।

| Model | ছবিপ্রতি আসল খরচ | কাটবে | মার্জিন |
|---|---|---|---|
| Gemini 3.1 Flash-Lite | প্রায় ০.৪০ ৳ | ২ ক্রেডিট | প্রায় ৮০% |
| Gemini 3.7 Flash (২০২৬) | প্রায় ১.০৬ ৳ | ৬ ক্রেডিট | প্রায় ৮২% |
| Gemini 3.7 Flash (২০২৭, ×৩) | প্রায় ২.১৩ ৳ | ৬ ক্রেডিট | প্রায় ৬৫% |
| Claude Haiku 4.5 / gpt-6.1-sol | — | — | প্রায় ৭৬% |

- **ফ্রি ১০০ ক্রেডিটে প্ল্যাটফর্মের খরচ:** প্রতি স্কুলে মাসে ডিফল্ট মডেলে প্রায় ৩৯ ৳। দামি মডেলে ক্রেডিট দ্রুত শেষ হয় (×৩ মডেলে প্রায় ৩৩টা কাজ), তাই খরচ সর্বোচ্চ প্রায় ৪৯–৭৪ ৳। ৫০০ স্কুল ডিফল্ট মডেলে থাকলে মোট প্রায় ১৯,৫০০ ৳/মাস। প্রতি ডিফল্ট প্যাকে মার্জিন প্রায় ১২১ ৳, তাই এটা তুলতে মাসে প্রায় ১৬০টা প্যাক বিক্রি লাগবে।
- **Vision সাপোর্ট:** OCR শুধু সেই মডেলে চলবে যার `SupportsVision = 1`। Gemini মডেল আর gpt-4o-mini-তে ছবি input অফিশিয়ালি নিশ্চিত। gpt-6-luna, gpt-6.1-sol আর Claude Haiku 4.5-এর জন্য চালুর আগে যাচাই করতে হবে। মডেল vision না পারলে OCR কাজটা স্কুলের "OCR fallback মডেলে" যাবে, আর কার্ডে সেটা দেখাবে।
- দীর্ঘ কথোপকথন বা বড় রিপোর্টে টোকেন বেশি লাগে। তাই প্রতি কাজে টোকেনের সীমা থাকবে, আর Reports ট্যাবে মডেলভিত্তিক **আসল** মার্জিন (`AI_Agent_Usage`-এর আসল টোকেন থেকে) দেখাবে।
- Gateway fee (১.৯%) SMS ফ্লোর মতোই ক্রেতার উপর যোগ হবে, তাই এটা মার্জিন থেকে কাটবে না।

### ৪.১১ মডেল বেছে নেওয়া আর মডেলভিত্তিক চার্জ
**ক্যাটালগ (`AI_Model`, পুরোপুরি DB-চালিত):**
- প্রতিটা মডেলের তথ্য: provider, provider-এর model id, বাংলা আর ইংরেজি নাম ও বিবরণ, স্তর (সাধারণ/স্মার্ট/প্রো), চালু/বন্ধ, গুণক, ঐচ্ছিক টাস্কভিত্তিক ওজন (যেমন `{"ocr_marks":2}`), vision বা audio সাপোর্ট, আসল খরচ (প্রতি ১০ লাখ input/output টোকেন, USD) আর "পরবর্তী দাম ও তারিখ", দাম যাচাইয়ের তারিখ, স্ট্যাটাস আর বাতিলের তারিখ, fallback মডেল, ক্রম।
- **API key DB-তে থাকবে না।** `AI_Model.Provider` থেকে কনফিগের `Agent:Providers:{Google|OpenAI|Anthropic}:ApiKey` পড়া হবে (§৩.৯)। কোনো provider-এর key না থাকলে তার মডেল চালু করা যাবে না। প্যানেল শুধু "key আছে/নেই" দেখাবে, key নিজে কখনো দেখাবে না।

**ক্রেডিটের হিসাব:**
- কাটা ক্রেডিট = `TaskWeight(task, model) × Multiplier(model)`। দুটোই পূর্ণসংখ্যা, প্রতিটা অন্তত ১।
- ওজনের উৎস এই ক্রমে: মডেলের নিজের `TaskWeightsJson` → না থাকলে `AI_Billing_Settings.ToolWeightsJson` → না থাকলে ১।
- **কোট (quote) করে আটকে রাখা হবে:** PendingAction তৈরির সময় মডেল, ওজন, গুণক আর `CreditsQuoted` `AI_Agent_Action`-এ লেখা হবে। কনফার্মে ঠিক ততটাই কাটবে (§৪.৩-এর SQL-এ `@Units = CreditsQuoted`)। মাঝখানে Authority গুণক বদলালেও আগের কোটে প্রভাব পড়বে না।
- Read কাজের (প্রশ্নোত্তর) কোট হবে LLM call-এর আগে। প্রি-চেকে ব্যালেন্স ≥ কোট হতে হবে।
- **নিয়ম: স্কুল কখনো কার্ডে দেখানো ক্রেডিটের বেশি দেবে না।**

**স্কুলের অ্যাডমিন UI (`/ai/billing`):**
- "মডেল বাছাই" অংশে চালু থাকা মডেলের কার্ড দেখাবে: বাংলা নাম, স্তরের ব্যাজ, "প্রতি কাজে ×N ক্রেডিট", ছোট বিবরণ (যেমন "দ্রুত ও সাশ্রয়ী" / "হাতের লেখা পড়তে ভালো"), আর "২০০ ক্রেডিটের প্যাকে প্রায় কতগুলো কাজ"।
- Admin বা `/ai/billing`-এর অনুমতি থাকা ইউজার প্রতিষ্ঠানের **ডিফল্ট মডেল** বাছবেন (`AI_School_Wallet.DefaultModelID`)। পরিবর্তন audit log-এ যাবে। কিছু বাছা না থাকলে গ্লোবাল ডিফল্ট (`AI_Billing_Settings.DefaultModelID`) চলবে।
- ঐচ্ছিক "OCR-এর জন্য আলাদা মডেল" রাখা যাবে, যেমন সাধারণ কাজে 3.1 Flash-Lite আর ছবিতে 3.7 Flash।
- **পরে (Phase 3 বা তার পরে):** ইউজারভিত্তিক বা টাস্কভিত্তিক override (`AI_Model_Override`, খসড়া SQL নিচে)। Admin ঠিক করে দিতে পারবেন কোন ইউজার কোন মডেল পাবেন।

**কনফার্মেশন কার্ড:** "মডেল: Gemini 3.1 Flash-Lite (সাধারণ) · এই কাজে কাটবে ১ × ১ = **১ ক্রেডিট** · ফ্রি বাকি ৩৫/১০০ · কেনা ১৮০"। দামি মডেলে দেখাবে, যেমন "Gemini 3.7 Flash (স্মার্ট+) · ২ × ৩ = **৬ ক্রেডিট**"। চ্যাট প্যানেলের হেডারেও বর্তমান মডেলের নাম আর গুণক দেখাবে।

**Authority প্যানেল, "Models" ট্যাব (`/authority/ai-billing`):**
- মডেল যোগ বা এডিট, চালু/বন্ধ, গুণক, টাস্ক ওজন, আসল খরচ আর "পরবর্তী দাম ও তারিখ", fallback, ক্রম, গ্লোবাল ডিফল্ট মডেল।
- **"টেস্ট" বাটন:** ২০–৩০টা বাংলা নমুনা কমান্ড চালিয়ে দেখাবে tool-calling ঠিক হচ্ছে কিনা। মডেল চালু করার আগে এটা পাস করা উচিত।
- **মার্জিনের সতর্কতা (সেভ করার আগে সার্ভারেই হিসাব):**
  - প্রত্যাশিত খরচ = (AvgInput × CostIn + AvgOutput × CostOut) ÷ ১০,০০,০০০ × `UsdToBdt`। বর্তমান আর "পরবর্তী" দামের মধ্যে যেটা বেশি সেটা ধরা হবে। প্রতিটা টাস্ক প্রোফাইলের জন্য আলাদা হিসাব (সাধারণ কাজ, OCR)।
  - আয়/ক্রেডিট = চালু থাকা প্যাকগুলোর মধ্যে **সবচেয়ে কম** ৳/ক্রেডিট (worst case, যেমন বড় প্যাকে ছাড় থাকলে)।
  - মার্জিন = ১ − খরচ ÷ (ওজন × গুণক × আয়/ক্রেডিট)।
  - মার্জিন **< ০% হলে লাল:** সাধারণভাবে সেভ হবে না। "জেনেশুনে লোকসানে চালু করছি" টিক আর কারণ লিখলে তবেই সেভ হবে, আর সেটা `AI_Billing_AuditLog`-এ থাকবে।
  - মার্জিন **< `MinMarginWarnPercent` (ডিফল্ট ২০%) হলে হলুদ** সতর্কতা।
  - প্যাকের দাম বদলালে, বিনিময় হার বা গড় টোকেন আপডেট করলে, বা "পরবর্তী দাম"-এর তারিখ ঘনিয়ে এলে সব চালু মডেলের মার্জিন আবার হিসাব হবে, আর সতর্কতার তালিকা দেখাবে।
- **Reports ট্যাব:** মডেলভিত্তিক ব্যবহার, আয় (ক্রেডিট × ৳), আসল খরচ আর আসল মার্জিন।

### ৪.১২ মডেল বাতিল (deprecation) আর fallback
- `AI_Model.Status`: `Active` / `Deprecated` / `Retired`, সাথে `DeprecatedAfter` (তারিখ) আর `FallbackModelID`। Provider-রা নিয়মিত পুরোনো মডেল বন্ধ করে। যেমন Google-এর pricing পেজে লেখা `gemini-2.5-flash-image` ২ অক্টোবর ২০২৬-এ বন্ধ হয়েছে।
- **মডেল ঠিক করার ক্রম (প্রতিটা অনুরোধে):** ইউজার বা টাস্ক override (পরে) → স্কুলের ডিফল্ট → (মডেল বন্ধ, `Retired`, বা আজকের তারিখ ≥ `DeprecatedAfter` হলে) `FallbackModelID`-এর চেইন (সর্বোচ্চ ৩ ধাপ, চক্র আটকানো থাকবে) → গ্লোবাল ডিফল্ট → কিছুই না পেলে "AI সাময়িকভাবে বন্ধ" বার্তা।
- **আগাম নোটিশ:** Authority কোনো মডেলকে `Deprecated` করে তারিখ দিলে, যেসব স্কুল সেটা ব্যবহার করছে তারা ১৪ দিন আগে থেকে banner দেখবে ("আপনার মডেল X বন্ধ হচ্ছে, Y-তে যাবে, চাইলে এখনই বদলান")। ঐ তারিখে নিজে থেকেই fallback-এ যাবে, আর audit-এ `ModelAutoSwitch` থাকবে।
- **হঠাৎ ব্যর্থতা:** provider থেকে "model not found" বা deprecated এরর এলে একবার fallback মডেলে চেষ্টা হবে। Authority-কে সতর্কবার্তা যাবে, মডেলটা `ProviderError` হিসেবে চিহ্নিত হবে, আর স্কুল-ড্যাশবোর্ডে জানাবে। চার্জ হবে কোট আর fallback মডেলের ক্রেডিটের মধ্যে **যেটা কম**।
- কনফার্মে কোনো LLM call লাগে না (Write কাজ বিদ্যমান service দিয়ে চলে), তাই কোট হয়ে যাওয়া কাজ মডেল বন্ধ হলেও কনফার্ম করা যাবে।

---

## ৫. পর্যায় (Phase) আর আনুমানিক সময়

> সময় দেওয়া হয়েছে কর্মদিবসে। কোড আমি লিখব, আপনি প্রতিটা ধাপ টেস্ট সার্ভারে পরীক্ষা করবেন। লাইভে দেওয়ার আগে localhost-এ (`LocalOfficeMode`, যেখানে আসল SMS যায় না) পুরো পরীক্ষা হবে।

### Phase 1: ভিত্তি + বিলিং + মডেল ক্যাটালগ + নোটিশ / হোমওয়ার্ক / SMS (খসড়া, কনফার্ম, পাঠানো): **১৯–২৪ দিন**

**ক) এজেন্টের ভিত্তি আর কাজ: ৮–১০ দিন**
- AgentController, AgentService, LlmClient, PermissionGuard, audit আর usage টেবিল, rate limit, কনফিগ
- চ্যাট প্যানেল (MainLayout + TeacherLayout), কনফার্মেশন কার্ড, সম্পাদনা আর বাতিল
- Tool: `find_class`, `get_sms_balance`, `draft/save_notice`, `draft/save_homework`, `draft/send_sms` (ক্লাস, সেকশন, শিক্ষক, contact গ্রুপ অনুযায়ী)
- **গ্রহণযোগ্যতার শর্ত:** Sub-Admin-এর `/sms/send` অনুমতি না থাকলে এজেন্ট SMS প্রস্তাবই করবে না। কনফার্ম ছাড়া কিছুই যাবে না। ব্যালেন্স কম হলে আটকাবে। Audit-এ সব কাজ থাকবে।

**খ) প্রিপেইড AI বিলিং + মডেল ক্যাটালগ (§৪): ১১–১৪ দিন**
- বিলিং টেবিল, wallet, atomic কাটা আর ফেরত, lazy মাসিক রিসেট, প্রি-চেক আর ব্লক: ২–৩ দিন
- প্যাক কেনা (শেয়ার্ড `ShurjoPayOrderClient`, callback-এ `isAi` শাখা, `SaveAiPackAsync`, `kind=ai` ফলাফল পেজ, local mode): ২ দিন
- স্কুলের `/ai/billing` পেজ, চ্যাট মিটার, ড্যাশবোর্ড কার্ড, ৮০% আর শূন্য নোটিফিকেশন: ১.৫–২ দিন
- Authority-র `/authority/ai-billing` (Settings, Packs, Institutions override আর টপ-আপ, Reports): ২ দিন
- **মডেল ক্যাটালগ (নতুন, ৩–৪ দিন):** `AI_Model` টেবিল আর seed, `AiModelResolver` (fallback চেইন, কোট), দ্বিতীয় provider adapter (Gemini + OpenAI), স্কুলের মডেল বাছাই আর কার্ডে মডেল ও ক্রেডিট, Authority-র Models ট্যাব আর মার্জিনের সতর্কতা, বাতিলের নোটিশ আর auto-switch। Anthropic (Claude) adapter লাগলে আরও ১ দিন।
- টেস্ট: একসাথে দুই কনফার্মে দুইবার কাটা যায় কিনা, একই callback দুইবার এলে কী হয়, মাস বদলের সময়, ব্যর্থ হলে ফেরত, sandbox পেমেন্ট: ১ দিন
- **শর্ত:** নতুন প্রতিষ্ঠান ১০০ ফ্রি ক্রেডিট পাবে। ×১ মডেলে ১০১তম কাজে প্যাক কেনার কার্ড আসবে, আর ×৩ মডেলে ৩৪তম কাজে। কার্ডে দেখানো ক্রেডিটের বেশি কখনো কাটবে না। মডেল বন্ধ করলে স্কুল নিজে থেকেই fallback মডেলে যাবে। লোকসানের গুণক সতর্কতা ছাড়া সেভ হবে না। একই ShurjoPay callback দুইবার এলেও ব্যালেন্স একবারই বাড়বে। ব্যালেন্স কখনো ঋণাত্মক হবে না।

### Phase 2: টাইপ করে নম্বর এন্ট্রি: **৫–৭ দিন**
- Tool: `list_exams`, `get_marks_sheet`, `draft_marks`, `save_marks`
- রোল বা নাম থেকে ছাত্র মেলানো, বাংলা সংখ্যা, অনুপস্থিত, sub-exam (CQ/MCQ/Practical), full mark চেক, lock চেক
- শিক্ষক শুধু নিজের বিষয়ে (`OwnsSubjectAsync`) এন্ট্রি দিতে পারবেন
- কার্ডে টেবিল দেখিয়ে সম্পাদনা করা যাবে, আগের নম্বর থাকলে কী বদলাবে তা দেখাবে
- **শর্ত:** ৪০ জনের একটা তালিকা এক মেসেজে দিলে ঠিকভাবে সেভ হবে। ভুল রোল বা full mark-এর বেশি নম্বর থাকলে আটকাবে।

### Phase 3: ছবি থেকে নম্বর (OCR) + ভয়েস + প্রশ্নোত্তর / রিপোর্ট: **১০–১৫ দিন**
- **ছবি থেকে নম্বর (৪–৫ দিন):** মার্কশিট বা "Marks Collect Paper" (`/exam/collect-paper`)-এর ছবি → vision model → রোল আর নম্বরের টেবিল → Phase 2-এর একই কনফার্মেশন কার্ড। হাতের লেখায় ভুল হতে পারে, তাই অনিশ্চিত ঘর হলুদ করে দেখাবে। হাইব্রিডের collect-paper প্রিন্ট ফরম্যাট ব্যবহার করলে নির্ভুলতা অনেক বাড়বে।
- **ভয়েস (৩–৪ দিন):** ব্রাউজারে MediaRecorder → `/api/agent/transcribe` → লেখা। MAUI অ্যাপে `RECORD_AUDIO` আর `AppWebChromeClient`-এ শুধু নিজের host-এর জন্য মাইক্রোফোন অনুমতি, নতুন Play রিলিজ আর Data safety আপডেট। বিকল্প হিসেবে Gboard ভয়েস টাইপিং (বিনা খরচে, এখনই ব্যবহার করা যায়)।
- **প্রশ্নোত্তর আর রিপোর্ট (৪–৬ দিন):** আজকের উপস্থিত/অনুপস্থিত, অনুপস্থিতদের অভিভাবককে SMS, বকেয়া তালিকা আর রিমাইন্ডার SMS, ফলাফলের সারাংশ আর মেধা তালিকা, "রিপোর্ট পেজ খুলে দাও" (লিংক)।

**মোট: প্রায় ৩৪–৪৬ কর্মদিবস (৭–৯ সপ্তাহ)।** এর মধ্যে Phase 1 প্রায় ১৯–২৪ দিন, Phase 2 ৫–৭ দিন, Phase 3 ১০–১৫ দিন। আপনার পরীক্ষা আর মতামতের গতির উপর এটা নির্ভর করবে।

---

## ৬. মাসিক খরচের হিসাব

### ৬.১ অফিশিয়াল দাম (যাচাই: ৬ অক্টোবর ২০২৬; প্রতি ১০ লাখ টোকেন, USD, Standard/paid tier)

| Model | Input | Output | সূত্র |
|---|---|---|---|
| Gemini 2.5 Flash-Lite | $0.10 | $0.40 | ai.google.dev/gemini-api/docs/pricing |
| Gemini 3.1 Flash-Lite | $0.25 (text/image) | $1.50 | একই |
| Gemini 3.5 Flash-Lite | $0.30 | $2.50 | একই |
| Gemini 2.5 Flash | $0.30 | $2.50 | একই |
| Gemini 3.7 Flash | $0.75 (৩১ ডিসেম্বর ২০২৬ পর্যন্ত), **$1.50 (১ জানুয়ারি ২০২৭ থেকে)** | $3.75, **$7.50 (২০২৭ থেকে)** | একই |
| OpenAI gpt-6-luna (ছোট মডেল) | $0.10 | $0.50 | platform.openai.com/docs/pricing |
| OpenAI gpt-4o-mini | $0.15 | $0.60 | platform.openai.com/docs/models/gpt-4o-mini (পেজে "Batch API price" লেবেল আছে, তাই এটা standard নাকি batch দাম তা স্পষ্ট না) |
| OpenAI gpt-6.1-sol (বড় মডেল) | $2.00 | $10.00 | platform.openai.com/docs/pricing |
| Claude Haiku 4.5 | $1.00 | $5.00 | docs.anthropic.com/en/docs/about-claude/pricing |

ভয়েস থেকে লেখা: Gemini 3.5 Transcribe প্রায় $0.005/মিনিট (blended), OpenAI gpt-4o-mini-transcribe প্রায় $0.003/মিনিট, gpt-transcribe প্রায় $0.0045/মিনিট (একই সূত্র)।
বিনিময় হার: ১ USD = ১২৩.২০ টাকা (Bangladesh Bank weighted average, ৬ অক্টোবর ২০২৬, bb.org.bd)।

> Google-এর পেজে লেখা "Last updated 2026-10-01 UTC"। দাম বদলাতে পারে, চালু করার আগে আবার মিলিয়ে নিতে হবে। Gemini 2.5 সিরিজ পুরোনো প্রজন্মের, কিছুদিন পর বন্ধ হয়ে যেতে পারে।

### ৬.২ টোকেনের অনুমান (প্রতি কাজে)
- System prompt + tool schema: প্রায় ২,৫০০ টোকেন। একটা কাজে গড়ে ২–৩টা LLM call হয়, আর প্রতিবার আগের কথোপকথন আবার পাঠাতে হয়।
- **গড় কাজ (নোটিশ, SMS খসড়া, প্রশ্ন):** input প্রায় ৮,০০০ আর output প্রায় ৮০০ টোকেন। বাংলা লেখায় ইংরেজির চেয়ে বেশি টোকেন লাগে, সেটা এই হিসাবে ধরা আছে।
- **৪০ জনের নম্বর এন্ট্রি:** প্রায় ৮,০০০ input + ১,০০০ output, মানে গড়ের কাছাকাছি।
- **একটা মার্কশিটের ছবি:** প্রায় ৪,০০০ input (ছবি সহ) + ১,৫০০ output।

### ৬.৩ প্রতি স্কুলে মাসিক খরচ (শুধু AI, SMS খরচ আলাদা)

**অনুমান A (অনেক বেশি ব্যবহার):** ২০ জন শিক্ষক/স্টাফ × দিনে ৩০টা কাজ × মাসে ২৬ কর্মদিবস = **১৫,৬০০ কাজ/মাস**
**অনুমান B (বাস্তবসম্মত):** ২০ জন × দিনে ১০টা কাজ × ২৬ দিন = **৫,২০০ কাজ/মাস**

| Model | প্রতি কাজ | A: ১৫,৬০০ কাজ/মাস | B: ৫,২০০ কাজ/মাস |
|---|---|---|---|
| Gemini 2.5 Flash-Lite | $0.0011 (০.১৪ ৳) | $17.5 (প্রায় ২,১৫০ ৳) | $5.8 (প্রায় ৭২০ ৳) |
| OpenAI gpt-6-luna | $0.0012 (০.১৫ ৳) | $18.7 (প্রায় ২,৩০০ ৳) | $6.2 (প্রায় ৭৭০ ৳) |
| OpenAI gpt-4o-mini | $0.0017 (০.২১ ৳) | $26.2 (প্রায় ৩,২৩০ ৳) | $8.7 (প্রায় ১,০৮০ ৳) |
| **Gemini 3.1 Flash-Lite** | $0.0032 (০.৩৯ ৳) | $49.9 (প্রায় ৬,১৫০ ৳) | **$16.6 (প্রায় ২,০৫০ ৳)** |
| Gemini 3.5 Flash-Lite / 2.5 Flash | $0.0044 (০.৫৪ ৳) | $68.6 (প্রায় ৮,৪৬০ ৳) | $22.9 (প্রায় ২,৮২০ ৳) |
| Gemini 3.7 Flash (২০২৬ দাম) | $0.0090 (১.১১ ৳) | $140 (প্রায় ১৭,৩০০ ৳) | $46.8 (প্রায় ৫,৭৭০ ৳) |
| Gemini 3.7 Flash (২০২৭ দাম) | $0.0180 (২.২২ ৳) | $281 (প্রায় ৩৪,৬০০ ৳) | $93.6 (প্রায় ১১,৫৩০ ৳) |
| Claude Haiku 4.5 | $0.0120 (১.৪৮ ৳) | $187 (প্রায় ২৩,০৬০ ৳) | $62.4 (প্রায় ৭,৬৯০ ৳) |
| OpenAI gpt-6.1-sol | $0.0240 (২.৯৬ ৳) | $374 (প্রায় ৪৬,১৩০ ৳) | $125 (প্রায় ১৫,৩৮০ ৳) |

**বাড়তি খরচ:**
- মার্কশিটের ছবি, মাসে ৩০০টা: Gemini 3.1 Flash-Lite-এ প্রায় $1 (১২০ ৳), Gemini 3.7 Flash-এ প্রায় $2.6–5.2।
- ভয়েস: ২০ জন × দিনে ১০টা × ১৫ সেকেন্ড × ২৬ দিন = প্রায় ১,৩০০ মিনিট/মাস, খরচ প্রায় $4–6.5 (৫০০–৮০০ ৳)।
- Prompt caching চালু করলে input খরচ উপরের হিসাবের চেয়ে আরও কমবে।
- **SMS খরচ AI খরচ থেকে আলাদা।** SMS-এর খরচ আগের মতোই স্কুলের SMS ব্যালেন্স থেকে কাটবে। কোডে ডিফল্ট রেট `PerSmsRate = 0.36` টাকা ধরা আছে।
- সার্ভারের খরচ বাড়বে না, কারণ একই SyncApi সার্ভারেই চলবে।

### ৬.৪ পরামর্শ
- **শুরু করুন Gemini 3.1 Flash-Lite দিয়ে** (সস্তা, tool calling আর ছবি দুটোই পারে)। অথবা OpenAI gpt-6-luna দিয়ে, যেটা আরও সস্তা। দুটো দিয়েই বাংলা কাজে একবার পরীক্ষা করে তারপর বাছাই করা ভালো।
- **গ্লোবাল ডিফল্ট মডেল Gemini 3.1 Flash-Lite (×১)।** প্রতিষ্ঠান চাইলে স্মার্ট বা প্রো মডেল বেছে নেবে, আর বেশি গুণকে সেই খরচ নিজেই দেবে (§৪.১০)। হাতের লেখা মার্কশিটে ছোট মডেল ভুল করলে স্কুল শুধু OCR-এর জন্য Gemini 3.7 Flash (×৩) বাছতে পারবে।
- বাস্তবসম্মত ব্যবহারে (অনুমান B) আসল খরচ **প্রতি স্কুলে মাসে মোটামুটি ৮০০–২,৫০০ টাকা**। এই খরচ প্রিপেইড AI প্যাক বিক্রি করে তোলা হবে (§৪)। মার্জিনের হিসাব §৪.১০-এ।

---

## ৭. ঝুঁকি আর আলাদা নিরাপত্তা ফাইন্ডিং

1. **সার্ভারে পেজ পারমিশন চেক নেই** (২.৩ দেখুন)। এজেন্টে আলাদা guard থাকবে। তবে বিদ্যমান endpoint-গুলোতেও একই guard লাগানোর পরামর্শ দিচ্ছি (আলাদা কাজ)।
2. **SMS আর পেমেন্টের credential সোর্স কোডে hardcoded আছে** (মান এখানে লেখা হয়নি):
   - `Sikkhaloy.SyncApi/Services/OfficeSmsGateway.cs`: Novocom ApiKey, ClientId আর SenderId constant হিসেবে, ফাইলটা git-এ track করা।
   - `SmsOfficeService.cs` আর `PlatformInvoiceService.cs`: ShurjoPay username আর password literal। (`ShurjoPayCallbackService` ইতিমধ্যে config থেকে পড়ে।)
   - পরামর্শ: এগুলো `appsettings.Secrets.json` বা environment variable-এ সরানো, আর Novocom key একবার rotate করা।
3. অফিসের marks save-এ `Marks_Input_Locked` চেক হয় না। এজেন্টে চেক থাকবে, অফিস পেজেও যোগ করার কথা ভাবা যায়।
4. `SaveNoticeAsync` যাচাই করে না ClassIDs এই স্কুলের কিনা। এজেন্টে যাচাই থাকবে।
5. OCR নির্ভুলতা হাতের লেখার উপর নির্ভর করে, তাই কনফার্মেশন টেবিলে মানুষের যাচাই বাধ্যতামূলক।
6. মডেল বা দাম বদলাতে পারে, আর মডেল বন্ধও হয়ে যেতে পারে। এর জন্য আছে provider-নিরপেক্ষ কোড, "পরবর্তী দাম"-সহ ক্যাটালগ, fallback চেইন আর `AI_Agent_Usage` দিয়ে খরচে নজর (§৪.১২)। বিভিন্ন মডেলের বাংলা আর tool-calling দক্ষতা আলাদা, তাই কোনো মডেল চালুর আগে "টেস্ট" সেট পাস করতে হবে।
7. ইন্টারনেট না থাকলে এজেন্ট কাজ করবে না। অফলাইন অবস্থায় প্যানেলে সেটা জানিয়ে দেবে।
8. **বিলিং:** ShurjoPay callback anonymous, তাই ব্যালেন্স যোগ হবে শুধু সার্ভার থেকে gateway verify করার পরে, আর টাকার পরিমাণ মিলিয়ে, SMS ফ্লোর মতোই। দুইবার কাটা আর দুইবার যোগ ঠেকাতে unique index আর একটাই transaction থাকবে। ১ ৳/ক্রেডিটে ডিফল্ট মডেলে (Gemini 3.1 Flash-Lite ×১) মার্জিন প্রায় ৬১%। Gemini 3.7 Flash, Claude Haiku আর gpt-6.1-sol ×১-এ লোকসান দেয়, তাই এদের গুণক ×৩ থেকে ×৬ (§৪.১০), আর লোকসানের গুণকে প্যানেল সতর্ক করবে।

---

## ৮. শুরু করার আগে আপনার সিদ্ধান্ত দরকার

1. **কোন AI কোম্পানির key?** মডেল বাছাই এখন প্রতিষ্ঠানের হাতে। তবে চালুর সময় কোন provider-গুলোর key রাখবেন সেটা ঠিক করতে হবে: শুধু Gemini, নাকি Gemini + OpenAI? Claude (Anthropic) রাখলে আলাদা adapter লাগবে, আরও ১ দিন। Key সার্ভারে আপনি নিজে বসাবেন, আমাকে চ্যাটে পাঠাবেন না।
2. ~~খরচ কে দেবে~~ আর ~~মডেল কে ঠিক করবে~~ → **সিদ্ধান্ত হয়েছে:** প্রিপেইড AI প্যাক, মাসে ডিফল্ট ১০০ ক্রেডিট ফ্রি, ডিফল্ট প্যাক ২০০ ক্রেডিট = ২০০ ৳। প্রতিষ্ঠান মডেল বাছবে, কাটা ক্রেডিট = ওজন × গুণক। কোটা, প্যাক আর মডেল সবই Authority প্যানেল থেকে বদলানো যাবে। এখন যা ঠিক করা বাকি:
   - §৪.১০-এর প্রস্তাবিত ক্যাটালগ আর গুণক (×১ / ×২ / ×৩ / ×৬) অনুমোদন। কোন মডেলগুলো শুরুতে চালু থাকবে?
   - Gemini 3.7 Flash-এর গুণক ১ জানুয়ারি ২০২৭ থেকে ×৪ করবেন, নাকি ×৩ রেখে কম মার্জিন (প্রায় ২৬%) মেনে নেবেন?
   - ফ্রি কোটা ক্রেডিটে গোনা হবে (দামি মডেলে দ্রুত শেষ হবে), নাকি ফ্রি কোটা শুধু সাধারণ (×১) মডেলে প্রযোজ্য হবে?
   - স্কুল কতবার মডেল বদলাতে পারবে (প্রস্তাব: যেকোনো সময়, audit-এ থাকবে)? ইউজার বা টাস্কভিত্তিক override কি পরে লাগবে?
   - কেনা ব্যালেন্সের মেয়াদ থাকবে কিনা (প্রস্তাব: মেয়াদ নেই)
   - প্রশ্নোত্তর বা রিপোর্টেও কি ১ কাজ কাটবে? (প্রস্তাব: হ্যাঁ)
   - OCR-এর ওজন ২ কাজ রাখবেন কিনা
   - ৮০% আর শূন্যের নোটিফিকেশন প্রতিষ্ঠানের মোবাইলে SMS-এও যাবে কিনা, গেলে সেই SMS-এর খরচ কোথা থেকে
   - `AAP_Invoice_Category`-তে নতুন 'AI' category বানানোর অনুমতি
3. প্রথমে কোন স্কুলে পাইলট হবে? শুরুতে Admin/Sub-Admin, নাকি শিক্ষকরাও (Teacher portal)?
4. এক কাজে সর্বোচ্চ কতজনকে SMS (প্রস্তাব: ৫০০)? এজেন্ট দিয়ে প্রতি ইউজারের দৈনিক সীমা কত?
5. Audit আর বিলিং টেবিলগুলো (`AI_Agent_*`, `AI_Billing_*`, `AI_Pack*`, `AI_School_Wallet`, `AI_Credit_Ledger`) `EduHybrid` / `Edu`-তে বানানোর অনুমতি?
6. ভয়েস লাগবে নাকি শুরুতে Gboard ভয়েস টাইপিংই যথেষ্ট? (মাইক্রোফোনের জন্য নতুন Play রিলিজ লাগবে।)
7. মার্কশিটের ছবির জন্য হাইব্রিডের "Marks Collect Paper" প্রিন্ট ব্যবহার করবেন, নাকি হাতে বানানো খাতা?
8. আলাদাভাবে: hardcoded SMS আর ShurjoPay credential সরানোর কাজটা কি আগে করে নেব?
