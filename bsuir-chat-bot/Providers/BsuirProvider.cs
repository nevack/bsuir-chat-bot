﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VkNet.Model.RequestParams;

namespace bsuir_chat_bot
{   
    
    public class BsuirProvider : VkBotProvider
    {   
        private const string BsuirApiLink =  "https://students.bsuir.by/api/v1/studentGroup/schedule";

        public BsuirProvider()
        {
            Functions = new Dictionary<string, string>
            {
                {"bsuir", "bsuir [now|today|tomorrow|week] [group No] - schedule for the selected group"}
            };
        }

        /// <summary>
        /// Get all schedule info for target group
        /// </summary>
        /// <param name="groupId">Target group number</param>
        /// <returns>A string with the schedule</returns>
        private static string GetSchedule(string groupId)
        {
            using (var webClient = new WebClient())
            {   
                webClient.QueryString.Add("studentGroup", groupId);
                return webClient.DownloadString(BsuirApiLink);
            }
        }

        /// <summary>
        /// Get current lesson
        /// </summary>
        /// <param name="groupId">Target group number</param>
        /// <returns>A string with the schedule</returns>
        private string Now(string groupId)
        {
            dynamic data = JsonConvert.DeserializeObject(GetSchedule(groupId));
            IEnumerable<dynamic> todaySchedule = data["todaySchedules"];
            var currentLesson = todaySchedule.Where(lesson =>
                DateTime.Parse(lesson["startLessonTime"].Value) < DateTime.Now &&
                DateTime.Parse(lesson["endLessonTime"].Value) > DateTime.Now);
            if (!currentLesson.Any())
                return $"Group {groupId} is free right now!";
            var output = $"Right now group {groupId} is supposed to be at:\n";
            return currentLesson.Aggregate(output, (current, lesson) => (string) (current + LessonToString(lesson)));
        }

        /// <summary>
        /// Get schedule for today
        /// </summary>
        /// <param name="groupId">Target group number</param>
        /// <returns>A string with the schedule</returns>
        private string Today(string groupId)
        {
            dynamic data = JsonConvert.DeserializeObject(GetSchedule(groupId));
            IEnumerable<dynamic> todaySchedule = data["todaySchedules"];
            if (!todaySchedule.Any())
                return $"Group {groupId} is free today!";
            var output = $"Today's schedule for group {groupId} is:\n";
            return todaySchedule.Aggregate(output, (current, lesson) => (string) (current + LessonToString(lesson)));
        }
        
        /// <summary>
        /// Get schedule for tomorrow
        /// </summary>
        /// <param name="groupId">Target group number</param>
        /// <returns>A string with the schedule</returns>
        private string Tomorrow(string groupId)
        {
            dynamic data = JsonConvert.DeserializeObject(GetSchedule(groupId));
            IEnumerable<dynamic> tomorrowSchedule = data["tomorrowSchedules"];
            if (!tomorrowSchedule.Any())
                return $"Group {groupId} is free tomorrow!";
            var output = $"Tomorrow's schedule for group {groupId} is:\n";
            return tomorrowSchedule.Aggregate(output, (current, lesson) => (string) (current + LessonToString(lesson)));
        }

        private string Week(string groupId)
        {
            dynamic data = JsonConvert.DeserializeObject(GetSchedule(groupId));
            if (data["schedules"] == null)
                return $"Group {groupId} is free this week!";
            var output = new StringBuilder($"This week's schedule for group {groupId} is:\n");
            foreach (var day in data["schedules"])
            {
                output += "\n"+day["weekDay"] + ":\n";
                bool freeDay = true;
                foreach (var lesson in day["schedule"])
                {
                    JArray weekNumber = lesson["weekNumber"];
                    var currentWeek = data["currentWeekNumber"];
                    if (weekNumber.All(kek => kek != currentWeek))
                        continue;
                    output += LessonToString(lesson);
                    freeDay = false;
                }

                if (freeDay) output.Append("This day is free this week!\n");
            }

            return output.ToString();
        }
        
        private static string LessonToString(dynamic lesson)
        {
            var output = new StringBuilder();
            output.Append($"▻︎ {lesson["lessonTime"]}: {(lesson["numSubgroup"] == 0 ? "" : "Подгруппа " + lesson["numSubgroup"].ToString() + " ")} {lesson["lessonType"]} {lesson["subject"]} {lesson["note"]}");
            foreach (var employee in lesson["employee"])
                output.Append($", {employee["fio"]}");
            foreach (var auditory in lesson["auditory"])
                output.Append($", {auditory}");
            output.Append("\n");
            return output.ToString();
        }

        protected override MessagesSendParams _handle(VkNet.Model.Message command)
        {
            var (_, args) = command.ParseFunc();

            string message;
            
            if (args.Length != 2)
                throw new ArgumentException("Wrong number of arguments provided");
            
            switch (args[0])
            {
                case "now":
                    message = Now(args[1]);
                    break;
                case "today":
                    message = Today(args[1]);
                    break;
                case "tomorrow":
                    message = Tomorrow(args[1]);
                    break;
                case "week":
                    message = Week(args[1]);
                    break;
                default:
                    throw new ArgumentException("Second argument is incorrect");
            }
            
            var param = new MessagesSendParams
            {
                Message = message,
                PeerId = command.GetPeerId()
            };

            return param;
        }
    }
}