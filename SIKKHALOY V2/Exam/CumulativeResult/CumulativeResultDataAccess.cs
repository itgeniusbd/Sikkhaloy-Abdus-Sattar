using System;
using System.ComponentModel;
using EDUCATION.COM.Exam.CumulativeResult.Cu_ExamTableAdapters;

namespace EDUCATION.COM.Exam.CumulativeResult
{
    /// <summary>
    /// Custom data access class for Cumulative Result with extended timeout
    /// This class is used by ObjectDataSource in the ASPX page
    /// </summary>
    [DataObject(true)]
    public class CumulativeResultDataAccess
    {
        private const int COMMAND_TIMEOUT = 900; // 15 minutes
        
        /// <summary>
        /// Get Cumulative Result data with extended timeout (15 minutes)
        /// </summary>
        [DataObjectMethod(DataObjectMethodType.Select, true)]
        public Cu_Exam.Sub_ResultDataTable GetData(
            int? CumulativeNameID, 
            int SchoolID, 
            int? EducationYearID, 
            int? ClassID, 
            string SectionID, 
            string SubjectGroupID, 
            string ShiftID)
        {
            var adapter = new Sub_ResultTableAdapter();
            
            // Set command timeout to 15 minutes
            adapter.SetCommandTimeout(COMMAND_TIMEOUT);
            
            // Now get the data
            return adapter.GetData(CumulativeNameID, SchoolID, EducationYearID, ClassID, SectionID, SubjectGroupID, ShiftID);
        }
    }
}
