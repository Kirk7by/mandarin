﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Presentation.Code;
using Domain.Model;
using Domain.Services;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace Presentation
{
    /// <summary>
    /// Логика взаимодействия для VIP.xaml
    /// </summary>
    public partial class VIPForm : Window
    {
        public List<Teacher> teachers = new List<Teacher>();
        Setting setting;
        public VIPForm()
        {
            InitializeComponent();
            setting = new Setting();
            TeacherslistBox.ItemsSource = setting.storage.Teachers;

        }
        private void FilterTeachers(string s)
        {


            teachers = new List<Teacher>(setting.storage.Teachers.OrderBy(i => !i.Name.ToLower().StartsWith(s)).
                Where(item => item.Name.ToLower().Contains(s)));
            TeacherslistBox.ItemsSource = teachers;
        }

        private void EnterTextInTeacherslistBox(object sender, TextChangedEventArgs e)
        {
            TeacherslistBox.SelectedIndex = -1;
            FilterTeachers(SearchTeachertextBox.Text.ToLower());

        }

        private void SelectTeacher(object sender, SelectionChangedEventArgs e)
        {
            if (TeacherslistBox.SelectedIndex != -1)
            {
                ClasseslistBox.ItemsSource = setting.GetListClases((Teacher)TeacherslistBox.SelectedItem);
            }
            else { ClasseslistBox.ItemsSource = null; }
        }

        private void SelectClass(object sender, SelectionChangedEventArgs e)
        {
            if (ClasseslistBox.SelectedIndex != -1)
            {
                btnClassRoom.IsEnabled = true;
                ClassRoomlistView.Items.Clear();
                StudentsClass sClass;
                sClass = (StudentsClass)ClasseslistBox.SelectedItem;
                InfoGrooplistView.ItemsSource = sClass.SubGroups;
                DaycomboBox.Items.Clear(); TimecomboBox.Items.Clear();
                DaycomboBox.Items.Add("Понедельник");
                DaycomboBox.Items.Add("Вторник");
                DaycomboBox.Items.Add("Среда");
                DaycomboBox.Items.Add("Четверг");
                DaycomboBox.Items.Add("Пятница");
                DaycomboBox.Items.Add("Суббота");

                TimecomboBox.Items.Add("8.30-10.05");
                TimecomboBox.Items.Add("10.25-12.00");
                TimecomboBox.Items.Add("12.20-13.55");
                TimecomboBox.Items.Add("14.15-15.50");
                TimecomboBox.Items.Add("16.00-17.35");
                TimecomboBox.Items.Add("17.45-19.20");


                foreach (FixedClasses item in setting.LVIP)
                {
                    if (sClass == item.sClass)
                    {
                        ClassRoomlistView.Items.Clear();
                        ClassRoomlistView.Items.Add(item.Room);
                        // indexDayOfWeek(0-5 Понедельник-Суббота верхней недели; 5-11 Понедельник-Суббота нижней недели)
                        int indexDayOfWeek = Constants.GetDayOfClass(item.Time);
                        if ((indexDayOfWeek + 1) > 6)
                        {
                            LowerWeekradioButton.IsChecked = true;
                            DaycomboBox.SelectedIndex = indexDayOfWeek - 6;
                        }
                        else
                        {
                            UpperWeekradioButton.IsChecked = true;
                            DaycomboBox.SelectedIndex = indexDayOfWeek;
                        }
                        TimecomboBox.SelectedIndex = Constants.GetTimeOfClass(item.Time);
                    }
                }
            }
            else
            {
                btnClassRoom.IsEnabled = false;
                InfoGrooplistView.ItemsSource = null; DaycomboBox.Items.Clear(); TimecomboBox.Items.Clear(); ClassRoomlistView.Items.Clear();
            }
        }

        private void SelectGroup(object sender, SelectionChangedEventArgs e)
        {
            InfoGrooplistView.SelectedIndex = -1;
        }

        private void ChooseClassRoom(object sender, RoutedEventArgs e)
        {
            StudentsClass sClass;
            sClass = (StudentsClass)ClasseslistBox.SelectedItem;
            ChooseClassRoom form = new ChooseClassRoom(-1, setting.storage, sClass);
            form.Owner = this;
            form.ShowDialog();
        }


        private void SelectClassRoom(object sender, SelectionChangedEventArgs e)
        {
            ClassRoomlistView.SelectedIndex = -1;
        }

        private void AddClasses(object sender, RoutedEventArgs e)
        {
            try
            {
                int timeIndex = -1;

                StudentsClass sClass;
                sClass = (StudentsClass)ClasseslistBox.SelectedItem;
                ClassRoom classRoom;
                if (UpperWeekradioButton.IsChecked == true)
                {
                    timeIndex = 0;
                    timeIndex += DaycomboBox.SelectedIndex * 6;
                    timeIndex += TimecomboBox.SelectedIndex;
                }
                if (LowerWeekradioButton.IsChecked == true)
                {
                    timeIndex = 0;
                    timeIndex += 36;
                    timeIndex += DaycomboBox.SelectedIndex * 6;
                    timeIndex += TimecomboBox.SelectedIndex;
                }
                classRoom = (ClassRoom)ClassRoomlistView.Items[0];
                bool freeClassroom = true, existAddClassInLVIP = false;
                foreach (var item in setting.LVIP)
                {
                    //Проверка не занята ли аудитория в это время
                    if (item.Room == classRoom && item.Time == timeIndex)
                    { freeClassroom = false; break; }
                }
                if (sClass != null)
                {
                    if (classRoom != null && timeIndex != -1)
                    {
                        if (freeClassroom)
                        {
                            foreach (var item in setting.LVIP)
                            {

                                if (item.sClass == sClass)
                                {
                                    item.Room = classRoom;
                                    item.Time = timeIndex;
                                    existAddClassInLVIP = true;
                                    MessageBox.Show("Всё ок");
                                }

                            }
                        }
                        else { MessageBox.Show("В это время в этой аудитории уже стоит пара"); }

                        if (freeClassroom)
                        {
                            if (!existAddClassInLVIP)
                            {
                                FixedClasses vi = new FixedClasses(sClass, timeIndex, classRoom);
                                setting.LVIP.Add(vi);
                                MessageBox.Show("Всё ок");
                            }

                        }
                        else { MessageBox.Show("В это время в этой аудитории уже стоит пара"); }


                    }
                }
                setting.LVIPB = new List<VIPClasesBin>();
                foreach (var item in setting.LVIP)
                {
                    VIPClasesBin vipClasesBin =
                        new VIPClasesBin(Array.FindIndex(CurrentBase.EStorage.Classes, c => c == item.sClass), item.Time, Array.FindIndex(CurrentBase.EStorage.ClassRooms, c => c == item.Room));
                    setting.LVIPB.Add(vipClasesBin);

                }
                BinaryFormatter formatter = new BinaryFormatter();
                using (FileStream fs = new FileStream("Setting.dat", FileMode.Create))
                {
                    formatter.Serialize(fs, setting.LVIPB);
                }
            }
            catch
            {

            }
        }

        private void RemoveClasses(object sender, RoutedEventArgs e)
        {
            StudentsClass sClass;
            sClass = (StudentsClass)ClasseslistBox.SelectedItem;
            if (sClass != null)
            {
                for (int indexVip = 0; indexVip < setting.LVIP.Count; indexVip++)
                {
                    if (setting.LVIP[indexVip].sClass == sClass)
                    {
                        setting.LVIP.RemoveAt(indexVip);
                        DaycomboBox.SelectedIndex = -1;
                        TimecomboBox.SelectedIndex = -1;
                        InfoGrooplistView.ItemsSource = null;
                        ClassRoomlistView.Items.Clear();
                        indexVip--;

                    }
                }
                setting.LVIPB = new List<VIPClasesBin>();
                foreach (var item in setting.LVIP)
                {
                    VIPClasesBin vipClasesBin = 
                        new VIPClasesBin(Array.FindIndex(CurrentBase.EStorage.Classes, c => c == item.sClass), item.Time, Array.FindIndex(CurrentBase.EStorage.ClassRooms, c => c == item.Room));
                    setting.LVIPB.Add(vipClasesBin);

                }
                BinaryFormatter formatter = new BinaryFormatter();
                using (FileStream fs = new FileStream("Setting.dat", FileMode.Create))
                {
                    formatter.Serialize(fs, setting.LVIPB);
                }
            }
        }
    }
}
