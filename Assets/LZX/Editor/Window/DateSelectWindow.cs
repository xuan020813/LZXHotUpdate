using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using DateTime = System.DateTime;
namespace LZX.MEditor.Window
{
    public class DateSelectWindow: EditorWindow
    {
        public VisualTreeAsset uxml;
        public StyleSheet uss;
        private DropdownField yearDropdown;
        private Label label_month;
        private VisualElement daysContainer;
        private Label label_text;
        public Action<string> OnYesButtonClick;
        private void CreateGUI()
        {
            var root = uxml.CloneTree();
            root.styleSheets.Add(uss);
            rootVisualElement.Add(root);

            
            yearDropdown = root.Q<DropdownField>("dropdown");
            InitYears();
            var btn_rise = root.Q<Button>("btn_rise");
            var btn_add = root.Q<Button>("btn_add");
            btn_rise.clicked += () => { OnRiseButtonClick(); };
            btn_add.clicked += () => { OnAddButtonClick(); };

            label_month = root.Q<Label>("label_month");
            label_month.text = DateTime.Now.Month.ToString();

            daysContainer = root.Q<VisualElement>("daycontainter");
            InitDays();

            #region Label+Button
            label_text = new Label();
            label_text.style.position = Position.Absolute;
            label_text.text = DateTime.Now.ToString("yyyy-MM-dd");
            label_text.style.bottom = new StyleLength(2);
            rootVisualElement.Add(label_text);

            var btn_yes = new Button();
            btn_yes.text = "确定";
            btn_yes.style.position = Position.Absolute;
            btn_yes.style.left = new StyleLength(90);
            btn_yes.style.bottom = new StyleLength(2);
            btn_yes.clicked += () => { OnYesButtonClick?.Invoke(label_text.text); };
            rootVisualElement.Add(btn_yes);
            #endregion
        }
        private void InitYears()
        {
            yearDropdown.choices.Clear();
            int year = DateTime.Now.Year;
            for (int i = year - 10; i <= year + 10; i++)
            {
                yearDropdown.choices.Add(i.ToString());
            }
            yearDropdown.value = year.ToString();
            yearDropdown.RegisterValueChangedCallback(OnYearChanged);
        }
        private void InitDays()
        {
            int dayCount = 0;
            switch (label_month.text)
            {
                case "1":
                case "3":
                case "5":
                case "7":
                case "8":
                case "10":
                case "12":
                    dayCount = 31;
                    break;
                case "4":
                case "6":
                case "9":
                case "11":
                    dayCount = 30;
                    break;
                case "2":
                    dayCount = 28;
                    break;
            }
            daysContainer.Clear();
            for (int i = 1; i <= 7; i++)
            {
                var weekitem = new Button();
                weekitem.style.width = new StyleLength(new Length(12, LengthUnit.Percent));
                weekitem.style.height = new StyleLength(30);
                weekitem.style.backgroundColor = new StyleColor(Color.gray);
                weekitem.style.color = new StyleColor(Color.black);
                switch (i)
                {
                    case 1:
                        weekitem.text = "日";
                        break;
                    case 2:
                        weekitem.text = "一";
                        break;
                    case 3:
                        weekitem.text = "二";
                        break;
                    case 4:
                        weekitem.text = "三";
                        break;
                    case 5:
                        weekitem.text = "四";
                        break;
                    case 6:
                        weekitem.text = "五";
                        break;
                    case 7:
                        weekitem.text = "六";
                        break;
                }
                daysContainer.Add(weekitem);
            }
            string year = yearDropdown.value;
            string month = label_month.text;
            DateTime date = new DateTime(int.Parse(year), int.Parse(month), 1);
            int dayOfWeek = (int)date.DayOfWeek;
            for (int i = 0; i < dayOfWeek; i++)
            {
                var day = new Button();
                day.style.width = new StyleLength(new Length(12, LengthUnit.Percent));
                day.style.height = new StyleLength(30);
                day.style.backgroundColor = new StyleColor(Color.clear);
                daysContainer.Add(day);
            }
            for (int i = 1; i <= dayCount; i++)
            {
                var day = new Button();
                day.style.width = new StyleLength(new Length(12, LengthUnit.Percent));
                day.style.height = new StyleLength(30);
                day.text = i.ToString();
                int idx = i;
                day.clicked += () =>
                {
                    OnDayClick(idx);
                };
                daysContainer.Add(day);
            }
        }
        private void OnAddButtonClick()
        {
            int month = int.Parse(label_month.text);
            if (month == 12)
                month = 0;
            month++;
            label_month.text = month.ToString();
            InitDays();
            OnMonthChanged();
        }
        private void OnRiseButtonClick()
        {
            int month = int.Parse(label_month.text);
            if (month == 1)
                month = 13;
            month--;
            label_month.text = month.ToString();
            InitDays();
            OnMonthChanged();
        }
        private void OnYearChanged(ChangeEvent<string> evt)
        {
            var date = label_text.text.Split('-');
            label_text.text = evt.newValue + "-" + date[1] + "-" + date[2];
            InitDays();
        }
        private void OnMonthChanged()
        {
            var date = label_text.text.Split('-');
            label_text.text = date[0] + "-" + label_month.text + "-" + date[2];
            InitDays();
        }
        private void OnDayClick(int i)
        {
            var date = label_text.text.Split('-');
            label_text.text = date[0] + "-" + date[1] + "-" + i.ToString();
        }
    }
}