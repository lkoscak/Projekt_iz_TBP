﻿using Neo4jClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace FindAndRead.Neo4j
{
    public class Neo4jConnectionHandler
    {
        public static IGraphClient Client { get; private set; }

        public static void Neo4JSetupConnection()
        {

            var url = ConfigurationManager.AppSettings["GraphDBUrl"];
            var user = ConfigurationManager.AppSettings["GraphDBUser"];
            var password = ConfigurationManager.AppSettings["GraphDBPassword"];
            var client = new GraphClient(new Uri(url), user, password);

            client.Connect();

            Client = client;
        }
    }
}