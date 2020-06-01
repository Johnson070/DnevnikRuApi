using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiDiaryLibrary
{
    public class ApiDiary
    {
        /// <summary>
        /// Инициализация ApiDiary
        /// </summary>
        /// <param name="login">Логин</param>
        /// <param name="pass">Пароль</param>
        /// <param name="client_id">Открытый id</param>
        /// <param name="client_secret">Секретный id</param>
        /// <param name="scope">Разрешения</param>
        public ApiDiary(string login, string pass, string client_id, string client_secret, string scope)
        {
            var client = new RestClient
            {
                BaseUrl = new Uri("https://api.dnevnik.ru/v2/authorizations/bycredentials")
            };

            var getToken = new RestRequest();

            getToken.AddHeader("Content-Type", "application/json");
            getToken.AddJsonBody(JsonConvert.SerializeObject(new Credentials(pass, login, client_id, client_secret, scope)));

            var response = client.Post(getToken);

            CheckConnection(response);

            var json = ((JObject)JsonConvert.DeserializeObject(response.Content));

            keyAccess = json["accessToken"].Value<string>();
        }

        /// <summary>
        /// Инициализация ApiDiary
        /// </summary>
        /// <param name="login">Логин</param>
        /// <param name="pass">Пароль</param>
        public ApiDiary(string login, string pass)
        {
            var client = new RestClient
            {
                BaseUrl = new Uri("https://api.dnevnik.ru/v2/authorizations/bycredentials")
            };

            var getToken = new RestRequest();

            getToken.AddHeader("Content-Type", "application/json");
            getToken.AddParameter("application/json", JsonConvert.SerializeObject(new Credentials(pass, login)), ParameterType.RequestBody);

            var response = client.Post(getToken);

            CheckConnection(response);

            var json = ((JObject)JsonConvert.DeserializeObject(response.Content));

            keyAccess = json["accessToken"].Value<string>();
        }


        /// <summary>
        /// Инициализация ApiDiary
        /// </summary>
        /// <param name="keyAccess">ключ-токен</param>
        public ApiDiary(string keyAccess)
        {
            this.keyAccess = keyAccess;

            var content = Get<IRestResponse>(new RestRequest(), "users/me");

            CheckConnection(content);
        }

        private string keyAccess;

        private const string apiUrl = "https://api.dnevnik.ru/v2.0/";

        public string GetAccessToken() => keyAccess;

        private IRestResponse Get<type>(RestRequest request, string url)
        {
            var client = new RestClient
            {
                BaseUrl = new Uri(apiUrl + url)
            };

            request.AddHeader("Access-Token", keyAccess);
            request.AddHeader("Accept", "application/json");

            return client.Get(request);
        }

        private string Get(RestRequest request, string url)
        {
            var client = new RestClient
            {
                BaseUrl = new Uri(apiUrl + url)
            };

            request.AddHeader("Access-Token", keyAccess);
            request.AddHeader("Accept", "application/json");

            return client.Get(request).Content;
        }

        private string Post(RestRequest request, string url)
        {
            var client = new RestClient
            {
                BaseUrl = new Uri(apiUrl + url)
            };

            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Access-Token", keyAccess);

            return client.Post(request).Content;
        }

        private string Delete(RestRequest request, string url)
        {
            var client = new RestClient
            {
                BaseUrl = new Uri(apiUrl + url)
            };

            request.AddHeader("Access-Token", keyAccess);

            return client.Delete(request).Content;
        }

        private string Put(RestRequest request, string url)
        {
            var client = new RestClient
            {
                BaseUrl = new Uri(apiUrl + url)
            };

            request.AddHeader("Access-Token", keyAccess);

            return client.Put(request).Content;
        }

        private void CheckConnection(IRestResponse content)
        {
            JObject json;

            if (content.StatusCode != System.Net.HttpStatusCode.ServiceUnavailable && content.StatusCode != System.Net.HttpStatusCode.MethodNotAllowed)
            {
                json = ((JObject)JsonConvert.DeserializeObject(content.Content));

                if (json.ContainsKey("type"))
                    switch (json["type"].Value<string>())
                    {
                        case string text when (text == "authorizationFailed" || text == "invalidToken" || text == "parameterInvalid"):
                            throw new Exception(json["description"].Value<string>());
                        case string text when (text == "apiServerError" || text == "apiUnknownError"):
                            throw new Exception("Неизвестная ошибка в API, проверьте правильность параметров");
                    }
            }
            else throw new Exception("Сайт лежит или ведутся технические работы, использование api временно невозможно");
        }


        /*
         * 
         * Запросы
         * 
         */

        private string Mass2String<t>(t[] mass)
        {
            string text = "[";

            foreach (t part in mass)
                text += part + ", ";

            text = text.Substring(0, text.Length - 3) + "]";

            return text;
        }

        /// <summary>
        /// Возращает все школы персоны
        /// </summary>
        /// <returns>json с schoolsIds</returns>
        public string GetSchool() => Get(new RestRequest(), "schools/person-schools");

        /// <summary>
        /// Профиль текущего пользователя
        /// </summary>
        /// <returns></returns>
        public string GetInfo() => Get(new RestRequest(), "users/me");


        public string GetClassmates() => Get(new RestRequest(), "users/me/classmates");

        /// <summary>
        /// Получение контекстной информации по пользователю
        /// </summary>
        /// <returns>
        /// json строка с контекстной информацией о пользователе
        /// </returns>
        public string GetContext() => Get(new RestRequest(), "users/me/context");


        public string GetOrganizations() => Get(new RestRequest(), "users/me/organizations");


        public string GetOrganizationInfo(long organizationId) => Get(new RestRequest(), $"users/me/organizations/{organizationId}");

        /// <summary>
        /// Получение контекстной информации по пользователю(userId)
        /// </summary>
        /// <param name="userId">userId персоны</param>
        /// <returns></returns>
        public string GetUserContext(long userId) => Get(new RestRequest(), $"users/{userId}/context");

        /// <summary>
        /// Список участий в школах для текущего пользователя
        /// </summary>
        /// <param name="userId">userId персоны</param>
        /// <returns></returns>
        public string GetUserMemberShips(long userId) => Get(new RestRequest(), $"users/{userId}/school-memberships");

        /// <summary>
        /// Список участий в школах для произвольного пользователя
        /// </summary>
        /// <param name="userId">userId персоны</param>
        /// <returns></returns>
        public string GetUserEducation(long userId) => Get(new RestRequest(), $"users/{userId}/education");

        /// <summary>
        /// Список участий в школах для произвольного пользователя
        /// </summary>
        /// <param name="personId">personId персоны</param>
        /// <returns></returns>
        public string GetPersonMemberships(long personId) => Get(new RestRequest(), $"users/{personId}/education");

        /// <summary>
        /// Список идентификаторов(schoolIds) школ текущего пользователя
        /// </summary>
        /// <returns></returns>
        public string GetSchools() => Get(new RestRequest(), "users/me/schools");

        /// <summary>
        /// Список идентификаторов(schoolIds) школ произвольного пользователя
        /// </summary>
        /// <param name="userId">userId персоны</param>
        /// <returns></returns>
        public string GetUserSchools(long userId) => Get(new RestRequest(), $"users/{userId}/schools");

        /// <summary>
        /// Список идентификаторов(groupIds) классов текущего пользователя
        /// </summary>
        /// <returns></returns>
        public string GetEduGroups() => Get(new RestRequest(), "users/me/edu-groups");

        /// <summary>
        /// Список идентификаторов(groupIds) классов текущего пользователя
        /// </summary>
        /// <param name="userId">userId персоны</param>
        /// <returns></returns>
        public string GetUserEduGroups(long userId) => Get(new RestRequest(), $"users/{userId}/edu-groups");

        /// <summary>
        /// Список участий в школах для текущего пользователя
        /// </summary>
        /// <returns></returns>
        public string GetMemberships() => Get(new RestRequest(), "users/me/school-memberships");

        /// <summary>
        /// Класс или учебная группа
        /// </summary>
        /// <param name="eduGroupId">eduGroupId или groupId персоны</param>
        /// <returns></returns>
        public string GetGroupInfo(long eduGroupId) => Get(new RestRequest(), $"edu-groups/{eduGroupId}");

        /// <summary>
        /// Список учебных групп
        /// </summary>
        /// <param name="eduGroupsList"></param>
        /// <returns></returns>
        public string GetGroupsInfo(params long[] eduGroupsList)
        {
            var request = new RestRequest();
            request.AddParameter("eduGroups", Mass2String(eduGroupsList));
            return Get(request, "edu-groups");
        }

        /// <summary>
        /// Список классов в школе
        /// </summary>
        /// <param name="schoolId">schoolId школы</param>
        /// <returns></returns>
        public string GetSchoolGroups(long schoolId) => Get(new RestRequest(), $"schools/{schoolId}/edu-groups");

        /// <summary>
        /// Учебные группы персоны
        /// </summary>
        /// <param name="personId">personId персоны</param>
        /// <returns></returns>
        public string GetPersonGroups(long personId) => Get(new RestRequest(), $"persons/{personId}/edu-groups");

        /// <summary>
        /// Все учебные группы персоны
        /// </summary>
        /// <param name="personId">personId персоны</param>
        /// <returns></returns>
        public string GetPersonGroupsAll(long personId) => Get(new RestRequest(), $"persons/{personId}/edu-groups/all");

        /// <summary>
        /// Учебные группы персоны в школе
        /// </summary>
        /// <param name="personId">personId персоны</param>
        /// <param name="schoolId">schoolId школы</param>
        /// <returns></returns>
        public string GetPersonSchoolGroups(long personId, int schoolId) => Get(new RestRequest(), $"persons/{personId}/schools/{schoolId}/edu-groups");


        public string GetGroupsPupils(long eduGroupId) => Get(new RestRequest(), $"edu-groups/{eduGroupId}/persons");


        public string GetGroupsParallel(long groupId) => Get(new RestRequest(), $"edu-groups/{groupId}/parallel");


        public string GetGroupMarks(long groupId) => Get(new RestRequest(), $"edu-groups/{groupId}/final-marks");


        public string GetPersonGroupMarks(long personId, long groupId, DateTime startTime, DateTime endTime) => Get(new RestRequest(), $"persons/{personId}/edu-groups/{groupId}/marks/{startTime.ToUniversalTime().ToString("o")}/{endTime.ToUniversalTime().ToString("o")}");


        public string GetPersonGroupMarks(long personId, long groupId) => Get(new RestRequest(), $"persons/{personId}/edu-groups/{groupId}/marks/{DateTime.UtcNow.ToUniversalTime().ToString("o")}/{DateTime.UtcNow.ToUniversalTime().ToString("o")}");


        public string GetPersonGroupMarksFinal(long personId, long groupId) => Get(new RestRequest(), $"persons/{personId}/edu-groups/{groupId}/final-marks");


        public string GetPersonGroupMarksAllFinal(long personId, long groupId) => Get(new RestRequest(), $"persons/{personId}/edu-groups/{groupId}/allfinalmarks");


        public string GetLessonsSubject(long groupId, long subjectId, DateTime startTime, DateTime endTime) => Get(new RestRequest(), $"edu-groups/{groupId}/subjects/{subjectId}/lessons/{startTime.ToUniversalTime().ToString("o")}/{endTime.ToUniversalTime().ToString("o")}");


        public string GetLessonsSubject(long groupId, long subjectId) => Get(new RestRequest(), $"edu-groups/{groupId}/subjects/{subjectId}/lessons/{DateTime.UtcNow.ToUniversalTime().ToString("o")}/{DateTime.UtcNow.ToUniversalTime().ToString("o")}");


        public string GetGroupSubjectFinalMarks(long groupId, long subjectId) => Get(new RestRequest(), $"edu-groups/{groupId}/subjects/{subjectId}/final-marks");


        public string GetFriends() => Get(new RestRequest(), $"users/me/friends");


        public string GetUserFriends(long userId) => Get(new RestRequest(), $"users/{userId}/friends");


        public string GetSchoolHomework(long schoolId)
        {
            var request = new RestRequest();
            request.AddParameter("startDate", DateTime.UtcNow.ToUniversalTime().ToString("o"));
            request.AddParameter("endDate", DateTime.UtcNow.ToUniversalTime().ToString("o"));
            return Get(request, $"users/me/school/{schoolId}/homeworks");
        }


        public string GetSchoolHomework(long schoolId, DateTime startTime, DateTime endTime)
        {
            var request = new RestRequest();
            request.AddParameter("startDate", startTime.ToUniversalTime().ToString("o"));
            request.AddParameter("endDate", endTime.ToUniversalTime().ToString("o"));
            return Get(request, $"users/me/school/{schoolId}/homeworks");
        }


        public string GetHomeworkById(params long[] homeworkId)
        {
            var request = new RestRequest();
            request.AddParameter("homeworkId", Mass2String(homeworkId));
            return Get(request, "users/me/school/homeworks");
        }


        public string GetPersonHomework(long schoolId, long personId, DateTime startTime, DateTime endTime)
        {
            var request = new RestRequest();
            request.AddParameter("startDate", startTime.ToUniversalTime().ToString("o"));
            request.AddParameter("endDate", endTime.ToUniversalTime().ToString("o"));
            return Get(request, $"persons/{personId}/school/{schoolId}/homeworks");
        }


        public string GetPersonHomework(long schoolId, long personId)
        {
            var request = new RestRequest();
            request.AddParameter("startDate", DateTime.UtcNow.ToUniversalTime().ToString("o"));
            request.AddParameter("endDate", DateTime.UtcNow.ToUniversalTime().ToString("o"));
            return Get(request, $"persons/{personId}/school/{schoolId}/homeworks");
        }


        public string DeleteLessonLog(long lessonId, long personId)
        {
            var request = new RestRequest();
            request.AddParameter("person", personId);
            return Delete(request, $"lessons/{lessonId}/log-entries");
        }


        public string GetLessonLog(long lessonId) { return string.Empty; } ///////Дописать!!!!!!!!!!!
        /*
         * """
         * lesson_log_entry example:
         *{
            "person": 0,
            "lesson": 0,
            "person_str": "string",
            "lesson_str": "string",
            "comment": "string",
            "status": "string",
            "createdDate": "2019-09-15T16:35:53.853Z"
        }
        """
        lesson_log = self.post(
            f"lessons/{lesson_id}/log-entries",
            data={"lessonLogEntry": lesson_log_entry},
        )
        */


        public string PutLessonLog(long lessonId, long personId) { return string.Empty; } ///////Дописать!!!!!!!!!!!
        /*"""
                lesson_log_entry example:
                {
                    "person": 0,
                    "lesson": 0,
                    "person_str": "string",
                    "lesson_str": "string",
                    "comment": "string",
                    "status": "string",
                    "createdDate": "2019-09-15T16:35:53.853Z"
                }
                """
        lesson_log = self.put(
            f"lessons/{lesson_id}/log-entries",
            data={"person": person_id, "lessonLogEntry": lesson_log_entry},
        )
        */


        public string GetLessonLogs(params long[] lessonsIds)
        {
            var request = new RestRequest();
            request.AddParameter("lessons", Mass2String(lessonsIds));
            return Delete(request, $"lesson-log-entries");
        }


        public string GetPersonLessonLog(long personId, long lessonId) => Get(new RestRequest(), $"lesson-log-entries/lesson/{lessonId}/person/{personId}");


        public string GetGroupLessonLog(long groupId, long subjectId, DateTime startTime, DateTime endTime)
        {
            var request = new RestRequest();
            request.AddParameter("subject", subjectId);
            request.AddParameter("from", startTime.ToUniversalTime().ToString("o"));
            request.AddParameter("to", endTime.ToUniversalTime().ToString("o"));
            return Get(request, $"lesson-log-entries/group/{groupId}");
        }


        public string GetGroupLessonLog(long groupId, long subjectId)
        {
            var request = new RestRequest();
            request.AddParameter("subject", subjectId);
            request.AddParameter("from", DateTime.UtcNow.ToUniversalTime().ToString("o"));
            request.AddParameter("to", DateTime.UtcNow.ToUniversalTime().ToString("o"));
            return Get(request, $"lesson-log-entries/group/{groupId}");
        }


        public string GetPersonSubjectLessonLog(long personId, long subjectId, DateTime startTime, DateTime endTime)
        {
            var request = new RestRequest();
            request.AddParameter("subject", subjectId);
            request.AddParameter("from", startTime.ToUniversalTime().ToString("o"));
            request.AddParameter("to", endTime.ToUniversalTime().ToString("o"));
            return Get(request, $"lesson-log-entries/person/{personId}/subject/{subjectId}");
        }


        public string GetPersonSubjectLessonLog(long personId, long subjectId)
        {
            var request = new RestRequest();
            request.AddParameter("subject", subjectId);
            request.AddParameter("from", DateTime.UtcNow.ToUniversalTime().ToString("o"));
            request.AddParameter("to", DateTime.UtcNow.ToUniversalTime().ToString("o"));
            return Get(request, $"lesson-log-entries/person/{personId}/subject/{subjectId}");
        }


        public string GetPersonLessonLogs(long personId, DateTime startTime, DateTime endTime)
        {
            var request = new RestRequest();
            request.AddParameter("startDate", startTime.ToUniversalTime().ToString("o"));
            request.AddParameter("endDate", endTime.ToUniversalTime().ToString("o"));
            return Get(request, $"persons/{personId}/lesson-log-entries");
        }


        public string GetPersonLessonLogs(long personId)
        {
            var request = new RestRequest();
            request.AddParameter("startDate", DateTime.UtcNow.ToUniversalTime().ToString("o"));
            request.AddParameter("endDate", DateTime.UtcNow.ToUniversalTime().ToString("o"));
            return Get(request, $"persons/{personId}/lesson-log-entries");
        }


        public string GetLessonLogStatuses() => Get(new RestRequest(), $"lesson-log-entries/statuses");


        public string GetLessonInfo(long lessonId) => Get(new RestRequest(), $"lessons/{lessonId}");


        public string GetManyLessonsInfo(params long[] lessonsList)// Get(new RestRequest(), $"lessons/many");
        {
            var request = new RestRequest();
            request.AddParameter("lessons", Mass2String(lessonsList));
            return Get(request, $"lessons/many");
        }


        public string GetGroupLessonsInfo(long groupId, DateTime startTime, DateTime endTime) => Get(new RestRequest(), $"edu-groups/{groupId}/lessons/{startTime.ToUniversalTime().ToString("o")}/{endTime.ToUniversalTime().ToString("o")}");


        public string GetGroupLessonsInfo(long groupId) => Get(new RestRequest(), $"edu-groups/{groupId}/lessons/{DateTime.UtcNow.ToUniversalTime().ToString("o")}/{DateTime.UtcNow.ToUniversalTime().ToString("o")}");


        public string GetMarksHistogram(long workId) => Get(new RestRequest(), $"works/{workId}/marks/histogram");


        public string GetSubjectMarksHistogram(long periodId, long subjectId, long groupId) => Get(new RestRequest(), $"periods/{periodId}/subjects/{subjectId}/groups/{groupId}/marks/histogram");


        public string GetMarkById(long markId) => Get(new RestRequest(), $"marks/{markId}");


        public string GetMarksByWork(long workId) => Get(new RestRequest(), $"works/{workId}/marks");


        public string GetMarksByLesson(long lessonId) => Get(new RestRequest(), $"lessons/{lessonId}/marks");


        public string GetMarksByLessons(params long[] lessonsIds)// => Get(new RestRequest(), $"lessons/marks");
        {
            var request = new RestRequest();
            request.AddParameter("lessons", Mass2String(lessonsIds));
            return Get(request, $"lessons/many");
        }


        public string GetGroupMarksPeriod(long groupId, DateTime startTime, DateTime endTime) => Get(new RestRequest(), $"edu-groups/{groupId}/marks/{startTime}/{endTime}");


        public string GetGroupMarksPeriod(long groupId) => Get(new RestRequest(), $"edu-groups/{groupId}/marks/{DateTime.UtcNow.ToUniversalTime().ToString("o")}/{DateTime.UtcNow.ToUniversalTime().ToString("o")}");


        public string GetGroupSubjectMarks(long groupId, long subjectId, DateTime startTime, DateTime endTime) => Get(new RestRequest(), $"edu-groups/{groupId}/subjects/{subjectId}/marks/{startTime.ToUniversalTime().ToString("o")}/{endTime.ToUniversalTime().ToString("o")}");


        public string GetGroupSubjectMarks(long groupId, long subjectId) => Get(new RestRequest(), $"edu-groups/{groupId}/subjects/{subjectId}/marks/{DateTime.UtcNow.ToUniversalTime().ToString("o")}/{DateTime.UtcNow.ToUniversalTime().ToString("o")}");


        public string GetPersonMarks(long personId, long schoolId, DateTime startTime, DateTime endTime) => Get(new RestRequest(), $"persons/{personId}/schools/{schoolId}/marks/{startTime.ToUniversalTime().ToString("o")}/{endTime.ToUniversalTime().ToString("o")}");


        public string GetPersonMarks(long personId, long schoolId) => Get(new RestRequest(), $"persons/{personId}/schools/{schoolId}/marks/{DateTime.UtcNow.ToUniversalTime().ToString("o")}/{DateTime.UtcNow.ToUniversalTime().ToString("o")}");


        public string GetPersonLessonsMarks(long personId, long lessonId) => Get(new RestRequest(), $"persons/{personId}/lessons/{lessonId}/marks");


        public string GetPersonWorkMarks(long personId, long workId) => Get(new RestRequest(), $"persons/{personId}/works/{workId}/marks");


        public string GetPersonSubjectMarks(long personId, long subjectId, DateTime startTime, DateTime endTime) => Get(new RestRequest(), $"persons/{personId}/subjects/{subjectId}/marks/{startTime.ToUniversalTime().ToString("o")}/{endTime.ToUniversalTime().ToString("o")}");


        public string GetPersonSubjectMarks(long personId, long subjectId) => Get(new RestRequest(), $"persons/{personId}/subjects/{subjectId}/marks/{DateTime.UtcNow.ToUniversalTime().ToString("o")}/{DateTime.UtcNow.ToUniversalTime().ToString("o")}");


        public string GetMarksByDate(long personId, DateTime date) => Get(new RestRequest(), $"persons/{personId}/marks/{date.ToUniversalTime().ToString("o")}");


        public string GetMarksByDate(long personId) => Get(new RestRequest(), $"persons/{personId}/marks/{DateTime.UtcNow.ToUniversalTime().ToString("o")}");


        public string GetMarksValues() => Get(new RestRequest(), $"marks/values");


        public string GetMarksValuesByType(string marksType) => Get(new RestRequest(), $"marks/values/type/{marksType}");


        public string GetPersonAverageMarks(long personId, long period) => Get(new RestRequest(), $"persons/{personId}/reporting-periods/{period}/avg-mark");


        public string GetPersonAverageMarksBySubject(long personId, long subjectId, long period) => Get(new RestRequest(), $"persons/{personId}/reporting-periods/{period}/subjects/{subjectId}/avg-mark");


        public string GetGroupAverageMarksByDate(long groupId, long period, DateTime date) => Get(new RestRequest(), $"edu-groups/{groupId}/reporting-periods/{period}/avg-marks/{date.ToUniversalTime().ToString("o")}");


        public string GetGroupAverageMarksByTime(long groupId, DateTime startTime, DateTime endTime) => Get(new RestRequest(), $"edu-groups/{groupId}/avg-marks/{startTime.ToUniversalTime().ToString("o")}/{endTime.ToUniversalTime().ToString("o")}");


        public string GetGroupAverageMarksByTime(long groupId) => Get(new RestRequest(), $"edu-groups/{groupId}/avg-marks/{DateTime.UtcNow.ToUniversalTime().ToString("o")}/{DateTime.UtcNow.ToUniversalTime().ToString("o")}");


        public string GetFinalGroupMarks(long groupId) => Get(new RestRequest(), $"edu-group/{groupId}/criteria-marks-totals");


        public string GetFinalGroupMarksBySubject(long groupId, long subjectId) => Get(new RestRequest(), $"edu-group/{groupId}/subject/{subjectId}/criteria-marks-totals");


        public string GetGroupPersons(long groupId, bool include)
        {
            var request = new RestRequest();
            request.AddQueryParameter("includeArchive", include.ToString());
            return Get(request, $"edu-groups/{groupId}/persons");
        }


        public string GetPersonInfo(long personId) => Get(new RestRequest(), $"persons/{personId}");


        public string GetRecentPersonMarks(long personId, long groupId) => Get(new RestRequest(), $"persons/{personId}/group/{groupId}/recentmarks");


        public string GetGroupReports(long groupId) => Get(new RestRequest(), $"edu-groups/{groupId}/reporting-periods");


        public string GetPersonSchedule(long personId, long groupId, DateTime startTime, DateTime endTime)// => Get(new RestRequest(), $"persons/{personId}/groups/{groupId}/schedules");
        {
            var request = new RestRequest();
            request.AddParameter("startDate", startTime.ToUniversalTime().ToString("o"));
            request.AddParameter("endDate", endTime.ToUniversalTime().ToString("o"));
            return Get(request, $"persons/{personId}/groups/{groupId}/schedules");
        }


        public string GetPersonSchedule(long personId, long groupId)// => Get(new RestRequest(), $"persons/{personId}/groups/{groupId}/schedules");
        {
            var request = new RestRequest();
            request.AddParameter("startDate", DateTime.UtcNow.ToUniversalTime().ToString("o"));
            request.AddParameter("endDate", DateTime.UtcNow.ToUniversalTime().ToString("o"));
            return Get(request, $"persons/{personId}/groups/{groupId}/schedules");
        }


        public string GetBestSchools(DateTime startTime, DateTime endTime) => Get(new RestRequest(), $"school-rating/from/{startTime.ToUniversalTime().ToString("o")}/to/{endTime.ToUniversalTime().ToString("o")}");


        public string GetBestSchools() => Get(new RestRequest(), $"school-rating/from/{DateTime.UtcNow.ToUniversalTime().ToString("o")}/to/{DateTime.UtcNow.ToUniversalTime().ToString("o")}");


        public string GetSchoolProfile(long schoolId) => Get(new RestRequest(), $"schools/{schoolId}");


        public string GetSchoolsProfiles(params long[] schoolIds)// Get(new RestRequest(), $"schools");
        {
            var request = new RestRequest();
            request.AddParameter("schools", Mass2String(schoolIds));
            return Get(request, $"schools");
        }


        public string GetMySchools() => Get(new RestRequest(), $"schools/person-schools");


        public string GetAllSchools() => Get(new RestRequest(), $"schools/cities");


        public string GetSchoolParams(long schoolId) => Get(new RestRequest(), $"schools/{schoolId}/parameters");


        public string GetGroupSubjects(long groupId) => Get(new RestRequest(), $"edu-groups/{groupId}/subjects");


        public string GetSchoolSubjects(long schoolId) => Get(new RestRequest(), $"schools/{schoolId}/subjects");


        public string GetTaskInfo(long taskId) => Get(new RestRequest(), $"tasks/{taskId}");


        public string GetLessonsTask(params long[] lessonsIds)// => Get(new RestRequest(), $"tasks");
        {
            var request = new RestRequest();
            request.AddParameter("lessons", Mass2String(lessonsIds));
            return Get(request, $"tasks");
        }


        public string GetLessonTask(long lessonId) => Get(new RestRequest(), $"lessons/{lessonId}/tasks");


        public string GetPersonTasks(long personId, long subjectId, DateTime startTime, DateTime endTime)// Get(new RestRequest(), $"persons/{personId}/tasks");
        {
            var request = new RestRequest();
            request.AddParameter("subject", subjectId);
            request.AddParameter("from", startTime.ToUniversalTime().ToString("o"));
            request.AddParameter("to", endTime.ToUniversalTime().ToString("o"));
            return Get(request, $"lesson-log-entries/person/{personId}/subject/{subjectId}");
        }


        public string GetPersonTasks(long personId, long subjectId)// Get(new RestRequest(), $"persons/{personId}/tasks");
        {
            var request = new RestRequest();
            request.AddParameter("subject", subjectId);
            request.AddParameter("from", DateTime.UtcNow.ToUniversalTime().ToString("o"));
            request.AddParameter("to", DateTime.UtcNow.ToUniversalTime().ToString("o"));
            return Get(request, $"lesson-log-entries/person/{personId}/subject/{subjectId}");
        }


        public string GetTeacherStudents(long teacherId) => Get(new RestRequest(), $"teacher/{teacherId}/students");


        public string GetSchoolTeachers(long schoolId) => Get(new RestRequest(), $"schools/{schoolId}/teachers");


        public string GetSchoolTimetable(long schoolId) => Get(new RestRequest(), $"schools/{schoolId}/timetables");


        public string GetGroupTimetable(long groupId) => Get(new RestRequest(), $"edu-groups/{groupId}/timetables");


        public string GetFeed(DateTime date)// Get(new RestRequest(), $"users/me/feed");
        {
            var request = new RestRequest();
            request.AddParameter("date", date.ToUniversalTime().ToString("o"));
            return Get(request, $"users/me/feed");
        }


        public string GetFeed()// Get(new RestRequest(), $"users/me/feed");
        {
            var request = new RestRequest();
            request.AddParameter("date", DateTime.UtcNow.ToUniversalTime().ToString("o"));
            return Get(request, $"users/me/feed");
        }


        public string GetStudentsGroupsList(params long[] groupsIds)// => Get(new RestRequest(), $"edu-groups/students");
        {
            var request = new RestRequest();
            request.AddParameter("groups", Mass2String(groupsIds));
            return Get(request, $"edu-groups/students");
        }


        public string GetUserGroups(long userId) => Get(new RestRequest(), $"users/{userId}/groups");


        public string GetPersonChildren(long personId) => Get(new RestRequest(), $"user/{personId}/children");


        public string GetUserChildren(long userId) => Get(new RestRequest(), $"user/{userId}/children");


        public string GetChildren() => Get(new RestRequest(), $"users/me/children");


        public string GetUserRelatives(long userId) => Get(new RestRequest(), $"users/{userId}/relatives");


        public string GetRelatives() => Get(new RestRequest(), $"users/me/relatives");


        public string GetChildrenRelatives() => Get(new RestRequest(), $"users/me/childrenrelatives");


        public string GetUserInfo(long userId) => Get(new RestRequest(), $"users/{userId}");


        public string GetRoles() => Get(new RestRequest(), $"users/me/roles");


        public string GetUserRoles(long userId) => Get(new RestRequest(), $"users/{userId}/roles");


        public string GetGroupAverageMarks(long groupId, DateTime startTime, DateTime endTime) => Get(new RestRequest(), $"edu-groups/{groupId}/wa-marks/{startTime.ToUniversalTime().ToString("o")}/{endTime.ToUniversalTime().ToString("o")}");


        public string GetGroupAverageMarks(long groupId) => Get(new RestRequest(), $"edu-groups/{groupId}/wa-marks/{DateTime.UtcNow.ToUniversalTime().ToString("o")}/{DateTime.UtcNow.ToUniversalTime().ToString("o")}");


        public string GetLessonWorks(long lessonId)// Get(new RestRequest(), $"works");
        {
            var request = new RestRequest();
            request.AddParameter("lesson", lessonId);
            return Get(request, $"works");
        }


        public string CreateLessonWork() => "Not avaliable!"; // Get(new RestRequest(), $"works");
        /*
         *"""
        work example:
                {
          "id": 0,
          "id_str": "string",
          "type": 0,
          "workType": 0,
          "markType": "string",
          "markCount": 0,
          "lesson": 0,
          "lesson_str": "string",
          "displayInJournal": true,
          "status": "string",
          "eduGroup": 0,
          "eduGroup_str": "string",
          "tasks": [
            {
              "id": 0,
              "id_str": "string",
              "person": 0,
              "person_str": "string",
              "work": 0,
              "work_str": "string",
              "status": "string",
              "targetDate": "2019-09-15T16:35:54.384Z"
            }
          ],
          "text": "string",
          "periodNumber": 0,
          "periodType": "string",
          "subjectId": 0,
          "isImportant": true,
          "targetDate": "2019-09-15T16:35:54.384Z",
          "sentDate": "2019-09-15T16:35:54.384Z",
          "createdBy": 0,
          "files": [
            0
          ],
          "oneDriveLinks": [
            0
          ]
        } 
         */


        public string GetWorkTypes(long schoolId) => Get(new RestRequest(), $"work-types/{schoolId}");


        public string InviteToEvent(long inviteId) => Get(new RestRequest(), $"events/{inviteId}/invite");
    }
}
