using Serilog;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Threading.Tasks;

namespace SmsSenderApp
{
    public class GlobalClass
    {
        private static readonly Lazy<GlobalClass> Lazy = new Lazy<GlobalClass>(() => new GlobalClass());

        public static GlobalClass Instance => Lazy.Value;

        private GlobalClass()
        {
            // Private constructor to prevent instantiation from outside the class
            LoadSettings();
        }

        public Attendance_SMS_Sender SmsSender { get; private set; }

        public SikkhaloySetting Setting { get; private set; }

        // Helper class for raw SQL query result
        private class SettingDto
        {
            public int SmsSendInterval { get; set; }
            public int SmsProcessingUnit { get; set; }
        }

        private void LoadSettings()
        {
            try
            {
                using (var db = new EduEntities())
                {
                    // Use raw SQL query with strongly-typed result
                    var sqlQuery = "SELECT SmsSendInterval, SmsProcessingUnit FROM SikkhaloySetting";
                    
                    try
                    {
                        // Execute raw SQL with strongly-typed class
                        var result = db.Database.SqlQuery<SettingDto>(sqlQuery).FirstOrDefault();
                        
                        if (result != null)
                        {
                            Log.Information($"RAW SQL successful - SmsSendInterval: {result.SmsSendInterval}, SmsProcessingUnit: {result.SmsProcessingUnit}");
                            
                            // Create setting object with values from raw query
                            Setting = new SikkhaloySetting
                            {
                                SmsSendInterval = result.SmsSendInterval,
                                SmsProcessingUnit = result.SmsProcessingUnit
                            };
                            
                            Log.Information($"Settings loaded successfully - SmsSendInterval: {Setting.SmsSendInterval}, SmsProcessingUnit: {Setting.SmsProcessingUnit}");
                        }
                        else
                        {
                            Log.Warning("No SikkhaloySetting found in database, using default values");
                            Setting = new SikkhaloySetting
                            {
                                SmsSendInterval = 5,
                                SmsProcessingUnit = 100
                            };
                        }
                    }
                    catch (Exception sqlEx)
                    {
                        Log.Error(sqlEx, "Raw SQL query failed, trying Entity Framework");
                        
                        // Fallback to EF if raw SQL fails
                        Setting = db.SikkhaloySettings.FirstOrDefault();

                        if (Setting == null)
                        {
                            Setting = new SikkhaloySetting
                            {
                                SmsSendInterval = 5,
                                SmsProcessingUnit = 100
                            };
                            Log.Warning("No SikkhaloySetting found via EF, using default values");
                        }
                        else
                        {
                            Log.Information($"EF query result - SmsSendInterval: {Setting.SmsSendInterval}, SmsProcessingUnit: {Setting.SmsProcessingUnit}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to load settings from database");
                // Create default setting if database connection fails
                Setting = new SikkhaloySetting
                {
                    SmsSendInterval = 5,
                    SmsProcessingUnit = 100
                };
            }
        }

        public void ReloadSettings()
        {
            Log.Information("Reloading settings from database");
            LoadSettings();
        }

        public void SenderInsert()
        {
            try
            {
                var sender = new Attendance_SMS_Sender
                {
                    AppStartTime = DateTime.Now,
                };

                using (var db = new EduEntities())
                {
                    db.Attendance_SMS_Sender.Add(sender);
                    db.SaveChanges();
                }

                SmsSender = sender;
            }
            catch (Exception e)
            {
                Log.Error(e, e.Message);
                // Don't throw, continue with null SmsSender
                SmsSender = new Attendance_SMS_Sender
                {
                    AppStartTime = DateTime.Now,
                };
            }

        }

        public void SenderUpdate()
        {
            try
            {
                if (SmsSender == null)
                {
                    Log.Warning("SmsSender is null, skipping update");
                    return;
                }

                using (var db = new EduEntities())
                {
                    var existing = db.Attendance_SMS_Sender.Find(SmsSender.AttendanceSmsSenderId);
                    if (existing != null)
                    {
                        existing.TotalEventCall = SmsSender.TotalEventCall;
                        existing.TotalSmsSend = SmsSender.TotalSmsSend;
                        existing.TotalSmsFailed = SmsSender.TotalSmsFailed;
                        existing.AppCloseTime = DateTime.Now;
                        db.SaveChanges();
                        Log.Debug($"Updated SmsSender in DB - ID: {SmsSender.AttendanceSmsSenderId}, Event: {SmsSender.TotalEventCall}, Sent: {SmsSender.TotalSmsSend}, Failed: {SmsSender.TotalSmsFailed}");
                    }
                    else
                    {
                        Log.Warning($"SmsSender with ID {SmsSender.AttendanceSmsSenderId} not found in database");
                    }
                }

            }
            catch (Exception e)
            {
                Log.Error(e, "Failed to update SmsSender in database");
                // Don't throw on exit
            }
        }

        public async Task<List<Attendance_SMS>> GetAttendanceSmsListAndDeleteFromDbAsync()
        {
            try
            {
                var smsList = new List<Attendance_SMS>();

                using (var db = new EduEntities())
                {
                    smsList = await db.Attendance_SMS.Take(Setting.SmsProcessingUnit).ToListAsync();
                    db.Attendance_SMS.RemoveRange(smsList);
                    await db.SaveChangesAsync();
                }


                return smsList;

            }
            catch (Exception e)
            {
                Log.Error(e, e.Message);
                throw;
            }

        }

        public async Task<List<int>> NoSmsBalanceSchoolIdsAsync(List<int> allSchoolIds)
        {
            try
            {
                var ids = new List<int>();

                using (var db = new EduEntities())
                {
                    // Simplified query - SMS table only has SMS_Balance column
                    var sql = @"SELECT SchoolID FROM SMS 
                                WHERE SMS_Balance < 1 
                                AND SchoolID IN (" + string.Join(",", allSchoolIds) + ")";
  
                    try
                    {
                        ids = await db.Database.SqlQuery<int>(sql).ToListAsync();
                    }
                    catch (Exception ex)
                    {
                        Log.Warning(ex, "Failed to query SMS balance, assuming all schools have balance");
                        // If query fails, return empty list (assume all have balance)
                        ids = new List<int>();
                    }
                }

                return ids;
            }
            catch (Exception e)
            {
                Log.Error(e, e.Message);
                // Don't throw, return empty list
                return new List<int>();
            }
        }

        public async Task SMS_OtherInfoAddAsync(IEnumerable<SMS_OtherInfo> dataList)
        {
            try
            {
                using (var db = new EduEntities())
                {
                    db.SMS_OtherInfo.AddRange(dataList);
                    await db.SaveChangesAsync();
                }
            }
            catch (Exception e)
            {
                Log.Error(e, e.Message);
            }
        }

        public async Task SMS_Send_RecordAddAsync(IEnumerable<SMS_Send_Record> dataList)
        {
            try
            {
                using (var db = new EduEntities())
                {
                    db.SMS_Send_Record.AddRange(dataList);
                    await db.SaveChangesAsync();
                }
            }
            catch (Exception e)
            {
                Log.Error(e, e.Message);
            }
        }

        public async Task Attendance_SMS_FailedAddAsync(IEnumerable<Attendance_SMS_Failed> dataList)
        {
            try
            {
                using (var db = new EduEntities())
                {
                    db.Attendance_SMS_Failed.AddRange(dataList);
                    await db.SaveChangesAsync();
                }
            }
            catch (Exception e)
            {
                Log.Error(e, e.Message);
                throw;
            }
        }
    }
}