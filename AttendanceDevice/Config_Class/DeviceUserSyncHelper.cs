using AttendanceDevice.Model;
using AttendanceDevice.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AttendanceDevice.Config_Class
{
    internal static class DeviceUserSyncHelper
    {
        public sealed class PendingDevicePush
        {
            public List<DeviceUserPushView> Views { get; set; } = new List<DeviceUserPushView>();
            public List<User> Users { get; set; } = new List<User>();
        }

        public static PendingDevicePush GetUsersPendingPush(IEnumerable<User> pcUsers, IEnumerable<User> deviceUsers)
        {
            var result = new PendingDevicePush();

            var uniquePcUsers = (pcUsers ?? Enumerable.Empty<User>())
                .GroupBy(u => u.DeviceID)
                .Select(g => g.First())
                .ToList();

            var deviceMap = (deviceUsers ?? Enumerable.Empty<User>())
                .GroupBy(u => u.DeviceID)
                .ToDictionary(g => g.Key, g => g.First());

            foreach (var pcUser in uniquePcUsers)
            {
                if (pcUser.DeviceID <= 0)
                    continue;

                User deviceUser;
                if (!deviceMap.TryGetValue(pcUser.DeviceID, out deviceUser))
                {
                    result.Views.Add(CreateView(pcUser, "New User", string.Empty));
                    result.Users.Add(pcUser);
                    continue;
                }

                var pcRfid = NormalizeRfid(pcUser.RFID);
                if (string.IsNullOrEmpty(pcRfid))
                    continue;

                var deviceRfid = NormalizeRfid(deviceUser.RFID);
                if (string.IsNullOrEmpty(deviceRfid))
                    continue;

                if (!RfidMatches(pcUser.RFID, deviceUser.RFID))
                {
                    result.Views.Add(CreateView(pcUser, "RFID Updated", deviceUser.RFID ?? string.Empty));
                    result.Users.Add(pcUser);
                }
            }

            return result;
        }

        private static DeviceUserPushView CreateView(User pcUser, string status, string deviceRfid)
        {
            return new DeviceUserPushView
            {
                DeviceID = pcUser.DeviceID,
                RFID = pcUser.RFID,
                ID = pcUser.ID,
                Name = pcUser.Name,
                Status = status,
                DeviceRfid = deviceRfid
            };
        }

        public static bool RfidMatches(string pcRfid, string deviceRfid)
        {
            var pc = NormalizeRfid(pcRfid);
            var dev = NormalizeRfid(deviceRfid);

            if (string.Equals(pc, dev, StringComparison.OrdinalIgnoreCase))
                return true;

            if (long.TryParse(pc, out var pcNum) && long.TryParse(dev, out var devNum))
                return pcNum == devNum;

            return false;
        }

        public static string NormalizeRfid(string rfid)
        {
            if (string.IsNullOrWhiteSpace(rfid) || rfid.Trim() == "0")
                return string.Empty;

            return rfid.Trim();
        }
    }
}
