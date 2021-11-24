using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TeleSharp.TL;
using TeleSharp.TL.Channels;
using TeleSharp.TL.Messages;
using TeleSharp.TL.Users;
using TLSharp.Core;
using TLSharp.Core.Utils;

namespace TelegramCostumLib
{
    public class TelegramClientSingelton
    {
        public enum destType
        {
            user,
            group,
            channel
        }

        private static TelegramClientSingelton _instance;
        public static TelegramClientSingelton Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new TelegramClientSingelton();
                return _instance;
            }
            set
            {
                _instance = value;
            }
        }
        public TelegramClient client { get; set; }
        public TelegramUserDetails userDetails { get; }


        public TelegramClientSingelton()
        {
            userDetails = TelegramUserDetails.Instance;
            client = new TelegramClient(userDetails.ApiAppID, userDetails.ApiHASH);
        }


        public async Task BroadcastMessage(List<string> phoneNumbers, TelegramMessage message)
        {
            foreach (var p in phoneNumbers)
            {
                await SendMessageToUser(p, message);
            }
                 //await ? Elay: of course !!! (or use a task pool and await all of them.)
        }


        public async Task SendFile(MediaType type, string path, string fileName, TLChat chat, string mimeType)
        {
            // type : 1 - image, 0 - document

            var fileResult = await client.UploadFile(fileName, new StreamReader(path));
            if (type == MediaType.image)
            {
                // send image
                await client.SendUploadedPhoto(new TLInputPeerUser() { UserId = chat.Id }, fileResult, fileName);
            }
            else
            {
                // send another file
                await client.SendUploadedDocument(
                    new TLInputPeerUser() { UserId = chat.Id },
                    fileResult,
                    "",
                    mimeType,
                    new TLVector<TLAbsDocumentAttribute>());
            }
        }


        public async Task SendMessageToUser(string phoneNumber, TelegramMessage message)
        {

            if (phoneNumber == null || message == default)
                // no username set
                return;

            await Connect();

            

            var results = await client.GetContactsAsync();

            if (results.Users.Count < 1)
                // user not found
                return;

            // skip the first 3 digits (052, 053, ...etc)
            phoneNumber = phoneNumber.Substring(3);

            var contact = results.Users
                .Where(x => x.GetType() == typeof(TLUser))
                .Cast<TLUser>()
                .FirstOrDefault(o => o.Phone.Contains(phoneNumber));


            await SendPrivateMessage(contact, message);


            //TODO - add media support

        }

        public async Task SendMessageToGroupChannel(string selectedChat, bool group, TelegramMessage message)
        {

            if (selectedChat == null)
                return;

            await Connect();

            if (group)
            {
                _ = Task.Run(async () =>
                  {
                      var ls = await GetGroupUserList(selectedChat);
                      foreach (var user in ls)
                      {
                          if(message.mediaType != MediaType.Non)
                          {
                              var chat = new TLChat { Id = user.Id };
                              await SendFile(message.mediaType, message.MediaPath, message.fileName, chat, message.MimeType);
                          }
                          await SendPrivateMessage(user, message);
                      }
                          
                  });
            }
            else
            {
                var AllChats = await GetAllDialogs();
                var channelChat = AllChats.Chats
                     .OfType<TLChannel>()
                     .FirstOrDefault(c => c.Title == selectedChat);
                await SendToChannel(channelChat, message);
            }
        }

        public async Task<TLDialogsSlice> GetAllDialogs()
        {
            await Connect();
            return await client.GetUserDialogsAsync() as TLDialogsSlice;
        }

        private async Task<bool> SendPrivateMessage(TLUser myContact, TelegramMessage message)
        {
            try
            {
                if (message.mediaType != MediaType.Non)
                {
                    var chat = new TLChat() { Id = myContact.Id };

                    await SendFile(message.mediaType, message.MediaPath, message.fileName, chat, message.MimeType);
                }

                await client.SendMessageAsync(new TLInputPeerUser()
                {
                    UserId = myContact.Id
                }, message.Message);
                //TODO -Add media support
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        private async Task<bool> SendToChannel(TLChannel channelChat, TelegramMessage message)
        {
            try
            {
                if (message.mediaType != MediaType.Non)
                {
                    var chat = new TLChat() { Id = channelChat.Id };

                    await SendFile(message.mediaType, message.MediaPath, message.fileName, chat, message.MimeType);
                }
                await client.SendMessageAsync(new TLInputPeerChannel()
                {
                    ChannelId = channelChat.Id,
                    AccessHash = channelChat.AccessHash.Value
                }, message.Message);
                return true;

            }
            catch (Exception e)
            {
                return false;
            }
        }


        private async Task Connect()
        {
            if (client == null)
                client = new TelegramClient(userDetails.ApiAppID, userDetails.ApiHASH);
            await client.ConnectAsync();
        }

        public async Task<List<TLUser>> GetGroupUserList(string groupName)
        {
            var dialogs = await GetAllDialogs();
            var group = dialogs.Chats
                     .OfType<TLChat>()
                     .FirstOrDefault(c => c.Title == groupName);
            var req = new TLRequestGetFullChat { ChatId = group.Id };
            var res = await client.SendRequestAsync<TeleSharp.TL.Messages.TLChatFull>(req);
            var result = (res.FullChat as TeleSharp.TL.TLChatFull).Participants as TLChatParticipants;
            List<TLUser> output = new List<TLUser>();
            foreach (var user in result.Participants)
            {
                TLInputUser reqId;
                if (user.GetType() == typeof(TLChatParticipant))
                {
                    reqId = new TLInputUser { UserId = (user as TLChatParticipant).UserId };
                }
                else if (user.GetType() == typeof(TLChatParticipantAdmin))
                {
                    reqId = new TLInputUser { UserId = (user as TLChatParticipantAdmin).UserId };
                }
                else if (user.GetType() == typeof(TLChatParticipantCreator))
                {
                    reqId = new TLInputUser { UserId = (user as TLChatParticipantCreator).UserId };
                }
                else
                    continue;
                var request = new TLRequestGetFullUser
                {
                    Id = reqId
                };
                var response = await client.SendRequestAsync<object>(request);
                var temp = (response as TLUserFull).User;
                output.Add(temp as TLUser);
            }
            return output;
        }



    }
}