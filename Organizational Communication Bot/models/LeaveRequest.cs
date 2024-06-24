using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Organizational_Communication_Bot.models
{
    public class LeaveRequest
    {
        public string Username { get; set; }
        public string LeaveType { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Reason { get; set; }

        public LeaveRequest(string username, string leaveType, DateTime startDate, DateTime endDate, string reason)
        {
            Username = username;
            LeaveType = leaveType;
            StartDate = startDate;
            EndDate = endDate;
            Reason = reason;
        }

        public override string ToString()
        {
            return $"{Username} ขอลา{LeaveType} ตั้งแต่ {StartDate.ToShortDateString()} ถึง {EndDate.ToShortDateString()} เหตุผล: {Reason}";
        }
    }
}