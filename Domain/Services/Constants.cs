﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Services
{
    public static class Constants
    {
        public const int WEEKS_IN_SCHEDULE = 2;
        public const int DAYS_IN_WEEK = 6;
        public const int CLASSES_IN_DAY = 6;

        public const int BLOCK_FINE = -1;

        public const int FINE_FOR_SECOND_CLASSROOM = 1000;

        public static int GetDayOfClass(int classTime)
        {
            return (int)Math.Floor((double)classTime / CLASSES_IN_DAY);
        }
        public static int GetTimeOfClass(int classTime)
        {
            return classTime - (CLASSES_IN_DAY * GetDayOfClass(classTime) - 1) - 1;
        }
        public static int GetWeekOfClass(int classTime)
        {
            return classTime < CLASSES_IN_DAY * DAYS_IN_WEEK ? 0 : 1;
        }
    }
}
