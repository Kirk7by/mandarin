﻿using Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Services;
using Domain.Model;

namespace OtherFactors
{
    public class SixStudentsClasses : IFactor
    {
        int fine;
        bool isBlock;

        public int GetFineOfAddedClass(ISchedule schedule, EntityStorage eStorage)
        {
            StudentSubGroup[] groups = schedule.GetTempClass().SubGroups;
            int fineResult = 0;
            for (int groupIndex = 0; groupIndex < groups.Length; groupIndex++)
            {
                PartialSchedule groupSchedule = schedule.GetPartialSchedule(groups[groupIndex]);
                int day = Constants.GetDayOfClass(schedule.GetTimeOfTempClass());
                if (Array.FindAll<StudentsClass>(groupSchedule.GetClassesOfDay(day), (c) => c != null).Count() == 6)
                {
                    if (isBlock)
                        return Constants.BLOCK_FINE;
                    else
                    fineResult += fine;
                }
            }
            return fineResult;
        }

        public int GetFineOfFullSchedule(ISchedule schedule, EntityStorage eStorage)
        {
            int fineResult = 0;
            for (int groupIndex = 0; groupIndex < eStorage.StudentSubGroups.Length; groupIndex++)
            {
                PartialSchedule groupSchedule = schedule.GetPartialSchedule(eStorage.StudentSubGroups[groupIndex]);
                for (int dayIndex = 0; dayIndex < Constants.WEEKS_IN_SCHEDULE * Constants.DAYS_IN_WEEK; dayIndex++)
                {
                    if (Array.FindAll<StudentsClass>(groupSchedule.GetClassesOfDay(dayIndex), (c) => c != null).Count() == 6)
                    {
                        if (isBlock)
                            return Constants.BLOCK_FINE;
                        else
                            fineResult += fine;
                    }
                }
            }
            return fineResult;
        }

        public string GetName()
        {
            return "6 пар";
        }
        public string GetDescription()
        {
            return "Шесть пар в день";
        }

        public void Initialize(int fine = 0, bool isBlock = false, object data = null)
        {
            if (fine >= 0 && fine <= 100)
            {
                this.fine = fine;
                this.isBlock = isBlock;
                if (fine == 100)
                    this.isBlock = true;
            }
        }
        public object GetDataType()
        {
            return null;
        }
    }
}
