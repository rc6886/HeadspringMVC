using System;

namespace HSMVC.Domain
{
    public class Conference
    {
        public virtual Guid Id { get; set; }
        public virtual string Name { get; protected set; }
        public virtual string HashTag { get; protected set; }
        public virtual DateTime StartDate { get; protected set; }
        public virtual DateTime EndDate { get; protected set; }
        public virtual decimal Cost { get; protected set; }
        public virtual int AttendeeCount { get; protected set; }
        public virtual int SessionCount { get; protected set; }

        public Conference() {}

        public Conference(string name, string hashTag, DateTime startDate, DateTime endDate, decimal cost, int attendeeCount, int sessionCount)
        {
            Name = name;
            HashTag = hashTag;
            StartDate = startDate;
            EndDate = endDate;
            Cost = cost;
            AttendeeCount = attendeeCount;
            SessionCount = sessionCount;
        }


        public virtual void ChangeName(string name)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            if (name == string.Empty)
                throw new ArgumentOutOfRangeException("name", "Must be a non-empty string.");

            Name = name;
        }

        public virtual void ChangeHashTag(string hashTag)
        {
            if (hashTag == null)
                throw new ArgumentNullException("hashTag");

            if (hashTag == string.Empty)
                throw new ArgumentOutOfRangeException("hashTag", "Must be a non-empty string.");

            HashTag = hashTag;
        }

        public virtual void ChangeDates(DateTime startDate, DateTime endDate)
        {
            if (endDate < startDate)
                throw new ArgumentOutOfRangeException("endDate", "Must be on or after startDate");

            StartDate = startDate;
            EndDate = endDate;
        }

        public virtual void ChangeCost(decimal cost)
        {
            Cost = cost;
        }
    }
}