﻿using GraphQL;
using GraphQL.MicrosoftDI;
using GraphQL.Types;
using System;
using System.Security.Claims;
using TimeTracker.GraphQL.Types.Time;
using TimeTracker.Models;
using TimeTracker.Repositories;
using TimeTracker.ViewModels;

namespace TimeTracker.GraphQL.Types.TimeQuery
{
    public class TimeQueryGraphqlType : ObjectGraphType
    {
        private readonly ITimeRepository _timeRepository;
        public TimeQueryGraphqlType(ITimeRepository timeRepository, IUserRepository userRepository)
        {
            _timeRepository = timeRepository;

            Field<TimeWithFlagOutPutGraphql>("getTime")
                .Resolve(context =>
                {
                    var userId = GetUserIdFromClaims(context.User!);
                    var time = _timeRepository.GetTime(userId);
                    return new TimeWithFlagViewModel(){Time = new TimeViewModel(CheckExpires(time, userId, _timeRepository)), IsStarted = time.StartTimeTrackDate != null};
                });
            Field<IntGraphType>("getTotalWorkTime")
                .Argument<NonNullGraphType<IntGraphType>>("id")
                .Resolve(context =>
                {
                    int id = context.GetArgument<int>("id");
                    return GetMonthWorkTime(id, userRepository);
                });
        }

        public static int GetUserIdFromClaims(ClaimsPrincipal user)
        {
            var id = int.Parse(user.Claims.FirstOrDefault(c => c.Type == "UserId")!.Value);
            return id;
        }

        public static TimeTracker.Models.Time CheckExpires(TimeTracker.Models.Time time, int userId, ITimeRepository repo)
        {
            if (time.ToDayDate == null)
            {
                time.ToDayDate = DateTime.Now;
            }
            else if (DateOnly.FromDateTime(ToUkraineDateTime(time.ToDayDate.Value)).AddDays(1) <= DateOnly.FromDateTime(ToUkraineDateTime(DateTime.Now)))
            {
                var date = DateOnly.FromDateTime(ToUkraineDateTime(time.ToDayDate.Value));

                if (date.DayNumber / 7 < DateOnly.FromDateTime(ToUkraineDateTime(DateTime.Now)).DayNumber / 7)
                {
                    time.WeekSeconds = 0;
                }

                time.ToDayDate = DateTime.Now;
                time.DaySeconds = 0;

                time.StartTimeTrackDate = null;
                time.EndTimeTrackDate = null;
            }
            else
            {
                TimeTrackManage(time, repo,userId);
            }

            repo.UpdateTime(time, userId,UpdateTimeE.FullTime);
            return time;
        }

        public static DateTime ToUkraineDateTime(DateTime date)
        {
            var dateTime = TimeZoneInfo.ConvertTime(date,TimeZoneInfo.FindSystemTimeZoneById("FLE Standard Time"));
            return dateTime;
        }

        public static void TimeTrackManage(TimeTracker.Models.Time time,ITimeRepository repo,int userId)
        {
            if (time.StartTimeTrackDate == null)
                return;
            
            if (time.EndTimeTrackDate == null)
                return;

            var addSecond = Convert.ToInt32((time.EndTimeTrackDate - time.StartTimeTrackDate).Value.TotalSeconds);

            time.DaySeconds+= addSecond;
            time.MonthSeconds += addSecond;
            time.WeekSeconds += addSecond;

            time.StartTimeTrackDate = null;
            time.EndTimeTrackDate = null;

        }

        public int GetMonthWorkTime(int id, IUserRepository userRepository)
        {
            User user = userRepository.GetUser(id);
            DateTime d = DateTime.Now.AddDays(1 - DateTime.Now.Day);
            DateTime nd = new DateTime(d.AddMonths(1).Year, d.AddMonths(1).Month, 1);
            int[] days = new int[DateTime.DaysInMonth(d.Year, d.Month)];
            Array.Fill(days, 8);
            int MonthWorkTime = 0;
            if (d.AddDays(days.Length).DayOfWeek == DayOfWeek.Sunday || d.AddDays(days.Length).DayOfWeek == DayOfWeek.Saturday)
            {
                days[days.Length - 1] = 0;
            }
            else
            {

                if (nd.DayOfWeek == DayOfWeek.Sunday || nd.DayOfWeek == DayOfWeek.Saturday)
                {
                    days[days.Length - 1] = 7;
                }
                else
                {
                    days[days.Length - 1] = 8;
                }
            }

            for (int i = days.Length - 1; i > 0; i--)
            {
                if (d.AddDays(i - 1).DayOfWeek == DayOfWeek.Sunday || d.AddDays(i - 1).DayOfWeek == DayOfWeek.Saturday)
                {
                    days[i] = 0;
                }
            }
            for (int i = 0; i < days.Length - 1; i++)
            {
                if (days[i + 1] == 0 && days[i] == 8)
                {
                    days[i] = 7;
                }
                MonthWorkTime += days[i];
            }
            return MonthWorkTime * 36 * user.WorkHours;
        }
    }
}
