using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Dynamic;
using Newtonsoft.Json;
using iflix.services;
using iflix.models;
using iflix.providers;

namespace iflix
{
    class Program
    {
        static void Main(string[] args)
        {
            var service = new OfferService();
            var accounts = service.LoadAccounts(@"data/accounts.json");
            if (accounts == null || !accounts.Any())
            {
                Console.WriteLine("failed to load any accounts. stop processing");
                return;
            }

            var actions = new List<ActionModel>();
            var partners = new [] { "amazecom", "wondertel"};
            foreach (var partner in partners)
            {
                var partnerActions = service.LoadActions($"data/{partner}.json", partner);
                actions.AddRange(partnerActions);                
                Console.WriteLine($"Added {partnerActions.Count()} actions for {partner}");
            }

            actions = actions
                .OrderBy(c => c.date) // first order by date
                .ThenBy(c => c.action == "revocation" ? 1 : 2) // then order by revocation before grant
                .ToList();
            
            foreach (var action in actions)
            {
                UserModel account;
                if (!accounts.TryGetValue(action.number, out account))
                {
                    Console.WriteLine($"{action.number} from {action.partner} not found in accounts list");
                    continue;
                }
                switch(action.action)
                {
                    case "revocation":
                        service.Revoke(account, action);
                        break;
                    case "grant":
                        service.Grant(account, action);
                        break;
                }
            }

            service.ConstructOutput(accounts.Values);
        }
    }
}
