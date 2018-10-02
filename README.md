# Unity MMO Framework

Yes this is what you think it is

![Test twitch chat](/Images/Test.gif)<br/>

## Features

* Mongodb connection
* Login, register
* chat, movement, animations

## TODO

* More examples
* More authority
* Enemy management
* Zone/Channel management
* Save data on the server (Like transformations and health...)

## How to use

Just edit the server, put in it your database data, the port and everything you want to change, run it, I recomend to use

'''
dotnet run
'''

you can upload this to Heroku or any Server that allows C#

Then open the unity project, put in it the server data (ip and port), then init an example. your unity version must be 2018.2.9f1 or greater and your script runtime version must be .Net 4.x equivalent.

## How it works

Both server and client works with Sockets and threads, also the project uses Newtonsoft.Json to hanle the Json data. Unity client uses NinjaThreads to handle info.

### The message

The project has the concept of message, a object made to exchange info between server and player.

```C#
namespace MMO.Models
{
    public struct MMOMessage
    {
        public string Sender;
        public string Reciever;
        public string Type;
        public string Message;
    }
}
```

The message field can have another type of data like transforms or login data.

### Send Data From the client

To (for example) broadcast a message you just have to send a message like that

```C#
void SendChatMessage ()
{
    MultiplayerClient.singleton.SendData("Chat", "", "All", inputText.text);
    inputText.text = "";
}
```

The framework it's fully compatible with all Unity engine api, you also can handle callbacks like this

```C#
MultiplayerClient.singleton.AddCallback("Error", (M) =>
{
    textChat.text += $"\n<color=#ff0000ff>{M.Message}</color>";
});

```

When the client recieve an "Error" type message, the text chat will show a message

## License
* This project uses [Thread Ninja](https://assetstore.unity.com/packages/tools/thread-ninja-multithread-coroutine-15717)
* This project uses [Json.NET](https://www.newtonsoft.com/json)
* This project uses [MongoDB Driver](https://docs.mongodb.com/ecosystem/drivers/csharp/)
* Everything else is 100% handcrafted for me and MIT license

## Support this on patreon
This project is free to use so please consider Support on Patreon<br/>
![Please consider support on patreon](/Images/Patreon.png)<br/>
https://www.patreon.com/HectorPulido