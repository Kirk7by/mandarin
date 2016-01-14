﻿using Domain;
using Domain.DataBaseTypes;
using Domain.Model;
using Domain.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Presentation.Code
{
    static class StorageLoader
    {
        public static EntityStorage CreateEntityStorage()
        {
            return MandarinCore.DataConvertor.ConvertData(
                new List<DBTeacher>(),
                new List<DBStudentSubGroup>(),
                new List<DBClassRoomType>(),
                new List<DBClassRoom>(),
                new List<DBStudentsClass>());
        }

        public static EntityStorage CreateEntityStorage(IRepository repo, string[] connectionStrings)
        {
            EntityStorage storage;
            try
            {
                repo.Init(connectionStrings);

                List<DBTeacher> teachers = repo.GetTeachers().ToList();
                List<DBStudentSubGroup> groups = repo.GetStudentsGroups().ToList();
                List<DBClassRoomType> roomTypes = repo.GetClassRoomsTypes().ToList();
                List<DBClassRoom> rooms = repo.GetClassRooms().ToList();
                List<DBStudentsClass> classes = repo.GetStudentsClasses().ToList();

                storage = MandarinCore.DataConvertor.ConvertData(teachers, groups, roomTypes, rooms, classes);
            }
            catch(Exception ex)
            {
                throw;
            }

            return storage;
        }

        //public static IRepository GetRepository(string filePath)
        //{
        //попытка загрузки dll, создание экземпляра репозитория
        //}
        
    }
}
