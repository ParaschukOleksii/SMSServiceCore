﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using WebApp.ModelsForSendingInUI;
using WebCustomerApp.BAL.Interfaces;
using WebCustomerApp.BAL.Repository;
using WebCustomerApp.Data;
using WebCustomerApp.Models;
using WebCustomerApp.Models.MessageViewModels;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebApp.Controllers
{
    public class MessageController : Controller
    {
        private IUnitOfWork _unitOfWork;

        public MessageController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        // GET: /<controller>/
        public IActionResult Index()
        {
            return View();
        }

        [Route("~/Message/GetMessagesCount")]
        public int GetMessagesCount()
        {
            List<Message> phones = _unitOfWork._messageRepository.GetMessagesBySenderId(_unitOfWork._userManager.GetUserId(User));
            return phones.Count;
        }

        [Route("~/Phone/GetPhones")]
        [HttpGet]
        public List<Message> GetMessage(int numberOfMessage)
        {
            return _unitOfWork._messageRepository.GetMessagesBySenderId(_unitOfWork._userManager.GetUserId(User))
                .Skip(numberOfMessage - 1).Take(1).ToList();
        }

        [HttpGet]
        public ActionResult Messages()
        {
            List<ModelForMessageForSendingToUI> messagesWithRecipients = new List<ModelForMessageForSendingToUI>();
            List<Message> messages = _unitOfWork._messageRepository.GetMessagesBySenderId(_unitOfWork._userManager.GetUserId(User));
            foreach (var mes in messages)
            {
                List<Phone> phones = new List<Phone>();
                List<RecepientMessage> recepientMessages = _unitOfWork._recepientMessageRepository.GetRecepientsMessagesByMessageId(mes.MessageId);
                foreach (var recepientMes in recepientMessages)
                {
                    phones.Add(_unitOfWork._phoneRepository.GetById(recepientMes.PhoneId));
                }
                messagesWithRecipients.Add(new ModelForMessageForSendingToUI(mes, phones));
            }
            ViewBag.MessagesWithRecipients = messagesWithRecipients;
            return View();
        }

        [HttpPost]
        public ActionResult Messages(MessageModel messageModel)
        {
            Message message = new Message();
            message.SenderId = _unitOfWork._userManager.GetUserId(User);
            message.TextOfMessage = messageModel.TextOfMessage;
            _unitOfWork._messageRepository.Create(message);
            List<Phone> phones = _unitOfWork._phoneRepository.GetPhonesByUserId(_unitOfWork._userManager.GetUserId(User));
            List<Phone> recepients = new List<Phone>();
            List<Phone> newphones = new List<Phone>();
            foreach (var recepient in messageModel.Recepients)
            {
                Phone phone = phones.Find(item => item.PhoneNumber == recepient);
                if (phone != null)
                {
                    recepients.Add(phone);
                }
                else
                {
                    phone = new Phone();
                    phone.PhoneNumber = recepient;
                    phone.UserId = _unitOfWork._userManager.GetUserId(User);
                    newphones.Add(phone);
                }
            }
            foreach(var newphone in newphones)
            {
                _unitOfWork._phoneRepository.Create(newphone);
                recepients.Add(newphone);
            }
            foreach (var recepient in recepients)
            {
                RecepientMessage recepientMessage = new RecepientMessage();
                recepientMessage.MessageId = message.MessageId;
                recepientMessage.PhoneId = recepient.PhoneId;
                _unitOfWork._recepientMessageRepository.Create(recepientMessage);
            }
            _unitOfWork.SaveChanges();

            return Json(new { message.MessageId });
        }

    }
}
