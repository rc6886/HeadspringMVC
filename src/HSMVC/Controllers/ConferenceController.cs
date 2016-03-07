using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using AutoMapper;
using HSMVC.DataAccess;
using HSMVC.Domain;
using HSMVC.Features.Conference.Commands;

namespace HSMVC.Controllers
{
    using Infrastructure;

    public class ConferenceController : Controller
    {
        private readonly IConferenceRepository _repository;

        public ConferenceController() :this(new ConferenceRepository(NHibernateHelper.OpenSession()))
        {
            
        }

        public ConferenceController(IConferenceRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public ActionResult Index()
        {
            var conferences = _repository.GetAll().ToList();
            return View(conferences);
        }

        [HttpGet]
        public ActionResult Edit(Guid id)
        {
            var conference = _repository.Load(id);
            var command = Mapper.Map<ConferenceEditCommand>(conference);
            return View(command);
        }

        [HttpPost]
        public ActionResult Edit(ConferenceEditCommand command)
        {
            if (!ModelState.IsValid) return View(command);

            var conference = _repository.Load(command.Id);
            conference.ChangeName(command.Name);
            conference.ChangeCost(command.Cost);
            conference.ChangeDates(command.StartDate.Value, command.EndDate.Value);
            _repository.Save(conference);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult Add()
        {
            return View(new ConferenceAddCommand());
        }

        [HttpPost]
        public ActionResult Add(ConferenceAddCommand command)
        {
            if (!ModelState.IsValid) return View(command);

            var conference = Mapper.Map<Conference>(command);
            _repository.Save(conference);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult BulkEdit()
        {
            var conferences = _repository.GetAll();
            var editCommands = Mapper.Map<IEnumerable<ConferenceEditCommand>>(conferences).ToList();
            return View(new ConferenceBulkEditCommand {Commands = editCommands});
        }

        [HttpPost]
        public ActionResult BulkEdit(ConferenceBulkEditCommand command)
        {
            if (!ModelState.IsValid) return View(command);

            var conferencesToEdit = new List<Conference>();
            foreach (var editCommand in command.Commands)
            {
                var conference = _repository.Load(editCommand.Id);
                conference.ChangeName(editCommand.Name);
                conference.ChangeCost(editCommand.Cost);
                conference.ChangeDates(editCommand.StartDate.Value, editCommand.EndDate.Value);
                conferencesToEdit.Add(conference);
            }

            _repository.BulkSave(conferencesToEdit);
            return RedirectToAction("Index");
        }
    }
}