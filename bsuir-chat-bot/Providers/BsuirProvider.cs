using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VkNet;
using VkNet.Model.Attachments;
using VkNet.Model.RequestParams;

namespace bsuir_chat_bot
{   
    
    public class BsuirProvider : VkBotProvider
    {   
        private const string BsuirApiLink =  "https://students.bsuir.by/api/v1/studentGroup/schedule";
        
        private readonly HttpClient _client = new HttpClient();
        
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
        private string GetSchedule(string groupId)
        {
            var webClient = new WebClient();
            webClient.QueryString.Add("studentGroup", groupId);
            return webClient.DownloadString(BsuirApiLink);
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
            if (todaySchedule == null)
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
            if (tomorrowSchedule == null)
                return $"Group {groupId} is free tomorrow!";
            var output = $"Tomorrow's schedule for group {groupId} is:\n";
            return tomorrowSchedule.Aggregate(output, (current, lesson) => (string) (current + LessonToString(lesson)));
        }

        private string Week(string groupId)
        {
            dynamic data = JsonConvert.DeserializeObject(GetSchedule(groupId));
            if (data["schedules"] == null)
                return $"Group {groupId} is free this week!";
            var output = $"This week's schedule for group {groupId} is:\n";
            foreach (var day in data["schedules"])
            {
                output += "\n"+day["weekDay"] + ":\n";
                bool freeDay = true;
                foreach (var lesson in day["schedule"])
                {
                    JArray weekNumber = lesson["weekNumber"];
                    var currentWeek = data["currentWeekNumber"];
                    if (weekNumber.Count(kek => kek == currentWeek) == 0)
                        continue;
                    output += LessonToString(lesson);
                    freeDay = false;
                }

                if (freeDay) output += "This day is free this week!\n";
            }

            return output;
        }
        
        private static string LessonToString(dynamic lesson)
        {
            var output = "";
            output +=
                $"▻︎ {lesson["lessonTime"]}: {(lesson["numSubgroup"] == 0 ? "" : "Подгруппа " + lesson["numSubgroup"].ToString() + " ")} {lesson["lessonType"]} {lesson["subject"]} {lesson["note"]}";
            foreach (var employee in lesson["employee"])
                output += $", {employee["fio"]}";
            foreach (var auditory in lesson["auditory"])
                output += $", {auditory}";
            output += "\n";
            return output;
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
                    throw new ArgumentException();
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