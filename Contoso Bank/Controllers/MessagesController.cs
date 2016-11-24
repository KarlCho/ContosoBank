using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using Contoso_Bank.Models;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Contoso_Bank.DataModels;
using Contoso_Bank;

namespace Contoso_Bank
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            if (activity.Type == ActivityTypes.Message)
            {
                ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));

                StateClient stateClient = activity.GetStateClient();
                BotData userData = await stateClient.BotState.GetUserDataAsync(activity.ChannelId, activity.From.Id);

                var userMessage = activity.Text;
                string endOutput = "Hello and welcome to Contoso Bot if you want to see what I can do please type help, for a list of commands. type commands";



                // calculate something for us to return
                if (userData.GetProperty<bool>("SentGreeting"))
                {
                    endOutput = "Hello again if you want to log in please type 'user (name)'";
                }
                else
                {   
                    userData.SetProperty<bool>("SentGreeting", true);
                    await stateClient.BotState.SetUserDataAsync(activity.ChannelId, activity.From.Id, userData);
                }
                bool isRequest = true;

                if (userMessage.Length > 6)
                {
                    if (userMessage.ToLower().Substring(0, 4).Equals("user"))
                    {
                        string username = userMessage.Substring(5);
                        userData.SetProperty<string>("username", username);
                        await stateClient.BotState.SetUserDataAsync(activity.ChannelId, activity.From.Id, userData);
                        endOutput = "Hello " + username;
                        isRequest = false;
                    }
                }


                if (userMessage.ToLower().Contains("clear"))
                {
                    endOutput = "User data cleared";
                    await stateClient.BotState.DeleteStateForUserAsync(activity.ChannelId, activity.From.Id);
                    isRequest = false;
                }

                if (userMessage.ToLower().Contains("help"))
                {
                    endOutput = "I can do quick foreign exchange, check your bank balance and edit it as needed. For commands please type commands";
                    await stateClient.BotState.DeleteStateForUserAsync(activity.ChannelId, activity.From.Id);
                    isRequest = false;
                }

                if (userMessage.ToLower().Contains("commands"))
                {
                    endOutput = "foreign exchange example: 'nzd usd', \n\n register or log into a username: 'user (username)' \n\n get your current balance: 'get my balance' \n\n clear data 'clear'";
                    await stateClient.BotState.DeleteStateForUserAsync(activity.ChannelId, activity.From.Id);
                    isRequest = false;
                }

                if (userMessage.ToLower().Equals("get my balance"))
                {
                    List<UserDatabase> userDatabase = await AzureManager.AzureManagerInstance.GetUserDatabase();
                    endOutput = "";
                    foreach (UserDatabase t in userDatabase)
                    {
                        if (userData.GetProperty<string>("username").ToLower() == t.Name.ToLower())
                        {
                            endOutput += "USD: " + t.USD + "\n\n" + "NZD: " + t.NZD + "\n\n" + "AUD: " + t.AUD;
                        }
                    }
                    isRequest = false;

                }
                if (userMessage.ToLower().Equals("create new account"))
                {
                    UserDatabase userDatabase = new UserDatabase()
                    {
                        Name = userData.GetProperty<string>("username").ToLower(),
                        USD = 0,
                        NZD = 0,
                        AUD = 0
                    };
                    await AzureManager.AzureManagerInstance.AddUserDatabase(userDatabase);
                    endOutput = "";
                    endOutput += "Congratulations your account has been saved to our database";
                }

                //if (userMessage.ToLower().Equals("Update my account"))
                //{
                //    List<UserDatabase> userDatabase = await AzureManager.AzureManagerInstance.GetUserDatabase();
                //    foreach (UserDatabase t in userDatabase)
                //    {
                //        if (userData.GetProperty<string>("username").ToLower() == t.Name.ToLower())
                //        {
                //            UserDatabase userDatabase = await

                //        }
                //    }
                //}

                if (userMessage.ToLower().Equals("delete my account"))
                {
                    UserDatabase toBeDeleted = new UserDatabase();
                    toBeDeleted.ID = "90ac02e9-f9f1-4830-95e2-15714e843be9";

                    await AzureManager.AzureManagerInstance.DeleteUserDatabase(toBeDeleted);
                    endOutput = "";
                    endOutput += "Congratulations your account has been deleted from our database";
                }


                // return our reply to the user
                Activity infoReply = activity.CreateReply(endOutput);

                    await connector.Conversations.ReplyToActivityAsync(infoReply);
                
                //else
                //{
                //    ForexObjects.RootObject rootObject;

                //    HttpClient client = new HttpClient();
                //    string x = await client.GetStringAsync(new Uri("http://api.fixer.io/latest?base=" + activity.Text ));

                //    rootObject = JsonConvert.DeserializeObject<ForexObjects.RootObject>(x);
                //    string baseName = rootObject.@base;
                //    double rates = rootObject.rates.NZD;

                //    Activity reply = activity.CreateReply($"Current value of {baseName} in NZD is ");
                //    await connector.Conversations.ReplyToActivityAsync(reply);

                //    Activity ForexReply = activity.CreateReply("hiya");
                //    ForexReply.Recipient = activity.From;
                //    ForexReply.Type = "message";
                //    ForexReply.Attachments = new List<Attachment>();

                //    List<CardImage> cardImages = new List<CardImage>();
                //    cardImages.Add(new CardImage(url: "http://4vector.com/i/free-vector-cb_059449_cb.png"));
                //}
                
            }
            else
            {
                HandleSystemMessage(activity);
            }
            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }

        private Activity HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }

            return null;
        }
    }
}