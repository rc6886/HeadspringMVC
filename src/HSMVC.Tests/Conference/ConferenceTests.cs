using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using AutoMapper;
using HSMVC.DataAccess;
using HSMVC.DependencyResolution;
using HSMVC.Features.Conference.Commands;
using HSMVC.Features.Conference.Validation;
using HSMVC.Infrastructure;
using HSMVC.Infrastructure.AutoMapper;
using NUnit.Framework;
using Shouldly;

namespace HSMVC.Tests.Conference
{
    [TestFixture]
    public class ConferenceTests
    {
        private IConferenceRepository _repository;
        private TransactionScope _transactionScope;

        [SetUp]
        public void SetupTestFixture()
        {
            _repository = new ConferenceRepository(NHibernateHelper.OpenSession());
            _transactionScope = new TransactionScope();
            IoC.Initialize();
            Mapper.Initialize(cfg => cfg.AddProfile<AutoMapperProfile>());
        }

        [TearDown]
        public void TestDownTestFixture()
        {
            _transactionScope.Dispose();
        }

        [Test]
        public void ShouldGetAllConferences()
        {
            var conferences = _repository.GetAll().ToArray();

            conferences.Length.ShouldBe(3);

            conferences[0].Id.ShouldBe(Guid.Parse("0E3E638E-DA0B-47DF-B2FC-07F6CC7618DE"));
            conferences[1].Id.ShouldBe(Guid.Parse("C2AD9DAC-B936-442E-B46D-A73C0B69C147"));
            conferences[2].Id.ShouldBe(Guid.Parse("F41C8CFC-A6E3-4309-8023-D1425D294468"));
        }

        [Test]
        public void ShouldEditConference()
        {
            var conference = _repository.Load(Guid.Parse("0E3E638E-DA0B-47DF-B2FC-07F6CC7618DE"));

            conference.ChangeName("New Conference");
            conference.ChangeCost(241);
            conference.ChangeHashTag("#new");
            conference.ChangeDates(DateTime.Parse("01/01/2000"), DateTime.Parse("01/01/2005"));
            _repository.Save(conference);

            var editedConference = _repository.Load(Guid.Parse("0E3E638E-DA0B-47DF-B2FC-07F6CC7618DE"));

            editedConference.Name.ShouldBe("New Conference");
            editedConference.Cost.ShouldBe(241);
            editedConference.HashTag.ShouldBe("#new");
            editedConference.StartDate.ShouldBe(DateTime.Parse("01/01/2000"));
            editedConference.EndDate.ShouldBe(DateTime.Parse("01/01/2005"));
        }

        [Test]
        public void ShouldAddConference()
        {
            var conference = new Domain.Conference("Test Conference", "#Test", DateTime.Parse("03/03/2013"),
                DateTime.Parse("04/04/2014"), 127, 499, 2);

            _repository.Save(conference);

            var addedConference = _repository.FindByName("Test Conference");

            addedConference.Name.ShouldBe("Test Conference");
            addedConference.HashTag.ShouldBe("#Test");
            addedConference.StartDate.ShouldBe(DateTime.Parse("03/03/2013"));
            addedConference.EndDate.ShouldBe(DateTime.Parse("04/04/2014"));
            addedConference.Cost.ShouldBe(127);
            addedConference.AttendeeCount.ShouldBe(499);
            addedConference.SessionCount.ShouldBe(2);
        }

        [Test]
        public void ShouldBulkEditConferencesWithoutNameChange()
        {
            var conferences = _repository.GetAll();
            var conferenceBulkEditCommand = new ConferenceBulkEditCommand
            {
                Commands = conferences.Select(x => new ConferenceEditCommand
                {
                    Cost = x.Cost,
                    EndDate = x.EndDate,
                    HashTag = x.HashTag,
                    Id = x.Id,
                    Name = x.Name,
                    StartDate = x.StartDate
                }).ToList()
            };

            var validator = new ConferenceEditCommandValidator();
            var validationResults = conferenceBulkEditCommand.Commands.Select(command => validator.Validate(command)).ToList();
            validationResults.Any(x => x.Errors.Count > 0).ShouldBeFalse();
        }

        [Test]
        public void ShouldBulkEditConferences()
        {
            var conferences = _repository.GetAll();
            var conferenceBulkEditCommand = new ConferenceBulkEditCommand
            {
                Commands = conferences.Select(x => new ConferenceEditCommand
                {
                    Cost = x.Cost + 1,
                    EndDate = x.EndDate.AddDays(1),
                    HashTag = x.HashTag,
                    Id = x.Id,
                    Name = x.Name + "1",
                    StartDate = x.StartDate.AddDays(1)
                }).ToList()
            };

            foreach (var editCommand in conferenceBulkEditCommand.Commands)
            {
                var conference = _repository.Load(editCommand.Id);
                conference.ChangeName(editCommand.Name);
                conference.ChangeCost(editCommand.Cost);
                conference.ChangeDates(editCommand.StartDate.Value, editCommand.EndDate.Value);
                _repository.Save(conference);
            }

            var editedConferences = _repository.GetAll().ToArray();

            editedConferences[0].Name.ShouldBe("Game Developer Conference 20161");
            editedConferences[0].Cost.ShouldBe(51);
            editedConferences[0].StartDate.ShouldBe(DateTime.Parse("03/15/2016"));
            editedConferences[0].EndDate.ShouldBe(DateTime.Parse("03/19/2016"));

            editedConferences[1].Name.ShouldBe("PASS Summit1");
            editedConferences[1].Cost.ShouldBe(300);
            editedConferences[1].StartDate.ShouldBe(DateTime.Parse("10/28/2016"));
            editedConferences[1].EndDate.ShouldBe(DateTime.Parse("10/31/2016"));

            editedConferences[2].Name.ShouldBe("Build 20161");
            editedConferences[2].Cost.ShouldBe(200);
            editedConferences[2].StartDate.ShouldBe(DateTime.Parse("03/31/2016"));
            editedConferences[2].EndDate.ShouldBe(DateTime.Parse("04/02/2016"));
        }
    }
}