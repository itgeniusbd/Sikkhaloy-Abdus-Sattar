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
            using (var db = new ModelContext())
            {
                Setting = db.SikkhaloySettings.FirstOrDefault();
            }
        }

        public Attendance_SMS_Sender SmsSender { get; private set; }

        public SikkhaloySetting Setting { get; }

        public void SenderInsert()
        {
            try
            {
                var sender = new Attendance_SMS_Sender
                {
                    AppStartTime = DateTime.Now,
                };

                using (var db = new ModelContext())
                {
                    db.Attendance_SMS_Sender.Add(sender);
                    db.SaveChanges();
                }

                SmsSender = sender;
            }
            catch (Exception e)
            {
                Log.Error(e, e.Message);
                throw;
            }

        }

        public void SenderUpdate()
        {
            try
            {
                using (var db = new ModelContext())
                {
                    SmsSender.AppCloseTime = DateTime.Now;
                    db.Attendance_SMS_Sender.AddOrUpdate(SmsSender);
                    db.SaveChanges();
                }

            }
            catch (Exception e)
            {
                Log.Error(e, e.Message);

                throw;
            }
        }

        public async Task<List<Attendance_SMS>> GetAttendanceSmsListAndDeleteFromDbAsync()
        {
            try
            {
                var smsList = new List<Attendance_SMS>();

                using (var db = new ModelContext())
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

                using (var db = new ModelContext())
                {
                    ids = await db.SMS.Where(s => s.SMS_Balance < 1 && allSchoolIds.Contains(s.SchoolID)).Select(s => s.SchoolID).ToListAsync();
                }

                return ids;

            }
            catch (Exception e)
            {
                Log.Error(e, e.Message);
                throw;
            }
        }

        public async Task SMS_OtherInfoAddAsync(IEnumerable<SMS_OtherInfo> dataList)
        {
            try
            {
                using (var db = new ModelContext())
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
                using (var db = new ModelContext())
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
                using (var db = new ModelContext())
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