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
                string endOutput = "Hello and welcome to Contoso Bot please type 'user (username)' to log in, if you want to see what I can do please type help or for a list of commands. type commands";



                // calculate something for us to return
                if (userData.GetProperty<bool>("SentGreeting"))
                {
                    endOutput = "Hello again for a list of commands type 'commands' for a list of what I can do type 'help'";
                }
                else
                {   
                    userData.SetProperty<bool>("SentGreeting", true);
                    await stateClient.BotState.SetUserDataAsync(activity.ChannelId, activity.From.Id, userData);
                }
                bool isRequest = false;

                if (userMessage.Length > 6)
                {
                    if (userMessage.ToLower().Substring(0, 4).Equals("user"))
                    {
                        string username = userMessage.Substring(5);
                        userData.SetProperty<string>("username", username);
                        await stateClient.BotState.SetUserDataAsync(activity.ChannelId, activity.From.Id, userData);
                        endOutput = "Hello " + username + " what would you like me to do today?";
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
                    endOutput = "foreign exchange example: 'nzd usd', \n\n register or log into a username: 'user (username)' \n\n get your current balance: 'get my balance' \n\n clear data 'clear' \n\n create an account: create new account";
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
                    isRequest = false;
                }
                
                if (userMessage.ToLower().Equals("delete my account"))
                {
                    List<UserDatabase> userDatabase = await AzureManager.AzureManagerInstance.GetUserDatabase();
                    foreach (UserDatabase t in userDatabase)
                    {
                        if (userData.GetProperty<string>("username").ToLower() == t.Name.ToLower())
                        {
                            await AzureManager.AzureManagerInstance.DeleteUserDatabase(t);

                        }

                        endOutput = "";
                        endOutput += "Congratulations your account has been deleted from our database";
                    }
                }

                if (userMessage.ToLower().Equals("contoso bot"))
                {
                    Activity replyToConversation = activity.CreateReply("More information about Contoso Bot");
                    replyToConversation.Recipient = activity.From;
                    replyToConversation.Type = "message";
                    replyToConversation.Attachments = new List<Attachment>();
                    List<CardImage> cardImages = new List<CardImage>();
                    cardImages.Add(new CardImage(url: "http://www.gmkfreelogos.com/logos/C/img/CB.gif"));
                    List<CardAction> cardButtons = new List<CardAction>();
                    CardAction plButton = new CardAction()
                    {
                        Value = "https://www.facebook.com/schocontosobank/",
                        Type = "openUrl",
                        Title = "Contoso Facebook"
                    };
                    cardButtons.Add(plButton);
                    ThumbnailCard plCard = new ThumbnailCard()
                    {
                        Title = "Visit Contoso Bot's Facebook",
                        Subtitle = "The Contoso Bot's here",
                        Images = cardImages,
                        Buttons = cardButtons
                    };
                    Attachment plAttachment = plCard.ToAttachment();
                    replyToConversation.Attachments.Add(plAttachment);
                    await connector.Conversations.SendToConversationAsync(replyToConversation);

                    return Request.CreateResponse(HttpStatusCode.OK);

                }
                if (userMessage.Length == 3)
                {
                    isRequest = true;
                }
                // return our reply to the user
                if (!isRequest){
                    Activity infoReply = activity.CreateReply(endOutput);

                    await connector.Conversations.ReplyToActivityAsync(infoReply);
                }
                else
                {

                    ForexObjects.RootObject rootObject;

                    HttpClient client = new HttpClient();
                    string x = await client.GetStringAsync(new Uri("http://api.fixer.io/latest?base=" + activity.Text));
                    //var json = JObject.Parse(x);
                    // var rates = parse.(json["rates"].

                    rootObject = JsonConvert.DeserializeObject<ForexObjects.RootObject>(x);
                    string baseName = rootObject.@base;
                    double rates = rootObject.rates.NZD;
                    string date = rootObject.date;

                    Activity reply = activity.CreateReply($"Current value of {baseName} in NZD is " + rates);
                    await connector.Conversations.ReplyToActivityAsync(reply);

                    Activity ForexReply = activity.CreateReply("hiya");
                    ForexReply.Recipient = activity.From;
                    ForexReply.Type = "message";
                    ForexReply.Attachments = new List<Attachment>();

                    List<CardImage> cardImages = new List<CardImage>();
                    cardImages.Add(new CardImage(url: "http://4vector.com/i/free-vector-cb_059449_cb.png"));
                }

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