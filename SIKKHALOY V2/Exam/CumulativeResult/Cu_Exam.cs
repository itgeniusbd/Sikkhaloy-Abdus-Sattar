using System.Data.SqlClient;

namespace EDUCATION.COM.Exam.CumulativeResult
{


    public partial class Cu_Exam
    {
    }
}

namespace EDUCATION.COM.Exam.CumulativeResult.Cu_ExamTableAdapters
{
    /// <summary>
    /// Partial class to extend Sub_ResultTableAdapter with increased command timeout
    /// </summary>
    public partial class Sub_ResultTableAdapter
    {
        /// <summary>
        /// Override InitCommandCollection to set longer command timeout
        /// Call this after the adapter is created
        /// </summary>
        public void SetCommandTimeout(int timeout = 900)
        {
            if (this._commandCollection == null)
            {
                this.InitCommandCollection();
            }
            
            foreach (SqlCommand cmd in this._commandCollection)
            {
                if (cmd != null)
                {
                    cmd.CommandTimeout = timeout;
                }
            }
        }
        
        /// <summary>
        /// Get data with extended timeout (15 minutes)
        /// </summary>
        public Cu_Exam.Sub_ResultDataTable GetDataWithTimeout(
            global::System.Nullable<int> CumulativeNameID, 
            int SchoolID, 
            global::System.Nullable<int> EducationYearID, 
            global::System.Nullable<int> ClassID, 
            string SectionID, 
            string SubjectGroupID, 
            string ShiftID,
            int timeout = 900)
        {
            // Set command timeout before executing
            SetCommandTimeout(timeout);
            
            // Now call the normal GetData
            return this.GetData(CumulativeNameID, SchoolID, EducationYearID, ClassID, SectionID, SubjectGroupID, ShiftID);
        }
    }
    
    /// <summary>
    /// Partial class to extend Cumi_Student_ProfileTableAdapter with increased command timeout
    /// </summary>
    public partial class Cumi_Student_ProfileTableAdapter
    {
        /// <summary>
        /// Set command timeout
        /// </summary>
        public void SetCommandTimeout(int timeout = 900)
        {
            if (this._commandCollection == null)
            {
                this.InitCommandCollection();
            }
            
            foreach (SqlCommand cmd in this._commandCollection)
            {
                if (cmd != null)
                {
                    cmd.CommandTimeout = timeout;
                }
            }
        }
        
        /// <summary>
        /// Get data with extended timeout (15 minutes)
        /// </summary>
        public Cu_Exam.Cumi_Student_ProfileDataTable GetDataWithTimeout(
            global::System.Nullable<int> CumulativeNameID, 
            int SchoolID, 
            global::System.Nullable<int> EducationYearID, 
            int StudentClassID,
            int timeout = 900)
        {
            // Set command timeout before executing
            SetCommandTimeout(timeout);
            
            // Now call the normal GetData
            return this.GetData(CumulativeNameID, SchoolID, EducationYearID, StudentClassID);
        }
    }
}
