using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

using Newtonsoft.Json;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;

namespace MMO
{
    using MMO.Models;
    using MMO.Server;
    class Program
    {
        static void Main(string[] args)
        {
            MMOServer server = new MMOServer();
            
            var serverUri = ""; //Your server url, i recommend Mlab to test
            var dbName = "mmodatabasetest"; // Your database name
            var collectionsNames = new string[]{"collectionUsers"}; // All the collection you will need
            
            server.Start(serverUri, dbName, collectionsNames, 5000);
        }
    }
}

