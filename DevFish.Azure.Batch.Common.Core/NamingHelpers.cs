using System;
using System.Collections.Generic;
using System.Text;

namespace DevFish.Azure.Batch.Common.Core
{
    public static class NamingHelpers
    {
        private static int m_JobSeq = 0;
        private static int m_TaskSeq = 0;

        public static string GenJobName( string suffix )
        {
            DateTime d = DateTime.Now;
            m_JobSeq += 1;
            return $"JOB{suffix}{d.Month.ToString("00")}{d.Day.ToString("00")}{d.Hour.ToString("00")}{d.Minute.ToString("00")}{d.Second.ToString("00")}{m_JobSeq.ToString("000")}";
        }

        public static string GenTaskName(string suffix)
        {
            DateTime d = DateTime.Now;
            m_TaskSeq += 1;
            return $"TASK{suffix}{d.Month.ToString("00")}{d.Day.ToString("00")}{d.Hour.ToString("00")}{d.Minute.ToString("00")}{d.Second.ToString("00")}{m_TaskSeq.ToString("000")}";
        }
    }
}
