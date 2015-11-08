﻿using Domain.Model;
using SimpleLogging.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESCore
{
    class Rollback
    {
        public ILoggingService logger { get; set; }

        int MaxCountRollbacks;
        int currentCountRollbacks = 0;

        StudentsClass[] classesForCountRollbackLength;//здесь хранятся пары всегда в первоначальной сортировке

        int[] rollbackLenghtArray;
        int[] rollbackCountArray;
        FullSchedule schedule;

        const int MAX_ROLLBACKS_WITHOUT_INCREASE = 5;

        public Rollback(StudentsClass[] classes, int maxCountRollabaks, FullSchedule schedule)
        {
            MaxCountRollbacks = maxCountRollabaks;
            rollbackLenghtArray = new int[classes.Length];
            rollbackCountArray = new int[classes.Length];
            classesForCountRollbackLength = (StudentsClass[])classes.Clone();
            this.schedule = schedule;
        }
        
        public bool DoRollback(ref StudentsClass[] sortedStudentsClasses, ref int classIndex)
        {
            logger.Trace("позиция: " + (classIndex+1).ToString());
            currentCountRollbacks++;
            //слишком много откатов
            if (currentCountRollbacks > MaxCountRollbacks)
            {
                logger.Trace("Слишком много откатов");
                return false;
            }
            //дошли до начала списка пар - не с чем менять
            if (classIndex == 1)
            {
                logger.Trace("начало списка - не с чем менять");
                return false;
            }


            int[] indexForRollback = GetIndexOfSuitablesClasses(sortedStudentsClasses, classIndex);
            if (indexForRollback == null)
            {
                //не с чем менять
                logger.Trace("Пару не с чем менять");
                return false;
            }
            //удаление откаченных пар из расписания
            logger.Trace("Пары будут откачены: ");
            for (int index = 0; index < indexForRollback.Length; index++)
            {
                logger.Trace("- " + sortedStudentsClasses[indexForRollback[index]].Name);
                schedule.RemoveClass(sortedStudentsClasses[indexForRollback[index]]);
            }
            
            //пары, стоящие после откаченных и до той, которой требуется откат, сдвигаются влево
            //откаченные пары ставятся после той, для которой происходит откат
            RealignClassesArray(ref sortedStudentsClasses, indexForRollback, classIndex);



            classIndex -= (indexForRollback.Length+1);
            logger.Trace("Откат завершен");
            return true;
        }

        void RealignClassesArray(ref StudentsClass[] sortedStudentsClasses, int[] indexForRollback, int indexTo)
        {
            StudentsClass[] classesForRollback = new StudentsClass[indexForRollback.Length];
            for (int classIndex = 0; classIndex < classesForRollback.Length; classIndex++)
            {
                classesForRollback[classIndex] = sortedStudentsClasses[indexForRollback[indexForRollback.Length - classIndex - 1]];
            }
            List<StudentsClass> tempList = sortedStudentsClasses.ToList<StudentsClass>();
            for (int classIndex = 0; classIndex < classesForRollback.Length; classIndex++)
            {
                tempList.Remove(classesForRollback[classIndex]);
            }
            int k = 0;
            for (int classIndex = indexTo - indexForRollback.Length + 1; classIndex <= indexTo; classIndex++)
            {
                tempList.Insert(classIndex, classesForRollback[k]);
                k++;
            }
            sortedStudentsClasses = tempList.ToArray<StudentsClass>();
        }

        StudentsClass lastRollbackClass;
        int[] GetIndexOfSuitablesClasses(StudentsClass[] sortedStudentsClasses, int classIndex)
        {
            int classIndexInLenghtArray = Array.IndexOf<StudentsClass>(classesForCountRollbackLength, sortedStudentsClasses[classIndex]);
            if (rollbackLenghtArray[classIndexInLenghtArray] == 0)//пара откатывается в первый раз
                rollbackLenghtArray[classIndexInLenghtArray]++;
            if(lastRollbackClass == null)
            {
                lastRollbackClass = sortedStudentsClasses[classIndex];
            }
            if(lastRollbackClass != sortedStudentsClasses[classIndex])
            {
                rollbackCountArray[Array.IndexOf<StudentsClass>(classesForCountRollbackLength, lastRollbackClass)]++;
                lastRollbackClass = sortedStudentsClasses[classIndex];
            }

            if (rollbackCountArray[classIndexInLenghtArray] >= MAX_ROLLBACKS_WITHOUT_INCREASE)
            {
                //много откатов пары - увеличивается длина отката
                rollbackCountArray[classIndexInLenghtArray] = 0;
                rollbackLenghtArray[classIndexInLenghtArray]++;
            }
            int rollbackLength = rollbackLenghtArray[classIndexInLenghtArray];

            //поиск подходящих для отката пар
            List<int> classesForRollbask = new List<int>();
            for (int index = classIndex - 1; index > 0; index--)
            {
                if (IsSuitableClass(sortedStudentsClasses[index], sortedStudentsClasses[classIndex]))
                {
                    classesForRollbask.Add(index);
                    if (classesForRollbask.Count == rollbackLength)
                        break;
                }
            }
            return classesForRollbask.Count == rollbackLength ? classesForRollbask.ToArray<int>() : null;
        }
        bool IsSuitableClass(StudentsClass suitableClass, StudentsClass cl)
        {
            for (int teacherIndex = 0; teacherIndex < cl.Teacher.Length; teacherIndex++)
            {
                Teacher teacher = Array.Find<Teacher>(suitableClass.Teacher, (t) => t == cl.Teacher[teacherIndex]);
                if (teacher != null)
                    return true;
            }
            for (int groupIndex = 0; groupIndex < cl.SubGroups.Length; groupIndex++)
            {
                StudentSubGroup group = Array.Find<StudentSubGroup>(suitableClass.SubGroups, (s) => s == cl.SubGroups[groupIndex]);
                if (group != null)
                    return true;
            }
            for (int requereIndex = 0; requereIndex < cl.RequireForClassRoom.Length; requereIndex++)
            {
                ClassRoomType requere = Array.Find<ClassRoomType>(suitableClass.RequireForClassRoom, (r) => r == cl.RequireForClassRoom[requereIndex]);
                if (requere != null)
                    return true;
            }
            return false;
        }
    }
}