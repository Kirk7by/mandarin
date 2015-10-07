﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Microsoft.Office.Interop.Excel;

using Data;
using Domain;
using ESCore;
using Domain.Services;
using Domain.Model;

namespace Presentation.Code
{
    class Logic
    {
        public IRepository Repo { get; private set; }
        public Dictionary<Type, int> FactorTypes { get; private set; }

        //TODO Заглушка для Dependency Inversion
        public void DI()
        {
            Repo = new Repository();
            FactorTypes = new Dictionary<Type, int>();

            Assembly asm = Assembly.Load("FactorsWindows");
            foreach (var factor in asm.GetTypes())
            {
                if(factor.GetInterface("IFactor") != null)
                {
                    int fine = 0;
                    switch (factor.Name)
                    {
                        case "StudentFiveWindows":
                            fine = 99;
                            break;
                        case "StudentFourWindows":
                            fine = 70;
                            break;
                        case "StudentsOneWindow":
                            fine = 20;
                            break;
                        case "StudentThreeWindows":
                            fine = 60;
                            break;
                        case "StudentTwoWindows":
                            fine = 40;
                            break;
                        default:
                            break;
                    }
                    FactorTypes.Add(factor, fine);
                }
            }
            
            asm = Assembly.Load("OtherFactors");
            foreach (var factor in asm.GetTypes())
            {
                if (factor.GetInterface("IFactor") != null)
                {
                    int fine = 0;
                    switch (factor.Name)
                    {
                        case "SixStudentsClasses":
                            fine = 100;
                            break;
                        case "TeacherDayOff":
                            fine = 100;
                            break;
                        default:
                            break;
                    }
                    FactorTypes.Add(factor, fine);
                }
            }
        }

        public void Start()
        {
            EntityStorage storage = Repo.GetEntityStorage();
            StudentsClass[] classes = Repo.GetStudentsClasses(storage).ToArray();

            ESProjectCore core = new ESProjectCore(classes, storage, FactorTypes);
            List<ISchedule> schedules = core.Run().ToList<ISchedule>();
            PartialSchedule asoi = schedules[0].GetPartialSchedule(storage.StudentSubGroups[0]);
            ScheduleExcel excel = new ScheduleExcel(schedules[0], storage);
        }

    }
    
}
